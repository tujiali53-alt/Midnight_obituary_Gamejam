# 《午夜讣告 / 明日讣告》程序开发 API

> 本文档是本项目的程序协作与 AI 编程规范。Cursor、队友或任何自动化代理在新增脚本、Prefab、数据表、UI 绑定和系统逻辑前，必须先阅读并遵守本文档。
>
> 当前项目文件夹使用 `Midnight_obituary_Gamejam`，策划案内显示名为《明日讣告》/ The Obituary of Tomorrow。代码层统一使用 `MidnightObituary` 作为命名空间与项目代号，中文标题只作为 UI 显示文本与发行配置。

---

## 1. 项目目标

### 1.1 Demo 闭环

21 天 Gamejam 的 P0 目标是完成一个可通关、可调参、可扩展的文字 CRPG Demo：

1. 开场收到未来讣告晚报。
2. 玩家阅读报纸并确认救援目标。
3. 通过黄页检索目标座机号码。
4. 拨号进入电话对话关卡。
5. 通过人格标签、压力值、NPC 崩溃值、骰子判定、通话计数形成选择后果。
6. 触发三类结局：深度救赎、拖延改写、劝告失败。
7. 结算奖惩后返回主小屋或进入最终演出。

### 1.2 工程原则

- 数据驱动优先：NPC、任务、对话、结局、数值阈值必须可通过数据资产调整，禁止把具体角色文本和数值硬编码在流程脚本里。
- P0 先闭环：任何 P1/P2 功能不得阻塞 P0 主流程。
- 逻辑与表现分离：玩法计算写在纯 C# 服务或模型中；Unity `MonoBehaviour` 只负责场景生命周期、输入、动画、UI 绑定。
- ID 稳定：需求 ID、NPC ID、任务 ID、对话节点 ID 一旦进入数据表，不允许因为显示文本修改而变更。
- 中文只进内容层：脚本类名、变量名、资源 ID 使用英文；中文文案放在数据资产、表格或本地化字段。
- 小文件优先：单个脚本超过约 300 行时，需要优先拆分职责，而不是继续堆逻辑。
- 可测试优先：骰子、人格匹配、压力变化、崩溃变化、结算规则必须能用 EditMode Test 覆盖。

---

## 2. Unity 项目结构

如项目尚未创建 Unity 结构，根目录应按以下结构组织。Unity 版本以团队 `ProjectSettings/ProjectVersion.txt` 为准；若尚未确定，选择团队统一的 LTS 版本。

```text
Assets/
  _Project/
    Art/
      Characters/
      Backgrounds/
      UI/
      Props/
      Materials/
      Fonts/
    Audio/
      BGM/
      SFX/
      Ambience/
      Mixers/
    Data/
      Configs/
      Dialogues/
      Missions/
      NPCs/
      Obituaries/
      Tutorials/
      Achievements/
    Prefabs/
      Gameplay/
      UI/
      Scene/
      Characters/
      VFX/
    Scenes/
      SCN_Boot.unity
      SCN_MainMenu.unity
      SCN_Opening.unity
      SCN_MainRoom.unity
      SCN_Call.unity
      SCN_Ending.unity
    Scripts/
      Core/
      Gameplay/
      UI/
      Presentation/
      Infrastructure/
      Editor/
    Settings/
    Tests/
      EditMode/
      PlayMode/
Docs/
  Planning/
  Requirements/
  Tech/
```

### 2.1 Assembly Definition

P0 阶段推荐先建立 3 个 asmdef，控制编译边界即可，不做过度拆分。

```text
Assets/_Project/Scripts/MidnightObituary.Runtime.asmdef
Assets/_Project/Scripts/Editor/MidnightObituary.Editor.asmdef
Assets/_Project/Tests/EditMode/MidnightObituary.Tests.EditMode.asmdef
```

命名空间统一：

```csharp
namespace MidnightObituary.Core
namespace MidnightObituary.Gameplay
namespace MidnightObituary.UI
namespace MidnightObituary.Presentation
namespace MidnightObituary.Infrastructure
namespace MidnightObituary.Editor
```

---

## 3. 资源命名规范

### 3.1 通用资源命名

资源文件命名格式：

```text
<PREFIX>_<ModuleOrOwner>_<Name>_C<Cycle>_v<Major>.<Minor>
```

示例：

```text
CHR_Lena_Silhouette_C2_v1.0.png
UI_Call_ChoiceButton_C2_v1.1.prefab
SFX_Dice_Roll_C3_v1.0.wav
PF_UI_ResultPopup_C3_v1.0.prefab
SO_NPC_Lena_C2_v1.0.asset
```

规则：

- 脚本文件不带版本号，例如 `DiceService.cs`。
- Scene 文件不带版本号，例如 `SCN_MainRoom.unity`。
- 同一资源更新覆盖原文件时只递增版本号，不新增 `final_final`、`new`、`copy`。
- 英文名优先；必须拼音时使用简短拼音，不使用空格。

