using UnityEngine;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Call;
using ObituaryTomorrow.Gameplay.NPC;
using ObituaryTomorrow.Gameplay.Player;

namespace ObituaryTomorrow.Gameplay.Ending
{
    public sealed class EndingEvaluator : MonoBehaviour
    {
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private NPCManager npcManager;
        [SerializeField] private CallCounterSystem callCounterSystem;

        private void Awake()
        {
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }

            if (npcManager == null)
            {
                npcManager = FindFirstObjectByType<NPCManager>();
            }

            if (callCounterSystem == null)
            {
                callCounterSystem = FindFirstObjectByType<CallCounterSystem>();
            }
        }

        public EndingResult EvaluateCallState(string missionId, bool deepRescueAchieved)
        {
            string npcId = npcManager != null ? npcManager.CurrentNpcId : string.Empty;

            if (playerManager != null && playerManager.IsStressMaxed())
            {
                return new EndingResult(
                    EndingType.PlayerBreakdown,
                    missionId,
                    npcId,
                    true,
                    true);
            }

            if (npcManager != null && npcManager.IsBreakdownMaxed())
            {
                return npcManager.CreateCallFailedEnding(missionId);
            }

            if (deepRescueAchieved)
            {
                return new EndingResult(
                    EndingType.DeepAnalysis,
                    missionId,
                    npcId,
                    true,
                    false);
            }

            if (callCounterSystem != null && callCounterSystem.HasReachedDelayTarget())
            {
                return new EndingResult(
                    EndingType.DelaySuccess,
                    missionId,
                    npcId,
                    true,
                    false);
            }

            return EndingResult.None();
        }
    }
}
