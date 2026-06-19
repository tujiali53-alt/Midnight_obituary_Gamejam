# 《明日讣告》前三日开发顺序与模块优先级

时间范围：循环2，6月20日-6月22日  
项目版本：Unity 2023.2.20  
目标：三天内完成第一位谈话对象的最小可玩闭环。  
交付标准：从新游戏进入主小屋，打开报纸，查黄页，拨号进入第一位NPC对话，触发深度救赎/拖延成功/失败挂断三种结局中的至少两种，第三种保留Debug强制触发入口。

## 三日优先级口径

| 优先级 | 定义                           | 示例                                                        |
| ------ | ------------------------------ | ----------------------------------------------------------- |
| P0     | 没有它就不能形成可玩闭环       | GameManager、Dialogue、Player/Item数值、NPC数值、骰子、结局 |
| P1     | 闭环可玩后提高可读性和可调试性 | 教学、调试面板、音效Hook、动画占位                          |
| P2     | 有余力才做                     | 成就完整UI、Credits、复杂存档                               |

## GameLoop最小闭环

```text
StartNewGame
  -> Game_Init
  -> Player_InitBaseStats
  -> Player_InitPersonality
  -> Item_Cigarette_Init
  -> Game_EnterMainRoom
  -> Obituary_OpenUI
  -> Mission_Publish
  -> Mission_Confirm
  -> Call_StartFromYellowPages
  -> Phone_StartCall
  -> NPC_LoadData
  -> Dialog_LoadTree
  -> Dialog_ShowNode
  -> Dialog_SelectChoice
      -> Rule_CheckPlayerTagMatch
      -> Rule_CheckNPCTagMatch
      -> Dice_Roll2D6PlusMinus, if needed
      -> Player_StressChange
      -> NPC_BreakdownChange
      -> CallCounter_AddOnPlayerSpeech
      -> EndingEvaluator checks:
          1. NPC breakdown <= 0 -> DeepAnalysis
          2. Call count >= threshold -> DelaySuccess
          3. NPC breakdown >= max -> CallFailed
          4. Player stress >= max -> PlayerBreakdown
  -> Result_Apply...
  -> Result_ShowMissionPopup
  -> Result_ReturnMainRoom
```

## 文件夹分工

```text
Scripts/
  Core/                  主程
  UI/                    主程
  Animation/             主程
  Dialogue/              双人共同
  Gameplay/
    Player/              主程
    NPC/                 副程
    Rules/               副程
    Dice/                副程
    Call/                副程，PhoneController由主程协作
    Items/               主程
    Mission/             主程负责UI流程，副程负责数据状态
    Ending/              副程负责判断，主程负责表现和结算弹窗
  Data/                  双人共同，主程定Player/Item字段，副程定NPC/Dialogue/Ending字段
  Debug/                 双人共同
```

## 第一天：6月20日

目标：工程能跑，流程骨架能从开始游戏进入主小屋，再进入占位通话。

| 任务         | 负责人 | 具体内容                                                     | 交付标准                              | 优先级 |
| ------------ | ------ | ------------------------------------------------------------ | ------------------------------------- | ------ |
| 工程骨架     | 主程   | 创建Boot/MainMenu/MainRoom/Call基础场景或单场景状态切换      | Play后能进入主菜单并开始新游戏        | P0     |
| Core状态机   | 主程   | GameManager、GameState、GameSessionData、SceneFlowController | 能切换MainMenu/MainRoom/InCall/Result | P0     |
| UI占位       | 主程   | 主菜单、主小屋HUD、报纸按钮、黄页按钮、通话面板占位          | 不追求美术，只要流程按钮齐全          | P0     |
| 玩家数据     | 主程   | 基础属性4/4/4/4、人格抽取2张、压力上限和当前压力             | Debug日志能看到初始结果，压力UI可读   | P0     |
| NPC数据      | 副程   | 第一位NPC配置：人格、崩溃初始1、上限3、拖延阈值              | 开始通话时可加载NPC数据               | P0     |
| 对话数据结构 | 双人   | DialogueNode、Choice、Condition、Effect、NextNode            | 能用测试数据跑一个节点和两个选项      | P0     |
| articy验证   | 双人   | 导入测试对话树，确认NodeId/Choice/Tag字段                    | 不阻塞正式对话接入                    | P0     |

## 第二天：6月21日

目标：通话内规则、骰子、计数、压力/崩溃变化跑通。

| 任务               | 负责人 | 具体内容                                                     | 交付标准                       | 优先级 |
| ------------------ | ------ | ------------------------------------------------------------ | ------------------------------ | ------ |
| DialogueController | 双人   | 加载节点、显示文本、显示选项、选择后跳转                     | 第一位NPC可连续选择3个以上节点 | P0     |
| RuleSystem         | 副程   | 检查玩家人格标签和NPC人格标签匹配                            | 选择不同标签可影响压力和崩溃   | P0     |
| StressSystem       | 主程   | 压力变化、Clamp、达到上限触发PlayerBreakdown，并同步压力UI/反馈 | Debug可验证压力到上限          | P0     |
| NPCBreakdownSystem | 副程   | 崩溃变化、Clamp、清零和满值判断                              | Debug可验证清零/挂断           | P0     |
| DiceSystem         | 副程   | 正负2D6、属性加成、难度阈值、成功失败结果                    | 关键选项能进成功/失败分支      | P0     |
| CallCounterSystem  | 副程   | 玩家发言+1、读取拖延阈值、每10句压力+1                       | 计数条或Debug日志能变化        | P0     |
| 通话UI绑定         | 主程   | 文本框、选项按钮、压力条、崩溃条、计数条                     | 玩家能看懂当前数值变化         | P0     |
| 占位反馈           | 主程   | 骰子结果、压力增减、崩溃增减的简单UI反馈                     | 无正式动画也能看见判定结果     | P1     |

