<template>
  <div class="page-container">
    <AppBreadcrumb />
    <el-card shadow="never">
      <el-result
        icon="success"
        title="歷史成交紀錄"
        sub-title="查看過去所有的已完成交易紀錄與報表"
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
