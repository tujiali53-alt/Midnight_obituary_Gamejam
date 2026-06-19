using System;
using System.Collections.Generic;

namespace ObituaryTomorrow.Core
{
    [Serializable]
    public sealed class GameSessionData
    {
        public PlayerRuntimeData Player { get; set; }
        public string CurrentMissionId { get; set; }
        public string CurrentNpcId { get; set; }
        public int CurrentDay { get; set; }

        public List<string> CompletedMissionIds { get; }
        public Dictionary<string, ObituaryState> ObituaryStates { get; }
        public Dictionary<string, bool> Flags { get; }

        public GameSessionData()
        {
            Player = new PlayerRuntimeData();
            CurrentMissionId = string.Empty;
            CurrentNpcId = string.Empty;
            CurrentDay = 1;
            CompletedMissionIds = new List<string>();
            ObituaryStates = new Dictionary<string, ObituaryState>();
            Flags = new Dictionary<string, bool>();
        }
    }

    [Serializable]
    public sealed class PlayerRuntimeData
    {
        private readonly List<PersonalityTag> personalityTags = new List<PersonalityTag>();

        public IReadOnlyList<PersonalityTag> PersonalityTags => personalityTags;

        public int Perception { get; set; }
        public int Logic { get; set; }
        public int Insight { get; set; }
        public int Resilience { get; set; }
        public int CurrentStress { get; set; }
        public int MaxStress { get; set; }
        public int CigaretteCount { get; set; }
        public int MaxCigaretteCount { get; set; }

        public PlayerRuntimeData()
        {
            Perception = 4;
            Logic = 4;
            Insight = 4;
            Resilience = 4;
            CurrentStress = 0;
            MaxStress = 5;
            CigaretteCount = 5;
            MaxCigaretteCount = 5;
        }

        public void SetPersonalityTags(IEnumerable<PersonalityTag> tags)
        {
            personalityTags.Clear();

            if (tags == null)
            {
                return;
            }

            foreach (PersonalityTag tag in tags)
            {
                if (!personalityTags.Contains(tag))
                {
                    personalityTags.Add(tag);
                }
            }
        }
    }
}