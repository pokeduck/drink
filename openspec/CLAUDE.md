# openspec/

OpenSpec 工作目錄。本專案以 **OpenSpec 為唯一的 spec 開發流程與 source of truth**。

## 四階段 lifecycle

| 階段 | Skill | 用途 |
|------|-------|------|
| Explore | `/opsx:explore` | 釐清需求 / 發想，產出物不一定入 spec |
| Propose | `/opsx:propose` | 描述變更意圖，產出 proposal / design / tasks / 預期 spec diff |
| Apply | `/opsx:apply` | 依 tasks 實作，逐項勾選完成 |
| Archive | `/opsx:archive` | 將 spec diff sync 到 `specs/`，change 移到 `changes/archive/` |

任何 spec 變更**必須**走 change 流程，不可繞過 lifecycle 直接編輯 `specs/`。

## 目錄結構

```
openspec/
├── config.yaml
├── specs/                # 當前系統契約（已實作 capabilities）
│   └── {capability}/spec.md
└── changes/
    ├── {change-id}/      # 進行中的變更
    │   ├── proposal.md
    │   ├── design.md
    │   ├── tasks.md
    │   └── specs/
    └── archive/          # 已歸檔變更
```

## 當前 capabilities（`specs/`）

- admin-feedback
- admin-forgot-password-verification-ui
- admin-permission
- admin-shop / admin-shop-hub / admin-shop-image / admin-shop-menu / admin-shop-options / admin-shop-override
- api-codegen
- client-design-system / client-layout / client-pages / client-shared-components
- image-upload
- onion-layer-structure
- openspec-format
- typed-api-client
- user-auth
- user-profile

## 進行中 changes（`changes/`，不含 archive）

- add-admin-order
- add-relax-guest-access

已歸檔變更請見 `changes/archive/`（依日期排序，例如 `2026-04-29-add-user-auth`）。

## 與其他層的關係

- 規範細則（spec 格式、delta 標頭、capability 命名）→ `../.claude/rules/spec.md`
- Legacy spec（Addy Osmani 風格）→ `../specs-legacy/`，**不再依此開發**，衝突時以本目錄為準

## 不要做的事

- 不要直接編輯 `specs/{capability}/spec.md`，必須透過 change → apply → archive
- 不要在 main spec 內使用 delta 標頭（`## ADDED` / `## MODIFIED` / `## REMOVED`），那些只屬於 change diff
- 不要新增 Osmani 風格 spec（六大區域 + Boundaries），現行流程一律走 OpenSpec
- Capability 目錄名與 `# {capability}` 標題必須完全一致（kebab-case）
