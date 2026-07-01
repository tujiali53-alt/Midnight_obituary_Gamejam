using UnityEngine;
using UnityEngine.UI;

namespace ObituaryTomorrow.UI
{
    /// <summary>
    /// Configures a Unity UI Button to swap sprites on hover / press.
    /// Attach to the same GameObject as Button + Image (Target Graphic).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonSpriteSwap : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite highlightedSprite;
        [SerializeField] private Sprite pressedSprite;

        [Header("Behavior")]
        [Tooltip("When enabled, clicking shows Pressed Sprite. When disabled, only hover highlight is used.")]
        [SerializeField] private bool usePressedSprite;

        private Button button;
        private Image targetImage;

        private void Reset()
        {
            CacheReferences();
            if (targetImage != null && normalSprite == null)
            {
                normalSprite = targetImage.sprite;
            }
        }

        private void Awake()
        {
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            Apply();
        }
#endif

        public void Apply()
        {
            CacheReferences();

            if (button == null || targetImage == null)
            {
                return;
            }

            if (normalSprite != null)
            {
                targetImage.sprite = normalSprite;
            }

            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = highlightedSprite != null ? highlightedSprite : normalSprite,
                pressedSprite = usePressedSprite ? pressedSprite : null,
                selectedSprite = highlightedSprite != null ? highlightedSprite : normalSprite,
                disabledSprite = normalSprite
            };

            // Prevent Color Tint from washing out custom art.
            var color = targetImage.color;
            color.r = 1f;
            color.g = 1f;
            color.b = 1f;
            color.a = 1f;
            targetImage.color = color;
        }

        private void CacheReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (targetImage == null && button != null && button.targetGraphic is Image image)
            {
                targetImage = image;
            }
        }
    }
}
