using System;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class GameRules
    {
        public int TargetScore { get; private set; } = GameplayConfig.DefaultTargetScore;
        public float PlayerSpeed { get; private set; } = GameplayConfig.DefaultPlayerSpeed;

        public event Action Changed;

        public void Apply(int targetScore, float playerSpeed)
        {
            TargetScore = Mathf.Clamp(targetScore, GameplayConfig.MinTargetScore, GameplayConfig.MaxTargetScore);
            PlayerSpeed = Mathf.Clamp(playerSpeed, GameplayConfig.MinPlayerSpeed, GameplayConfig.MaxPlayerSpeed);
            Changed?.Invoke();
        }

        public void Reset()
        {
            Apply(GameplayConfig.DefaultTargetScore, GameplayConfig.DefaultPlayerSpeed);
        }
    }
}
