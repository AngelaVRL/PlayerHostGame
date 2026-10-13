using System;
using UnityEngine;

namespace PlayerHostGame.Networking
{
    public sealed class NetworkSession : MonoBehaviour
    {
        private const int NoPlayerId = -1;

        public event Action<PlayerInfo> PlayerJoined;
        public event Action<PlayerInfo> PlayerLeft;
        public event Action<int, NetworkMessage> MessageReceived;
        public event Action<string> ConnectionClosed;

        public HostServer Host { get; private set; }
        public ClientConnection Client { get; private set; }

        public bool IsHost
        {
            get { return Host != null; }
        }

        public bool IsSessionActive
        {
            get { return IsHost || (Client != null && Client.State == ConnectionState.Connected); }
        }

        public int LocalPlayerId
        {
            get
            {
                if (IsHost)
                {
                    return HostServer.HostPlayerId;
                }

                return Client != null ? Client.LocalPlayerId : NoPlayerId;
            }
        }

        public PlayerRoster Roster
        {
            get { return IsHost ? Host.Roster : Client?.Roster; }
        }

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Update()
        {
            Host?.Tick();
            Client?.Tick();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public bool StartHost(string playerName, out string errorMessage)
        {
            if (!TryPrepareSession(playerName, out string cleanName, out errorMessage))
            {
                return false;
            }

            HostServer host = new HostServer(cleanName, NetworkConfig.DefaultPort);
            host.PlayerJoined += RaisePlayerJoined;
            host.PlayerLeft += RaisePlayerLeft;
            host.MessageReceived += RaiseMessageReceived;
            Host = host;

            if (!host.TryStart(out errorMessage))
            {
                Host = null;
                return false;
            }

            return true;
        }

        public bool StartClient(string playerName, string hostAddress, out string errorMessage)
        {
            if (!TryPrepareSession(playerName, out string cleanName, out errorMessage))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(hostAddress))
            {
                errorMessage = "Escribe la dirección del host";
                return false;
            }

            ClientConnection client = new ClientConnection(cleanName);
            client.PlayerJoined += RaisePlayerJoined;
            client.PlayerLeft += RaisePlayerLeft;
            client.MessageReceived += HandleHostMessage;
            client.Disconnected += HandleClientDisconnected;
            Client = client;
            client.BeginConnect(hostAddress.Trim(), NetworkConfig.DefaultPort);
            return true;
        }

        public void SendToHost(NetworkMessage message)
        {
            if (Client != null)
            {
                Client.Send(message);
            }
        }

        public void BroadcastToClients(NetworkMessage message)
        {
            if (Host != null)
            {
                Host.Broadcast(message);
            }
        }

        public void KickPlayer(int playerId)
        {
            if (Host != null)
            {
                Host.KickPlayer(playerId);
            }
        }

        public void SendToPlayer(int playerId, NetworkMessage message)
        {
            if (Host != null)
            {
                Host.SendTo(playerId, message);
            }
        }

        public void Shutdown()
        {
            if (Host != null)
            {
                Host.Stop();
                Host = null;
                ConnectionClosed?.Invoke("Partida cerrada por el host");
            }

            if (Client != null)
            {
                Client.Disconnect();
            }
        }

        private bool TryPrepareSession(string playerName, out string cleanName, out string errorMessage)
        {
            cleanName = playerName == null ? string.Empty : playerName.Trim();
            errorMessage = null;

            if (Host != null || Client != null)
            {
                errorMessage = "Ya hay una sesión activa";
                return false;
            }

            if (!PlayerNameValidator.IsValid(cleanName))
            {
                errorMessage = "Nombre de jugador inválido";
                return false;
            }

            return true;
        }

        private void HandleClientDisconnected(string reason)
        {
            Client = null;
            ConnectionClosed?.Invoke(reason);
        }

        private void HandleHostMessage(NetworkMessage message)
        {
            RaiseMessageReceived(HostServer.HostPlayerId, message);
        }

        private void RaiseMessageReceived(int senderId, NetworkMessage message)
        {
            MessageReceived?.Invoke(senderId, message);
        }

        private void RaisePlayerJoined(PlayerInfo player)
        {
            PlayerJoined?.Invoke(player);
        }

        private void RaisePlayerLeft(PlayerInfo player)
        {
            PlayerLeft?.Invoke(player);
        }
    }
}
