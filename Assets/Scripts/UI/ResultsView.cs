using System.Collections.Generic;
using PlayerHostGame.Gameplay;
using PlayerHostGame.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class ResultsView : MonoBehaviour
    {
        private const string WinnerFormat = "¡Ganó {0}!";
        private const string NoWinnerTitle = "Partida terminada";

        [SerializeField] private NetworkSession session;
        [SerializeField] private MatchContext matchContext;
        [SerializeField] private HostMatchController hostMatchController;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private PodiumSlotView[] podiumSlots;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private GameObject waitingForHostText;

        private readonly List<PlayerInfo> rankedPlayers = new List<PlayerInfo>();

        private void OnEnable()
        {
            playAgainButton.onClick.AddListener(hostMatchController.StartMatch);
            leaveButton.onClick.AddListener(session.Shutdown);

            bool isHost = session.IsHost;
            playAgainButton.gameObject.SetActive(isHost);
            waitingForHostText.SetActive(!isHost);
            Refresh();
        }

        private void OnDisable()
        {
            playAgainButton.onClick.RemoveListener(hostMatchController.StartMatch);
            leaveButton.onClick.RemoveListener(session.Shutdown);
        }

        private void Refresh()
        {
            PlayerRoster roster = session.Roster;
            if (roster == null)
            {
                return;
            }

            MatchModel model = matchContext.Model;
            titleText.text = roster.TryGet(model.WinnerId, out PlayerInfo winner)
                ? string.Format(WinnerFormat, winner.Name)
                : NoWinnerTitle;

            PlayerOrdering.FillByScore(roster, model, rankedPlayers);
            for (int index = 0; index < podiumSlots.Length; index++)
            {
                if (index < rankedPlayers.Count)
                {
                    PlayerInfo player = rankedPlayers[index];
                    podiumSlots[index].Show(player.Name, model.GetScore(player.Id), PlayerColors.GetColor(player.Id));
                }
                else
                {
                    podiumSlots[index].Hide();
                }
            }
        }
    }
}
