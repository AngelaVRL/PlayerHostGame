namespace PlayerHostGame.Gameplay
{
    public static class GameplayConfig
    {
        public const float DefaultPlayerSpeed = 5f;
        public const float MinPlayerSpeed = 2f;
        public const float MaxPlayerSpeed = 10f;
        public const float PlayerRadius = 0.5f;
        public const float ArenaHalfWidth = 8f;
        public const float ArenaHalfHeight = 4.5f;
        public const float SpawnSpacing = 1.5f;
        public const float InterpolationDelaySeconds = 0.1f;
        public const int SnapshotsPerSecond = 20;
        public const float CoinRadius = 0.3f;
        public const float CoinSpawnMargin = 0.6f;
        public const int CoinsOnField = 5;
        public const int DefaultTargetScore = 10;
        public const int MinTargetScore = 3;
        public const int MaxTargetScore = 30;
    }
}
