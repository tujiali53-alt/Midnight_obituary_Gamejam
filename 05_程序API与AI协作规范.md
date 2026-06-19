# 《明日讣告》程序API与AI协作规范

适用对象：Cursor、Codex、Claude Code、Copilot、其它AI编程工具  
项目版本：Unity 2023.2.20  
对话插件：articy:draft X Importer  
动画格式：Spine 4.2 Json / Atlas / Texture  
文档目标：固定模块边界、公开API、数据流、命名规范和AI生成代码时必须遵守的约束。

## AI工具使用前必须读取

AI在生成、修改或重构代码前，必须先读取并遵守以下文档：

| 顺序 | 文档 | 用途 |
| --- | --- | --- |
| 1 | `01_程序分工.md` | 确认主程/副程负责边界 |
| 2 | `03_Unity项目结构与架构.md` | 确认目录、架构、依赖方向 |
| 3 | `05_程序API与AI协作规范.md` | 确认API契约和禁止事项 |

## Cursor提示词模板

将以下内容放在Cursor任务开头：

```text
你正在开发Unity 2023.2.20项目《明日讣告》。
请先遵守 05_程序API与AI协作规范.md。
本次只修改指定模块，不要跨层直接改RuntimeData。
UI不得写规则，Dialogue不得直接改Player/NPC字段，Ending只输出EndingResult。
如果需要改变玩家压力或香烟，调用PlayerManager/CigaretteSystem公开API。
如果需要改变NPC崩溃，调用NPCManager公开API。
如果需要判断骰子、计数或结局，调用DiceSystem/CallCounterSystem/EndingEvaluator。
新增代码必须符合现有目录、命名空间、事件和P0闭环优先级。
```

## 模块所有权

| 模块 | 主负责人 | AI修改权限 |
| --- | --- | --- |
| Core / Flow | 主程 | 可新增状态和事件，但不得让Core依赖UI或Gameplay具体类 |
| UI / Interaction | 主程 | 可绑定公开API，不得直接修改RuntimeData字段 |
| Animation / Spine | 主程 | 只响应语义事件，不得把业务规则写进动画脚本 |
| Player / Item | 主程 | 可实现玩家、压力、香烟逻辑；其它模块必须通过公开方法调用 |
| NPC / Rule | 副程 | 可实现NPC崩溃和人格匹配；不得直接改Player字段 |
| Dice / Counter | 副程 | 可实现骰子、通话计数；压力变化必须请求Player接口 |
| Dialogue / articy | 双人 | UI显示归主程，条件/效果/跳转归副程，必须通过Adapter隔离articy字段 |
| Ending / Result | 副程判断，主程表现 | Ending只输出结局类型；Result负责应用奖惩和表现 |
| Save / Achievement | P1后置 | 不得阻塞P0闭环 |

## 目录和命名

```text
Assets/
  _Project/
    Scripts/
      Core/
      Data/
      Dialogue/
      Gameplay/
        Player/
        NPC/
        Rules/
        Dice/
        Call/
        Items/
        Mission/
        Ending/
        Save/
        Achievement/
      UI/
      Animation/
      Audio/
      Debug/
```

| 类型 | 命名方式 | 示例 |
| --- | --- | --- |
| 脚本类 | PascalCase，职责明确 | `PlayerManager.cs` |
| 数据类 | `Definition` / `RuntimeData`结尾 | `NPCDefinition.cs` |
| 请求参数 | `Request`结尾 | `StressChangeRequest.cs` |
| 结果返回 | `Result`结尾 | `DiceResult.cs` |
| 事件参数 | `EventArgs`结尾 | `StressChangedEventArgs.cs` |
| 资源 | 使用需求表前缀 | `ANI_DiceRoll_v3.2` |

推荐命名空间：

```csharp
ObituaryTomorrow.Core
ObituaryTomorrow.Data
ObituaryTomorrow.Dialogue
ObituaryTomorrow.Gameplay.Player
ObituaryTomorrow.Gameplay.NPC
ObituaryTomorrow.Gameplay.Rules
ObituaryTomorrow.Gameplay.Dice
ObituaryTomorrow.Gameplay.Call
ObituaryTomorrow.Gameplay.Items
ObituaryTomorrow.Gameplay.Ending
ObituaryTomorrow.UI
ObituaryTomorrow.Animation
```

## 依赖规则

