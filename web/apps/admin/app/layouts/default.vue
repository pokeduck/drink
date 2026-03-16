<script setup lang="ts">
/**
 * 後台管理系統主佈局 (Nuxt 4 + Element Plus)
 * 整合：全站初始化 Loading + 局部路由切換 Loading
 */
import { useMenuStore } from "~/stores/menu";
import { storeToRefs } from "pinia";
import defaultAvatar from "~/assets/avatar.png";

// 1. 狀態宣告：一開始就設為 true，確保全站 Loading 立即啟動
const isInitialLoading = ref(true); // 全站初次載入 (Fullscreen)
const isPageLoading = ref(false); // 局部路由換頁載入 (Main Region)

const menuStore = useMenuStore();
const { isCollapsed } = storeToRefs(menuStore);
const nuxtApp = useNuxtApp();

// 2. 全站初始化邏輯 (onMounted)
onMounted(() => {
  // 設定最低載入時間為 1000ms (1秒)，確保使用者能看清楚初始化文字，並提供平滑過渡
  setTimeout(() => {
    isInitialLoading.value = false;
  }, 1000);
});

// 3. 路由切換監聽 (useNuxtApp hooks)
let pageLoadStartTime = 0;

// 監聽開始換頁
nuxtApp.hook("page:start", () => {
  // 為了避免跟全站 Loading 衝突，只有在非初次載入時，才觸發局部 Loading
  if (!isInitialLoading.value) {
    isPageLoading.value = true;
    pageLoadStartTime = Date.now();
  }
});

// 監聽換頁完成 (包含新頁面的 async data 獲取完畢)
nuxtApp.hook("page:finish", () => {
  if (!isPageLoading.value) return;

  // 確保局部 Loading 至少顯示 1000ms (1秒)
  const elapsed = Date.now() - pageLoadStartTime;
  const remaining = Math.max(0, 1000 - elapsed);

  setTimeout(() => {
    isPageLoading.value = false;
  }, remaining);
});

// 錯誤兜底：萬一換頁出錯 (例如 404)，把 Loading 關掉，避免畫面卡死
nuxtApp.hook("app:error", () => {
  isInitialLoading.value = false;
  isPageLoading.value = false;
});
</script>

<template>
  <!-- 
    全站 Loading 綁定在最外層 
    v-loading.fullscreen.lock 會在 isInitialLoading 為 true 時鎖定全螢幕
  -->
  <div v-loading.fullscreen.lock="isInitialLoading" element-loading-text="Loading..." element-loading-background="rgba(255, 255, 255, 1)" class="layout-wrapper">
    <!-- 加上 v-show 防止在 Loading 解除前看到未完全渲染的組件 -->
    <el-container v-show="!isInitialLoading" class="admin-layout">
      <!-- 頂部導覽列 (固定高度) -->
      <el-header class="admin-header" height="60px">
        <div class="header-left">
          <el-link :underline="false" href="/" class="logo-link">
            <div class="logo-wrapper">
              <el-icon :size="24" class="logo-icon">
                <ColdDrink />
              </el-icon>
              <span class="logo-text">DRINK ADMIN</span>
            </div>
          </el-link>
          <el-button type="text" :icon="isCollapsed ? 'Expand' : 'Fold'" @click="menuStore.toggleCollapse" class="collapse-btn" />
        </div>
        <div class="header-right">
          <el-avatar :size="32" :src="defaultAvatar" />
        </div>
      </el-header>

      <!-- 下半部主體 -->
      <el-container class="admin-body">
        <!-- 左側側邊欄 -->
        <el-aside :width="isCollapsed ? '0' : '250px'" class="admin-aside" :class="{ 'is-collapsed': isCollapsed }">
          <SideMenu :collapsed="isCollapsed" />
        </el-aside>

        <!-- 右側主要內容區：綁定局部 Loading -->
        <el-main class="admin-main" v-loading="isPageLoading" element-loading-text="載入中...">
          <!-- 頁面渲染出口 -->
          <slot />
        </el-main>
      </el-container>
    </el-container>
  </div>
</template>

<style scoped>
/* 佈局高度鎖定為視窗高度 */
.layout-wrapper {
  height: 100vh;
  width: 100vw;
  background-color: #fff; /* 背景色與 Loading 遮罩一致 */
}

.admin-layout {
  height: 100%;
  width: 100%;
}

.admin-header {
  background-color: #2c3e50;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #fff;
  padding: 0 20px;
  z-index: 1000;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.collapse-btn {
  color: #fff;
  font-size: 20px;
}

.logo-link {
  color: #fff !important;
}

.logo-wrapper {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 200px;
}

.logo-text {
  font-size: 18px;
  font-weight: bold;
  letter-spacing: 1px;
}

.admin-body {
  height: calc(100vh - 60px); /* 扣除 Header 高度 */
  overflow: hidden;
}

.admin-aside {
  background-color: #fff;
  border-right: 1px solid var(--el-border-color-light);
  transition: width 0.3s ease;
  overflow: hidden;
}

.admin-aside.is-collapsed {
  width: 0;
  border-right: none;
}

.admin-main {
  background-color: #f5f7fa;
  padding: 24px;
  overflow-y: auto; /* 內容區獨立捲動 */
  position: relative; /* 確保 Loading 能正確覆蓋 */
}

/* 捲軸美化 */
.admin-main::-webkit-scrollbar {
  width: 6px;
}
.admin-main::-webkit-scrollbar-thumb {
  background: #c0c4cc;
  border-radius: 3px;
}
.admin-main::-webkit-scrollbar-track {
  background: transparent;
}
</style>
