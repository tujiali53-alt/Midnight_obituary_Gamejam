# 《明日讣告》Demo 程序需求ID总表（最终整合版）

角色：CHR_场景：BG_UI：UI_卡牌：CARD_物品：ITEM_音乐：BGM_Bar音效：SFX_Button动画：ANI_DiceRoll预制体：PF_脚本：自定义，比如DiceSystem.cs

命名模式。XXX_XXX_v3.2*例如   item_axe_v3.2*   意思是 3循环期间做的物品-斧头，第二版，目前的最新版。

## 主菜单

| 功能模块   | 需求ID       | 程序命名              | 类型   | 触发条件     | 程序逻辑                 | 优先级 |
| ---------- | ------------ | --------------------- | ------ | ------------ | ------------------------ | ------ |
| 主菜单系统 | SYS_MENU_001 | Menu_ShowMain         | 主菜单 | 游戏启动     | 显示主菜单界面           | P0     |
| 主菜单系统 | SYS_MENU_002 | Menu_StartNewGame     | 主菜单 | 点击开始游戏 | 创建新游戏并进入开场流程 | P0     |
| 主菜单系统 | SYS_MENU_003 | Menu_ContinueGame     | 主菜单 | 点击继续游戏 | 读取存档进入游戏         | P1     |
| 主菜单系统 | SYS_MENU_004 | Menu_OpenAchievements | 主菜单 | 点击成就     | 打开成就界面             | P1     |
| 主菜单系统 | SYS_MENU_005 | Menu_OpenCredits      | 主菜单 | 点击作者信息 | 打开制作人员界面         | P2     |
| 主菜单系统 | SYS_MENU_006 | Menu_QuitGame         | 主菜单 | 点击退出     | 关闭游戏                 | P0     |
|            |              |                       |        |              |                          |        |

## 游戏

| 功能模块 | 需求ID       | 程序命名           | 类型 | 触发条件          | 程序逻辑                   | 优先级 |
| -------- | ------------ | ------------------ | ---- | ----------------- | -------------------------- | ------ |
| 游戏系统 | SYS_GAME_001 | Game_Init          | 系统 | 新游戏开始        | 初始化运行时数据           | P0     |
| 游戏系统 | SYS_GAME_002 | Game_EnterMainRoom | 场景 | 开场结束          | 进入主小屋场景             | P0     |
| 游戏系统 | SYS_GAME_003 | Game_Restart       | 系统 | 点击重新开始      | 清空运行时数据并重新初始化 | P1     |
| 游戏系统 | SYS_UI_001   | UI_SetInteractable | UI   | 全屏界面打开/关闭 | 锁定或释放底层场景交互     | P0     |
|          |              |                    |      |                   |                            |        |

## 教学

