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
        [Tooltip("命中目標時 hitmarker 顯示秒數")]
        [SerializeField] private float hitmarkerTime = 0.12f;

        private bool gameOver;
        private GUIStyle style;
        private float hitmarkerTimer; // > 0 時畫 hitmarker

        private void Start()
        {
            if (playerHealth == null)
            {
                var pc = Object.FindFirstObjectByType<FirstPersonController>();
                if (pc != null) playerHealth = pc.GetComponent<Health>();
            }
            if (playerHealth != null) playerHealth.Died += OnPlayerDied;

            // 訂閱槍的命中事件，命中就閃 hitmarker
            var gun = Object.FindFirstObjectByType<Gun>();
            if (gun != null) gun.HitConfirmed += OnHitConfirmed;
        }

        private void Update()
        {
            if (hitmarkerTimer > 0f) hitmarkerTimer -= Time.unscaledDeltaTime;
        }

        private void OnHitConfirmed() => hitmarkerTimer = hitmarkerTime;

        private void OnPlayerDied()
        {
            gameOver = true;
            Time.timeScale = 0f; // 凍結
        }

        // 在中央準心畫一個小小的 X（四條斜短線）
        private void DrawHitmarker(float cx, float cy)
        {
            GUI.color = Color.white;
            const float gap = 4f, len = 7f, thick = 2f;
            // 用旋轉的細長矩形畫四道斜線
            Matrix4x4 prev = GUI.matrix;
            void Slash(float angle, float ox, float oy)
            {
                GUIUtility.RotateAroundPivot(angle, new Vector2(cx, cy));
                GUI.DrawTexture(new Rect(cx + ox, cy + oy, len, thick), Texture2D.whiteTexture);
                GUI.matrix = prev;
            }
            Slash(45f, gap, -thick * 0.5f);
            Slash(45f, -gap - len, -thick * 0.5f);
            Slash(-45f, gap, -thick * 0.5f);
            Slash(-45f, -gap - len, -thick * 0.5f);
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

            // 命中回饋（hitmarker）
            if (hitmarkerTimer > 0f) DrawHitmarker(cx, cy);

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