```text
UI -> Core事件 / Gameplay公开接口
Animation -> Core事件 / Audio事件
Dialogue -> Gameplay公开接口
Gameplay -> Data / Core事件
Core -> 不依赖业务模块
Data -> 不依赖场景对象
```

禁止事项：

| 禁止项 | 正确做法 |
| --- | --- |
| UI直接改`PlayerRuntimeData.CurrentStress` | 调用`PlayerManager.RequestStressChange()` |
| Dialogue直接改NPC崩溃字段 | 调用`NPCManager.RequestBreakdownChange()` |
| Counter直接改玩家压力 | 调用`PlayerManager.RequestStressChange()` |
| RuleSystem播放UI或动画 | 返回`RuleEvaluationResult`，由调用方触发表现 |
| EndingEvaluator打开结算UI | 输出`EndingResult`，交给`ResultSystem`和`ResultView` |
| articy字段散落在业务脚本 | 统一经`ArticyDialogueAdapter`转换 |
| 动画脚本决定游戏规则 | 动画只发`AnimationEventRouter`事件 |
| 新增重复Manager | 先查现有Manager，扩展公开API |

## 全局枚举

```csharp
public enum GameState
{
    Boot,
    MainMenu,
    Opening,
    MainRoom,
    ObituaryView,
    YellowPagesView,
    Dialing,
    InCall,
    Result,
    GameOver
}

public enum PersonalityTag
{
    Emotional,
    Rational,
    Idealistic,
    Practical
}

public enum PlayerAttributeType
{
    Perception,
    Logic,
    Insight,
    Resilience
}

public enum EndingType
{
    None,
    DeepAnalysis,
    DelaySuccess,
    CallFailed,
    PlayerBreakdown
}

public enum StatChangeReason
{
    DialogueChoice,
    DiceResult,
    CallCounterMilestone,
    CigaretteUse,
    ResultReward,
    ResultPenalty,
    Debug
}
```

## 通用返回结构

所有会改变状态的方法，优先返回Result对象，不要只返回`bool`。

```csharp
public readonly struct OperationResult
{
    public bool Success { get; }
    public string Message { get; }
}

public readonly struct StatChangeResult
{
    public bool Applied { get; }
    public int OldValue { get; }
    public int NewValue { get; }
    public int Delta { get; }
    public bool ReachedMin { get; }
    public bool ReachedMax { get; }
    public StatChangeReason Reason { get; }
}
```

## Core API

### GameManager

路径：`Scripts/Core/GameManager.cs`  
负责人：主程  
职责：游戏入口、状态切换、流程调度，不写具体数值规则。

```csharp
public sealed class GameManager : MonoBehaviour
{
    public GameState CurrentState { get; }
    public GameSessionData Session { get; }

    public void InitializeGame();
    public void StartNewGame(NewGameRequest request);
    public void ContinueGame();
    public void ChangeState(GameState nextState);
    public void EnterMainRoom();
    public void StartCall(string npcId, string dialogueId);
    public void FinishCall(EndingResult endingResult);
    public void ReturnToMainMenu();
    public void QuitGame();
}
```

调用规则：

| 调用方 | 允许调用 |
| --- | --- |
| MainMenu UI | `StartNewGame()` / `ContinueGame()` / `QuitGame()` |
| PhoneController | `StartCall()` |
| ResultSystem | `FinishCall()` / `EnterMainRoom()` |
| DebugPanel | `ChangeState()` |

### GameEventBus

路径：`Scripts/Core/GameEventBus.cs`  
职责：跨模块广播，不保存业务状态。

```csharp
public static class GameEventBus
{
    public static event Action<GameStateChangedEventArgs> StateChanged;
    public static event Action<StressChangedEventArgs> PlayerStressChanged;
    public static event Action<CigaretteChangedEventArgs> CigaretteChanged;
    public static event Action<NPCBreakdownChangedEventArgs> NPCBreakdownChanged;
    public static event Action<CallCounterChangedEventArgs> CallCounterChanged;
    public static event Action<DiceRolledEventArgs> DiceRolled;
    public static event Action<EndingResult> EndingTriggered;

    public static void RaiseStateChanged(GameStateChangedEventArgs args);
    public static void RaisePlayerStressChanged(StressChangedEventArgs args);
    public static void RaiseCigaretteChanged(CigaretteChangedEventArgs args);
    public static void RaiseNPCBreakdownChanged(NPCBreakdownChangedEventArgs args);
    public static void RaiseCallCounterChanged(CallCounterChangedEventArgs args);
    public static void RaiseDiceRolled(DiceRolledEventArgs args);
    public static void RaiseEndingTriggered(EndingResult result);
}
```

