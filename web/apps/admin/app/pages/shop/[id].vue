<script setup lang="ts">
import { ArrowLeft } from '@element-plus/icons-vue'
import { useAdminApi } from '~/composable/useAdminApi'
import { usePermission } from '~/composable/usePermission'
import { MENU } from '@app/core'

const route = useRoute()
const router = useRouter()
const api = useAdminApi()
const { can } = usePermission()

const shopId = computed(() => Number(route.params.id))

const shopName = ref('')
const createdAt = ref('')
const updatedAt = ref('')

const tabs = computed(() => {
  const result: { name: string; label: string; path: string; show: boolean }[] = [
    { name: 'edit', label: '基本資訊', path: 'edit', show: can(MENU.ShopList, 'read') },
    { name: 'images', label: '圖片管理', path: 'images', show: can(MENU.ShopList, 'read') },
    { name: 'options', label: '選項啟用', path: 'options', show: can(MENU.ShopOptions, 'read') },
    { name: 'overrides', label: '覆寫設定', path: 'overrides', show: can(MENU.ShopOverride, 'read') },
  ]
  return result.filter(t => t.show)
})

const activeTab = computed(() => {
  const segments = route.path.split('/').filter(Boolean)
  return segments[segments.length - 1] ?? 'edit'
})

function handleTabChange(tabName: string) {
  router.push(`/shop/${shopId.value}/${tabName}`)
}

async function fetchShop() {
  const { data: res } = await api.GET('/api/admin/shops/{id}', {
    params: { path: { id: shopId.value } },
  })
  if (res?.data) {
    shopName.value = res.data.name ?? ''
    createdAt.value = res.data.created_at ?? ''
    updatedAt.value = res.data.updated_at ?? ''
  }
}

watch(() => shopId.value, fetchShop, { immediate: true })

// 進入 /shop/{id} 沒帶 sub-path → redirect 到第一個有權限的 tab
onMounted(() => {
  const segments = route.path.split('/').filter(Boolean)
  const last = segments[segments.length - 1]
  const isHubRoot = last === String(shopId.value)
  if (isHubRoot && tabs.value.length > 0) {
    router.replace(`/shop/${shopId.value}/${tabs.value[0]!.path}`)
  }
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <div style="display: flex; align-items: center; gap: 8px">
            <el-button text @click="router.push('/shop/list')">
              <el-icon><ArrowLeft /></el-icon>返回
            </el-button>
            <span>店家：{{ shopName }}</span>
          </div>
          <AppTimestamp v-if="createdAt" :created-at="createdAt" :updated-at="updatedAt" />
        </div>
      </template>

      <el-tabs :model-value="activeTab" @tab-change="handleTabChange as any">
        <el-tab-pane
          v-for="tab in tabs"
          :key="tab.name"
          :label="tab.label"
          :name="tab.name"
        />
      </el-tabs>

      <NuxtPage />
    </el-card>
  </div>
</template>

<style scoped>
:deep(.el-tabs__header) {
  margin-bottom: 16px;
}
</style>
