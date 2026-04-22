<script setup lang="ts">
/**
 * 後台管理系統主佈局 (Nuxt 4 + Element Plus)
 * 整合：全站初始化 Loading + 局部路由切換 Loading
 */
import { useMenuStore } from "~/stores/menu";
import { useAuthStore } from "~/stores/auth";
import { storeToRefs } from "pinia";
import { EditPen, SwitchButton } from "@element-plus/icons-vue";
import defaultAvatar from "~/assets/avatar.png";

const authStore = useAuthStore();
const menuStore = useMenuStore();
const { isCollapsed, loading: isMenuLoading } = storeToRefs(menuStore);

// 在 layout 層級立即發起 menu fetch，避免等到 SideMenu onMounted 才抓
// 僅在 client 端執行，避免 SSR 時 Node.js 發 HTTPS 請求遇到 self-signed cert 錯誤
if (import.meta.client && menuStore.menuData.length === 0) {
  menuStore.fetchMenuData();
}

// Mobile 判斷
const isMobile = ref(false);
const mobileDrawerVisible = ref(false);

const checkMobile = () => {
  isMobile.value = window.innerWidth < 768;
  if (!isMobile.value) {
    mobileDrawerVisible.value = false;
  }
};

onMounted(() => {
  checkMobile();
  window.addEventListener("resize", checkMobile);
});

onUnmounted(() => {
  window.removeEventListener("resize", checkMobile);
});

const toggleMenu = () => {
  if (isMobile.value) {
    mobileDrawerVisible.value = !mobileDrawerVisible.value;
  } else {
    menuStore.toggleCollapse();
  }
};

// Mobile 點選 menu item 後自動關閉 drawer
const route = useRoute();
watch(() => route.path, () => {
  if (isMobile.value) {
    mobileDrawerVisible.value = false;
  }
});

// 1. 狀態宣告：一開始就設為 true，確保全站 Loading 立即啟動
const isInitialLoading = ref(true); // 全站初次載入 (Fullscreen)
const isPageLoading = ref(false); // 局部路由換頁載入 (Main Region)

const nuxtApp = useNuxtApp();

// 2. 全站初始化邏輯 (onMounted)
onMounted(() => {
  const startTime = Date.now();

  const hideInitialLoading = () => {
    const elapsed = Date.now() - startTime;
    const remaining = Math.max(0, 1000 - elapsed);
    setTimeout(() => {
      isInitialLoading.value = false;
    }, remaining);
  };

  // 等 API 請求（如 menu fetch）完成
  if (isMenuLoading.value) {
    const stop = watch(isMenuLoading, (pending) => {
      if (!pending) {
        stop();
        hideInitialLoading();
      }
    });
  } else {
    hideInitialLoading();
  }
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

// 監聽換頁完成 — 等 API 請求也結束後才關閉 loading
nuxtApp.hook("page:finish", () => {
  if (!isPageLoading.value) return;

  const hideLoading = () => {
    const elapsed = Date.now() - pageLoadStartTime;
    const remaining = Math.max(0, 1000 - elapsed);
    setTimeout(() => {
      isPageLoading.value = false;
    }, remaining);
  };

  // 如果還有 API 請求在跑，等它們完成
  if (isMenuLoading.value) {
    const stop = watch(isMenuLoading, (pending) => {
      if (!pending) {
        stop();
        hideLoading();
      }
    });
  } else {
    hideLoading();
  }
});

// 錯誤兜底：萬一換頁出錯 (例如 404)，把 Loading 關掉，避免畫面卡死
nuxtApp.hook("app:error", () => {
  isInitialLoading.value = false;
  isPageLoading.value = false;
});

// Header Dropdown 操作
const handleCommand = async (command: string) => {
  if (command === "logout") {
    await authStore.logout();
    menuStore.clearMenu();
    await navigateTo("/login");
  } else if (command === "change-password") {
    await navigateTo("/change-password");
  }
};
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
          <el-link v-if="isMobile" underline="never" icon="Menu" @click="toggleMenu" class="collapse-btn" />
          <el-link underline="never" href="/" class="logo-link">
            <div class="logo-wrapper">
              <el-icon :size="24" class="logo-icon">
                <ColdDrink />
              </el-icon>
              <span class="logo-text">DRINK ADMIN</span>
            </div>
          </el-link>
          <el-link v-if="!isMobile" underline="never" :icon="isCollapsed ? 'Expand' : 'Fold'" @click="toggleMenu" class="collapse-btn" />
        </div>
        <div class="header-right">
          <el-popover placement="bottom-end" :width="160" :offset="8" trigger="click" popper-class="header-user-popover">
            <template #reference>
              <el-avatar :size="32" :src="defaultAvatar" class="avatar-trigger" />
            </template>
            <div class="user-popover-menu">
              <div class="user-popover-item" @click="handleCommand('change-password')">
                <el-icon><EditPen /></el-icon>
                <span>修改密碼</span>
              </div>
              <div class="user-popover-divider" />
              <div class="user-popover-item" @click="handleCommand('logout')">
                <el-icon><SwitchButton /></el-icon>
                <span>登出</span>
              </div>
            </div>
          </el-popover>
        </div>
      </el-header>

      <!-- 下半部主體 -->
      <el-container class="admin-body">
        <!-- Desktop 側邊欄 -->
        <el-aside v-if="!isMobile" :width="isCollapsed ? '0' : '250px'" class="admin-aside" :class="{ 'is-collapsed': isCollapsed }">
          <SideMenu :collapsed="isCollapsed" />
        </el-aside>

        <!-- Mobile Drawer -->
        <el-drawer
          v-if="isMobile"
          v-model="mobileDrawerVisible"
          direction="ltr"
          :with-header="false"
          size="250px"
          :z-index="999"
          class="mobile-menu-drawer"
        >
          <SideMenu />
        </el-drawer>

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
  padding: 0 32px 0 20px;
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

.avatar-trigger {
  cursor: pointer;
}
</style>

<style>
.mobile-menu-drawer .el-drawer__body {
  padding: 0;
  overflow: hidden;
}

.header-user-popover {
  padding: 0 !important;
}

.user-popover-menu {
  padding: 4px 0;
}

.user-popover-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  cursor: pointer;
  font-size: 14px;
  color: var(--el-text-color-regular);
  transition: background-color 0.2s;
}

.user-popover-item:hover {
  background-color: var(--el-fill-color-light);
}

.user-popover-divider {
  height: 1px;
  background-color: var(--el-border-color-lighter);
  margin: 4px 0;
}
</style>