| 功能模块 | 需求ID      | 程序命名                      | 类型 | 触发条件       | 程序逻辑                   | 优先级 |
| -------- | ----------- | ----------------------------- | ---- | -------------- | -------------------------- | ------ |
| 教学系统 | SYS_TUT_001 | Tutorial_CheckFirstRun        | 教学 | 新游戏开始     | 判断是否进入首次教学       | P1     |
| 教学系统 | SYS_TUT_002 | Tutorial_StartSequence        | 教学 | 进入主小屋     | 锁定自由操作并启动教学流程 | P1     |
| 教学系统 | SYS_TUT_003 | Tutorial_ShowPersonalityCard  | 教学 | 教学开始       | 引导点击人格牌             | P1     |
| 教学系统 | SYS_TUT_004 | Tutorial_ShowStressSystem     | 教学 | 人格牌查看后   | 介绍玩家压力系统           | P1     |
| 教学系统 | SYS_TUT_005 | Tutorial_ShowBreakdownSystem  | 教学 | 压力教学后     | 介绍NPC崩溃系统            | P1     |
| 教学系统 | SYS_TUT_006 | Tutorial_ShowPersonalityRules | 教学 | 崩溃教学后     | 说明人格与压力/崩溃关系    | P1     |
| 教学系统 | SYS_TUT_007 | Tutorial_ShowCallPenalty      | 教学 | 人格规则教学后 | 说明每10句增加1压力        | P1     |
| 教学系统 | SYS_TUT_008 | Tutorial_ForceInitialStress   | 教学 | 新游戏初始化   | 玩家初始压力为1            | P1     |
| 教学系统 | SYS_TUT_009 | Tutorial_ForceSmokeUse        | 教学 | 压力教学结束   | 强制抽烟一次               | P1     |
| 教学系统 | SYS_TUT_010 | Tutorial_OpenNewspaper        | 教学 | 抽烟结束       | 引导打开报纸               | P1     |
| 教学系统 | SYS_TUT_011 | Tutorial_ShowMissionPanel     | 教学 | 报纸打开       | 引导查看任务               | P1     |
| 教学系统 | SYS_TUT_012 | Tutorial_CloseNewspaper       | 教学 | 任务确认       | 引导关闭报纸               | P1     |
| 教学系统 | SYS_TUT_013 | Tutorial_StartCall            | 教学 | 报纸关闭       | 引导点击黄页开始通话       | P1     |
| 教学系统 | SYS_TUT_014 | Tutorial_EndSequence          | 教学 | 电话接通       | 结束教学                   | P1     |
|          |             |                               |      |                |                            |        |

## 玩家

| 功能模块 | 需求ID         | 程序命名                     | 类型     | 触发条件   | 程序逻辑                                    | 优先级 |
| -------- | -------------- | ---------------------------- | -------- | ---------- | ------------------------------------------- | ------ |
| 玩家系统 | SYS_PLAYER_001 | Player_InitBaseStats         | 玩家     | 新游戏开始 | 初始化基础属性4/4/4/4                       | P0     |
| 玩家系统 | SYS_PLAYER_002 | Player_InitPersonality       | 玩家     | 新游戏开始 | 随机抽取2张人格卡                           | P0     |
| 玩家系统 | SYS_PLAYER_003 | Player_ApplyPersonalityStats | 玩家     | 人格生成后 | 根据人格修正属性                            | P0     |
| 玩家系统 | SYS_PLAYER_004 | Player_ShowInitStats         | 玩家     | 人格生成后 | 显示最终属性                                | P1     |
| 玩家系统 | SYS_PLAYER_005 | Player_StressChange          | 数值     | 压力变化时 | 修改玩家压力值，并限制在0到当前压力上限之间 | P0     |
| 玩家系统 | SYS_PLAYER_006 | Player_CheckBreakdown        | 失败判断 | 压力变化后 | 当前压力值达到压力上限时触发精神失控结局    | P0     |
| 玩家系统 | SYS_PLAYER_007 | Player_OpenPersonalityInfo   | 玩家     | 点击人格牌 | 打开人格说明纸条                            | P0     |
|          |                |                              |          |            |                                             |        |

## 规则

| 功能模块 | 需求ID       | 程序命名                 | 类型 | 触发条件     | 程序逻辑             | 优先级 |
| -------- | ------------ | ------------------------ | ---- | ------------ | -------------------- | ------ |
| 规则系统 | SYS_RULE_001 | Rule_CheckPlayerTagMatch | 规则 | 玩家选择选项 | 判断是否符合玩家人格 | P0     |
| 规则系统 | SYS_RULE_002 | Rule_CheckNPCTagMatch    | 规则 | 玩家选择选项 | 判断是否符合NPC人格  | P0     |
|          |              |                          |      |              |                      |        |

## 道具

