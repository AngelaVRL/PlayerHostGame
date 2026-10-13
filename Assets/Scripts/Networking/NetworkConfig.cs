namespace PlayerHostGame.Networking
{
    public static class NetworkConfig
    {
        public const int DefaultPort = 7777;
        public const int MaxPlayers = 4;
        public const int MaxPlayerNameLength = 16;
        public const int MaxMessageBytes = 4096;
        public const int ConnectTimeoutMs = 3000;
        public const int JoinTimeoutMs = 5000;
        public const int SendTimeoutMs = 2000;
        public const int PingIntervalMs = 1000;
        public const int IdleTimeoutMs = 8000;
    }
}
