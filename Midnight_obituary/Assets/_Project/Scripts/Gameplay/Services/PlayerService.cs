using System;
using System.Collections.Generic;
using MidnightObituary.Core;
using MidnightObituary.Gameplay.Definitions;
using UnityEngine;

namespace MidnightObituary.Gameplay.Services
{
    public sealed class PlayerService : IPlayerService
    {
        public event Action<int, int, StressChangeReason> StressChanged;
        public event Action PlayerBreakdownTriggered;

        public PlayerState Player { get; private set; } = new PlayerState();

        public PlayerState InitializeNewPlayer(PlayerInitialConfig config)
        {
            Player = new PlayerState();

            if (config != null)
            {
                Player.Stats = config.Stats;
                Player.StressMax = Mathf.Max(1, config.StressMax);
                Player.Cigarettes = Mathf.Max(0, config.Cigarettes);
                Player.PersonalityTags.Clear();
                foreach (PersonalityTag tag in config.InitialPersonalityTags)
                {
                    Player.PersonalityTags.Add(tag);
                }
            }

            return Player;
        }

        public void ApplyPersonalityCards(IReadOnlyList<PersonalityDefinition> personalities)
        {
            if (personalities == null)
            {
                return;
            }

            Player.PersonalityTags.Clear();
            for (int i = 0; i < personalities.Count; i++)
            {
                PersonalityDefinition personality = personalities[i];
                if (personality == null)
                {
                    continue;
                }

                Player.PersonalityTags.Add(personality.Tag);
                Player.Stats.Add(personality.StatModifierType, personality.StatModifierDelta);
            }
        }

        public void ChangeStress(int delta, StressChangeReason reason)
        {
            int previous = Player.Stress;
            Player.Stress = Mathf.Clamp(Player.Stress + delta, 0, Player.StressMax);

            if (previous != Player.Stress)
            {
                StressChanged?.Invoke(previous, Player.Stress, reason);
            }

            if (Player.IsBrokenDown)
            {
                PlayerBreakdownTriggered?.Invoke();
            }
        }

        public bool CanUseCigarette()
        {
            return Player.Cigarettes > 0 && Player.Stress > 0;
        }

        public UseItemResult UseCigarette()
        {
            if (!CanUseCigarette())
            {
                return new UseItemResult(false, 0, 0);
            }

            Player.Cigarettes -= 1;
            ChangeStress(-1, StressChangeReason.Cigarette);
            return new UseItemResult(true, -1, -1);
        }
    }
}
