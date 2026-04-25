<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>

<style>
body {
  margin: 0;
  font-family: sans-serif;
}

/* 表單錯誤紅字：不預留空間，出現時自然往下推 */
.el-form-item .el-form-item__error {
  position: relative;
  top: auto;
  left: auto;
  padding-top: 0;
  margin-top: 4px;
}

/* Dialog 響應式：確保左右最小間距 */
.el-dialog {
  max-width: calc(100vw - 32px);
  margin-left: auto !important;
  margin-right: auto !important;
}

/* Dialog 內的錯誤 Alert：陰影區分層級 */
.api-error-in-dialog {
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3) !important;
  border: 1px solid var(--el-border-color-lighter);
}

@media (max-width: 767px) {
  .el-message-box {
    width: calc(100vw - 64px) !important;
  }
}

/* Scroll to top 按鈕：常駐半透明，hover 時完全顯示 */
.el-backtop {
  opacity: 0.7;
  transition: opacity 0.2s;
}

.el-backtop:hover {
  opacity: 1;
}

/* 全站通用：分頁區塊 */
.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid var(--el-border-color-lighter);
}
</style>
