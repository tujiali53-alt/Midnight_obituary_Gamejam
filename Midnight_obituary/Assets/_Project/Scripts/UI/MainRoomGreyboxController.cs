using MidnightObituary.Infrastructure;
using UnityEngine;

namespace MidnightObituary.UI
{
    public sealed class MainRoomGreyboxController : MonoBehaviour
    {
        [SerializeField] private MainRoomGreyboxView view;
        [SerializeField] private SceneFlowController sceneFlowController;

        private GameBootstrap Bootstrap => GameBootstrap.Instance;

        private void Start()
        {
            Refresh();

            view.OpenNewspaperButton.onClick.AddListener(OpenNewspaper);
            view.ConfirmMissionButton.onClick.AddListener(ConfirmMission);
            view.OpenYellowPagesButton.onClick.AddListener(OpenYellowPages);
            view.DialButton.onClick.AddListener(StartCall);
        }

        private void Refresh()
        {
            var player = Bootstrap.PlayerService.Player;
            view.SetHud(player.Stress, player.StressMax, player.Cigarettes);

            view.SetObituary(
                Bootstrap.Database.Obituary.Headline,
                Bootstrap.Database.Obituary.Body);

            view.SetYellowPages(
                Bootstrap.Database.YellowPageEntry.DisplayName,
                Bootstrap.Database.YellowPageEntry.PhoneNumber,
                Bootstrap.Database.YellowPageEntry.Address);

            view.ShowNewspaper(false);
            view.ShowYellowPages(false);
        }

        private void OpenNewspaper()
        {
            // SYS_OBIT_002
            view.ShowNewspaper(true);
        }

        private void ConfirmMission()
        {
            // SYS_MISSION_003
            Bootstrap.PublishAndConfirmMission();
            view.ShowNewspaper(false);
        }

        private void OpenYellowPages()
        {
            // SYS_CALL_001
            view.ShowYellowPages(true);
        }

        private void StartCall()
        {
            // SYS_PHONE_001
            Bootstrap.StartCallSession();
            sceneFlowController.LoadCall();
        }
    }
}