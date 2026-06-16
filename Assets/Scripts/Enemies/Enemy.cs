using UnityEngine;

namespace FPS
{
    /// <summary>
    /// 敵人：朝玩家走（只在水平面 XZ 移動）、貼近就週期性扣玩家血、自己血歸零就消失。
    /// 跟 2D 的敵人同個概念，只是換成 3D 向量。需要 Collider 才能被射線打到，需要 Health。
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float contactDamage = 10f;
        [Tooltip("貼到這個距離內就造成接觸傷害")]
        [SerializeField] private float contactRange = 1.6f;
        [SerializeField] private float contactInterval = 1f;

        private Transform player;
        private Health playerHealth;
        private float contactTimer;

        private void Start()
        {
            // 找玩家（用控制器當標記），抓它的位置與血量
            var pc = Object.FindFirstObjectByType<FirstPersonController>();
            if (pc != null)
            {
                player = pc.transform;
                playerHealth = pc.GetComponent<Health>();
            }

            GetComponent<Health>().Died += OnDied; // 自己死了就移除
        }

        private void OnDied() => Destroy(gameObject);

        private void Update()
        {
            if (player == null) return;

            Vector3 to = player.position - transform.position;
            to.y = 0f; // 只在地面追，不要飛起來
            float dist = to.magnitude;
            Vector3 dir = dist > 0.001f ? to / dist : Vector3.zero;

            // 移動 + 面向玩家
            transform.position += dir * (moveSpeed * Time.deltaTime);
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

            // 接觸傷害
            contactTimer -= Time.deltaTime;
            if (dist <= contactRange && contactTimer <= 0f)
            {
                if (playerHealth != null) playerHealth.TakeDamage(contactDamage);
                contactTimer = contactInterval;
            }
        }
    }
}
