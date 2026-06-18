using System;
using MidnightObituary.Gameplay.Definitions;
using UnityEngine;

namespace MidnightObituary.Gameplay.Services
{
    public sealed class CallCounterService : ICallCounterService
    {
        private CallCounterConfig _config = new CallCounterConfig();
        private int _pendingStressPenalty;

        public event Action<int, int> CountChanged;
        public event Action DelayEndingReached;

        public int CurrentCount { get; private set; }
        public int DelayTarget { get; private set; }

        public void Initialize(NpcDefinition npc, CallCounterConfig config)
        {
            _config = config ?? new CallCounterConfig();
            DelayTarget = npc != null && npc.DelayTargetCount > 0 ? npc.DelayTargetCount : _config.DefaultDelayTarget;
            CurrentCount = 0;
            _pendingStressPenalty = 0;
        }

        public void AddPlayerSpeech()
        {
            CurrentCount += 1;
            CountChanged?.Invoke(CurrentCount, DelayTarget);

            if (HasReachedDelayTarget())
            {
                DelayEndingReached?.Invoke();
            }

            if (_config.LongCallStartsAfter >= 0 &&
                _config.LongCallStressInterval > 0 &&
                CurrentCount > _config.LongCallStartsAfter &&
                (CurrentCount - _config.LongCallStartsAfter) % _config.LongCallStressInterval == 0)
            {
                _pendingStressPenalty += Mathf.Max(0, _config.LongCallStressDelta);
            }
        }

        public bool HasReachedDelayTarget()
        {
            return DelayTarget > 0 && CurrentCount >= DelayTarget;
        }

        public int ConsumePendingStressPenalty()
        {
            int penalty = _pendingStressPenalty;
            _pendingStressPenalty = 0;
            return penalty;
        }
    }
}
