using System;
using UnityEngine;
using UnityEngine.UI;
using ObituaryTomorrow.Core;

namespace ObituaryTomorrow.UI
{
    public sealed class PlayerCardMessageController : MonoBehaviour
    {
        [Serializable]
        private struct PersonalityTipMapping
        {
            public PersonalityTag tag;
            public GameObject tipText;
        }

        [Header("Panels")]
        [SerializeField] private GameObject panelPopupRoot;
        [SerializeField] private GameObject panelCardMessage;

        [Header("Dialogue Area Buttons")]
        [SerializeField] private Button dialogueCardButton01;
        [SerializeField] private Button dialogueCardButton02;

        [Header("Message Panel")]
        [SerializeField] private Button messageCardButton01;
        [SerializeField] private Button messageCardButton02;
        [SerializeField] private Image messageCardImage01;
        [SerializeField] private Image messageCardImage02;
        [SerializeField] private Button confirmButton;

        [Header("Personality Tips")]
        [SerializeField] private PersonalityTipMapping[] personalityTips = new PersonalityTipMapping[4];

        private Sprite gachaSprite01;
        private Sprite gachaSprite02;
        private PersonalityTag selectedTag01;
        private PersonalityTag selectedTag02;
        private bool hasSelectedCards;
        private bool dialogueListenersRegistered;

        private void Awake()
        {
            ResolveReferences();
            HidePanelImmediate();
            HideAllTips();
        }

        private void Start()
        {
            ResolveReferences();
            RegisterDialogueButtonListeners();
        }

        private void OnEnable()
        {
            RegisterMessagePanelListeners();
        }

        private void OnDisable()
        {
            UnregisterMessagePanelListeners();
        }

        private void OnDestroy()
        {
            UnregisterDialogueButtonListeners();
            UnregisterMessagePanelListeners();
        }

        public void SetSelectedCards(
            Sprite sprite01,
            Sprite sprite02,
            PersonalityTag tag01,
            PersonalityTag tag02)
        {
            gachaSprite01 = sprite01;
            gachaSprite02 = sprite02;
            selectedTag01 = tag01;
            selectedTag02 = tag02;
            hasSelectedCards = true;

            ApplyMessageCardSprites();
            SetDialogueButtonsInteractable(true);
        }

        public void ClearSelectedCards()
        {
            gachaSprite01 = null;
            gachaSprite02 = null;
            hasSelectedCards = false;

            ClearImage(messageCardImage01);
            ClearImage(messageCardImage02);
            SetDialogueButtonsInteractable(false);
            HidePanelImmediate();
        }

        private void OpenPanel()
        {
            if (!hasSelectedCards || panelCardMessage == null)
            {
                return;
            }

            if (panelPopupRoot != null)
            {
                panelPopupRoot.SetActive(true);
            }

            panelCardMessage.SetActive(true);
            ApplyMessageCardSprites();
            HideAllTips();
        }

        private void ClosePanel()
        {
            HidePanelImmediate();
        }

        private void ShowTipForCard01()
        {
            ShowTipForTag(selectedTag01);
        }

        private void ShowTipForCard02()
        {
            ShowTipForTag(selectedTag02);
        }

        private void ShowTipForTag(PersonalityTag tag)
        {
            HideAllTips();

            GameObject tipObject = GetTipObjectForTag(tag);
            if (tipObject != null)
            {
                tipObject.SetActive(true);
            }
        }

        private void HidePanelImmediate()
        {
            if (panelCardMessage != null)
            {
                panelCardMessage.SetActive(false);
            }

            HideAllTips();

            if (panelPopupRoot != null)
            {
                panelPopupRoot.SetActive(false);
            }
        }

        private void HideAllTips()
        {
            if (personalityTips == null)
            {
                return;
            }

            for (int i = 0; i < personalityTips.Length; i++)
            {
                GameObject tipObject = personalityTips[i].tipText;
                if (tipObject != null)
                {
                    tipObject.SetActive(false);
                }
            }
        }

        private void ApplyMessageCardSprites()
        {
            ApplySprite(messageCardImage01, gachaSprite01);
            ApplySprite(messageCardImage02, gachaSprite02);
        }

        private GameObject GetTipObjectForTag(PersonalityTag tag)
        {
            if (personalityTips == null)
            {
                return null;
            }

            for (int i = 0; i < personalityTips.Length; i++)
            {
                if (personalityTips[i].tag == tag)
                {
                    return personalityTips[i].tipText;
                }
            }

            return null;
        }

        private void SetDialogueButtonsInteractable(bool interactable)
        {
            SetButtonInteractable(dialogueCardButton01, interactable);
            SetButtonInteractable(dialogueCardButton02, interactable);
        }

        private void RegisterDialogueButtonListeners()
        {
            if (dialogueListenersRegistered)
            {
                return;
            }

            ResolveDialogueButtons();
            AddListener(dialogueCardButton01, OpenPanel);
            AddListener(dialogueCardButton02, OpenPanel);
            dialogueListenersRegistered = dialogueCardButton01 != null || dialogueCardButton02 != null;
        }

