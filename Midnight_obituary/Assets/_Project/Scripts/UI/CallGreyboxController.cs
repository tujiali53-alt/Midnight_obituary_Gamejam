using MidnightObituary.Core;
using MidnightObituary.Gameplay.Services;
using MidnightObituary.Infrastructure;
using UnityEngine;

namespace MidnightObituary.UI
{
    public sealed class CallGreyboxController : MonoBehaviour
    {
        [SerializeField] private CallGreyboxView view;
        [SerializeField] private SceneFlowController sceneFlowController;

        private GreyboxDialogueService _dialogueService;

        private GameBootstrap Bootstrap => GameBootstrap.Instance;

        private void Start()
        {
            _dialogueService = new GreyboxDialogueService(
                Bootstrap.Database.DialogueTree,
                Bootstrap.PlayerService.Player,
                Bootstrap.CurrentNpcState,
                Bootstrap.CurrentCallSession,
                Bootstrap.PersonalityRuleService,
                Bootstrap.DiceService,
                Bootstrap.CallCounterService,
                Bootstrap.EndingService);

            view.ReturnButton.onClick.AddListener(ReturnMainRoom);

            view.SetNpc(
                Bootstrap.Database.Npc.DisplayName,
                Bootstrap.Database.Npc.PersonalityTag.ToString());

            Refresh();
        }

        private void Refresh()
        {
            var player = Bootstrap.PlayerService.Player;
            var npc = Bootstrap.CurrentNpcState;

            view.SetHud(
                player.Stress,
                player.StressMax,
                player.Cigarettes,
                npc.Breakdown,
                npc.BreakdownMax,
                Bootstrap.CallCounterService.CurrentCount,
                Bootstrap.CallCounterService.DelayTarget);

            view.ShowNode(_dialogueService.GetCurrentNode(), SelectChoice);
        }

        private void SelectChoice(string choiceId)
        {
            EndingType ending = _dialogueService.SelectChoice(choiceId);

            if (ending == EndingType.None)
            {
                Refresh();
                return;
            }

            var result = Bootstrap.EndingService.BuildMissionResult(
                ending,
                Bootstrap.CurrentCallSession.MissionId);

            new ResultService(
                Bootstrap.PlayerService.Player,
                Bootstrap.CurrentMissionState).ApplyResult(result);

            view.ShowResult($"Result: {ending}\nObituary: {result.ObituaryState}");
            Refresh();
        }

        private void ReturnMainRoom()
        {
            // SYS_RESULT_005
            sceneFlowController.LoadMainRoom();
        }
    }
}