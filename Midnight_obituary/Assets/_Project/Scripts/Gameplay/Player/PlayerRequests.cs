using System.Collections.Generic;
using ObituaryTomorrow.Core;

namespace ObituaryTomorrow.Gameplay.Player
{
    public readonly struct PlayerInitRequest
    {
        public IReadOnlyList<PersonalityTag> PersonalityTags { get; }

        public PlayerInitRequest(IReadOnlyList<PersonalityTag> personalityTags)
        {
            PersonalityTags = personalityTags;
        }
    }

    public readonly struct StressChangeRequest
    {
        public int Delta { get; }
        public StatChangeReason Reason { get; }
        public string SourceId { get; }
        public bool AllowTriggerEnding { get; }

        public StressChangeRequest(
            int delta,
            StatChangeReason reason,
            string sourceId,
            bool allowTriggerEnding)
        {
            Delta = delta;
            Reason = reason;
            SourceId = sourceId;
            AllowTriggerEnding = allowTriggerEnding;
        }
    }
}