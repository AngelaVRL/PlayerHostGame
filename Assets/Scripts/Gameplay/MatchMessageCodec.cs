using System;
using System.Globalization;
using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public static class MatchMessageCodec
    {
        public static NetworkMessage EncodeMatchState(MatchState state, int winnerId)
        {
            return new NetworkMessage(
                MessageType.MatchStateChanged,
                ((int)state).ToString(CultureInfo.InvariantCulture),
                winnerId.ToString(CultureInfo.InvariantCulture));
        }

        public static bool TryDecodeMatchState(NetworkMessage message, out MatchState state, out int winnerId)
        {
            state = MatchState.Lobby;
            winnerId = MatchModel.NoWinnerId;

            if (!message.TryGetInt(0, out int stateValue)
                || !message.TryGetInt(1, out winnerId)
                || !Enum.IsDefined(typeof(MatchState), stateValue))
            {
                return false;
            }

            state = (MatchState)stateValue;
            return true;
        }

        public static NetworkMessage EncodeCoinSpawned(int coinId, Vector2 position)
        {
            return new NetworkMessage(
                MessageType.CoinSpawned,
                coinId.ToString(CultureInfo.InvariantCulture),
                position.x.ToString(GameMessageCodec.PositionFormat, CultureInfo.InvariantCulture),
                position.y.ToString(GameMessageCodec.PositionFormat, CultureInfo.InvariantCulture));
        }

        public static bool TryDecodeCoinSpawned(NetworkMessage message, out int coinId, out Vector2 position)
        {
            position = Vector2.zero;
            if (!message.TryGetInt(0, out coinId)
                || !message.TryGetFloat(1, out float x)
                || !message.TryGetFloat(2, out float y))
            {
                return false;
            }

            position = new Vector2(x, y);
            return true;
        }

        public static NetworkMessage EncodeCoinRemoved(int coinId)
        {
            return new NetworkMessage(MessageType.CoinRemoved, coinId.ToString(CultureInfo.InvariantCulture));
        }

        public static bool TryDecodeCoinRemoved(NetworkMessage message, out int coinId)
        {
            return message.TryGetInt(0, out coinId);
        }

        public static NetworkMessage EncodeScore(int playerId, int score)
        {
            return new NetworkMessage(
                MessageType.ScoreUpdated,
                playerId.ToString(CultureInfo.InvariantCulture),
                score.ToString(CultureInfo.InvariantCulture));
        }

        public static bool TryDecodeScore(NetworkMessage message, out int playerId, out int score)
        {
            score = 0;
            return message.TryGetInt(0, out playerId)
                && message.TryGetInt(1, out score)
                && score >= 0;
        }

        public static NetworkMessage EncodePause(bool isPaused)
        {
            return new NetworkMessage(MessageType.PauseChanged, isPaused ? "1" : "0");
        }

        public static bool TryDecodePause(NetworkMessage message, out bool isPaused)
        {
            isPaused = false;
            if (!message.TryGetInt(0, out int value))
            {
                return false;
            }

            isPaused = value != 0;
            return true;
        }

        public static NetworkMessage EncodeRules(GameRules rules)
        {
            return new NetworkMessage(
                MessageType.RulesChanged,
                rules.TargetScore.ToString(CultureInfo.InvariantCulture),
                rules.PlayerSpeed.ToString("F2", CultureInfo.InvariantCulture));
        }

        public static bool TryDecodeRules(NetworkMessage message, out int targetScore, out float playerSpeed)
        {
            playerSpeed = 0f;
            return message.TryGetInt(0, out targetScore)
                && message.TryGetFloat(1, out playerSpeed);
        }
    }
}
