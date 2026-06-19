using System;

namespace ObituaryTomorrow.Core
{
    public enum GameState
    {
        Boot,
        MainMenu,
        Opening,
        MainRoom,
        ObituaryView,
        YellowPagesView,
        Dialing,
        InCall,
        Result,
        GameOver
    }

    public enum PersonalityTag
    {
        Emotional,
        Rational,
        Idealistic,
        Practical
    }

    public enum PlayerAttributeType
    {
        Perception,
        Logic,
        Insight,
        Resilience
    }

    public enum EndingType
    {
        None,
        DeepAnalysis,
        DelaySuccess,
        CallFailed,
        PlayerBreakdown
    }

    public enum StatChangeReason
    {
        DialogueChoice,
        DiceResult,
        CallCounterMilestone,
        CigaretteUse,
        ResultReward,
        ResultPenalty,
        Debug
    }

    public enum ObituaryState
    {
        Active,
        Faded,
        Removed,
        Darkened
    }

    public readonly struct NewGameRequest
    {
        public string PlayerName { get; }

        public NewGameRequest(string playerName)
        {
            PlayerName = playerName;
        }
    }

    public readonly struct OperationResult
    {
        public bool Success { get; }
        public string Message { get; }

        public OperationResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static OperationResult Ok(string message = "")
        {
            return new OperationResult(true, message);
        }

        public static OperationResult Fail(string message)
        {
            return new OperationResult(false, message);
        }
    }

    public readonly struct StatChangeResult
    {
        public bool Applied { get; }
        public int OldValue { get; }
        public int NewValue { get; }
        public int Delta { get; }
        public bool ReachedMin { get; }
        public bool ReachedMax { get; }
        public StatChangeReason Reason { get; }

        public StatChangeResult(
            bool applied,
            int oldValue,
            int newValue,
            int delta,
            bool reachedMin,
            bool reachedMax,
            StatChangeReason reason)
        {
            Applied = applied;
            OldValue = oldValue;
            NewValue = newValue;
            Delta = delta;
            ReachedMin = reachedMin;
            ReachedMax = reachedMax;
            Reason = reason;
        }
    }

    public readonly struct EndingResult
    {
        public EndingType Type { get; }
        public string MissionId { get; }
        public string NpcId { get; }
        public bool ShouldEndCall { get; }
        public bool ShouldEndGame { get; }

        public EndingResult(
            EndingType type,
            string missionId,
            string npcId,
            bool shouldEndCall,
            bool shouldEndGame)
        {
            Type = type;
            MissionId = missionId;
            NpcId = npcId;
            ShouldEndCall = shouldEndCall;
            ShouldEndGame = shouldEndGame;
        }

        public static EndingResult None()
        {
            return new EndingResult(EndingType.None, string.Empty, string.Empty, false, false);
        }
    }
}