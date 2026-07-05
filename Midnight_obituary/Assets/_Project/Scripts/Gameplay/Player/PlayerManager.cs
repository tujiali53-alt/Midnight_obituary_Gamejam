using System.Collections.Generic;
using UnityEngine;
using ObituaryTomorrow.Core;

namespace ObituaryTomorrow.Gameplay.Player
{
    public sealed class PlayerManager : MonoBehaviour
    {
        [SerializeField] private bool initializeOnStart = true;

        public PlayerRuntimeData RuntimeData { get; private set; }

        public int CurrentStress => RuntimeData != null ? RuntimeData.CurrentStress : 0;
        public int MaxStress => RuntimeData != null ? RuntimeData.MaxStress : 0;
        public int CigaretteCount => RuntimeData != null ? RuntimeData.CigaretteCount : 0;

        private void Start()
        {
            EnsureRuntimeData();

            if (initializeOnStart && GameManager.Instance == null)
            {
                InitializeNewPlayer(new PlayerInitRequest(new[]
                {
                    PersonalityTag.Emotional,
                    PersonalityTag.Practical
                }));
            }
        }

        public void InitializeNewPlayer(PlayerInitRequest request)
        {
            EnsureRuntimeData();

            RuntimeData.Perception = 4;
            RuntimeData.Logic = 4;
            RuntimeData.Insight = 4;
            RuntimeData.Resilience = 4;
            RuntimeData.CurrentStress = 0;
            RuntimeData.MaxStress = 5;
            RuntimeData.CigaretteCount = 5;
            RuntimeData.MaxCigaretteCount = 5;
            RuntimeData.SetPersonalityTags(request.PersonalityTags);

            ApplyPersonalityStats();
        }

        public IReadOnlyList<PersonalityTag> GetPersonalityTags()
        {
            EnsureRuntimeData();
            return RuntimeData.PersonalityTags;
        }

        public int GetAttribute(PlayerAttributeType attributeType)
        {
            EnsureRuntimeData();

            switch (attributeType)
            {
                case PlayerAttributeType.Perception:
                    return RuntimeData.Perception;
                case PlayerAttributeType.Logic:
                    return RuntimeData.Logic;
                case PlayerAttributeType.Insight:
                    return RuntimeData.Insight;
                case PlayerAttributeType.Resilience:
                    return RuntimeData.Resilience;
                default:
                    return 0;
            }
        }

        public bool HasPersonalityTag(PersonalityTag tag)
        {
            EnsureRuntimeData();

            foreach (PersonalityTag personalityTag in RuntimeData.PersonalityTags)
            {
                if (personalityTag == tag)
                {
                    return true;
                }
            }

            return false;
        }

        public StatChangeResult RequestStressChange(StressChangeRequest request)
        {
            EnsureRuntimeData();

            int oldValue = RuntimeData.CurrentStress;
            int unclampedValue = oldValue + request.Delta;
            int newValue = Mathf.Clamp(unclampedValue, 0, RuntimeData.MaxStress);

            RuntimeData.CurrentStress = newValue;

            bool reachedMin = newValue == 0;
            bool reachedMax = newValue == RuntimeData.MaxStress;
            bool applied = oldValue != newValue;

            if (applied)
            {
                GameEventBus.RaisePlayerStressChanged(
                    new StressChangedEventArgs(oldValue, newValue, RuntimeData.MaxStress, request.Reason));
            }

            if (request.AllowTriggerEnding && IsStressMaxed())
            {
                GameEventBus.RaiseEndingTriggered(CreatePlayerBreakdownEnding());
            }

            return new StatChangeResult(
                applied,
                oldValue,
                newValue,
                newValue - oldValue,
                reachedMin,
                reachedMax,
                request.Reason);
        }

        public bool IsStressMaxed()
        {
            EnsureRuntimeData();
            return RuntimeData.CurrentStress >= RuntimeData.MaxStress;
        }

        public EndingResult CreatePlayerBreakdownEnding()
        {
            string missionId = GameManager.Instance != null && GameManager.Instance.Session != null
                ? GameManager.Instance.Session.CurrentMissionId
                : string.Empty;

            return new EndingResult(
                EndingType.PlayerBreakdown,
                missionId,
                string.Empty,
                true,
                true);
        }

