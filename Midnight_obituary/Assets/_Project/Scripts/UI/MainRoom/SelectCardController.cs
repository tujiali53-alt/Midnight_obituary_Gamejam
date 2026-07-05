using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Player;

namespace ObituaryTomorrow.UI
{
    public sealed class SelectCardController : MonoBehaviour
    {
        [Serializable]
        private struct CardDialogueSpriteMapping
        {
            public PersonalityTag tag;
            public Sprite dialogueButtonSprite;
        }

        [Header("Panels")]
        [SerializeField] private GameObject selectCardPanel;
        [SerializeField] private GameObject cardPanel;
        [SerializeField] private GameObject showResultPanel;

        [Header("Buttons")]
        [SerializeField] private Button clickGachaButton;
        [SerializeField] private Button confirmButton;

        [Header("Result Cards")]
        [SerializeField] private Image resultCard01;
        [SerializeField] private Image resultCard02;

        [Header("Dialogue Area Card Buttons")]
        [SerializeField] private Image dialogueButtonImage01;
        [SerializeField] private Image dialogueButtonImage02;
        [SerializeField] private CardDialogueSpriteMapping[] dialogueSpriteMappings = new CardDialogueSpriteMapping[4];

        [Header("Card Message")]
        [SerializeField] private PlayerCardMessageController cardMessageController;

        [Header("Gameplay")]
        [SerializeField] private PlayerManager playerManager;

        private readonly List<CardOption> cardOptions = new List<CardOption>();
        private Image popupCard01;
        private Image popupCard02;

        private Sprite selectedSprite01;
        private Sprite selectedSprite02;
        private PersonalityTag selectedTag01;
        private PersonalityTag selectedTag02;
        private bool gachaCompleted;

        public bool IsBlockingInput { get; private set; }

        public event Action SelectionCompleted;

        private void Awake()
        {
            ResolveReferences();
            BuildCardOptions();
        }

        private void OnEnable()
        {
            AddListener(clickGachaButton, PerformGacha);
            AddListener(confirmButton, ConfirmSelection);
        }

        private void OnDisable()
        {
            RemoveListener(clickGachaButton, PerformGacha);
            RemoveListener(confirmButton, ConfirmSelection);
        }

        public void Initialize()
        {
            ResolveReferences();
            BuildCardOptions();

            GameSessionData session = GameManager.Instance?.Session;
            bool cardsConfirmed = session != null
                && (session.PlayerCardsConfirmed || session.Player.PersonalityTags.Count >= 2);

            if (cardsConfirmed)
            {
                if (session != null && !session.PlayerCardsConfirmed)
                {
                    session.PlayerCardsConfirmed = true;
                }

                ApplySpritesFromSession(session);
                HideSelectionPanel();
                IsBlockingInput = false;
                return;
            }

            ResetSelectionState();
            ShowSelectionPanel();
            IsBlockingInput = true;
        }

        private void ResetSelectionState()
        {
            gachaCompleted = false;
            selectedSprite01 = null;
            selectedSprite02 = null;

            SetPanelVisible(cardPanel, true);
            SetPanelVisible(showResultPanel, false);

            if (clickGachaButton != null)
            {
                clickGachaButton.interactable = true;
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }

            ClearImage(resultCard01);
            ClearImage(resultCard02);
            cardMessageController?.ClearSelectedCards();
        }

