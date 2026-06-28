using System;
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
using ObituaryTomorrow.Gameplay.Player;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ObituaryTomorrow.UI
{
    public sealed class CallGreyboxController : MonoBehaviour
    {
        private const string DefaultOpeningFragmentTechnicalName = "DFr_DD2859CE";
        private const int DelayTargetCount = 30;
        private const int StressMilestone = 10;

        [Header("Gameplay")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private CallCounterSystem callCounterSystem;
        [SerializeField] private bool autoStartOnStart = true;

        [Header("Articy")]
        [SerializeField] private string openingFragmentTechnicalName = DefaultOpeningFragmentTechnicalName;
        [SerializeField] private int maxChoiceCount = 4;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI textNpc;
        [SerializeField] private TextMeshProUGUI textDialogue;
        [SerializeField] private TextMeshProUGUI textHud;
        [SerializeField] private TextMeshProUGUI textResult;

        [Header("Choices")]
        [SerializeField] private Transform groupChoiceButtons;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Buttons")]
        [SerializeField] private Button buttonReturnMainRoom;

        private readonly List<Button> spawnedChoiceButtons = new List<Button>();
        private TMP_FontAsset runtimeChineseFont;
        private DialogueFragment currentFragment;
        private bool delayReminderShown;
        private bool callInitialized;
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
            textDialogue = dialogueText != null ? dialogueText : textDialogue;
            textHud = hudText != null ? hudText : textHud;
            textResult = resultText != null ? resultText : textResult;
            groupChoiceButtons = choicesRoot != null ? choicesRoot : groupChoiceButtons;
            choiceButtonPrefab = choicePrefab != null ? choicePrefab : choiceButtonPrefab;
            buttonReturnMainRoom = returnButton != null ? returnButton : buttonReturnMainRoom;
        }

        public void BeginCall(bool resetCounter)
        {
            if (resetCounter || !callInitialized)
            {
                callCount = 0;
                delayReminderShown = false;
            }

            callInitialized = true;
            EnsureReadableChineseFont();
            Resources.Load("ArticyDatabase");

            currentFragment = FindOpeningFragment();
            SetText(textNpc, GetNpcLabel());
            SetText(textDialogue, currentFragment != null ? GetNormalizedArticyText(currentFragment) : "呼叫中......");

            if (textResult != null)
            {
                textResult.gameObject.SetActive(false);
            }

            RefreshHud();
            BuildArticyChoices();
        }

        private string GetNpcLabel()
        {
            string npcId = GameManager.Instance != null && GameManager.Instance.Session != null
                ? GameManager.Instance.Session.CurrentNpcId
                : "NPC_Lena_001";

            return NormalizeDisplayText($"{npcId} [Articy]");
        }

        private DialogueFragment FindOpeningFragment()
        {
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

            ClearChoiceButtons();
            choiceButtonPrefab.gameObject.SetActive(false);

            if (currentFragment == null)
            {
                CreateFallbackChoice("继续通话");
                return;
            }

            List<DialogueFragment> nextFragments = GetNextDialogueFragments(currentFragment);
            int visibleCount = Mathf.Min(nextFragments.Count, Mathf.Max(1, maxChoiceCount));

            for (int i = 0; i < visibleCount; i++)
            {
                DialogueFragment fragment = nextFragments[i];
                string label = GetChoiceLabel(fragment);

                if (string.IsNullOrWhiteSpace(label) && nextFragments.Count == 1)
                {
                    label = "继续通话";
                }

                CreateChoice(fragment, label);
            }

            if (visibleCount == 0)
            {
                CreateFallbackChoice("结束通话");
            }
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
            buttonRect.sizeDelta = new Vector2(520f, 64f);

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
        private void CreateChoice(DialogueFragment fragment, string label)
        {
            Button button = Instantiate(choiceButtonPrefab, groupChoiceButtons);
            button.gameObject.SetActive(true);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                ApplyRuntimeFont(buttonText);
                buttonText.text = FitChoiceLabel(label);
            }

            button.onClick.AddListener(() => SelectChoice(fragment, label));
            spawnedChoiceButtons.Add(button);
        }

        private void CreateFallbackChoice(string label)
        {
            Button button = Instantiate(choiceButtonPrefab, groupChoiceButtons);
            button.gameObject.SetActive(true);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                ApplyRuntimeFont(buttonText);
                buttonText.text = label;
            }

            button.onClick.AddListener(() => SelectFallbackChoice(label));
            spawnedChoiceButtons.Add(button);
        }

        private void SelectChoice(DialogueFragment fragment, string label)
        {
            RegisterPlayerSpeech(label);
            currentFragment = fragment;

            SetText(textDialogue, GetNormalizedArticyText(fragment));
            RefreshHud();
            BuildArticyChoices();
            CheckGreyboxResult();
        }

        private void SelectFallbackChoice(string label)
        {
            RegisterPlayerSpeech(label);
            SetText(textDialogue, label);
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
            string message = $"已达到 {GetDelayTargetCount()} 次通话计数，拖延阈值已达成。";

            if (textResult != null)
            {
                textResult.gameObject.SetActive(true);
                textResult.text = message;
                return;
            }

            if (textDialogue != null)
            {
                textDialogue.text = $"{textDialogue.text}\n\n{message}";
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

            textHud.text = $"压力：{currentStress}/{maxStress} | 香烟：{cigaretteCount} | 通话计数：{callCount}/{GetDelayTargetCount()}";
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

            const string probeText = "呼叫中继续通话结束压力香烟计数你仍旧坐在那把陈旧的扶手椅之中城市夜色铜绞线网络";

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
            const string probeText = "呼叫中继续通话结束压力香烟计数你仍旧坐在那把陈旧的扶手椅之中城市夜色铜绞线网络";
            string[] assetPaths =
            {
                "Assets/_Project/Art/Fonts/WenQuanyi Micro Hei.ttf",
                "Assets/_Project/Art/Fonts/迫真打字油印體.ttf"
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

        private static List<DialogueFragment> GetNextDialogueFragments(DialogueFragment fragment)
        {
            List<DialogueFragment> fragments = new List<DialogueFragment>();

            if (fragment == null || fragment.OutputPins == null)
            {
                return fragments;
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

                    AddPlayableTargets(connection.Target, fragments, new HashSet<ulong>());
                }
            }

            return fragments;
        }

        private static void AddPlayableTargets(ArticyObject target, List<DialogueFragment> fragments, HashSet<ulong> visitedIds)
        {
            if (target == null || visitedIds.Contains(target.Id))
            {
                return;
            }

            visitedIds.Add(target.Id);

            if (target is DialogueFragment targetFragment)
            {
                fragments.Add(targetFragment);
                return;
            }

            if (target is Dialogue targetDialogue)
            {
                AddFirstChildDialogueFragment(targetDialogue, fragments);
                return;
            }

            foreach (DialogueFragment childFragment in ArticyDatabase.GetAllOfType<DialogueFragment>())
            {
                if (childFragment != null && childFragment.ParentId == target.Id)
                {
                    fragments.Add(childFragment);
                    return;
                }
            }
        }

        private static void AddFirstChildDialogueFragment(Dialogue dialogue, List<DialogueFragment> fragments)
        {
            DialogueFragment firstChild = null;

            foreach (DialogueFragment fragment in ArticyDatabase.GetAllOfType<DialogueFragment>())
            {
                if (fragment == null || fragment.ParentId != dialogue.Id)
                {
                    continue;
                }

                if (firstChild == null || fragment.Id < firstChild.Id)
                {
                    firstChild = fragment;
                }
            }

            if (firstChild != null)
            {
                fragments.Add(firstChild);
            }
        }
        private static string GetChoiceLabel(DialogueFragment fragment)
        {
            if (fragment == null)
            {
                return string.Empty;
            }

            string label = NormalizeDisplayText(fragment.MenuText);

            if (!string.IsNullOrWhiteSpace(label))
            {
                return label;
            }

            return FitChoiceLabel(GetArticyText(fragment));
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
                if (character == '�' || character == '□')
                {
                    score += 3;
                }
                else if ("氓盲莽脙脗鍛煎彨涓閫氳瘽璁℃暟棣欑儫鍘嬪姏".IndexOf(character) >= 0)
                {
                    score++;
                }
            }

            return score;
        }

        private static bool LooksLikeMojibake(string text)
        {
            return text.IndexOf('氓') >= 0
                || text.IndexOf('盲') >= 0
                || text.IndexOf('莽') >= 0
                || text.IndexOf('脙') >= 0
                || text.IndexOf('脗') >= 0
                || text.IndexOf('鍛') >= 0
                || text.IndexOf('煎') >= 0
                || text.IndexOf('彨') >= 0
                || text.IndexOf('閫') >= 0
                || text.IndexOf('瘽') >= 0
                || text.IndexOf('□') >= 0;
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
