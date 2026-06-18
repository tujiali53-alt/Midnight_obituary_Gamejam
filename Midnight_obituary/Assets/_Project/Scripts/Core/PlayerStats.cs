using System;

namespace MidnightObituary.Core
{
    [Serializable]
    public struct PlayerStats
    {
        public int Perception;
        public int Logic;
        public int Insight;
        public int Resilience;

        public PlayerStats(int perception, int logic, int insight, int resilience)
        {
            Perception = perception;
            Logic = logic;
            Insight = insight;
            Resilience = resilience;
        }

        public static PlayerStats CreateDefault(int value = 4)
        {
            return new PlayerStats(value, value, value, value);
        }

        public int Get(StatType statType)
        {
            switch (statType)
            {
                case StatType.Perception:
                    return Perception;
                case StatType.Logic:
                    return Logic;
                case StatType.Insight:
                    return Insight;
                case StatType.Resilience:
                    return Resilience;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }

        public void Add(StatType statType, int delta)
        {
            switch (statType)
            {
                case StatType.Perception:
                    Perception += delta;
                    break;
                case StatType.Logic:
                    Logic += delta;
                    break;
                case StatType.Insight:
                    Insight += delta;
                    break;
                case StatType.Resilience:
                    Resilience += delta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }
    }
}
