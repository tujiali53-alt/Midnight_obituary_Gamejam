using System;

namespace ObituaryTomorrow.Core
{
    public static class GameEventBus
    {
        public static event Action<GameStateChangedEventArgs> StateChanged;
        public static event Action<StressChangedEventArgs> PlayerStressChanged;
        public static event Action<CigaretteChangedEventArgs> CigaretteChanged;
        public static event Action<NPCBreakdownChangedEventArgs> NPCBreakdownChanged;
        public static event Action<CallCounterChangedEventArgs> CallCounterChanged;
        public static event Action<DiceRolledEventArgs> DiceRolled;
        public static event Action<EndingResult> EndingTriggered;

        public static void RaiseStateChanged(GameStateChangedEventArgs args)
        {
            StateChanged?.Invoke(args);
        }

        public static void RaisePlayerStressChanged(StressChangedEventArgs args)
        {
            PlayerStressChanged?.Invoke(args);
        }

        public static void RaiseCigaretteChanged(CigaretteChangedEventArgs args)
        {
            CigaretteChanged?.Invoke(args);
        }

        public static void RaiseNPCBreakdownChanged(NPCBreakdownChangedEventArgs args)
        {
            NPCBreakdownChanged?.Invoke(args);
        }

        public static void RaiseCallCounterChanged(CallCounterChangedEventArgs args)
        {
            CallCounterChanged?.Invoke(args);
        }

        public static void RaiseDiceRolled(DiceRolledEventArgs args)
        {
            DiceRolled?.Invoke(args);
        }

        public static void RaiseEndingTriggered(EndingResult result)
        {
            EndingTriggered?.Invoke(result);
        }
    }

    public readonly struct GameStateChangedEventArgs
    {
        public GameState PreviousState { get; }
        public GameState CurrentState { get; }

        public GameStateChangedEventArgs(GameState previousState, GameState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    public readonly struct StressChangedEventArgs
    {
        public int OldValue { get; }
        public int NewValue { get; }
        public int MaxValue { get; }
        public StatChangeReason Reason { get; }

        public StressChangedEventArgs(int oldValue, int newValue, int maxValue, StatChangeReason reason)
        {
            OldValue = oldValue;
            NewValue = newValue;
            MaxValue = maxValue;
            Reason = reason;
        }
    }

    public readonly struct CigaretteChangedEventArgs
    {
        public int OldValue { get; }
        public int NewValue { get; }
        public int MaxValue { get; }
        public StatChangeReason Reason { get; }

        public CigaretteChangedEventArgs(int oldValue, int newValue, int maxValue, StatChangeReason reason)
        {
            OldValue = oldValue;
            NewValue = newValue;
            MaxValue = maxValue;
            Reason = reason;
        }
    }

    public readonly struct NPCBreakdownChangedEventArgs
    {
        public string NpcId { get; }
        public int OldValue { get; }
        public int NewValue { get; }
        public int MaxValue { get; }
        public StatChangeReason Reason { get; }

        public NPCBreakdownChangedEventArgs(
            string npcId,
            int oldValue,
            int newValue,
            int maxValue,
            StatChangeReason reason)
        {
            NpcId = npcId;
            OldValue = oldValue;
            NewValue = newValue;
            MaxValue = maxValue;
            Reason = reason;
        }
    }

    public readonly struct CallCounterChangedEventArgs
    {
        public string NpcId { get; }
        public int OldValue { get; }
        public int NewValue { get; }
        public int TargetValue { get; }

        public CallCounterChangedEventArgs(string npcId, int oldValue, int newValue, int targetValue)
        {
            NpcId = npcId;
            OldValue = oldValue;
            NewValue = newValue;
            TargetValue = targetValue;
        }
    }

    public readonly struct DiceRolledEventArgs
    {
        public string CheckId { get; }
        public int PositiveD6 { get; }
        public int NegativeD6 { get; }
        public int Total { get; }
        public bool Success { get; }

        public DiceRolledEventArgs(
            string checkId,
            int positiveD6,
            int negativeD6,
            int total,
            bool success)
        {
            CheckId = checkId;
            PositiveD6 = positiveD6;
            NegativeD6 = negativeD6;
            Total = total;
            Success = success;
        }
    }
}