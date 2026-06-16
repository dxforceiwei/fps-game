using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS
{
    /// <summary>
    /// 射擊：按住左鍵依冷卻從「相機正前方」打一條射線（hitscan）。
    /// Physics.Raycast 是 3D 版的命中判定，取代 2D 專案用的距離比較。
    /// 射線命中有 Health 的東西就扣血。掛在玩家身上。
    ///
    /// 射擊手感（全部在執行時自動產生，不需在 Inspector 拉東西）：
    /// 槍口閃光（一盞瞬亮的燈）、曳光彈（LineRenderer）、命中回饋事件（HUD 畫 hitmarker）、
    /// 開火音效（沒指定 AudioClip 就用程式合成的噪音爆裂）。
    /// </summary>
    public class Gun : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 100f;
        [Tooltip("連射間隔（秒）")]
        [SerializeField] private float fireCooldown = 0.15f;

        [Header("槍口位置（選填）")]
        [Tooltip("曳光彈與閃光的起點；留空就用相機前方一點點")]
        [SerializeField] private Transform muzzle;

        [Header("槍口閃光")]
        [SerializeField] private bool muzzleFlash = true;
        [SerializeField] private Color flashColor = new Color(1f, 0.85f, 0.4f);
        [SerializeField] private float flashIntensity = 20f;
        [SerializeField] private float flashRange = 8f;
        [Tooltip("閃光持續秒數")]
        [SerializeField] private float flashTime = 0.04f;

        [Header("曳光彈")]
        [SerializeField] private bool tracer = true;
        [SerializeField] private Color tracerColor = new Color(1f, 0.9f, 0.5f);
        [SerializeField] private float tracerWidth = 0.03f;
        [SerializeField] private float tracerTime = 0.04f;

        [Header("音效")]
        [SerializeField] private bool playSound = true;
        [Tooltip("留空就用程式合成的開槍聲")]
        [SerializeField] private AudioClip shotClip;
        [Range(0f, 1f)][SerializeField] private float volume = 0.5f;

        /// <summary>命中有 Health 的目標時觸發，給 HUD 畫 hitmarker。</summary>
        public event Action HitConfirmed;

        private float timer;
        private Light flashLight;
        private AudioSource audioSrc;
        private Material tracerMat;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
            SetupEffects();
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
            Vector3 from = muzzle != null ? muzzle.position : origin;
            Vector3 to = origin + dir * range; // 沒打到東西時曳光彈的終點

            if (Physics.Raycast(origin, dir, out RaycastHit hit, range))
            {
                to = hit.point;

                // 命中物或其父層上若有 Health 就扣血
                Health h = hit.collider.GetComponentInParent<Health>();
                if (h != null)
                {
                    h.TakeDamage(damage);
                    HitConfirmed?.Invoke(); // 通知 HUD 畫 hitmarker
                }
            }

            PlayShotFeedback(from, to);
        }

        // ── 開火回饋 ────────────────────────────────────────────────

        private void PlayShotFeedback(Vector3 from, Vector3 to)
        {
            if (muzzleFlash && flashLight != null) StartCoroutine(FlashOnce());
            if (tracer) SpawnTracer(from, to);
            if (playSound && audioSrc != null) audioSrc.PlayOneShot(audioSrc.clip, volume);
        }

        private System.Collections.IEnumerator FlashOnce()
        {
            if (muzzle != null) flashLight.transform.position = muzzle.position;
            flashLight.enabled = true;
            yield return new WaitForSeconds(flashTime);
            flashLight.enabled = false;
        }

        private void SpawnTracer(Vector3 from, Vector3 to)
        {
            var go = new GameObject("Tracer");
            var lr = go.AddComponent<LineRenderer>();
            lr.material = tracerMat;
            lr.startColor = lr.endColor = tracerColor;
            lr.startWidth = lr.endWidth = tracerWidth;
            lr.numCapVertices = 2;
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);
            Destroy(go, tracerTime);
        }

        // ── 初始化（執行時自動產生燈、音源、材質）────────────────────

        private void SetupEffects()
        {
            // 槍口閃光：一盞預設關閉的點光源，開火時瞬亮
            var lightGo = new GameObject("MuzzleFlash");
            lightGo.transform.SetParent(muzzle != null ? muzzle : transform, false);
            flashLight = lightGo.AddComponent<Light>();
            flashLight.type = LightType.Point;
            flashLight.color = flashColor;
            flashLight.intensity = flashIntensity;
            flashLight.range = flashRange;
            flashLight.enabled = false;

            // 音源：沒指定 clip 就合成一段噪音爆裂當槍聲
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 0f; // 2D，固定在耳邊
            audioSrc.clip = shotClip != null ? shotClip : MakeShotClip();

            // 曳光彈材質：URP Unlit，找不到就退回永遠存在的 Sprites/Default
            Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            tracerMat = new Material(sh);
            if (tracerMat.HasProperty("_BaseColor")) tracerMat.SetColor("_BaseColor", tracerColor);
            tracerMat.color = tracerColor;
        }

        /// <summary>程式合成一段約 0.12 秒、快速衰減的白噪音，當作開槍聲。</summary>
        private AudioClip MakeShotClip()
        {
            const int sampleRate = 44100;
            int len = (int)(sampleRate * 0.12f);
            var data = new float[len];
            var rng = new System.Random();
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)len;
                float env = Mathf.Pow(1f - t, 3f);              // 快速衰減包絡
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                data[i] = noise * env;
            }
            var clip = AudioClip.Create("ShotSfx", len, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
