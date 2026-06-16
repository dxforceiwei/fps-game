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
        private Animator anim;       // 有掛模型才有；沒有就全部跳過（仍能跑膠囊版）
        private float contactTimer;

        // 動畫參數（由 FPS → 套用敵人模型+動畫 建好的 Animator 使用）
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private void Start()
        {
            // 找玩家（用控制器當標記），抓它的位置與血量
            var pc = Object.FindFirstObjectByType<FirstPersonController>();
            if (pc != null)
            {
                player = pc.transform;
                playerHealth = pc.GetComponent<Health>();
            }

            anim = GetComponentInChildren<Animator>(); // 模型子物件上的 Animator
            GetComponent<Health>().Died += OnDied;     // 自己死了就移除
        }

        private void OnDied() => Destroy(gameObject);

        private void Update()
        {
            if (player == null) return;

            Vector3 to = player.position - transform.position;
            to.y = 0f; // 只在地面追，不要飛起來
            float dist = to.magnitude;
            Vector3 dir = dist > 0.001f ? to / dist : Vector3.zero;
            bool inRange = dist <= contactRange;

            // 到貼身距離就停下來打，否則往玩家走
            if (!inRange) transform.position += dir * (moveSpeed * Time.deltaTime);
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

            // 驅動動畫：走路速度 → idle/run 切換
            if (anim != null) anim.SetFloat(SpeedHash, inRange ? 0f : moveSpeed, 0.1f, Time.deltaTime);

            // 接觸傷害（同時播攻擊動畫）
            contactTimer -= Time.deltaTime;
            if (inRange && contactTimer <= 0f)
            {
                if (playerHealth != null) playerHealth.TakeDamage(contactDamage);
                if (anim != null) anim.SetTrigger(AttackHash);
                contactTimer = contactInterval;
            }
        }
    }
}
