<script setup lang="ts">
const router = useRouter()

onMounted(() => {
  ElMessage.info('覆寫設定已整合至店家編輯')
  router.replace('/shop/list')
})
</script>

<template>
  <div />
</template>
