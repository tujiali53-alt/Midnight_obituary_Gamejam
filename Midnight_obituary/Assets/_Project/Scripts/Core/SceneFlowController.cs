using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObituaryTomorrow.Core
{
    public sealed class SceneFlowController : MonoBehaviour
    {
        [SerializeField] private string mainRoomSceneName = "SCN_MainRoom";
        [SerializeField] private string callSceneName = "SCN_Call";

        public void LoadMainRoom()
        {
            LoadScene(mainRoomSceneName);
        }

        public void LoadCallScene()
        {
            LoadScene(callSceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LoadScene(string sceneName)
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