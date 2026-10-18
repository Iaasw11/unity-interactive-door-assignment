using System.Collections.Generic;
using UnityEngine;

/// <summary>角色碰撞触发区。触发器只负责检测，具体开关门行为交给 DoorController。</summary>
[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class DoorTrigger : MonoBehaviour
{
    public DoorController door;
    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private BoxCollider volume;
    public int OccupantCount { get { return occupants.Count; } }

    private void Awake() { volume = GetComponent<BoxCollider>(); }

    private void FixedUpdate() { RefreshOccupants(); }

    /// <summary>
    /// 禁用 CharacterController 或传送角色时，物理系统可能不发送 Exit。
    /// 用边界检查清除残留引用，保证 R 重置后仍能自动关门。
    /// </summary>
    public void RefreshOccupants()
    {
        if (volume == null) volume = GetComponent<BoxCollider>();
        int removed = occupants.RemoveWhere(c => c == null || !c.enabled ||
            !c.gameObject.activeInHierarchy || !volume.bounds.Intersects(c.bounds));
        if (removed > 0 && door != null) door.NotifyOccupancy(occupants.Count > 0);
    }

    private bool IsPlayer(Collider other)
    {
        return other.GetComponentInParent<PlayerController>() != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        occupants.Add(other);
        if (door != null) door.NotifyOccupancy(true);
    }

    // 某些控制器重新启用后先收到 Stay，也要恢复正确的区域占用记录。
    private void OnTriggerStay(Collider other)
    {
        if (IsPlayer(other) && occupants.Add(other) && door != null) door.NotifyOccupancy(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!occupants.Remove(other)) return;
        if (door != null) door.NotifyOccupancy(occupants.Count > 0);
    }

    private void OnDisable()
    {
        occupants.Clear();
        if (door != null) door.NotifyOccupancy(false);
    }
}
