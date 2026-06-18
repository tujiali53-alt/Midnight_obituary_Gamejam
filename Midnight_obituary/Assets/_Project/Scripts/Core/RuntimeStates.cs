using System.Collections.Generic;

namespace MidnightObituary.Core
{
    public sealed class GameRuntimeState
    {
        public GameFlowState FlowState { get; set; } = GameFlowState.Boot;
        public PlayerState Player { get; set; } = new PlayerState();
        public string CurrentMissionId { get; set; } = string.Empty;
        public Dictionary<string, MissionState> Missions { get; } = new Dictionary<string, MissionState>();
    }

    public sealed class PlayerState
    {
        public PlayerStats Stats { get; set; } = PlayerStats.CreateDefault();
        public HashSet<PersonalityTag> PersonalityTags { get; } = new HashSet<PersonalityTag>();
        public int Stress { get; set; }
        public int StressMax { get; set; } = 5;
        public int Cigarettes { get; set; } = 5;

        public bool IsBrokenDown => StressMax > 0 && Stress >= StressMax;
    }

    public sealed class NpcRuntimeState
    {
        public string NpcId { get; set; } = string.Empty;
        public PersonalityTag PersonalityTag { get; set; }
        public int Breakdown { get; set; }
        public int BreakdownMax { get; set; } = 3;
        public int DelayTargetCount { get; set; } = 30;

        public bool HasHungUp => BreakdownMax > 0 && Breakdown >= BreakdownMax;
    }

    public sealed class CallSessionState
    {
        public string MissionId { get; set; } = string.Empty;
        public string NpcId { get; set; } = string.Empty;
        public string DialogueNodeId { get; set; } = string.Empty;
        public int PlayerSpeechCount { get; set; }
        public EndingType PendingEnding { get; set; } = EndingType.None;
        public bool DeepRedemptionReady { get; set; }
        public bool DelayRewriteReady { get; set; }
    }

    public sealed class MissionState
    {
        public string MissionId { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCompleted { get; set; }
        public EndingType EndingType { get; set; } = EndingType.None;
        public ObituaryState ObituaryState { get; set; } = ObituaryState.Pending;
    }
}
