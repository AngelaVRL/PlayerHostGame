using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class ClientMatchReceiver : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private MatchContext matchContext;

        private void OnEnable()
        {
            session.MessageReceived += HandleMessage;
        }

        private void OnDisable()
        {
            session.MessageReceived -= HandleMessage;
        }

        private void HandleMessage(int senderId, NetworkMessage message)
        {
            if (session.IsHost)
            {
                return;
            }

            switch (message.Type)
            {
                case MessageType.MatchStateChanged:
                    ApplyMatchState(message);
                    break;
                case MessageType.CoinSpawned:
                    ApplyCoinSpawned(message);
                    break;
                case MessageType.CoinRemoved:
                    ApplyCoinRemoved(message);
                    break;
                case MessageType.ScoreUpdated:
                    ApplyScore(message);
                    break;
                case MessageType.PauseChanged:
                    ApplyPause(message);
                    break;
                case MessageType.RulesChanged:
                    ApplyRules(message);
                    break;
            }
        }

        private void ApplyMatchState(NetworkMessage message)
        {
            if (MatchMessageCodec.TryDecodeMatchState(message, out MatchState state, out int winnerId))
            {
                matchContext.Model.ApplyState(state, winnerId);
            }
        }

        private void ApplyCoinSpawned(NetworkMessage message)
        {
            if (MatchMessageCodec.TryDecodeCoinSpawned(message, out int coinId, out Vector2 position))
            {
                matchContext.Model.AddCoin(coinId, position);
            }
        }

        private void ApplyCoinRemoved(NetworkMessage message)
        {
            if (MatchMessageCodec.TryDecodeCoinRemoved(message, out int coinId))
            {
                matchContext.Model.RemoveCoin(coinId);
            }
        }

        private void ApplyPause(NetworkMessage message)
        {
            if (MatchMessageCodec.TryDecodePause(message, out bool isPaused))
            {
                matchContext.Model.SetPaused(isPaused);
            }
        }

        private void ApplyRules(NetworkMessage message)
        {
            if (MatchMessageCodec.TryDecodeRules(message, out int targetScore, out float playerSpeed))
            {
                matchContext.Model.Rules.Apply(targetScore, playerSpeed);
            }
        }

        private void ApplyScore(NetworkMessage message)
        {
            if (MatchMessageCodec.TryDecodeScore(message, out int playerId, out int score))
            {
                matchContext.Model.SetScore(playerId, score);
            }
        }
    }
}
