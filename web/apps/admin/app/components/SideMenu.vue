<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useMenuStore } from '~/stores/menu'
import { storeToRefs } from 'pinia'

defineProps<{
  collapsed?: boolean
}>()

const route = useRoute()
const menuStore = useMenuStore()
const { menuData, loading } = storeToRefs(menuStore)

// 收集所有 leaf menu 的 index（即有 endpoint 的項目）
function collectLeafIndexes(items: MenuModel[]): string[] {
  const result: string[] = []
  for (const item of items) {
    if (item.children?.length) {
      result.push(...collectLeafIndexes(item.children))
    } else {
      result.push(item.index)
    }
  }
  return result
}

// 匹配當前路徑到最接近的 menu index
// e.g. /admin-account/create → /admin-account/list
// e.g. /admin-account/role/create → /admin-account/role
const activeIndex = computed(() => {
  const path = route.path
  const indexes = collectLeafIndexes(menuData.value)
  // 優先完全匹配
  if (indexes.includes(path)) return path
  // 找最長前綴匹配的 menu index
  let best = ''
  for (const idx of indexes) {
    const prefix = idx.replace(/\/list$/, '')
    if (path.startsWith(prefix) && prefix.length > best.length) {
      best = idx
    }
  }
  return best || path
})

// 在組件初始化時獲取選單資料
onMounted(() => {
  if (menuData.value.length === 0) {
    menuStore.fetchMenuData()
  }
})
</script>

<template>
  <aside
    class="side-menu-container"
    v-loading="loading"
  >
    <el-scrollbar>
      <el-menu
        v-if="menuData.length > 0"
        :default-active="activeIndex"
        class="recursive-menu"
        router
        unique-opened
      >
        <SideMenuItem
          v-for="item in menuData"
          :key="item.index"
          :item="item"
        />
      </el-menu>
    </el-scrollbar>
  </aside>
</template>

<style scoped>
.side-menu-container {
  width: 250px;
  height: calc(100vh - 60px);
  background-color: #fff;
  border-right: 1px solid var(--el-border-color-light);
}

.recursive-menu {
  border-right: none;
}

:deep(.el-scrollbar__view) {
  height: 100%;
}
</style>
