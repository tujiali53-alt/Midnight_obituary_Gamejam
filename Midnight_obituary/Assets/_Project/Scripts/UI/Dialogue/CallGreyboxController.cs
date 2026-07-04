using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Articy.ArticyProject;
using Articy.Unity;
using Articy.Unity.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Call;
using ObituaryTomorrow.Gameplay.Dice;
using ObituaryTomorrow.Gameplay.Player;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ObituaryTomorrow.UI
{
    public sealed class CallGreyboxController : MonoBehaviour
    {
        private const string DefaultOpeningFlowName = "judith";
        private const string DefaultOpeningFragmentTechnicalName = "DFr_DD2859CE";
        private const int DelayTargetCount = 30;
        private const int StressMilestone = 10;
        private const float ChoiceButtonWidth = 520f;
        private const float ChoiceButtonHeight = 64f;
        private const float ChoiceButtonSpacing = 16f;
        private const int DefaultDiceDifficulty = 7;
        private const int DiceAnimationFrames = 10;
        private const float DiceAnimationStepSeconds = 0.05f;

        [Header("Gameplay")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private CallCounterSystem callCounterSystem;
        [SerializeField] private DiceSystem diceSystem;
        [SerializeField] private bool autoStartOnStart = true;

        [Header("Articy")]
        [SerializeField] private string openingFlowName = DefaultOpeningFlowName;
        [SerializeField] private string openingFragmentTechnicalName = DefaultOpeningFragmentTechnicalName;
        [SerializeField] private int maxChoiceCount = 4;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI textNpc;
        [SerializeField] private Image imageNpcPortrait;
        [SerializeField] private TextMeshProUGUI textDialogue;
        [SerializeField] private TextMeshProUGUI textHud;
        [SerializeField] private TextMeshProUGUI textResult;
        [SerializeField] private TextMeshProUGUI textDiceResult;
        [SerializeField] private ScrollRect dialogueScrollRect;

        [Header("Choices")]
        [SerializeField] private Transform groupChoiceButtons;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Buttons")]
        [SerializeField] private Button buttonReturnMainRoom;

        [Header("Dice")]
        [SerializeField] private int diceDifficulty = DefaultDiceDifficulty;
        [SerializeField] private GameObject[] leftDiceFaceObjects;
        [SerializeField] private GameObject[] rightDiceFaceObjects;

        private readonly List<Button> spawnedChoiceButtons = new List<Button>();
        private readonly StringBuilder dialogueHistory = new StringBuilder();
        private readonly HashSet<ulong> completedDiceCheckIds = new HashSet<ulong>();
        private TMP_FontAsset runtimeChineseFont;
        private DialogueFragment currentFragment;
        private bool delayReminderShown;
        private bool callInitialized;
        private bool isResolvingDiceCheck;
        private Coroutine diceAnimationRoutine;
        private int callCount;

        private void Awake()
        {
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }

            if (callCounterSystem == null)
            {
                callCounterSystem = FindFirstObjectByType<CallCounterSystem>();
            }

            if (diceSystem == null)
            {
                diceSystem = FindFirstObjectByType<DiceSystem>();
            }
        }

        private void OnEnable()
        {
            GameEventBus.PlayerStressChanged += OnPlayerStressChanged;
            GameEventBus.CigaretteChanged += OnCigaretteChanged;

            if (buttonReturnMainRoom != null)
            {
                buttonReturnMainRoom.onClick.AddListener(ReturnMainRoom);
            }
        }

        private void OnDisable()
        {
            GameEventBus.PlayerStressChanged -= OnPlayerStressChanged;
            GameEventBus.CigaretteChanged -= OnCigaretteChanged;

            if (buttonReturnMainRoom != null)
            {
                buttonReturnMainRoom.onClick.RemoveListener(ReturnMainRoom);
            }

            ClearChoiceButtons();
            isResolvingDiceCheck = false;
        }

        private void Start()
        {
            if (autoStartOnStart)
            {
                BeginCall(true);
            }
        }

        public void ConfigureForMainRoom(
            PlayerManager sourcePlayerManager,
            CallCounterSystem sourceCallCounterSystem,
            TextMeshProUGUI npcText,
            Image npcPortraitImage,
            TextMeshProUGUI dialogueText,
            TextMeshProUGUI hudText,
            TextMeshProUGUI resultText,
            Transform choicesRoot,
            Button choicePrefab,
            Button returnButton)
        {
            autoStartOnStart = false;
            playerManager = sourcePlayerManager != null ? sourcePlayerManager : playerManager;
            callCounterSystem = sourceCallCounterSystem != null ? sourceCallCounterSystem : callCounterSystem;
            textNpc = npcText != null ? npcText : textNpc;
            imageNpcPortrait = npcPortraitImage != null ? npcPortraitImage : imageNpcPortrait;
            textDialogue = dialogueText != null ? dialogueText : textDialogue;
            textHud = hudText != null ? hudText : textHud;
            textResult = resultText != null ? resultText : textResult;
            textDiceResult = textDiceResult != null ? textDiceResult : FindComponentByObjectName<TextMeshProUGUI>("DiceResult");
            imageNpcPortrait = imageNpcPortrait != null ? imageNpcPortrait : FindComponentByObjectName<Image>("NPCNameImage");
            imageNpcPortrait = imageNpcPortrait != null ? imageNpcPortrait : FindComponentByObjectName<Image>("Image_NpcPortrait");
            groupChoiceButtons = choicesRoot != null ? choicesRoot : groupChoiceButtons;
            choiceButtonPrefab = choicePrefab != null ? choicePrefab : choiceButtonPrefab;
            buttonReturnMainRoom = returnButton != null ? returnButton : buttonReturnMainRoom;

            if (diceSystem == null)
            {
                diceSystem = FindFirstObjectByType<DiceSystem>();
            }
        }

        public void BeginCall(bool resetCounter)
        {
            if (resetCounter || !callInitialized)
            {
                callCount = 0;
                delayReminderShown = false;
                completedDiceCheckIds.Clear();
            }

            callInitialized = true;
            EnsureReadableChineseFont();
            EnsureDialogueScrollView();
            Resources.Load("ArticyDatabase");

            currentFragment = FindOpeningFragment();
            RefreshCurrentArticySpeakerPresentation();
            ResetDialogueHistory();
            AppendDialogueLine(currentFragment != null ? GetDialogueLineText(currentFragment) : "\u547c\u53eb\u4e2d.....");

            if (textResult != null)
            {
                textResult.gameObject.SetActive(false);
            }

            RefreshHud();
            BuildArticyChoices();
        }

        public ulong GetCurrentArticyFragmentId()
        {
            return currentFragment != null ? currentFragment.Id : 0UL;
        }

        public int GetCurrentCallCount()
        {
            return callCounterSystem != null ? callCounterSystem.CurrentCount : callCount;
        }

        public bool GetDelayReminderShown()
        {
            return delayReminderShown;
        }

        public bool RestoreArticyState(ulong fragmentId, int restoredCallCount, bool restoredDelayReminderShown, string restoredDialogueHistory = null)
        {
            callCount = Mathf.Max(0, restoredCallCount);
            delayReminderShown = restoredDelayReminderShown;
            callInitialized = true;
            EnsureReadableChineseFont();
            EnsureDialogueScrollView();
            Resources.Load("ArticyDatabase");

            currentFragment = FindFragmentById(fragmentId);

            if (currentFragment == null)
            {
                currentFragment = FindOpeningFragment();
            }

            RefreshCurrentArticySpeakerPresentation();
            RestoreDialogueHistory(restoredDialogueHistory);

            if (dialogueHistory.Length == 0)
            {
                AppendDialogueLine(currentFragment != null ? GetDialogueLineText(currentFragment) : "\u547c\u53eb\u4e2d.....");
            }

            if (textResult != null && !delayReminderShown)
            {
                textResult.gameObject.SetActive(false);
            }

            RefreshHud();
            BuildArticyChoices();
            CheckGreyboxResult();
            return currentFragment != null;
        }

        private static DialogueFragment FindFragmentById(ulong fragmentId)
        {
            if (fragmentId == 0UL)
            {
                return null;
            }

            Resources.Load("ArticyDatabase");

            foreach (DialogueFragment fragment in ArticyDatabase.GetAllOfType<DialogueFragment>())
            {
                if (fragment != null && fragment.Id == fragmentId)
                {
                    return fragment;
                }
            }

            return null;
        }

        public string GetDialogueHistory()
        {
            return dialogueHistory.ToString();
        }

        private void ResetDialogueHistory()
        {
            dialogueHistory.Clear();
            RefreshDialogueHistoryText();
        }

        private void RestoreDialogueHistory(string restoredDialogueHistory)
        {
            dialogueHistory.Clear();

            if (!string.IsNullOrWhiteSpace(restoredDialogueHistory))
            {
                dialogueHistory.Append(NormalizeDisplayText(restoredDialogueHistory));
            }

            RefreshDialogueHistoryText();
        }

        private void AppendDialogueLine(string line)
        {
            string normalized = NormalizeDisplayText(line);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return;
            }

            if (dialogueHistory.Length > 0)
            {
                dialogueHistory.AppendLine();
                dialogueHistory.AppendLine();
            }

            dialogueHistory.Append(normalized);
            RefreshDialogueHistoryText();
        }

        private void AppendPlayerChoice(string label)
        {
            string normalized = NormalizeDisplayText(label).Replace("\n", " ");

            if (string.IsNullOrWhiteSpace(normalized) || IsContinueChoice(normalized))
            {
                return;
            }

            AppendDialogueLine($"\u4f60\uff1a{normalized}");
        }

        private static bool IsContinueChoice(string label)
        {
            return string.Equals(NormalizeDisplayText(label), "\u7ee7\u7eed", StringComparison.Ordinal);
        }

        private void RefreshDialogueHistoryText()
        {
            SetText(textDialogue, dialogueHistory.ToString());
            ScrollDialogueToBottom();
        }

        private void ScrollDialogueToBottom()
        {
            if (dialogueScrollRect == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            dialogueScrollRect.verticalNormalizedPosition = 0f;
        }

        private void EnsureDialogueScrollView()
        {
            if (textDialogue == null)
            {
                return;
            }

            if (dialogueScrollRect != null)
            {
                ConfigureDialogueTextForScroll();
                return;
            }

            dialogueScrollRect = textDialogue.GetComponentInParent<ScrollRect>();

            if (dialogueScrollRect != null)
            {
                ConfigureDialogueTextForScroll();
                return;
            }

            RectTransform textRect = textDialogue.rectTransform;
            RectTransform originalParent = textRect.parent as RectTransform;

            if (originalParent == null)
            {
                ConfigureDialogueTextForScroll();
                return;
            }

            int siblingIndex = textRect.GetSiblingIndex();
            Vector2 anchorMin = textRect.anchorMin;
            Vector2 anchorMax = textRect.anchorMax;
            Vector2 anchoredPosition = textRect.anchoredPosition;
            Vector2 sizeDelta = textRect.sizeDelta;
            Vector2 pivot = textRect.pivot;

            GameObject scrollObject = new GameObject("Scroll_DialogueHistory", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(originalParent, false);
            scrollObject.transform.SetSiblingIndex(siblingIndex);

            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = anchorMin;
            scrollRectTransform.anchorMax = anchorMax;
            scrollRectTransform.anchoredPosition = anchoredPosition;
            scrollRectTransform.sizeDelta = sizeDelta;
            scrollRectTransform.pivot = pivot;

            Image scrollImage = scrollObject.GetComponent<Image>();
            scrollImage.color = new Color(0f, 0f, 0f, 0f);

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            textDialogue.transform.SetParent(contentObject.transform, false);
            dialogueScrollRect = scrollObject.GetComponent<ScrollRect>();
            dialogueScrollRect.viewport = viewportRect;
            dialogueScrollRect.content = contentRect;
            dialogueScrollRect.horizontal = false;
            dialogueScrollRect.vertical = true;
            dialogueScrollRect.movementType = ScrollRect.MovementType.Clamped;
            dialogueScrollRect.scrollSensitivity = 28f;

            ConfigureDialogueTextForScroll();
        }

        private void ConfigureDialogueTextForScroll()
        {
            if (textDialogue == null)
            {
                return;
            }

            RectTransform textRect = textDialogue.rectTransform;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(0f, textRect.sizeDelta.y);
            textDialogue.enableWordWrapping = true;
            textDialogue.alignment = TextAlignmentOptions.TopLeft;

            LayoutElement layoutElement = textDialogue.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = textDialogue.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredWidth = -1f;
            layoutElement.minHeight = 0f;
        }

        private string GetNpcLabel()
        {
            string npcId = GameManager.Instance != null && GameManager.Instance.Session != null
                ? GameManager.Instance.Session.CurrentNpcId
                : "NPC_Lena_001";

            return NormalizeDisplayText($"{npcId} [Articy]");
        }

        public bool RefreshCurrentArticySpeakerPresentation()
        {
            if (currentFragment == null)
            {
                Resources.Load("ArticyDatabase");
                currentFragment = FindOpeningFragment();
            }

            return RefreshNpcPresentation(currentFragment);
        }

        public bool RefreshArticySpeakerPresentation(ulong fragmentId)
        {
            return RefreshNpcPresentation(FindFragmentById(fragmentId));
        }

        private bool RefreshNpcPresentation(DialogueFragment fragment)
        {
            ArticyObject speaker = GetFragmentSpeaker(fragment);
            string speakerName = GetSpeakerDisplayName(speaker);

            if (string.IsNullOrWhiteSpace(speakerName))
            {
                speakerName = GetNpcLabel();
            }

            SetText(textNpc, speakerName);

            Sprite portraitSprite = GetPreviewSprite(speaker);
            if (portraitSprite == null)
            {
                portraitSprite = GetPreviewSprite(fragment);
            }

            SetNpcPortrait(portraitSprite);
            return !string.IsNullOrWhiteSpace(speakerName) || portraitSprite != null;
        }

        private static ArticyObject GetFragmentSpeaker(DialogueFragment fragment)
        {
            if (fragment == null)
            {
                return null;
            }

            return fragment.Speaker;
        }

        private static string GetSpeakerDisplayName(ArticyObject speaker)
        {
            if (speaker == null)
            {
                return string.Empty;
            }

            if (speaker is IObjectWithFeaturecharacter_info objectWithCharacterInfo)
            {
                string cardName = NormalizeDisplayText(objectWithCharacterInfo.GetFeaturecharacter_info()?.name);
                if (!string.IsNullOrWhiteSpace(cardName))
                {
                    return cardName;
                }
            }

            string displayName = GetDisplayName(speaker);
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            return NormalizeDisplayText(speaker.TechnicalName);
        }

        private static Sprite GetPreviewSprite(ArticyObject articyObject)
        {
            if (!(articyObject is IObjectWithPreviewImage objectWithPreviewImage) || objectWithPreviewImage.PreviewImage == null)
            {
                return null;
            }

            IAsset articyAsset = objectWithPreviewImage.PreviewImage.Asset;
            return articyAsset != null ? articyAsset.LoadAssetAsSprite() : null;
        }

        private void SetNpcPortrait(Sprite portraitSprite)
        {
            if (imageNpcPortrait == null)
            {
                return;
            }

            imageNpcPortrait.sprite = portraitSprite;
            imageNpcPortrait.enabled = portraitSprite != null;
            imageNpcPortrait.preserveAspect = true;
        }

        private DialogueFragment FindOpeningFragment()
        {
            DialogueFragment flowFragment = FindFirstFragmentInFlow(openingFlowName);

            if (flowFragment != null)
            {
                return flowFragment;
            }

            string configuredName = string.IsNullOrWhiteSpace(openingFragmentTechnicalName)
                ? DefaultOpeningFragmentTechnicalName
                : openingFragmentTechnicalName;

            foreach (DialogueFragment fragment in ArticyDatabase.GetAllOfType<DialogueFragment>())
            {
                if (fragment != null && fragment.TechnicalName == configuredName)
                {
                    return fragment;
                }
            }

            foreach (DialogueFragment fragment in ArticyDatabase.GetAllOfType<DialogueFragment>())
            {
                if (fragment != null && !string.IsNullOrWhiteSpace(GetArticyText(fragment)))
                {
                    return fragment;
                }
            }

            return null;
        }

        private static DialogueFragment FindFirstFragmentInFlow(string flowName)
        {
            if (string.IsNullOrWhiteSpace(flowName))
            {
                return null;
            }

            string normalizedFlowName = NormalizeDisplayText(flowName);

            foreach (Dialogue dialogue in ArticyDatabase.GetAllOfType<Dialogue>())
            {
                if (dialogue != null && IsNamedArticyObject(dialogue, normalizedFlowName))
                {
                    return FindFirstDescendantDialogueFragment(dialogue.Id, new HashSet<ulong>());
                }
            }

            foreach (FlowFragment flowFragment in ArticyDatabase.GetAllOfType<FlowFragment>())
            {
                if (flowFragment == null || !IsNamedArticyObject(flowFragment, normalizedFlowName))
                {
                    continue;
                }

                DialogueFragment childFragment = FindFirstDescendantDialogueFragment(flowFragment.Id, new HashSet<ulong>());

                if (childFragment != null)
                {
                    return childFragment;
                }

                List<ArticyObject> playableTargets = new List<ArticyObject>();
                AddPlayableTargets(flowFragment, playableTargets, new HashSet<ulong>());

                if (playableTargets.Count > 0 && playableTargets[0] is DialogueFragment playableFragment)
                {
                    return playableFragment;
                }
            }

            return null;
        }

        private static bool IsNamedArticyObject(ArticyObject articyObject, string expectedName)
        {
            if (articyObject == null || string.IsNullOrWhiteSpace(expectedName))
            {
                return false;
            }

            if (string.Equals(articyObject.TechnicalName, expectedName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(GetDisplayName(articyObject), expectedName, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDisplayName(object articyObject)
        {
            if (articyObject is IObjectWithDisplayName objectWithDisplayName)
            {
                return NormalizeDisplayText(objectWithDisplayName.DisplayName);
            }

            if (articyObject is IObjectWithLocalizableDisplayName objectWithLocalizableDisplayName)
            {
                return NormalizeDisplayText(objectWithLocalizableDisplayName.DisplayName);
            }

            return string.Empty;
        }

        private void BuildArticyChoices()
        {
            if (groupChoiceButtons == null)
            {
                Debug.LogWarning("Choice button group is missing.");
                return;
            }

            EnsureChoiceButtonPrefab();

            if (choiceButtonPrefab == null)
            {
                Debug.LogWarning("Choice button prefab is missing.");
                return;
            }

            EnsureChoiceButtonLayout();
            ClearChoiceButtons();
            choiceButtonPrefab.gameObject.SetActive(false);

            if (currentFragment == null)
            {
                CreateFallbackChoice("\u7ee7\u7eed\u901a\u8bdd");
                return;
            }

            List<ArticyObject> nextTargets = GetNextPlayableTargets(currentFragment);

            if (!isResolvingDiceCheck && TryGetPendingDiceCheck(nextTargets, out FlowFragment pendingCheck))
            {
                StartDiceCheck(pendingCheck, false);
                return;
            }

            nextTargets.RemoveAll(IsDiceCheckNode);

            int visibleCount = Mathf.Min(nextTargets.Count, Mathf.Max(1, maxChoiceCount));

            for (int i = 0; i < visibleCount; i++)
            {
                ArticyObject target = nextTargets[i];
                string label = GetChoiceButtonLabel(target);

                CreateChoice(target, label);
            }

            if (visibleCount == 0)
            {
                if (TryGetFirstRawContinuationTarget(currentFragment, out ArticyObject continuationTarget))
                {
                    CreateChoice(continuationTarget, "\u7ee7\u7eed");
                }
                else
                {
                    CreateFallbackChoice("\u7ed3\u675f\u901a\u8bdd");
                }
            }

            RebuildChoiceButtonLayout();
        }

        private static bool TryGetFirstRawContinuationTarget(DialogueFragment fragment, out ArticyObject target)
        {
            target = null;

            if (fragment == null || fragment.OutputPins == null)
            {
                return false;
            }

            foreach (OutputPin outputPin in fragment.OutputPins)
            {
                if (outputPin == null || outputPin.Connections == null)
                {
                    continue;
                }

                foreach (OutgoingConnection connection in outputPin.Connections)
                {
                    if (connection != null && connection.Target != null)
                    {
                        target = connection.Target;
                        return true;
                    }
                }
            }

            return false;
        }

        private void EnsureChoiceButtonPrefab()
        {
            if (choiceButtonPrefab != null || groupChoiceButtons == null)
            {
                return;
            }

            choiceButtonPrefab = groupChoiceButtons.GetComponentInChildren<Button>(true);

            if (choiceButtonPrefab != null)
            {
                return;
            }

            GameObject buttonObject = new GameObject("Button_ArticyChoiceTemplate", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(groupChoiceButtons, false);
            buttonObject.SetActive(false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(ChoiceButtonWidth, ChoiceButtonHeight);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.08f, 0.075f, 0.055f, 0.92f);

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 8f);
            textRect.offsetMax = new Vector2(-16f, -8f);

            TextMeshProUGUI buttonText = textObject.GetComponent<TextMeshProUGUI>();
            buttonText.fontSize = 24f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.enableWordWrapping = true;
            ApplyRuntimeFont(buttonText);

            choiceButtonPrefab = buttonObject.GetComponent<Button>();
        }

        private void EnsureChoiceButtonLayout()
        {
            if (groupChoiceButtons == null)
            {
                return;
            }

            VerticalLayoutGroup layout = groupChoiceButtons.GetComponent<VerticalLayoutGroup>();

            if (layout == null)
            {
                layout = groupChoiceButtons.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = ChoiceButtonSpacing;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private static void ConfigureChoiceButtonLayout(Button button)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rectTransform = button.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(ChoiceButtonWidth, ChoiceButtonHeight);
            }

            LayoutElement layoutElement = button.GetComponent<LayoutElement>();

            if (layoutElement == null)
            {
                layoutElement = button.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredWidth = ChoiceButtonWidth;
            layoutElement.preferredHeight = ChoiceButtonHeight;
            layoutElement.minHeight = ChoiceButtonHeight;
        }

        private void RebuildChoiceButtonLayout()
        {
            if (groupChoiceButtons is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        private void CreateChoice(ArticyObject target, string label)
        {
            Button button = Instantiate(choiceButtonPrefab, groupChoiceButtons);
            button.gameObject.SetActive(true);
            ConfigureChoiceButtonLayout(button);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                ApplyRuntimeFont(buttonText);
                buttonText.text = FitChoiceLabel(label);
            }

            button.onClick.AddListener(() => SelectChoice(target, label));
            spawnedChoiceButtons.Add(button);
        }

        private void CreateFallbackChoice(string label)
        {
            Button button = Instantiate(choiceButtonPrefab, groupChoiceButtons);
            button.gameObject.SetActive(true);
            ConfigureChoiceButtonLayout(button);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                ApplyRuntimeFont(buttonText);
                buttonText.text = label;
            }

            button.onClick.AddListener(() => SelectFallbackChoice(label));
            spawnedChoiceButtons.Add(button);
        }

        private void SelectChoice(ArticyObject target, string label)
        {
            AppendPlayerChoice(label);
            RegisterPlayerSpeech(label);
            AdvanceToArticyTarget(target);
        }

        private void AdvanceToArticyTarget(ArticyObject target)
        {
            if (target == null)
            {
                BuildArticyChoices();
                return;
            }

            if (target is FlowFragment flowFragment && IsDiceCheckNode(flowFragment))
            {
                StartDiceCheck(flowFragment, true);
                return;
            }

            if (target is DialogueFragment fragment)
            {
                currentFragment = fragment;
                RefreshCurrentArticySpeakerPresentation();
                AppendDialogueLine(GetDialogueLineText(fragment));
                RefreshHud();
                BuildArticyChoices();
                CheckGreyboxResult();
                return;
            }

            List<ArticyObject> resolvedTargets = new List<ArticyObject>();
            HashSet<ulong> visitedIds = new HashSet<ulong>();
            if (currentFragment != null)
            {
                visitedIds.Add(currentFragment.Id);
            }

            AddPlayableTargets(target, resolvedTargets, visitedIds);

            if (resolvedTargets.Count > 0)
            {
                AdvanceToArticyTarget(resolvedTargets[0]);
                return;
            }

            BuildArticyChoices();
        }

        private void StartDiceCheck(FlowFragment checkNode, bool registerAsChoice)
        {
            if (checkNode == null || isResolvingDiceCheck)
            {
                return;
            }

            if (diceAnimationRoutine != null)
            {
                StopCoroutine(diceAnimationRoutine);
            }

            diceAnimationRoutine = StartCoroutine(ResolveDiceCheck(checkNode, registerAsChoice));
        }

        private IEnumerator ResolveDiceCheck(FlowFragment checkNode, bool registerAsChoice)
        {
            isResolvingDiceCheck = true;
            ClearChoiceButtons();

            completedDiceCheckIds.Add(checkNode.Id);
            DiceCheckConfig checkConfig = CreateDiceCheckConfig(checkNode);
            string pendingText = string.Format("\u5224\u5b9a\uff1a{0} / \u96be\u5ea6 {1}", checkConfig.Label, checkConfig.Difficulty);
            SetText(textResult, pendingText);
            SetText(textDiceResult, pendingText);

            if (textResult != null)
            {
                textResult.gameObject.SetActive(true);
            }

            if (textDiceResult != null)
            {
                textDiceResult.gameObject.SetActive(true);
            }

            DiceResult diceResult = diceSystem != null
                ? diceSystem.RollCheck(new DiceCheckRequest(checkConfig.Label, checkConfig.AttributeType, checkConfig.Difficulty))
                : new DiceResult(checkConfig.Label, checkConfig.AttributeType, checkConfig.Difficulty, 0, 0, 0, 0, checkConfig.Difficulty, true);

            yield return PlayDiceAnimation(diceResult);

            DiceBranchResult branchResult = GetDiceResultBranch(checkNode, diceResult.Success);
            string resultText = diceSystem != null
                ? FormatDiceResultText(diceResult, branchResult.ConditionLabel)
                : "\u9ab0\u5b50\u7cfb\u7edf\u7f3a\u5931\uff0c\u5df2\u6309\u901a\u8fc7\u5904\u7406\u3002";
            string diceResultText = diceSystem != null
                ? FormatDiceResultConditionText(diceResult, branchResult.ConditionLabel)
                : FormatDiceResultConditionText(diceResult, branchResult.ConditionLabel);
            SetText(textResult, resultText);
            SetText(textDiceResult, diceResultText);

            ArticyObject resultTarget = branchResult.Target;
            isResolvingDiceCheck = false;
            diceAnimationRoutine = null;

            if (resultTarget != null)
            {
                AdvanceToArticyTarget(resultTarget);
            }
            else
            {
                BuildArticyChoices();
            }
        }

        private IEnumerator PlayDiceAnimation(DiceResult result)
        {
            EnsureDiceFaceObjects();
            int frames = Mathf.Max(1, DiceAnimationFrames);

            for (int i = 0; i < frames; i++)
            {
                ShowDiceFace(leftDiceFaceObjects, UnityEngine.Random.Range(1, 7));
                ShowDiceFace(rightDiceFaceObjects, UnityEngine.Random.Range(1, 7));
                yield return new WaitForSeconds(Mathf.Max(0.01f, DiceAnimationStepSeconds));
            }

            if (result.PositiveD6 > 0)
            {
                ShowDiceFace(leftDiceFaceObjects, result.PositiveD6);
            }

            if (result.NegativeD6 > 0)
            {
                ShowDiceFace(rightDiceFaceObjects, result.NegativeD6);
            }
        }

        private void EnsureDiceFaceObjects()
        {
            if (leftDiceFaceObjects == null || leftDiceFaceObjects.Length < 6)
            {
                leftDiceFaceObjects = FindDiceFaceObjects("Left");
            }

            if (rightDiceFaceObjects == null || rightDiceFaceObjects.Length < 6)
            {
                rightDiceFaceObjects = FindDiceFaceObjects("Right");
            }
        }

        private static T FindComponentByObjectName<T>(string objectName) where T : Component
        {
            Transform transform = FindTransformByName(objectName);
            return transform != null ? transform.GetComponent<T>() : null;
        }

        private static GameObject[] FindDiceFaceObjects(string parentName)
        {
            Transform parent = FindTransformByName(parentName);

            if (parent == null)
            {
                return Array.Empty<GameObject>();
            }

            Image[] images = parent.GetComponentsInChildren<Image>(true);
            Array.Sort(images, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

            GameObject[] faces = new GameObject[Mathf.Min(6, images.Length)];

            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = images[i].gameObject;
            }

            return faces;
        }

        private static Transform FindTransformByName(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

            foreach (Transform transform in transforms)
            {
                if (transform != null && transform.name == objectName && transform.gameObject.scene.IsValid())
                {
                    return transform;
                }
            }

            return null;
        }

        private static void ShowDiceFace(GameObject[] diceFaces, int point)
        {
            if (diceFaces == null || diceFaces.Length == 0)
            {
                return;
            }

            int index = Mathf.Clamp(point, 1, 6) - 1;

            for (int i = 0; i < diceFaces.Length; i++)
            {
                if (diceFaces[i] != null)
                {
                    diceFaces[i].SetActive(i == index);
                }
            }
        }

        private static string FormatDiceResultText(DiceResult result, string conditionLabel)
        {
            string state = result.Success ? "\u6210\u529f" : "\u5931\u8d25";
            string attributeName = GetAttributeDisplayName(result.AttributeType);
            string resultText = string.Format(
                "{0}\u68c0\u5b9a\uff1a+{1} -{2} + {3}({4}) = {5} / \u96be\u5ea6 {6}\uff0c{7}",
                attributeName,
                result.PositiveD6,
                result.NegativeD6,
                attributeName,
                result.AttributeBonus,
                result.Total,
                result.Difficulty,
                state);

            if (string.IsNullOrWhiteSpace(conditionLabel))
            {
                conditionLabel = string.Format("{0}\u68c0\u5b9a{1}", attributeName, state);
            }

            return string.Format("{0}\n\u8fdb\u5165\u6761\u4ef6\uff1a{1}", resultText, conditionLabel);
        }

        private static string FormatDiceResultConditionText(DiceResult result, string conditionLabel)
        {
            string state = result.Success ? "\u6210\u529f" : "\u5931\u8d25";
            return string.Format("{0}\u68c0\u5b9a{1}", GetAttributeDisplayName(result.AttributeType), state);
        }

        private static string GetAttributeDisplayName(PlayerAttributeType attributeType)
        {
            switch (attributeType)
            {
                case PlayerAttributeType.Logic:
                    return "\u903b\u8f91";
                case PlayerAttributeType.Insight:
                    return "\u654f\u9510";
                case PlayerAttributeType.Resilience:
                    return "\u52a1\u5b9e";
                case PlayerAttributeType.Perception:
                default:
                    return "\u611f\u77e5";
            }
        }

        private void SelectFallbackChoice(string label)
        {
            AppendPlayerChoice(label);
            RegisterPlayerSpeech(label);
            RefreshHud();
            CheckGreyboxResult();
        }

        private void RegisterPlayerSpeech(string sourceId)
        {
            if (callCounterSystem != null)
            {
                callCount = callCounterSystem.RegisterPlayerLine(sourceId);
                return;
            }

            callCount++;

            if (StressMilestone > 0 && callCount % StressMilestone == 0)
            {
                IncreasePlayerStress(sourceId);
            }
        }
        private void IncreasePlayerStress(string sourceId)
        {
            if (playerManager != null)
            {
                playerManager.RequestStressChange(new StressChangeRequest(
                    1,
                    StatChangeReason.CallCounterMilestone,
                    sourceId,
                    true));
                return;
            }

            PlayerRuntimeData playerData = GetPlayerRuntimeData();

            if (playerData == null)
            {
                Debug.LogWarning("Cannot increase player stress because no PlayerManager or session player data exists.");
                return;
            }

            int oldValue = playerData.CurrentStress;
            int newValue = Mathf.Clamp(oldValue + 1, 0, playerData.MaxStress);
            playerData.CurrentStress = newValue;

            if (oldValue != newValue)
            {
                GameEventBus.RaisePlayerStressChanged(new StressChangedEventArgs(
                    oldValue,
                    newValue,
                    playerData.MaxStress,
                    StatChangeReason.CallCounterMilestone));
            }
        }

        private PlayerRuntimeData GetPlayerRuntimeData()
        {
            if (playerManager != null && playerManager.RuntimeData != null)
            {
                return playerManager.RuntimeData;
            }

            return GameManager.Instance != null && GameManager.Instance.Session != null
                ? GameManager.Instance.Session.Player
                : null;
        }
        private void CheckGreyboxResult()
        {
            if (callCount >= GetDelayTargetCount())
            {
                ShowDelayReminder();
            }
        }

        private void ShowDelayReminder()
        {
            if (delayReminderShown)
            {
                return;
            }

            delayReminderShown = true;
            string message = $"\u5df2\u8fbe\u5230 {GetDelayTargetCount()} \u6b21\u901a\u8bdd\u8ba1\u6570\uff0c\u62d6\u5ef6\u9608\u503c\u5df2\u8fbe\u6210\u3002";

            if (textResult != null)
            {
                textResult.gameObject.SetActive(true);
                textResult.text = message;
                return;
            }

            if (textDialogue != null)
            {
                AppendDialogueLine(message);
            }
        }
        private void ClearChoiceButtons()
        {
            for (int i = 0; i < spawnedChoiceButtons.Count; i++)
            {
                if (spawnedChoiceButtons[i] != null)
                {
                    Destroy(spawnedChoiceButtons[i].gameObject);
                }
            }

            spawnedChoiceButtons.Clear();
        }

        private void RefreshHud()
        {
            if (textHud == null)
            {
                return;
            }

            PlayerRuntimeData playerData = GetPlayerRuntimeData();
            int currentStress = playerData != null ? playerData.CurrentStress : 0;
            int maxStress = playerData != null ? playerData.MaxStress : 5;
            int cigaretteCount = playerData != null ? playerData.CigaretteCount : 5;

            textHud.text = $"\u538b\u529b\uff1a{currentStress}/{maxStress} | \u9999\u70df\uff1a{cigaretteCount} | \u901a\u8bdd\u8ba1\u6570\uff1a{callCount}/{GetDelayTargetCount()}";
        }

        private int GetDelayTargetCount()
        {
            return callCounterSystem != null ? callCounterSystem.DelayTargetCount : DelayTargetCount;
        }

        private void EnsureReadableChineseFont()
        {
            if (runtimeChineseFont == null)
            {
                runtimeChineseFont = CreateVerifiedChineseFont();
            }

            ApplyRuntimeFont(textNpc);
            ApplyRuntimeFont(textDialogue);
            ApplyRuntimeFont(textHud);
            ApplyRuntimeFont(textResult);
            ApplyRuntimeFont(textDiceResult);

            foreach (TextMeshProUGUI text in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                ApplyRuntimeFont(text);
            }
        }

        private static TMP_FontAsset CreateVerifiedChineseFont()
        {
            TMP_FontAsset projectFont = TryCreateProjectFontAsset();

            if (projectFont != null)
            {
                return projectFont;
            }

            string[] fontNames =
            {
                "Noto Sans SC",
                "Source Han Sans CN",
                "Source Han Sans SC",
                "Microsoft YaHei UI",
                "Microsoft YaHei",
                "SimHei",
                "SimSun",
                "DengXian",
                "WenQuanYi Micro Hei",
                "Arial Unicode MS"
            };

            const string probeText = "\u547c\u53eb\u4e2d\u7ee7\u7eed\u901a\u8bdd\u7ed3\u675f\u538b\u529b\u9999\u70df\u8ba1\u6570\u4f60\u4ecd\u65e7\u5750\u5728\u90a3\u628a\u9648\u65e7\u7684\u6276\u624b\u6905\u4e4b\u4e2d\u57ce\u5e02\u591c\u8272\u94dc\u7ede\u7ebf\u7f51\u7edc";

            foreach (string fontName in fontNames)
            {
                TMP_FontAsset fontAsset = TryCreateFontAsset(fontName, probeText);

                if (fontAsset != null)
                {
                    return fontAsset;
                }
            }

            Debug.LogWarning("Failed to create a verified Chinese TMP font. Chinese text may render as squares.");
            return null;
        }

        private static TMP_FontAsset TryCreateProjectFontAsset()
        {
#if UNITY_EDITOR
            const string probeText = "\u547c\u53eb\u4e2d\u7ee7\u7eed\u901a\u8bdd\u7ed3\u675f\u538b\u529b\u9999\u70df\u8ba1\u6570\u4f60\u4ecd\u65e7\u5750\u5728\u90a3\u628a\u9648\u65e7\u7684\u6276\u624b\u6905\u4e4b\u4e2d\u57ce\u5e02\u591c\u8272\u94dc\u7ede\u7ebf\u7f51\u7edc";
            string[] assetPaths =
            {
                "Assets/_Project/Art/Fonts/WenQuanyi Micro Hei.ttf",
                "Assets/_Project/Art/Fonts/\u8feb\u771f\u6253\u5b57\u6cb9\u5370\u9ad4.ttf"
            };

            foreach (string assetPath in assetPaths)
            {
                Font font = AssetDatabase.LoadAssetAtPath<Font>(assetPath);

                if (font == null)
                {
                    continue;
                }

                TMP_FontAsset fontAsset = TryCreateFontAsset(font, assetPath, probeText);

                if (fontAsset != null)
                {
                    Debug.Log($"Using project Chinese TMP font: {assetPath}");
                    return fontAsset;
                }
            }
#endif
            return null;
        }

        private static TMP_FontAsset TryCreateFontAsset(Font font, string sourceName, string probeText)
        {
            try
            {
                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 4096, 4096);
                fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                fontAsset.name = $"{sourceName} Runtime Chinese SDF";

                if (!fontAsset.TryAddCharacters(probeText, out string missingCharacters) || !string.IsNullOrEmpty(missingCharacters))
                {
                    Debug.LogWarning($"Chinese font candidate '{sourceName}' is missing: {missingCharacters}");
                    return null;
                }

                if (TMP_Settings.fallbackFontAssets != null && !TMP_Settings.fallbackFontAssets.Contains(fontAsset))
                {
                    TMP_Settings.fallbackFontAssets.Insert(0, fontAsset);
                }

                return fontAsset;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to create Chinese TMP font from '{sourceName}': {exception.Message}");
                return null;
            }
        }
        private static TMP_FontAsset TryCreateFontAsset(string fontName, string probeText)
        {
            try
            {
                Font osFont = Font.CreateDynamicFontFromOSFont(fontName, 24);

                if (osFont == null)
                {
                    return null;
                }

                TMP_FontAsset fontAsset = TryCreateFontAsset(osFont, fontName, probeText);

                if (fontAsset == null)
                {
                    return null;
                }

                Debug.Log($"Using runtime Chinese TMP font: {fontName}");
                return fontAsset;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to create Chinese TMP font '{fontName}': {exception.Message}");
                return null;
            }
        }

        private void ApplyRuntimeFont(TextMeshProUGUI text)
        {
            if (text != null && runtimeChineseFont != null)
            {
                text.font = runtimeChineseFont;
            }
        }

        private static List<ArticyObject> GetNextPlayableTargets(DialogueFragment fragment)
        {
            List<ArticyObject> targets = new List<ArticyObject>();

            if (fragment == null || fragment.OutputPins == null)
            {
                return targets;
            }

            AddOutputPinTargets(fragment.OutputPins, targets, new HashSet<ulong> { fragment.Id });
            return targets;
        }

        private bool TryGetPendingDiceCheck(List<ArticyObject> targets, out FlowFragment checkNode)
        {
            checkNode = null;

            if (targets == null)
            {
                return false;
            }

            foreach (ArticyObject target in targets)
            {
                if (target is FlowFragment flowFragment && IsDiceCheckNode(flowFragment) && !completedDiceCheckIds.Contains(flowFragment.Id))
                {
                    checkNode = flowFragment;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetConditionDiceCheckTarget(DialogueFragment fragment, out FlowFragment checkNode)
        {
            checkNode = null;

            if (fragment == null || fragment.OutputPins == null)
            {
                return false;
            }

            foreach (OutputPin outputPin in fragment.OutputPins)
            {
                if (outputPin == null || outputPin.Connections == null)
                {
                    continue;
                }

                foreach (OutgoingConnection connection in outputPin.Connections)
                {
                    if (connection == null || connection.Target == null)
                    {
                        continue;
                    }

                    if (TryFindConditionDiceCheckTarget(connection.Target, new HashSet<ulong>(), out checkNode))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryFindConditionDiceCheckTarget(ArticyObject target, HashSet<ulong> visitedIds, out FlowFragment checkNode)
        {
            checkNode = null;

            if (target == null || !visitedIds.Add(target.Id))
            {
                return false;
            }

            if (target is FlowFragment flowFragment && IsDiceCheckNode(flowFragment))
            {
                return false;
            }

            if (target is Condition condition)
            {
                return TryFindDiceCheckInOutputs(condition.OutputPins, visitedIds, out checkNode);
            }

            if (target is Hub hub)
            {
                return TryFindConditionDiceCheckInOutputs(hub.OutputPins, visitedIds, out checkNode);
            }

            if (target is Instruction instruction)
            {
                return TryFindConditionDiceCheckInOutputs(instruction.OutputPins, visitedIds, out checkNode);
            }

            if (target is FlowFragment flow)
            {
                return TryFindConditionDiceCheckInOutputs(flow.OutputPins, visitedIds, out checkNode);
            }

            if (target is Jump jump)
            {
                return TryFindConditionDiceCheckTarget(jump.Target, visitedIds, out checkNode);
            }

            return false;
        }

        private static bool TryFindConditionDiceCheckInOutputs(List<OutputPin> outputPins, HashSet<ulong> visitedIds, out FlowFragment checkNode)
        {
            checkNode = null;

            if (outputPins == null)
            {
                return false;
            }

            foreach (OutputPin outputPin in outputPins)
            {
                if (outputPin == null || outputPin.Connections == null)
                {
                    continue;
                }

                foreach (OutgoingConnection connection in outputPin.Connections)
                {
                    if (connection == null || connection.Target == null)
                    {
                        continue;
                    }

                    if (TryFindConditionDiceCheckTarget(connection.Target, visitedIds, out checkNode))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryFindDiceCheckInOutputs(List<OutputPin> outputPins, HashSet<ulong> visitedIds, out FlowFragment checkNode)
        {
            checkNode = null;

            if (outputPins == null)
            {
                return false;
            }

            foreach (OutputPin outputPin in outputPins)
            {
                if (outputPin == null || outputPin.Connections == null)
                {
                    continue;
                }

                foreach (OutgoingConnection connection in outputPin.Connections)
                {
                    if (connection == null || connection.Target == null)
                    {
                        continue;
                    }

                    if (TryFindDiceCheckTarget(connection.Target, visitedIds, out checkNode))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryFindDiceCheckTarget(ArticyObject target, HashSet<ulong> visitedIds, out FlowFragment checkNode)
        {
            checkNode = null;

            if (target == null || !visitedIds.Add(target.Id))
            {
                return false;
            }

            if (target is FlowFragment flowFragment && IsDiceCheckNode(flowFragment))
            {
                checkNode = flowFragment;
                return true;
            }

            if (target is Jump jump)
            {
                return TryFindDiceCheckTarget(jump.Target, visitedIds, out checkNode);
            }

            if (target is FlowFragment flow)
            {
                return TryFindDiceCheckInOutputs(flow.OutputPins, visitedIds, out checkNode);
            }

            if (target is Hub hub)
            {
                return TryFindDiceCheckInOutputs(hub.OutputPins, visitedIds, out checkNode);
            }

            if (target is Condition condition)
            {
                return TryFindDiceCheckInOutputs(condition.OutputPins, visitedIds, out checkNode);
            }

            if (target is Instruction instruction)
            {
                return TryFindDiceCheckInOutputs(instruction.OutputPins, visitedIds, out checkNode);
            }

            return false;
        }

        private static void AddOutputPinTargets(List<OutputPin> outputPins, List<ArticyObject> targets, HashSet<ulong> visitedIds)
        {
            if (outputPins == null)
            {
                return;
            }

            foreach (OutputPin outputPin in outputPins)
            {
                if (outputPin == null || outputPin.Connections == null)
                {
                    continue;
                }

                foreach (OutgoingConnection connection in outputPin.Connections)
                {
                    if (connection == null || connection.Target == null)
                    {
                        continue;
                    }

                    AddPlayableTargets(connection.Target, targets, visitedIds);
                }
            }
        }

        private static void AddPlayableTargets(ArticyObject target, List<ArticyObject> targets, HashSet<ulong> visitedIds)
        {
            if (target == null || visitedIds.Contains(target.Id))
            {
                return;
            }

            visitedIds.Add(target.Id);

            if (target is DialogueFragment || IsDiceCheckNode(target))
            {
                targets.Add(target);
                return;
            }

            if (target is Dialogue targetDialogue)
            {
                DialogueFragment firstChild = FindFirstDescendantDialogueFragmentSkipping(targetDialogue.Id, visitedIds);

                if (firstChild != null)
                {
                    targets.Add(firstChild);
                }

                return;
            }

            if (target is Jump jump)
            {
                AddPlayableTargets(jump.Target, targets, visitedIds);
                return;
            }

            if (target is FlowFragment flowFragment)
            {
                AddOutputPinTargets(flowFragment.OutputPins, targets, visitedIds);
                return;
            }

            if (target is Hub hub)
            {
                AddOutputPinTargets(hub.OutputPins, targets, visitedIds);
                return;
            }

            if (target is Condition condition)
            {
                AddOutputPinTargets(condition.OutputPins, targets, visitedIds);
                return;
            }

            if (target is Instruction instruction)
            {
                AddOutputPinTargets(instruction.OutputPins, targets, visitedIds);
                return;
            }

            DialogueFragment childFragment = FindFirstChildDialogueFragmentSkipping(target.Id, visitedIds);

            if (childFragment != null)
            {
                targets.Add(childFragment);
            }
        }

        private static DiceBranchResult GetDiceResultBranch(FlowFragment checkNode, bool success)
        {
            if (checkNode == null || checkNode.OutputPins == null)
            {
                return DiceBranchResult.Empty();
            }

            List<DiceBranchResult> branches = new List<DiceBranchResult>();

            foreach (OutputPin outputPin in checkNode.OutputPins)
            {
                if (outputPin == null || outputPin.Connections == null)
                {
                    continue;
                }

                foreach (OutgoingConnection connection in outputPin.Connections)
                {
                    if (connection == null || connection.Target == null)
                    {
                        continue;
                    }

                    string conditionLabel = GetConnectionConditionLabel(connection);
                    List<ArticyObject> resolvedTargets = new List<ArticyObject>();
                    AddPlayableTargets(connection.Target, resolvedTargets, new HashSet<ulong>());
                    ArticyObject target = resolvedTargets.Count > 0 ? resolvedTargets[0] : connection.Target;
                    branches.Add(new DiceBranchResult(target, conditionLabel));
                }
            }

            if (branches.Count == 0)
            {
                return DiceBranchResult.Empty();
            }

            for (int i = 0; i < branches.Count; i++)
            {
                if (IsMatchingDiceCondition(branches[i].ConditionLabel, success))
                {
                    return branches[i];
                }
            }

            int fallbackIndex = success ? 0 : Mathf.Min(1, branches.Count - 1);
            return branches[fallbackIndex];
        }

        private static string GetConnectionConditionLabel(OutgoingConnection connection)
        {
            if (connection == null)
            {
                return string.Empty;
            }

            string label = NormalizeDisplayText(connection.Label);

            if (!string.IsNullOrWhiteSpace(label))
            {
                return label;
            }

            if (connection.Target != null)
            {
                string inputPinLabel = GetInputPinConditionLabel(connection.Target, connection.TargetPin);

                if (!string.IsNullOrWhiteSpace(inputPinLabel))
                {
                    return inputPinLabel;
                }
            }

            return string.Empty;
        }

        private static string GetInputPinConditionLabel(ArticyObject target, ulong targetPin)
        {
            if (target == null || targetPin == 0)
            {
                return string.Empty;
            }

            List<InputPin> inputPins = GetInputPins(target);

            for (int i = 0; i < inputPins.Count; i++)
            {
                InputPin inputPin = inputPins[i];

                if (inputPin == null || inputPin.Id != targetPin)
                {
                    continue;
                }

                return NormalizeConditionLabel(ExtractArticyScriptText(inputPin.Text));
            }

            return string.Empty;
        }

        private static List<InputPin> GetInputPins(ArticyObject articyObject)
        {
            if (articyObject is DialogueFragment dialogueFragment)
            {
                return dialogueFragment.InputPins;
            }

            if (articyObject is Dialogue dialogue)
            {
                return dialogue.InputPins;
            }

            if (articyObject is FlowFragment flowFragment)
            {
                return flowFragment.InputPins;
            }

            if (articyObject is Hub hub)
            {
                return hub.InputPins;
            }

            if (articyObject is Jump jump)
            {
                return jump.InputPins;
            }

            if (articyObject is Condition condition)
            {
                return condition.InputPins;
            }

            if (articyObject is Instruction instruction)
            {
                return instruction.InputPins;
            }

            return new List<InputPin>();
        }

        private static string ExtractArticyScriptText(object scriptObject)
        {
            if (scriptObject == null)
            {
                return string.Empty;
            }

            Type scriptType = scriptObject.GetType();
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic;

            string[] memberNames = { "RawScript", "rawScript", "mRawScript", "Script", "script", "mScript" };

            for (int i = 0; i < memberNames.Length; i++)
            {
                System.Reflection.PropertyInfo property = scriptType.GetProperty(memberNames[i], flags);

                if (property != null && property.PropertyType == typeof(string))
                {
                    string value = property.GetValue(scriptObject, null) as string;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }

                System.Reflection.FieldInfo field = scriptType.GetField(memberNames[i], flags);

                if (field != null && field.FieldType == typeof(string))
                {
                    string value = field.GetValue(scriptObject) as string;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            string fallback = scriptObject.ToString();
            return fallback != scriptType.FullName ? fallback : string.Empty;
        }

        private static string NormalizeConditionLabel(string rawLabel)
        {
            string label = NormalizeDisplayText(rawLabel);

            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }

            label = label.Replace("//", string.Empty).Trim();
            return label;
        }

        private static bool IsMatchingDiceCondition(string conditionLabel, bool success)
        {
            if (string.IsNullOrWhiteSpace(conditionLabel))
            {
                return false;
            }

            return success
                ? ContainsAny(conditionLabel, "\u6210\u529f", "\u901a\u8fc7", "success", "true")
                : ContainsAny(conditionLabel, "\u5931\u8d25", "\u672a\u901a\u8fc7", "fail", "failure", "false");
        }

        private static DialogueFragment FindFirstChildDialogueFragment(ulong parentId)
        {
            return FindFirstChildDialogueFragmentSkipping(parentId, null);
        }

        private static DialogueFragment FindFirstChildDialogueFragmentSkipping(ulong parentId, HashSet<ulong> skipIds)
        {
            DialogueFragment firstChild = null;

            foreach (DialogueFragment fragment in ArticyDatabase.GetAllOfType<DialogueFragment>())
            {
                if (fragment == null || fragment.ParentId != parentId || (skipIds != null && skipIds.Contains(fragment.Id)))
                {
                    continue;
                }

                if (firstChild == null || fragment.Id < firstChild.Id)
                {
                    firstChild = fragment;
                }
            }

            return firstChild;
        }

        private static DialogueFragment FindFirstDescendantDialogueFragmentSkipping(ulong parentId, HashSet<ulong> skipIds)
        {
            return FindFirstDescendantDialogueFragmentSkipping(parentId, new HashSet<ulong>(), skipIds);
        }

        private static DialogueFragment FindFirstDescendantDialogueFragmentSkipping(ulong parentId, HashSet<ulong> traversalVisitedIds, HashSet<ulong> skipIds)
        {
            if (!traversalVisitedIds.Add(parentId))
            {
                return null;
            }

            DialogueFragment directChild = FindFirstChildDialogueFragmentSkipping(parentId, skipIds);

            if (directChild != null)
            {
                return directChild;
            }

            DialogueFragment firstDescendant = null;

            foreach (Dialogue dialogue in ArticyDatabase.GetAllOfType<Dialogue>())
            {
                if (dialogue == null || dialogue.ParentId != parentId)
                {
                    continue;
                }

                DialogueFragment descendant = FindFirstDescendantDialogueFragmentSkipping(dialogue.Id, traversalVisitedIds, skipIds);

                if (descendant != null && (firstDescendant == null || descendant.Id < firstDescendant.Id))
                {
                    firstDescendant = descendant;
                }
            }

            foreach (FlowFragment flowFragment in ArticyDatabase.GetAllOfType<FlowFragment>())
            {
                if (flowFragment == null || flowFragment.ParentId != parentId)
                {
                    continue;
                }

                DialogueFragment descendant = FindFirstDescendantDialogueFragmentSkipping(flowFragment.Id, traversalVisitedIds, skipIds);

                if (descendant != null && (firstDescendant == null || descendant.Id < firstDescendant.Id))
                {
                    firstDescendant = descendant;
                }
            }

            return firstDescendant;
        }

        private static DialogueFragment FindFirstDescendantDialogueFragment(ulong parentId, HashSet<ulong> visitedIds)
        {
            if (!visitedIds.Add(parentId))
            {
                return null;
            }

            DialogueFragment directChild = FindFirstChildDialogueFragment(parentId);

            if (directChild != null)
            {
                return directChild;
            }

            DialogueFragment firstDescendant = null;

            foreach (Dialogue dialogue in ArticyDatabase.GetAllOfType<Dialogue>())
            {
                if (dialogue == null || dialogue.ParentId != parentId)
                {
                    continue;
                }

                DialogueFragment descendant = FindFirstDescendantDialogueFragment(dialogue.Id, visitedIds);

                if (descendant != null && (firstDescendant == null || descendant.Id < firstDescendant.Id))
                {
                    firstDescendant = descendant;
                }
            }

            foreach (FlowFragment flowFragment in ArticyDatabase.GetAllOfType<FlowFragment>())
            {
                if (flowFragment == null || flowFragment.ParentId != parentId)
                {
                    continue;
                }

                DialogueFragment descendant = FindFirstDescendantDialogueFragment(flowFragment.Id, visitedIds);

                if (descendant != null && (firstDescendant == null || descendant.Id < firstDescendant.Id))
                {
                    firstDescendant = descendant;
                }
            }

            return firstDescendant;
        }

        private DiceCheckConfig CreateDiceCheckConfig(FlowFragment checkNode)
        {
            string label = GetChoiceLabel(checkNode);
            string sourceText = NormalizeDisplayText(string.Format("{0} {1}", label, GetArticyText(checkNode)));
            PlayerAttributeType attributeType = GetDiceAttributeType(sourceText);
            return new DiceCheckConfig(label, attributeType, Mathf.Max(1, diceDifficulty));
        }

        private static bool IsDiceCheckNode(ArticyObject articyObject)
        {
            if (!(articyObject is FlowFragment))
            {
                return false;
            }

            string label = GetChoiceLabel(articyObject);
            string text = GetNormalizedArticyText(articyObject);
            return ContainsAny(label, "\u68c0\u5b9a", "\u8bc4\u4f30") || ContainsAny(text, "\u68c0\u5b9a", "\u8bc4\u4f30");
        }

        private static PlayerAttributeType GetDiceAttributeType(string sourceText)
        {
            if (ContainsAny(sourceText, "\u903b\u8f91", "Logic"))
            {
                return PlayerAttributeType.Logic;
            }

            if (ContainsAny(sourceText, "\u611f\u77e5", "\u89c2\u5bdf", "Perception"))
            {
                return PlayerAttributeType.Perception;
            }

            if (ContainsAny(sourceText, "\u654f\u9510", "\u6d1e\u5bdf", "\u7406\u60f3", "Insight"))
            {
                return PlayerAttributeType.Insight;
            }

            if (ContainsAny(sourceText, "\u52a1\u5b9e", "\u6297\u538b", "\u97e7\u6027", "Resilience"))
            {
                return PlayerAttributeType.Resilience;
            }

            return PlayerAttributeType.Perception;
        }

        private static bool ContainsAny(string text, params string[] candidates)
        {
            if (string.IsNullOrWhiteSpace(text) || candidates == null)
            {
                return false;
            }

            foreach (string candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) && text.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private readonly struct DiceBranchResult
        {
            public ArticyObject Target { get; }
            public string ConditionLabel { get; }

            public DiceBranchResult(ArticyObject target, string conditionLabel)
            {
                Target = target;
                ConditionLabel = conditionLabel;
            }

            public static DiceBranchResult Empty()
            {
                return new DiceBranchResult(null, string.Empty);
            }
        }

        private readonly struct DiceCheckConfig
        {
            public string Label { get; }
            public PlayerAttributeType AttributeType { get; }
            public int Difficulty { get; }

            public DiceCheckConfig(string label, PlayerAttributeType attributeType, int difficulty)
            {
                Label = string.IsNullOrWhiteSpace(label) ? "DiceCheck" : label;
                AttributeType = attributeType;
                Difficulty = difficulty;
            }
        }

        private static string GetChoiceLabel(ArticyObject articyObject)
        {
            if (articyObject == null)
            {
                return string.Empty;
            }

            if (articyObject is DialogueFragment fragment)
            {
                return NormalizeDisplayText(fragment.MenuText);
            }

            return string.Empty;
        }

        private static string GetChoiceButtonLabel(ArticyObject articyObject)
        {
            string label = GetChoiceLabel(articyObject);
            return string.IsNullOrWhiteSpace(label) ? "\u7ee7\u7eed" : label;
        }

        private static string GetDialogueLineText(DialogueFragment fragment)
        {
            string text = GetNormalizedArticyText(fragment);

            if (fragment == null || string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            string speakerName = GetSpeakerDisplayName(GetFragmentSpeaker(fragment));
            if (string.IsNullOrWhiteSpace(speakerName))
            {
                return text;
            }

            return $"{speakerName}\uff1a{text}";
        }

        private static string GetNormalizedArticyText(object articyObject)
        {
            return NormalizeDisplayText(GetArticyText(articyObject));
        }

        private static string GetArticyText(object articyObject)
        {
            if (articyObject is IObjectWithText objectWithText)
            {
                return objectWithText.Text;
            }

            if (articyObject is IObjectWithLocalizableText objectWithLocalizableText)
            {
                return objectWithLocalizableText.Text;
            }

            return string.Empty;
        }

        private static string NormalizeDisplayText(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return string.Empty;
            }

            string text = rawText.Replace("\\n", "\n").Replace("\\r", "\r");

            if (text.Contains("\\u"))
            {
                try
                {
                    text = Regex.Unescape(text);
                }
                catch (ArgumentException)
                {
                    // Keep original text when it is not a valid escaped string.
                }
            }

            string repaired = TryRepairMojibake(text);

            if (!string.IsNullOrWhiteSpace(repaired))
            {
                text = repaired;
            }

            return text.Trim();
        }

        private static string TryRepairMojibake(string text)
        {
            if (!LooksLikeMojibake(text))
            {
                return string.Empty;
            }

            string best = text;
            int bestScore = GetMojibakeScore(text);

            Encoding windowsLatin = TryGetEncoding(1252);
            Encoding gbk = TryGetEncoding(936);

            if (windowsLatin != null)
            {
                TryCandidate(windowsLatin, Encoding.UTF8, text, ref best, ref bestScore);
            }

            if (gbk != null)
            {
                TryCandidate(Encoding.UTF8, gbk, text, ref best, ref bestScore);
                TryCandidate(gbk, Encoding.UTF8, text, ref best, ref bestScore);
            }

            return bestScore < GetMojibakeScore(text) ? best : string.Empty;
        }

        private static Encoding TryGetEncoding(int codePage)
        {
            try
            {
                return Encoding.GetEncoding(codePage);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void TryCandidate(Encoding sourceEncoding, Encoding targetEncoding, string text, ref string best, ref int bestScore)
        {
            try
            {
                string candidate = targetEncoding.GetString(sourceEncoding.GetBytes(text));
                int score = GetMojibakeScore(candidate);

                if (score < bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }
            catch (Exception)
            {
                // Some platforms do not include every code page. The original text remains usable.
            }
        }

        private static int GetMojibakeScore(string text)
        {
            int score = 0;

            foreach (char character in text)
            {
                if (character == '\uFFFD' || character == '\u25A1')
                {
                    score += 3;
                }
                else if ("\u6C13\u76F2\u83BD\u8119\u8117\u935B\u714E\u5F68\u95AB\u763D".IndexOf(character) >= 0)
                {
                    score++;
                }
            }

            return score;
        }

        private static bool LooksLikeMojibake(string text)
        {
            return text.IndexOf('\u6C13') >= 0
                || text.IndexOf('\u76F2') >= 0
                || text.IndexOf('\u83BD') >= 0
                || text.IndexOf('\u8119') >= 0
                || text.IndexOf('\u8117') >= 0
                || text.IndexOf('\u935B') >= 0
                || text.IndexOf('\u714E') >= 0
                || text.IndexOf('\u5F68') >= 0
                || text.IndexOf('\u95AB') >= 0
                || text.IndexOf('\u763D') >= 0
                || text.IndexOf('\u25A1') >= 0;
        }

        private static string FitChoiceLabel(string label)
        {
            const int maxLength = 42;
            string normalized = NormalizeDisplayText(label).Replace("\n", " ");
            return normalized.Length > maxLength ? normalized.Substring(0, maxLength) + "..." : normalized;
        }

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void OnPlayerStressChanged(StressChangedEventArgs args)
        {
            RefreshHud();
        }

        private void OnCigaretteChanged(CigaretteChangedEventArgs args)
        {
            RefreshHud();
        }

        private void ReturnMainRoom()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing. Return requires starting from SCN_MainRoom.");
                return;
            }

            GameManager.Instance.EnterMainRoom();
        }
    }
}
