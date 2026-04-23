## Context

前台 client app 共 6 頁面 + 1 layout + 1 元件（GroupCard），已完成 React → Nuxt 翻譯。現有重複 pattern 需要抽取為共用元件。

## Goals / Non-Goals

**Goals:**
- 抽取 5 個高複用 pattern 為獨立元件
- 所有元件使用 Vue `<script setup>` + props + slots
- 零視覺變化（純重構）
- 元件 API 簡潔，避免過多 props

**Non-Goals:**
- 不重新設計 UI 或改變樣式
- 不抽取 BrutalistCard（已是 CSS class，無需 Vue 包裝）
- 不抽取只出現一次的 pattern（StoreLogo、OptionGrid、IconInput）

## Decisions

### 1. BackButton — 簡單 props 設計

```vue
<BackButton />                    <!-- 預設：ChevronLeft + router.back() -->
<BackButton icon="x" />           <!-- 關閉按鈕：X icon -->
<BackButton @click="custom" />    <!-- 自訂行為 -->
```

**Props:** `icon?: 'back' | 'close'`（預設 `'back'`）
**Events:** `@click`（有綁定時覆蓋預設 router.back）

### 2. SectionHeader — title + 可選右側 slot

```vue
<SectionHeader title="Active Groups">
  <template #right>Showing 2 open buys</template>
</SectionHeader>
```

**Props:** `title: string`, `size?: 'lg' | 'sm'`（lg = text-4xl border-black, sm = text-2xl opacity-40）

### 3. EmptyState — icon slot + 文字 props

```vue
<EmptyState title="NO ACTIVE BUYS" subtitle="Wait for someone to start!">
  <ShoppingBag class="w-16 h-16" />
</EmptyState>
```

**Props:** `title: string`, `subtitle?: string`
**Slot:** default slot 放 icon

### 4. StatusBadge — status enum → 顏色 + label

```vue
<StatusBadge :status="group.status" />
```

**Props:** `status: OrderStatus`
**內部：** 包含 statusColors 和 statusLabels mapping（從 GroupCard 抽出）

### 5. BrutalistModal — Teleport + backdrop + slot

```vue
<BrutalistModal v-model="isOpen" size="md">
  <template #header>Choose Shop</template>
  <template #default>...content...</template>
  <template #footer>...footer...</template>
</BrutalistModal>
```

**Props:** `modelValue: boolean`, `size?: 'full' | 'md' | 'sm'`（控制桌面版寬度）
**Slots:** `header`, `default`, `footer`
**Features:** 點擊 backdrop 關閉、Transition 動畫、mobile 全螢幕 / desktop 置中

## Risks / Trade-offs

**[過度抽象]** → 只抽確實重複的 pattern，不做預測性抽象。5 個元件都有 3+ 處使用。

**[Modal 差異]** → create 和 group detail 的 Modal 結構相似但內容差異大。用 slots 處理差異，骨架統一。
