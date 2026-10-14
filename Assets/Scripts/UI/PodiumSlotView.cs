using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class PodiumSlotView : MonoBehaviour
    {
        [SerializeField] private Image cardImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;

        public void Show(string playerName, int score, Color playerColor)
        {
            gameObject.SetActive(true);
            cardImage.color = playerColor;
            nameText.text = playerName;
            scoreText.text = score.ToString();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
