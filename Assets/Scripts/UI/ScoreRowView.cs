using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class ScoreRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Image colorSwatch;
        [SerializeField] private Color localNameColor = new Color(1f, 0.42f, 0.42f);
        [SerializeField] private Color defaultNameColor = Color.white;

        public void Show(string displayName, int score, Color playerColor, bool isLocal)
        {
            gameObject.SetActive(true);
            nameText.text = displayName;
            nameText.color = isLocal ? localNameColor : defaultNameColor;
            scoreText.text = score.ToString();

            if (colorSwatch != null)
            {
                colorSwatch.color = playerColor;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
