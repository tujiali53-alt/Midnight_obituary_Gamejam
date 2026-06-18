测试：

# 游戏主场景：SCN_MainRoom

Canvas_Root：整张屏幕，所有 UI 都放在它下面

Button_OpenNewspaper：打开报纸按钮

Button_ConfirmMission：确认/接受任务按钮

Button_OpenYellowPages：打开黄页按钮

Button_Dial：拨号按钮

Panel_Newspaper：报纸弹窗

Panel_YellowPages：黄页弹窗

Text_HUD：左上角状态文字，比如压力、香烟

Text_Obituary：报纸里的讣告文字

Text_YellowPages：黄页里的电话信息

EventSystem：Unity UI 点击系统，必须有

MainRoomController：控制这些按钮的脚本物体，不显示在画面上

# SCN_Call，也就是电话通话界面。这些东西的意思是：

Text_NPC：显示当前 NPC 名字和人格
例：Lena [Feeling]

Text_Dialogue：显示 NPC 当前说的话
例：Who is this? Why are you calling so late?

Text_HUD：显示状态
例：玩家压力、香烟数量、NPC 崩溃值、通话计数

Text_Result：显示结算结果
例：Result: DeepRedemption

PF_UI_ChoiceButton_C1_v1.0：对话选项按钮模板
游戏运行时会复制它，生成多个可点击选项。
它不是玩家固定看到的按钮，而是“按钮模板”。

Button_ReturnMainRoom：返回主房间按钮
通话结束后点它回 SCN_MainRoom

Group_ChoiceButtons：放对话选项按钮的容器
程序会把复制出来的选项按钮放到这里。

CallController：控制通话逻辑的空物体
它不显示，只负责读取对话、处理选项、刷新 UI。

SceneFlowController：负责切场景
比如从通话场景回主房间。
