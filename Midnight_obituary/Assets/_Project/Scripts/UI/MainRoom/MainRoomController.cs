using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Call;
using ObituaryTomorrow.Gameplay.Dice;
using ObituaryTomorrow.Gameplay.Ending;
using ObituaryTomorrow.Gameplay.Items;
using ObituaryTomorrow.Gameplay.NPC;
using ObituaryTomorrow.Gameplay.Player;
using ObituaryTomorrow.Gameplay.Save;

namespace ObituaryTomorrow.UI
{
    public sealed class MainRoomController : MonoBehaviour
    {
        private const string DefaultMissionId = "MIS_Lena_001";
        private const string DefaultNpcId = "NPC_Lena_001";
        private const string DefaultDialogueId = "DIA_Lena_001";
        [Header("Gameplay")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private NPCManager npcManager;
        [SerializeField] private DiceSystem diceSystem;
        [SerializeField] private CallCounterSystem callCounterSystem;
        [SerializeField] private EndingEvaluator endingEvaluator;
        [SerializeField] private ObituaryTomorrow.Gameplay.Items.CigaretteSystem cigaretteSystem;
        [SerializeField] private ObituaryTomorrow.Gameplay.Save.SaveManager saveManager;
        [SerializeField] private int requiredDeepRescueSuccesses = 3;

        [Header("Desk Buttons")]
        [SerializeField] private Button buttonSettings;
        [SerializeField] private Button buttonOpenNewspaper;
        [SerializeField] private Button buttonOpenYellowPages;
        [SerializeField] private Button buttonOpenTaskBook;
        [SerializeField] private Button buttonOpenCard;
        [SerializeField] private Button buttonOpenAchievement;
        [SerializeField] private Button buttonOpenSave;
        [SerializeField] private Button buttonDial;
        [SerializeField] private Button buttonSmoking;
        [SerializeField] private Button buttonDiceTest;

        [Header("Popup Buttons")]
        [SerializeField] private Button buttonConfirmMission;
        [SerializeField] private Button[] buttonClosePopups;
        [SerializeField] private Button buttonConfirmResult;

        [Header("Popup Panels")]
        [SerializeField] private GameObject panelPopupRoot;
        [SerializeField] private GameObject panelSettings;
        [SerializeField] private GameObject panelNewspaper;
        [SerializeField] private GameObject panelYellowPages;
        [SerializeField] private GameObject panelTaskBook;
        [SerializeField] private GameObject panelCard;
        [SerializeField] private GameObject panelAchievement;
        [SerializeField] private GameObject panelSave;
        [SerializeField] private GameObject panelResult;

        [Header("Smoking Animation")]
        [SerializeField] private SmokingAnimationController smokingAnimationController;

        [Header("HUD Texts")]
        [SerializeField] private TextMeshProUGUI textHud;
        [SerializeField] private TextMeshProUGUI textObituary;
        [SerializeField] private TextMeshProUGUI textYellowPages;
        [SerializeField] private TextMeshProUGUI textTaskBook;
        [SerializeField] private TextMeshProUGUI textAchievement;
        [SerializeField] private TextMeshProUGUI textSaveStatus;

        [Header("Dialogue Area")]
        [SerializeField] private GameObject dialogueAreaRoot;
        [SerializeField] private Image imageNpcPortrait;
        [SerializeField] private Image imageNpcName;
        [SerializeField] private Image imageNpcCard;
        [SerializeField] private TextMeshProUGUI textNpcName;
        [SerializeField] private TextMeshProUGUI textNpcBreakdown;
        [SerializeField] private TextMeshProUGUI textNpcCard;
        [SerializeField] private TextMeshProUGUI textDialogue;
        [SerializeField] private TextMeshProUGUI textDice;
        [SerializeField] private Transform groupChoiceButtons;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private CallGreyboxController callGreyboxController;
        [SerializeField] private Image imageCounter;
        [SerializeField] private Image imageStress;
        [SerializeField] private Image imagePerception;
        [SerializeField] private Image imageLogic;
        [SerializeField] private Image imagePractical;
        [SerializeField] private Image imageIdeal;
        [SerializeField] private int maxVisibleAttributeValue = 6;

        [Header("Dice Test")]
        [SerializeField] private TextMeshProUGUI textDiceResult;
        [SerializeField] private GameObject[] leftDiceFaceObjects;
        [SerializeField] private GameObject[] rightDiceFaceObjects;
        [SerializeField] private float diceAnimationStepSeconds = 0.08f;
        [SerializeField] private int diceAnimationFrames = 10;

        [Header("Result Texts")]
        [SerializeField] private TextMeshProUGUI textResultEnding;
        [SerializeField] private TextMeshProUGUI textResultAchievement;

        [Header("Temporary Dialogue Data")]
        [SerializeField] private DialogueChoiceConfig[] dialogueChoices =
        {
            new DialogueChoiceConfig(
                "\u6211\u4e0d\u4f1a\u903c\u4f60\u56de\u7b54\u3002\u5148\u544a\u8bc9\u6211\uff0c\u4f60\u73b0\u5728\u5b89\u5168\u5417\uff1f",
                PersonalityTag.Emotional,
                PlayerAttributeType.Perception,
                3,
                true,
                -1,
                1,
                "\u7535\u8bdd\u53e6\u4e00\u7aef\u7684\u547c\u5438\u58f0\u6162\u4e86\u4e00\u70b9\uff0c\u5bf9\u65b9\u613f\u610f\u7ee7\u7eed\u542c\u4f60\u8bf4\u3002",
                "\u4f60\u7684\u8bed\u6c14\u6ca1\u80fd\u7a7f\u8fc7\u566a\u58f0\uff0c\u5bf9\u65b9\u53d8\u5f97\u66f4\u9632\u5907\u3002"),
            new DialogueChoiceConfig(
                "\u6309\u65f6\u95f4\u7ebf\u68b3\u7406\u4e00\u4e0b\uff1a\u4eca\u665a\u53d1\u751f\u4e86\u4ec0\u4e48\uff1f",
                PersonalityTag.Rational,
                PlayerAttributeType.Logic,
                3,
                true,
                -1,
                1,
                "\u4f60\u6293\u4f4f\u4e86\u77db\u76fe\u5904\uff0c\u5bf9\u65b9\u5f00\u59cb\u590d\u8ff0\u4eca\u665a\u7684\u7ecf\u8fc7\u3002",
                "\u8fc7\u4e8e\u51b7\u9759\u7684\u95ee\u9898\u523a\u75db\u4e86\u5bf9\u65b9\uff0c\u7535\u8bdd\u91cc\u51fa\u73b0\u957f\u65f6\u95f4\u6c89\u9ed8\u3002"),
            new DialogueChoiceConfig(
                "\u5148\u505a\u4e00\u4ef6\u80fd\u7acb\u523b\u5b8c\u6210\u7684\u5c0f\u4e8b\uff1a\u628a\u7a97\u6237\u5173\u4e0a\uff0c\u5750\u56de\u6905\u5b50\u3002",
                PersonalityTag.Practical,
                PlayerAttributeType.Resilience,
                5,
                true,
                -1,
                1,
                "\u5bf9\u65b9\u7167\u505a\u4e86\u3002\u623f\u95f4\u91cc\u7684\u96e8\u58f0\u88ab\u9694\u5728\u7a97\u5916\u3002",
                "\u73b0\u5b9e\u5efa\u8bae\u6765\u5f97\u592a\u5feb\uff0c\u5bf9\u65b9\u89c9\u5f97\u4f60\u6839\u672c\u6ca1\u6709\u7406\u89e3\u75db\u82e6\u3002")
        };

        private readonly List<Button> spawnedChoiceButtons = new List<Button>();
        private bool missionConfirmed;
        private bool inCall;
        private int deepRescueSuccessCount;
        private Coroutine diceAnimationRoutine;

        private void Awake()
        {
            ResolveGameplayReferences();
            ResolveSceneReferences();
        }

        private void OnEnable()
        {
            GameEventBus.PlayerStressChanged += OnPlayerStressChanged;
            GameEventBus.NPCBreakdownChanged += OnNPCBreakdownChanged;
            GameEventBus.CallCounterChanged += OnCallCounterChanged;
            GameEventBus.CigaretteChanged += OnCigaretteChanged;

            AddListener(buttonSettings, OpenSettings);
            AddListener(buttonOpenNewspaper, OpenNewspaper);
            AddListener(buttonOpenYellowPages, OpenYellowPages);
            AddListener(buttonOpenTaskBook, OpenTaskBook);
            AddListener(buttonOpenCard, OpenCard);
            AddListener(buttonOpenAchievement, OpenAchievement);
            AddListener(buttonOpenSave, OpenSave);
            AddListener(buttonDial, StartCall);
            AddListener(buttonSmoking, StartSmoking);
            AddListener(buttonDiceTest, RollDiceTest);
            AddListener(buttonConfirmMission, ConfirmMission);
            AddListeners(buttonClosePopups, ClosePopup);
            AddListener(buttonConfirmResult, ConfirmResult);
        }

        private void OnDisable()
        {
            GameEventBus.PlayerStressChanged -= OnPlayerStressChanged;
            GameEventBus.NPCBreakdownChanged -= OnNPCBreakdownChanged;
            GameEventBus.CallCounterChanged -= OnCallCounterChanged;
            GameEventBus.CigaretteChanged -= OnCigaretteChanged;

            RemoveListener(buttonSettings, OpenSettings);
            RemoveListener(buttonOpenNewspaper, OpenNewspaper);
            RemoveListener(buttonOpenYellowPages, OpenYellowPages);
            RemoveListener(buttonOpenTaskBook, OpenTaskBook);
            RemoveListener(buttonOpenCard, OpenCard);
            RemoveListener(buttonOpenAchievement, OpenAchievement);
            RemoveListener(buttonOpenSave, OpenSave);
            RemoveListener(buttonDial, StartCall);
            RemoveListener(buttonSmoking, StartSmoking);
            RemoveListener(buttonDiceTest, RollDiceTest);
            RemoveListener(buttonConfirmMission, ConfirmMission);
            RemoveListeners(buttonClosePopups, ClosePopup);
            RemoveListener(buttonConfirmResult, ConfirmResult);

            ClearChoiceButtons();
        }

        private void Start()
        {
            missionConfirmed = false;
            inCall = false;
            deepRescueSuccessCount = 0;

            HideAllPopups();
            ResetDialogueArea();
            RefreshStaticTexts();
            RefreshHud();
            RefreshInteractableState();
        }

        public void SaveSlot1()
        {
            EnsureSaveManager().SaveSlot1();
        }

        public void SaveSlot2()
        {
            EnsureSaveManager().SaveSlot2();
        }

        public void SaveSlot3()
        {
            EnsureSaveManager().SaveSlot3();
        }

        public void LoadSlot1()
        {
            EnsureSaveManager().LoadSlot1();
        }

        public void LoadSlot2()
        {
            EnsureSaveManager().LoadSlot2();
        }

        public void LoadSlot3()
        {
            EnsureSaveManager().LoadSlot3();
        }

        public OperationResult SaveGameSlot(int slotIndex)
        {
            return EnsureSaveManager().SaveSlot(slotIndex);
        }

        public OperationResult LoadGameSlot(int slotIndex)
        {
            return EnsureSaveManager().LoadSlot(slotIndex);
        }

        private ObituaryTomorrow.Gameplay.Save.SaveManager EnsureSaveManager()
        {
            if (saveManager == null)
            {
                saveManager = FindFirstObjectByType<ObituaryTomorrow.Gameplay.Save.SaveManager>();
            }

            if (saveManager == null)
            {
                saveManager = gameObject.AddComponent<ObituaryTomorrow.Gameplay.Save.SaveManager>();
            }

            return saveManager;
        }

        private void OpenSettings()
        {
            OpenPopup(panelSettings, GameState.MainRoom);
        }

        private void OpenNewspaper()
        {
            OpenPopup(panelNewspaper, GameState.ObituaryView);
        }

        private void OpenYellowPages()
        {
            OpenPopup(panelYellowPages, GameState.YellowPagesView);
        }

        private void OpenTaskBook()
        {
            OpenPopup(panelTaskBook, GameState.MainRoom);
        }

        private void OpenCard()
        {
            OpenPopup(panelCard, GameState.MainRoom);
        }

        private void OpenAchievement()
        {
            OpenPopup(panelAchievement, GameState.MainRoom);
        }


        private void OpenSave()
        {
            EnsureSavePanel();
            RefreshSavePanel();
            HideAllPopups();
            SetPanelVisible(panelPopupRoot, true);
            SetPanelVisible(panelSave, true);

            if (!inCall)
            {
                GameManager.Instance?.ChangeState(GameState.MainRoom);
            }
        }

        private void SaveToSlot(int slotIndex)
        {
            OperationResult result = SaveGameSlot(slotIndex);
            SetText(textSaveStatus, result.Success ? "\u5b58\u6863\u6210\u529f" : result.Message);
            RefreshSavePanel();
        }

        private void LoadFromSlot(int slotIndex)
        {
            OperationResult result = LoadGameSlot(slotIndex);
            SetText(textSaveStatus, result.Success ? "\u8bfb\u6863\u6210\u529f" : result.Message);
            RefreshRuntimeIndicators();
            RefreshSavePanel();
        }

        private void RefreshSavePanel()
        {
            if (panelSave == null)
            {
                return;
            }

            ObituaryTomorrow.Gameplay.Save.SaveManager manager = EnsureSaveManager();
            TextMeshProUGUI[] texts = panelSave.GetComponentsInChildren<TextMeshProUGUI>(true);

            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshProUGUI text = texts[i];

                if (text == null || !text.name.StartsWith("Text_SaveSlot_", StringComparison.Ordinal))
                {
                    continue;
                }

                string suffix = text.name.Substring("Text_SaveSlot_".Length);
                if (!int.TryParse(suffix, out int slotIndex))
                {
                    continue;
                }

                string state = manager.HasSlot(slotIndex) ? "\u5df2\u6709\u5b58\u6863" : "\u7a7a\u5b58\u6863";
                text.text = string.Format("\u5b58\u6863 {0}  {1}", slotIndex, state);
            }
        }

