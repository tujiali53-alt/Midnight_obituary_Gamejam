using MidnightObituary.Core;
using MidnightObituary.Gameplay.Definitions;
using MidnightObituary.Gameplay.Services;
using NUnit.Framework;

namespace MidnightObituary.Tests.EditMode
{
    public sealed class PersonalityRuleServiceTests
    {
        [Test]
        public void ResolveChoice_PlayerMismatchAddsStressAndNpcMatchReducesBreakdown()
        {
            var player = new PlayerState { Stress = 1, StressMax = 5 };
            player.PersonalityTags.Add(PersonalityTag.Rational);
            var npc = new NpcRuntimeState { PersonalityTag = PersonalityTag.Feeling, Breakdown = 2, BreakdownMax = 3 };
            var choice = new DialogueChoiceDefinition();
            choice.Tags.Add(PersonalityTag.Feeling);

            var service = new PersonalityRuleService();
            PersonalityRuleResolution resolution = service.ResolveChoice(player, npc, choice);

            Assert.IsFalse(resolution.PlayerMatch.IsMatch);
            Assert.IsTrue(resolution.NpcMatch.IsMatch);
            Assert.AreEqual(2, player.Stress);
            Assert.AreEqual(1, npc.Breakdown);
        }

        [Test]
        public void ResolveChoice_PlayerMatchAvoidsStressAndNpcMismatchAddsBreakdown()
        {
            var player = new PlayerState { Stress = 0, StressMax = 5 };
            player.PersonalityTags.Add(PersonalityTag.Idealist);
            var npc = new NpcRuntimeState { PersonalityTag = PersonalityTag.Pragmatic, Breakdown = 0, BreakdownMax = 3 };
            var choice = new DialogueChoiceDefinition();
            choice.Tags.Add(PersonalityTag.Idealist);

            var service = new PersonalityRuleService();
            PersonalityRuleResolution resolution = service.ResolveChoice(player, npc, choice);

            Assert.IsTrue(resolution.PlayerMatch.IsMatch);
            Assert.IsFalse(resolution.NpcMatch.IsMatch);
            Assert.AreEqual(0, player.Stress);
            Assert.AreEqual(1, npc.Breakdown);
        }
    }
}
