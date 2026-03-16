<template>
  <div class="page-container">
    <AppBreadcrumb />
    <el-card shadow="never">
      <el-result
        icon="info"
        title="系統設定"
        sub-title="管理後台的核心系統參數與權限設定"
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
