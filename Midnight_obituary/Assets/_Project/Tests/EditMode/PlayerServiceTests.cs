using MidnightObituary.Core;
using MidnightObituary.Gameplay.Services;
using NUnit.Framework;

namespace MidnightObituary.Tests.EditMode
{
    public sealed class PlayerServiceTests
    {
        [Test]
        public void ChangeStress_ClampsStressAndTriggersBreakdownAtMax()
        {
            var service = new PlayerService();
            service.Player.StressMax = 5;
            bool breakdownTriggered = false;
            service.PlayerBreakdownTriggered += () => breakdownTriggered = true;

            service.ChangeStress(99, StressChangeReason.Debug);

            Assert.AreEqual(5, service.Player.Stress);
            Assert.IsTrue(breakdownTriggered);
        }

        [Test]
        public void UseCigarette_ConsumesOneCigaretteAndReducesStress()
        {
            var service = new PlayerService();
            service.Player.Stress = 2;
            service.Player.Cigarettes = 3;

            UseItemResult result = service.UseCigarette();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, service.Player.Stress);
            Assert.AreEqual(2, service.Player.Cigarettes);
        }

        [Test]
        public void UseCigarette_FailsAtZeroStress()
        {
            var service = new PlayerService();
            service.Player.Stress = 0;
            service.Player.Cigarettes = 3;

            UseItemResult result = service.UseCigarette();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(3, service.Player.Cigarettes);
        }
    }
}
