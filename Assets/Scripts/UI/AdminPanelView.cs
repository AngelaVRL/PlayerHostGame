using System.Collections.Generic;
using PlayerHostGame.Gameplay;
using PlayerHostGame.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class AdminPanelView : MonoBehaviour
    {
        [SerializeField] private NetworkSession session;
        [SerializeField] private MatchContext matchContext;
        [SerializeField] private HostAdminCommands adminCommands;
        [SerializeField] private KickRowView[] kickRows;
        [SerializeField] private Slider targetScoreSlider;
        [SerializeField] private Slider playerSpeedSlider;
        [SerializeField] private TMP_Text targetScoreValueText;
        [SerializeField] private TMP_Text playerSpeedValueText;
        [SerializeField] private GameObject rulesLockedText;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;

        private readonly List<PlayerInfo> orderedPlayers = new List<PlayerInfo>();

        public void Open()
        {
            if (session.IsHost)
            {
                gameObject.SetActive(true);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            foreach (KickRowView row in kickRows)
            {
                row.Initialize(HandleKickRequested);
            }

            ConfigureSlider(targetScoreSlider, GameplayConfig.MinTargetScore, GameplayConfig.MaxTargetScore);
            ConfigureSlider(playerSpeedSlider, GameplayConfig.MinPlayerSpeed, GameplayConfig.MaxPlayerSpeed);
        }

        private void OnEnable()
        {
            applyButton.onClick.AddListener(HandleApplyClicked);
            closeButton.onClick.AddListener(Close);
            targetScoreSlider.onValueChanged.AddListener(HandleTargetScoreChanged);
            playerSpeedSlider.onValueChanged.AddListener(HandlePlayerSpeedChanged);
            LoadSlidersFromRules();
        }

        private void OnDisable()
        {
            applyButton.onClick.RemoveListener(HandleApplyClicked);
            closeButton.onClick.RemoveListener(Close);
            targetScoreSlider.onValueChanged.RemoveListener(HandleTargetScoreChanged);
            playerSpeedSlider.onValueChanged.RemoveListener(HandlePlayerSpeedChanged);
        }

        private void Update()
        {
            if (!session.IsHost)
            {
                Close();
                return;
            }

            RefreshKickRows();
            RefreshRulesLock();
        }

        private static void ConfigureSlider(Slider slider, float minimum, float maximum)
        {
            slider.wholeNumbers = true;
            slider.minValue = minimum;
            slider.maxValue = maximum;
        }

        private void LoadSlidersFromRules()
        {
            GameRules rules = matchContext.Model.Rules;
            targetScoreSlider.value = rules.TargetScore;
            playerSpeedSlider.value = rules.PlayerSpeed;
            HandleTargetScoreChanged(targetScoreSlider.value);
            HandlePlayerSpeedChanged(playerSpeedSlider.value);
        }

        private void RefreshKickRows()
        {
            PlayerOrdering.FillById(session.Roster, orderedPlayers);

            int rowIndex = 0;
            foreach (PlayerInfo player in orderedPlayers)
            {
                if (player.Id == session.LocalPlayerId || rowIndex >= kickRows.Length)
                {
                    continue;
                }

                kickRows[rowIndex].Show(player);
                rowIndex++;
            }

            for (; rowIndex < kickRows.Length; rowIndex++)
            {
                kickRows[rowIndex].Hide();
            }
        }

        private void RefreshRulesLock()
        {
            bool canEditRules = matchContext.Model.State != MatchState.Playing;
            targetScoreSlider.interactable = canEditRules;
            playerSpeedSlider.interactable = canEditRules;
            applyButton.interactable = canEditRules;
            rulesLockedText.SetActive(!canEditRules);
        }

        private void HandleTargetScoreChanged(float value)
        {
            targetScoreValueText.text = Mathf.RoundToInt(value).ToString();
        }

        private void HandlePlayerSpeedChanged(float value)
        {
            playerSpeedValueText.text = Mathf.RoundToInt(value).ToString();
        }

        private void HandleApplyClicked()
        {
            adminCommands.TryApplyRules(Mathf.RoundToInt(targetScoreSlider.value), playerSpeedSlider.value);
        }

        private void HandleKickRequested(int playerId)
        {
            adminCommands.KickPlayer(playerId);
        }
    }
}
