using System.Collections.Generic;
using PlayerHostGame.Gameplay;
using PlayerHostGame.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class LobbyView : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private HostMatchController hostMatchController;
        [SerializeField] private AdminPanelView adminPanel;
        [SerializeField] private PlayerSlotView[] slots;
        [SerializeField] private Button startButton;
        [SerializeField] private Button adminButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private GameObject waitingForHostText;

        private readonly List<PlayerInfo> orderedPlayers = new List<PlayerInfo>();

        private void OnEnable()
        {
            startButton.onClick.AddListener(hostMatchController.StartMatch);
            adminButton.onClick.AddListener(adminPanel.Open);
            leaveButton.onClick.AddListener(session.Shutdown);

            bool isHost = session.IsHost;
            startButton.gameObject.SetActive(isHost);
            adminButton.gameObject.SetActive(isHost);
            waitingForHostText.SetActive(!isHost);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(hostMatchController.StartMatch);
            adminButton.onClick.RemoveListener(adminPanel.Open);
            leaveButton.onClick.RemoveListener(session.Shutdown);
        }

        private void Update()
        {
            PlayerRoster roster = session.Roster;
            if (roster == null)
            {
                return;
            }

            PlayerOrdering.FillById(roster, orderedPlayers);
            for (int index = 0; index < slots.Length; index++)
            {
                if (index < orderedPlayers.Count)
                {
                    ShowPlayer(slots[index], orderedPlayers[index]);
                }
                else
                {
                    slots[index].ShowEmpty();
                }
            }
        }

        private void ShowPlayer(PlayerSlotView slot, PlayerInfo player)
        {
            bool isLocal = player.Id == session.LocalPlayerId;
            string displayName = isLocal ? player.Name + " (tú)" : player.Name;
            slot.ShowPlayer(displayName, PlayerColors.GetColor(player.Id), GetDetail(player));
        }

        private string GetDetail(PlayerInfo player)
        {
            if (player.Id == HostServer.HostPlayerId)
            {
                return "Host";
            }

            return session.IsHost ? player.PingMilliseconds + " ms" : string.Empty;
        }
    }
}