        public void ResetStress(StatChangeReason reason)
        {
            RequestStressChange(new StressChangeRequest(
                -CurrentStress,
                reason,
                "PlayerManager.ResetStress",
                false));
        }

        public StatChangeResult ModifyMaxStress(int delta, StatChangeReason reason)
        {
            EnsureRuntimeData();

            int oldValue = RuntimeData.MaxStress;
            int newValue = Mathf.Max(3, oldValue + delta);

            RuntimeData.MaxStress = newValue;

            if (RuntimeData.CurrentStress > RuntimeData.MaxStress)
            {
                RuntimeData.CurrentStress = RuntimeData.MaxStress;
            }

            return new StatChangeResult(
                oldValue != newValue,
                oldValue,
                newValue,
                newValue - oldValue,
                false,
                RuntimeData.CurrentStress >= RuntimeData.MaxStress,
                reason);
        }

        public void RestoreRuntimeData(PlayerRuntimeData data, bool raiseEvents = true)
        {
            if (data == null)
            {
                return;
            }

            EnsureRuntimeData();

            int oldStress = RuntimeData.CurrentStress;
            int oldCigarettes = RuntimeData.CigaretteCount;

            RuntimeData.Perception = Mathf.Max(1, data.Perception);
            RuntimeData.Logic = Mathf.Max(1, data.Logic);
            RuntimeData.Insight = Mathf.Max(1, data.Insight);
            RuntimeData.Resilience = Mathf.Max(1, data.Resilience);
            RuntimeData.MaxStress = Mathf.Max(1, data.MaxStress);
            RuntimeData.CurrentStress = Mathf.Clamp(data.CurrentStress, 0, RuntimeData.MaxStress);
            RuntimeData.MaxCigaretteCount = Mathf.Max(0, data.MaxCigaretteCount);
            RuntimeData.CigaretteCount = Mathf.Clamp(data.CigaretteCount, 0, RuntimeData.MaxCigaretteCount);
            RuntimeData.SetPersonalityTags(data.PersonalityTags);

            if (GameManager.Instance != null && GameManager.Instance.Session != null)
            {
                GameManager.Instance.Session.Player = RuntimeData;
            }

            if (!raiseEvents)
            {
                return;
            }

            GameEventBus.RaisePlayerStressChanged(
                new StressChangedEventArgs(oldStress, RuntimeData.CurrentStress, RuntimeData.MaxStress, StatChangeReason.Debug));
            GameEventBus.RaiseCigaretteChanged(
                new CigaretteChangedEventArgs(oldCigarettes, RuntimeData.CigaretteCount, RuntimeData.MaxCigaretteCount, StatChangeReason.Debug));
        }
        private void EnsureRuntimeData()
        {
            if (RuntimeData != null)
            {
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.Session != null)
            {
                RuntimeData = GameManager.Instance.Session.Player;
                return;
            }

            RuntimeData = new PlayerRuntimeData();
            Debug.LogWarning("PlayerManager created fallback runtime data because GameManager session was missing.");
        }

        private void ApplyPersonalityStats()
        {
            foreach (PersonalityTag tag in RuntimeData.PersonalityTags)
            {
                ApplyPersonalityStat(tag);
            }

            RuntimeData.Perception = Mathf.Max(1, RuntimeData.Perception);
            RuntimeData.Logic = Mathf.Max(1, RuntimeData.Logic);
            RuntimeData.Insight = Mathf.Max(1, RuntimeData.Insight);
            RuntimeData.Resilience = Mathf.Max(1, RuntimeData.Resilience);
        }

        private void ApplyPersonalityStat(PersonalityTag tag)
        {
            switch (tag)
            {
                case PersonalityTag.Emotional:
                    RuntimeData.Perception += 1;
                    RuntimeData.Resilience -= 1;
                    break;
                case PersonalityTag.Rational:
                    RuntimeData.Logic += 1;
                    RuntimeData.Perception -= 1;
                    break;
                case PersonalityTag.Idealistic:
                    RuntimeData.Insight += 1;
                    RuntimeData.Logic -= 1;
                    break;
                case PersonalityTag.Practical:
                    RuntimeData.Resilience += 1;
                    RuntimeData.Insight -= 1;
                    break;
            }
        }
    }
}