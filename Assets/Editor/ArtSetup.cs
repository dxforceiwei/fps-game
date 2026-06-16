#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FPS.EditorTools
{
    /// <summary>
    /// 一鍵把 CC0 槍枝模型掛到玩家相機底下（view model），並建立 Muzzle 接到 Gun。
    /// 用法：Unity 上方選單 → FPS → 套用槍枝模型到玩家。
    /// 模型來源：Kenney Blaster Kit（CC0，見 Assets/Models/Blasters/License.txt）。
    /// </summary>
    public static class ArtSetup
    {
        // 想換別把槍就改這個檔名（blaster-a ~ blaster-r 都有）
        private const string BlasterPath = "Assets/Models/Blasters/blaster-h.fbx";

        [MenuItem("FPS/套用槍枝模型到玩家")]
        public static void AttachGun()
        {
            var gun = Object.FindFirstObjectByType<Gun>();
            if (gun == null)
            {
                EditorUtility.DisplayDialog("找不到 Gun",
                    "場景裡沒有掛 Gun 元件的物件。請先確定玩家身上有 Gun。", "OK");
                return;
            }

            // 找玩家相機（通常是玩家底下的子相機；退而求其次用 Main Camera）
            Camera cam = gun.GetComponentInChildren<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null)
            {
                EditorUtility.DisplayDialog("找不到相機", "場景裡找不到相機。", "OK");
                return;
            }

            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(BlasterPath);
            if (fbx == null)
            {
                EditorUtility.DisplayDialog("還沒匯入模型",
                    $"找不到 {BlasterPath}。\n請先讓 Unity 完成匯入（回到 Unity 視窗等它編譯），再執行一次。", "OK");
                return;
            }

            // 移除上次掛的，避免重複
            var oldModel = cam.transform.Find("GunModel");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);
            var oldMuzzle = cam.transform.Find("Muzzle");
            if (oldMuzzle != null) Object.DestroyImmediate(oldMuzzle.gameObject);

            // 槍模型：掛在相機底下，放在右下前方當 view model
            var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx, cam.transform);
            model.name = "GunModel";
            model.transform.localPosition = new Vector3(0.18f, -0.18f, 0.45f);
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // 朝向之後在 Scene 微調
            model.transform.localScale = Vector3.one * 0.5f;

            // Muzzle：掛相機底下、放在槍口大致位置，當曳光彈/閃光起點
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(cam.transform, false);
            muzzle.transform.localPosition = new Vector3(0.18f, -0.12f, 0.9f);

            // 把 Muzzle 寫進 Gun 的私有 [SerializeField] muzzle 欄位
            var so = new SerializedObject(gun);
            var prop = so.FindProperty("muzzle");
            if (prop != null)
            {
                prop.objectReferenceValue = muzzle.transform;
                so.ApplyModifiedProperties();
            }

            // 存檔讓變更寫回場景
            EditorUtility.SetDirty(gun);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cam.gameObject.scene);

            Selection.activeGameObject = model;
            Debug.Log($"[ArtSetup] 已把 {System.IO.Path.GetFileName(BlasterPath)} 掛到相機底下並接好 Muzzle。" +
                      "選到 GunModel，用 Move/Rotate 工具在 Game 視窗對著畫面微調到順眼即可。");
        }
    }
}
#endif