        private void UnregisterDialogueButtonListeners()
        {
            RemoveListener(dialogueCardButton01, OpenPanel);
            RemoveListener(dialogueCardButton02, OpenPanel);
            dialogueListenersRegistered = false;
        }

        private void RegisterMessagePanelListeners()
        {
            AddListener(messageCardButton01, ShowTipForCard01);
            AddListener(messageCardButton02, ShowTipForCard02);
            AddListener(confirmButton, ClosePanel);
        }

        private void UnregisterMessagePanelListeners()
        {
            RemoveListener(messageCardButton01, ShowTipForCard01);
            RemoveListener(messageCardButton02, ShowTipForCard02);
            RemoveListener(confirmButton, ClosePanel);
        }

        private void ResolveReferences()
        {
            AssignIfMissing(ref panelPopupRoot, FindGameObjectByName("Panel_PopupRoot"));
            AssignIfMissing(ref panelCardMessage, FindGameObjectByName("Panel_CardMessage"));

            ResolveDialogueButtons();

            Transform messageRoot = panelCardMessage != null ? panelCardMessage.transform : null;
            if (messageRoot == null)
            {
                return;
            }

            messageCardButton01 = FindButtonInChildren(messageRoot, "Button_PlayerCard01");
            messageCardButton02 = FindButtonInChildren(messageRoot, "Button_PlayerCard02");
            confirmButton = FindButtonInChildren(messageRoot, "Button_Comfirm");

            if (messageCardButton01 != null)
            {
                messageCardImage01 = messageCardButton01.GetComponent<Image>();
            }

            if (messageCardButton02 != null)
            {
                messageCardImage02 = messageCardButton02.GetComponent<Image>();
            }

            EnsureDefaultTipMappings();
        }

        private void ResolveDialogueButtons()
        {
            Transform dialogueArea = FindTransformByName("Panel_DialogueArea");
            if (dialogueArea == null)
            {
                return;
            }

            Transform dialogImageArea = FindChildTransform(dialogueArea, "DialogImage_Area");
            if (dialogImageArea != null)
            {
                Button button01 = FindButtonInChildren(dialogImageArea, "Button_PlayerCard01");
                Button button02 = FindButtonInChildren(dialogImageArea, "Button_PlayerCard02");
                if (button01 != null)
                {
                    dialogueCardButton01 = button01;
                }

                if (button02 != null)
                {
                    dialogueCardButton02 = button02;
                }
            }

            if (dialogueCardButton01 == null)
            {
                dialogueCardButton01 = FindButtonInChildren(dialogueArea, "Button_PlayerCard01");
            }

            if (dialogueCardButton02 == null)
            {
                dialogueCardButton02 = FindButtonInChildren(dialogueArea, "Button_PlayerCard02");
            }
        }

        private void EnsureDefaultTipMappings()
        {
            if (personalityTips == null || personalityTips.Length == 0)
            {
                personalityTips = CreateDefaultTipMappings();
                return;
            }

            bool hasAnyTip = false;
            for (int i = 0; i < personalityTips.Length; i++)
            {
                if (personalityTips[i].tipText != null)
                {
                    hasAnyTip = true;
                    break;
                }
            }

            if (!hasAnyTip)
            {
                personalityTips = CreateDefaultTipMappings();
            }
        }

        private PersonalityTipMapping[] CreateDefaultTipMappings()
        {
            Transform messageRoot = panelCardMessage != null ? panelCardMessage.transform : null;
            if (messageRoot == null)
            {
                return Array.Empty<PersonalityTipMapping>();
            }

            return new[]
            {
                new PersonalityTipMapping
                {
                    tag = PersonalityTag.Emotional,
                    tipText = FindGameObjectInChildren(messageRoot, "ArtTip_Text")
                },
                new PersonalityTipMapping
                {
                    tag = PersonalityTag.Rational,
                    tipText = FindGameObjectInChildren(messageRoot, "RationalTip_Text")
                },
                new PersonalityTipMapping
                {
                    tag = PersonalityTag.Practical,
                    tipText = FindGameObjectInChildren(messageRoot, "GuardTip_Text (1)")
                },
                new PersonalityTipMapping
                {
                    tag = PersonalityTag.Idealistic,
                    tipText = FindGameObjectInChildren(messageRoot, "IdealismTip_Text (2)")
                }
            };
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private static void ClearImage(Image image)
        {
            ApplySprite(image, null);
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private static void AssignIfMissing<T>(ref T target, T value) where T : class
        {
            if (target == null && value != null)
            {
                target = value;
            }
        }

        private static Button FindButtonInChildren(Transform root, string objectName)
        {
            Transform found = FindChildTransform(root, objectName);
            return found != null ? found.GetComponent<Button>() : null;
        }

        private static GameObject FindGameObjectInChildren(Transform root, string objectName)
        {
            Transform found = FindChildTransform(root, objectName);
            return found != null ? found.gameObject : null;
        }

        private static GameObject FindGameObjectByName(string objectName)
        {
            Transform transform = FindTransformByName(objectName);
            return transform != null ? transform.gameObject : null;
        }

        private static Transform FindTransformByName(string objectName)
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == objectName)
                {
                    return transforms[i];
                }
            }

            return null;
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
