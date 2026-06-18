using System;
using MidnightObituary.Core;
using UnityEngine;

namespace MidnightObituary.Gameplay.Services
{
    public sealed class ResultService : IResultService
    {
        private readonly PlayerState _player;
        private readonly MissionState _mission;

        public ResultService(PlayerState player, MissionState mission = null)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _mission = mission;
        }

        public void ApplyResult(MissionResult result)
        {
            if (result.ClearStress)
            {
                _player.Stress = 0;
            }

            _player.Cigarettes = Mathf.Max(0, _player.Cigarettes + result.CigarettesDelta);
            _player.StressMax = Mathf.Max(3, _player.StressMax + result.StressMaxDelta);
            _player.Stress = Mathf.Clamp(_player.Stress, 0, _player.StressMax);

            if (_mission != null)
            {
                _mission.IsCompleted = result.EndingType != EndingType.None;
                _mission.EndingType = result.EndingType;
                _mission.ObituaryState = result.ObituaryState;
            }
        }
    }
}
