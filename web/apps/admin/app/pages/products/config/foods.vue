<template>
  <div class="page-container">
    <AppBreadcrumb />
    <el-card shadow="never">
      <el-result
        icon="warning"
        title="熟食分類"
        sub-title="這是分類配置下的熟食分類管理頁面"
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