## Data API

### GameSessionData

路径：`Scripts/Core/GameSessionData.cs`或`Scripts/Data/GameSessionData.cs`  
职责：一局游戏的运行时总数据。只允许Manager修改，不允许UI直接写字段。

```csharp
[Serializable]
public sealed class GameSessionData
{
    public PlayerRuntimeData Player { get; set; }
    public string CurrentMissionId { get; set; }
    public string CurrentNpcId { get; set; }
    public int CurrentDay { get; set; }
    public List<string> CompletedMissionIds { get; }
    public Dictionary<string, ObituaryState> ObituaryStates { get; }
    public Dictionary<string, bool> Flags { get; }
}
```

### PlayerRuntimeData

负责人：主程。

```csharp
[Serializable]
public sealed class PlayerRuntimeData
{
    public IReadOnlyList<PersonalityTag> PersonalityTags { get; }
    public int Perception { get; set; }
    public int Logic { get; set; }
    public int Insight { get; set; }
    public int Resilience { get; set; }
    public int CurrentStress { get; set; }
    public int MaxStress { get; set; }
    public int CigaretteCount { get; set; }
    public int MaxCigaretteCount { get; set; }
}
```

### NPCDefinition

负责人：副程。

```csharp
[CreateAssetMenu(menuName = "ObituaryTomorrow/NPC Definition")]
public sealed class NPCDefinition : ScriptableObject
{
    public string NpcId;
    public string DisplayName;
    public PersonalityTag PersonalityTag;
    public int InitialBreakdown;
    public int MaxBreakdown;
    public int DelayThreshold;
    public string DialogueId;
}
```

### DialogueDefinition

负责人：双人，来源优先为articy导入。

```csharp
public sealed class DialogueDefinition
{
    public string DialogueId { get; }
    public string StartNodeId { get; }
    public IReadOnlyDictionary<string, DialogueNodeRuntime> Nodes { get; }
}
```

## Player / Item API

负责人：主程  
路径：`Scripts/Gameplay/Player/`、`Scripts/Gameplay/Items/`

### PlayerManager

```csharp
public sealed class PlayerManager : MonoBehaviour
{
    public PlayerRuntimeData RuntimeData { get; }
    public int CurrentStress { get; }
    public int MaxStress { get; }
    public int CigaretteCount { get; }

    public void InitializeNewPlayer(PlayerInitRequest request);
    public IReadOnlyList<PersonalityTag> GetPersonalityTags();
    public int GetAttribute(PlayerAttributeType attributeType);
    public bool HasPersonalityTag(PersonalityTag tag);

    public StatChangeResult RequestStressChange(StressChangeRequest request);
    public bool IsStressMaxed();
    public EndingResult CreatePlayerBreakdownEnding();
    public void ResetStress(StatChangeReason reason);
    public StatChangeResult ModifyMaxStress(int delta, StatChangeReason reason);
}
```

### StressChangeRequest

```csharp
public readonly struct StressChangeRequest
{
    public int Delta { get; }
    public StatChangeReason Reason { get; }
    public string SourceId { get; }
    public bool AllowTriggerEnding { get; }
}
```

调用规则：

| 调用方 | 调用方法 | 说明 |
| --- | --- | --- |
| DialogueEffectExecutor | `RequestStressChange()` | 对话选项导致压力变化 |
| CallCounterSystem | `RequestStressChange()` | 每10句或超时惩罚 |
| CigaretteSystem | `RequestStressChange()` | 抽烟减压 |
| ResultSystem | `ResetStress()` / `ModifyMaxStress()` | 结算奖惩 |
| UI | 只读属性或订阅事件 | 不直接写RuntimeData |

### CigaretteSystem

```csharp
public sealed class CigaretteSystem : MonoBehaviour
{
    public int Count { get; }
    public int MaxCount { get; }

    public OperationResult CanUseCigarette();
    public OperationResult RequestUseCigarette();
    public StatChangeResult ConfirmUseCigarette();
    public StatChangeResult AddCigarette(int amount, StatChangeReason reason);
    public StatChangeResult SetCigaretteCount(int value, StatChangeReason reason);
}
```

