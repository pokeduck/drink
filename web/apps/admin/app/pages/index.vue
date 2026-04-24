<script setup lang="ts">
import { useApiFeedback } from '~/composable/useApiFeedback'

const route = useRoute()
const router = useRouter()
const { handleError } = useApiFeedback()

onMounted(() => {
  if (route.query.forbidden === '1') {
    handleError({ message: '您沒有權限存取該頁面' }, '您沒有權限存取該頁面')
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
