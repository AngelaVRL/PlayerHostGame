using System.Collections.Generic;
using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class PlayerAvatarManager : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private PlayerAvatar avatarPrefab;

        private readonly Dictionary<int, PlayerAvatar> avatarsById = new Dictionary<int, PlayerAvatar>();

        public IEnumerable<PlayerAvatar> Avatars
        {
            get { return avatarsById.Values; }
        }

        private void OnEnable()
        {
            session.PlayerJoined += HandlePlayerJoined;
            session.PlayerLeft += HandlePlayerLeft;
            session.ConnectionClosed += HandleConnectionClosed;
        }

        private void OnDisable()
        {
            session.PlayerJoined -= HandlePlayerJoined;
            session.PlayerLeft -= HandlePlayerLeft;
            session.ConnectionClosed -= HandleConnectionClosed;
        }

        public bool TryGetAvatar(int playerId, out PlayerAvatar avatar)
        {
            return avatarsById.TryGetValue(playerId, out avatar);
        }

        private void HandlePlayerJoined(PlayerInfo player)
        {
            if (avatarsById.ContainsKey(player.Id))
            {
                return;
            }

            PlayerAvatar avatar = Instantiate(avatarPrefab, GetSpawnPosition(player.Id), Quaternion.identity, transform);
            avatar.Initialize(player.Id, PlayerColors.GetColor(player.Id));
            avatarsById[player.Id] = avatar;
        }

        private void HandlePlayerLeft(PlayerInfo player)
        {
            if (avatarsById.TryGetValue(player.Id, out PlayerAvatar avatar))
            {
                Destroy(avatar.gameObject);
                avatarsById.Remove(player.Id);
            }
        }

        private void HandleConnectionClosed(string reason)
        {
            foreach (PlayerAvatar avatar in avatarsById.Values)
            {
                Destroy(avatar.gameObject);
            }

            avatarsById.Clear();
        }

        private static Vector2 GetSpawnPosition(int playerId)
        {
            int slot = playerId % NetworkConfig.MaxPlayers;
            float centeredSlot = slot - (NetworkConfig.MaxPlayers - 1) * 0.5f;
            return new Vector2(centeredSlot * GameplayConfig.SpawnSpacing, 0f);
        }
    }
}
