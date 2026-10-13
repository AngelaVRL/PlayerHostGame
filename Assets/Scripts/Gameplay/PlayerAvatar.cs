using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class PlayerAvatar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SnapshotInterpolator interpolator;

        public int PlayerId { get; private set; }
        public Vector2 MoveInput { get; set; }

        public void Initialize(int playerId, Color color)
        {
            PlayerId = playerId;
            bodyRenderer.color = color;
        }

        public void ApplyRemoteState(Vector2 position)
        {
            interpolator.AddSample(position);
        }
    }
}
