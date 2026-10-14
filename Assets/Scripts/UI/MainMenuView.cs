using PlayerHostGame.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class MainMenuView : MonoBehaviour
    {
        private const string ConnectingMessage = "Conectando...";

        [SerializeField] private NetworkSession session;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField addressInput;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_Text statusText;

        private void Awake()
        {
            nameInput.characterLimit = NetworkConfig.MaxPlayerNameLength;
        }

        private void OnEnable()
        {
            hostButton.onClick.AddListener(HandleHostClicked);
            joinButton.onClick.AddListener(HandleJoinClicked);
        }

        private void OnDisable()
        {
            hostButton.onClick.RemoveListener(HandleHostClicked);
            joinButton.onClick.RemoveListener(HandleJoinClicked);
        }

        private void Update()
        {
            bool isConnecting = session.Client != null;
            hostButton.interactable = !isConnecting;
            joinButton.interactable = !isConnecting;
        }

        public void ShowStatus(string message)
        {
            statusText.text = message;
        }

        private void HandleHostClicked()
        {
            bool started = session.StartHost(nameInput.text, out string errorMessage);
            ShowStatus(started ? string.Empty : errorMessage);
        }

        private void HandleJoinClicked()
        {
            bool started = session.StartClient(nameInput.text, addressInput.text, out string errorMessage);
            ShowStatus(started ? ConnectingMessage : errorMessage);
        }
    }
}
