<script setup lang="ts">
import { Home, PlusCircle, List, User, Moon, Sun, LogOut, Settings, LogIn, UserPlus, Star } from 'lucide-vue-next'
import { onClickOutside } from '@vueuse/core'
import { useAuthStore } from '~/stores/auth'

const route = useRoute()
const colorMode = useColorMode()
const authStore = useAuthStore()

const isUserMenuOpen = ref(false)
const menuRef = ref<HTMLDivElement>()

onClickOutside(menuRef, () => {
  isUserMenuOpen.value = false
})

onMounted(() => {
  if (authStore.isLoggedIn && !authStore.currentUser) {
    authStore.fetchProfile()
  }
})

const navItems = [
  { label: '首頁', path: '/', icon: Home },
  { label: '發起揪團', path: '/create', icon: PlusCircle },
  { label: '我的訂單', path: '/my-orders', icon: List },
  { label: '個人檔案', path: '/profile', icon: User }
]

const dropdownItems = [
  { label: '偏好設定', path: '/settings', icon: Settings },
  { label: '我的收藏', path: '/favorites', icon: Star }
]

const avatarSeed = computed(() => authStore.currentUser?.email ?? 'guest')
const currentUserName = computed(() => authStore.currentUser?.name ?? 'Guest')

function toggleTheme() {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

function isActive(path: string) {
  return route.path === path
}

async function handleLogout() {
  await authStore.logout()
  isUserMenuOpen.value = false
  await navigateTo('/login')
}
</script>

<template>
  <div class="min-h-screen bg-page-bg dark:bg-dark-bg flex flex-col font-sans text-dark dark:text-dark-text overflow-x-hidden transition-colors duration-300">
    <!-- Header -->
    <header class="fixed top-0 left-0 right-0 h-16 bg-white dark:bg-dark-surface border-b-4 border-black dark:border-white z-50 flex items-center shrink-0 transition-colors">
      <div class="max-w-7xl mx-auto px-4 md:px-8 w-full flex items-center justify-between">
        <div class="flex items-center gap-8">
          <NuxtLink to="/" class="flex items-center gap-2">
            <h1 class="text-2xl md:text-3xl font-black tracking-tighter italic">DRINK.UP</h1>
          </NuxtLink>

          <!-- Desktop Nav -->
          <nav class="hidden md:flex items-center gap-6">
            <NuxtLink
              v-for="item in navItems"
              :key="item.path"
              :to="item.path"
              :class="[
                'text-sm font-black uppercase tracking-widest transition-colors',
                isActive(item.path)
                  ? 'text-brand underline underline-offset-4 decoration-2'
                  : 'text-dark/60 dark:text-dark-text/60 hover:text-dark dark:hover:text-dark-text'
              ]"
            >
              {{ item.label }}
            </NuxtLink>
          </nav>
        </div>

        <div class="flex items-center gap-4">
          <button
            class="p-2 border-2 border-black dark:border-white hover:bg-page-bg dark:hover:bg-white/10 transition-colors"
            title="Toggle Dark Mode"
            @click="toggleTheme"
          >
            <Moon v-if="colorMode.value === 'light'" class="w-5 h-5" />
            <Sun v-else class="w-5 h-5 text-brand" />
          </button>

          <NuxtLink to="/create" class="hidden md:flex brutalist-button brutalist-button-primary scale-75 origin-right text-black">
            + Create
          </NuxtLink>

          <div ref="menuRef" class="relative">
            <button
              class="w-8 md:w-10 h-8 md:h-10 border-2 border-black dark:border-white rounded-full overflow-hidden bg-brand/10 hover:shadow-brutalist-sm dark:hover:shadow-brutalist-dark-sm transition-shadow active:scale-95 block"
              @click="isUserMenuOpen = !isUserMenuOpen"
            >
              <img :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${avatarSeed}`" :alt="currentUserName" class="w-full h-full object-cover">
            </button>

            <Transition name="dropdown">
              <div
                v-if="isUserMenuOpen"
                class="absolute right-0 mt-4 w-56 bg-white dark:bg-dark-surface border-4 border-black dark:border-white shadow-brutalist-lg dark:shadow-brutalist-dark-lg p-2 z-[60]"
              >
                <template v-if="authStore.isLoggedIn">
                  <div class="p-4 border-b-2 border-black dark:border-white/10 mb-2">
                    <p class="text-[10px] font-black uppercase tracking-widest opacity-40">Signed in as</p>
                    <p class="text-sm font-black italic truncate">{{ currentUserName }}</p>
                  </div>

                  <div class="space-y-1">
                    <NuxtLink
                      v-for="item in dropdownItems"
                      :key="item.label"
                      :to="item.path"
                      class="flex items-center gap-3 w-full p-3 text-xs font-black uppercase tracking-widest hover:bg-brand hover:text-white dark:hover:text-dark-bg transition-colors group"
                      @click="isUserMenuOpen = false"
                    >
                      <component :is="item.icon" class="w-4 h-4 group-hover:scale-110 transition-transform" />
                      {{ item.label }}
                    </NuxtLink>

                    <button
                      class="flex items-center gap-3 w-full p-3 text-xs font-black uppercase tracking-widest text-red-500 hover:bg-red-500 hover:text-white transition-colors group border-t-2 border-black/5 dark:border-white/5 mt-2"
                      @click="handleLogout"
                    >
                      <LogOut class="w-4 h-4" />
                      Logout
                    </button>
                  </div>
                </template>

                <template v-else>
                  <div class="p-4 border-b-2 border-black dark:border-white/10 mb-2">
                    <p class="text-[10px] font-black uppercase tracking-widest opacity-40">Not signed in</p>
                    <p class="text-sm font-black italic truncate">Guest</p>
                  </div>

                  <div class="space-y-1">
                    <NuxtLink
                      to="/login"
                      class="flex items-center gap-3 w-full p-3 text-xs font-black uppercase tracking-widest hover:bg-brand hover:text-white dark:hover:text-dark-bg transition-colors group"
                      @click="isUserMenuOpen = false"
                    >
                      <LogIn class="w-4 h-4 group-hover:scale-110 transition-transform" />
                      登入
                    </NuxtLink>
                    <NuxtLink
                      to="/register"
                      class="flex items-center gap-3 w-full p-3 text-xs font-black uppercase tracking-widest hover:bg-brand hover:text-white dark:hover:text-dark-bg transition-colors group"
                      @click="isUserMenuOpen = false"
                    >
                      <UserPlus class="w-4 h-4 group-hover:scale-110 transition-transform" />
                      註冊
                    </NuxtLink>
                  </div>
                </template>
              </div>
            </Transition>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-1 mt-16 mb-20 md:mb-0 px-4 py-8 md:py-12 max-w-7xl mx-auto w-full">
      <slot />
    </main>

    <!-- Bottom Nav (Mobile Only) -->
    <nav class="md:hidden fixed bottom-0 left-0 right-0 bg-white dark:bg-dark-surface border-t-4 border-black dark:border-white z-50 transition-colors">
      <div class="flex justify-around items-center h-16">
        <NuxtLink
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          :class="[
            'flex flex-col items-center justify-center gap-1 transition-all duration-200',
            isActive(item.path) ? 'text-brand' : 'text-dark/40 dark:text-dark-text/40 hover:text-dark dark:hover:text-dark-text'
          ]"
        >
          <component
            :is="item.icon"
            :class="[
              'w-6 h-6',
              isActive(item.path) && 'scale-110 drop-shadow-[2px_2px_0px_var(--color-dark)] dark:drop-shadow-[2px_2px_0px_var(--color-dark-text)]'
            ]"
          />
          <span class="text-[9px] font-black uppercase tracking-widest leading-none text-center">{{ item.label }}</span>
        </NuxtLink>
      </div>
    </nav>

    <!-- Footer Status Bar (Desktop) -->
    <footer class="hidden md:flex h-10 bg-black dark:bg-black text-white px-8 items-center justify-between text-[10px] font-mono tracking-widest uppercase transition-colors">
      <div class="flex gap-4">
        <span>System: Healthy</span>
        <span>•</span>
        <span class="text-brand">Mode: {{ colorMode.value?.toUpperCase() }}</span>
      </div>
      <div class="flex gap-4">
        <span>Current User: {{ currentUserName }}</span>
        <span>•</span>
        <span>API v2.1.0</span>
      </div>
    </footer>

    <ToastContainer />
  </div>
</template>
