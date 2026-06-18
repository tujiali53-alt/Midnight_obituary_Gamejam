using System;
using MidnightObituary.Core;
using MidnightObituary.Gameplay.Services;

namespace MidnightObituary.Infrastructure
{
    public sealed class GameFlowService : IGameFlowService
    {
        public event Action<GameFlowState, GameFlowState> FlowStateChanged;

        public GameFlowState CurrentState { get; private set; } = GameFlowState.Boot;
        public string CurrentMissionId { get; private set; } = string.Empty;

        public void StartNewGame()
        {
            TransitionTo(GameFlowState.Opening);
        }

        public void GoToMainRoom()
        {
            TransitionTo(GameFlowState.MainRoom);
        }

        public void StartDialing(string missionId)
        {
            CurrentMissionId = missionId ?? string.Empty;
            TransitionTo(GameFlowState.Dialing);
        }

        public void StartCall(string missionId)
        {
            CurrentMissionId = missionId ?? string.Empty;
            TransitionTo(GameFlowState.Call);
        }

        public void ShowResult(MissionResult result)
        {
            CurrentMissionId = result.MissionId ?? string.Empty;
            TransitionTo(GameFlowState.Result);
        }

        public void RestartGame()
        {
            CurrentMissionId = string.Empty;
            TransitionTo(GameFlowState.MainMenu);
        }

        private void TransitionTo(GameFlowState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            GameFlowState previous = CurrentState;
            CurrentState = nextState;
            FlowStateChanged?.Invoke(previous, CurrentState);
        }
    }
}