        private void EnsureSavePanel()
        {
            if (panelSave != null)
            {
                return;
            }

            AssignIfMissing(ref panelSave, FindGameObjectByName("Panel_Save"));

            if (panelSave != null)
            {
                return;
            }

            EnsurePopupRoot();

            if (panelPopupRoot == null)
            {
                Debug.LogWarning("Cannot create save panel because no popup root or canvas exists.");
                return;
            }

            panelSave = CreateSavePanel(panelPopupRoot.transform);
        }

        private void EnsurePopupRoot()
        {
            if (panelPopupRoot != null)
            {
                return;
            }

            AssignIfMissing(ref panelPopupRoot, FindGameObjectByName("Panel_PopupRoot"));
            AssignIfMissing(ref panelPopupRoot, FindGameObjectByName("Panel_PopUpRoot"));
            AssignIfMissing(ref panelPopupRoot, FindGameObjectByName("Panel_Popup"));

            if (panelPopupRoot != null)
            {
                return;
            }

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            panelPopupRoot = new GameObject("Panel_PopupRoot", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform rectTransform = panelPopupRoot.GetComponent<RectTransform>();
            panelPopupRoot.transform.SetParent(canvas.transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private GameObject CreateSavePanel(Transform parent)
        {
            GameObject panel = new GameObject("Panel_Save", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(760f, 500f);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.07f, 0.05f, 0.94f);

            TextMeshProUGUI title = CreateRuntimeText(panel.transform, "Text_SaveTitle", "\u5b58\u6863", new Vector2(0f, 196f), new Vector2(640f, 54f), 34f, TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;

            for (int i = 1; i <= 3; i++)
            {
                CreateSaveSlotRow(panel.transform, i, 118f - (i - 1) * 102f);
            }

            textSaveStatus = CreateRuntimeText(panel.transform, "Text_SaveStatus", string.Empty, new Vector2(0f, -178f), new Vector2(560f, 32f), 22f, TextAlignmentOptions.Center);
            CreateRuntimeButton(panel.transform, "Button_SaveBack", "\u8fd4\u56de", new Vector2(0f, -224f), new Vector2(180f, 48f), ClosePopup);
            panel.SetActive(false);
            return panel;
        }

        private void CreateSaveSlotRow(Transform parent, int slotIndex, float y)
        {
            GameObject row = new GameObject($"SaveSlot_{slotIndex}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            row.transform.SetParent(parent, false);

            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.sizeDelta = new Vector2(620f, 74f);
            rowRect.anchoredPosition = new Vector2(0f, y);

            Image rowImage = row.GetComponent<Image>();
            rowImage.color = new Color(0.18f, 0.15f, 0.11f, 0.88f);

            CreateRuntimeText(row.transform, $"Text_SaveSlot_{slotIndex}", string.Empty, new Vector2(-160f, 0f), new Vector2(260f, 48f), 24f, TextAlignmentOptions.Left);
            int capturedSlotIndex = slotIndex;
            CreateRuntimeButton(row.transform, $"Button_SaveSlot_{slotIndex}", "\u4fdd\u5b58", new Vector2(108f, 0f), new Vector2(112f, 44f), () => SaveToSlot(capturedSlotIndex));
            CreateRuntimeButton(row.transform, $"Button_LoadSlot_{slotIndex}", "\u52a0\u8f7d", new Vector2(242f, 0f), new Vector2(112f, 44f), () => LoadFromSlot(capturedSlotIndex));
        }

        private static TextMeshProUGUI CreateRuntimeText(Transform parent, string objectName, string value, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.93f, 0.86f, 0.72f, 1f);
            text.enableWordWrapping = false;
            return text;
        }

        private static Button CreateRuntimeButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.78f, 0.68f, 0.48f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            TextMeshProUGUI buttonText = CreateRuntimeText(buttonObject.transform, "Text", label, Vector2.zero, size, 22f, TextAlignmentOptions.Center);
            buttonText.color = new Color(0.08f, 0.07f, 0.05f, 1f);
            buttonText.fontStyle = FontStyles.Bold;
            return button;
        }

        private void ConfirmMission()
        {
            missionConfirmed = true;
            GameManager.Instance?.SetCurrentMission(DefaultMissionId);
            RefreshStaticTexts();
            RefreshInteractableState();
            ClosePopup();
            ShowDialoguePrompt("\u4efb\u52a1\u5df2\u8bb0\u5f55\u3002\u73b0\u5728\u53ef\u4ee5\u67e5\u9ec4\u9875\u5e76\u62e8\u6253\u7535\u8bdd\u3002");
        }

        private void ClosePopup()
        {
            HideAllPopups();

            if (!inCall && GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.MainRoom);
            }
        }

        private void StartSmoking()
        {
            if (cigaretteSystem == null)
            {
                Debug.LogWarning("CigaretteSystem is missing.");
                return;
            }

            if (cigaretteSystem.Count <= 0)
            {
                ShowDialoguePrompt("\u6ca1\u6709\u9999\u70df\u4e86\u3002");
                return;
            }

            OperationResult requestResult = cigaretteSystem.RequestUseCigarette();
            if (!requestResult.Success)
            {
                ShowDialoguePrompt(requestResult.Message);
                return;
            }

            RefreshInteractableState();

            if (smokingAnimationController != null)
            {
                smokingAnimationController.OnAnimationComplete -= OnSmokingAnimationComplete;
                smokingAnimationController.OnAnimationComplete += OnSmokingAnimationComplete;
                smokingAnimationController.Play();
            }
            else
            {
                ConfirmSmoke();
            }
        }

        private void OnSmokingAnimationComplete()
        {
            if (smokingAnimationController != null)
            {
                smokingAnimationController.OnAnimationComplete -= OnSmokingAnimationComplete;
            }

            ConfirmSmoke();
        }

        private void ConfirmSmoke()
        {
            if (cigaretteSystem == null)
            {
                return;
            }

            StatChangeResult result = cigaretteSystem.ConfirmUseCigarette();
            RefreshHud();
            RefreshInteractableState();

            if (result.NewValue <= 0)
            {
                ShowDialoguePrompt("\u9999\u70df\u5df2\u7ecf\u62bd\u5b8c\u4e86\u3002");
            }
        }

        private void StartCall()
        {
            if (!missionConfirmed)
            {
                missionConfirmed = true;
                GameManager.Instance?.SetCurrentMission(DefaultMissionId);
                RefreshStaticTexts();
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing. Put GameManager in SCN_MainRoom before testing the formal scene.");
                return;
            }

            ClosePopup();
            inCall = true;
            deepRescueSuccessCount = 0;

            GameManager.Instance.SetCurrentMission(DefaultMissionId);
            GameManager.Instance.StartCall(DefaultNpcId, DefaultDialogueId);
            npcManager?.BeginCall(DefaultNpcId);
            callCounterSystem?.BeginCall(DefaultNpcId);
            StartEmbeddedCallFlow();
            RefreshInteractableState();
        }

        private void StartEmbeddedCallFlow()
        {
            if (dialogueAreaRoot == null)
            {
                Debug.LogWarning("Panel_DialogueArea is missing; cannot start embedded call flow.");
                return;
            }

            SetPanelVisible(dialogueAreaRoot, true);

            if (callGreyboxController == null)
            {
                callGreyboxController = dialogueAreaRoot.GetComponent<CallGreyboxController>();
            }

            if (callGreyboxController == null)
            {
                callGreyboxController = dialogueAreaRoot.AddComponent<CallGreyboxController>();
            }

            callGreyboxController.ConfigureForMainRoom(
                playerManager,
                callCounterSystem,
                textNpcName,
                imageNpcName != null ? imageNpcName : imageNpcPortrait,
                textDialogue,
                textHud,
                textDice,
                groupChoiceButtons,
                choiceButtonPrefab,
                null);
            callGreyboxController.BeginCall(true);
            RefreshRuntimeIndicators();
        }


        public bool RefreshCurrentArticySpeakerPresentation()
        {
            if (callGreyboxController == null)
            {
                callGreyboxController = dialogueAreaRoot != null
                    ? dialogueAreaRoot.GetComponent<CallGreyboxController>()
                    : FindFirstObjectByType<CallGreyboxController>();
            }

            if (callGreyboxController == null)
            {
                return false;
            }

            callGreyboxController.ConfigureForMainRoom(
                playerManager,
                callCounterSystem,
                textNpcName,
                imageNpcName != null ? imageNpcName : imageNpcPortrait,
                textDialogue,
                textHud,
                textDice,
                groupChoiceButtons,
                choiceButtonPrefab,
                null);

            return callGreyboxController.RefreshCurrentArticySpeakerPresentation();
        }

        private void RollDiceTest()
        {
            if (diceSystem == null)
            {
                SetText(textDiceResult, "\u9ab0\u5b50\u7cfb\u7edf\u7f3a\u5931");
                return;
            }

            DiceResult result = diceSystem.RollCheck(new DiceCheckRequest(
                "DiceTest",
                PlayerAttributeType.Perception,
                7));

            if (diceAnimationRoutine != null)
            {
                StopCoroutine(diceAnimationRoutine);
            }

            diceAnimationRoutine = StartCoroutine(PlayDiceAnimation(result));
        }

        private IEnumerator PlayDiceAnimation(DiceResult result)
        {
            EnsureDiceFaceObjects();
            int frames = Mathf.Max(1, diceAnimationFrames);
            for (int i = 0; i < frames; i++)
            {
                ShowDiceFace(leftDiceFaceObjects, UnityEngine.Random.Range(1, 7));
                ShowDiceFace(rightDiceFaceObjects, UnityEngine.Random.Range(1, 7));
                yield return new WaitForSeconds(Mathf.Max(0.01f, diceAnimationStepSeconds));
            }

            ShowDiceFace(leftDiceFaceObjects, result.PositiveD6);
            ShowDiceFace(rightDiceFaceObjects, result.NegativeD6);
            SetText(textDiceResult, result.Success ? "\u6210\u529f" : "\u5931\u8d25");
            diceAnimationRoutine = null;
        }

        private void ShowDiceFace(GameObject[] diceFaces, int point)
        {
            EnsureDiceFaceObjects();

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
        private void SelectChoice(DialogueChoiceConfig choice)
        {
            if (!inCall)
            {
                return;
            }

            callCounterSystem?.RegisterPlayerLine(choice.Label);

            if (playerManager != null && !playerManager.HasPersonalityTag(choice.PersonalityTag))
            {
                playerManager.RequestStressChange(new StressChangeRequest(
                    1,
                    StatChangeReason.DialogueChoice,
                    choice.Label,
                    true));
            }

            bool diceSuccess = true;
            DiceResult diceResult = default;

            if (choice.RequiresDice && diceSystem != null)
            {
                diceResult = diceSystem.RollCheck(new DiceCheckRequest(
                    choice.Label,
                    choice.AttributeType,
                    choice.Difficulty));
                diceSuccess = diceResult.Success;
            }

            ApplyNpcBreakdown(choice, diceSuccess);

            if (diceSuccess && IsChoiceMatchingNpc(choice) && choice.MarksDeepRescueProgress)
            {
                deepRescueSuccessCount++;
            }

            RefreshNpcTexts();
            RefreshDiceText(choice, diceSuccess, diceResult);
            ShowDialoguePrompt(diceSuccess ? choice.SuccessText : choice.FailureText);

            bool deepRescueAchieved = deepRescueSuccessCount >= Mathf.Max(1, requiredDeepRescueSuccesses);
            EndingResult endingResult = endingEvaluator != null
                ? endingEvaluator.EvaluateCallState(DefaultMissionId, deepRescueAchieved)
                : EndingResult.None();

            if (endingResult.Type != EndingType.None)
            {
                FinishCall(endingResult);
            }
        }

        private void ApplyNpcBreakdown(DialogueChoiceConfig choice, bool diceSuccess)
        {
            if (npcManager == null)
            {
                return;
            }

            int delta;

            if (IsChoiceMatchingNpc(choice))
            {
                delta = diceSuccess ? choice.NpcBreakdownDeltaOnSuccess : choice.NpcBreakdownDeltaOnFailure;
            }
            else
            {
                delta = Mathf.Max(1, choice.NpcBreakdownDeltaOnFailure);
            }

            npcManager.RequestBreakdownChange(new NPCBreakdownChangeRequest(
                npcManager.CurrentNpcId,
                delta,
                StatChangeReason.DialogueChoice,
                choice.Label));
        }

        private bool IsChoiceMatchingNpc(DialogueChoiceConfig choice)
        {
            return npcManager == null || npcManager.PersonalityTag == choice.PersonalityTag;
        }

        private void FinishCall(EndingResult endingResult)
        {
            inCall = false;
            ClearChoiceButtons();
            GameManager.Instance?.FinishCall(endingResult);
            ShowResult(endingResult);
            RefreshInteractableState();
        }

        private void ShowResult(EndingResult endingResult)
        {
            OpenPopup(panelResult, endingResult.ShouldEndGame ? GameState.GameOver : GameState.Result);

            if (textResultEnding != null)
            {
                textResultEnding.text = $"\u8fbe\u6210\u7ed3\u5c40\uff1a{GetEndingDisplayName(endingResult.Type)}";
            }

            if (textResultAchievement != null)
            {
                textResultAchievement.text = $"\u8fbe\u6210\u6210\u5c31\uff1a{GetAchievementDisplayName(endingResult.Type)}";
            }
        }

        private void ConfirmResult()
        {
            ClosePopup();
            ResetDialogueArea();
            GameManager.Instance?.ChangeState(GameState.MainRoom);
        }

        private void BuildChoiceButtons()
        {
            if (groupChoiceButtons == null || choiceButtonPrefab == null)
            {
                Debug.LogWarning("Choice button group or prefab is missing.");
                return;
            }

            ClearChoiceButtons();
            choiceButtonPrefab.gameObject.SetActive(false);

            DialogueChoiceConfig[] choices = GetDialogueChoices();

            for (int i = 0; i < choices.Length; i++)
            {
                DialogueChoiceConfig choice = choices[i];
                Button button = Instantiate(choiceButtonPrefab, groupChoiceButtons);
                button.gameObject.SetActive(true);

                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    buttonText.text = choice.GetButtonLabel();
                }

                button.onClick.AddListener(() => SelectChoice(choice));
                spawnedChoiceButtons.Add(button);
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

        private DialogueChoiceConfig[] GetDialogueChoices()
        {
            return dialogueChoices != null && dialogueChoices.Length > 0
                ? dialogueChoices
                : Array.Empty<DialogueChoiceConfig>();
        }

        private void OpenPopup(GameObject targetPanel, GameState state)
        {
            HideAllPopups();
            SetPanelVisible(panelPopupRoot, true);
            SetPanelVisible(targetPanel, true);
            GameManager.Instance?.ChangeState(state);
        }

        private void HideAllPopups()
        {
            SetPanelVisible(panelPopupRoot, false);
            SetPanelVisible(panelSettings, false);
            SetPanelVisible(panelNewspaper, false);
            SetPanelVisible(panelYellowPages, false);
            SetPanelVisible(panelTaskBook, false);
            SetPanelVisible(panelCard, false);
            SetPanelVisible(panelAchievement, false);
            SetPanelVisible(panelSave, false);
            SetPanelVisible(panelResult, false);
        }


        private void ResetDialogueArea()
        {
            SetPanelVisible(dialogueAreaRoot, true);
            ShowDialoguePrompt("\u6587\u672c\u533a\uff1a\u70b9\u51fb\u62a5\u7eb8\u3001\u9ec4\u9875\u6216\u62e8\u6253\u7535\u8bdd\u5f00\u59cb\u63a8\u8fdb\u3002");
            SetText(textDice, string.Empty);
            SetText(textDiceResult, "\u7ed3\u679c");
            EnsureDiceFaceObjects();
            ShowDiceFace(leftDiceFaceObjects, 1);
            ShowDiceFace(rightDiceFaceObjects, 1);
            RefreshNpcTexts();
        }

        private void ShowDialoguePrompt(string message)
        {
            SetText(textDialogue, message);
        }

        private void RefreshStaticTexts()
        {
            SetText(textObituary, "\u8ba3\u544a\uff1aLena\uff0c\u5c06\u4e8e\u4eca\u665a 11:45 \u6b7b\u4ea1\u3002\u7535\u8bdd\u7ebf\u7d22\u5c1a\u5f85\u6838\u5bf9\u3002");
            SetText(textYellowPages, missionConfirmed ? "Lena - 555-0134" : "");
            SetText(textTaskBook, "\u4efb\u52a1\n\n- \u9605\u8bfb\u62a5\u7eb8\n- \u67e5\u627e\u9ec4\u9875\n- \u62e8\u6253\u7535\u8bdd\n- \u5728\u901a\u8bdd\u4e2d\u5b8c\u6210\u9ab0\u5b50\u5224\u5b9a");
            SetText(textAchievement, "\u6210\u5c31\u56fe\u9274\n\n- \u5f7b\u5e95\u6551\u8d4e\n- \u62d6\u65f6\u6539\u5199\n- \u672a\u80fd\u633d\u56de\n- \u7cbe\u795e\u5d29\u6e83");
        }

        private void RefreshHud()
        {
            PlayerRuntimeData playerData = GetPlayerRuntimeData();

            if (textHud != null)
            {
                textHud.text = playerData != null
                    ? $"\u538b\u529b\uff1a{playerData.CurrentStress}/{playerData.MaxStress}"
                    : "\u538b\u529b\uff1a?/?";
            }

            RefreshStressImage(playerData);
            RefreshPlayerAttributeImages(playerData);
        }

        private void RefreshNpcTexts()
        {
            string breakdown = npcManager != null
                ? $"\u5d29\u6e83\u503c\uff1a{npcManager.CurrentBreakdown}/{npcManager.MaxBreakdown}"
                : "\u5d29\u6e83\u503c\uff1a?/?";
            string card = npcManager != null ? $"NPC\u5361\u724c\uff1a{npcManager.PersonalityTag}" : "NPC\u5361\u724c\uff1a\u672a\u77e5";

            SetText(textNpcBreakdown, breakdown);
            RefreshBreakdownPips();
            SetText(textNpcCard, card);

            if (imageNpcCard != null)
            {
                imageNpcCard.enabled = imageNpcCard.sprite != null;
            }
        }

        private void RefreshDiceText(DialogueChoiceConfig choice, bool diceSuccess, DiceResult diceResult)
        {
            if (textDice == null)
            {
                return;
            }

            if (!choice.RequiresDice)
            {
                textDice.text = "\u672c\u9009\u9879\u65e0\u9700\u9ab0\u5b50\u5224\u5b9a\u3002";
                return;
            }

            if (diceSystem == null)
            {
                textDice.text = "\u9ab0\u5b50\u7cfb\u7edf\u7f3a\u5931\uff0c\u5df2\u6309\u901a\u8fc7\u5904\u7406\u3002";
                return;
            }

            string diceState = diceSuccess ? "\u6210\u529f" : "\u5931\u8d25";
            textDice.text = $"\u9ab0\u5b50\uff1a+{diceResult.PositiveD6} -{diceResult.NegativeD6} + {diceResult.AttributeType}({diceResult.AttributeBonus}) = {diceResult.Total} / \u96be\u5ea6 {diceResult.Difficulty}\uff0c{diceState}";
        }

        private void RefreshInteractableState()
        {
            if (buttonSettings != null)
            {
                buttonSettings.interactable = !inCall;
            }

            if (buttonOpenYellowPages != null)
            {
                buttonOpenYellowPages.interactable = !inCall;
            }

            if (buttonDial != null)
            {
                buttonDial.interactable = !inCall;
            }

            if (buttonOpenNewspaper != null)
            {
                buttonOpenNewspaper.interactable = !inCall;
            }

            if (buttonOpenSave != null)
            {
                buttonOpenSave.interactable = true;
            }

            if (buttonOpenTaskBook != null)
            {
                buttonOpenTaskBook.interactable = !inCall;
            }

            if (buttonOpenCard != null)
            {
                buttonOpenCard.interactable = !inCall;
            }

            if (buttonSmoking != null)
            {
                bool hasCigarettes = cigaretteSystem != null && cigaretteSystem.Count > 0;
                buttonSmoking.interactable = hasCigarettes;
            }
        }

        private void RefreshRuntimeIndicators()
        {
            RefreshNpcTexts();
            RefreshHud();
            RefreshCallCounterImage();
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

        private void RefreshCallCounterImage()
        {
            int count = callCounterSystem != null ? callCounterSystem.CurrentCount : 0;
            int target = callCounterSystem != null ? callCounterSystem.DelayTargetCount : 30;
            RefreshCallCounterImage(count, target);
        }

        private void RefreshCallCounterImage(int count, int target)
        {
            SetImageValue(imageCounter, count, target, $"{count}/{target}");
        }

        private void RefreshStressImage(PlayerRuntimeData playerData)
        {
            if (playerData == null)
            {
                SetPipImageValue(imageStress, 0, 1);
                return;
            }

            SetPipImageValue(imageStress, playerData.CurrentStress, playerData.MaxStress);
        }

        private void RefreshBreakdownPips()
        {
            if (npcManager == null)
            {
                SetPipImageValue(textNpcBreakdown, 0, 1);
                return;
            }

            SetPipImageValue(textNpcBreakdown, npcManager.CurrentBreakdown, npcManager.MaxBreakdown);
        }

        private void RefreshPlayerAttributeImages(PlayerRuntimeData playerData)
        {
            if (playerData == null)
            {
                SetImageValue(imagePerception, 0, 1, "?");
                SetImageValue(imageLogic, 0, 1, "?");
                SetImageValue(imagePractical, 0, 1, "?");
                SetImageValue(imageIdeal, 0, 1, "?");
                return;
            }

            int maxAttribute = Mathf.Max(1, maxVisibleAttributeValue);
            SetImageValue(imagePerception, playerData.Perception, maxAttribute, playerData.Perception.ToString());
            SetImageValue(imageLogic, playerData.Logic, maxAttribute, playerData.Logic.ToString());
            SetImageValue(imagePractical, playerData.Resilience, maxAttribute, playerData.Resilience.ToString());
            SetImageValue(imageIdeal, playerData.Insight, maxAttribute, playerData.Insight.ToString());
        }

        private static void SetPipImageValue(Component root, int current, int max)
        {
            if (root == null)
            {
                return;
            }

            Image[] images = root.GetComponentsInChildren<Image>(true);
            if (images.Length == 0)
            {
                return;
            }

            Image baseImage = null;
            Image activeTemplate = null;
            float widest = float.MinValue;
            float smallestArea = float.MaxValue;

            for (int i = 0; i < images.Length; i++)
            {
                Image candidate = images[i];
                if (candidate.transform == root.transform || candidate.name.StartsWith("RuntimePip", StringComparison.Ordinal))
                {
                    continue;
                }

                Rect rect = candidate.rectTransform.rect;
                float width = Mathf.Abs(rect.width);
                float height = Mathf.Abs(rect.height);
                float area = Mathf.Max(1f, width * height);

                if (width > widest)
                {
                    widest = width;
                    baseImage = candidate;
                }

                if (area < smallestArea)
                {
                    smallestArea = area;
                    activeTemplate = candidate;
                }
            }

            if (activeTemplate == baseImage)
            {
                activeTemplate = null;
                for (int i = 0; i < images.Length; i++)
                {
                    Image candidate = images[i];
                    if (candidate.transform == root.transform || candidate == baseImage || candidate.name.StartsWith("RuntimePip", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    activeTemplate = candidate;
                    break;
                }
            }

            if (baseImage != null)
            {
                baseImage.gameObject.SetActive(true);
            }

            if (activeTemplate == null)
            {
                return;
            }

            Transform parent = activeTemplate.transform.parent;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith("RuntimePip", StringComparison.Ordinal))
                {
                    Destroy(child.gameObject);
                }
            }

            int safeMax = Mathf.Max(1, max);
            int visibleCount = Mathf.Clamp(current, 0, safeMax);
            activeTemplate.gameObject.SetActive(visibleCount > 0);

            if (visibleCount <= 0)
            {
                return;
            }

            RectTransform templateRect = activeTemplate.rectTransform;
            Vector2 origin = templateRect.anchoredPosition;
            float[] runtimePipX = { 152f, 225f, 296f };

            for (int i = 0; i < visibleCount; i++)
            {
                Image pip = i == 0 ? activeTemplate : Instantiate(activeTemplate, parent);
                pip.name = i == 0 ? activeTemplate.name : $"RuntimePip_{i}";
                pip.gameObject.SetActive(true);

                RectTransform pipRect = pip.rectTransform;
                float x = i > 0 && i <= runtimePipX.Length ? runtimePipX[i - 1] : origin.x;
                pipRect.anchoredPosition = new Vector2(x, origin.y);
                pipRect.sizeDelta = templateRect.sizeDelta;
                pipRect.localScale = templateRect.localScale;
            }
        }
        private static void SetImageValue(Image image, int current, int max, string label)
        {
            if (image == null)
            {
                return;
            }

            int safeMax = Mathf.Max(1, max);
            image.fillAmount = Mathf.Clamp01((float)current / safeMax);

            TextMeshProUGUI text = image.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.text = label;
            }
        }

        private void ResolveGameplayReferences()
        {
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }

            if (npcManager == null)
            {
                npcManager = FindFirstObjectByType<NPCManager>();
            }

            if (diceSystem == null)
            {
                diceSystem = FindFirstObjectByType<DiceSystem>();
            }

            if (callCounterSystem == null)
            {
                callCounterSystem = FindFirstObjectByType<CallCounterSystem>();
            }

            if (endingEvaluator == null)
            {
                endingEvaluator = FindFirstObjectByType<EndingEvaluator>();
            }

            if (cigaretteSystem == null)
            {
                cigaretteSystem = FindFirstObjectByType<ObituaryTomorrow.Gameplay.Items.CigaretteSystem>();
            }
        }

        private void ResolveSceneReferences()
        {
            AssignIfMissing(ref buttonSettings, FindComponentByObjectName<Button>("Button_Settings"));
            AssignIfMissing(ref buttonOpenTaskBook, FindComponentByObjectName<Button>("Button_OpenTaskBook"));
            AssignIfMissing(ref buttonOpenCard, FindComponentByObjectName<Button>("Button_OpenCard"));
            AssignIfMissing(ref buttonOpenAchievement, FindComponentByObjectName<Button>("Button_OpenAchievement"));
            AssignIfMissing(ref buttonOpenNewspaper, FindComponentByObjectName<Button>("Button_OpenNewspaper"));
            AssignIfMissing(ref buttonOpenYellowPages, FindComponentByObjectName<Button>("Button_OpenYellowPages"));
            AssignIfMissing(ref buttonOpenSave, FindOrAddButtonByObjectName("Button_OpenSave"));
            AssignIfMissing(ref buttonDial, FindComponentByObjectName<Button>("Button_Dial"));
            AssignIfMissing(ref buttonSmoking, FindComponentByObjectName<Button>("Button_Smoking"));
            AssignIfMissing(ref buttonDiceTest, FindOrAddButtonByObjectName("DiceTest"));

            AssignIfMissing(ref panelSettings, FindGameObjectByName("Panel_Settings"));
            AssignIfMissing(ref panelPopupRoot, FindGameObjectByName("Panel_PopupRoot"));
            AssignIfMissing(ref panelSave, FindGameObjectByName("Panel_Save"));
            AssignIfMissing(ref dialogueAreaRoot, FindGameObjectByName("Panel_DialogueArea"));
            AssignIfMissing(ref imageNpcPortrait, FindComponentByObjectName<Image>("Image_NpcPortrait"));
            AssignIfMissing(ref imageNpcName, FindComponentByObjectName<Image>("NPCNameImage"));
            AssignIfMissing(ref imageNpcCard, FindComponentByObjectName<Image>("Image_NpcCard"));
            AssignIfMissing(ref imageNpcCard, FindComponentByObjectName<Image>("Image_NpcCard (1)"));
            AssignIfMissing(ref textNpcName, FindComponentByObjectName<TextMeshProUGUI>("Text_NpcName"));
            AssignIfMissing(ref textNpcBreakdown, FindComponentByObjectName<TextMeshProUGUI>("Text_NpcBreakdown"));
            AssignIfMissing(ref textNpcCard, FindComponentByObjectName<TextMeshProUGUI>("Text_NpcCard"));
            AssignIfMissing(ref textDialogue, FindComponentByObjectName<TextMeshProUGUI>("Text_Dialogue"));
            AssignIfMissing(ref textDice, FindComponentByObjectName<TextMeshProUGUI>("Text_Dice"));
            AssignIfMissing(ref groupChoiceButtons, FindComponentByObjectName<Transform>("Group_ChoiceButtons"));
            AssignIfMissing(ref textDiceResult, FindComponentByObjectName<TextMeshProUGUI>("DiceResult"));
            AssignIfMissing(ref textSaveStatus, FindComponentByObjectName<TextMeshProUGUI>("Text_SaveStatus"));
            AssignIfMissing(ref imageCounter, FindComponentByObjectName<Image>("CounterImage"));
            AssignIfMissing(ref imageStress, FindComponentByObjectName<Image>("StressImage"));
            AssignIfMissing(ref imagePerception, FindComponentByObjectName<Image>("PerceptionImage"));
            AssignIfMissing(ref imageLogic, FindComponentByObjectName<Image>("LogicImage"));
            AssignIfMissing(ref imagePractical, FindComponentByObjectName<Image>("practicalImage"));
            AssignIfMissing(ref imageIdeal, FindComponentByObjectName<Image>("IdealImage"));
            EnsureDiceFaceObjects();
            if (dialogueAreaRoot != null && callGreyboxController == null)
            {
                callGreyboxController = dialogueAreaRoot.GetComponent<CallGreyboxController>();
            }
        }

        private static GameObject FindGameObjectByName(string objectName)
        {
            Transform transform = FindTransformByName(objectName);
            return transform != null ? transform.gameObject : null;
        }

        private static Button FindOrAddButtonByObjectName(string objectName)
        {
            Transform transform = FindTransformByName(objectName);
            if (transform == null)
            {
                return null;
            }

            Button button = transform.GetComponent<Button>();
            if (button != null)
            {
                return button;
            }

            button = transform.gameObject.AddComponent<Button>();
            button.targetGraphic = transform.GetComponent<Graphic>();
            return button;
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

        private static void AssignIfMissing<T>(ref T target, T value) where T : class
        {
            if (target == null && value != null)
            {
                target = value;
            }
        }

        private void OnPlayerStressChanged(StressChangedEventArgs args)
        {
            RefreshHud();
        }

        private void OnNPCBreakdownChanged(NPCBreakdownChangedEventArgs args)
        {
            RefreshNpcTexts();
        }

        private void OnCallCounterChanged(CallCounterChangedEventArgs args)
        {
            RefreshCallCounterImage(args.NewValue, args.TargetValue);
        }

        private void OnCigaretteChanged(CigaretteChangedEventArgs args)
        {
            RefreshInteractableState();
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void AddListeners(IReadOnlyList<Button> buttons, UnityEngine.Events.UnityAction action)
        {
            if (buttons == null)
            {
                return;
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                AddListener(buttons[i], action);
            }
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private static void RemoveListeners(IReadOnlyList<Button> buttons, UnityEngine.Events.UnityAction action)
        {
            if (buttons == null)
            {
                return;
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                RemoveListener(buttons[i], action);
            }
        }

        private static void SetPanelVisible(GameObject panel, bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static string GetEndingDisplayName(EndingType endingType)
        {
            switch (endingType)
            {
                case EndingType.DeepAnalysis:
                    return "\u5f7b\u5e95\u6551\u8d4e";
                case EndingType.DelaySuccess:
                    return "\u62d6\u65f6\u6539\u5199\u65f6\u95f4\u7ebf";
                case EndingType.CallFailed:
                    return "\u529d\u544a\u5931\u8d25";
                case EndingType.PlayerBreakdown:
                    return "\u7cbe\u795e\u5931\u63a7";
                default:
                    return "\u672a\u5b9a\u7ed3\u5c40";
            }
        }

        private static string GetAchievementDisplayName(EndingType endingType)
        {
            switch (endingType)
            {
                case EndingType.DeepAnalysis:
                    return "\u6df1\u591c\u91cc\u7684\u56de\u58f0";
                case EndingType.DelaySuccess:
                    return "\u62d6\u8fc7\u96f6\u70b9";
                case EndingType.CallFailed:
                    return "\u65ad\u7ebf";
                case EndingType.PlayerBreakdown:
                    return "\u88ab\u8ba3\u544a\u541e\u6ca1";
                default:
                    return "\u65e0";
            }
        }

        [Serializable]
        private sealed class DialogueChoiceConfig
        {
            [SerializeField] private string label;
            [SerializeField] private PersonalityTag personalityTag;
            [SerializeField] private PlayerAttributeType attributeType;
            [SerializeField] private int difficulty;
            [SerializeField] private bool requiresDice;
            [SerializeField] private int npcBreakdownDeltaOnSuccess;
            [SerializeField] private int npcBreakdownDeltaOnFailure;
            [SerializeField] private bool marksDeepRescueProgress = true;
            [SerializeField] private string successText;
            [SerializeField] private string failureText;

            public string Label => label;
            public PersonalityTag PersonalityTag => personalityTag;
            public PlayerAttributeType AttributeType => attributeType;
            public int Difficulty => difficulty;
            public bool RequiresDice => requiresDice;
            public int NpcBreakdownDeltaOnSuccess => npcBreakdownDeltaOnSuccess;
            public int NpcBreakdownDeltaOnFailure => npcBreakdownDeltaOnFailure;
            public bool MarksDeepRescueProgress => marksDeepRescueProgress;
            public string SuccessText => successText;
            public string FailureText => failureText;

            public DialogueChoiceConfig()
            {
            }

            public DialogueChoiceConfig(
                string label,
                PersonalityTag personalityTag,
                PlayerAttributeType attributeType,
                int difficulty,
                bool requiresDice,
                int npcBreakdownDeltaOnSuccess,
                int npcBreakdownDeltaOnFailure,
                string successText,
                string failureText)
            {
                this.label = label;
                this.personalityTag = personalityTag;
                this.attributeType = attributeType;
                this.difficulty = difficulty;
                this.requiresDice = requiresDice;
                this.npcBreakdownDeltaOnSuccess = npcBreakdownDeltaOnSuccess;
                this.npcBreakdownDeltaOnFailure = npcBreakdownDeltaOnFailure;
                this.successText = successText;
                this.failureText = failureText;
            }

            public string GetButtonLabel()
            {
                return RequiresDice
                    ? $"{label} [{attributeType} \u96be\u5ea6 {difficulty}]"
                    : label;
            }
        }
    }
}


