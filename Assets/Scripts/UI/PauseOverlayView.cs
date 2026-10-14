using PlayerHostGame.Gameplay;
using PlayerHostGame.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class PauseOverlayView : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private HostAdminCommands adminCommands;
        [SerializeField] private AdminPanelView adminPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button adminButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private GameObject waitingForHostText;

        private void OnEnable()
        {
            resumeButton.onClick.AddListener(adminCommands.TogglePause);
            adminButton.onClick.AddListener(adminPanel.Open);
            leaveButton.onClick.AddListener(session.Shutdown);

            bool isHost = session.IsHost;
            resumeButton.gameObject.SetActive(isHost);
            adminButton.gameObject.SetActive(isHost);
            waitingForHostText.SetActive(!isHost);
        }

        private void OnDisable()
        {
            resumeButton.onClick.RemoveListener(adminCommands.TogglePause);
            adminButton.onClick.RemoveListener(adminPanel.Open);
            leaveButton.onClick.RemoveListener(session.Shutdown);
        }
    }
}