        private void PerformGacha()
        {
            if (cardOptions.Count < 2)
            {
                Debug.LogWarning("SelectCardController requires at least two cards in Card_Panel.");
                return;
            }

            int firstIndex = UnityEngine.Random.Range(0, cardOptions.Count);
            int secondIndex = UnityEngine.Random.Range(0, cardOptions.Count - 1);
            if (secondIndex >= firstIndex)
            {
                secondIndex++;
            }

            CardOption firstCard = cardOptions[firstIndex];
            CardOption secondCard = cardOptions[secondIndex];

            selectedSprite01 = firstCard.Sprite;
            selectedSprite02 = secondCard.Sprite;
            selectedTag01 = firstCard.Tag;
            selectedTag02 = secondCard.Tag;
            gachaCompleted = true;

            ApplySprite(resultCard01, selectedSprite01);
            ApplySprite(resultCard02, selectedSprite02);

            SetPanelVisible(cardPanel, false);
            SetPanelVisible(showResultPanel, true);

            if (clickGachaButton != null)
            {
                clickGachaButton.interactable = false;
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
        }

        private void ConfirmSelection()
        {
            if (!gachaCompleted)
            {
                return;
            }

            ApplyDialogueButtonSprites(selectedTag01, selectedTag02);
            ApplySprite(popupCard01, selectedSprite01);
            ApplySprite(popupCard02, selectedSprite02);
            cardMessageController?.SetSelectedCards(
                selectedSprite01,
                selectedSprite02,
                selectedTag01,
                selectedTag02);

            PersonalityTag[] selectedTags = { selectedTag01, selectedTag02 };
            GameSessionData session = GameManager.Instance?.Session;
            if (session != null)
            {
                session.Player.SetPersonalityTags(selectedTags);
                session.PlayerCardsConfirmed = true;
            }

            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }

            playerManager?.InitializeNewPlayer(new PlayerInitRequest(selectedTags));

            HideSelectionPanel();
            IsBlockingInput = false;
            SelectionCompleted?.Invoke();
        }

        private void ApplySpritesFromSession(GameSessionData session)
        {
            IReadOnlyList<PersonalityTag> tags = session.Player.PersonalityTags;
            if (tags.Count < 2)
            {
                return;
            }

            Sprite sprite01 = GetSpriteForTag(tags[0]);
            Sprite sprite02 = GetSpriteForTag(tags[1]);

            ApplyDialogueButtonSprites(tags[0], tags[1]);
            ApplySprite(popupCard01, sprite01);
            ApplySprite(popupCard02, sprite02);
            cardMessageController?.SetSelectedCards(sprite01, sprite02, tags[0], tags[1]);
        }

        private void ApplyDialogueButtonSprites(PersonalityTag tag01, PersonalityTag tag02)
        {
            ApplySprite(dialogueButtonImage01, GetDialogueSpriteForTag(tag01));
            ApplySprite(dialogueButtonImage02, GetDialogueSpriteForTag(tag02));
        }

        private Sprite GetDialogueSpriteForTag(PersonalityTag tag)
        {
            if (dialogueSpriteMappings != null)
            {
                for (int i = 0; i < dialogueSpriteMappings.Length; i++)
                {
                    if (dialogueSpriteMappings[i].tag == tag
                        && dialogueSpriteMappings[i].dialogueButtonSprite != null)
                    {
                        return dialogueSpriteMappings[i].dialogueButtonSprite;
                    }
                }
            }

            return GetSpriteForTag(tag);
        }

        private Sprite GetSpriteForTag(PersonalityTag tag)
        {
            for (int i = 0; i < cardOptions.Count; i++)
            {
                if (cardOptions[i].Tag == tag)
                {
                    return cardOptions[i].Sprite;
                }
            }

            return null;
        }

        private void BuildCardOptions()
        {
            cardOptions.Clear();

            if (cardPanel == null)
            {
                return;
            }

            Image[] images = cardPanel.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image.gameObject == cardPanel)
                {
                    continue;
                }

                if (!TryMapCardName(image.gameObject.name, out PersonalityTag tag))
                {
                    continue;
                }

