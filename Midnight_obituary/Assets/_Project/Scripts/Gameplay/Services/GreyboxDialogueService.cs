using System;
using System.Linq;
using MidnightObituary.Core;
using MidnightObituary.Gameplay.Definitions;

namespace MidnightObituary.Gameplay.Services
{
    public sealed class GreyboxDialogueService
    {
        private readonly DialogueTreeDefinition _tree;
        private readonly PlayerState _player;
        private readonly NpcRuntimeState _npc;
        private readonly CallSessionState _session;
        private readonly PersonalityRuleService _personalityRuleService;
        private readonly DiceService _diceService;
        private readonly CallCounterService _callCounterService;
        private readonly EndingService _endingService;

        public GreyboxDialogueService(
            DialogueTreeDefinition tree,
            PlayerState player,
            NpcRuntimeState npc,
            CallSessionState session,
            PersonalityRuleService personalityRuleService,
            DiceService diceService,
            CallCounterService callCounterService,
            EndingService endingService)
        {
            _tree = tree;
            _player = player;
            _npc = npc;
            _session = session;
            _personalityRuleService = personalityRuleService;
            _diceService = diceService;
            _callCounterService = callCounterService;
            _endingService = endingService;
        }

        public DialogueNodeDefinition GetCurrentNode()
        {
            return _tree.Nodes.FirstOrDefault(node => node.NodeId == _session.DialogueNodeId);
        }

        public EndingType SelectChoice(string choiceId)
        {
            // SYS_DIALOG_003
            DialogueNodeDefinition node = GetCurrentNode();
            DialogueChoiceDefinition choice = node.Choices.FirstOrDefault(item => item.ChoiceId == choiceId);

            if (choice == null)
            {
                throw new InvalidOperationException($"Choice not found: {choiceId}");
            }

            // SYS_DIALOG_004 / SYS_DIALOG_005
            _personalityRuleService.ResolveChoice(_player, _npc, choice);

            string nextNodeId = choice.NextNodeId;

            if (choice.HasDiceCheck)
            {
                // SYS_DICE_001 / SYS_DICE_002 / SYS_DICE_003
                var check = new DiceCheckDefinition
                {
                    StatType = choice.DiceStat,
                    Difficulty = choice.Difficulty,
                    ExtraBonus = choice.ExtraBonus
                };

                DiceRollResult result = _diceService.Roll(check, _player.Stats);
                nextNodeId = result.IsSuccess ? choice.SuccessNodeId : choice.FailureNodeId;
            }

            if (choice.CountsAsPlayerSpeech)
            {
                // SYS_COUNT_002
                _callCounterService.AddPlayerSpeech();
                _session.PlayerSpeechCount = _callCounterService.CurrentCount;

                int penalty = _callCounterService.ConsumePendingStressPenalty();
                if (penalty > 0)
                {
                    _player.Stress = Math.Min(_player.StressMax, _player.Stress + penalty);
                }
            }

            if (_callCounterService.HasReachedDelayTarget())
            {
                // SYS_COUNT_003 / SYS_END_003
                _session.DelayRewriteReady = true;
            }

            if (!string.IsNullOrEmpty(nextNodeId))
            {
                _session.DialogueNodeId = nextNodeId;
            }

            DialogueNodeDefinition nextNode = GetCurrentNode();
            if (nextNode != null && nextNode.IsDeepRedemptionGate && _npc.Breakdown <= 0)
            {
                // SYS_END_001 / SYS_END_002
                _session.DeepRedemptionReady = true;
            }

            return _endingService.EvaluateEnding(_session, _player, _npc);
        }
    }
}