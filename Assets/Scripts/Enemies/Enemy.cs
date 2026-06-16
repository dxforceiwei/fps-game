using UnityEngine;

namespace FPS
{
    /// <summary>
    /// 敵人（遠程兵）：跑向玩家，進入射程就停下來朝玩家開槍（射線命中扣血）。
    /// 血歸零就消失。需要 Collider 才能被射線打到，需要 Health。
    /// 動畫：Speed 參數切 idle/run，開槍時觸發 Attack。
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        [Header("移動")]
        [SerializeField] private float moveSpeed = 3f;

        [Header("遠程攻擊")]
        [Tooltip("進到這個距離內就停下來開槍")]
        [SerializeField] private float shootRange = 12f;
        [SerializeField] private float shotDamage = 8f;
        [Tooltip("每幾秒開一槍")]
        [SerializeField] private float fireInterval = 1.2f;

        private Transform player;
        private Health playerHealth;
        private Animator anim;       // 有掛模型才有；沒有就全部跳過（仍能跑膠囊版）
        private float fireTimer;

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
            bool inRange = dist <= shootRange;

            // 射程外就追，進射程就停下來打
            if (!inRange) transform.position += dir * (moveSpeed * Time.deltaTime);
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

            // 驅動動畫：移動時 run、停下時 idle（瞄準待機）
            if (anim != null) anim.SetFloat(SpeedHash, inRange ? 0f : moveSpeed, 0.1f, Time.deltaTime);

            // 進射程定時開槍
            fireTimer -= Time.deltaTime;
            if (inRange && fireTimer <= 0f)
            {
                Shoot();
                fireTimer = fireInterval;
            }
        }

        private void Shoot()
        {
            if (playerHealth != null) playerHealth.TakeDamage(shotDamage);
            if (anim != null) anim.SetTrigger(AttackHash); // 播開槍動畫
        }
    }
}
