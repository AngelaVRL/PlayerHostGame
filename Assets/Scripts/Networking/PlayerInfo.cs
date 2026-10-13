namespace PlayerHostGame.Networking
{
    public sealed class PlayerInfo
    {
        public int Id { get; }
        public string Name { get; }
        public int PingMilliseconds { get; set; }

        public PlayerInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