### 3.2 Prefix 表

| 类型 | Prefix | 示例 |
| --- | --- | --- |
| 场景 | `SCN` | `SCN_MainRoom.unity` |
| 预制体 | `PF` | `PF_UI_CallPanel_C2_v1.0.prefab` |
| ScriptableObject | `SO` | `SO_NPC_Lena_C2_v1.0.asset` |
| 角色图 | `CHR` | `CHR_Lena_Silhouette_C2_v1.0.png` |
| 背景 | `BG` | `BG_MainRoom_Night_C2_v1.0.png` |
| UI 图 | `UI` | `UI_StressBar_Frame_C2_v1.0.png` |
| 卡牌 | `CARD` | `CARD_Personality_Feeling_C2_v1.0.png` |
| 物品 | `ITEM` | `ITEM_Cigarette_C2_v1.0.png` |
| 材质 | `MAT` | `MAT_NeonMagenta_C2_v1.0.mat` |
| 动画片段 | `ANI` | `ANI_DiceRoll_C3_v1.0.anim` |
| Animator | `AC` | `AC_DicePanel_C3_v1.0.controller` |
| 音乐 | `BGM` | `BGM_LateNightJazz_C2_v1.0.wav` |
| 音效 | `SFX` | `SFX_Phone_Dial_C2_v1.0.wav` |
| 字体 | `FONT` | `FONT_Newspaper_C2_v1.0.ttf` |

---

## 4. 代码命名规范

### 4.1 C# 基础风格

- 类型、接口、枚举、方法、属性使用 `PascalCase`。
- 私有字段使用 `_camelCase`。
- 局部变量和参数使用 `camelCase`。
- 常量使用 `PascalCase`。
- 接口以 `I` 开头，例如 `IDiceService`。
- 事件命名使用过去式或变化事实，例如 `StressChanged`、`MissionCompleted`。
- `MonoBehaviour` 按职责后缀命名：`View`、`Controller`、`Binder`、`Animator`、`Installer`。
- 纯逻辑类按职责后缀命名：`Service`、`System`、`Resolver`、`Repository`、`State`、`Definition`。

示例：

```csharp
public sealed class DiceService : IDiceService
{
    private readonly IRandomProvider _randomProvider;

    public DiceRollResult Roll(DiceCheckDefinition check, PlayerStats stats)
    {
        // ...
    }
}
```

### 4.2 需求 ID 与代码方法

需求表里的 `Menu_StartNewGame`、`Dice_CheckResult` 是策划/程序沟通名，不强制作为 C# 方法名。代码中使用 C# 风格方法名，并在注释、数据或常量中保留需求 ID。

```csharp
public static class RequirementIds
{
    public const string MenuStartNewGame = "SYS_MENU_002";
    public const string DiceCheckResult = "SYS_DICE_002";
}
```

```csharp
// SYS_MENU_002
public void StartNewGame()
{
    _gameFlow.StartNewGame();
}
```

---

## 5. 系统总览

### 5.1 P0 必做系统

| 系统 | 职责 | 主要需求 ID |
| --- | --- | --- |
| Game Flow | 启动、新游戏、场景/流程状态切换 | `SYS_GAME_*`, `SYS_MENU_*` |
| Player | 玩家四维属性、人格卡、压力值 | `SYS_PLAYER_*` |
| Personality Rule | 判断玩家/NPC 人格标签匹配 | `SYS_RULE_*` |
| Item | 香烟数量、使用条件、压力减少 | `SYS_ITEM_*` |
| Obituary & Mission | 报纸、任务发布、讣告状态变化 | `SYS_OBIT_*`, `SYS_MISSION_*` |
| Yellow Pages & Phone | 黄页查号、拨号、进入通话 | `SYS_CALL_*`, `SYS_PHONE_*` |
| NPC | NPC 固定人格、崩溃值、拖延阈值 | `SYS_NPC_*` |
| Dialogue | 对话树、选项、节点跳转、标签结算 | `SYS_DIALOG_*` |
| Dice | 2D6 正负骰判定 | `SYS_DICE_*` |
| Call Counter | 玩家发言计数、拖延结局、长通话压力 | `SYS_COUNT_*` |
| Ending & Result | 三类结局、奖惩、返回主小屋 | `SYS_END_*`, `SYS_RESULT_*` |
| UI Feedback | 压力/崩溃/骰子/结算反馈 | `SYS_FEEDBACK_*`, `SYS_UI_*` |

### 5.2 P1 / P2 延后系统

| 优先级 | 系统 | 说明 |
| --- | --- | --- |
| P1 | Tutorial | 首次引导、强制抽烟、报纸/黄页引导 |
| P1 | Save | 结算后保存玩家状态、任务状态、讣告状态 |
| P1 | Achievement | 成就初始化、检测、解锁、Toast、UI |
| P2 | Credits | 制作人员界面 |
| P2 | 高级愿望单 | 极限减压对话、复杂多标签推理、更多演出分支 |

