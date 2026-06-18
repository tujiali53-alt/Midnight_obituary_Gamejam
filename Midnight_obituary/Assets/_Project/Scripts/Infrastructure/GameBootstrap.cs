using MidnightObituary.Core;
using MidnightObituary.Gameplay.Services;
using MidnightObituary.Infrastructure;
using UnityEngine;

namespace MidnightObituary.Infrastructure
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GreyboxMissionDatabase database;

        public static GameBootstrap Instance { get; private set; }

        public GreyboxMissionDatabase Database => database;
        public PlayerService PlayerService { get; private set; }
        public PersonalityRuleService PersonalityRuleService { get; private set; }
        public DiceService DiceService { get; private set; }
        public CallCounterService CallCounterService { get; private set; }
        public EndingService EndingService { get; private set; }
        public GameFlowService GameFlowService { get; private set; }

        public MissionState CurrentMissionState { get; private set; }
        public NpcRuntimeState CurrentNpcState { get; private set; }
        public CallSessionState CurrentCallSession { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            PlayerService = new PlayerService();
            PersonalityRuleService = new PersonalityRuleService();
            DiceService = new DiceService(new UnityRandomProvider());
            CallCounterService = new CallCounterService();
            EndingService = new EndingService();
            GameFlowService = new GameFlowService();

            // SYS_GAME_001
            PlayerService.InitializeNewPlayer(database != null && database.GameConfig != null
                ? database.GameConfig.PlayerInitialConfig
                : null);

            CurrentMissionState = new MissionState();
            if (database != null && database.Mission != null)
            {
                CurrentMissionState.MissionId = database.Mission.MissionId;
            }
        }

        public void PublishAndConfirmMission()
        {
            // SYS_MISSION_001 / SYS_MISSION_003
            CurrentMissionState.IsPublished = true;
            CurrentMissionState.IsConfirmed = true;
        }

        public void StartCallSession()
        {
            if (database == null || database.Npc == null || database.Mission == null)
            {
                Debug.LogError("Greybox database is missing mission or NPC data.");
                return;
            }

            // SYS_NPC_001 / SYS_NPC_005
            CurrentNpcState = new NpcRuntimeState
            {
                NpcId = database.Npc.NpcId,
                PersonalityTag = database.Npc.PersonalityTag,
                Breakdown = 1,
                BreakdownMax = database.Npc.BreakdownMax,
                DelayTargetCount = database.Npc.DelayTargetCount
            };

            CurrentCallSession = new CallSessionState
            {
                MissionId = database.Mission.MissionId,
                NpcId = database.Npc.NpcId,
                DialogueNodeId = database.DialogueTree.RootNodeId
            };

            CallCounterService.Initialize(
                database.Npc,
                database.GameConfig != null ? database.GameConfig.CallCounterConfig : null);
        }
    }
}