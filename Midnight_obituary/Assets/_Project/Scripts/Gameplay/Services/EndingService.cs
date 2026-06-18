using MidnightObituary.Core;

namespace MidnightObituary.Gameplay.Services
{
    public sealed class EndingService : IEndingService
    {
        public EndingType EvaluateEnding(CallSessionState session, PlayerState player, NpcRuntimeState npc)
        {
            if (player != null && player.IsBrokenDown)
            {
                return EndingType.PlayerBreakdown;
            }

            if (npc != null && npc.HasHungUp)
            {
                return EndingType.CallFailed;
            }

            if (session != null && session.DeepRedemptionReady)
            {
                return EndingType.DeepRedemption;
            }

            if (session != null && session.DelayRewriteReady)
            {
                return EndingType.DelayRewrite;
            }

            return EndingType.None;
        }

        public MissionResult BuildMissionResult(EndingType endingType, string missionId)
        {
            switch (endingType)
            {
                case EndingType.DeepRedemption:
                    return new MissionResult(missionId, endingType, true, 1, 0, ObituaryState.Removed, false);
                case EndingType.DelayRewrite:
                    return new MissionResult(missionId, endingType, true, 0, 0, ObituaryState.Faded, false);
                case EndingType.CallFailed:
                    return new MissionResult(missionId, endingType, false, 0, -1, ObituaryState.Darkened, false);
                case EndingType.PlayerBreakdown:
                    return new MissionResult(missionId, endingType, false, 0, 0, ObituaryState.Darkened, true);
                default:
                    return new MissionResult(missionId, EndingType.None, false, 0, 0, ObituaryState.Pending, false);
            }
        }
    }
}
