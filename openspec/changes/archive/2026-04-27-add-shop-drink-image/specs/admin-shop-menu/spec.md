## MODIFIED Requirements

### Requirement: 刪除品項
系統 SHALL 提供 `DELETE /api/admin/shops/{shopId}/categories/{categoryId}/items/{itemId}` 端點，執行 soft delete。同一 transaction 內，該 (ShopId, DrinkItemId) 的所有 ShopImage SHALL 孤兒化（DrinkItemId=null、Sort=0、IsCover=false）。

#### Scenario: 成功刪除品項
- **WHEN** 管理員刪除一個存在的品項
- **THEN** 系統將品項標記為 IsDeleted=true, DeletedAt=now

#### Scenario: 刪除品項時圖片孤兒化
- **WHEN** 管理員刪除一個存在的品項，且該 (ShopId, DrinkItemId) 已綁有 ShopImage
- **THEN** 系統在同一 transaction 將所有受影響 ShopImage DrinkItemId 設為 null、Sort 設為 0、IsCover 設為 false

### Requirement: 刪除分類
系統 SHALL 提供 `DELETE /api/admin/shops/{shopId}/categories/{categoryId}` 端點。刪除分類時底下所有品項 SHALL 一併 soft delete。每個受影響品項 (ShopId, DrinkItemId) 的 ShopImage SHALL 隨之孤兒化（行為與「刪除品項」一致）。

#### Scenario: 成功刪除分類
- **WHEN** 管理員刪除一個存在的分類
- **THEN** 系統刪除該分類，底下品項標記為 soft deleted

#### Scenario: 刪除分類時連帶圖片孤兒化
- **WHEN** 管理員刪除一個含多個品項的分類，部分品項有圖
- **THEN** 系統在同一 transaction 將所有受影響 (ShopId, DrinkItemId) 的 ShopImage 孤兒化
