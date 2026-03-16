<script setup lang="ts">
import type { MenuModel } from './SideMenu.vue'

defineProps<{
  item: MenuModel
}>()
</script>

<template>
  <!-- 有子選單時渲染 sub-menu 並遞迴 -->
  <el-sub-menu v-if="item.children && item.children.length > 0" :index="item.index">
    <template #title>
      <el-icon v-if="item.icon">
        <component :is="item.icon" />
      </el-icon>
      <span>{{ item.title }}</span>
    </template>
    
    <SideMenuItem
      v-for="child in item.children"
      :key="child.index"
      :item="child"
    />
  </el-sub-menu>

  <!-- 無子選單時渲染一般的 menu-item -->
  <el-menu-item v-else :index="item.index">
    <el-icon v-if="item.icon">
      <component :is="item.icon" />
    </el-icon>
    <template #title>{{ item.title }}</template>
  </el-menu-item>
</template>
