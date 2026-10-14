using System.Collections.Generic;
using PlayerHostGame.Gameplay;
using PlayerHostGame.Networking;

namespace PlayerHostGame.UI
{
    public static class PlayerOrdering
    {
        public static void FillById(PlayerRoster roster, List<PlayerInfo> ordered)
        {
            ordered.Clear();
            foreach (PlayerInfo player in roster.Players)
            {
                int index = ordered.Count;
                while (index > 0 && player.Id < ordered[index - 1].Id)
                {
                    index--;
                }

                ordered.Insert(index, player);
            }
        }

        public static void FillByScore(PlayerRoster roster, MatchModel model, List<PlayerInfo> ordered)
        {
            ordered.Clear();
            foreach (PlayerInfo player in roster.Players)
            {
                int index = ordered.Count;
                while (index > 0 && IsRankedAbove(player, ordered[index - 1], model))
                {
                    index--;
                }

                ordered.Insert(index, player);
            }
        }

        private static bool IsRankedAbove(PlayerInfo candidate, PlayerInfo other, MatchModel model)
        {
            int candidateScore = model.GetScore(candidate.Id);
            int otherScore = model.GetScore(other.Id);
            if (candidateScore != otherScore)
            {
                return candidateScore > otherScore;
            }

            return candidate.Id < other.Id;
        }
    }
}
