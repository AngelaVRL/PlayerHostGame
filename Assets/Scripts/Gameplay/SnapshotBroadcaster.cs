using System.Collections.Generic;
using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class SnapshotBroadcaster : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private PlayerAvatarManager avatarManager;

        private readonly List<PlayerState> states = new List<PlayerState>();
        private float nextSnapshotTime;

        private void Update()
        {
            if (!session.IsHost || Time.unscaledTime < nextSnapshotTime)
            {
                return;
            }

            nextSnapshotTime = Time.unscaledTime + 1f / GameplayConfig.SnapshotsPerSecond;

            states.Clear();
            foreach (PlayerAvatar avatar in avatarManager.Avatars)
            {
                states.Add(new PlayerState(avatar.PlayerId, avatar.transform.position));
            }

            session.BroadcastToClients(GameMessageCodec.EncodeSnapshot(states));
        }
    }
}
