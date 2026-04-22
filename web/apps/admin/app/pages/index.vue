<script setup lang="ts">
const route = useRoute()
const router = useRouter()

onMounted(() => {
  if (route.query.forbidden === '1') {
    ElMessage.error('您沒有權限存取該頁面')
    router.replace({ query: {} })
  }
})
</script>

<template>
  <div>
    <AppBreadcrumb />
    <h2>後台管理系統</h2>
    <p style="color: #909399">歡迎使用 Drink Admin 後台管理系統</p>
  </div>
</template>