---

## 6. 核心数据模型

所有玩法配置优先使用 ScriptableObject。对话文本量变大后，可以由 JSON/CSV 导入生成 ScriptableObject，但运行时代码仍读取统一的 Definition 类型。

### 6.1 枚举

```csharp
public enum PersonalityTag
{
    Feeling,
    Rational,
    Idealist,
    Pragmatic
}

public enum StatType
{
    Perception,
    Logic,
    Insight,
    Resilience
}

public enum EndingType
{
    None,
    DeepRedemption,
    DelayRewrite,
    CallFailed,
    PlayerBreakdown
}

public enum GameFlowState
{
    Boot,
    MainMenu,
    Opening,
    MainRoom,
    Newspaper,
    YellowPages,
    Dialing,
    Call,
    Result,
    Ending
}
```

### 6.2 Definition 资产

| 类型 | 路径 | 职责 |
| --- | --- | --- |
| `GameConfig` | `Data/Configs` | 全局初始值、场景引用、P0 数值默认值 |
| `PersonalityDefinition` | `Data/Configs/Personalities` | 四象限标签、显示名、描述、属性修正 |
| `PlayerInitialConfig` | `Data/Configs` | 玩家基础四维、初始压力上限、初始香烟 |
| `NpcDefinition` | `Data/NPCs` | NPC ID、人格、崩溃上限、初始崩溃、拖延阈值 |
| `MissionDefinition` | `Data/Missions` | 任务 ID、NPC 引用、报纸条目、奖励结算 |
| `ObituaryDefinition` | `Data/Obituaries` | 讣告显示内容、普通新闻替换内容、状态 |
| `YellowPageEntryDefinition` | `Data/Missions` | 姓名、地址、电话号码、关联 NPC |
| `DialogueTreeDefinition` | `Data/Dialogues` | 对话节点、选项、判定、跳转 |
| `EndingDefinition` | `Data/Missions` | 结局类型、奖惩、展示文案、讣告变化 |
| `AudioCueDefinition` | `Data/Configs/Audio` | 音效 ID、音量、Mixer Group |

### 6.3 Runtime State

Runtime State 只保存运行中会变化的数据，不直接保存 Definition。

```csharp
public sealed class GameRuntimeState
{
    public GameFlowState FlowState { get; set; }
    public PlayerState Player { get; set; }
    public string CurrentMissionId { get; set; }
    public Dictionary<string, MissionState> Missions { get; } = new();
}

public sealed class PlayerState
{
    public PlayerStats Stats { get; set; }
    public HashSet<PersonalityTag> PersonalityTags { get; } = new();
    public int Stress { get; set; }
    public int StressMax { get; set; }
    public int CigaretteCount { get; set; }
}

public sealed class NpcRuntimeState
{
    public string NpcId { get; set; }
    public PersonalityTag PersonalityTag { get; set; }
    public int Breakdown { get; set; }
    public int BreakdownMax { get; set; }
    public int DelayTargetCount { get; set; }
}

public sealed class CallSessionState
{
    public string MissionId { get; set; }
    public string NpcId { get; set; }
    public string CurrentNodeId { get; set; }
    public int PlayerSpeechCount { get; set; }
    public EndingType PendingEnding { get; set; }
}
```

---

## 7. 服务层 API

服务层是纯 C# 或轻量 Unity 服务，供 UI、场景控制器和测试调用。

### 7.1 Game Flow

```csharp
public interface IGameFlowService
{
    GameRuntimeState RuntimeState { get; }
    event Action<GameFlowState> FlowStateChanged;

    void Boot();
    void StartNewGame();
    void EnterMainRoom();
    void OpenNewspaper();
    void OpenYellowPages();
    void StartDialing(string missionId);
    void StartCall(string missionId);
    void ShowResult(MissionResult result);
    void RestartGame();
}
```

规则：

- `StartNewGame()` 只初始化数据，不直接播放 UI 动画。
- 场景切换由 `SceneFlowController` 监听 `FlowStateChanged`。
- UI 打开/关闭由对应 UI Controller 监听状态，不允许 UI 自己改全局流程状态，必须调用服务。

### 7.2 Player

```csharp
public interface IPlayerService
{
    event Action<int, int> StressChanged;
    event Action<int> CigaretteChanged;
    event Action PlayerBreakdownTriggered;

    PlayerState InitializeNewPlayer(PlayerInitialConfig config);
    void ApplyPersonalityCards(IReadOnlyList<PersonalityDefinition> personalities);
    void ChangeStress(int delta, StressChangeReason reason);
    bool CanUseCigarette();
    bool TryUseCigarette();
    void AddCigarette(int amount);
}
```

