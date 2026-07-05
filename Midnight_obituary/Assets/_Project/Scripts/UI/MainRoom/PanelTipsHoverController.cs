using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Items;

namespace ObituaryTomorrow.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class PanelTipsHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Text smokingCountText;
        [SerializeField] private GameObject noStressText;
        [SerializeField] private GameObject noSmokingText;
        [SerializeField] private CigaretteSystem cigaretteSystem;
        [SerializeField] private RectTransform additionalHoverArea;

        private int hoverRefCount;

        private void Awake()
        {
            ResolveReferences();
            EnsureRaycastTarget();
            DisableBlockingTipRaycasts();
            HideCountText();
        }

        private void Start()
        {
            RegisterAdditionalHoverArea();
        }

        private void OnEnable()
        {
            GameEventBus.CigaretteChanged += OnCigaretteChanged;
        }

        private void OnDisable()
        {
            GameEventBus.CigaretteChanged -= OnCigaretteChanged;
            hoverRefCount = 0;
            HideCountText();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            NotifyPointerEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            NotifyPointerExit();
        }

        public void NotifyPointerEnter()
        {
            hoverRefCount++;
            UpdateAndShowCountText();
        }

        public void NotifyPointerExit()
        {
            hoverRefCount = Mathf.Max(0, hoverRefCount - 1);
            if (hoverRefCount == 0)
            {
                HideCountText();
            }
        }

        public void RefreshVisibleCountText()
        {
            if (hoverRefCount > 0)
            {
                UpdateAndShowCountText();
            }
        }

        private void OnCigaretteChanged(CigaretteChangedEventArgs args)
        {
            RefreshVisibleCountText();
        }

        private void UpdateAndShowCountText()
        {
            if (smokingCountText == null || IsBlockingTipVisible())
            {
                HideCountText();
                return;
            }

            if (cigaretteSystem == null)
            {
                cigaretteSystem = FindFirstObjectByType<CigaretteSystem>();
            }

            int count = cigaretteSystem != null ? cigaretteSystem.Count : 0;
            smokingCountText.text = $"\u9999\u70df\u6570\u91cf\uff1a{count}";
            smokingCountText.gameObject.SetActive(true);
        }

        private bool IsBlockingTipVisible()
        {
            return (noStressText != null && noStressText.activeSelf)
                || (noSmokingText != null && noSmokingText.activeSelf);
        }

        private void HideCountText()
        {
            if (smokingCountText != null)
            {
                smokingCountText.gameObject.SetActive(false);
            }
        }

        private void EnsureRaycastTarget()
        {
            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = true;
            }

            if (smokingCountText != null)
            {
                smokingCountText.raycastTarget = false;
            }
        }

        private void DisableBlockingTipRaycasts()
        {
            DisableRaycastOnGraphic(noStressText);
            DisableRaycastOnGraphic(noSmokingText);
        }

        private static void DisableRaycastOnGraphic(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = false;
            }
        }

        private void RegisterAdditionalHoverArea()
        {
            if (additionalHoverArea == null)
            {
                Button smokingButton = FindFirstObjectByType<MainRoomController>() != null
                    ? FindButtonByName("Button_Smoking")
                    : null;
                if (smokingButton != null)
                {
                    additionalHoverArea = smokingButton.transform as RectTransform;
                }
            }

            if (additionalHoverArea == null || additionalHoverArea.gameObject == gameObject)
            {
                return;
            }

            PanelTipsHoverForwarder forwarder = additionalHoverArea.GetComponent<PanelTipsHoverForwarder>();
            if (forwarder == null)
            {
                forwarder = additionalHoverArea.gameObject.AddComponent<PanelTipsHoverForwarder>();
            }

            forwarder.Bind(this);
        }

        private static Button FindButtonByName(string objectName)
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name == objectName)
                {
                    return buttons[i];
                }
            }

            return null;
        }

        private void ResolveReferences()
        {
            if (smokingCountText == null)
            {
                smokingCountText = FindTextInChildren(transform, "SmokingCount_Text");
            }

            if (noStressText == null)
            {
                noStressText = FindGameObjectInChildren(transform, "NoStress_Text");
            }

            if (noSmokingText == null)
            {
                noSmokingText = FindGameObjectInChildren(transform, "NoSmoking_Text");
            }

            if (cigaretteSystem == null)
            {
                cigaretteSystem = FindFirstObjectByType<CigaretteSystem>();
            }
        }

        private static Text FindTextInChildren(Transform root, string objectName)
        {
            Transform found = FindChildTransform(root, objectName);
            return found != null ? found.GetComponent<Text>() : null;
        }

        private static GameObject FindGameObjectInChildren(Transform root, string objectName)
        {
            Transform found = FindChildTransform(root, objectName);
            return found != null ? found.gameObject : null;
        }

        private static Transform FindChildTransform(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildTransform(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
