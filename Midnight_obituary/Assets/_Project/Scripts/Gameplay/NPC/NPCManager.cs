using System;
using UnityEngine;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Data;

namespace ObituaryTomorrow.Gameplay.NPC
{
    public sealed class NPCManager : MonoBehaviour
    {
        [Header("NPC Definitions")]
        [SerializeField] private NPCDefinition[] npcDefinitions;

        [Header("Default NPC")]
        [SerializeField] private string defaultNpcId = "NPC_Lena_001";
        [SerializeField] private string defaultDisplayName = "Lena";
        [SerializeField] private PersonalityTag defaultPersonalityTag = PersonalityTag.Emotional;
        [SerializeField] private int defaultMaxBreakdown = 3;
        [SerializeField] private int defaultStartingBreakdown = 1;
        [SerializeField] private int defaultDelayThreshold = 30;
        [SerializeField] private string defaultDialogueId = "DIA_Lena_001";

        public NPCRuntimeData CurrentNPC { get; private set; }
        public bool HasActiveNPC => CurrentNPC != null;
        public string CurrentNpcId => CurrentNPC != null ? CurrentNPC.NpcId : string.Empty;
        public string DisplayName => CurrentNPC != null ? CurrentNPC.DisplayName : string.Empty;
        public PersonalityTag PersonalityTag => CurrentNPC != null ? CurrentNPC.PersonalityTag : defaultPersonalityTag;
        public int CurrentBreakdown => CurrentNPC != null ? CurrentNPC.Breakdown : 0;
        public int MaxBreakdown => CurrentNPC != null ? CurrentNPC.MaxBreakdown : Mathf.Max(1, defaultMaxBreakdown);

        private void Awake()
        {
            if (!HasActiveNPC)
            {
                BeginCall(defaultNpcId);
            }
        }

        public void BeginCall(string npcId)
        {
            LoadNPC(npcId);
        }

        public OperationResult LoadNPC(string npcId)
        {
            string resolvedNpcId = string.IsNullOrWhiteSpace(npcId) ? defaultNpcId : npcId;
            NPCDefinition definition = FindDefinition(resolvedNpcId);
            CurrentNPC = definition != null ? definition.CreateRuntimeData() : CreateDefaultRuntimeData(resolvedNpcId);

            GameEventBus.RaiseNPCBreakdownChanged(
                new NPCBreakdownChangedEventArgs(CurrentNpcId, CurrentBreakdown, CurrentBreakdown, MaxBreakdown, StatChangeReason.Debug));

            return OperationResult.Ok(definition != null
                ? $"Loaded NPC definition: {CurrentNpcId}"
                : $"Loaded fallback NPC data: {CurrentNpcId}");
        }

        public void ClearCurrentNPC()
        {
            CurrentNPC = null;
        }

        public PersonalityTag GetCurrentNPCPersonality()
        {
            return PersonalityTag;
        }

        public StatChangeResult RequestBreakdownChange(BreakdownChangeRequest request)
        {
            string sourceId = string.IsNullOrWhiteSpace(request.SourceChoiceId)
                ? request.SourceNodeId
                : $"{request.SourceNodeId}:{request.SourceChoiceId}";

            return RequestBreakdownChange(new NPCBreakdownChangeRequest(CurrentNpcId, request.Delta, request.Reason, sourceId));
        }

        public StatChangeResult RequestBreakdownChange(NPCBreakdownChangeRequest request)
        {
            if (!HasActiveNPC)
            {
                BeginCall(request.NpcId);
            }

            int oldValue = CurrentBreakdown;
            int newValue = Mathf.Clamp(oldValue + request.Delta, 0, MaxBreakdown);
            CurrentNPC.Breakdown = newValue;

            bool applied = oldValue != newValue;

            if (applied)
            {
                GameEventBus.RaiseNPCBreakdownChanged(
                    new NPCBreakdownChangedEventArgs(CurrentNpcId, oldValue, newValue, MaxBreakdown, request.Reason));
            }

            return new StatChangeResult(
                applied,
                oldValue,
                newValue,
                newValue - oldValue,
                newValue <= 0,
                newValue >= MaxBreakdown,
                request.Reason);
        }

