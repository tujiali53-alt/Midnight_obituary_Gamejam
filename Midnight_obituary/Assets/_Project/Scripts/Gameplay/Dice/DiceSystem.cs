using UnityEngine;
using ObituaryTomorrow.Core;
using ObituaryTomorrow.Gameplay.Player;

namespace ObituaryTomorrow.Gameplay.Dice
{
    public sealed class DiceSystem : MonoBehaviour
    {
        [SerializeField] private PlayerManager playerManager;

        private void Awake()
        {
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }
        }

        public DiceResult RollCheck(DiceCheckRequest request)
        {
            int positiveD6 = Random.Range(1, 7);
            int negativeD6 = Random.Range(1, 7);
            int attributeBonus = playerManager != null ? playerManager.GetAttribute(request.AttributeType) : 0;
            int total = positiveD6 - negativeD6 + attributeBonus + request.FlatBonus;
            bool success = total >= request.Difficulty;

            DiceResult result = new DiceResult(
                request.CheckId,
                request.AttributeType,
                request.Difficulty,
                positiveD6,
                negativeD6,
                attributeBonus,
                request.FlatBonus,
                total,
                success);

            GameEventBus.RaiseDiceRolled(
                new DiceRolledEventArgs(request.CheckId, positiveD6, negativeD6, total, success));

            return result;
        }
    }

    public readonly struct DiceCheckRequest
    {
        public string CheckId { get; }
        public PlayerAttributeType AttributeType { get; }
        public int Difficulty { get; }
        public int FlatBonus { get; }

        public DiceCheckRequest(string checkId, PlayerAttributeType attributeType, int difficulty, int flatBonus = 0)
        {
            CheckId = string.IsNullOrWhiteSpace(checkId) ? "DICE_Check" : checkId;
            AttributeType = attributeType;
            Difficulty = difficulty;
            FlatBonus = flatBonus;
        }
    }

    public readonly struct DiceResult
    {
        public string CheckId { get; }
        public PlayerAttributeType AttributeType { get; }
        public int Difficulty { get; }
        public int PositiveD6 { get; }
        public int NegativeD6 { get; }
        public int AttributeBonus { get; }
        public int FlatBonus { get; }
        public int Total { get; }
        public bool Success { get; }

        public DiceResult(
            string checkId,
            PlayerAttributeType attributeType,
            int difficulty,
            int positiveD6,
            int negativeD6,
            int attributeBonus,
            int flatBonus,
            int total,
            bool success)
        {
            CheckId = checkId;
            AttributeType = attributeType;
            Difficulty = difficulty;
            PositiveD6 = positiveD6;
            NegativeD6 = negativeD6;
            AttributeBonus = attributeBonus;
            FlatBonus = flatBonus;
            Total = total;
            Success = success;
        }
    }
}