标准数值：

- 基础四维：感知、逻辑、敏锐、强韧默认各 4。
- 初始压力上限：默认 5，允许由强韧或结算数据修改。
- 初始香烟：默认 5。
- 压力值范围始终 Clamp 到 `[0, StressMax]`。
- 压力达到上限立即触发 `PlayerBreakdown`，流程进入 `EndingType.PlayerBreakdown`。

### 7.3 Personality Rule

```csharp
public interface IPersonalityRuleService
{
    TagMatchResult CheckPlayerTagMatch(PlayerState player, DialogueChoiceDefinition choice);
    TagMatchResult CheckNpcTagMatch(NpcRuntimeState npc, DialogueChoiceDefinition choice);
}

public readonly struct TagMatchResult
{
    public bool IsMatch { get; init; }
    public int MatchedCount { get; init; }
    public int MismatchedCount { get; init; }
}
```

P0 判定标准：

- 每个对话选项必须至少有 1 个 `PersonalityTag`。
- 玩家匹配：选项标签与玩家两张人格卡任一标签相交即匹配。
- NPC 匹配：选项标签包含 NPC 固定人格标签即匹配。
- 玩家不匹配：压力 `+1`。
- NPC 不匹配：NPC 崩溃值 `+1`。
- NPC 匹配：NPC 崩溃值 `-1`，最低为 0。

多标签规则：

- P0 推荐每个选项只配置 1 个主标签。
- 若使用多标签，`MatchedCount` 和 `MismatchedCount` 必须按标签数量计算，不能只返回 bool。

### 7.4 Item / Cigarette

```csharp
public interface IItemService
{
    bool CanUseCigarette(PlayerState player);
    UseItemResult UseCigarette(PlayerState player);
    void ClampCigarette(PlayerState player);
}
```

香烟规则：

- 香烟数量下限为 0。
- 默认上限为 5。如策划要求“无上限”，只改 `GameConfig.CigaretteMax`，不要改代码。
- 压力为 0 时不能使用香烟。
- 使用成功：香烟 `-1`，压力 `-1`。

### 7.5 Mission / Obituary

```csharp
public interface IMissionService
{
    MissionDefinition GetCurrentMission();
    void PublishMission(string missionId);
    void ConfirmMission(string missionId);
    void CompleteMission(MissionResult result);
}

public interface IObituaryService
{
    ObituaryViewData LoadCurrentObituary(string missionId);
    void ApplyMissionResult(MissionResult result);
}
```

讣告状态：

```csharp
public enum ObituaryState
{
    Pending,
    Faded,
    Rewritten,
    Darkened,
    Removed
}
```

结局映射：

- `DeepRedemption`：讣告永久消失或替换为普通新闻。
- `DelayRewrite`：讣告淡化或临时改写。
- `CallFailed`：讣告加粗变黑。
- `PlayerBreakdown`：进入玩家失败结局，不继续当前任务结算。

### 7.6 Phone / Call Session

```csharp
public interface IPhoneService
{
    event Action<CallSessionState> CallStarted;
    event Action<MissionResult> CallEnded;

    void StartCall(string missionId);
    void EndCall(EndingType endingType);
}
```

通话开始时必须按顺序初始化：

1. 加载 `MissionDefinition`。
2. 加载 `NpcDefinition` 并创建 `NpcRuntimeState`。
3. 初始化 `CallSessionState`。
4. 初始化 `CallCounter`。
5. 加载 `DialogueTreeDefinition`。
6. 进入根节点。

### 7.7 Dialogue

```csharp
public interface IDialogueService
{
    event Action<DialogueNodeViewData> NodeChanged;
    event Action<DialogueChoiceResolution> ChoiceResolved;

    void LoadTree(DialogueTreeDefinition tree, CallSessionState session);
    DialogueNodeViewData GetCurrentNode();
    void SelectChoice(string choiceId);
}
```

`SelectChoice()` 标准流程：

1. 找到当前节点中的 `DialogueChoiceDefinition`。
2. 判断玩家人格匹配并结算压力。
3. 判断 NPC 人格匹配并结算崩溃值。
4. 如果选项配置了 Dice Check，调用 `IDiceService`。
5. 玩家发言完成后调用 `ICallCounterService.AddPlayerSpeech()`。
6. 按优先级检查结局：玩家崩溃 > NPC 挂断 > 深度救赎 > 拖延成功 > 节点跳转。
7. 如果没有结局，跳转到成功/失败/默认下一个节点。

### 7.8 Dice

```csharp
public interface IDiceService
{
    DiceRollResult Roll(DiceCheckDefinition check, PlayerStats stats);
    bool CheckResult(DiceRollResult result, int difficulty);
}

public readonly struct DiceRollResult
{
    public int PositiveDie { get; init; }
    public int NegativeDie { get; init; }
    public int StatBonus { get; init; }
    public int ExtraBonus { get; init; }
    public int Total { get; init; }
    public bool IsSuccess { get; init; }
}
```

