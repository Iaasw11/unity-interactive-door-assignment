using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>仅使用内置输入系统与 CharacterController，支持桌面键鼠演示。</summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform view;
    public float moveSpeed = 3.2f;
    public float mouseSensitivity = 1.8f;
    public Vector3 startPosition = new Vector3(0, 0.08f, -5.8f);
    private CharacterController controller;
    private float pitch;
    private float verticalVelocity;

    private void Awake() { controller = GetComponent<CharacterController>(); }
    private void Start() { LockCursor(true); }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked &&
            (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())) LockCursor(true);
        if (Input.GetKeyDown(KeyCode.R)) ResetPosition();

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -65, 70);
            if (view != null) view.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2;
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        Vector3 input = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1);
        Vector3 direction = transform.TransformDirection(input) * moveSpeed;
        direction.y = verticalVelocity;
        controller.Move(direction * Time.deltaTime);
    }

    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    /// <summary>重置与测试共用，暂时关闭控制器，防止传送被碰撞约束抵消。</summary>
    public void Teleport(Vector3 position, float yaw = 0)
    {
        controller.enabled = false;
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, yaw, 0);
        if (view != null) view.localRotation = Quaternion.identity;
        pitch = 0;
        verticalVelocity = 0;
        controller.enabled = true;
        Physics.SyncTransforms();
    }

    public void ResetPosition() { Teleport(startPosition); }
}