| 功能模块 | 需求ID       | 程序命名                      | 类型 | 触发条件     | 程序逻辑         | 优先级 |
| -------- | ------------ | ----------------------------- | ---- | ------------ | ---------------- | ------ |
| 道具系统 | SYS_ITEM_001 | Item_Cigarette_Init           | 道具 | 新游戏开始   | 初始化香烟数量5  | P0     |
| 道具系统 | SYS_ITEM_002 | Item_Cigarette_RequestUse     | 道具 | 点击香烟     | 进入抽烟确认状态 | P0     |
| 道具系统 | SYS_ITEM_003 | Item_Cigarette_CheckCondition | 道具 | 点击确认抽烟 | 检查使用条件     | P0     |
| 道具系统 | SYS_ITEM_004 | Item_Cigarette_ConfirmUse     | 道具 | 条件满足     | 香烟-1，压力-1   | P0     |
| 道具系统 | SYS_ITEM_005 | Item_Cigarette_BlockEmpty     | 道具 | 香烟≤0       | 阻止使用         | P0     |
| 道具系统 | SYS_ITEM_006 | Item_Cigarette_BlockNoNeed    | 道具 | 压力≤0       | 阻止使用         | P0     |
| 道具系统 | SYS_ITEM_007 | Item_Cigarette_ClampMax       | 道具 | 香烟增加时   | 香烟上限5        | P0     |
|          |              |                               |      |              |                  |        |

## 任务与报纸

| 功能模块       | 需求ID          | 程序命名                  | 类型 | 触发条件   | 程序逻辑                           | 优先级 |
| -------------- | --------------- | ------------------------- | ---- | ---------- | ---------------------------------- | ------ |
| 任务与报纸系统 | SYS_OBIT_001    | Obituary_LoadCurrent      | 报纸 | 返回主小屋 | 刷新报纸内容                       | P0     |
| 任务与报纸系统 | SYS_OBIT_002    | Obituary_OpenUI           | 报纸 | 点击报纸   | 打开报纸界面                       | P0     |
| 任务与报纸系统 | SYS_OBIT_003    | Obituary_UpdateState      | 报纸 | 任务结算后 | 更新新闻内容                       | P0     |
| 任务与报纸系统 | SYS_MISSION_001 | Mission_Publish           | 任务 | 阅读报纸后 | 发布当前任务                       | P0     |
| 任务与报纸系统 | SYS_MISSION_002 | Mission_OpenPanel         | 任务 | 点击任务框 | 显示任务详情与奖励                 | P0     |
| 任务与报纸系统 | SYS_MISSION_003 | Mission_Confirm           | 任务 | 点击确认   | 接受任务                           | P0     |
| 任务与报纸系统 | SYS_CALL_001    | Call_StartFromYellowPages | 流程 | 点击黄页   | 播放查号/拨号动画后进入当前NPC通话 | P0     |
|                |                 |                           |      |            |                                    |        |

## 电话与NPC

| 功能模块      | 需求ID        | 程序命名             | 类型     | 触发条件     | 程序逻辑                                              | 优先级 |
| ------------- | ------------- | -------------------- | -------- | ------------ | ----------------------------------------------------- | ------ |
| 电话与NPC系统 | SYS_PHONE_001 | Phone_StartCall      | 电话     | 动画结束     | 进入通话界面                                          | P0     |
| 电话与NPC系统 | SYS_PHONE_002 | Phone_EndCall        | 电话     | 通话结束     | 卸载通话数据并进入结算                                | P0     |
| 电话与NPC系统 | SYS_NPC_001   | NPC_LoadData         | NPC      | 通话开始     | 加载NPC固定人格、初始崩溃值1、崩溃上限3、拖延阈值配置 | P0     |
| 电话与NPC系统 | SYS_NPC_002   | NPC_ShowPersonality  | NPC      | 通话开始     | 显示NPC人格                                           | P0     |
| 电话与NPC系统 | SYS_NPC_003   | NPC_BreakdownChange  | 数值     | 玩家发言后   | 修改NPC崩溃值，并限制在0到崩溃上限之间                | P0     |
| 电话与NPC系统 | SYS_NPC_004   | NPC_CheckHangup      | 失败判断 | 崩溃值变化后 | 达到上限则挂断                                        | P0     |
| 电话与NPC系统 | SYS_NPC_005   | NPC_InitDefaultState | NPC      | 通话开始     | 初始化NPC当前崩溃值为1                                | P0     |
|               |               |                      |          |              |                                                       |        |

