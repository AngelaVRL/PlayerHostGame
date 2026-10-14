using System.Collections.Generic;
using PlayerHostGame.Gameplay;
using PlayerHostGame.Networking;
using TMPro;
using UnityEngine;

namespace PlayerHostGame.UI
{
    public sealed class HudView : MonoBehaviour
    {
        private const string TargetFormat = "Meta: {0} monedas";

        [SerializeField] private NetworkSession session;
        [SerializeField] private MatchContext matchContext;
        [SerializeField] private HostAdminCommands adminCommands;
        [SerializeField] private ScoreRowView[] rows;
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private GameObject hostHint;

        private readonly List<PlayerInfo> rankedPlayers = new List<PlayerInfo>();

        private void OnEnable()
        {
            hostHint.SetActive(session.IsHost);
        }

        private void Update()
        {
            if (session.IsHost && Input.GetKeyDown(KeyCode.Escape))
            {
                adminCommands.TogglePause();
            }

            PlayerRoster roster = session.Roster;
            if (roster == null)
            {
                return;
            }

            MatchModel model = matchContext.Model;
            targetText.text = string.Format(TargetFormat, model.Rules.TargetScore);
            PlayerOrdering.FillByScore(roster, model, rankedPlayers);

            for (int index = 0; index < rows.Length; index++)
            {
                if (index < rankedPlayers.Count)
                {
                    ShowRow(rows[index], rankedPlayers[index], model);
                }
                else
                {
                    rows[index].Hide();
                }
            }
        }

        private void ShowRow(ScoreRowView row, PlayerInfo player, MatchModel model)
        {
            bool isLocal = player.Id == session.LocalPlayerId;
            string displayName = isLocal ? player.Name + " (tú)" : player.Name;
            row.Show(displayName, model.GetScore(player.Id), PlayerColors.GetColor(player.Id), isLocal);
        }
    }
}
