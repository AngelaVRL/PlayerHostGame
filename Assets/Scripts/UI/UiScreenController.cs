using PlayerHostGame.Gameplay;
using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.UI
{
    public sealed class UiScreenController : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private MatchContext matchContext;
        [SerializeField] private MainMenuView mainMenu;
        [SerializeField] private GameObject menuScreen;
        [SerializeField] private GameObject lobbyScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject pauseOverlay;
        [SerializeField] private GameObject resultsScreen;

        private void OnEnable()
        {
            session.ConnectionClosed += HandleConnectionClosed;
        }

        private void OnDisable()
        {
            session.ConnectionClosed -= HandleConnectionClosed;
        }

        private void Update()
        {
            MatchModel model = matchContext.Model;
            bool inSession = session.IsSessionActive;
            bool isPlaying = inSession && model.State == MatchState.Playing;

            menuScreen.SetActive(!inSession);
            lobbyScreen.SetActive(inSession && model.State == MatchState.Lobby);
            hudScreen.SetActive(isPlaying);
            pauseOverlay.SetActive(isPlaying && model.IsPaused);
            resultsScreen.SetActive(inSession && model.State == MatchState.Finished);
        }

        private void HandleConnectionClosed(string reason)
        {
            mainMenu.ShowStatus(reason);
        }
    }
}