骰子公式：

```text
Total = PositiveDie - NegativeDie + StatBonus + ExtraBonus
Success = Total >= Difficulty
```

难度标准：

| 难度 | 值 |
| --- | --- |
| 必过 | -4 |
| 简单 | 1 |
| 普通 | 3 |
| 困难 | 5 |
| 不可能 | 10 |

### 7.9 Call Counter

```csharp
public interface ICallCounterService
{
    event Action<int, int> CountChanged;
    event Action DelayEndingReached;

    void Initialize(NpcDefinition npc, CallCounterConfig config);
    void AddPlayerSpeech();
    bool HasReachedDelayTarget();
}
```

规则：

- 只有玩家选择对话并完成发言后，计数 `+1`。
- NPC 台词、系统提示、骰子动画不计数。
- 不同 NPC 可以配置不同拖延阈值。
- 长通话压力惩罚使用配置字段：

```csharp
public sealed class CallCounterConfig
{
    public int DefaultDelayTarget = 30;
    public int StressPenaltyStartCount = 30;
    public int StressPenaltyInterval = 3;
    public int StressPenaltyAmount = 1;
}
```

说明：策划案写“达到 30 后每多 3 句 +1 压力”，需求 ID 表写“每累计 10 句 +1 压力”。本项目以配置为准。P0 默认采用策划案规则：`StartCount = 30`，`Interval = 3`。

### 7.10 Ending / Result

```csharp
public interface IEndingService
{
    EndingType EvaluateEnding(CallSessionState session, PlayerState player, NpcRuntimeState npc);
    MissionResult BuildMissionResult(EndingType endingType, string missionId);
}

public interface IResultService
{
    void ApplyResult(MissionResult result);
}

public readonly struct MissionResult
{
    public string MissionId { get; init; }
    public string NpcId { get; init; }
    public EndingType EndingType { get; init; }
    public int StressDelta { get; init; }
    public int StressMaxDelta { get; init; }
    public int CigaretteDelta { get; init; }
    public ObituaryState ObituaryState { get; init; }
}
```

结局优先级：

1. 玩家压力达到上限：`PlayerBreakdown`。
2. NPC 崩溃达到上限：`CallFailed`。
3. NPC 崩溃清零并满足深度救赎节点条件：`DeepRedemption`。
4. 通话计数达到拖延阈值：`DelayRewrite`。
5. 否则继续对话。

默认结算：

| 结局 | 压力 | 香烟 | 压力上限 | 讣告 |
| --- | --- | --- | --- | --- |
| 深度救赎 | 清零 | `+1` | 不变 | 消失/普通新闻 |
| 拖延改写 | 清零 | `+0` | 不变 | 淡化/临时改写 |
| NPC 挂断失败 | 不清零 | `+0` | `-1`，不低于 3 | 加粗变黑 |
| 玩家精神失控 | 游戏失败 | `+0` | 不变 | 进入失败演出 |

说明：需求 ID 表中 `Result_ApplyDelaySuccess` 写了拖延成功香烟 `+1`，但策划案“拖延路线无收益”更符合机制区分。本规范 P0 默认拖延不奖励香烟。如项目管理决定采用需求表版本，只改 `EndingDefinition` 数据，不改代码。

---

## 8. UI 与表现层规范

### 8.1 UI 层级

```text
Canvas_Root
  Layer_Background
  Layer_HUD
  Layer_Panel
  Layer_Popup
  Layer_Tutorial
  Layer_Debug
```

UI Prefab 命名：

```text
PF_UI_MainMenu_C1_v1.0
PF_UI_Hud_C2_v1.0
PF_UI_NewspaperPanel_C2_v1.0
PF_UI_YellowPagesPanel_C2_v1.0
PF_UI_CallPanel_C2_v1.0
PF_UI_DicePanel_C3_v1.0
PF_UI_ResultPopup_C3_v1.0
```

### 8.2 View / Controller 分工

- `View`：只持有 `SerializeField` 引用并提供渲染方法，例如 `SetStress(int current, int max)`。
- `Controller`：监听服务事件，调用 View，处理按钮点击。
- `Service`：不引用 UI，不播放动画，不依赖 Canvas。

示例：

```csharp
public sealed class StressBarView : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _label;

    public void SetValue(int current, int max)
    {
        _slider.maxValue = max;
        _slider.value = current;
        _label.text = $"{current}/{max}";
    }
}
```

### 8.3 反馈事件

表现层监听事件播放动画和音效：

```csharp
public readonly struct StressChangedEvent
{
    public int Previous { get; init; }
    public int Current { get; init; }
    public int Max { get; init; }
    public StressChangeReason Reason { get; init; }
}

public readonly struct DiceRolledEvent
{
    public DiceRollResult Result { get; init; }
    public StatType StatType { get; init; }
}
```

