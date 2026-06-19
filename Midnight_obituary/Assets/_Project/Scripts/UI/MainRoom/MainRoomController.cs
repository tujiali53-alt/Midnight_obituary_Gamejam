using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Player;

namespace ObituaryTomorrow.UI
{
    public sealed class MainRoomController : MonoBehaviour
    {
        private const string GreyboxNpcId = "NPC_Lena_001";
        private const string GreyboxDialogueId = "DIA_Lena_001";

        [Header("Gameplay")]
        [SerializeField] private PlayerManager playerManager;

        [Header("Buttons")]
        [SerializeField] private Button buttonOpenNewspaper;
        [SerializeField] private Button buttonConfirmMission;
        [SerializeField] private Button buttonOpenYellowPages;
        [SerializeField] private Button buttonDial;

        [Header("Panels")]
        [SerializeField] private GameObject panelNewspaper;
        [SerializeField] private GameObject panelYellowPages;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI textHud;
        [SerializeField] private TextMeshProUGUI textObituary;
        [SerializeField] private TextMeshProUGUI textYellowPages;

        private bool missionConfirmed;

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

            if (buttonOpenNewspaper != null)
            {
                buttonOpenNewspaper.onClick.AddListener(OpenNewspaper);
            }

            if (buttonConfirmMission != null)
            {
                buttonConfirmMission.onClick.AddListener(ConfirmMission);
            }

            if (buttonOpenYellowPages != null)
            {
                buttonOpenYellowPages.onClick.AddListener(OpenYellowPages);
            }

            if (buttonDial != null)
            {
                buttonDial.onClick.AddListener(StartCall);
            }
        }

        private void OnDisable()
        {
            GameEventBus.PlayerStressChanged -= OnPlayerStressChanged;
            GameEventBus.CigaretteChanged -= OnCigaretteChanged;

            if (buttonOpenNewspaper != null)
            {
                buttonOpenNewspaper.onClick.RemoveListener(OpenNewspaper);
            }

            if (buttonConfirmMission != null)
            {
                buttonConfirmMission.onClick.RemoveListener(ConfirmMission);
            }

            if (buttonOpenYellowPages != null)
            {
                buttonOpenYellowPages.onClick.RemoveListener(OpenYellowPages);
            }

            if (buttonDial != null)
            {
                buttonDial.onClick.RemoveListener(StartCall);
            }
        }

        private void Start()
        {
            missionConfirmed = false;

            SetPanelVisible(panelNewspaper, false);
            SetPanelVisible(panelYellowPages, false);

            if (buttonOpenYellowPages != null)
            {
                buttonOpenYellowPages.interactable = false;
            }

            if (buttonDial != null)
            {
                buttonDial.interactable = false;
            }

            RefreshTexts();
        }

        private void OpenNewspaper()
        {
            SetPanelVisible(panelNewspaper, true);
            SetPanelVisible(panelYellowPages, false);

            GameManager.Instance?.ChangeState(GameState.ObituaryView);
        }

        private void ConfirmMission()
        {
            missionConfirmed = true;

            if (buttonOpenYellowPages != null)
            {
                buttonOpenYellowPages.interactable = true;
            }

            if (buttonDial != null)
            {
                buttonDial.interactable = true;
            }
        }

        private void OpenYellowPages()
        {
            if (!missionConfirmed)
            {
                Debug.Log("Confirm the mission before opening Yellow Pages.");
                return;
            }

            SetPanelVisible(panelNewspaper, false);
            SetPanelVisible(panelYellowPages, true);

            GameManager.Instance?.ChangeState(GameState.YellowPagesView);
        }

        private void StartCall()
        {
            if (!missionConfirmed)
            {
                Debug.Log("Confirm the mission before dialing.");
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing. Start from SCN_MainRoom and make sure a GameManager exists.");
                return;
            }

            GameManager.Instance.StartCall(GreyboxNpcId, GreyboxDialogueId);
        }

        private void RefreshTexts()
        {
            RefreshHud();

            if (textObituary != null)
            {
                textObituary.text = "讣告：Lena，将于今晚 11:45 死亡。电话：555-0134";
            }

            if (textYellowPages != null)
            {
                textYellowPages.text = "Lena - 555-0134";
            }
        }

        private void RefreshHud()
        {
            if (textHud == null)
            {
                return;
            }

            if (playerManager == null)
            {
                textHud.text = "压力：?/ ? | 香烟：?";
                return;
            }

            textHud.text = $"压力：{playerManager.CurrentStress}/{playerManager.MaxStress} | 香烟：{playerManager.CigaretteCount}";
        }

        private void OnPlayerStressChanged(StressChangedEventArgs args)
        {
            RefreshHud();
        }

        private void OnCigaretteChanged(CigaretteChangedEventArgs args)
        {
            RefreshHud();
        }

        private static void SetPanelVisible(GameObject panel, bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }
    }
}