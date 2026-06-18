using System;
using MidnightObituary.Core;
using MidnightObituary.Gameplay.Definitions;

namespace MidnightObituary.Gameplay.Services
{
    public sealed class DiceService : IDiceService
    {
        private readonly IRandomProvider _randomProvider;

        public DiceService(IRandomProvider randomProvider)
        {
            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        }

        public DiceRollResult Roll(DiceCheckDefinition check, PlayerStats stats)
        {
            if (check == null)
            {
                throw new ArgumentNullException(nameof(check));
            }

            int positiveDie = _randomProvider.RangeInclusive(1, 6);
            int negativeDie = _randomProvider.RangeInclusive(1, 6);
            int statBonus = stats.Get(check.StatType);
            return new DiceRollResult(positiveDie, negativeDie, statBonus, check.ExtraBonus, check.Difficulty);
        }

        public bool CheckResult(DiceRollResult result, int difficulty)
        {
            return result.Total >= difficulty;
        }
    }
}