                cardOptions.Add(new CardOption(image, tag));
            }
        }

        private static bool TryMapCardName(string objectName, out PersonalityTag tag)
        {
            switch (objectName)
            {
                case "emotional_Card":
                    tag = PersonalityTag.Emotional;
                    return true;
                case "Rational_Card":
                    tag = PersonalityTag.Rational;
                    return true;
                case "Ideal_Card":
                    tag = PersonalityTag.Idealistic;
                    return true;
                case "reality_Card":
                    tag = PersonalityTag.Practical;
                    return true;
                default:
                    tag = default;
                    return false;
            }
        }

        private void ResolveReferences()
        {
            AssignIfMissing(ref selectCardPanel, gameObject);
            AssignIfMissing(ref cardPanel, FindGameObjectByName("Card_Panel"));
            AssignIfMissing(ref showResultPanel, FindGameObjectByName("ShowResult_Panel"));
            AssignIfMissing(ref clickGachaButton, FindComponentByObjectName<Button>("clickGacha_Button"));
            AssignIfMissing(ref confirmButton, FindComponentByObjectName<Button>("Comfirm_Button"));

            if (showResultPanel != null)
            {
                AssignIfMissing(ref resultCard01, FindImageInChildren(showResultPanel.transform, "PlayerCard01"));
                AssignIfMissing(ref resultCard02, FindImageInChildren(showResultPanel.transform, "PlayerCard02"));
            }

            Transform dialogueArea = FindTransformByName("Panel_DialogueArea");
            if (dialogueArea != null)
            {
                Transform dialogueImageArea = FindChildTransform(dialogueArea, "DialogImage_Area");
                Transform dialogueButtonRoot = dialogueImageArea != null ? dialogueImageArea : dialogueArea;

                AssignIfMissing(
                    ref dialogueButtonImage01,
                    FindButtonImageInChildren(dialogueButtonRoot, "Button_PlayerCard01"));
                AssignIfMissing(
                    ref dialogueButtonImage02,
                    FindButtonImageInChildren(dialogueButtonRoot, "Button_PlayerCard02"));

                if (dialogueButtonImage01 == null)
                {
                    AssignIfMissing(
                        ref dialogueButtonImage01,
                        FindButtonImageInChildren(dialogueArea, "Button_PlayerCard01"));
                }

                if (dialogueButtonImage02 == null)
                {
                    AssignIfMissing(
                        ref dialogueButtonImage02,
                        FindButtonImageInChildren(dialogueArea, "Button_PlayerCard02"));
                }
            }

            if (cardMessageController == null)
            {
                cardMessageController = FindFirstObjectByType<PlayerCardMessageController>(FindObjectsInactive.Include);
            }

            EnsureDefaultDialogueSpriteMappings();

            Transform popupRoot = FindTransformByName("Panel_PopupRoot");
            Transform popupCardPanel = popupRoot != null ? FindChildTransform(popupRoot, "Panel_Card") : null;
            if (popupCardPanel != null)
            {
                popupCard01 = FindImageInChildren(popupCardPanel, "PlayerCard01");
                popupCard02 = FindImageInChildren(popupCardPanel, "PlayerCard02");
            }

            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }
        }

        private void ShowSelectionPanel()
        {
            SetPanelVisible(selectCardPanel, true);
        }

        private void HideSelectionPanel()
        {
            SetPanelVisible(selectCardPanel, false);
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

        private static void SetPanelVisible(GameObject panel, bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
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

        private static GameObject FindGameObjectByName(string objectName)
        {
            Transform transform = FindTransformByName(objectName);
            return transform != null ? transform.gameObject : null;
        }

        private static T FindComponentByObjectName<T>(string objectName) where T : Component
        {
            Transform transform = FindTransformByName(objectName);
            return transform != null ? transform.GetComponent<T>() : null;
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

        private void EnsureDefaultDialogueSpriteMappings()
        {
            if (dialogueSpriteMappings == null || dialogueSpriteMappings.Length == 0)
            {
                dialogueSpriteMappings = CreateDefaultDialogueSpriteMappings();
                return;
            }

            bool hasAnyTag = false;
            for (int i = 0; i < dialogueSpriteMappings.Length; i++)
            {
                if (dialogueSpriteMappings[i].tag != default)
                {
                    hasAnyTag = true;
                    break;
                }
            }

            if (!hasAnyTag)
            {
                dialogueSpriteMappings = CreateDefaultDialogueSpriteMappings();
            }
        }

        private static CardDialogueSpriteMapping[] CreateDefaultDialogueSpriteMappings()
        {
            return new[]
            {
                new CardDialogueSpriteMapping { tag = PersonalityTag.Emotional },
                new CardDialogueSpriteMapping { tag = PersonalityTag.Rational },
                new CardDialogueSpriteMapping { tag = PersonalityTag.Idealistic },
                new CardDialogueSpriteMapping { tag = PersonalityTag.Practical }
            };
        }

        private static Image FindButtonImageInChildren(Transform root, string buttonName)
        {
            Transform buttonTransform = FindChildTransform(root, buttonName);
            return buttonTransform != null ? buttonTransform.GetComponent<Image>() : null;
        }

        private static Image FindImageInChildren(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root.GetComponent<Image>();
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Image found = FindImageInChildren(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private readonly struct CardOption
        {
            public Sprite Sprite { get; }
            public PersonalityTag Tag { get; }

            public CardOption(Image image, PersonalityTag tag)
            {
                Sprite = image != null ? image.sprite : null;
                Tag = tag;
            }
        }
    }
}
