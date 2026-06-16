# fps-game（Unity 6 / C#）

第一人稱射擊（FPS）學習專案。從 survivors-like（2D）延伸過來，把通用觀念（Health、敵人追擊、事件、HUD）帶到 3D。

- **引擎**：Unity 6 LTS + URP（3D）
- **輸入**：新版 Input System（直接讀 Mouse/Keyboard 裝置）

## 目前進度

- ✅ **M0**：第一人稱控制器（滑鼠看視角 + WASD + 跳）、Raycast 射擊 + 準心、敵人追擊 + 接觸傷害、Health（玩家/敵人共用）、Game Over HUD。
- ⬜ 敵人生成器 / 波次
- ⬜ 射擊手感（槍口火花、hitmarker、音效）
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

## Mac 編輯器小提醒

macOS 編輯器鎖游標常失效 → 用 **Play Maximized**（Game 工具列）測滑鼠視角；打包成 .app 後游標鎖定 100% 正常。
