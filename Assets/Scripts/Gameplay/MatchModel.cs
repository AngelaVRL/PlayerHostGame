using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class MatchModel
    {
        public const int NoWinnerId = -1;

        private readonly Dictionary<int, int> scoresByPlayerId = new Dictionary<int, int>();
        private readonly Dictionary<int, Vector2> coinPositionsById = new Dictionary<int, Vector2>();
        private readonly List<int> idsBuffer = new List<int>();

        public event Action<MatchState> StateChanged;
        public event Action<int, int> ScoreChanged;
        public event Action<int, Vector2> CoinAdded;
        public event Action<int> CoinRemoved;
        public event Action<bool> PausedChanged;

        public MatchState State { get; private set; } = MatchState.Lobby;
        public int WinnerId { get; private set; } = NoWinnerId;
        public bool IsPaused { get; private set; }
        public GameRules Rules { get; } = new GameRules();

        public IEnumerable<KeyValuePair<int, Vector2>> Coins
        {
            get { return coinPositionsById; }
        }

        public IEnumerable<KeyValuePair<int, int>> Scores
        {
            get { return scoresByPlayerId; }
        }

        public int GetScore(int playerId)
        {
            return scoresByPlayerId.TryGetValue(playerId, out int score) ? score : 0;
        }

        public void ApplyState(MatchState state, int winnerId)
        {
            ClearCoins();
            SetPaused(false);
            if (state == MatchState.Playing)
            {
                ClearScores();
            }

            State = state;
            WinnerId = winnerId;
            StateChanged?.Invoke(state);
        }

        public void Reset()
        {
            ApplyState(MatchState.Lobby, NoWinnerId);
            ClearScores();
            Rules.Reset();
        }

        public void SetPaused(bool isPaused)
        {
            if (IsPaused == isPaused)
            {
                return;
            }

            IsPaused = isPaused;
            PausedChanged?.Invoke(isPaused);
        }

        public void SetScore(int playerId, int score)
        {
            scoresByPlayerId[playerId] = score;
            ScoreChanged?.Invoke(playerId, score);
        }

        public void RemoveScore(int playerId)
        {
            if (scoresByPlayerId.Remove(playerId))
            {
                ScoreChanged?.Invoke(playerId, 0);
            }
        }

        public void AddCoin(int coinId, Vector2 position)
        {
            if (coinPositionsById.ContainsKey(coinId))
            {
                return;
            }

            coinPositionsById[coinId] = position;
            CoinAdded?.Invoke(coinId, position);
        }

        public void RemoveCoin(int coinId)
        {
            if (coinPositionsById.Remove(coinId))
            {
                CoinRemoved?.Invoke(coinId);
            }
        }

        private void ClearCoins()
        {
            idsBuffer.Clear();
            idsBuffer.AddRange(coinPositionsById.Keys);
            foreach (int coinId in idsBuffer)
            {
                RemoveCoin(coinId);
            }
        }

        private void ClearScores()
        {
            idsBuffer.Clear();
            idsBuffer.AddRange(scoresByPlayerId.Keys);
            scoresByPlayerId.Clear();
            foreach (int playerId in idsBuffer)
            {
                ScoreChanged?.Invoke(playerId, 0);
            }
        }
    }
}
