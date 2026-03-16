<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
import { useMenuStore, type MenuModel } from "~/stores/menu";
import { ArrowRight } from "@element-plus/icons-vue";

const route = useRoute();
const menuStore = useMenuStore();

/**
 * 遞迴從選單資料中建立路徑與標題的對應表
 */
const breadcrumbNameMap = computed(() => {
  const map: Record<string, string> = { "/": "控制台" };

  const traverse = (items: MenuModel[]) => {
    items.forEach((item) => {
      map[item.index] = item.title;
      if (item.children) {
        traverse(item.children);
      }
    });
  };

  if (menuStore.menuData) {
    traverse(menuStore.menuData);
  }
  return map;
});

const breadcrumbs = computed(() => {
  const pathNodes = route.path.split("/").filter(Boolean);
  let currentPath = "";

  const items = pathNodes.map((node) => {
    currentPath += `/${node}`;
    return {
      path: "", // 🌟 關鍵 1：這裡一律設為空值，不給跳轉
      title: breadcrumbNameMap.value[currentPath] || node,
      uniqueKey: currentPath, // 🌟 關鍵 2：保留原始路徑作為 Vue 的 key
    };
  });

  if (route.path !== "/") {
    return [{ path: "/", title: "首頁", uniqueKey: "/" }, ...items];
  }
  return [{ path: "/", title: "首頁", uniqueKey: "/" }];
});
</script>

<template>
  <el-breadcrumb :separator-icon="ArrowRight" class="app-breadcrumb">
    <el-breadcrumb-item v-for="item in breadcrumbs" :key="item.uniqueKey" :to="item.path ? item.path : undefined">
      {{ item.title }}
    </el-breadcrumb-item>
  </el-breadcrumb>
</template>

<style scoped>
.app-breadcrumb {
  margin-bottom: 16px;
}
</style>
