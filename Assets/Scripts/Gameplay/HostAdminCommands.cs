using PlayerHostGame.Networking;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class HostAdminCommands : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private MatchContext matchContext;

        public void KickPlayer(int playerId)
        {
            if (session.IsHost)
            {
                session.KickPlayer(playerId);
            }
        }

        public void TogglePause()
        {
            MatchModel model = matchContext.Model;
            if (!session.IsHost || model.State != MatchState.Playing)
            {
                return;
            }

            bool isPaused = !model.IsPaused;
            model.SetPaused(isPaused);
            session.BroadcastToClients(MatchMessageCodec.EncodePause(isPaused));
        }

        public bool TryApplyRules(int targetScore, float playerSpeed)
        {
            MatchModel model = matchContext.Model;
            if (!session.IsHost || model.State == MatchState.Playing)
            {
                return false;
            }

            model.Rules.Apply(targetScore, playerSpeed);
            session.BroadcastToClients(MatchMessageCodec.EncodeRules(model.Rules));
            return true;
        }
    }
}
