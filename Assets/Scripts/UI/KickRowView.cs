using System;
using PlayerHostGame.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class KickRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text pingText;
        [SerializeField] private Button kickButton;

        private int playerId;
        private Action<int> kickRequested;

        public void Initialize(Action<int> onKickRequested)
        {
            kickRequested = onKickRequested;
            kickButton.onClick.AddListener(HandleKickClicked);
        }

        public void Show(PlayerInfo player)
        {
            gameObject.SetActive(true);
            playerId = player.Id;
            nameText.text = player.Name;
            pingText.text = player.PingMilliseconds + " ms";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleKickClicked()
        {
            kickRequested?.Invoke(playerId);
        }
    }
}