        public void RestoreRuntimeData(
            string npcId,
            string displayName,
            PersonalityTag personalityTag,
            int breakdown,
            int maxBreakdown,
            int delayThreshold,
            string dialogueId)
        {
            int oldValue = CurrentBreakdown;
            string resolvedNpcId = string.IsNullOrWhiteSpace(npcId) ? defaultNpcId : npcId;
            string resolvedDisplayName = string.IsNullOrWhiteSpace(displayName) ? defaultDisplayName : displayName;
            string resolvedDialogueId = string.IsNullOrWhiteSpace(dialogueId) ? defaultDialogueId : dialogueId;

            CurrentNPC = new NPCRuntimeData(
                resolvedNpcId,
                resolvedDisplayName,
                personalityTag,
                breakdown,
                maxBreakdown,
                delayThreshold,
                resolvedDialogueId);

            if (GameManager.Instance != null && GameManager.Instance.Session != null)
            {
                GameManager.Instance.Session.CurrentNpcId = CurrentNPC.NpcId;
            }

            GameEventBus.RaiseNPCBreakdownChanged(
                new NPCBreakdownChangedEventArgs(CurrentNpcId, oldValue, CurrentBreakdown, MaxBreakdown, StatChangeReason.Debug));
        }
        public bool IsBreakdownZero()
        {
            return CurrentBreakdown <= 0;
        }

        public bool IsBreakdownMaxed()
        {
            return CurrentBreakdown >= MaxBreakdown;
        }

        public EndingResult CreateCallFailedEnding(string missionId)
        {
            return new EndingResult(
                EndingType.CallFailed,
                missionId,
                CurrentNpcId,
                true,
                false);
        }

        private NPCDefinition FindDefinition(string npcId)
        {
            if (npcDefinitions == null || npcDefinitions.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < npcDefinitions.Length; i++)
            {
                NPCDefinition definition = npcDefinitions[i];
                if (definition != null && string.Equals(definition.NpcId, npcId, StringComparison.Ordinal))
                {
                    return definition;
                }
            }

            return null;
        }

        private NPCRuntimeData CreateDefaultRuntimeData(string npcId)
        {
            int maxBreakdown = Mathf.Max(1, defaultMaxBreakdown);
            int currentBreakdown = Mathf.Clamp(defaultStartingBreakdown, 0, maxBreakdown);

            return new NPCRuntimeData(
                string.IsNullOrWhiteSpace(npcId) ? defaultNpcId : npcId,
                defaultDisplayName,
                defaultPersonalityTag,
                currentBreakdown,
                maxBreakdown,
                Mathf.Max(1, defaultDelayThreshold),
                defaultDialogueId);
        }
    }

    public readonly struct BreakdownChangeRequest
    {
        public int Delta { get; }
        public StatChangeReason Reason { get; }
        public string SourceNodeId { get; }
        public string SourceChoiceId { get; }

        public BreakdownChangeRequest(int delta, StatChangeReason reason, string sourceNodeId, string sourceChoiceId)
        {
            Delta = delta;
            Reason = reason;
            SourceNodeId = sourceNodeId;
            SourceChoiceId = sourceChoiceId;
        }
    }

    public readonly struct NPCBreakdownChangeRequest
    {
        public string NpcId { get; }
        public int Delta { get; }
        public StatChangeReason Reason { get; }
        public string SourceId { get; }

        public NPCBreakdownChangeRequest(string npcId, int delta, StatChangeReason reason, string sourceId)
        {
            NpcId = npcId;
            Delta = delta;
            Reason = reason;
            SourceId = sourceId;
        }
    }
}
