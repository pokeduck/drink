# Tasks: migrate-specs-to-purpose-requirements

## 1. 遷移 admin 系列 spec

- [x] 1.1 `openspec/specs/admin-feedback/spec.md`：加 `# admin-feedback` 標題、補 `## Purpose`、`## ADDED Requirements` → `## Requirements`
- [x] 1.2 `openspec/specs/admin-forgot-password-verification-ui/spec.md`：同上
- [x] 1.3 `openspec/specs/admin-permission/spec.md`：同上
- [x] 1.4 `openspec/specs/admin-shop/spec.md`：同上
- [x] 1.5 `openspec/specs/admin-shop-menu/spec.md`：同上
- [x] 1.6 `openspec/specs/admin-shop-override/spec.md`：同上

## 2. 遷移 client 系列 spec

- [x] 2.1 `openspec/specs/client-design-system/spec.md`：加 `# client-design-system` 標題、補 `## Purpose`、`## ADDED Requirements` → `## Requirements`
- [x] 2.2 `openspec/specs/client-layout/spec.md`：同上
- [x] 2.3 `openspec/specs/client-pages/spec.md`：同上
- [x] 2.4 `openspec/specs/client-shared-components/spec.md`：同上

## 3. 遷移基礎建設 / 跨切面 spec

- [x] 3.1 `openspec/specs/api-codegen/spec.md`：加 `# api-codegen` 標題、補 `## Purpose`、`## ADDED Requirements` → `## Requirements`
- [x] 3.2 `openspec/specs/onion-layer-structure/spec.md`：同上
- [x] 3.3 `openspec/specs/typed-api-client/spec.md`：同上

## 4. 文件約定（rules）

- [x] 4.0 在 `.claude/rules/spec.md` 補上「main spec 結構（# 標題 + ## Purpose + ## Requirements）與 change diff（## ADDED/MODIFIED/REMOVED Requirements）區分」的說明，與本 change 的 `openspec-format` capability 對齊

## 5. 驗收

- [x] 5.1 全部 13 份 spec 開頭兩個非空 line 應為 `# {capability}` 與 `## Purpose`
- [x] 5.2 `grep -rn "^## ADDED Requirements" openspec/specs/` 結果為空
- [x] 5.3 `grep -rn "^## Requirements$" openspec/specs/ | wc -l` 等於 15（13 + 2 既有新格式）
- [x] 5.4 每份 spec 的 `### Requirement:` 與 `#### Scenario:` 數量遷移前後一致（用 `grep -c` 比對）
- [x] 5.5 `openspec validate migrate-specs-to-purpose-requirements --strict` 通過
- [x] 5.6 `openspec list --specs` 列出 15 份 spec 無錯誤；`openspec-format` 在 archive 後新增為第 16 份
