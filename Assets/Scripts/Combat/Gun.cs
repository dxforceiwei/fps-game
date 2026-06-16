using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS
{
    /// <summary>
    /// 射擊：按住左鍵依冷卻從「相機正前方」打一條射線（hitscan）。
    /// Physics.Raycast 是 3D 版的命中判定，取代 2D 專案用的距離比較。
    /// 射線命中有 Health 的東西就扣血。掛在玩家身上。
    /// </summary>
    public class Gun : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 100f;
        [Tooltip("連射間隔（秒）")]
        [SerializeField] private float fireCooldown = 0.15f;

        private float timer;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
            timer -= Time.deltaTime;

            // 按住左鍵連射
            if (Mouse.current != null && Mouse.current.leftButton.isPressed && timer <= 0f)
            {
                Fire();
                timer = fireCooldown;
            }
        }

        private void Fire()
        {
            if (cam == null) return;

            // origin 往前推一點，避免射線從玩家自己的碰撞體內部出發而打到自己
            Vector3 origin = cam.transform.position + cam.transform.forward * 0.6f;
            Vector3 dir = cam.transform.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, range))
            {
                // 命中物或其父層上若有 Health 就扣血
                Health h = hit.collider.GetComponentInParent<Health>();
                if (h != null) h.TakeDamage(damage);

                Debug.DrawLine(origin, hit.point, Color.red, 0.1f); // Scene 視窗可見射線（除錯用）
            }
        }
    }
}
