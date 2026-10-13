using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class LocalInputController : MonoBehaviour
    {
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";

        [SerializeField] private NetworkSession session;
        [SerializeField] private PlayerAvatarManager avatarManager;

        private Vector2Int lastSentInput;

        private void Update()
        {
            if (!session.IsSessionActive)
            {
                lastSentInput = Vector2Int.zero;
                return;
            }

            Vector2Int input = ReadInput();
            if (session.IsHost)
            {
                ApplyToLocalAvatar(input);
            }
            else
            {
                SendIfChanged(input);
            }
        }

        private static Vector2Int ReadInput()
        {
            return new Vector2Int(
                Mathf.RoundToInt(Input.GetAxisRaw(HorizontalAxis)),
                Mathf.RoundToInt(Input.GetAxisRaw(VerticalAxis)));
        }

        private void ApplyToLocalAvatar(Vector2Int input)
        {
            if (avatarManager.TryGetAvatar(session.LocalPlayerId, out PlayerAvatar avatar))
            {
                avatar.MoveInput = input;
            }
        }

        private void SendIfChanged(Vector2Int input)
        {
            if (input == lastSentInput)
            {
                return;
            }

            lastSentInput = input;
            session.SendToHost(GameMessageCodec.EncodeInput(input));
        }
    }
}
