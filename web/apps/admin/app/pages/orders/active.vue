<template>
  <div class="page-container">
    <AppBreadcrumb />
    <el-card shadow="never">
      <el-result
        icon="info"
        title="處理中訂單"
        sub-title="您可以在此處查看當前正在處理的所有活躍訂單"
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
