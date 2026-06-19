# 《明日讣告》Unity项目结构与架构

Unity版本：Unity 2023.2.20  
对话插件：articy:draft X Importer  
动画来源：Spine 4.2，导出 Json / Atlas / Texture  
架构目标：用轻量分层保证21天内可集成、可调试、可扩内容。P0优先服务完整Demo闭环，P1/P2只在闭环稳定后补齐。

## 架构分层

| 层级 | 作用 | 主要目录 | 负责人 |
| --- | --- | --- | --- |
| Core | 全局状态、流程切换、事件、服务注册 | Scripts/Core | 主程 |
| Data | ScriptableObject、运行时数据、配置映射；Player/Item字段由主程定，NPC/Dialogue/Ending字段由副程定 | Scripts/Data / Data | 双人 |
| Gameplay | Player/Item由主程负责，NPC/Rule/Dice/Counter/Ending判断由副程负责 | Scripts/Gameplay | 双人 |
| Dialogue | articy导入适配、对话运行时、节点跳转、效果执行 | Scripts/Dialogue | 双人 |
| UI | 主菜单、HUD、报纸、黄页、通话、结算、成就 | Scripts/UI | 主程 |
| Animation | Spine控制、动画事件、电话/骰子/抽烟/光影演出 | Scripts/Animation | 主程 |
| Integration | 场景Prefab、资源绑定、跨系统串联 | Prefabs / Scenes | 主程 |
| Debug | 调试面板、快捷跳转、强制结局、数值修改 | Scripts/Debug | 双人 |

## 推荐目录结构

```text
Assets/
    Art/
      Characters/
      Backgrounds/
      UI/
      Props/
      Spine/
        Characters/
        Props/
    Audio/
      BGM/
      SFX/
      Ambience/
    Data/
      Dialogue/
        Articy/
        RuntimeExports/
      NPC/
      Missions/
      Items/
      Endings/
      Achievements/
    Materials/
    Prefabs/
      Core/
      UI/
      Dialogue/
      Characters/
      Props/
      Effects/
    Scenes/
      Boot.unity
      MainMenu.unity
      MainRoom.unity
      Call.unity
    Scripts/
      Core/
        GameManager.cs
        GameState.cs
        GameSessionData.cs
        SceneFlowController.cs
        GameEventBus.cs
      Data/
        PlayerRuntimeData.cs
        NPCRuntimeData.cs
        NPCDefinition.cs
        MissionDefinition.cs
        DialogueDefinition.cs
        EndingDefinition.cs
      Dialogue/
        DialogueController.cs
        DialogueNodeRuntime.cs
        DialogueChoiceRuntime.cs
        DialogueConditionEvaluator.cs
        DialogueEffectExecutor.cs
        ArticyDialogueAdapter.cs
      Gameplay/
        Player/
          PlayerManager.cs
          PersonalitySystem.cs
          StressSystem.cs
        NPC/
          NPCManager.cs
          NPCBreakdownSystem.cs
        Rules/
          RuleSystem.cs
        Dice/
          DiceSystem.cs
          DiceResult.cs
        Call/
          PhoneController.cs
          CallCounterSystem.cs
        Items/
          CigaretteSystem.cs
        Mission/
          ObituarySystem.cs
          MissionSystem.cs
        Ending/
          EndingEvaluator.cs
          ResultSystem.cs
        Save/
          SaveSystem.cs
        Achievement/
          AchievementSystem.cs
      UI/
        Common/
          UIView.cs
          UIManager.cs
          UIBlocker.cs
        MainMenu/
        MainRoom/
        Obituary/
        YellowPages/
        Dialogue/
        Result/
        Achievement/
      Animation/
        SpineAnimationController.cs
        AnimationEventRouter.cs
        PhoneAnimationController.cs
        DiceAnimationController.cs
        CigaretteAnimationController.cs
        ObituaryAnimationController.cs
      Audio/
        AudioCue.cs
        AudioManager.cs
      Debug/
        DebugPanel.cs
        DebugCommand.cs
    Settings/
    Shaders/
    ThirdParty/
      ArticyDraftXImporter/
      Spine/
```

## 命名规范

| 类型 | 前缀 | 示例 |
| --- | --- | --- |
| 角色 | CHR_ | CHR_Lena_v2.1 |
| 场景背景 | BG_ | BG_MainRoom_Night_v1.0 |
| UI资源 | UI_ | UI_ObituaryPanel_v1.3 |
| 卡牌 | CARD_ | CARD_Personality_Rational_v1.0 |
| 物品 | ITEM_ | ITEM_Cigarette_v1.0 |
| BGM | BGM_ | BGM_RainJazzLoop_v1.0 |
| 音效 | SFX_ | SFX_DiceRoll_v1.0 |
| 动画 | ANI_ | ANI_DiceRoll_v3.2 |
| 预制体 | PF_ | PF_DialogueView_v1.0 |
| 脚本 | 按职责命名 | DiceSystem.cs / DialogueController.cs |

版本号遵循 `资源名_v循环.版本`，例如 `ANI_DiceRoll_v3.2` 表示循环3期间的第二版骰子动画。

## 核心运行流程

```text
Boot
  -> GameManager 初始化服务
  -> MainMenu 显示主菜单
  -> StartNewGame 创建 GameSessionData
  -> MainRoom 进入主小屋
  -> Obituary 打开当前讣告
  -> Mission 发布并确认任务
  -> YellowPages 检索号码
  -> Phone 播放拨号动画
  -> Call 加载NPC、对话树、计数和UI
  -> Dialogue 选择选项、执行规则、骰子、计数
  -> EndingEvaluator 监听压力/崩溃/计数/深度解析
  -> ResultSystem 应用奖惩
  -> ResultView 显示结算
  -> MainRoom 刷新报纸状态
```

