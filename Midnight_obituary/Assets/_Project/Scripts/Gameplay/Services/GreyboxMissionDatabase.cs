using MidnightObituary.Gameplay.Definitions;
using UnityEngine;

namespace MidnightObituary.Gameplay.Services
{
    [CreateAssetMenu(menuName = "Midnight Obituary/Greybox/Greybox Mission Database")]
    public sealed class GreyboxMissionDatabase : ScriptableObject
    {
        public GameConfig GameConfig;
        public MissionDefinition Mission;
        public NpcDefinition Npc;
        public ObituaryDefinition Obituary;
        public YellowPageEntryDefinition YellowPageEntry;
        public DialogueTreeDefinition DialogueTree;
    }
}