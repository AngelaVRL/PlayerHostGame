using System;
using System.Collections.Generic;

namespace PlayerHostGame.Networking
{
    public sealed class PlayerRoster
    {
        private readonly Dictionary<int, PlayerInfo> playersById = new Dictionary<int, PlayerInfo>();

        public int Count
        {
            get { return playersById.Count; }
        }

        public IEnumerable<PlayerInfo> Players
        {
            get { return playersById.Values; }
        }

        public PlayerInfo Add(int id, string name)
        {
            PlayerInfo player = new PlayerInfo(id, name);
            playersById[id] = player;
            return player;
        }

        public bool TryGet(int id, out PlayerInfo player)
        {
            return playersById.TryGetValue(id, out player);
        }

        public bool TryRemove(int id, out PlayerInfo player)
        {
            if (!playersById.TryGetValue(id, out player))
            {
                return false;
            }

            playersById.Remove(id);
            return true;
        }

        public bool ContainsName(string name)
        {
            foreach (PlayerInfo player in playersById.Values)
            {
                if (string.Equals(player.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            playersById.Clear();
        }
    }
}
