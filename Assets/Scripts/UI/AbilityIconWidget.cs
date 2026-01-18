using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    public class AbilityIconWidget : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Text keyLabel;
        [SerializeField] private Text cooldownText;
        [SerializeField] private Text emptyLabel;
        [SerializeField] private Text abilityNameLabel;
        [SerializeField] private Color cooldownColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color emptyColor = new Color(0.35f, 0.35f, 0.35f);
        private Color baseColor = Color.white;
        private static Sprite placeholderSprite;

        public void Configure(string keyText, Color baseColor)
        {
            this.baseColor = baseColor;
            if (iconImage != null)
            {
                iconImage.color = baseColor;
                if (iconImage.sprite == null)
                {
                    iconImage.sprite = GetPlaceholderSprite();
                }
            }

            if (keyLabel != null)
            {
                keyLabel.text = keyText;
            }
        }

        public void SetEmpty()
        {
            SetAbilityInfo(null, null);
            SetState(0f, 0f, true);
            if (emptyLabel != null)
            {
                emptyLabel.text = "—";
                emptyLabel.enabled = true;
            }
        }

        public void SetState(float remaining, float total, bool isEmpty)
        {
            if (iconImage != null)
            {
                if (isEmpty)
                {
                    iconImage.color = emptyColor;
                }
                else if (remaining > 0f && total > 0f)
                {
                    iconImage.color = Color.Lerp(baseColor, cooldownColor, 0.55f);
                }
                else
                {
                    iconImage.color = baseColor;
                }
            }

            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = !isEmpty && remaining > 0f && total > 0f;
                if (cooldownOverlay.enabled)
                {
                    cooldownOverlay.fillAmount = Mathf.Clamp01(remaining / total);
                }
            }

            if (cooldownText != null)
            {
                bool showCooldown = !isEmpty && remaining > 0f;
                cooldownText.enabled = showCooldown;
                if (showCooldown)
                {
                    cooldownText.text = remaining < 10f ? remaining.ToString("F1") : Mathf.CeilToInt(remaining).ToString();
                }
            }

            if (emptyLabel != null)
            {
                emptyLabel.enabled = isEmpty;
            }

            if (keyLabel != null)
            {
                keyLabel.enabled = !isEmpty;
            }

            if (abilityNameLabel != null)
            {
                abilityNameLabel.enabled = !isEmpty;
            }
        }

        public void SetAbilityInfo(string displayName, Sprite icon)
        {
            if (abilityNameLabel != null)
            {
                abilityNameLabel.text = displayName ?? string.Empty;
            }

            if (iconImage != null)
            {
                iconImage.sprite = icon != null ? icon : GetPlaceholderSprite();
            }
        }

        private static Sprite GetPlaceholderSprite()
        {
            if (placeholderSprite != null)
            {
                return placeholderSprite;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;

            placeholderSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            placeholderSprite.hideFlags = HideFlags.HideAndDontSave;
            return placeholderSprite;
        }
    }
}
