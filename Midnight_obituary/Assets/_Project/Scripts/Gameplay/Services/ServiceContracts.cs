using System;
using System.Collections.Generic;
using MidnightObituary.Core;
using MidnightObituary.Gameplay.Definitions;

namespace MidnightObituary.Gameplay.Services
{
    public interface IRandomProvider
    {
        int RangeInclusive(int minInclusive, int maxInclusive);
    }

    public interface IGameFlowService
    {
        event Action<GameFlowState, GameFlowState> FlowStateChanged;
        GameFlowState CurrentState { get; }
        void StartNewGame();
        void GoToMainRoom();
        void StartDialing(string missionId);
        void StartCall(string missionId);
        void ShowResult(MissionResult result);
        void RestartGame();
    }

    public interface IPlayerService
    {
        event Action<int, int, StressChangeReason> StressChanged;
        event Action PlayerBreakdownTriggered;
        PlayerState Player { get; }
        PlayerState InitializeNewPlayer(PlayerInitialConfig config);
        void ApplyPersonalityCards(IReadOnlyList<PersonalityDefinition> personalities);
        void ChangeStress(int delta, StressChangeReason reason);
        bool CanUseCigarette();
        UseItemResult UseCigarette();
    }

    public interface IPersonalityRuleService
    {
        TagMatchResult CheckPlayerTagMatch(PlayerState player, DialogueChoiceDefinition choice);
        TagMatchResult CheckNpcTagMatch(NpcRuntimeState npc, DialogueChoiceDefinition choice);
        PersonalityRuleResolution ResolveChoice(PlayerState player, NpcRuntimeState npc, DialogueChoiceDefinition choice);
    }

    public interface IDiceService
    {
        DiceRollResult Roll(DiceCheckDefinition check, PlayerStats stats);
        bool CheckResult(DiceRollResult result, int difficulty);
    }

    public interface ICallCounterService
    {
        event Action<int, int> CountChanged;
        event Action DelayEndingReached;
        void Initialize(NpcDefinition npc, CallCounterConfig config);
        void AddPlayerSpeech();
        bool HasReachedDelayTarget();
        int ConsumePendingStressPenalty();
        int CurrentCount { get; }
        int DelayTarget { get; }
    }

    public interface IEndingService
    {
        EndingType EvaluateEnding(CallSessionState session, PlayerState player, NpcRuntimeState npc);
        MissionResult BuildMissionResult(EndingType endingType, string missionId);
    }

    public interface IResultService
    {
        void ApplyResult(MissionResult result);
    }

    public readonly struct TagMatchResult
    {
        public TagMatchResult(bool isMatch, PersonalityTag matchedTag)
        {
            IsMatch = isMatch;
            MatchedTag = matchedTag;
        }

        public bool IsMatch { get; }
        public PersonalityTag MatchedTag { get; }
    }

    public readonly struct PersonalityRuleResolution
    {
        public PersonalityRuleResolution(TagMatchResult playerMatch, TagMatchResult npcMatch, int stressDelta, int npcBreakdownDelta)
        {
            PlayerMatch = playerMatch;
            NpcMatch = npcMatch;
            StressDelta = stressDelta;
            NpcBreakdownDelta = npcBreakdownDelta;
        }

        public TagMatchResult PlayerMatch { get; }
        public TagMatchResult NpcMatch { get; }
        public int StressDelta { get; }
        public int NpcBreakdownDelta { get; }
    }

    public readonly struct DiceRollResult
    {
        public DiceRollResult(int positiveDie, int negativeDie, int statBonus, int extraBonus, int difficulty)
        {
            PositiveDie = positiveDie;
            NegativeDie = negativeDie;
            StatBonus = statBonus;
            ExtraBonus = extraBonus;
            Difficulty = difficulty;
            Total = positiveDie - negativeDie + statBonus + extraBonus;
        }

        public int PositiveDie { get; }
        public int NegativeDie { get; }
        public int StatBonus { get; }
        public int ExtraBonus { get; }
        public int Difficulty { get; }
        public int Total { get; }
        public bool IsSuccess => Total >= Difficulty;
    }

    public readonly struct UseItemResult
    {
        public UseItemResult(bool success, int stressDelta, int cigarettesDelta)
        {
            Success = success;
            StressDelta = stressDelta;
            CigarettesDelta = cigarettesDelta;
        }

        public bool Success { get; }
        public int StressDelta { get; }
        public int CigarettesDelta { get; }
    }

    public readonly struct MissionResult
    {
        public MissionResult(string missionId, EndingType endingType, bool clearStress, int cigarettesDelta, int stressMaxDelta, ObituaryState obituaryState, bool isGameOver)
        {
            MissionId = missionId;
            EndingType = endingType;
            ClearStress = clearStress;
            CigarettesDelta = cigarettesDelta;
            StressMaxDelta = stressMaxDelta;
            ObituaryState = obituaryState;
            IsGameOver = isGameOver;
        }

        public string MissionId { get; }
        public EndingType EndingType { get; }
        public bool ClearStress { get; }
        public int CigarettesDelta { get; }
        public int StressMaxDelta { get; }
        public ObituaryState ObituaryState { get; }
        public bool IsGameOver { get; }
    }
}
