using UnityEngine;

namespace FPS
{
    /// <summary>
    /// 最小 HUD（IMGUI，零設定）：畫面中央準心 + 玩家血量 + Game Over。
    /// 跟 2D 專案的 HUD 同套做法。掛在任一空物件上，會自動找玩家血量。
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        private bool gameOver;
        private GUIStyle style;

        private void Start()
        {
            if (playerHealth == null)
            {
                var pc = Object.FindFirstObjectByType<FirstPersonController>();
                if (pc != null) playerHealth = pc.GetComponent<Health>();
            }
            if (playerHealth != null) playerHealth.Died += OnPlayerDied;
        }

        private void OnPlayerDied()
        {
            gameOver = true;
            Time.timeScale = 0f; // 凍結
        }

        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                style.normal.textColor = Color.white;
            }

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;

            // 準心（中央小白點）
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(cx - 2f, cy - 2f, 4f, 4f), Texture2D.whiteTexture);

            // 血量
            if (playerHealth != null)
                GUI.Label(new Rect(20, 15, 320, 30),
                    $"HP {Mathf.CeilToInt(playerHealth.Current)} / {Mathf.CeilToInt(playerHealth.Max)}", style);

            // Game Over
            if (gameOver)
            {
                var big = new GUIStyle(style) { fontSize = 48, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(0, cy - 30f, Screen.width, 60f), "GAME OVER", big);
            }
        }
    }
}
