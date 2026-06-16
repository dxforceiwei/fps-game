using UnityEngine;

namespace FPS
{
    /// <summary>
    /// 敵人生成器：隨時間在玩家周圍環形生成敵人，間隔越來越短（波次壓力）。
    /// 跟 2D 的 SpawnDirector 同概念。掛在一個空物件上、指定 Enemy prefab。
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Enemy enemyPrefab;

        [Header("生成節奏（隨時間變密）")]
        [SerializeField] private float startInterval = 2f;
        [SerializeField] private float minInterval = 0.4f;
        [Tooltip("花多少秒從 start 降到 min")]
        [SerializeField] private float rampSeconds = 180f;

        [Header("生成位置 / 上限")]
        [Tooltip("在玩家周圍多遠生成（畫面外）")]
        [SerializeField] private float spawnRadius = 25f;
        [Tooltip("敵人站立的地面高度")]
        [SerializeField] private float groundY = 1f;
        [Tooltip("同時存活上限")]
        [SerializeField] private int maxAlive = 40;

        private Transform player;
        private float timer;
        private float elapsed;

        private void Start()
        {
            var pc = Object.FindFirstObjectByType<FirstPersonController>();
            if (pc != null) player = pc.transform;
        }

        private void Update()
        {
            if (player == null || enemyPrefab == null) return;

            elapsed += Time.deltaTime;
            float t = rampSeconds > 0f ? Mathf.Clamp01(elapsed / rampSeconds) : 1f;
            float interval = Mathf.Lerp(startInterval, minInterval, t);

            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                if (CountAlive() < maxAlive) Spawn();
            }
        }

        private int CountAlive() =>
            Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;

        private void Spawn()
        {
            Vector2 ring = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 pos = player.position + new Vector3(ring.x, 0f, ring.y);
            pos.y = groundY;
            Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
    }
}
