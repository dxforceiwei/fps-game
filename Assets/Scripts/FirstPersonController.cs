using UnityEngine;
using UnityEngine.InputSystem; // 新版 Input System（專案已裝）

namespace FPS
{
    /// <summary>
    /// 最小第一人稱控制器：滑鼠看視角 + WASD 移動 + 跳躍。
    /// 用 CharacterController 處理移動與重力，直接讀 Mouse/Keyboard 裝置（不需 Action 資產）。
    /// 掛在玩家膠囊上；相機要當它的子物件、拖進 Camera Transform 欄位。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("移動")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("視角")]
        [Tooltip("玩家的子物件相機")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("滑鼠靈敏度")]
        [SerializeField] private float mouseSensitivity = 0.15f;
        [Tooltip("上下視角夾住的角度")]
        [SerializeField] private float pitchClamp = 85f;

        private CharacterController cc;
        private float pitch;       // 累積的上下視角
        private float verticalVel;  // 垂直速度（重力 / 跳躍）
        private bool lookEnabled = true; // 視角是否啟用（Esc 關、點畫面開）

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Start()
        {
            LockCursor(true); // 開場鎖定+隱藏游標
        }

        private void Update()
        {
            HandleCursor();
            Look();
            Move();
        }

        private void HandleCursor()
        {
            // Esc 關閉視角（放游標）、點畫面重新接管。
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                lookEnabled = false;
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                lookEnabled = true;

            LockCursor(lookEnabled);
        }

        private void Look()
        {
            if (Mouse.current == null || !lookEnabled) return;

            // 直接用系統滑鼠位移。編輯器搭配 Play Maximized 就有足夠範圍；
            // 打包後游標會真正鎖定，可無限轉。
            Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;

            // 左右：旋轉整個身體（yaw）
            transform.Rotate(Vector3.up * delta.x);

            // 上下：只轉相機（pitch），並夾住範圍避免翻過頭
            pitch -= delta.y;
            pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move()
        {
            if (Keyboard.current == null) return;

            // WASD（直接讀按鍵狀態）
            float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            float z = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
            Vector3 input = new Vector3(x, 0f, z);
            if (input.sqrMagnitude > 1f) input.Normalize(); // 斜走不加速

            // 依玩家面向換算成世界方向
            Vector3 move = transform.right * input.x + transform.forward * input.z;

            // 重力與跳躍
            if (cc.isGrounded)
            {
                if (verticalVel < 0f) verticalVel = -2f; // 貼地，避免越積越快
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                    verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity); // 由跳躍高度反推初速
            }
            verticalVel += gravity * Time.deltaTime;

            Vector3 velocity = move * moveSpeed + Vector3.up * verticalVel;
            cc.Move(velocity * Time.deltaTime);
        }

        private void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