## 对话与骰子

| 功能模块       | 需求ID         | 程序命名                    | 类型     | 触发条件   | 程序逻辑                                     | 优先级 |
| -------------- | -------------- | --------------------------- | -------- | ---------- | -------------------------------------------- | ------ |
| 对话与骰子系统 | SYS_DIALOG_001 | Dialog_LoadTree             | 对话     | 通话开始   | 加载对话树、节点跳转、判定条件与深度解析条件 | P0     |
| 对话与骰子系统 | SYS_DIALOG_002 | Dialog_ShowNode             | 对话     | 进入节点   | 显示文本与选项                               | P0     |
| 对话与骰子系统 | SYS_DIALOG_003 | Dialog_SelectChoice         | 对话     | 选择选项   | 执行逻辑结算                                 | P0     |
| 对话与骰子系统 | SYS_DIALOG_004 | Dialog_ApplyPlayerTagResult | 对话数值 | 规则判断后 | 根据人格匹配结果修改玩家压力值               | P0     |
| 对话与骰子系统 | SYS_DIALOG_005 | Dialog_ApplyNPCTagResult    | 对话数值 | 规则判断后 | 根据人格匹配结果修改NPC崩溃值                | P0     |
| 对话与骰子系统 | SYS_DIALOG_006 | Dialog_NodeJump             | 对话     | 结算完成   | 跳转到下一对话节点                           | P0     |
| 对话与骰子系统 | SYS_DICE_001   | Dice_Roll2D6PlusMinus       | 骰子     | 触发判定   | 投掷正骰与负骰                               | P0     |
| 对话与骰子系统 | SYS_DICE_002   | Dice_CheckResult            | 骰子     | 投骰结束   | 计算判定是否成功                             | P0     |
| 对话与骰子系统 | SYS_DICE_003   | Dice_ApplyBranch            | 骰子     | 判定结束   | 根据成功或失败进入对应分支                   | P0     |
|                |                |                             |          |            |                                              |        |

## 通话计数与反馈

| 功能模块           | 需求ID           | 程序命名                      | 类型     | 触发条件           | 程序逻辑                                    | 优先级 |
| ------------------ | ---------------- | ----------------------------- | -------- | ------------------ | ------------------------------------------- | ------ |
| 通话计数与反馈系统 | SYS_COUNT_001    | CallCounter_Init              | 计数     | 通话开始           | 初始化通话计数，并读取当前NPC配置的拖延阈值 | P0     |
| 通话计数与反馈系统 | SYS_COUNT_002    | CallCounter_AddOnPlayerSpeech | 计数     | 玩家发言完成       | 通话计数+1                                  | P0     |
| 通话计数与反馈系统 | SYS_COUNT_003    | CallCounter_CheckDelayEnding  | 结局判断 | 计数变化后         | 达到目标则触发拖延成功结局                  | P0     |
| 通话计数与反馈系统 | SYS_COUNT_004    | CallCounter_StressMilestone   | 压力     | 每累计10句玩家发言 | 玩家压力+1                                  | P0     |
| 通话计数与反馈系统 | SYS_COUNT_005    | CallCounter_SmokeCravingHint  | 提示     | 通话导致压力增加时 | 提示玩家想抽烟了                            | P1     |
| 通话计数与反馈系统 | SYS_FEEDBACK_001 | Feedback_PlayerStressGain     | 反馈     | 玩家压力增加       | 播放压力增加反馈                            | P0     |
| 通话计数与反馈系统 | SYS_FEEDBACK_002 | Feedback_PlayerStressReduce   | 反馈     | 玩家压力减少       | 播放压力减少反馈                            | P0     |
| 通话计数与反馈系统 | SYS_FEEDBACK_003 | Feedback_NPCBreakdownGain     | 反馈     | NPC崩溃值增加      | 播放崩溃增加反馈                            | P0     |
| 通话计数与反馈系统 | SYS_FEEDBACK_004 | Feedback_NPCBreakdownReduce   | 反馈     | NPC崩溃值减少      | 播放崩溃减少反馈                            | P0     |
|                    |                  |                               |          |                    |                                             |        |

