# client-design-system

## Purpose

定義 Client 前台 Brutalist 風格設計系統契約：Tailwind v4 `@theme` tokens、`.brutalist-card` / `.brutalist-button` 元件樣式、字體（Inter + Space Grotesk）、Dark mode 切換策略與自訂 scrollbar，作為前台所有頁面與元件的視覺基礎。

## Requirements

### Requirement: Brutalist CSS 設計系統

系統 SHALL 在 `assets/css/main.css` 定義完整的 Brutalist 風格設計系統，包含：
- Tailwind v4 `@theme` tokens：`--font-sans`（Inter）、`--font-display`（Space Grotesk）、`--color-brand`（#FF5C00）、`--color-page-bg`（#F7F7F2）、`--color-sidebar-bg`（#EBEBE3）、`--color-dark`（#1A1A1A）
- `.brutalist-card` class：白底、2px 黑邊框、4px 4px 硬陰影、hover 上浮效果
- `.brutalist-button` class：2px 黑邊框、font-black、uppercase、tracking-widest
- `.brutalist-button-primary` class：brand orange 底色、黑陰影、hover 上浮
- 標題樣式：h1-h6 使用 font-display font-black uppercase tracking-tighter italic
- 自訂 scrollbar：6px 寬、brand color hover thumb

#### Scenario: Light mode 預設外觀
- **WHEN** 頁面載入且無 dark class
- **THEN** page-bg 為 #F7F7F2，card 為白底黑邊框，button 為黑邊框

#### Scenario: Dark mode 外觀
- **WHEN** html 元素有 `dark` class
- **THEN** 背景切換為 #121212 / #0D0D0D，card 邊框改為白色半透明，文字為白色

#### Scenario: Brutalist card hover 效果
- **WHEN** 使用者 hover brutalist-card
- **THEN** 陰影從 4px 4px 變為 6px 6px，元素向左上位移（negative translate）

### Requirement: Google Fonts 載入

系統 SHALL 透過 `@nuxtjs/google-fonts` 模組載入 Inter（400, 500, 600, 700）和 Space Grotesk（700, 800, 900）字體。

#### Scenario: 字體載入完成
- **WHEN** 頁面完成載入
- **THEN** body 文字使用 Inter，標題使用 Space Grotesk，無 FOUT

### Requirement: Dark mode 切換

系統 SHALL 透過 `@nuxtjs/color-mode` 支援 light/dark 模式切換，class-based 策略（在 `<html>` 加上 `dark` class）。

#### Scenario: 手動切換 dark mode
- **WHEN** 使用者點擊 dark mode toggle 按鈕
- **THEN** 模式切換，`<html>` class 更新，所有 dark: 樣式生效，偏好存入 localStorage

#### Scenario: 首次訪問依系統偏好
- **WHEN** 使用者首次訪問且系統為 dark mode
- **THEN** 自動套用 dark mode
