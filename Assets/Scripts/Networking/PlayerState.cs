using UnityEngine;

namespace PlayerHostGame.Networking
{
    public readonly struct PlayerState
    {
        public int PlayerId { get; }
        public Vector2 Position { get; }

        public PlayerState(int playerId, Vector2 position)
        {
            PlayerId = playerId;
            Position = position;
        }
    }
}
