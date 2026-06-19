using UnityEngine;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Player;

namespace ObituaryTomorrow.Gameplay.Items
{
    public sealed class CigaretteSystem : MonoBehaviour
    {
        [SerializeField] private PlayerManager playerManager;

        public int Count => RuntimeData != null ? RuntimeData.CigaretteCount : 0;
        public int MaxCount => RuntimeData != null ? RuntimeData.MaxCigaretteCount : 0;

        private PlayerRuntimeData RuntimeData => playerManager != null ? playerManager.RuntimeData : null;
        private bool hasPendingUseRequest;

        private void Awake()
        {
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }
        }

        public OperationResult CanUseCigarette()
        {
            if (playerManager == null || playerManager.RuntimeData == null)
            {
                return OperationResult.Fail("PlayerManager is missing.");
            }

            if (Count <= 0)
            {
                return OperationResult.Fail("No cigarettes left.");
            }

            if (playerManager.CurrentStress <= 0)
            {
                return OperationResult.Fail("Stress is already zero.");
            }

            return OperationResult.Ok("Cigarette can be used.");
        }

        public OperationResult RequestUseCigarette()
        {
            OperationResult condition = CanUseCigarette();

            if (!condition.Success)
            {
                hasPendingUseRequest = false;
                return condition;
            }

            hasPendingUseRequest = true;
            return OperationResult.Ok("Cigarette use requested.");
        }

        public StatChangeResult ConfirmUseCigarette()
        {
            if (!hasPendingUseRequest)
            {
                OperationResult requestResult = RequestUseCigarette();

                if (!requestResult.Success)
                {
                    return new StatChangeResult(
                        false,
                        Count,
                        Count,
                        0,
                        Count <= 0,
                        Count >= MaxCount,
                        StatChangeReason.CigaretteUse);
                }
            }

            int oldCount = Count;
            int newCount = Mathf.Clamp(oldCount - 1, 0, MaxCount);

            RuntimeData.CigaretteCount = newCount;
            hasPendingUseRequest = false;

            GameEventBus.RaiseCigaretteChanged(
                new CigaretteChangedEventArgs(oldCount, newCount, MaxCount, StatChangeReason.CigaretteUse));

            playerManager.RequestStressChange(new StressChangeRequest(
                -1,
                StatChangeReason.CigaretteUse,
                "CigaretteSystem.ConfirmUseCigarette",
                false));

            return new StatChangeResult(
                oldCount != newCount,
                oldCount,
                newCount,
                newCount - oldCount,
                newCount <= 0,
                newCount >= MaxCount,
                StatChangeReason.CigaretteUse);
        }

        public StatChangeResult AddCigarette(int amount, StatChangeReason reason)
        {
            if (playerManager == null || playerManager.RuntimeData == null)
            {
                return new StatChangeResult(false, 0, 0, 0, true, false, reason);
            }

            int oldCount = Count;
            int newCount = Mathf.Clamp(oldCount + amount, 0, MaxCount);

            RuntimeData.CigaretteCount = newCount;

            if (oldCount != newCount)
            {
                GameEventBus.RaiseCigaretteChanged(
                    new CigaretteChangedEventArgs(oldCount, newCount, MaxCount, reason));
            }

            return new StatChangeResult(
                oldCount != newCount,
                oldCount,
                newCount,
                newCount - oldCount,
                newCount <= 0,
                newCount >= MaxCount,
                reason);
        }

        public StatChangeResult SetCigaretteCount(int value, StatChangeReason reason)
        {
            if (playerManager == null || playerManager.RuntimeData == null)
            {
                return new StatChangeResult(false, 0, 0, 0, true, false, reason);
            }

            int oldCount = Count;
            int newCount = Mathf.Clamp(value, 0, MaxCount);

            RuntimeData.CigaretteCount = newCount;

            if (oldCount != newCount)
            {
                GameEventBus.RaiseCigaretteChanged(
                    new CigaretteChangedEventArgs(oldCount, newCount, MaxCount, reason));
            }

            return new StatChangeResult(
                oldCount != newCount,
                oldCount,
                newCount,
                newCount - oldCount,
                newCount <= 0,
                newCount >= MaxCount,
                reason);
        }
    }
}