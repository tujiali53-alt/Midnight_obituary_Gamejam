using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Data;
using ObituaryTomorrow.Gameplay.Call;
using ObituaryTomorrow.Gameplay.NPC;
using ObituaryTomorrow.Gameplay.Player;
using ObituaryTomorrow.UI;

namespace ObituaryTomorrow.Gameplay.Save
{
    public sealed class SaveManager : MonoBehaviour
    {
        private const int SlotCount = 3;
        private const int SaveVersion = 1;

        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private NPCManager npcManager;
        [SerializeField] private CallCounterSystem callCounterSystem;
        [SerializeField] private CallGreyboxController callGreyboxController;

        private void Awake()
        {
            ResolveReferences();
        }

        public void SaveSlot1() { SaveSlot(1); }
        public void SaveSlot2() { SaveSlot(2); }
        public void SaveSlot3() { SaveSlot(3); }
        public void LoadSlot1() { LoadSlot(1); }
        public void LoadSlot2() { LoadSlot(2); }
        public void LoadSlot3() { LoadSlot(3); }

        public bool HasSlot(int slotIndex)
        {
            return IsValidSlot(slotIndex) && File.Exists(GetSlotPath(slotIndex));
        }

        public OperationResult SaveSlot(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return OperationResult.Fail($"Invalid save slot: {slotIndex}");
            }

            ResolveReferences();
            SaveData data = CaptureSaveData(slotIndex);
            string json = JsonUtility.ToJson(data, true);
            Directory.CreateDirectory(GetSaveDirectory());
            File.WriteAllText(GetSlotPath(slotIndex), json);
            Debug.Log($"Saved game to slot {slotIndex}: {GetSlotPath(slotIndex)}");
            return OperationResult.Ok($"Saved slot {slotIndex}.");
        }

        public OperationResult LoadSlot(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return OperationResult.Fail($"Invalid save slot: {slotIndex}");
            }

            string path = GetSlotPath(slotIndex);

            if (!File.Exists(path))
            {
                return OperationResult.Fail($"Save slot {slotIndex} is empty.");
            }

