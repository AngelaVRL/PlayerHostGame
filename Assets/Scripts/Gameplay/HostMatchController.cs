using System.Collections.Generic;
using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class HostMatchController : MonoBehaviour
    {
        private const float PickupDistance = GameplayConfig.PlayerRadius + GameplayConfig.CoinRadius;

        [SerializeField] private NetworkSession session;
        [SerializeField] private PlayerAvatarManager avatarManager;
        [SerializeField] private MatchContext matchContext;

        private int nextCoinId;

        private MatchModel Model
        {
            get { return matchContext.Model; }
        }

        private void OnEnable()
        {
            session.PlayerJoined += HandlePlayerJoined;
        }

        private void OnDisable()
        {
            session.PlayerJoined -= HandlePlayerJoined;
        }

        private void Update()
        {
            if (!session.IsHost || Model.State != MatchState.Playing || Model.IsPaused)
            {
                return;
            }

            foreach (PlayerAvatar avatar in avatarManager.Avatars)
            {
                if (!TryFindTouchedCoin(avatar.transform.position, out int coinId))
                {
                    continue;
                }

                CollectCoin(avatar.PlayerId, coinId);
                if (Model.State != MatchState.Playing)
                {
                    return;
                }
            }
        }

        public void StartMatch()
        {
            if (!session.IsHost || Model.State == MatchState.Playing)
            {
                return;
            }

            Model.ApplyState(MatchState.Playing, MatchModel.NoWinnerId);
            session.BroadcastToClients(MatchMessageCodec.EncodeMatchState(MatchState.Playing, MatchModel.NoWinnerId));

            for (int index = 0; index < GameplayConfig.CoinsOnField; index++)
            {
                SpawnCoin();
            }
        }

        private void EndMatch(int winnerId)
        {
            Model.ApplyState(MatchState.Finished, winnerId);
            session.BroadcastToClients(MatchMessageCodec.EncodeMatchState(MatchState.Finished, winnerId));
        }

        private void SpawnCoin()
        {
            int coinId = nextCoinId++;
            Vector2 position = ArenaBounds.GetRandomPosition(GameplayConfig.CoinSpawnMargin);
            Model.AddCoin(coinId, position);
            session.BroadcastToClients(MatchMessageCodec.EncodeCoinSpawned(coinId, position));
        }

        private void CollectCoin(int playerId, int coinId)
        {
            Model.RemoveCoin(coinId);
            session.BroadcastToClients(MatchMessageCodec.EncodeCoinRemoved(coinId));

            int newScore = Model.GetScore(playerId) + 1;
            Model.SetScore(playerId, newScore);
            session.BroadcastToClients(MatchMessageCodec.EncodeScore(playerId, newScore));

            if (newScore >= Model.Rules.TargetScore)
            {
                EndMatch(playerId);
                return;
            }

            SpawnCoin();
        }

        private bool TryFindTouchedCoin(Vector2 position, out int coinId)
        {
            coinId = 0;
            foreach (KeyValuePair<int, Vector2> coin in Model.Coins)
            {
                if ((coin.Value - position).sqrMagnitude <= PickupDistance * PickupDistance)
                {
                    coinId = coin.Key;
                    return true;
                }
            }

            return false;
        }

        private void HandlePlayerJoined(PlayerInfo player)
        {
            if (!session.IsHost || player.Id == session.LocalPlayerId)
            {
                return;
            }

            session.SendToPlayer(player.Id, MatchMessageCodec.EncodeMatchState(Model.State, Model.WinnerId));
            session.SendToPlayer(player.Id, MatchMessageCodec.EncodePause(Model.IsPaused));
            session.SendToPlayer(player.Id, MatchMessageCodec.EncodeRules(Model.Rules));
            foreach (KeyValuePair<int, Vector2> coin in Model.Coins)
            {
                session.SendToPlayer(player.Id, MatchMessageCodec.EncodeCoinSpawned(coin.Key, coin.Value));
            }

            foreach (KeyValuePair<int, int> score in Model.Scores)
            {
                session.SendToPlayer(player.Id, MatchMessageCodec.EncodeScore(score.Key, score.Value));
            }
        }
    }
}
