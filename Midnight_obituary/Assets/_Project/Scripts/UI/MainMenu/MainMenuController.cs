using UnityEngine;
using UnityEngine.UI;
using ObituaryTomorrow.Core;

namespace ObituaryTomorrow.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject panelMainMenu;
        [SerializeField] private GameObject panelCredits;
        [SerializeField] private MainMenuIntroAnimationController introAnimation;

        [Header("Buttons")]
        [SerializeField] private Button buttonStartNewGame;
        [SerializeField] private Button buttonContinueGame;
        [SerializeField] private Button buttonAchievements;
        [SerializeField] private Button buttonCredits;
        [SerializeField] private Button buttonQuitGame;
        [SerializeField] private Button buttonCloseCredits;

        private bool introPlaying;

        private void OnEnable()
        {
            AddButtonListener(buttonStartNewGame, StartNewGame);
            AddButtonListener(buttonContinueGame, ContinueGame);
            AddButtonListener(buttonAchievements, OpenAchievements);
            AddButtonListener(buttonCredits, OpenCredits);
            AddButtonListener(buttonQuitGame, QuitGame);
            AddButtonListener(buttonCloseCredits, CloseAllPopups);
        }

        private void OnDisable()
        {
            RemoveButtonListener(buttonStartNewGame, StartNewGame);
            RemoveButtonListener(buttonContinueGame, ContinueGame);
            RemoveButtonListener(buttonAchievements, OpenAchievements);
            RemoveButtonListener(buttonCredits, OpenCredits);
            RemoveButtonListener(buttonQuitGame, QuitGame);
            RemoveButtonListener(buttonCloseCredits, CloseAllPopups);

            if (introAnimation != null)
            {
                introAnimation.OnAnimationComplete -= OnIntroAnimationComplete;
            }
        }

        private void Start()
        {
            ResolveSceneReferences();

            if (buttonContinueGame != null)
            {
                buttonContinueGame.interactable = false;
            }

            CloseAllPopups();
            GameManager.Instance?.ChangeState(GameState.MainMenu);
        }

        private void StartNewGame()
        {
            if (introPlaying)
            {
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing in SCN_MainMenu.");
                return;
            }

            introPlaying = true;
            SetMainMenuInteractable(false);
            CloseAllPopups();
            GameManager.Instance.PrepareNewGame(new NewGameRequest("Player"));

            if (introAnimation != null)
            {
                introAnimation.OnAnimationComplete -= OnIntroAnimationComplete;
                introAnimation.OnAnimationComplete += OnIntroAnimationComplete;
                SetPanelVisible(panelMainMenu, false);
                introAnimation.Play();
                return;
            }

            GameManager.Instance.EnterMainRoom();
            introPlaying = false;
            SetMainMenuInteractable(true);
        }

        private void OnIntroAnimationComplete()
        {
            if (introAnimation != null)
            {
                introAnimation.OnAnimationComplete -= OnIntroAnimationComplete;
            }

            introPlaying = false;
            GameManager.Instance?.EnterMainRoom();
        }

        private void ContinueGame()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing in SCN_MainMenu.");
                return;
            }

            GameManager.Instance.ContinueGame();
        }

        private void OpenAchievements()
        {
            CloseAllPopups();
            Debug.Log("Achievements UI is P1 and not implemented yet.");
        }

        private void OpenCredits()
        {
            CloseAllPopups();
            SetPanelVisible(panelCredits, true);
        }

        private void QuitGame()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing in SCN_MainMenu.");
                return;
            }

            GameManager.Instance.QuitGame();
        }

        private void CloseAllPopups()
        {
            SetPanelVisible(panelCredits, false);
        }

        private void SetMainMenuInteractable(bool interactable)
        {
            SetButtonInteractable(buttonStartNewGame, interactable);
            SetButtonInteractable(buttonContinueGame, interactable);
            SetButtonInteractable(buttonAchievements, interactable);
            SetButtonInteractable(buttonCredits, interactable);
            SetButtonInteractable(buttonQuitGame, interactable);
            SetButtonInteractable(buttonCloseCredits, interactable);
        }

        private void ResolveSceneReferences()
        {
            if (panelMainMenu == null)
            {
                panelMainMenu = FindGameObjectByName("Panel_MainMenu");
            }

            if (introAnimation == null)
            {
                introAnimation = FindFirstObjectByType<MainMenuIntroAnimationController>(FindObjectsInactive.Include);
            }
        }

        private static GameObject FindGameObjectByName(string objectName)
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == objectName)
                {
                    return transforms[i].gameObject;
                }
            }

            return null;
        }

        private static void SetPanelVisible(GameObject panel, bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private static void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveButtonListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }
    }
}