约束：

| 规则 | API表现 |
| --- | --- |
| 初始香烟5 | `InitializeNewPlayer()`或`CigaretteSystem`初始化 |
| 香烟上限5 | `AddCigarette()`内Clamp |
| 压力为0不可使用 | `CanUseCigarette()`返回失败 |
| 香烟为0不可使用 | `CanUseCigarette()`返回失败 |
| 抽烟表现由主程动画触发 | `ConfirmUseCigarette()`成功后发`CigaretteChanged`和动画事件 |

## NPC / Rule API

负责人：副程  
路径：`Scripts/Gameplay/NPC/`、`Scripts/Gameplay/Rules/`

### NPCManager

```csharp
public sealed class NPCManager : MonoBehaviour
{
    public NPCRuntimeData CurrentNPC { get; }
    public bool HasActiveNPC { get; }

    public OperationResult LoadNPC(string npcId);
    public void ClearCurrentNPC();
    public PersonalityTag GetCurrentNPCPersonality();
    public StatChangeResult RequestBreakdownChange(BreakdownChangeRequest request);
    public bool IsBreakdownZero();
    public bool IsBreakdownMaxed();
}
```

### BreakdownChangeRequest

```csharp
public readonly struct BreakdownChangeRequest
{
    public int Delta { get; }
    public StatChangeReason Reason { get; }
    public string SourceNodeId { get; }
    public string SourceChoiceId { get; }
}
```

### RuleSystem

```csharp
public sealed class RuleSystem
{
    public RuleEvaluationResult EvaluateChoice(
        IReadOnlyList<PersonalityTag> playerTags,
        PersonalityTag npcTag,
        IReadOnlyList<PersonalityTag> choiceTags);

    public bool CheckPlayerTagMatch(
        IReadOnlyList<PersonalityTag> playerTags,
        IReadOnlyList<PersonalityTag> choiceTags);

    public bool CheckNPCTagMatch(
        PersonalityTag npcTag,
        IReadOnlyList<PersonalityTag> choiceTags);
}
```

### RuleEvaluationResult

```csharp
public readonly struct RuleEvaluationResult
{
    public bool MatchesPlayer { get; }
    public bool MatchesNPC { get; }
    public int PlayerStressDelta { get; }
    public int NPCBreakdownDelta { get; }
}
```

规则：

| 条件 | 结果 |
| --- | --- |
| 选项违背玩家人格 | 玩家压力+1 |
| 选项符合玩家人格 | 玩家压力不增加 |
| 选项违背NPC人格 | NPC崩溃+1 |
| 选项符合NPC人格 | NPC崩溃-1 |

## Dice API

负责人：副程  
路径：`Scripts/Gameplay/Dice/`

```csharp
public sealed class DiceSystem
{
    public DiceResult Roll(DiceCheckRequest request);
    public bool CheckResult(DiceResult result);
}

public readonly struct DiceCheckRequest
{
    public string CheckId { get; }
    public PlayerAttributeType AttributeType { get; }
    public int AttributeValue { get; }
    public int Difficulty { get; }
    public string SuccessNodeId { get; }
    public string FailureNodeId { get; }
}

public readonly struct DiceResult
{
    public string CheckId { get; }
    public int PositiveD6 { get; }
    public int NegativeD6 { get; }
    public int AttributeValue { get; }
    public int Difficulty { get; }
    public int Total { get; }
    public bool Success { get; }
    public string NextNodeId { get; }
}
```

计算规则：

```text
Total = PositiveD6 - NegativeD6 + AttributeValue
Success = Total >= Difficulty
```

AI不得改动公式，除非策划明确更新需求。

## CallCounter API

负责人：副程  
路径：`Scripts/Gameplay/Call/CallCounterSystem.cs`

```csharp
public sealed class CallCounterSystem : MonoBehaviour
{
    public int CurrentCount { get; }
    public int TargetCount { get; }

    public void Initialize(CallCounterInitRequest request);
    public CallCounterResult AddPlayerSpeech(string sourceNodeId);
    public bool HasReachedDelayThreshold();
    public float GetProgress01();
    public void ResetCounter();
}

public readonly struct CallCounterInitRequest
{
    public string NpcId { get; }
    public int TargetCount { get; }
    public int StressMilestoneInterval { get; }
}

public readonly struct CallCounterResult
{
    public int OldCount { get; }
    public int NewCount { get; }
    public bool ReachedDelayThreshold { get; }
    public bool ShouldRequestStressIncrease { get; }
}
```

