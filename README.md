# Motion Lab：交互式开关门动画系统

虚拟现实动画设计课程作业。以实验室入口为场景，演示 Unity 关键帧动画、Animator 状态机、角色碰撞触发、音效与 UI 提示。

## 打开项目

1. 使用 **Unity 2020.3.0f1c1** 打开本项目文件夹，或在 Unity Hub 的“打开”中选择它。
2. 在 Project 面板打开 `Assets/Scenes/DoorDemo.unity`。
3. 点击播放按钮，再点击 Game 画面以接收键盘输入。
4. 首次导入时等待 Package Manager 和脚本编译完成。项目使用内置渲染管线及 Unity 自带组件，不需要 Mixamo、XR 头显或额外插件。

## 操作

| 按键 | 功能 |
| --- | --- |
| WASD | 移动 |
| 鼠标 | 观察 |
| E | 靠近门后开门或关门，仅在按键模式有效 |
| M | 在按键交互与自动感应模式之间切换 |
| R | 返回入口起点 |
| Esc | 释放鼠标 |
| 鼠标左键 | 在 Game 画面重新锁定鼠标 |

按键模式需要进入 2.6 米交互范围。自动模式下，玩家进入门前后感应区域时开门，离开后关门。一次动画持续 1.2 秒，最大开角 102°。运动中忽略重复手动输入，自动请求保留到当前动作结束再执行。

## 结构

```text
Assets/
  Scenes/DoorDemo.unity
  Scripts/
    DoorController.cs       门状态、按键入口、Animator 参数及音效
    DoorTrigger.cs          角色进入/退出检测与碰撞体计数
    PlayerController.cs     移动、观察与重置
    LabHud.cs               模式、门状态和中文交互提示
  Animations/
    DoorAnimator.controller
    Door_Closed.anim        关闭状态
    Door_Open.anim          开门关键帧
    Door_Opened.anim        打开状态
    Door_Close.anim         关门关键帧
  Audio/                    原创合成开关门音效
  Materials/                内置 Standard Shader 材质
  Editor/
    DoorLabBuilder.cs       场景搭建与动画检查辅助工具
    DoorLabValidation.cs    Play 模式验收工具
Docs/                       PDF 设计报告与验证结果
```

门对象层级为 `DoorRig > Hinge > DoorPanel`。Animator 挂在 DoorRig 上，旋转曲线绑定 Hinge 的本地 Y 轴，门板按半宽偏移，使旋转中心位于侧边。StatusLight 的强度关键帧与门轴旋转放在同一动画片段中。

## 动画与验收

Animator 使用布尔参数 `Open`：`Closed → Opening → Opened → Closing → Closed`。两段运动动画关闭 Loop Time，旋转曲线使用缓入缓出。

- 选择 DoorRig，在 Animation 窗口检查 `Door_Open`、`Door_Close` 的门轴及灯光曲线。
- 顶部菜单 `Door Lab` 提供打开场景、录制准备、保存和验收功能。F5 打开演示场景，F6/F7 分别准备开门/关门片段。`4. Save all` 保存全部资源。
- `Door Lab > 5. Run acceptance checks` 会进入 Play 模式执行验收，期间会移动测试角色。请勿在正式演示过程中启动。
- `Door Lab > 1. Build demonstration scene` 会重建演示场景，仅用于复现搭建，不要对已自行修改的场景执行此项。

在本机 Unity Play 模式完成 **23 项验收，全部通过**，覆盖近/远交互、重复输入、Animator 终态、音效启动、灯光变化、真实触发器进出与途中反向请求。完整记录和运行截图见 [Docs/Verification](Docs/Verification/)。

设计报告：[交互式开关门动画系统-设计报告.pdf](Docs/交互式开关门动画系统-设计报告.pdf)。报告记录了门轴层级、关键帧、状态机、代码分工、测试与修复。

## 素材与环境

场景全部使用 Unity 基本几何体和自制材质。两个 WAV 音效为原创程序合成，没有使用第三方人物或音频资源。运行时中文 UI 从 Windows 字体读取，优先使用 Microsoft YaHei。其他操作系统需要可用的中文字体。

项目不包含 Library、Temp、Logs 等缓存。请保留 Assets 中的 `.meta` 文件，以免引用失效。GitHub 仓库为私有，交作业时需要让教师账号拥有访问权限。

## 参考资料

- 课程教学 PPT《第1章 Unity动画系统入门》
- 作业要求 PPT《演示文稿1》
- Unity 2020.3 Animation 窗口：https://docs.unity3d.com/2020.3/Documentation/Manual/animeditor-CreatingANewAnimationClip.html
- Unity 2020.3 Animator：https://docs.unity3d.com/2020.3/Documentation/Manual/class-Animator.html
- Unity 2020.3 触发器：https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Collider.OnTriggerEnter.html
