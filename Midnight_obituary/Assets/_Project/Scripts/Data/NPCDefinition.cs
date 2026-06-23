using UnityEngine;
using ObituaryTomorrow.Core;

namespace ObituaryTomorrow.Data
{
    [CreateAssetMenu(menuName = "ObituaryTomorrow/NPC Definition")]
    public sealed class NPCDefinition : ScriptableObject
    {
        [field: SerializeField] public string NpcId { get; private set; } = "NPC_Lena_001";
        [field: SerializeField] public string DisplayName { get; private set; } = "Lena";
        [field: SerializeField] public PersonalityTag PersonalityTag { get; private set; } = PersonalityTag.Emotional;
        [field: SerializeField, Min(0)] public int InitialBreakdown { get; private set; } = 1;
        [field: SerializeField, Min(1)] public int MaxBreakdown { get; private set; } = 3;
        [field: SerializeField, Min(1)] public int DelayThreshold { get; private set; } = 30;
        [field: SerializeField] public string DialogueId { get; private set; } = "DIA_Lena_001";

        public NPCRuntimeData CreateRuntimeData()
        {
            int maxBreakdown = Mathf.Max(1, MaxBreakdown);
            int currentBreakdown = Mathf.Clamp(InitialBreakdown, 0, maxBreakdown);

            return new NPCRuntimeData(
                NpcId,
                DisplayName,
                PersonalityTag,
                currentBreakdown,
                maxBreakdown,
                Mathf.Max(1, DelayThreshold),
                DialogueId);
        }
    }

    public sealed class NPCRuntimeData
    {
        public string NpcId { get; }
        public string DisplayName { get; }
        public PersonalityTag PersonalityTag { get; }
        public int Breakdown { get; set; }
        public int MaxBreakdown { get; }
        public int DelayThreshold { get; }
        public string DialogueId { get; }

        public NPCRuntimeData(
            string npcId,
            string displayName,
            PersonalityTag personalityTag,
            int breakdown,
            int maxBreakdown,
            int delayThreshold,
            string dialogueId)
        {
            NpcId = npcId;
            DisplayName = displayName;
            PersonalityTag = personalityTag;
            MaxBreakdown = Mathf.Max(1, maxBreakdown);
            Breakdown = Mathf.Clamp(breakdown, 0, MaxBreakdown);
            DelayThreshold = Mathf.Max(1, delayThreshold);
            DialogueId = dialogueId;
        }
    }
}