禁止在 `DiceService` 里直接播放骰子动画。正确方式是：

1. `DialogueService` 调用 `DiceService.Roll()`。
2. 发布 `DiceRolledEvent`。
3. `DicePanelController` 播放动画。
4. 动画结束后通知 `DialogueService` 继续节点跳转。

---

## 9. 数据 ID 规范

### 9.1 业务 ID

业务 ID 使用小写英文和下划线。

```text
npc_lena
mission_lena_001
obit_lena_001
yp_lena_home
dialogue_lena_root
node_lena_opening_001
choice_lena_empathy_001
ending_lena_deep_redemption
```

规则：

- ID 不使用中文。
- ID 不使用版本号。
- 显示文本改动不影响 ID。
- 数据被引用后，不删除 ID；废弃时标记 `Deprecated`。

### 9.2 需求 ID

新增需求 ID 格式：

```text
SYS_<MODULE>_<NNN>
```

模块名示例：

```text
MENU, GAME, UI, TUT, PLAYER, RULE, ITEM, OBIT, MISSION,
CALL, PHONE, NPC, DIALOG, DICE, COUNT, FEEDBACK, END,
RESULT, ACH, SAVE
```

新增功能必须在 `Docs/Requirements/` 或需求总表里登记，再写代码。

---

## 10. 对话数据规范

### 10.1 DialogueTree

每个 NPC 一棵或多棵对话树：

```csharp
public sealed class DialogueTreeDefinition : ScriptableObject
{
    public string DialogueTreeId;
    public string NpcId;
    public string StartNodeId;
    public List<DialogueNodeDefinition> Nodes;
}
```

### 10.2 DialogueNode

```csharp
public sealed class DialogueNodeDefinition
{
    public string NodeId;
    public string SpeakerId;
    public string Text;
    public bool IsDeepRedemptionGate;
    public List<DialogueChoiceDefinition> Choices;
}
```

### 10.3 DialogueChoice

```csharp
public sealed class DialogueChoiceDefinition
{
    public string ChoiceId;
    public string Text;
    public List<PersonalityTag> Tags;
    public StatType? DiceStat;
    public int Difficulty;
    public string SuccessNodeId;
    public string FailureNodeId;
    public string NextNodeId;
    public bool CountsAsPlayerSpeech = true;
}
```

P0 配置要求：

- 每个普通节点至少 1 个选项。
- 每个选项必须有 `NextNodeId`，或同时有 `SuccessNodeId` / `FailureNodeId`。
- 关键选项需要 `DiceStat` 和 `Difficulty`。
- 深度救赎节点用 `IsDeepRedemptionGate = true` 标记，不用靠节点名猜测。

---

## 11. 场景与流程

### 11.1 场景职责

| Scene | 职责 |
| --- | --- |
| `SCN_Boot` | 初始化配置、服务注册、加载主菜单 |
| `SCN_MainMenu` | 主菜单、开始/继续/退出 |
| `SCN_Opening` | 开场剪影演出 |
| `SCN_MainRoom` | 小屋主界面、报纸、黄页、电话、香烟 |
| `SCN_Call` | 通话 UI、NPC 剪影、骰子、计数条 |
| `SCN_Ending` | 最终成功/失败演出 |

### 11.2 P0 流程状态机

```text
Boot
  -> MainMenu
  -> Opening
  -> MainRoom
  -> Newspaper
  -> YellowPages
  -> Dialing
  -> Call
  -> Result
  -> MainRoom 或 Ending
```

禁止从 UI Button 直接 `SceneManager.LoadScene()`。必须调用 `IGameFlowService`，再由 `SceneFlowController` 统一切场景或打开面板。

---

## 12. Audio / Animation 接入规范

### 12.1 Audio

音频由 `AudioCueDefinition` 管理，不在脚本中直接填 `AudioClip`。

```csharp
public interface IAudioService
{
    void PlaySfx(string cueId);
    void PlayBgm(string cueId, float fadeSeconds = 0.5f);
    void StopBgm(float fadeSeconds = 0.5f);
}
```

Cue ID 示例：

```text
sfx_button_click
sfx_newspaper_open
sfx_phone_dial
sfx_dice_roll
sfx_dice_success
sfx_dice_failure
sfx_cigarette_light
bgm_late_night_jazz
amb_rain_window
```

### 12.2 Animation

动画只处理表现状态，不写玩法结果：

- 拨号动画结束后调用 `PhoneDialController.OnDialAnimationComplete()`。
- 骰子动画结束后调用 `DicePanelController.OnDiceAnimationComplete()`。
- 抽烟动画结束后调用 `CigaretteController.OnSmokeAnimationComplete()`。

玩法结果在动画开始前或结束后由 Service 结算，但不得把数值计算写进 Animation Event。

