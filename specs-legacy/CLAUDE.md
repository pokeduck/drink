# specs-legacy/

> [!WARNING]
> **此目錄為 Legacy，不要新增、不要修改、不要刪除任何檔案。**
>
> 本目錄為早期 Addy Osmani spec-driven development 流程的產物，**現行已停用**。
> 任何新規格、修改、移除一律透過 **OpenSpec** 走 `../openspec/` 的 lifecycle。
> 若 legacy 文件與 `../openspec/specs/` 衝突，**一律以 `openspec/specs/` 為準**。

## 為什麼還留著

- 部分既有功能背景說明仍寫在這裡，閱讀時可作為脈絡參考
- 完整 Error Code Registry 等歷史對照仍指向 `SPEC.md`
- 已歸納成 OpenSpec 的部分由 `../openspec/changes/archive/2026-04-26-migrate-specs-to-purpose-requirements` 完成搬遷

## 內容物（僅供讀取）

```
specs-legacy/
├── SPEC.md                      # 全域 spec（含 Error Code Registry）
├── index.md                     # 舊版索引
├── admin-admin-user.md          # 後台帳號
├── admin-notification.md        # 後台通知
├── admin-order.md               # 後台訂單
├── admin-role.md                # 後台角色 / Menu
├── admin-system-setting.md      # 系統設定
├── admin-user-management.md     # 後台管前台會員
├── admin-verification.md        # 驗證信
├── drink-option.md              # 飲料選項
├── shop.md                      # 店家與菜單
├── user-auth.md                 # 前台會員認證
└── user-order.md                # 前台揪團訂單
```

## 何時可參考

- **可以**：理解既有功能的歷史脈絡、查 Error Code Registry、追早期決策原因
- **不可以**：當作新功能依據、視為 source of truth、依此產生新 spec

## 與其他層的關係

- 現行 spec 流程 → `../openspec/`（OpenSpec 為唯一 source of truth）
- Spec 規範細則 → `../.claude/rules/spec.md`
- Legacy 規範背景說明 → `../.claude/rules/spec-legacy-osmani.md`

## 不要做的事

- **不要新增**任何檔案到本目錄
- **不要修改**現有檔案（即使發現過時也不修；改寫請走 OpenSpec change）
- **不要刪除**檔案（保留歷史脈絡）
- **不要**沿用 Osmani 六大區域 / 三層 Boundaries 格式撰寫新 spec
