using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace PlayerHostGame.Networking
{
    public static class GameMessageCodec
    {
        private const int SnapshotFieldsPerPlayer = 3;
        public const string PositionFormat = "F3";

        public static NetworkMessage EncodeInput(Vector2Int input)
        {
            return new NetworkMessage(
                MessageType.InputUpdate,
                input.x.ToString(CultureInfo.InvariantCulture),
                input.y.ToString(CultureInfo.InvariantCulture));
        }

        public static bool TryDecodeInput(NetworkMessage message, out Vector2Int input)
        {
            input = Vector2Int.zero;
            if (!message.TryGetInt(0, out int x) || !message.TryGetInt(1, out int y))
            {
                return false;
            }

            input = new Vector2Int(Mathf.Clamp(x, -1, 1), Mathf.Clamp(y, -1, 1));
            return true;
        }

        public static NetworkMessage EncodeSnapshot(List<PlayerState> states)
        {
            string[] fields = new string[states.Count * SnapshotFieldsPerPlayer];
            for (int index = 0; index < states.Count; index++)
            {
                PlayerState state = states[index];
                int offset = index * SnapshotFieldsPerPlayer;
                fields[offset] = state.PlayerId.ToString(CultureInfo.InvariantCulture);
                fields[offset + 1] = state.Position.x.ToString(PositionFormat, CultureInfo.InvariantCulture);
                fields[offset + 2] = state.Position.y.ToString(PositionFormat, CultureInfo.InvariantCulture);
            }

            return new NetworkMessage(MessageType.StateSnapshot, fields);
        }

        public static bool TryDecodeSnapshot(NetworkMessage message, List<PlayerState> states)
        {
            states.Clear();
            if (message.Fields.Length % SnapshotFieldsPerPlayer != 0)
            {
                return false;
            }

            for (int offset = 0; offset < message.Fields.Length; offset += SnapshotFieldsPerPlayer)
            {
                if (!message.TryGetInt(offset, out int playerId)
                    || !message.TryGetFloat(offset + 1, out float x)
                    || !message.TryGetFloat(offset + 2, out float y))
                {
                    states.Clear();
                    return false;
                }

                states.Add(new PlayerState(playerId, new Vector2(x, y)));
            }

            return true;
        }
    }
}
