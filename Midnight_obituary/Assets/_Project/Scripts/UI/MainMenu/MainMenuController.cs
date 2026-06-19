using UnityEngine;
using UnityEngine.UI;
using ObituaryTomorrow.Core;

namespace ObituaryTomorrow.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject panelCredits;

        [Header("Buttons")]
        [SerializeField] private Button buttonStartNewGame;
        [SerializeField] private Button buttonContinueGame;
        [SerializeField] private Button buttonAchievements;
        [SerializeField] private Button buttonCredits;
        [SerializeField] private Button buttonQuitGame;
        [SerializeField] private Button buttonCloseCredits;

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
        }

        private void Start()
        {
            if (buttonContinueGame != null)
            {
                buttonContinueGame.interactable = false;
            }

            CloseAllPopups();
            GameManager.Instance?.ChangeState(GameState.MainMenu);
        }

        private void StartNewGame()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing in SCN_MainMenu.");
                return;
            }

            GameManager.Instance.StartNewGame(new NewGameRequest("Player"));
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

        private static void SetPanelVisible(GameObject panel, bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
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