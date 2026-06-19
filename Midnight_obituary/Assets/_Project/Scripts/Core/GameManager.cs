using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObituaryTomorrow.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scenes")]
        [SerializeField] private string mainRoomSceneName = "SCN_MainRoom";
        [SerializeField] private string callSceneName = "SCN_Call";
        [SerializeField] private string mainMenuSceneName = "SCN_MainMenu";

        public GameState CurrentState { get; private set; }
        public GameSessionData Session { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeGame();
        }

        public void InitializeGame()
        {
            if (Session == null)
            {
                Session = new GameSessionData();
            }

            ChangeState(GameState.Boot);
        }

        public void StartNewGame(NewGameRequest request)
        {
            Session = new GameSessionData();
            Session.Player.SetPersonalityTags(new[]
            {
                PersonalityTag.Emotional,
                PersonalityTag.Practical
            });

            ChangeState(GameState.Opening);
            EnterMainRoom();
        }

        public void ContinueGame()
        {
            if (Session == null)
            {
                Debug.LogWarning("No existing session. Starting a new game instead.");
                StartNewGame(new NewGameRequest("Player"));
                return;
            }

            EnterMainRoom();
        }

        public void ChangeState(GameState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            GameState previousState = CurrentState;
            CurrentState = nextState;

            GameEventBus.RaiseStateChanged(new GameStateChangedEventArgs(previousState, CurrentState));
        }

        public void EnterMainRoom()
        {
            ChangeState(GameState.MainRoom);
            LoadScene(mainRoomSceneName);
        }

        public void StartCall(string npcId, string dialogueId)
        {
            EnsureSession();

            Session.CurrentNpcId = npcId;
            ChangeState(GameState.Dialing);

            LoadScene(callSceneName);
            ChangeState(GameState.InCall);
        }

        public void FinishCall(EndingResult endingResult)
        {
            GameEventBus.RaiseEndingTriggered(endingResult);

            if (endingResult.ShouldEndGame)
            {
                ChangeState(GameState.GameOver);
                return;
            }

            ChangeState(GameState.Result);
        }

        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);

            if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
            {
                LoadScene(mainMenuSceneName);
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnsureSession()
        {
            if (Session == null)
            {
                Session = new GameSessionData();
            }
        }

        private static void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("Scene name is empty.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}