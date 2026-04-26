# Proposal: 統一 OpenSpec 格式為 Purpose + Requirements

## Why

`openspec/specs/` 下 15 份 spec 中有 13 份仍使用早期的 `## ADDED Requirements` 格式（change diff 用的標頭直接被歸檔成 main spec），只有 `admin-shop-image` 與 `image-upload` 採用新約定的 `# {capability}` + `## Purpose` + `## Requirements` 結構。

格式不一致造成幾個問題：

- 新讀者無法從 spec 一眼看出該 capability 的目的（缺 Purpose 段落）
- 工具 / sync agent 反覆回報格式不符警告，雜訊蓋過真正的 spec drift
- 未來若以 spec 結構做自動化（驗證、產生文件、diff），需要先處理這層歧異
- `## ADDED Requirements` 是 change diff 專用標頭，留在 main spec 裡語意錯誤（已不是 diff，而是當前契約）

此 change 純粹是 spec 格式遷移，不變更任何系統行為與 Requirement 內容。

## What Changes

把以下 13 份 spec 從舊格式遷移到新格式：

- `admin-feedback`
- `admin-forgot-password-verification-ui`
- `admin-permission`
- `admin-shop`
- `admin-shop-menu`
- `admin-shop-override`
- `api-codegen`
- `client-design-system`
- `client-layout`
- `client-pages`
- `client-shared-components`
- `onion-layer-structure`
- `typed-api-client`

每份 spec 的調整：

1. 檔頭加上 `# {capability}` 一級標題（用 capability 目錄名）
2. 在 `## Requirements` 前插入 `## Purpose` 段落，從既有 Requirements 萃取一段 1–3 句的目的描述（資訊已在 spec 內，不另外發明內容）
3. 把 `## ADDED Requirements` 改為 `## Requirements`
4. 既有 `### Requirement: ...` 與 `#### Scenario: ...` 區塊一字不動

`admin-shop-image` 與 `image-upload` 已是新格式，跳過。

## Capabilities

### New Capabilities

- `openspec-format`：明文記錄 main spec 必須採 `# {capability}` + `## Purpose` + `## Requirements` 結構、capability 命名為 kebab-case，並區分 main spec 與 change diff 標頭的使用情境。讓「格式約定」本身成為可被驗證、可被未來變更引用的契約。

### Modified Capabilities

13 份 spec 結構調整，但 Requirement 與 Scenario 內容皆不變更，視為純文件結構遷移（不在本 change 的 spec diff 中描述，因內容未變更）。

## Impact

**Affected files:**

- `openspec/specs/admin-feedback/spec.md`
- `openspec/specs/admin-forgot-password-verification-ui/spec.md`
- `openspec/specs/admin-permission/spec.md`
- `openspec/specs/admin-shop/spec.md`
- `openspec/specs/admin-shop-menu/spec.md`
- `openspec/specs/admin-shop-override/spec.md`
- `openspec/specs/api-codegen/spec.md`
- `openspec/specs/client-design-system/spec.md`
- `openspec/specs/client-layout/spec.md`
- `openspec/specs/client-pages/spec.md`
- `openspec/specs/client-shared-components/spec.md`
- `openspec/specs/onion-layer-structure/spec.md`
- `openspec/specs/typed-api-client/spec.md`

**Affected code:** 無（僅 spec 文件）。

**Affected dependencies:** 無。

**Affected data:** 無。

**Permissions:** 無。

**Frontend:** 無。

**Out of scope:**

- 不重寫 / 重組 Requirement 與 Scenario 內容
- 不合併或拆分既有 capability
- 不調整 `specs-legacy/` 下的 Osmani 風格舊文件
- 不調整 OpenSpec 工具設定 / template
