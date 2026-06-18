namespace MidnightObituary.Core
{
    public enum PersonalityTag
    {
        Feeling,
        Rational,
        Idealist,
        Pragmatic
    }

    public enum StatType
    {
        Perception,
        Logic,
        Insight,
        Resilience
    }

    public enum EndingType
    {
        None,
        DeepRedemption,
        DelayRewrite,
        CallFailed,
        PlayerBreakdown
    }

    public enum ObituaryState
    {
        Pending,
        Faded,
        Rewritten,
        Darkened,
        Removed
    }

    public enum GameFlowState
    {
        Boot,
        MainMenu,
        Opening,
        MainRoom,
        Newspaper,
        YellowPages,
        Dialing,
        Call,
        Result,
        Ending
    }

    public enum StressChangeReason
    {
        Unknown,
        PersonalityMismatch,
        LongCallPenalty,
        Cigarette,
        MissionResult,
        Debug
    }
}
