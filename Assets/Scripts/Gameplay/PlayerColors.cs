using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public static class PlayerColors
    {
        private static readonly Color[] Palette =
        {
            new Color(0.91f, 0.30f, 0.24f),
            new Color(0.20f, 0.60f, 0.86f),
            new Color(0.18f, 0.80f, 0.44f),
            new Color(0.61f, 0.35f, 0.71f)
        };

        public static Color GetColor(int playerId)
        {
            return Palette[playerId % Palette.Length];
        }
    }
}
