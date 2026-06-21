using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Call;
using ObituaryTomorrow.Gameplay.Dice;
using ObituaryTomorrow.Gameplay.Ending;
using ObituaryTomorrow.Gameplay.NPC;
using ObituaryTomorrow.Gameplay.Player;

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
        [SerializeField] private int requiredDeepRescueSuccesses = 3;

        [Header("Desk Buttons")]
        [SerializeField] private Button buttonSettings;
        [SerializeField] private Button buttonOpenNewspaper;
        [SerializeField] private Button buttonOpenYellowPages;
        [SerializeField] private Button buttonOpenTaskBook;
        [SerializeField] private Button buttonOpenCard;
        [SerializeField] private Button buttonOpenAchievement;
        [SerializeField] private Button buttonDial;

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
        [SerializeField] private GameObject panelResult;

        [Header("HUD Texts")]
        [SerializeField] private TextMeshProUGUI textHud;
        [SerializeField] private TextMeshProUGUI textObituary;
        [SerializeField] private TextMeshProUGUI textYellowPages;
        [SerializeField] private TextMeshProUGUI textTaskBook;
        [SerializeField] private TextMeshProUGUI textAchievement;

        [Header("Dialogue Area")]
        [SerializeField] private GameObject dialogueAreaRoot;
        [SerializeField] private Image imageNpcPortrait;
        [SerializeField] private Image imageNpcCard;
        [SerializeField] private TextMeshProUGUI textNpcName;
        [SerializeField] private TextMeshProUGUI textNpcBreakdown;
        [SerializeField] private TextMeshProUGUI textNpcCard;
        [SerializeField] private TextMeshProUGUI textDialogue;
        [SerializeField] private TextMeshProUGUI textDice;
        [SerializeField] private Transform groupChoiceButtons;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Result Texts")]
        [SerializeField] private TextMeshProUGUI textResultEnding;
        [SerializeField] private TextMeshProUGUI textResultAchievement;

        [Header("Temporary Dialogue Data")]
        [SerializeField] private DialogueChoiceConfig[] dialogueChoices =
        {
            new DialogueChoiceConfig(
                "我不会逼你回答。先告诉我，你现在安全吗？",
                PersonalityTag.Emotional,
                PlayerAttributeType.Perception,
                3,
                true,
                -1,
                1,
                "电话另一端的呼吸声慢了一点，对方愿意继续听你说。",
                "你的语气没能穿过噪声，对方变得更防备。"),
            new DialogueChoiceConfig(
                "按时间线梳理一下：今晚发生了什么？",
                PersonalityTag.Rational,
                PlayerAttributeType.Logic,
                3,
                true,
                -1,
                1,
                "你抓住了矛盾处，对方开始复述今晚的经过。",
                "过于冷静的问题刺痛了对方，电话里出现长时间沉默。"),
            new DialogueChoiceConfig(
                "先做一件能立刻完成的小事：把窗户关上，坐回椅子。",
                PersonalityTag.Practical,
                PlayerAttributeType.Resilience,
                5,
                true,
                -1,
                1,
                "对方照做了。房间里的雨声被隔在窗外。",
                "现实建议来得太快，对方觉得你根本没有理解痛苦。")
        };

        private readonly List<Button> spawnedChoiceButtons = new List<Button>();
        private bool missionConfirmed;
        private bool inCall;
        private int deepRescueSuccessCount;

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

            AddListener(buttonSettings, OpenSettings);
            AddListener(buttonOpenNewspaper, OpenNewspaper);
            AddListener(buttonOpenYellowPages, OpenYellowPages);
            AddListener(buttonOpenTaskBook, OpenTaskBook);
            AddListener(buttonOpenCard, OpenCard);
            AddListener(buttonOpenAchievement, OpenAchievement);
            AddListener(buttonDial, StartCall);
            AddListener(buttonConfirmMission, ConfirmMission);
            AddListeners(buttonClosePopups, ClosePopup);
            AddListener(buttonConfirmResult, ConfirmResult);
        }

        private void OnDisable()
        {
            GameEventBus.PlayerStressChanged -= OnPlayerStressChanged;
            GameEventBus.NPCBreakdownChanged -= OnNPCBreakdownChanged;
            GameEventBus.CallCounterChanged -= OnCallCounterChanged;

            RemoveListener(buttonSettings, OpenSettings);
            RemoveListener(buttonOpenNewspaper, OpenNewspaper);
            RemoveListener(buttonOpenYellowPages, OpenYellowPages);
            RemoveListener(buttonOpenTaskBook, OpenTaskBook);
            RemoveListener(buttonOpenCard, OpenCard);
            RemoveListener(buttonOpenAchievement, OpenAchievement);
            RemoveListener(buttonDial, StartCall);
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
            if (!missionConfirmed)
            {
                ShowDialoguePrompt("先阅读报纸并确认当前任务，再查黄页。");
                return;
            }

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

        private void ConfirmMission()
        {
            missionConfirmed = true;
            GameManager.Instance?.SetCurrentMission(DefaultMissionId);
            RefreshStaticTexts();
            RefreshInteractableState();
            ClosePopup();
            ShowDialoguePrompt("任务已记录。现在可以查黄页并拨打电话。");
        }

        private void ClosePopup()
        {
            HideAllPopups();

            if (!inCall && GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.MainRoom);
            }
        }

        private void StartCall()
        {
            if (!missionConfirmed)
            {
                ShowDialoguePrompt("你还没有确认任务。先查看报纸。");
                return;
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

            SetPanelVisible(dialogueAreaRoot, true);
            RefreshNpcTexts();
            ShowDialoguePrompt("电话接通了。雨声、电流声和陌生人的呼吸一起挤进听筒。");
            BuildChoiceButtons();
            RefreshInteractableState();
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
                textResultEnding.text = $"达成结局：{GetEndingDisplayName(endingResult.Type)}";
            }

            if (textResultAchievement != null)
            {
                textResultAchievement.text = $"达成成就：{GetAchievementDisplayName(endingResult.Type)}";
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
            SetPanelVisible(panelResult, false);
        }

        private void ResetDialogueArea()
        {
            SetPanelVisible(dialogueAreaRoot, true);
            ShowDialoguePrompt("文本区：点击报纸、黄页或拨打电话开始推进。");
            SetText(textDice, string.Empty);
            RefreshNpcTexts();
        }

        private void ShowDialoguePrompt(string message)
        {
            SetText(textDialogue, message);
        }

        private void RefreshStaticTexts()
        {
            SetText(textObituary, "讣告：Lena，将于今晚 11:45 死亡。电话线索尚待核对。");
            SetText(textYellowPages, missionConfirmed ? "Lena - 555-0134" : "请先确认任务后再检索号码。");
            SetText(textTaskBook, "任务\n\n- 阅读报纸\n- 查找黄页\n- 拨打电话\n- 在通话中完成骰子判定");
            SetText(textAchievement, "成就图鉴\n\n- 彻底救赎\n- 拖时改写\n- 未能挽回\n- 精神崩溃");
        }

        private void RefreshHud()
        {
            if (textHud == null)
            {
                return;
            }

            if (playerManager == null)
            {
                textHud.text = "压力：?/?";
                return;
            }

            textHud.text = $"压力：{playerManager.CurrentStress}/{playerManager.MaxStress}";
        }

        private void RefreshNpcTexts()
        {
            string npcName = npcManager != null ? npcManager.DisplayName : "NPC";
            string breakdown = npcManager != null
                ? $"崩溃值：{npcManager.CurrentBreakdown}/{npcManager.MaxBreakdown}"
                : "崩溃值：?/?";
            string card = npcManager != null ? $"NPC卡牌：{npcManager.PersonalityTag}" : "NPC卡牌：未知";

            SetText(textNpcName, npcName);
            SetText(textNpcBreakdown, breakdown);
            SetText(textNpcCard, card);

            if (imageNpcPortrait != null)
            {
                imageNpcPortrait.enabled = imageNpcPortrait.sprite != null;
            }

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
                textDice.text = "本选项无需骰子判定。";
                return;
            }

            if (diceSystem == null)
            {
                textDice.text = "骰子系统缺失，已按通过处理。";
                return;
            }

            textDice.text = $"骰子：+{diceResult.PositiveD6} -{diceResult.NegativeD6} + {diceResult.AttributeType}({diceResult.AttributeBonus}) = {diceResult.Total} / 难度 {diceResult.Difficulty}，{(diceSuccess ? "成功" : "失败")}";
        }

        private void RefreshInteractableState()
        {
            if (buttonSettings != null)
            {
                buttonSettings.interactable = !inCall;
            }

            if (buttonOpenYellowPages != null)
            {
                buttonOpenYellowPages.interactable = missionConfirmed && !inCall;
            }

            if (buttonDial != null)
            {
                buttonDial.interactable = missionConfirmed && !inCall;
            }

            if (buttonOpenNewspaper != null)
            {
                buttonOpenNewspaper.interactable = !inCall;
            }

            if (buttonOpenTaskBook != null)
            {
                buttonOpenTaskBook.interactable = !inCall;
            }

            if (buttonOpenCard != null)
            {
                buttonOpenCard.interactable = !inCall;
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
        }

        private void ResolveSceneReferences()
        {
            AssignIfMissing(ref buttonSettings, FindComponentByObjectName<Button>("Button_Settings"));
            AssignIfMissing(ref buttonOpenTaskBook, FindComponentByObjectName<Button>("Button_OpenTaskBook"));
            AssignIfMissing(ref buttonOpenCard, FindComponentByObjectName<Button>("Button_OpenCard"));
            AssignIfMissing(ref buttonOpenAchievement, FindComponentByObjectName<Button>("Button_OpenAchievement"));

            AssignIfMissing(ref panelSettings, FindGameObjectByName("Panel_Settings"));
            AssignIfMissing(ref dialogueAreaRoot, FindGameObjectByName("Panel_DialogueArea"));
            AssignIfMissing(ref imageNpcPortrait, FindComponentByObjectName<Image>("Image_NpcPortrait"));
            AssignIfMissing(ref imageNpcPortrait, FindComponentByObjectName<Image>("Image_NpcCard"));
            AssignIfMissing(ref imageNpcCard, FindComponentByObjectName<Image>("Image_NpcCard (1)"));
            AssignIfMissing(ref textNpcName, FindComponentByObjectName<TextMeshProUGUI>("Text_NpcName"));
            AssignIfMissing(ref textNpcBreakdown, FindComponentByObjectName<TextMeshProUGUI>("Text_NpcBreakdown"));
            AssignIfMissing(ref textNpcCard, FindComponentByObjectName<TextMeshProUGUI>("Text_NpcCard"));
            AssignIfMissing(ref textDialogue, FindComponentByObjectName<TextMeshProUGUI>("Text_Dialogue"));
            AssignIfMissing(ref textDice, FindComponentByObjectName<TextMeshProUGUI>("Text_Dice"));
            AssignIfMissing(ref groupChoiceButtons, FindComponentByObjectName<Transform>("Group_ChoiceButtons"));
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
            RefreshHud();
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
                    return "彻底救赎";
                case EndingType.DelaySuccess:
                    return "拖时改写时间线";
                case EndingType.CallFailed:
                    return "劝告失败";
                case EndingType.PlayerBreakdown:
                    return "精神失控";
                default:
                    return "未定结局";
            }
        }

        private static string GetAchievementDisplayName(EndingType endingType)
        {
            switch (endingType)
            {
                case EndingType.DeepAnalysis:
                    return "深夜里的回声";
                case EndingType.DelaySuccess:
                    return "拖过零点";
                case EndingType.CallFailed:
                    return "断线";
                case EndingType.PlayerBreakdown:
                    return "被讣告吞没";
                default:
                    return "无";
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
                    ? $"{label} [{attributeType} 难度 {difficulty}]"
                    : label;
            }
        }
    }
}