## 第三天：6月22日

目标：第一位NPC最小闭环完成，接入占位结算和返回主小屋。

| 任务             | 负责人 | 具体内容                                                     | 交付标准                        | 优先级 |
| ---------------- | ------ | ------------------------------------------------------------ | ------------------------------- | ------ |
| EndingEvaluator  | 副程   | 深度救赎、拖延成功、失败挂断、玩家崩溃判断                   | 至少两种结局可自然触发          | P0     |
| ResultSystem     | 主程   | 接收副程EndingEvaluator输出，应用香烟+1、压力清零、压力上限-1、报纸状态更新 | 结算后数据正确                  | P0     |
| ResultView       | 主程   | 显示结局类型、奖励惩罚、返回按钮                             | 结算后能回主小屋                | P0     |
| Obituary状态刷新 | 主程   | 根据结局淡化/改变/加粗占位显示                               | 返回主小屋能看到报纸变化        | P0     |
| Phone流程        | 主程   | 黄页点击后进入拨号占位动画，动画结束进通话                   | Call_StartFromYellowPages可跑通 | P0     |
| CigaretteSystem  | 主程   | 香烟初始5、使用-1、压力-1、阻止空烟和无压力使用，并触发抽烟UI/动画反馈 | 通话内或主小屋可使用            | P0     |
| 第一位NPC联调    | 双人   | 用真实或半真实文本替换测试节点                               | 第一位NPC从开始到结算可完整跑   | P0     |
| 三日复盘清单     | 双人   | 记录Bug、缺资源、缺文案、循环3风险                           | 循环3开始前有明确补齐清单       | P1     |

## 三日内每人具体模块

| 人员 | 必须完成                                                     | 可延后                                  | 风险点                                                |
| ---- | ------------------------------------------------------------ | --------------------------------------- | ----------------------------------------------------- |
| 主程 | Core状态机、基础UI、通话UI、报纸/黄页/电话流程、Player数值、Stress、Cigarette、ResultSystem/ResultView、动画事件接口 | 正式Spine表现、完整主菜单美术、音效混音 | UI等待美术资源时先用占位Prefab，Player/Item接口先固定 |
| 副程 | NPC数值、Rule、Dice、Counter、Dialogue效果/分支逻辑、Ending判断 | 成就、完整存档、复杂Debug工具           | articy字段不稳定时先用临时测试数据，但Runtime结构不变 |
| 双人 | Dialogue运行时、articy映射、第一位NPC联调                    | 多NPC批量导入工具                       | 对话节点ID和效果字段必须每日和策划/文案确认           |

## 三日最小文件交付

| 文件或Prefab                               | 负责人 | 说明                     |
| ------------------------------------------ | ------ | ------------------------ |
| Scripts/Core/GameManager.cs                | 主程   | 全局入口和状态切换       |
| Scripts/Core/GameSessionData.cs            | 主程   | 运行时总数据             |
| Scripts/UI/Dialogue/DialogueView.cs        | 主程   | 对话文本和选项显示       |
| Scripts/UI/Result/ResultView.cs            | 主程   | 结算展示                 |
| Scripts/Gameplay/Player/PlayerManager.cs   | 主程   | 玩家属性、人格、压力入口 |
| Scripts/Gameplay/NPC/NPCManager.cs         | 副程   | NPC数据和崩溃入口        |
| Scripts/Gameplay/Rules/RuleSystem.cs       | 副程   | 人格匹配                 |
| Scripts/Gameplay/Dice/DiceSystem.cs        | 副程   | 骰子判定                 |
| Scripts/Gameplay/Call/CallCounterSystem.cs | 副程   | 通话计数                 |
| Scripts/Gameplay/Items/CigaretteSystem.cs  | 主程   | 香烟使用                 |
| Scripts/Gameplay/Ending/EndingEvaluator.cs | 副程   | 结局判断                 |
| Scripts/Dialogue/DialogueController.cs     | 双人   | 对话主控                 |
| Scripts/Dialogue/ArticyDialogueAdapter.cs  | 双人   | articy导入适配           |

## 每日验收命令式清单

| 日期    | 验收清单                                                     |
| ------- | ------------------------------------------------------------ |
| 6月20日 | 能启动；能开始新游戏；能进入主小屋；能点报纸和黄页；能进入占位通话 |
| 6月21日 | 能显示对话节点；能选择选项；压力/崩溃/计数会变化；骰子会返回成功失败 |
| 6月22日 | 能结束通话；能显示结算；能返回主小屋；至少两种结局自然触发；第三种可Debug触发 |

## 三日后进入循环3的准备

| 准备项             | 负责人       | 进入循环3前状态                        |
| ------------------ | ------------ | -------------------------------------- |
| 第一位NPC真实文本  | 双人对接文案 | 已接入或字段映射完成                   |
| 第二位NPC数据模板  | 副程         | 可复制第一位NPC配置快速新增            |
| Spine资源接入模板  | 主程         | 至少一个电话或骰子动画Prefab可播放     |
| 结局与报纸变化规则 | 双人对接策划 | 深度救赎/拖延/失败的表现和数值结果确认 |
| Bug清单            | 双人         | P0阻塞项不超过3个且有负责人            |
