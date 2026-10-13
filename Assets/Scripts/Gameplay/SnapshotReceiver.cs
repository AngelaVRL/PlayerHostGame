using System.Collections.Generic;
using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class SnapshotReceiver : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private PlayerAvatarManager avatarManager;

        private readonly List<PlayerState> states = new List<PlayerState>();

        private void OnEnable()
        {
            session.MessageReceived += HandleMessage;
        }

        private void OnDisable()
        {
            session.MessageReceived -= HandleMessage;
        }

        private void HandleMessage(int senderId, NetworkMessage message)
        {
            if (session.IsHost || message.Type != MessageType.StateSnapshot)
            {
                return;
            }

            if (!GameMessageCodec.TryDecodeSnapshot(message, states))
            {
                return;
            }

            foreach (PlayerState state in states)
            {
                if (avatarManager.TryGetAvatar(state.PlayerId, out PlayerAvatar avatar))
                {
                    avatar.ApplyRemoteState(state.Position);
                }
            }
        }
    }
}
