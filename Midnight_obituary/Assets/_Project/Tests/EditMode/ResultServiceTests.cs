using MidnightObituary.Core;
using MidnightObituary.Gameplay.Services;
using NUnit.Framework;

namespace MidnightObituary.Tests.EditMode
{
    public sealed class ResultServiceTests
    {
        [Test]
        public void DeepRedemption_ClearsStressRewardsCigaretteAndRemovesObituary()
        {
            var player = new PlayerState { Stress = 4, StressMax = 5, Cigarettes = 1 };
            var mission = new MissionState { MissionId = "mission_lena_001" };
            var ending = new EndingService();
            MissionResult result = ending.BuildMissionResult(EndingType.DeepRedemption, mission.MissionId);

            new ResultService(player, mission).ApplyResult(result);

            Assert.AreEqual(0, player.Stress);
            Assert.AreEqual(2, player.Cigarettes);
            Assert.AreEqual(ObituaryState.Removed, mission.ObituaryState);
        }

        [Test]
        public void DelayRewrite_ClearsStressWithoutCigaretteReward()
        {
            var player = new PlayerState { Stress = 3, StressMax = 5, Cigarettes = 1 };
            MissionResult result = new EndingService().BuildMissionResult(EndingType.DelayRewrite, "mission_lena_001");

            new ResultService(player).ApplyResult(result);

            Assert.AreEqual(0, player.Stress);
            Assert.AreEqual(1, player.Cigarettes);
        }

        [Test]
        public void CallFailed_ReducesStressMaxButNotBelowThree()
        {
            var player = new PlayerState { Stress = 4, StressMax = 3, Cigarettes = 1 };
            MissionResult result = new EndingService().BuildMissionResult(EndingType.CallFailed, "mission_lena_001");

            new ResultService(player).ApplyResult(result);

            Assert.AreEqual(3, player.StressMax);
            Assert.AreEqual(3, player.Stress);
        }
    }
}