调用规则：

| 条件 | 行为 |
| --- | --- |
| 玩家完成一句发言 | `AddPlayerSpeech()` |
| 计数到阈值 | `EndingEvaluator`触发`DelaySuccess` |
| 每10句玩家发言 | 返回`ShouldRequestStressIncrease`，由调用方请求`PlayerManager.RequestStressChange()` |
| NPC台词 | 不计数 |

## Dialogue API

负责人：双人  
路径：`Scripts/Dialogue/`

分工：

| 子模块 | 负责人 | 说明 |
| --- | --- | --- |
| DialogueController | 双人 | 对话主控，协调UI和逻辑 |
| DialogueView | 主程 | 文本、选项、按钮、数值反馈 |
| ArticyDialogueAdapter | 双人 | articy字段映射 |
| DialogueConditionEvaluator | 副程 | 判断条件 |
| DialogueEffectExecutor | 副程 | 执行效果，但改Player必须走主程接口 |

### DialogueController

```csharp
public sealed class DialogueController : MonoBehaviour
{
    public string CurrentDialogueId { get; }
    public string CurrentNodeId { get; }

    public OperationResult StartDialogue(DialogueStartRequest request);
    public OperationResult ShowNode(string nodeId);
    public OperationResult SelectChoice(string choiceId);
    public OperationResult JumpToNode(string nodeId);
    public void EndDialogue(EndingResult endingResult);
}

public readonly struct DialogueStartRequest
{
    public string DialogueId { get; }
    public string NpcId { get; }
    public string StartNodeId { get; }
}
```

### Dialogue节点结构

```csharp
public sealed class DialogueNodeRuntime
{
    public string NodeId { get; }
    public string Speaker { get; }
    public string Text { get; }
    public IReadOnlyList<DialogueChoiceRuntime> Choices { get; }
    public string DefaultNextNodeId { get; }
}

public sealed class DialogueChoiceRuntime
{
    public string ChoiceId { get; }
    public string Text { get; }
    public IReadOnlyList<PersonalityTag> Tags { get; }
    public DiceCheckRequest? DiceCheck { get; }
    public IReadOnlyList<DialogueCondition> Conditions { get; }
    public IReadOnlyList<DialogueEffect> Effects { get; }
    public string NextNodeId { get; }
}
```

### DialogueEffectExecutor

```csharp
public sealed class DialogueEffectExecutor
{
    public DialogueEffectResult ExecuteEffects(
        DialogueChoiceRuntime choice,
        DialogueRuntimeContext context);
}

public sealed class DialogueRuntimeContext
{
    public PlayerManager PlayerManager { get; }
    public NPCManager NPCManager { get; }
    public RuleSystem RuleSystem { get; }
    public DiceSystem DiceSystem { get; }
    public CallCounterSystem CallCounterSystem { get; }
}
```

执行顺序必须固定：

```text
1. 检查Choice条件
2. RuleSystem判断玩家/NPC人格匹配
3. 应用玩家压力变化，调用PlayerManager.RequestStressChange()
4. 应用NPC崩溃变化，调用NPCManager.RequestBreakdownChange()
5. 玩家发言计数，调用CallCounterSystem.AddPlayerSpeech()
6. 如需要骰子，调用DiceSystem.Roll()
7. EndingEvaluator检查结局
8. DialogueController跳转下一个节点或结束通话
```

## Ending / Result API

### EndingEvaluator

负责人：副程  
职责：只判断结局，不打开UI，不播放动画。

```csharp
public sealed class EndingEvaluator
{
    public EndingResult Evaluate(EndingEvaluationContext context);
    public EndingResult CreateDeepAnalysisResult(string npcId, string missionId);
    public EndingResult CreateDelaySuccessResult(string npcId, string missionId);
    public EndingResult CreateCallFailedResult(string npcId, string missionId);
    public EndingResult CreatePlayerBreakdownResult(string missionId);
}

public readonly struct EndingEvaluationContext
{
    public string MissionId { get; }
    public string NpcId { get; }
    public int PlayerStress { get; }
    public int PlayerMaxStress { get; }
    public int NPCBreakdown { get; }
    public int NPCMaxBreakdown { get; }
    public int CallCount { get; }
    public int DelayThreshold { get; }
}

public readonly struct EndingResult
{
    public EndingType Type { get; }
    public string MissionId { get; }
    public string NpcId { get; }
    public bool ShouldEndCall { get; }
    public bool ShouldEndGame { get; }
}
```

