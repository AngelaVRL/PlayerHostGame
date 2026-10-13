namespace PlayerHostGame.Networking
{
    public enum MessageType
    {
        JoinRequest,
        JoinAccepted,
        JoinRejected,
        PlayerJoined,
        PlayerLeft,
        Ping,
        Pong,
        InputUpdate,
        StateSnapshot,
        MatchStateChanged,
        CoinSpawned,
        CoinRemoved,
        ScoreUpdated,
        Kicked,
        PauseChanged,
        RulesChanged
    }
}
