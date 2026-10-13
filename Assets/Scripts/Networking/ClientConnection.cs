using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PlayerHostGame.Networking
{
    public sealed class ClientConnection
    {
        private readonly string playerName;
        private TcpMessageChannel channel;
        private Task<TcpClient> connectTask;

        public event Action<PlayerInfo> PlayerJoined;
        public event Action<PlayerInfo> PlayerLeft;
        public event Action<string> Disconnected;
        public event Action<NetworkMessage> MessageReceived;

        public PlayerRoster Roster { get; } = new PlayerRoster();
        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
        public int LocalPlayerId { get; private set; }

        public ClientConnection(string playerName)
        {
            this.playerName = playerName;
        }

        public void BeginConnect(string hostAddress, int port)
        {
            if (State != ConnectionState.Disconnected)
            {
                return;
            }

            State = ConnectionState.Connecting;
            connectTask = Task.Run(() => OpenConnection(hostAddress, port));
        }

        public void Disconnect()
        {
            CloseConnection("Te desconectaste de la partida");
        }

        public void Send(NetworkMessage message)
        {
            if (State == ConnectionState.Connected && channel != null)
            {
                channel.Send(message);
            }
        }

        public void Tick()
        {
            if (State == ConnectionState.Disconnected)
            {
                return;
            }

            if (State == ConnectionState.Connecting)
            {
                CheckConnectResult();
                return;
            }

            ProcessMessages();
            if (channel != null)
            {
                CheckConnectionHealth();
            }
        }

        private static TcpClient OpenConnection(string hostAddress, int port)
        {
            TcpClient client = new TcpClient();
            try
            {
                Task connectAttempt = client.ConnectAsync(hostAddress, port);
                if (!connectAttempt.Wait(NetworkConfig.ConnectTimeoutMs))
                {
                    throw new TimeoutException("el host no respondió a tiempo");
                }

                return client;
            }
            catch
            {
                client.Close();
                throw;
            }
        }

        private void CheckConnectResult()
        {
            if (!connectTask.IsCompleted)
            {
                return;
            }

            if (connectTask.IsFaulted)
            {
                string detail = connectTask.Exception.GetBaseException().Message;
                CloseConnection("No se pudo conectar: " + detail);
                return;
            }

            channel = new TcpMessageChannel(connectTask.Result);
            State = ConnectionState.Joining;
            channel.Send(new NetworkMessage(MessageType.JoinRequest, playerName));
        }

        private void ProcessMessages()
        {
            while (channel != null && channel.TryDequeue(out NetworkMessage message))
            {
                HandleMessage(message);
            }
        }

        private void HandleMessage(NetworkMessage message)
        {
            switch (message.Type)
            {
                case MessageType.JoinAccepted:
                    HandleJoinAccepted(message);
                    break;
                case MessageType.JoinRejected:
                    CloseConnection(message.HasFields(1) ? message.Fields[0] : "Conexión rechazada");
                    break;
                case MessageType.PlayerJoined:
                    HandlePlayerJoined(message);
                    break;
                case MessageType.PlayerLeft:
                    HandlePlayerLeft(message);
                    break;
                case MessageType.Kicked:
                    CloseConnection(message.HasFields(1) ? message.Fields[0] : "Fuiste expulsado de la partida");
                    break;
                case MessageType.Ping:
                    channel.Send(new NetworkMessage(MessageType.Pong, message.Fields));
                    break;
                default:
                    MessageReceived?.Invoke(message);
                    break;
            }
        }

        private void HandleJoinAccepted(NetworkMessage message)
        {
            if (State != ConnectionState.Joining)
            {
                return;
            }

            if (!message.TryGetInt(0, out int playerId))
            {
                CloseConnection("Respuesta inválida del host");
                return;
            }

            LocalPlayerId = playerId;
            State = ConnectionState.Connected;
        }

        private void HandlePlayerJoined(NetworkMessage message)
        {
            if (!message.HasFields(2)
                || !message.TryGetInt(0, out int playerId)
                || !PlayerNameValidator.IsValid(message.Fields[1]))
            {
                return;
            }

            PlayerInfo player = Roster.Add(playerId, message.Fields[1]);
            PlayerJoined?.Invoke(player);
        }

        private void HandlePlayerLeft(NetworkMessage message)
        {
            if (message.TryGetInt(0, out int playerId) && Roster.TryRemove(playerId, out PlayerInfo player))
            {
                PlayerLeft?.Invoke(player);
            }
        }

        private void CheckConnectionHealth()
        {
            bool isIdle = channel.MillisecondsSinceLastReceive > NetworkConfig.IdleTimeoutMs;
            if (!channel.IsOpen || isIdle)
            {
                CloseConnection("Se perdió la conexión con el host (" + channel.CloseReason + ")");
            }
        }

        private void CloseConnection(string reason)
        {
            if (State == ConnectionState.Disconnected)
            {
                return;
            }

            if (State == ConnectionState.Connecting)
            {
                ReleasePendingConnection();
            }

            if (channel != null)
            {
                channel.Close(reason);
                channel = null;
            }

            Roster.Clear();
            State = ConnectionState.Disconnected;
            Disconnected?.Invoke(reason);
        }

        private void ReleasePendingConnection()
        {
            connectTask.ContinueWith(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    task.Result.Close();
                }
            });
        }
    }
}
