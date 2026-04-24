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

/* 表單錯誤紅字與輸入框間距 */
.el-form-item__error {
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
