using UnityEngine;
using ObituaryTomorrow.Core;

namespace ObituaryTomorrow.Gameplay.NPC
{
    public sealed class NPCManager : MonoBehaviour
    {
        [Header("Default NPC")]
        [SerializeField] private string defaultNpcId = "NPC_Lena_001";
        [SerializeField] private string defaultDisplayName = "Lena";
        [SerializeField] private PersonalityTag defaultPersonalityTag = PersonalityTag.Emotional;
        [SerializeField] private int defaultMaxBreakdown = 3;
        [SerializeField] private int defaultStartingBreakdown = 1;

        public string CurrentNpcId { get; private set; }
        public string DisplayName { get; private set; }
        public PersonalityTag PersonalityTag { get; private set; }
        public int CurrentBreakdown { get; private set; }
        public int MaxBreakdown { get; private set; }

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(CurrentNpcId))
            {
                BeginCall(defaultNpcId);
            }
        }

        public void BeginCall(string npcId)
        {
            CurrentNpcId = string.IsNullOrWhiteSpace(npcId) ? defaultNpcId : npcId;
            DisplayName = defaultDisplayName;
            PersonalityTag = defaultPersonalityTag;
            MaxBreakdown = Mathf.Max(1, defaultMaxBreakdown);
            CurrentBreakdown = Mathf.Clamp(defaultStartingBreakdown, 0, MaxBreakdown);

            GameEventBus.RaiseNPCBreakdownChanged(
                new NPCBreakdownChangedEventArgs(CurrentNpcId, CurrentBreakdown, CurrentBreakdown, MaxBreakdown, StatChangeReason.Debug));
        }

        public StatChangeResult RequestBreakdownChange(NPCBreakdownChangeRequest request)
        {
            if (string.IsNullOrWhiteSpace(CurrentNpcId))
            {
                BeginCall(request.NpcId);
            }

            int oldValue = CurrentBreakdown;
            int newValue = Mathf.Clamp(oldValue + request.Delta, 0, MaxBreakdown);
            CurrentBreakdown = newValue;

            bool applied = oldValue != newValue;

            if (applied)
            {
                GameEventBus.RaiseNPCBreakdownChanged(
                    new NPCBreakdownChangedEventArgs(CurrentNpcId, oldValue, newValue, MaxBreakdown, request.Reason));
            }

            return new StatChangeResult(
                applied,
                oldValue,
                newValue,
                newValue - oldValue,
                newValue <= 0,
                newValue >= MaxBreakdown,
                request.Reason);
        }

        public bool IsBreakdownMaxed()
        {
            return CurrentBreakdown >= MaxBreakdown;
        }

        public EndingResult CreateCallFailedEnding(string missionId)
        {
            return new EndingResult(
                EndingType.CallFailed,
                missionId,
                CurrentNpcId,
                true,
                false);
        }
    }

    public readonly struct NPCBreakdownChangeRequest
    {
        public string NpcId { get; }
        public int Delta { get; }
        public StatChangeReason Reason { get; }
        public string SourceId { get; }

        public NPCBreakdownChangeRequest(string npcId, int delta, StatChangeReason reason, string sourceId)
        {
            NpcId = npcId;
            Delta = delta;
            Reason = reason;
            SourceId = sourceId;
        }
    }
}