---

## 13. 存档与成就

P1 实装，P0 可先保留接口。

```csharp
public interface ISaveService
{
    bool HasSave();
    void Save(GameRuntimeState state);
    GameRuntimeState Load();
    void DeleteSave();
}

public interface IAchievementService
{
    void Initialize();
    void CheckOnMissionResult(MissionResult result);
    void Unlock(string achievementId);
}
```

存档必须保存：

- 玩家人格、四维属性、压力、压力上限、香烟数量。
- 已发布/已完成任务状态。
- 每条讣告状态。
- 已解锁成就。

存档不保存：

- UI 打开状态。
- 动画播放进度。
- 当前临时骰子结果。

---

## 14. 测试规范

### 14.1 EditMode 必测

路径：

```text
Assets/_Project/Tests/EditMode/
```

必测清单：

- `DiceServiceTests`
  - 正负骰公式正确。
  - 总值大于等于难度时成功。
  - 不同属性加成正确加入。
- `PersonalityRuleServiceTests`
  - 玩家标签匹配不加压力。
  - 玩家标签不匹配加压力。
  - NPC 标签匹配减少崩溃。
  - NPC 标签不匹配增加崩溃。
- `PlayerServiceTests`
  - 压力 Clamp。
  - 香烟使用条件。
  - 压力达到上限触发玩家崩溃。
- `CallCounterServiceTests`
  - 只有玩家发言计数。
  - 达到阈值触发拖延结局。
  - 长通话压力惩罚按配置生效。
- `ResultServiceTests`
  - 深度救赎奖励正确。
  - 拖延改写无香烟奖励。
  - 失败降低压力上限且不低于 3。

### 14.2 PlayMode 冒烟测试

P0 完成后至少跑通：

1. 新游戏进入小屋。
2. 打开报纸并发布任务。
3. 黄页拨号进入通话。
4. 通过一组固定数据触发深度救赎。
5. 通过一组固定数据触发拖延改写。
6. 通过一组固定数据触发 NPC 挂断失败。

---

## 15. Git 与协作

### 15.1 Unity 设置

项目创建后必须确认：

- Version Control Mode：Visible Meta Files。
- Asset Serialization Mode：Force Text。
- 使用 Unity Smart Merge 或避免多人同时编辑同一 Scene/Prefab。

### 15.2 分工所有权

推荐所有权：

- 程序主程：`Scripts/Core`、`Scripts/Gameplay`、`Data/Configs`。
- UI 程序：`Scripts/UI`、`Prefabs/UI`、UI Scene 绑定。
- 美术：`Art`、`Prefabs/Scene`、角色/背景资源。
- 音效：`Audio`、`AudioCueDefinition`。
- 文案/策划：`Data/Dialogues`、`Data/Missions`、`Data/NPCs`。

多人协作时：

- Scene 文件一天内只允许一个人主改。
- Prefab 改动先沟通 Owner。
- 文案改数据，不改脚本。
- 程序新增 public API 必须更新本文档对应章节。

### 15.3 Commit 建议

```text
feat(dialogue): add dialogue service skeleton
feat(dice): implement 2d6 roll rule
data(npc): add lena npc definition
ui(call): bind stress and breakdown bars
fix(result): clamp stress max after failed mission
docs(api): update dialogue data contract
```

---

## 16. Cursor / AI 编程规则

给 Cursor 或其他 AI 代理派活时，必须附上以下规则：

1. 先阅读 `程序开发API.md`。
2. 不允许新增本文档未定义的顶级目录。
3. 不允许把角色具体文本、NPC 数值、任务奖励硬编码进脚本。
4. 新增脚本必须放在 `Assets/_Project/Scripts/` 对应子目录，并使用 `MidnightObituary.*` 命名空间。
5. 新增数据必须使用稳定英文 ID。
6. 新增系统必须先查是否已有 Service/Controller/View 可扩展。
7. 修改公共接口时必须同步更新本文档。
8. P0 阶段优先实现闭环，拒绝无关架构升级。
9. 所有数值变化必须通过对应 Service，UI 不直接改 State。
10. 每次实现核心规则后补 EditMode Test。

推荐任务提示模板：

```text
请先阅读根目录《程序开发API.md》，只实现 [需求ID]。
遵守 MidnightObituary 命名空间和 Service/View/Controller 分层。
不要硬编码 NPC 文案和数值；需要数据时新增 ScriptableObject Definition。
完成后说明修改了哪些文件、如何在 Unity 中验证、是否需要补数据。
```

---

## 17. 开发里程碑

### 循环 1：技术准备

- 建立 Unity 工程结构。
- 建立 asmdef。
- 建立 `GameConfig`、基础枚举、Runtime State。
- 建立 Game Flow、Player、Rule、Dice、CallCounter 的纯逻辑骨架。
- 建立基础测试。