优先级：

```text
1. PlayerBreakdown：玩家压力达到上限，直接游戏结束
2. CallFailed：NPC崩溃达到上限，任务失败
3. DeepAnalysis：NPC崩溃清零，深度救赎
4. DelaySuccess：通话计数达到拖延阈值，拖延成功
```

### ResultSystem

负责人：主程  
职责：接收`EndingResult`，应用奖惩，刷新报纸状态，通知UI表现。

```csharp
public sealed class ResultSystem : MonoBehaviour
{
    public ResultApplyResult ApplyResult(EndingResult endingResult);
    public void ReturnToMainRoom();
}

public readonly struct ResultApplyResult
{
    public EndingResult Ending { get; }
    public int StressDelta { get; }
    public int MaxStressDelta { get; }
    public int CigaretteDelta { get; }
    public ObituaryState NewObituaryState { get; }
}
```

奖惩规则：

| 结局 | ResultSystem行为 |
| --- | --- |
| DeepAnalysis | 香烟+1，玩家压力清零，讣告改变 |
| DelaySuccess | 玩家压力清零，讣告改变 |
| CallFailed | 玩家压力上限-1，不低于3，讣告加重 |
| PlayerBreakdown | 进入GameOver，不返回主小屋 |

## UI API

负责人：主程  
路径：`Scripts/UI/`

UI只做展示和输入转发，不拥有规则。

```csharp
public abstract class UIView : MonoBehaviour
{
    public virtual void Show();
    public virtual void Hide();
    public virtual void SetInteractable(bool interactable);
}

public sealed class DialogueView : UIView
{
    public void RenderNode(DialogueNodeRuntime node);
    public void RenderChoices(IReadOnlyList<DialogueChoiceRuntime> choices);
    public void SetStressValue(int current, int max);
    public void SetNPCBreakdownValue(int current, int max);
    public void SetCallCounterValue(int current, int target);
    public void SetCigaretteCount(int count);
}

public sealed class ResultView : UIView
{
    public void RenderResult(ResultApplyResult result);
}
```

UI禁止：

| 禁止 | 替代 |
| --- | --- |
| 按钮里直接`player.CurrentStress++` | 调`PlayerManager.RequestStressChange()` |
| 按钮里直接改`GameState`字段 | 调`GameManager.ChangeState()` |
| UI里写骰子公式 | 调`DiceSystem.Roll()` |
| UI里判断结局 | 调`EndingEvaluator.Evaluate()` |

## Animation / Spine API

负责人：主程  
路径：`Scripts/Animation/`

```csharp
public sealed class SpineAnimationController : MonoBehaviour
{
    public void Play(string animationName, bool loop = false);
    public void Stop();
    public void SetSkin(string skinName);
}

public sealed class AnimationEventRouter : MonoBehaviour
{
    public event Action<string> AnimationEventRaised;
    public void RaiseAnimationEvent(string eventId);
}

public sealed class PhoneAnimationController : MonoBehaviour
{
    public void PlayDialStart(string phoneNumber);
    public void PlayPickup();
    public void PlayHangup();
}

public sealed class DiceAnimationController : MonoBehaviour
{
    public void PlayRoll(DiceResult result);
}

public sealed class CigaretteAnimationController : MonoBehaviour
{
    public void PlayUseCigarette();
}
```

动画事件命名：

| 事件ID | 说明 |
| --- | --- |
| `ANI_PHONE_DIAL_FINISHED` | 拨号动画结束，进入通话 |
| `ANI_PHONE_HANGUP_FINISHED` | 挂断动画结束，进入结算 |
| `ANI_DICE_ROLL_FINISHED` | 骰子动画结束，显示结果 |
| `ANI_CIGARETTE_USE_FINISHED` | 抽烟动画结束，恢复交互 |
| `ANI_OBITUARY_UPDATE_FINISHED` | 报纸变化动画结束 |

## articy字段规范

articy导入后必须转换为运行时结构，不允许业务脚本直接依赖articy原始类。

