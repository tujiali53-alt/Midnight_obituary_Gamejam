using MidnightObituary.Core;
using MidnightObituary.Gameplay.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace MidnightObituary.Tests.EditMode
{
    public sealed class NpcDefinitionTests
    {
        [Test]
        public void CreateRuntimeState_UsesConfiguredNpcModelValues()
        {
            var npc = ScriptableObject.CreateInstance<NpcDefinition>();
            npc.NpcId = "npc_test";
            npc.PersonalityTag = PersonalityTag.Rational;
            npc.InitialBreakdown = 2;
            npc.BreakdownMax = 4;
            npc.DelayTargetCount = 12;

            NpcRuntimeState state = npc.CreateRuntimeState();

            Assert.AreEqual("npc_test", state.NpcId);
            Assert.AreEqual(PersonalityTag.Rational, state.PersonalityTag);
            Assert.AreEqual(2, state.Breakdown);
            Assert.AreEqual(4, state.BreakdownMax);
            Assert.AreEqual(12, state.DelayTargetCount);
        }

        [Test]
        public void CreateRuntimeState_ClampsInitialBreakdownToBreakdownMax()
        {
            var npc = ScriptableObject.CreateInstance<NpcDefinition>();
            npc.InitialBreakdown = 9;
            npc.BreakdownMax = 3;

            NpcRuntimeState state = npc.CreateRuntimeState();

            Assert.AreEqual(3, state.Breakdown);
        }

        [Test]
        public void CreateRuntimeState_NormalizesNegativeBreakdownMax()
        {
            var npc = ScriptableObject.CreateInstance<NpcDefinition>();
            npc.InitialBreakdown = 1;
            npc.BreakdownMax = -2;

            NpcRuntimeState state = npc.CreateRuntimeState();

            Assert.AreEqual(0, state.Breakdown);
            Assert.AreEqual(0, state.BreakdownMax);
        }
    }
}