### 循环 2：第一位 NPC 可玩

- 接入一位 NPC 的 `NpcDefinition`、`MissionDefinition`、`DialogueTreeDefinition`。
- 完成小屋、报纸、黄页、拨号、通话基础 UI。
- 跑通深度救赎、拖延改写、失败三种结局。

### 循环 3：核心系统完成

- 完成骰子表现、电话动画、压力/崩溃反馈。
- 接入第一位和第二位 NPC 文本。
- 统一结算弹窗和讣告状态变化。

### 循环 4：冻结玩法

- 冻结系统 API。
- 只允许调整数值、文本、UI 表现和 Bug。
- 审核愿望单，P2 不影响 P0。

### 循环 5-6：内容扩展与外部试玩

- 接入剩余 NPC、音频、美术。
- 根据反馈调数值和文案。
- 保存系统进入 P1 实装。

### 循环 7-8：收尾提交

- 修 Bug。
- IL2CPP 构建测试。
- 最终资源检查、版权检查、打包上传。

---

## 18. 当前策划冲突与处理准则

以下点在策划案与需求 ID 表中存在差异，程序层必须以“数据可配置”处理，避免反复改代码。

| 冲突点 | 策划案 | 需求 ID 表 | 程序规范 |
| --- | --- | --- | --- |
| 长通话压力 | 30 后每 3 句 +1 | 每累计 10 句 +1 | `CallCounterConfig` 配置 |
| 拖延结局奖励 | 无收益 | 香烟 +1 | `EndingDefinition` 配置，P0 默认无香烟 |
| 游戏标题 | 《明日讣告》 | 文件名/项目名含午夜讣告 | 代码用 `MidnightObituary`，显示名走配置 |
| 香烟上限 | 消耗品无上限 / 初始 5 | 香烟上限 5 | `GameConfig.CigaretteMax` 配置 |

处理原则：

- 如果是文案、数值、奖励差异，改数据。
- 如果是流程和系统边界差异，项目负责人确认后再改 API。
- 不允许在 UI 或单个 NPC 脚本里写特殊规则绕过系统。

---

## 19. Definition of Done

任何 P0 系统完成必须满足：

1. 需求 ID 能映射到明确脚本、方法或数据。
2. 核心逻辑不依赖场景对象即可测试。
3. UI 只通过 Service 或事件更新，不直接写全局状态。
4. 数据资产能替换 NPC 或任务，不需要改代码。
5. 至少有一条正常路径和一条失败路径验证。
6. 新增 public API 已同步本文档。
7. Unity Console 无 Error。

---

## 20. 禁止事项

- 禁止新建 `Assets/Scripts`、`Assets/Prefabs` 等与 `_Project` 并列的散乱目录。
- 禁止在 Button OnClick 中直接写复杂业务逻辑。
- 禁止 UI 组件直接修改 `PlayerState.Stress`、`NpcRuntimeState.Breakdown`。
- 禁止把 `npc_lena`、电话号码、对话文本写死在脚本里。
- 禁止用中文类名、中文变量名、中文资源 ID。
- 禁止复制多个几乎相同的 NPC 流程脚本。
- 禁止未登记 ID 就新增系统需求。
- 禁止为了单个演出破坏通用结算流程。

---

## 21. 最小 P0 脚本清单

第一轮程序搭建至少创建以下脚本：

```text
Scripts/Core/
  GameRuntimeState.cs
  PlayerState.cs
  PlayerStats.cs
  RequirementIds.cs
  Enums.cs

Scripts/Gameplay/
  GameFlowService.cs
  PlayerService.cs
  PersonalityRuleService.cs
  ItemService.cs
  MissionService.cs
  ObituaryService.cs
  PhoneService.cs
  DialogueService.cs
  DiceService.cs
  CallCounterService.cs
  EndingService.cs
  ResultService.cs

Scripts/Gameplay/Definitions/
  GameConfig.cs
  PersonalityDefinition.cs
  PlayerInitialConfig.cs
  NpcDefinition.cs
  MissionDefinition.cs
  ObituaryDefinition.cs
  YellowPageEntryDefinition.cs
  DialogueTreeDefinition.cs
  EndingDefinition.cs

Scripts/UI/
  MainMenuController.cs
  MainRoomController.cs
  NewspaperPanelController.cs
  YellowPagesPanelController.cs
  CallPanelController.cs
  DicePanelController.cs
  ResultPopupController.cs
  StressBarView.cs
  BreakdownBarView.cs
  CallCounterView.cs

Scripts/Infrastructure/
  SceneFlowController.cs
  AudioService.cs
  SaveService.cs
  RandomProvider.cs
```

如果时间紧张，P0 可以暂缓：

```text
TutorialService.cs
AchievementService.cs
SaveService.cs 的完整实现
Editor Importer
```

但接口位置应保留，避免后续硬插。

