using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class MatchContext : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;

        public MatchModel Model { get; } = new MatchModel();

        private void OnEnable()
        {
            session.PlayerLeft += HandlePlayerLeft;
            session.ConnectionClosed += HandleConnectionClosed;
        }

        private void OnDisable()
        {
            session.PlayerLeft -= HandlePlayerLeft;
            session.ConnectionClosed -= HandleConnectionClosed;
        }

        private void HandlePlayerLeft(PlayerInfo player)
        {
            Model.RemoveScore(player.Id);
        }

        private void HandleConnectionClosed(string reason)
        {
            Model.Reset();
        }
    }
}
