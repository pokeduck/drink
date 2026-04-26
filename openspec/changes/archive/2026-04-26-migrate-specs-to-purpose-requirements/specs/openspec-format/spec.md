## ADDED Requirements

### Requirement: Main spec 結構

`openspec/specs/{capability}/spec.md` SHALL 以下列結構撰寫，作為 capability 的當前契約來源：

1. 第一個非空白 line 為 `# {capability}` 一級標題（與目錄名一致）
2. 接著 `## Purpose` 段落，以 1–3 句說明該 capability 的目的與涵蓋範圍
3. 接著 `## Requirements` 段落，包含一個或多個 `### Requirement:` 條目
4. 每個 `### Requirement:` 至少含一個 `#### Scenario:` 區塊

Main spec SHALL NOT 直接使用 `## ADDED Requirements` / `## MODIFIED Requirements` / `## REMOVED Requirements` 等 delta 標頭 —— 這些標頭僅用於 `openspec/changes/{change-id}/specs/` 下的 spec diff。

#### Scenario: 新增 capability 的 main spec
- **WHEN** 一個 change 被歸檔，且其 spec diff 套用至 `openspec/specs/{capability}/spec.md`
- **THEN** 套用後的 main spec 第一行 SHALL 為 `# {capability}`，並在 `## Requirements` 之前包含 `## Purpose` 段落

#### Scenario: Main spec 不應含 delta 標頭
- **WHEN** 開發者 / 工具掃描 `openspec/specs/**/spec.md`
- **THEN** 不應出現 `## ADDED Requirements` / `## MODIFIED Requirements` / `## REMOVED Requirements` 等 delta 標頭

#### Scenario: Change 內的 spec diff 仍使用 delta 標頭
- **WHEN** 撰寫 `openspec/changes/{change-id}/specs/{capability}/spec.md`
- **THEN** 該檔案 SHALL 使用 `## ADDED Requirements` / `## MODIFIED Requirements` / `## REMOVED Requirements` 描述差異，而非 main spec 的 Purpose + Requirements 結構

### Requirement: Capability 命名

Capability 目錄名 SHALL 採 kebab-case，與 `# {capability}` 標題完全一致；spec 檔名 SHALL 固定為 `spec.md`。

#### Scenario: 合法 capability 命名
- **WHEN** 新增 capability `admin-shop-image`
- **THEN** 路徑 SHALL 為 `openspec/specs/admin-shop-image/spec.md`，且檔內第一行 SHALL 為 `# admin-shop-image`

#### Scenario: 非 kebab-case 命名
- **WHEN** 開發者嘗試以 `AdminShopImage` 或 `admin_shop_image` 作為 capability 目錄名
- **THEN** 該命名 SHALL 被視為違反約定，需改為 kebab-case
