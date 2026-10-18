using UnityEngine;

/// <summary>
/// 门的交互入口。动画由 Animator/AnimationClip 播放，脚本不直接逐帧旋转门板。
/// Open 参数决定目标状态；运动期间锁定手动操作，自动请求会保留到当前动画结束。
/// </summary>
[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class DoorController : MonoBehaviour
{
    [Header("场景引用")]
    public Transform player;
    public Transform hinge;
    public DoorTrigger sensor;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("交互与动画")]
    public float interactRange = 2.6f;
    public float animationDuration = 1.2f;
    public bool automaticMode;

    public bool IsOpen { get; private set; }
    public bool IsMoving { get; private set; }
    public bool RequestedOpen { get; private set; }
    public int TransitionCount { get; private set; }
    public bool PlayerNearby
    {
        get
        {
            if (player == null) return false;
            Vector3 delta = player.position - transform.position;
            delta.y = 0;
            return delta.sqrMagnitude <= interactRange * interactRange;
        }
    }
    public string StateLabel { get { return IsMoving ? (movingToOpen ? "正在开启" : "正在关闭") : (IsOpen ? "已开启" : "已关闭"); } }

    private Animator doorAnimator;
    private AudioSource sound;
    private bool movingToOpen;
    private float finishTime;
    private static readonly int OpenId = Animator.StringToHash("Open");

    private void Awake()
    {
        doorAnimator = GetComponent<Animator>();
        sound = GetComponent<AudioSource>();
        sound.playOnAwake = false;
        sound.spatialBlend = 0.65f;
        sound.volume = 0.48f;
        doorAnimator.SetBool(OpenId, false);
    }

    private void Update()
    {
        // 运动结束后再接受下一个方向，避免关键帧从起点重播造成跳变。
        // 时间阈值与 Animator 的终态同时满足才算完成，避免提前一帧反向请求。
        bool animationFinished = doorAnimator.GetCurrentAnimatorStateInfo(0).IsName(movingToOpen ? "Opened" : "Closed");
        if (IsMoving && Time.time >= finishTime && animationFinished)
        {
            IsMoving = false;
            IsOpen = movingToOpen;
        }
        if (!IsMoving && IsOpen != RequestedOpen) BeginMotion(RequestedOpen);

        if (Input.GetKeyDown(KeyCode.M)) SetAutomaticMode(!automaticMode);
        if (!automaticMode && Input.GetKeyDown(KeyCode.E)) TryInteract();
    }

    /// <summary>范围和状态检查集中在这里，UI、按键和测试共用同一入口。</summary>
    public bool TryInteract()
    {
        if (automaticMode || !PlayerNearby || IsMoving) return false;
        RequestOpen(!RequestedOpen);
        return true;
    }

    /// <summary>自动区域可在门运动时更新目标；本次动画结束后执行最后的目标。</summary>
    public void RequestOpen(bool value)
    {
        RequestedOpen = value;
        if (!IsMoving && IsOpen != value) BeginMotion(value);
    }

    private void BeginMotion(bool opening)
    {
        IsMoving = true;
        movingToOpen = opening;
        finishTime = Time.time + animationDuration;
        TransitionCount++;
        doorAnimator.SetBool(OpenId, opening);
        // 每个完整动作只播放一次。停止上一段声音，避免反复操作时堆叠。
        sound.Stop();
        AudioClip clip = opening ? openSound : closeSound;
        if (clip != null) sound.PlayOneShot(clip);
    }

    public void SetAutomaticMode(bool enabled)
    {
        automaticMode = enabled;
        if (enabled)
        {
            if (sensor != null) sensor.RefreshOccupants();
            RequestOpen(sensor != null && sensor.OccupantCount > 0);
        }
    }

    public void NotifyOccupancy(bool occupied)
    {
        if (automaticMode) RequestOpen(occupied);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up, interactRange);
    }
}
