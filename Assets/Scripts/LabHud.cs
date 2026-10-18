using UnityEngine;
using UnityEngine.UI;

/// <summary>显示模式、门状态、交互提示；中文字体从操作系统读取，不依赖额外插件。</summary>
public class LabHud : MonoBehaviour
{
    public DoorController door;
    public PlayerController player;
    public Text modeText;
    public Text stateText;
    public Text promptText;
    public Text subtitleText;
    public Image statusDot;

    private void Awake()
    {
        Font chinese = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial Unicode MS", "Arial" }, 24);
        if (chinese != null)
            foreach (Text text in GetComponentsInChildren<Text>(true)) text.font = chinese;
    }

    private void Update()
    {
        if (door == null) return;
        modeText.text = door.automaticMode ? "自动感应模式" : "按键交互模式";
        stateText.text = door.StateLabel;
        statusDot.color = door.IsMoving ? new Color(1, 0.7f, 0.25f) : (door.IsOpen ? new Color(0.25f, 1, 0.7f) : new Color(0.25f, 0.85f, 1));
        if (door.IsMoving)
        {
            promptText.text = door.StateLabel + "…";
            subtitleText.text = "门板运动中，请稍候";
        }
        else if (door.automaticMode)
        {
            promptText.text = door.IsOpen ? "通道已开启" : "走近感应区，门将自动开启";
            subtitleText.text = "离开感应区后自动关闭  ·  按 M 切换模式";
        }
        else
        {
            promptText.text = door.PlayerNearby ? (door.IsOpen ? "按 E 关闭舱门" : "按 E 开启舱门") : "靠近舱门开始交互";
            subtitleText.text = door.PlayerNearby ? "门轴旋转动画  ·  灯光与音效同步" : "WASD 移动  ·  鼠标观察  ·  按 M 切换模式";
        }
    }

    public void SwitchMode() { if (door != null) door.SetAutomaticMode(!door.automaticMode); }
    public void ResetPlayer() { if (player != null) player.ResetPosition(); }
}