## 结局与结算

| 功能模块       | 需求ID         | 程序命名                   | 类型     | 触发条件          | 程序逻辑                                         | 优先级 |
| -------------- | -------------- | -------------------------- | -------- | ----------------- | ------------------------------------------------ | ------ |
| 结局与结算系统 | SYS_END_001    | Ending_UnlockAnalysisRoute | 结局门槛 | NPC崩溃值≤0       | 解锁深度解析路线                                 | P0     |
| 结局与结算系统 | SYS_END_002    | Ending_CheckDeepAnalysis   | 结局判断 | 深度解析节点成功  | 根据文案配置的深度解析条件判断并触发深度解析结局 | P0     |
| 结局与结算系统 | SYS_END_003    | Ending_DelaySuccess        | 结局     | 达到拖延阈值      | 触发拖延成功结局                                 | P0     |
| 结局与结算系统 | SYS_END_004    | Ending_CallFailed          | 结局     | NPC崩溃值达到上限 | 触发任务失败结局                                 | P0     |
| 结局与结算系统 | SYS_END_005    | Ending_PlayerBreakdown     | 结局     | 玩家压力达到上限  | 触发精神失控结局                                 | P0     |
| 结局与结算系统 | SYS_RESULT_001 | Result_ApplyDeepAnalysis   | 结算     | 深度解析成功      | 香烟+1，并将对应讣告替换为普通新闻               | P0     |
| 结局与结算系统 | SYS_RESULT_005 | Result_ReturnMainRoom      | 结算     | 结算完成          | 返回主小屋场景                                   | P0     |
| 结局与结算系统 | SYS_RESULT_002 | Result_ApplyDelaySuccess   | 结算     | 拖延成功          | 香烟+1，并将对应讣告替换为普通新闻               | P0     |
| 结局与结算系统 | SYS_RESULT_003 | Result_ApplyFailure        | 结算     | 任务失败          | 玩家压力上限-1，但不得低于3                      | P0     |
| 结局与结算系统 | SYS_RESULT_004 | Result_ShowMissionPopup    | 结算     | 任意结局          | 显示任务结果、奖励与惩罚                         | P0     |
|                |                |                            |          |                   |                                                  |        |

## 成就与存档

| 功能模块       | 需求ID       | 程序命名                  | 类型 | 触发条件       | 程序逻辑                                   | 优先级 |
| -------------- | ------------ | ------------------------- | ---- | -------------- | ------------------------------------------ | ------ |
| 成就与存档系统 | SYS_ACH_001  | Achievement_Init          | 成就 | 游戏启动       | 初始化成就数据                             | P1     |
| 成就与存档系统 | SYS_ACH_002  | Achievement_CheckOnResult | 成就 | 任务结算后     | 检查成就达成条件                           | P1     |
| 成就与存档系统 | SYS_ACH_003  | Achievement_Unlock        | 成就 | 达成条件满足   | 解锁成就                                   | P1     |
| 成就与存档系统 | SYS_ACH_004  | Achievement_ShowToast     | 成就 | 成就解锁       | 弹出成就提示                               | P1     |
| 成就与存档系统 | SYS_ACH_005  | Achievement_OpenUI        | 成就 | 打开成就界面   | 显示成就列表                               | P1     |
| 成就与存档系统 | SYS_ACH_006  | Achievement_ShowList      | 成就 | 成就界面打开   | 显示已解锁与未解锁成就                     | P1     |
| 成就与存档系统 | SYS_ACH_007  | Achievement_SaveState     | 成就 | 成就状态变化后 | 保存成就数据                               | P1     |
| 成就与存档系统 | SYS_SAVE_001 | Save_RuntimeState         | 存档 | 每次结算后     | 保存玩家状态、任务状态、新闻状态与成就状态 | P1     |