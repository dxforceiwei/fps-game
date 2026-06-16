# fps-game（Unity 6 / C#）

第一人稱射擊（FPS）學習專案。從 survivors-like（2D）延伸過來，把通用觀念（Health、敵人追擊、事件、HUD）帶到 3D。

- **引擎**：Unity 6 LTS + URP（3D）
- **輸入**：新版 Input System（直接讀 Mouse/Keyboard 裝置）

## 目前進度

- ✅ **M0**：第一人稱控制器（滑鼠看視角 + WASD + 跳）、Raycast 射擊 + 準心、敵人追擊 + 接觸傷害、Health（玩家/敵人共用）、Game Over HUD。
- ✅ **M1**：敵人生成器 / 波次（隨時間變密）。
- ✅ **M2**：射擊手感（槍口閃光、曳光彈、hitmarker、開火音效）。全部執行時自動產生，不需 Inspector 拉設定。
- ⬜ 更多武器、NavMesh 尋路、3D 模型

## 程式結構（`Assets/Scripts/`）

```
FirstPersonController.cs   滑鼠看視角 + CharacterController 移動 + 跳躍
Core/Health.cs             通用血量（玩家/敵人共用）
Combat/Gun.cs              Physics.Raycast 射擊
Enemies/Enemy.cs           朝玩家移動 + 接觸傷害
UI/HUD.cs                  準心 + 血量 + Game Over（IMGUI）
```

> 註：`Assets/Starter Assets/` 是當初試用後放棄的 Unity 官方控制器，未使用，可刪。

## 套用 3D 模型（槍 / 人）

程式碼跟外觀是分開的：邏輯掛在物件上，模型只是子物件的「外殼」。所以**現在就能換模型，不用改任何腳本**。

**槍**
1. 匯入模型（`.fbx` / `.glb`）到 `Assets/Models/`，Unity 會自動轉成可用資產。
2. 把模型拖到相機（或玩家手部）底下當子物件，調好位置/縮放 → 這就是 view model。
3. 在模型槍口處建一個空物件 `Muzzle`，拉進 `Gun` 的 **Muzzle** 欄位 → 閃光與曳光彈就從槍口發射。

**敵人 / 人物**
1. 匯入「有骨架（rigged）」的人物模型，建議用 [Mixamo](https://www.mixamo.com)（免費、可直接下載走路/攻擊動畫）。
2. 把 `Enemy.prefab` 裡的膠囊外觀換成模型子物件（保留 Collider 與 `Health`、`Enemy` 腳本）。
3. 加 `Animator` + Animator Controller，動畫之後再接（`Enemy.cs` 已經有移動方向可驅動 walk 動畫）。

> 我（Claude）能寫好程式與掛點，但**模型/貼圖是二進位美術檔，得你自己匯入**。免費來源：Mixamo（人）、Kenney、Quaternius、Unity Asset Store、Sketchfab(CC)。

## Mac 編輯器小提醒

macOS 編輯器鎖游標常失效 → 用 **Play Maximized**（Game 工具列）測滑鼠視角；打包成 .app 後游標鎖定 100% 正常。
