using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidnightObituary.Infrastructure
{
    public sealed class SceneFlowController : MonoBehaviour
    {
        public void LoadMainRoom()
        {
            // SYS_GAME_002 / SYS_RESULT_005
            SceneManager.LoadScene("SCN_MainRoom");
        }

        public void LoadCall()
        {
            // SYS_PHONE_001
            SceneManager.LoadScene("SCN_Call");
        }
    }
}