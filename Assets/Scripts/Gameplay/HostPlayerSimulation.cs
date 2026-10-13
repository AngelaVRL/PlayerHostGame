using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class HostPlayerSimulation : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private PlayerAvatarManager avatarManager;
        [SerializeField] private MatchContext matchContext;

        private void OnEnable()
        {
            session.MessageReceived += HandleMessage;
        }

        private void OnDisable()
        {
            session.MessageReceived -= HandleMessage;
        }

        private void Update()
        {
            if (!session.IsHost || matchContext.Model.IsPaused)
            {
                return;
            }

            float speed = matchContext.Model.Rules.PlayerSpeed;
            foreach (PlayerAvatar avatar in avatarManager.Avatars)
            {
                MoveAvatar(avatar, speed);
            }
        }

        private void HandleMessage(int senderId, NetworkMessage message)
        {
            if (!session.IsHost || message.Type != MessageType.InputUpdate)
            {
                return;
            }

            if (GameMessageCodec.TryDecodeInput(message, out Vector2Int input)
                && avatarManager.TryGetAvatar(senderId, out PlayerAvatar avatar))
            {
                avatar.MoveInput = input;
            }
        }

        private static void MoveAvatar(PlayerAvatar avatar, float speed)
        {
            Vector2 velocity = avatar.MoveInput.normalized * speed;
            Vector2 nextPosition = (Vector2)avatar.transform.position + velocity * Time.deltaTime;
            avatar.transform.position = ArenaBounds.Clamp(nextPosition);
        }
    }
}