            ResolveReferences();
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));

            if (data == null)
            {
                return OperationResult.Fail($"Save slot {slotIndex} is invalid.");
            }

            ApplySaveData(data);
            Debug.Log($"Loaded game from slot {slotIndex}: {path}");
            return OperationResult.Ok($"Loaded slot {slotIndex}.");
        }

        private SaveData CaptureSaveData(int slotIndex)
        {
            PlayerRuntimeData playerData = GetPlayerData();
            NPCRuntimeData npcData = npcManager != null ? npcManager.CurrentNPC : null;
            GameSessionData session = GameManager.Instance != null ? GameManager.Instance.Session : null;

            return new SaveData
            {
                version = SaveVersion,
                slotIndex = slotIndex,
                savedAtUtc = DateTime.UtcNow.ToString("O"),
                currentMissionId = session != null ? session.CurrentMissionId : string.Empty,
                currentNpcId = npcData != null ? npcData.NpcId : session != null ? session.CurrentNpcId : string.Empty,
                currentDay = session != null ? session.CurrentDay : 1,
                currentArticyFragmentId = callGreyboxController != null ? callGreyboxController.GetCurrentArticyFragmentId().ToString() : string.Empty,
                callCount = callGreyboxController != null ? callGreyboxController.GetCurrentCallCount() : callCounterSystem != null ? callCounterSystem.CurrentCount : 0,
                delayReminderShown = callGreyboxController != null && callGreyboxController.GetDelayReminderShown(),
                dialogueHistory = callGreyboxController != null ? callGreyboxController.GetDialogueHistory() : string.Empty,
                player = PlayerSaveData.FromRuntime(playerData),
                npc = NPCSaveData.FromRuntime(npcData)
            };
        }

        private void ApplySaveData(SaveData data)
        {
            if (GameManager.Instance != null && GameManager.Instance.Session != null)
            {
                GameManager.Instance.Session.CurrentMissionId = NullToEmpty(data.currentMissionId);
                GameManager.Instance.Session.CurrentNpcId = NullToEmpty(data.currentNpcId);
                GameManager.Instance.Session.CurrentDay = Mathf.Max(1, data.currentDay);
            }

            PlayerRuntimeData playerData = data.player != null ? data.player.ToRuntime() : new PlayerRuntimeData();

            if (playerManager != null)
            {
                playerManager.RestoreRuntimeData(playerData);
            }
            else if (GameManager.Instance != null && GameManager.Instance.Session != null)
            {
                GameManager.Instance.Session.Player = playerData;
                GameEventBus.RaisePlayerStressChanged(new StressChangedEventArgs(playerData.CurrentStress, playerData.CurrentStress, playerData.MaxStress, StatChangeReason.Debug));
                GameEventBus.RaiseCigaretteChanged(new CigaretteChangedEventArgs(playerData.CigaretteCount, playerData.CigaretteCount, playerData.MaxCigaretteCount, StatChangeReason.Debug));
            }

            if (data.npc != null && npcManager != null)
            {
                npcManager.RestoreRuntimeData(data.npc.npcId, data.npc.displayName, data.npc.personalityTag, data.npc.breakdown, data.npc.maxBreakdown, data.npc.delayThreshold, data.npc.dialogueId);
            }

            string npcId = data.npc != null && !string.IsNullOrWhiteSpace(data.npc.npcId) ? data.npc.npcId : data.currentNpcId;

            if (callCounterSystem != null)
            {
                callCounterSystem.RestoreState(npcId, data.callCount);
            }

            if (callGreyboxController != null)
            {
                callGreyboxController.RestoreArticyState(ParseArticyFragmentId(data.currentArticyFragmentId), data.callCount, data.delayReminderShown, data.dialogueHistory);
            }
        }

        private PlayerRuntimeData GetPlayerData()
        {
            if (playerManager != null && playerManager.RuntimeData != null)
            {
                return playerManager.RuntimeData;
            }

            return GameManager.Instance != null && GameManager.Instance.Session != null ? GameManager.Instance.Session.Player : new PlayerRuntimeData();
        }

        private void ResolveReferences()
        {
            if (playerManager == null) { playerManager = FindFirstObjectByType<PlayerManager>(); }
            if (npcManager == null) { npcManager = FindFirstObjectByType<NPCManager>(); }
            if (callCounterSystem == null) { callCounterSystem = FindFirstObjectByType<CallCounterSystem>(); }
            if (callGreyboxController == null) { callGreyboxController = FindFirstObjectByType<CallGreyboxController>(); }
        }

        private static bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 1 && slotIndex <= SlotCount;
        }

        private static string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "Saves");
        }

        private static string GetSlotPath(int slotIndex)
        {
            return Path.Combine(GetSaveDirectory(), $"slot_{slotIndex}.json");
        }

        private static string NullToEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }


        private static ulong ParseArticyFragmentId(string value)
        {
            return ulong.TryParse(value, out ulong fragmentId) ? fragmentId : 0UL;
        }
        [Serializable]
        private sealed class SaveData
        {
            public int version;
            public int slotIndex;
            public string savedAtUtc;
            public string currentMissionId;
            public string currentNpcId;
            public int currentDay;
            public string currentArticyFragmentId;
            public int callCount;
            public bool delayReminderShown;
            public string dialogueHistory;
            public PlayerSaveData player;
            public NPCSaveData npc;
        }


        [Serializable]
        private sealed class PlayerSaveData
        {
            public int perception;
            public int logic;
            public int insight;
            public int resilience;
            public int currentStress;
            public int maxStress;
            public int cigaretteCount;
            public int maxCigaretteCount;
            public PersonalityTag[] personalityTags;

            public static PlayerSaveData FromRuntime(PlayerRuntimeData runtimeData)
            {
                if (runtimeData == null)
                {
                    runtimeData = new PlayerRuntimeData();
                }

                List<PersonalityTag> tags = new List<PersonalityTag>();

                if (runtimeData.PersonalityTags != null)
                {
                    tags.AddRange(runtimeData.PersonalityTags);
                }

                return new PlayerSaveData
                {
                    perception = runtimeData.Perception,
                    logic = runtimeData.Logic,
                    insight = runtimeData.Insight,
                    resilience = runtimeData.Resilience,
                    currentStress = runtimeData.CurrentStress,
                    maxStress = runtimeData.MaxStress,
                    cigaretteCount = runtimeData.CigaretteCount,
                    maxCigaretteCount = runtimeData.MaxCigaretteCount,
                    personalityTags = tags.ToArray()
                };
            }

            public PlayerRuntimeData ToRuntime()
            {
                PlayerRuntimeData runtimeData = new PlayerRuntimeData
                {
                    Perception = Mathf.Max(1, perception),
                    Logic = Mathf.Max(1, logic),
                    Insight = Mathf.Max(1, insight),
                    Resilience = Mathf.Max(1, resilience),
                    MaxStress = Mathf.Max(1, maxStress),
                    MaxCigaretteCount = Mathf.Max(0, maxCigaretteCount)
                };

                runtimeData.CurrentStress = Mathf.Clamp(currentStress, 0, runtimeData.MaxStress);
                runtimeData.CigaretteCount = Mathf.Clamp(cigaretteCount, 0, runtimeData.MaxCigaretteCount);
                runtimeData.SetPersonalityTags(personalityTags);
                return runtimeData;
            }
        }


        [Serializable]
        private sealed class NPCSaveData
        {
            public string npcId;
            public string displayName;
            public PersonalityTag personalityTag;
            public int breakdown;
            public int maxBreakdown;
            public int delayThreshold;
            public string dialogueId;

            public static NPCSaveData FromRuntime(NPCRuntimeData runtimeData)
            {
                if (runtimeData == null)
                {
                    return new NPCSaveData
                    {
                        npcId = string.Empty,
                        displayName = string.Empty,
                        personalityTag = PersonalityTag.Emotional,
                        breakdown = 0,
                        maxBreakdown = 3,
                        delayThreshold = 30,
                        dialogueId = string.Empty
                    };
                }

                return new NPCSaveData
                {
                    npcId = runtimeData.NpcId,
                    displayName = runtimeData.DisplayName,
                    personalityTag = runtimeData.PersonalityTag,
                    breakdown = runtimeData.Breakdown,
                    maxBreakdown = runtimeData.MaxBreakdown,
                    delayThreshold = runtimeData.DelayThreshold,
                    dialogueId = runtimeData.DialogueId
                };
            }
        }
    }
}