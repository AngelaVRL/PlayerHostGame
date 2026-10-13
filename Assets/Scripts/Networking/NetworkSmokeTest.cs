using PlayerHostGame.Gameplay;
using UnityEngine;

namespace PlayerHostGame.Networking
{
    public sealed class NetworkSmokeTest : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private MatchContext matchContext;
        [SerializeField] private HostMatchController hostMatchController;
        [SerializeField] private HostAdminCommands adminCommands;
        [SerializeField] private string hostAddress = "127.0.0.1";

        private string playerName;
        private string status = "Sin conexión";
        private bool useFastRules;

        private void Awake()
        {
            playerName = "Player" + Random.Range(100, 1000);
            session.PlayerJoined += player => status = "Entró: " + player.Name;
            session.PlayerLeft += player => status = "Salió: " + player.Name;
            session.ConnectionClosed += reason => status = "Cerrada: " + reason;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                bool started = session.StartHost(playerName, out string error);
                status = started ? "Host activo" : error;
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                bool started = session.StartClient(playerName, hostAddress, out string error);
                status = started ? "Conectando..." : error;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                hostMatchController.StartMatch();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                KickFirstRemotePlayer();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                adminCommands.TogglePause();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ToggleRules();
            }
        }

        private void KickFirstRemotePlayer()
        {
            PlayerRoster roster = session.Roster;
            if (roster == null)
            {
                return;
            }

            foreach (PlayerInfo player in roster.Players)
            {
                if (player.Id != session.LocalPlayerId)
                {
                    adminCommands.KickPlayer(player.Id);
                    return;
                }
            }
        }

        private void ToggleRules()
        {
            useFastRules = !useFastRules;
            int targetScore = useFastRules ? 5 : GameplayConfig.DefaultTargetScore;
            float playerSpeed = useFastRules ? 8f : GameplayConfig.DefaultPlayerSpeed;
            adminCommands.TryApplyRules(targetScore, playerSpeed);
        }

        private void OnGUI()
        {
            GUILayout.Label("H = host | J = unirse a " + hostAddress + " | Yo: " + playerName);
            GUILayout.Label("Host: Enter = iniciar | K = expulsar | P = pausa | R = cambiar reglas (fuera de partida)");
            GUILayout.Label(status);

            PlayerRoster roster = session.Roster;
            if (roster == null)
            {
                return;
            }

            MatchModel model = matchContext.Model;
            string pauseText = model.IsPaused ? " | PAUSA" : string.Empty;
            GUILayout.Label("Partida: " + model.State + pauseText);
            GUILayout.Label("Reglas: meta " + model.Rules.TargetScore + " | velocidad " + model.Rules.PlayerSpeed);

            if (model.State == MatchState.Finished && roster.TryGet(model.WinnerId, out PlayerInfo winner))
            {
                GUILayout.Label("Ganador: " + winner.Name);
            }

            foreach (PlayerInfo player in roster.Players)
            {
                GUILayout.Label(player.Id + " - " + player.Name + " - " + player.PingMilliseconds + " ms - " + model.GetScore(player.Id) + " monedas");
            }
        }
    }
}
