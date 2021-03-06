# For those who have security concern

Right, this mod will check announcement and new version from my server via announcement.txt and version.txt.

And if there is a new version, you can choose to click the update button(named "立即更新" before v1.3).

This will start downloading the latest mod dll from my server and create a bat file to replace the old one and restart the game client.

The bat file you can find at the UpdateVersion() function.

And I will show it below.


`@echo off`

`taskkill /f /im Craftopia.exe`

`del /q CraftopiaInGameTrainer.dll`

`move CraftopiaInGameTrainer.new CraftopiaInGameTrainer.dll`

`start steam://rungameid/1307550`

`del %0`


As you can see, this bat is safe.

What about the new dll?

You can use dotPeek or dnSpy or whatever to check the code of the new dll. As for open source project, the dll is not encrypted. You can check it easily.

You know how to do it right?

If you still can't believe me, you can choose to skip the update by simply click the button(named "下次再说"  before v1.3).

# How to use
1.Download the zip file(_.Craftopia_InGameTrainer_v1.1.zip
) from the release tag. 

As for latest version:https://github.com/simurayousuke/CraftopiaInGameTrainer/releases/tag/1.1a

(At bottom of [this page](https://github.com/simurayousuke/CraftopiaInGameTrainer/releases/tag/1.1a))

2.Follow the video named "2使用教程整合版.mp4" in the zip file.

The file name is in Chinese but it's ok for those who can't speak Chinese.

3.Start the game, in the pop up menu click "关闭" and then click "立即更新", the game will restart and then there you go.

# Hotkeys & Functions
## Hotkeys
Home Show/Hide the Menu

F1	Infinite HP

F2	Infinite MP

F3	Infinite Stamina

F4	Infinite Satiety

F5	Add Money(add 10000 per click as default)

F6	Add Exp(add 10000 per click as default)

F7	Add Skill Point(add 1 per click as default)

## Other Functions
You can enable/disable other functions in the menu(press HOME to show).

Such as:

* Set max player level to 100.

* Repairing doesn't need money.

* Repair fastly.

* Repairing never fail.

* etc.

# 更新历史
## 1.4
Add a button to toggle dev console which is built in game.

## 1.3
Add English support.

## 1.2
Add buttons.

## 1.1
1.移除ConfigurationManager软依赖，菜单由自己实现。

2.增加主界面载入后显示公告的功能。

3.加入新版本检测功能。

4.加入自动更新功能。

# Bug report/Suggest
Use the issue system of github please.

## Menu Translation
"已开启" means "Enabled" while "已关闭" means "Disabled".

![Menu](https://github.com/simurayousuke/CraftopiaInGameTrainer/blob/master/trainer.png?raw=true)

# 使用方法
1.将Craftopia文件夹内文件解压到游戏根目录

2.如果有重复文件而你不知道怎么处理请选择覆盖全部(这会覆盖掉BepinEx的二进制文件及配置文件)

3.打开游戏

4.按Home(默认)呼出菜单

# 默认快捷键
Home 显示/隐藏菜单

F1	无限血

F2	无限蓝

F3	无限耐力（绿条）

F4	永不饥饿

F5	增加金钱（默认一下10000）

F6	增加经验（默认一下10000）

F7	增加技能点（默认一下1点）

# 其他功能
*玩家等级上限提升为100级

*免费修理

*修理不会失败

*快速修理

*设置升级获得的技能点数

其他功能请在菜单中打开，Enable为开启，Disable为关闭。

（懂配置文件的玩家也可以手动修改配置文件开启）

# Bug反馈
请使用Github的Issue系统汇报Bug，谢谢您的合作。