| articy字段 | 类型 | Runtime字段 | 必填 |
| --- | --- | --- | --- |
| `DialogueId` | string | `DialogueDefinition.DialogueId` | 是 |
| `NodeId` | string | `DialogueNodeRuntime.NodeId` | 是 |
| `Speaker` | string | `DialogueNodeRuntime.Speaker` | 是 |
| `Text` | string | `DialogueNodeRuntime.Text` | 是 |
| `ChoiceId` | string | `DialogueChoiceRuntime.ChoiceId` | 是 |
| `ChoiceText` | string | `DialogueChoiceRuntime.Text` | 是 |
| `Tags` | string list | `PersonalityTag[]` | 否 |
| `DiceCheck` | bool | `DiceCheckRequest?` | 否 |
| `Difficulty` | int | `DiceCheckRequest.Difficulty` | 骰子时必填 |
| `SuccessNode` | string | `DiceCheckRequest.SuccessNodeId` | 骰子时必填 |
| `FailureNode` | string | `DiceCheckRequest.FailureNodeId` | 骰子时必填 |
| `Effects` | string/json | `DialogueEffect[]` | 否 |
| `NextNode` | string | `NextNodeId` | 否 |

AI生成导入逻辑时，必须把字段映射集中写在`ArticyDialogueAdapter`。

## Debug API

Debug工具只在开发构建或编辑器内启用。

```csharp
public sealed class DebugPanel : MonoBehaviour
{
    public void ForceGameState(GameState state);
    public void ForceStress(int value);
    public void ForceNPCBreakdown(int value);
    public void ForceCallCount(int value);
    public void ForceEnding(EndingType endingType);
    public void JumpDialogueNode(string nodeId);
}
```

约束：

| 规则 | 说明 |
| --- | --- |
| Debug方法必须集中在`Scripts/Debug/` | 不散落在正式业务脚本 |
| Debug修改数值也要走Manager接口 | 保持事件和UI刷新一致 |
| 发布包默认关闭Debug入口 | 避免提交版本误触 |

## AI生成代码规范

AI工具必须遵守：

| 规范 | 说明 |
| --- | --- |
| 先查现有类 | 不确认现有结构前，不新增同职责Manager |
| 不跨负责人边界 | 主程模块和副程模块通过公开API通信 |
| 不硬编码正式文本 | 正式文本来自articy或配置 |
| 不硬编码资源路径 | 使用Inspector引用、配置表或Addressable预留接口 |
| 不在UI里写规则 | UI只展示和转发玩家输入 |
| 不在Data类里查找场景对象 | Data保持纯数据 |
| 不在Animation里写业务判断 | 动画只做表现 |
| 新增状态变化必须发事件 | UI和动画靠事件刷新 |
| 所有Clamp在系统内部完成 | 调用方不负责修正上下限 |
| P0优先 | 不为P2功能牺牲主循环稳定 |

AI生成代码前应回答以下问题：

```text
1. 我要修改的模块属于主程、副程还是双人共同？
2. 这个功能是否已有Manager或System？
3. 是否需要新增公开API，而不是直接访问字段？
4. 是否会破坏GameLoop闭环？
5. 是否需要同步UI、动画或事件？
6. 是否需要Debug入口验证？
```

## 推荐实现顺序

```text
1. Data结构
2. Manager/System公开API
3. 内部规则实现
4. GameEventBus事件
5. UI/Animation绑定
6. Debug入口
7. 第一位NPC联调
```

不要先做表现再补规则；不要先做P1/P2再补P0。

## 最小验收清单

| 模块 | 验收 |
| --- | --- |
| Core | 能从MainMenu进入MainRoom，再进入InCall和Result |
| Player | 能初始化人格/属性，压力变化会Clamp并发事件 |
| Item | 香烟能使用、阻止、增加、Clamp，并触发反馈 |
| NPC | 能加载NPC，崩溃变化会Clamp并发事件 |
| Rule | 选项标签能正确影响压力和崩溃 |
| Dice | 2D6正负骰公式正确，能返回成功/失败节点 |
| Counter | 玩家发言计数，NPC台词不计数，达到阈值可触发拖延 |
| Dialogue | 能加载节点、显示选项、执行效果、跳转分支 |
| Ending | 能判断四种结局，不直接打开UI |
| Result | 能应用奖惩，刷新报纸，返回主小屋 |
| UI | 只通过公开API和事件更新 |
| Animation | 只播放表现并发动画结束事件 |
