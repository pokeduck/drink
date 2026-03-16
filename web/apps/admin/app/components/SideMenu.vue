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
        :default-active="route.path"
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
