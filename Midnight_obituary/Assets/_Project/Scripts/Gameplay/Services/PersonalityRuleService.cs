using System;
using MidnightObituary.Core;
using MidnightObituary.Gameplay.Definitions;
using UnityEngine;

namespace MidnightObituary.Gameplay.Services
{
    public sealed class PersonalityRuleService : IPersonalityRuleService
    {
        public TagMatchResult CheckPlayerTagMatch(PlayerState player, DialogueChoiceDefinition choice)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (choice == null || choice.Tags == null)
            {
                return new TagMatchResult(false, default);
            }

            for (int i = 0; i < choice.Tags.Count; i++)
            {
                PersonalityTag tag = choice.Tags[i];
                if (player.PersonalityTags.Contains(tag))
                {
                    return new TagMatchResult(true, tag);
                }
            }

            return new TagMatchResult(false, default);
        }

        public TagMatchResult CheckNpcTagMatch(NpcRuntimeState npc, DialogueChoiceDefinition choice)
        {
            if (npc == null)
            {
                throw new ArgumentNullException(nameof(npc));
            }

            if (choice == null || choice.Tags == null)
            {
                return new TagMatchResult(false, default);
            }

            bool isMatch = choice.Tags.Contains(npc.PersonalityTag);
            return new TagMatchResult(isMatch, isMatch ? npc.PersonalityTag : default);
        }

        public PersonalityRuleResolution ResolveChoice(PlayerState player, NpcRuntimeState npc, DialogueChoiceDefinition choice)
        {
            TagMatchResult playerMatch = CheckPlayerTagMatch(player, choice);
            TagMatchResult npcMatch = CheckNpcTagMatch(npc, choice);
            int stressDelta = playerMatch.IsMatch ? 0 : 1;
            int breakdownDelta = npcMatch.IsMatch ? -1 : 1;

            player.Stress = Mathf.Clamp(player.Stress + stressDelta, 0, player.StressMax);
            npc.Breakdown = Mathf.Clamp(npc.Breakdown + breakdownDelta, 0, npc.BreakdownMax);

            return new PersonalityRuleResolution(playerMatch, npcMatch, stressDelta, breakdownDelta);
        }
    }
}