## GameLoop状态机

| GameState | 进入条件 | 主要系统 | 退出条件 |
| --- | --- | --- | --- |
| Boot | 游戏启动 | GameManager / ServiceRegistry | 初始化完成 |
| MainMenu | Boot完成或返回标题 | Menu UI / SaveSystem | 新游戏、继续游戏、退出 |
| Opening | 新游戏开始 | Timeline / Animation / UIBlocker | 开场结束 |
| MainRoom | 开场结束或结算返回 | Obituary / Mission / Item / UI | 打开报纸、黄页、电话 |
| ObituaryView | 点击报纸 | ObituarySystem / Obituary UI | 关闭报纸或确认任务 |
| YellowPagesView | 点击黄页 | MissionSystem / YellowPages UI | 选择号码并开始拨号 |
| Dialing | 点击拨号 | PhoneController / PhoneAnimation | 动画结束 |
| InCall | 电话接通 | Dialogue / Dice / Rule / Counter / Player / NPC | 触发任意结局 |
| Result | 通话结束 | EndingEvaluator / ResultSystem / Result UI | 结算确认 |
| GameOver | 玩家压力到上限 | Ending / Result / Save | 返回菜单或重新开始 |

## 数据流

| 数据 | 来源 | 运行时使用者 | 保存时机 |
| --- | --- | --- | --- |
| 玩家人格和属性 | 新游戏随机 + Personality配置 | PlayerManager / RuleSystem / DiceSystem | 新游戏、结算后 |
| 玩家压力和香烟 | GameSessionData | StressSystem / CigaretteSystem / UI | 数值变化、结算后 |
| NPC配置 | NPCDefinition / articy字段 | NPCManager / Dialogue / EndingEvaluator | 不保存静态配置 |
| 对话树 | articy:draft X Importer | DialogueController | 不保存静态配置 |
| 当前任务 | MissionDefinition | Obituary / YellowPages / Phone | 接受任务、结算后 |
| 通话计数 | CallCounterSystem | EndingEvaluator / UI | 通话内临时，结算后归档 |
| 结局结果 | EndingEvaluator | ResultSystem / Result UI / Achievement | 结算后 |

## articy对接规则

| 字段 | 用途 | 程序处理 |
| --- | --- | --- |
| DialogueId | 对话树唯一ID | ArticyDialogueAdapter映射到DialogueDefinition |
| NodeId | 节点唯一ID | DialogueController按NodeId跳转 |
| Speaker | 说话人 | UI显示角色名或旁白 |
| Text | 正文 | DialogueView显示 |
| Choices | 玩家选项 | 转换为DialogueChoiceRuntime |
| Tags | 人格/属性标签 | RuleSystem判断玩家和NPC匹配 |
| DiceCheck | 是否触发骰子 | DiceSystem执行2D6判定 |
| Difficulty | 骰子阈值 | DiceSystem计算成功/失败 |
| Effects | 数值效果 | DialogueEffectExecutor修改压力、崩溃、计数 |
| NextNode | 默认下一节点 | DialogueController跳转 |
| SuccessNode / FailNode | 骰子分支 | Dice_ApplyBranch后跳转 |

## Spine 4.2接入规则

| 资源 | 路径 | 接入方式 |
| --- | --- | --- |
| Json / Atlas / Texture | Art/Spine | 保持美术导出文件同名同目录 |
| SkeletonDataAsset | Art/Spine对应目录 | 由主程建立并绑定材质 |
| SkeletonAnimation Prefab | Prefabs/Characters 或 Prefabs/Props | 业务脚本只调用动画控制器 |
| 动画事件 | AnimationEventRouter | 将动画结束、关键帧反馈转为程序事件 |
| UI动效 | UI或Props Prefab | 需要和UIBlocker配合锁定底层交互 |

## 依赖方向

```text
UI -> Core事件 / Gameplay公开接口
Animation -> Core事件 / Audio事件
Dialogue -> Gameplay规则与数值接口
Gameplay -> Data / Core事件
Core -> 不依赖业务模块
Data -> 不依赖场景对象
```

禁止依赖：

| 禁止项 | 原因 |
| --- | --- |
| UI直接修改PlayerRuntimeData | 避免数值变化绕过校验和反馈 |
| Dialogue直接播放具体Spine动画 | 对话只发语义事件，动画模块决定表现 |
| Gameplay脚本直接查找UI对象 | 保持逻辑可测试，减少场景引用丢失 |
| articy导入数据直接散落在业务脚本 | 统一通过Adapter转换，方便后期换字段 |

## 最小可玩版本架构范围

| 必须包含 | 可降级 |
| --- | --- |
| GameManager / GameSessionData / GameState | 多场景可先用单场景Canvas切换模拟 |
| Player / Item + NPC / Rule / Dice / Counter | Debug面板可先替代部分正式UI；主程负责Player/Item，副程负责NPC/Rule/Dice/Counter |
| DialogueController + articy适配 | articy字段不稳定时可临时JSON导入，但接口不变 |
| Obituary / YellowPages / Phone / Result UI | 成就UI、继续游戏可后置到P1 |
| Spine动画控制接口 | 资源未完成时用占位动画，但事件名先固定 |

