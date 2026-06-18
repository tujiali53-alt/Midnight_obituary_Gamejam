using System;
using System.Collections.Generic;
using MidnightObituary.Core;
using UnityEngine;

namespace MidnightObituary.Gameplay.Definitions
{
    [CreateAssetMenu(menuName = "Midnight Obituary/Config/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        public PlayerInitialConfig PlayerInitialConfig;
        public CallCounterConfig CallCounterConfig = new CallCounterConfig();
    }

    [CreateAssetMenu(menuName = "Midnight Obituary/Config/Player Initial Config")]
    public sealed class PlayerInitialConfig : ScriptableObject
    {
        public PlayerStats Stats = PlayerStats.CreateDefault();
        public List<PersonalityTag> InitialPersonalityTags = new List<PersonalityTag>();
        public int StressMax = 5;
        public int Cigarettes = 5;
    }

    [CreateAssetMenu(menuName = "Midnight Obituary/Personality Definition")]
    public sealed class PersonalityDefinition : ScriptableObject
    {
        public string PersonalityId;
        public PersonalityTag Tag;
        public StatType StatModifierType;
        public int StatModifierDelta = 1;
    }

    [CreateAssetMenu(menuName = "Midnight Obituary/NPC Definition")]
    public sealed class NpcDefinition : ScriptableObject
    {
        public string NpcId;
        public string DisplayName;
        public PersonalityTag PersonalityTag;
        public int BreakdownMax = 3;
        public int DelayTargetCount = 30;
    }

    [CreateAssetMenu(menuName = "Midnight Obituary/Mission Definition")]
    public sealed class MissionDefinition : ScriptableObject
    {
        public string MissionId;
        public string NpcId;
        public string ObituaryId;
        public string YellowPageEntryId;
        public string DialogueTreeId;
    }

    [CreateAssetMenu(menuName = "Midnight Obituary/Obituary Definition")]
    public sealed class ObituaryDefinition : ScriptableObject
    {
        public string ObituaryId;
        public string MissionId;
        public string Headline;
        [TextArea] public string Body;
        public ObituaryState InitialState = ObituaryState.Pending;
    }

    [CreateAssetMenu(menuName = "Midnight Obituary/Yellow Page Entry")]
    public sealed class YellowPageEntryDefinition : ScriptableObject
    {
        public string EntryId;
        public string MissionId;
        public string DisplayName;
        public string PhoneNumber;
        public string Address;
    }

    [CreateAssetMenu(menuName = "Midnight Obituary/Dialogue Tree")]
    public sealed class DialogueTreeDefinition : ScriptableObject
    {
        public string DialogueTreeId;
        public string NpcId;
        public string RootNodeId;
        public List<DialogueNodeDefinition> Nodes = new List<DialogueNodeDefinition>();
    }

    [Serializable]
    public sealed class DialogueNodeDefinition
    {
        public string NodeId;
        public string SpeakerId;
        [TextArea] public string Text;
        public bool IsDeepRedemptionGate;
        public List<DialogueChoiceDefinition> Choices = new List<DialogueChoiceDefinition>();
    }

    [Serializable]
    public sealed class DialogueChoiceDefinition
    {
        public string ChoiceId;
        [TextArea] public string Text;
        public List<PersonalityTag> Tags = new List<PersonalityTag>();
        public bool HasDiceCheck;
        public StatType DiceStat;
        public int Difficulty;
        public int ExtraBonus;
        public string SuccessNodeId;
        public string FailureNodeId;
        public string NextNodeId;
        public bool CountsAsPlayerSpeech = true;
    }

    [CreateAssetMenu(menuName = "Midnight Obituary/Ending Definition")]
    public sealed class EndingDefinition : ScriptableObject
    {
        public EndingType EndingType;
        public bool ClearStress;
        public int CigarettesDelta;
        public int StressMaxDelta;
        public ObituaryState ObituaryState;
    }

    [Serializable]
    public sealed class DiceCheckDefinition
    {
        public StatType StatType;
        public int Difficulty;
        public int ExtraBonus;
    }

    [Serializable]
    public sealed class CallCounterConfig
    {
        public int DefaultDelayTarget = 30;
        public int LongCallStartsAfter = 30;
        public int LongCallStressInterval = 3;
        public int LongCallStressDelta = 1;
    }
}
