#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FPS.EditorTools
{
    /// <summary>
    /// 一鍵把 Mixamo 角色 + 動畫套到 Enemy.prefab：
    /// 設成 Humanoid → 跑步動畫去掉位移(root motion) → 自動建 AnimatorController →
    /// 把模型掛進敵人、加 Animator、藏掉膠囊外觀。
    /// 用法：上方選單 FPS → 套用敵人模型+動畫。
    /// </summary>
    public static class EnemySetup
    {
        // 檔名照你下載的。想換動畫改這幾個路徑即可。
        private const string Swat       = "Assets/Animation/Swat.fbx";
        private const string IdleFbx    = "Assets/Animation/Remy@Rifle Aiming Idle.fbx";
        private const string RunFbx     = "Assets/Animation/Remy@Run Forward.fbx";
        private const string AttackFbx  = "Assets/Animation/Remy@Firing Rifle.fbx";
        private const string Controller = "Assets/Animation/EnemyAnimator.controller";
        private const string EnemyPrefab = "Assets/Prefabs/Enemy.prefab";

        [MenuItem("FPS/套用敵人模型+動畫")]
        public static void Setup()
        {
            // 0) 確認檔案都在（沒匯入就提醒先點回 Unity 讓它 import）
            foreach (var p in new[] { Swat, IdleFbx, RunFbx, AttackFbx })
            {
                if (AssetImporter.GetAtPath(p) == null)
                {
                    EditorUtility.DisplayDialog("找不到檔案",
                        $"找不到 {p}\n\n如果檔案其實在資料夾裡，請先點回 Unity 視窗等它匯入完成，再執行一次。", "OK");
                    return;
                }
            }

            // 1) 全部設成 Humanoid；跑步動畫去掉水平位移、設成循環
            MakeHumanoid(Swat, loop: false, bakeXZ: false);
            MakeHumanoid(IdleFbx, loop: true, bakeXZ: false);
            MakeHumanoid(RunFbx, loop: true, bakeXZ: true);
            MakeHumanoid(AttackFbx, loop: false, bakeXZ: false);
            AssetDatabase.Refresh();

            // 2) 抓 avatar 與三個 clip
            var avatar = LoadAvatar(Swat);
            var idle = LoadClip(IdleFbx);
            var run = LoadClip(RunFbx);
            var attack = LoadClip(AttackFbx);
            if (avatar == null || idle == null || run == null || attack == null)
            {
                EditorUtility.DisplayDialog("抓不到動畫/Avatar",
                    $"avatar={(avatar!=null)} idle={(idle!=null)} run={(run!=null)} attack={(attack!=null)}\n" +
                    "通常是 Unity 還沒匯入完。請等 Console 不再轉圈後再執行一次。", "OK");
                return;
            }

            // 3) 建 AnimatorController（Idle ↔ Run 用 Speed 切換，Attack 由 trigger 觸發）
            var ctrl = BuildController(idle, run, attack);

            // 4) 把模型 + Animator 裝進 Enemy.prefab
            var root = PrefabUtility.LoadPrefabContents(EnemyPrefab);
            try
            {
                var old = root.transform.Find("Model");
                if (old != null) Object.DestroyImmediate(old.gameObject);

                // 藏掉原本的膠囊外觀（保留 CapsuleCollider 給射線/接觸判定）
                var mr = root.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;

                var swatGo = AssetDatabase.LoadAssetAtPath<GameObject>(Swat);
                var model = Object.Instantiate(swatGo);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = new Vector3(0f, -1f, 0f); // 腳對齊膠囊底部
                model.transform.localRotation = Quaternion.identity;

                var anim = model.GetComponent<Animator>();
                if (anim == null) anim = model.AddComponent<Animator>();
                anim.runtimeAnimatorController = ctrl;
                anim.avatar = avatar;
                anim.applyRootMotion = false; // 位移交給 Enemy.cs

                PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[EnemySetup] 完成！Enemy.prefab 已換成 Swat 模型 + Idle/Run/Attack 動畫。" +
                      "若模型位置/大小不對，開 Enemy.prefab 調 Model 的 localPosition / Scale。");
            EditorUtility.DisplayDialog("完成", "敵人模型+動畫已套好，按 Play 試試。", "讚");
        }

        // ── helpers ────────────────────────────────────────────────

        private static void MakeHumanoid(string path, bool loop, bool bakeXZ)
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(path);
            imp.animationType = ModelImporterAnimationType.Human;
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            var clips = imp.defaultClipAnimations;
            if (clips != null && clips.Length > 0)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    clips[i].loopTime = loop;
                    if (bakeXZ)
                    {
                        clips[i].lockRootPositionXZ = true; // 把水平位移烘進原地 → 跑步不會自己往前飄
                        clips[i].lockRootHeightY = true;
                    }
                }
                imp.clipAnimations = clips;
            }
            imp.SaveAndReimport();
        }

        private static AnimatorController BuildController(AnimationClip idle, AnimationClip run, AnimationClip attack)
        {
            if (File.Exists(Controller)) AssetDatabase.DeleteAsset(Controller);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(Controller);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            var sm = ctrl.layers[0].stateMachine;
            var sIdle = sm.AddState("Idle");   sIdle.motion = idle;
            var sRun = sm.AddState("Run");      sRun.motion = run;
            var sAtk = sm.AddState("Attack");   sAtk.motion = attack;
            sm.defaultState = sIdle;

            var toRun = sIdle.AddTransition(sRun);
            toRun.hasExitTime = false; toRun.duration = 0.15f;
            toRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            var toIdle = sRun.AddTransition(sIdle);
            toIdle.hasExitTime = false; toIdle.duration = 0.15f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            var toAtk = sm.AddAnyStateTransition(sAtk);
            toAtk.hasExitTime = false; toAtk.duration = 0.05f; toAtk.canTransitionToSelf = false;
            toAtk.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            var atkBack = sAtk.AddTransition(sIdle);
            atkBack.hasExitTime = true; atkBack.exitTime = 0.8f; atkBack.duration = 0.1f;

            AssetDatabase.SaveAssets();
            return ctrl;
        }

        private static AnimationClip LoadClip(string path)
        {
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                if (o is AnimationClip c && !c.name.StartsWith("__preview")) return c;
            return null;
        }

        private static Avatar LoadAvatar(string path)
        {
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                if (o is Avatar a) return a;
            return null;
        }
    }
}
#endif
