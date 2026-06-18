using MidnightObituary.Gameplay.Definitions;
using MidnightObituary.Gameplay.Services;
using NUnit.Framework;

namespace MidnightObituary.Tests.EditMode
{
    public sealed class CallCounterServiceTests
    {
        [Test]
        public void AddPlayerSpeech_CountsOnlyExplicitCalls()
        {
            var service = new CallCounterService();
            service.Initialize(null, new CallCounterConfig { DefaultDelayTarget = 3 });

            service.AddPlayerSpeech();
            service.AddPlayerSpeech();

            Assert.AreEqual(2, service.CurrentCount);
            Assert.IsFalse(service.HasReachedDelayTarget());
        }

        [Test]
        public void AddPlayerSpeech_ReachesDelayTarget()
        {
            var service = new CallCounterService();
            bool delayReached = false;
            service.DelayEndingReached += () => delayReached = true;
            service.Initialize(null, new CallCounterConfig { DefaultDelayTarget = 2 });

            service.AddPlayerSpeech();
            service.AddPlayerSpeech();

            Assert.IsTrue(service.HasReachedDelayTarget());
            Assert.IsTrue(delayReached);
        }

        [Test]
        public void AddPlayerSpeech_AccumulatesLongCallStressPenaltyByConfig()
        {
            var service = new CallCounterService();
            service.Initialize(null, new CallCounterConfig
            {
                DefaultDelayTarget = 99,
                LongCallStartsAfter = 3,
                LongCallStressInterval = 2,
                LongCallStressDelta = 1
            });

            for (int i = 0; i < 5; i++)
            {
                service.AddPlayerSpeech();
            }

            Assert.AreEqual(1, service.ConsumePendingStressPenalty());
            Assert.AreEqual(0, service.ConsumePendingStressPenalty());
        }
    }
}
