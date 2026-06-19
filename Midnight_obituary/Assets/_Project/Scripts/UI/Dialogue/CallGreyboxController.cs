using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Player;

namespace ObituaryTomorrow.UI
{
    public sealed class CallGreyboxController : MonoBehaviour
    {
        [Header("Gameplay")]
        [SerializeField] private PlayerManager playerManager;

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

        private int callCount;

        private void Awake()
        {
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
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
        }

        private void Start()
        {
            callCount = 0;

            if (textNpc != null)
            {
                string npcId = GameManager.Instance != null && GameManager.Instance.Session != null
                    ? GameManager.Instance.Session.CurrentNpcId
                    : "NPC_Lena_001";

                textNpc.text = $"{npcId} [Emotional]";
            }

            if (textDialogue != null)
            {
                textDialogue.text = "Who is this? Why are you calling so late?";
            }

            if (textResult != null)
            {
                textResult.gameObject.SetActive(false);
            }

            RefreshHud();
            BuildGreyboxChoices();
        }

        private void BuildGreyboxChoices()
        {
            if (groupChoiceButtons == null || choiceButtonPrefab == null)
            {
                Debug.LogWarning("Choice button group or prefab is missing.");
                return;
            }

            choiceButtonPrefab.gameObject.SetActive(false);

            CreateChoice("我只是想确认你是否安全。");
            CreateChoice("听起来你今晚很痛苦。");
            CreateChoice("请先别挂电话，我们可以慢慢说。");
        }

        private void CreateChoice(string label)
        {
            Button button = Instantiate(choiceButtonPrefab, groupChoiceButtons);
            button.gameObject.SetActive(true);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = label;
            }

            button.onClick.AddListener(() => SelectChoice(label));
        }

        private void SelectChoice(string label)
        {
            callCount++;

            if (textDialogue != null)
            {
                textDialogue.text = $"你选择了：{label}";
            }

            RefreshHud();

            if (callCount >= 3 && textResult != null)
            {
                textResult.gameObject.SetActive(true);
                textResult.text = "Result: Greybox call finished.";
            }
        }

        private void RefreshHud()
        {
            if (textHud == null)
            {
                return;
            }

            int currentStress = playerManager != null ? playerManager.CurrentStress : 0;
            int maxStress = playerManager != null ? playerManager.MaxStress : 5;
            int cigaretteCount = playerManager != null ? playerManager.CigaretteCount : 5;

            textHud.text = $"压力：{currentStress}/{maxStress} | 香烟：{cigaretteCount} | NPC崩溃：1/3 | 通话计数：{callCount}/30";
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