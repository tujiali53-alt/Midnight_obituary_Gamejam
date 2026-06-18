using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MidnightObituary.UI
{
    public sealed class MainRoomGreyboxView : MonoBehaviour
    {
        [SerializeField] private GameObject newspaperPanel;
        [SerializeField] private GameObject yellowPagesPanel;
        [SerializeField] private TMP_Text obituaryText;
        [SerializeField] private TMP_Text yellowPagesText;
        [SerializeField] private TMP_Text hudText;
        [SerializeField] private Button openNewspaperButton;
        [SerializeField] private Button confirmMissionButton;
        [SerializeField] private Button openYellowPagesButton;
        [SerializeField] private Button dialButton;

        public Button OpenNewspaperButton => openNewspaperButton;
        public Button ConfirmMissionButton => confirmMissionButton;
        public Button OpenYellowPagesButton => openYellowPagesButton;
        public Button DialButton => dialButton;

        public void SetHud(int stress, int stressMax, int cigarettes)
        {
            hudText.text = $"Stress: {stress}/{stressMax} | Cigarettes: {cigarettes}";
        }

        public void SetObituary(string headline, string body)
        {
            obituaryText.text = $"{headline}\n\n{body}";
        }

        public void SetYellowPages(string displayName, string phoneNumber, string address)
        {
            yellowPagesText.text = $"{displayName}\n{phoneNumber}\n{address}";
        }

        public void ShowNewspaper(bool visible)
        {
            newspaperPanel.SetActive(visible);
        }

        public void ShowYellowPages(bool visible)
        {
            yellowPagesPanel.SetActive(visible);
        }
    }
}