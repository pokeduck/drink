<template>
  <div class="page-container">
    <AppBreadcrumb />
    <el-card shadow="never">
      <el-result
        icon="success"
        title="飲品分類"
        sub-title="這是分類配置下的飲品分類詳細設定頁面"
      >
        <template #extra>
          <el-text class="mx-1" type="info">Default text</el-text>
        </template>
      </el-result>
    </el-card>
  </div>
</template>

<style scoped>
.page-container {
  padding: 20px;
}
</style>
