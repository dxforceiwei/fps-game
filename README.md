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

**槍（已附 CC0 模型，一鍵套用）**

`Assets/Models/Blasters/` 已放入 [Kenney Blaster Kit](https://kenney.nl/assets/blaster-kit)（CC0，18 把 `blaster-a~r.fbx` + 貼圖）。

1. 開 Unity，等它把 `Assets/Models/Blasters/` 匯入完成（Console 不再轉圈）。
2. 上方選單 **FPS → 套用槍枝模型到玩家**。會自動：把 `blaster-h` 掛到相機底下當 view model、建立 `Muzzle` 並接到 `Gun`。
3. 選到階層裡的 `GunModel`，用移動/旋轉工具對著 Game 視窗微調到順眼（view model 的位置很吃個人喜好）。
4. 想換別把槍：改 `Assets/Editor/ArtSetup.cs` 裡的 `BlasterPath`（`blaster-a`~`blaster-r`），再點一次選單。
5. 槍若是灰白色，點該 `.fbx` → Inspector **Materials → Extract Materials**，把材質的 Base Map 指到 `Textures/colormap.png`。

**敵人 / 人物（用 Mixamo，免費）**
1. 到 [mixamo.com](https://www.mixamo.com)（Adobe 免費帳號）選一個角色，例如 **Zombie / Mutant / X Bot**。
2. 下載：**Format = FBX for Unity(.fbx)**、**Pose = T-pose**、**With Skin** → 這是本體；放到 `Assets/Models/Enemy/`。
3. 再下載動畫（同角色，**Without Skin**）：至少 **Walking** + 一個 **Attack/Punch**（殭屍就抓 Zombie 系列）。
4. 把模型放到 `Enemy.prefab` 底下當外觀（保留膠囊上的 Collider、`Health`、`Enemy` 腳本），加 `Animator`。
5. 動畫接線（`Enemy.cs` 已有移動方向可驅動 walk）我可以再幫你寫，先把檔案放進來即可。

> 我（Claude）能下載 CC0 模型、寫好掛點與一鍵腳本，但 **Mixamo 需登入、貼圖/動畫是二進位美術檔**，這部分得你自己匯入。免費來源：Kenney、Quaternius、[poly.pizza](https://poly.pizza)、Mixamo（人）、Sketchfab(CC)。

## Mac 編輯器小提醒

macOS 編輯器鎖游標常失效 → 用 **Play Maximized**（Game 工具列）測滑鼠視角；打包成 .app 後游標鎖定 100% 正常。
