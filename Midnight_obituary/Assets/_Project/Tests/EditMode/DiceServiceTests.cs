using MidnightObituary.Core;
using MidnightObituary.Gameplay.Definitions;
using MidnightObituary.Gameplay.Services;
using NUnit.Framework;

namespace MidnightObituary.Tests.EditMode
{
    public sealed class DiceServiceTests
    {
        [Test]
        public void Roll_UsesPositiveMinusNegativePlusStatAndExtraBonus()
        {
            var service = new DiceService(new DeterministicRandomProvider(5, 2));
            var stats = new PlayerStats(perception: 4, logic: 6, insight: 3, resilience: 4);
            var check = new DiceCheckDefinition
            {
                StatType = StatType.Logic,
                Difficulty = 10,
                ExtraBonus = 1
            };

            DiceRollResult result = service.Roll(check, stats);

            Assert.AreEqual(5, result.PositiveDie);
            Assert.AreEqual(2, result.NegativeDie);
            Assert.AreEqual(6, result.StatBonus);
            Assert.AreEqual(10, result.Total);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public void CheckResult_FailsWhenTotalBelowDifficulty()
        {
            var service = new DiceService(new DeterministicRandomProvider(1, 6));
            var result = new DiceRollResult(1, 6, 4, 0, 1);

            Assert.IsFalse(service.CheckResult(result, 1));
        }
    }
}
