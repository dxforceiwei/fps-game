using System;
using UnityEngine;

namespace FPS
{
    /// <summary>
    /// 通用血量元件，玩家和敵人共用（就像 2D 專案的 PlayerHealth，但這裡做成可重用）。
    /// 受傷由外部呼叫 TakeDamage；歸零發 Died 事件，誰要處理死亡自己去聽。
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;

        private float current;

        public float Current => current;
        public float Max => maxHealth;
        public bool IsDead => current <= 0f;

        public event Action Died;                 // 死亡
        public event Action<float, float> Changed; // (目前, 最大) 給 HUD 用

        private void Awake() => current = maxHealth;

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            current = Mathf.Max(0f, current - amount);
            Changed?.Invoke(current, maxHealth);

            if (current <= 0f) Died?.Invoke();
        }
    }
}
