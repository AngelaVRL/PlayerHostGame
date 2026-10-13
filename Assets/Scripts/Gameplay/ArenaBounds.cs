using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public static class ArenaBounds
    {
        public static Vector2 Clamp(Vector2 position)
        {
            float limitX = GameplayConfig.ArenaHalfWidth - GameplayConfig.PlayerRadius;
            float limitY = GameplayConfig.ArenaHalfHeight - GameplayConfig.PlayerRadius;
            return new Vector2(
                Mathf.Clamp(position.x, -limitX, limitX),
                Mathf.Clamp(position.y, -limitY, limitY));
        }

        public static Vector2 GetRandomPosition(float margin)
        {
            float limitX = GameplayConfig.ArenaHalfWidth - margin;
            float limitY = GameplayConfig.ArenaHalfHeight - margin;
            return new Vector2(Random.Range(-limitX, limitX), Random.Range(-limitY, limitY));
        }
    }
}
