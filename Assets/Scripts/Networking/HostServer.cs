using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PlayerHostGame.Networking
{
    public sealed class HostServer
    {
        public const int HostPlayerId = 0;

        private const int NoExcludedPlayer = -1;
        private const string KickReason = "Fuiste expulsado por el host";

        private readonly string hostName;
        private readonly int port;
        private readonly PlayerRoster roster = new PlayerRoster();
        private readonly Dictionary<int, TcpMessageChannel> channelsByPlayerId = new Dictionary<int, TcpMessageChannel>();
        private readonly List<PendingConnection> pendingConnections = new List<PendingConnection>();
        private readonly ConcurrentQueue<TcpClient> acceptedClients = new ConcurrentQueue<TcpClient>();
        private readonly List<int> disconnectedPlayerIds = new List<int>();
        private readonly List<int> playerIdsBuffer = new List<int>();

        private TcpListener listener;
        private volatile bool isRunning;
        private int nextPlayerId = HostPlayerId + 1;
        private long lastPingMilliseconds;

        public event Action<PlayerInfo> PlayerJoined;
        public event Action<PlayerInfo> PlayerLeft;
        public event Action<int, NetworkMessage> MessageReceived;

        public PlayerRoster Roster
        {
            get { return roster; }
        }

        public HostServer(string hostName, int port)
        {
            this.hostName = hostName;
            this.port = port;
        }

        public bool TryStart(out string errorMessage)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
            }
            catch (SocketException exception)
            {
                listener = null;
                errorMessage = "No se pudo abrir el puerto " + port + ": " + exception.Message;
                return false;
            }

            PlayerInfo hostPlayer = roster.Add(HostPlayerId, hostName);
            isRunning = true;

            Thread acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            acceptThread.Start();

            errorMessage = null;
            PlayerJoined?.Invoke(hostPlayer);
            return true;
        }

        public void Tick()
        {
            if (!isRunning)
            {
                return;
            }

            AcceptPendingClients();
            ProcessPendingConnections();
            ProcessPlayerMessages();
            RemoveDisconnectedPlayers();
            SendPingsIfDue();
        }

        public void Stop()
        {
            isRunning = false;

            if (listener != null)
            {
                listener.Stop();
            }

            foreach (TcpMessageChannel channel in channelsByPlayerId.Values)
            {
                channel.Close();
            }

            foreach (PendingConnection pending in pendingConnections)
            {
                pending.Channel.Close();
            }

            while (acceptedClients.TryDequeue(out TcpClient client))
            {
                client.Close();
            }

            channelsByPlayerId.Clear();
            pendingConnections.Clear();
            roster.Clear();
        }

        public void Broadcast(NetworkMessage message)
        {
            BroadcastToClients(message, NoExcludedPlayer);
        }

        public void KickPlayer(int playerId)
        {
            if (playerId == HostPlayerId || !channelsByPlayerId.TryGetValue(playerId, out TcpMessageChannel channel))
            {
                return;
            }

            channel.Send(new NetworkMessage(MessageType.Kicked, KickReason));
            RemovePlayer(playerId);
        }

        public void SendTo(int playerId, NetworkMessage message)
        {
            if (channelsByPlayerId.TryGetValue(playerId, out TcpMessageChannel channel))
            {
                channel.Send(message);
            }
        }

        private void AcceptLoop()
        {
            while (isRunning)
            {
                try
                {
                    acceptedClients.Enqueue(listener.AcceptTcpClient());
                }
                catch (SocketException)
                {
                    if (!isRunning)
                    {
                        return;
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }

        private void AcceptPendingClients()
        {
            while (acceptedClients.TryDequeue(out TcpClient client))
            {
                try
                {
                    pendingConnections.Add(new PendingConnection(new TcpMessageChannel(client)));
                }
                catch (Exception exception) when (exception is InvalidOperationException || exception is ObjectDisposedException)
                {
                    client.Close();
                }
            }
        }

        private void ProcessPendingConnections()
        {
            for (int index = pendingConnections.Count - 1; index >= 0; index--)
            {
                PendingConnection pending = pendingConnections[index];
                bool resolved = TryResolvePending(pending);

                if (!resolved && IsPendingExpired(pending))
                {
                    pending.Channel.Close();
                    resolved = true;
                }

                if (resolved)
                {
                    pendingConnections.RemoveAt(index);
                }
            }
        }

        private static bool IsPendingExpired(PendingConnection pending)
        {
            long ageMilliseconds = NetworkClock.NowMilliseconds - pending.CreatedAtMilliseconds;
            return !pending.Channel.IsOpen || ageMilliseconds > NetworkConfig.JoinTimeoutMs;
        }

        private bool TryResolvePending(PendingConnection pending)
        {
            if (!pending.Channel.TryDequeue(out NetworkMessage message))
            {
                return false;
            }

            if (message.Type != MessageType.JoinRequest || !message.HasFields(1))
            {
                pending.Channel.Close("Invalid join request");
                return true;
            }

            string playerName = message.Fields[0];
            string rejectionReason = GetJoinRejectionReason(playerName);
            if (rejectionReason != null)
            {
                pending.Channel.Send(new NetworkMessage(MessageType.JoinRejected, rejectionReason));
                pending.Channel.Close(rejectionReason);
                return true;
            }

            AcceptPlayer(pending.Channel, playerName);
            return true;
        }

        private string GetJoinRejectionReason(string playerName)
        {
            if (roster.Count >= NetworkConfig.MaxPlayers)
            {
                return "La sala está llena";
            }

            if (!PlayerNameValidator.IsValid(playerName))
            {
                return "Nombre de jugador inválido";
            }

            if (roster.ContainsName(playerName))
            {
                return "Ese nombre ya está en uso";
            }

            return null;
        }

        private void AcceptPlayer(TcpMessageChannel channel, string playerName)
        {
            PlayerInfo newPlayer = roster.Add(nextPlayerId++, playerName);
            channelsByPlayerId[newPlayer.Id] = channel;

            channel.Send(new NetworkMessage(MessageType.JoinAccepted, newPlayer.Id.ToString()));
            foreach (PlayerInfo player in roster.Players)
            {
                channel.Send(CreatePlayerJoinedMessage(player));
            }

            BroadcastToClients(CreatePlayerJoinedMessage(newPlayer), newPlayer.Id);
            PlayerJoined?.Invoke(newPlayer);
        }

        private void ProcessPlayerMessages()
        {
            playerIdsBuffer.Clear();
            playerIdsBuffer.AddRange(channelsByPlayerId.Keys);

            foreach (int playerId in playerIdsBuffer)
            {
                if (!channelsByPlayerId.TryGetValue(playerId, out TcpMessageChannel channel))
                {
                    continue;
                }

                while (channel.TryDequeue(out NetworkMessage message))
                {
                    DispatchClientMessage(playerId, message);
                }
            }
        }

        private void DispatchClientMessage(int playerId, NetworkMessage message)
        {
            if (message.Type == MessageType.Pong)
            {
                HandlePong(playerId, message);
                return;
            }

            MessageReceived?.Invoke(playerId, message);
        }

        private void HandlePong(int playerId, NetworkMessage message)
        {
            if (!message.TryGetLong(0, out long sentAtMilliseconds))
            {
                return;
            }

            if (roster.TryGet(playerId, out PlayerInfo player))
            {
                long roundTrip = NetworkClock.NowMilliseconds - sentAtMilliseconds;
                player.PingMilliseconds = (int)Math.Max(0, roundTrip);
            }
        }

        private void RemoveDisconnectedPlayers()
        {
            disconnectedPlayerIds.Clear();
            foreach (KeyValuePair<int, TcpMessageChannel> entry in channelsByPlayerId)
            {
                bool isIdle = entry.Value.MillisecondsSinceLastReceive > NetworkConfig.IdleTimeoutMs;
                if (!entry.Value.IsOpen || isIdle)
                {
                    disconnectedPlayerIds.Add(entry.Key);
                }
            }

            foreach (int playerId in disconnectedPlayerIds)
            {
                RemovePlayer(playerId);
            }
        }

        private void RemovePlayer(int playerId)
        {
            channelsByPlayerId[playerId].Close();
            channelsByPlayerId.Remove(playerId);

            if (roster.TryRemove(playerId, out PlayerInfo player))
            {
                BroadcastToClients(new NetworkMessage(MessageType.PlayerLeft, playerId.ToString()), NoExcludedPlayer);
                PlayerLeft?.Invoke(player);
            }
        }

        private void SendPingsIfDue()
        {
            long now = NetworkClock.NowMilliseconds;
            if (now - lastPingMilliseconds < NetworkConfig.PingIntervalMs)
            {
                return;
            }

            lastPingMilliseconds = now;
            BroadcastToClients(new NetworkMessage(MessageType.Ping, now.ToString()), NoExcludedPlayer);
        }

        private void BroadcastToClients(NetworkMessage message, int excludedPlayerId)
        {
            foreach (KeyValuePair<int, TcpMessageChannel> entry in channelsByPlayerId)
            {
                if (entry.Key != excludedPlayerId)
                {
                    entry.Value.Send(message);
                }
            }
        }

        private static NetworkMessage CreatePlayerJoinedMessage(PlayerInfo player)
        {
            return new NetworkMessage(MessageType.PlayerJoined, player.Id.ToString(), player.Name);
        }

        private sealed class PendingConnection
        {
            public TcpMessageChannel Channel { get; }
            public long CreatedAtMilliseconds { get; }

            public PendingConnection(TcpMessageChannel channel)
            {
                Channel = channel;
                CreatedAtMilliseconds = NetworkClock.NowMilliseconds;
            }
        }
    }
}
