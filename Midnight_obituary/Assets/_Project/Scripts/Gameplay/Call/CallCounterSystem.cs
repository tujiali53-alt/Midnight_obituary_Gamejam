using UnityEngine;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Player;

namespace ObituaryTomorrow.Gameplay.Call
{
    public sealed class CallCounterSystem : MonoBehaviour
    {
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private int delayTargetCount = 30;
        [SerializeField] private int stressMilestone = 10;

        public string CurrentNpcId { get; private set; }
        public int CurrentCount { get; private set; }
        public int DelayTargetCount => Mathf.Max(1, delayTargetCount);

        private void Awake()
        {
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }
        }

        public void BeginCall(string npcId)
        {
            CurrentNpcId = npcId;
            CurrentCount = 0;
            GameEventBus.RaiseCallCounterChanged(
                new CallCounterChangedEventArgs(CurrentNpcId, CurrentCount, CurrentCount, DelayTargetCount));
        }

        public int RegisterPlayerLine(string sourceId)
        {
            int oldValue = CurrentCount;
            CurrentCount++;

            GameEventBus.RaiseCallCounterChanged(
                new CallCounterChangedEventArgs(CurrentNpcId, oldValue, CurrentCount, DelayTargetCount));

            if (stressMilestone > 0 && CurrentCount % stressMilestone == 0 && playerManager != null)
            {
                playerManager.RequestStressChange(new StressChangeRequest(
                    1,
                    StatChangeReason.CallCounterMilestone,
                    sourceId,
                    true));
            }

            return CurrentCount;
        }

        public bool HasReachedDelayTarget()
        {
            return CurrentCount >= DelayTargetCount;
        }
    }
}
