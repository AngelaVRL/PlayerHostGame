using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHostGame.UI
{
    public sealed class PlayerSlotView : MonoBehaviour
    {
        [SerializeField] private Image cardImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private Color emptyColor = new Color(0.14f, 0.36f, 0.36f);
        [SerializeField] private string emptyLabel = "Esperando...";

        public void ShowPlayer(string displayName, Color color, string detail)
        {
            cardImage.color = color;
            nameText.text = displayName;
            SetDetail(detail);
        }

        public void ShowEmpty()
        {
            cardImage.color = emptyColor;
            nameText.text = emptyLabel;
            SetDetail(string.Empty);
        }

        private void SetDetail(string detail)
        {
            if (detailText != null)
            {
                detailText.text = detail;
            }
        }
    }
}
