<script setup lang="ts">
import { Settings, BadgeCheck, AlertCircle, Coffee, Store, ChevronRight, Edit3 } from 'lucide-vue-next'
import { useAuthStore } from '~/stores/auth'

const authStore = useAuthStore()

onMounted(async () => {
  if (!authStore.currentUser) await authStore.fetchProfile()
})

const avatarSrc = computed(() => {
  const u = authStore.currentUser
  if (!u) return 'https://api.dicebear.com/7.x/avataaars/svg?seed=guest'
  return u.avatar || `https://api.dicebear.com/7.x/avataaars/svg?seed=${u.email}`
})

const joinedLabel = computed(() => {
  const iso = authStore.currentUser?.created_at
  if (!iso) return ''
  const d = new Date(iso)
  return new Intl.DateTimeFormat('en-US', { year: 'numeric', month: 'short' }).format(d)
})

// Mockup stats — will be replaced with real data when order capability lands
const mockStats = [
  { label: '本月花費', value: 'NT$ 480', accent: 'text-brand' },
  { label: '累計揪團', value: '12', accent: '' },
  { label: 'Top Shop', value: '五十嵐', accent: '' },
  { label: 'Last Drink', value: '蜂蜜檸檬綠', sub: '半糖少冰', accent: '' }
]

const mockFavoriteShops = [
  { id: 1, name: '五十嵐', tag: '經典珍奶' },
  { id: 2, name: '可不可', tag: '茶飲專門' },
  { id: 3, name: '清心', tag: '老字號' },
  { id: 4, name: 'CoCo', tag: '國民飲料' },
  { id: 5, name: '麻古', tag: '芋頭專門' }
]

const mockFavoriteDrinks = [
  { id: 1, shop: '五十嵐', name: '蜂蜜檸檬綠', spec: '半糖少冰' },
  { id: 2, shop: '可不可', name: '鐵觀音奶茶', spec: '微糖去冰' },
  { id: 3, shop: '清心', name: '紅茶拿鐵', spec: '無糖' }
]
</script>

<template>
  <div class="space-y-12 md:space-y-16">
    <!-- IDENTITY HEADER -->
    <section class="flex flex-col md:flex-row items-center md:items-end gap-8 md:gap-12 pt-4 md:pt-8 border-b-8 border-black dark:border-white pb-12">
      <div class="relative">
        <div class="w-40 h-40 md:w-64 md:h-64 brutalist-card p-1 overflow-hidden">
          <img :src="avatarSrc" :alt="authStore.currentUser?.name ?? 'avatar'" class="w-full h-full object-cover">
        </div>
        <NuxtLink
          to="/settings"
          class="absolute -bottom-4 -right-4 w-12 h-12 md:w-16 md:h-16 bg-brand border-4 border-black dark:border-white shadow-brutalist dark:shadow-brutalist-dark-sm flex items-center justify-center text-white hover:scale-110 transition-transform"
          aria-label="Settings"
        >
          <Settings class="w-6 h-6 md:w-8 md:h-8" />
        </NuxtLink>
      </div>

      <div class="flex-1 text-center md:text-left flex flex-col items-center md:items-start min-w-0 w-full">
        <h2 class="text-4xl md:text-7xl lg:text-8xl italic leading-none truncate w-full">
          {{ authStore.currentUser?.name ?? '—' }}
        </h2>
        <p class="text-sm md:text-base font-bold italic opacity-60 mt-3 truncate w-full">
          {{ authStore.currentUser?.email ?? '—' }}
        </p>
        <p v-if="joinedLabel" class="text-[10px] md:text-xs font-black uppercase tracking-widest opacity-40 mt-2">
          Joined {{ joinedLabel }}
        </p>

        <div class="flex flex-wrap items-center gap-3 md:gap-6 mt-5">
          <div v-if="authStore.currentUser?.email_verified" class="flex items-center gap-2">
            <BadgeCheck class="w-4 h-4 text-green-500" />
            <p class="text-[10px] md:text-xs font-black uppercase tracking-widest text-green-600 dark:text-green-400">
              Email Verified
            </p>
          </div>
          <div v-else class="flex items-center gap-2">
            <AlertCircle class="w-4 h-4 text-red-500" />
            <p class="text-[10px] md:text-xs font-black uppercase tracking-widest text-red-500">
              Email Not Verified
            </p>
          </div>
          <div class="flex items-center gap-2">
            <span :class="['w-2 h-2 rounded-full', authStore.currentUser?.is_google_connected ? 'bg-brand' : 'bg-dark/40 dark:bg-white/40']" />
            <p class="text-[10px] md:text-xs font-black uppercase tracking-widest opacity-60">
              Google {{ authStore.currentUser?.is_google_connected ? 'Connected' : 'Not Connected' }}
            </p>
          </div>
        </div>
      </div>
    </section>

    <!-- STATS (Mockup) -->
    <section>
      <SectionHeader title="Stats">
        <template #right>
          <span class="text-[10px] font-black uppercase tracking-widest opacity-40 italic">
            Mockup data — coming soon
          </span>
        </template>
      </SectionHeader>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 md:gap-6 mt-6">
        <div
          v-for="(stat, idx) in mockStats"
          :key="stat.label"
          class="brutalist-card p-6 bg-white dark:bg-dark-surface relative overflow-hidden animate-fade-in"
          :style="{ animationDelay: `${idx * 50}ms` }"
        >
          <p class="text-[10px] font-black uppercase tracking-widest opacity-40 mb-3">{{ stat.label }}</p>
          <p :class="['text-2xl md:text-4xl font-black italic leading-none', stat.accent]">{{ stat.value }}</p>
          <p v-if="stat.sub" class="text-[10px] font-bold uppercase tracking-widest opacity-50 mt-2">{{ stat.sub }}</p>
        </div>
      </div>
    </section>

    <!-- MY FAVORITES (Mockup) -->
    <section>
      <SectionHeader title="My Favorites">
        <template #right>
          <NuxtLink
            to="/favorites"
            class="brutalist-button px-3 py-1.5 text-[10px] flex items-center gap-1"
          >
            <Edit3 class="w-3 h-3" />
            <span>編輯</span>
          </NuxtLink>
        </template>
      </SectionHeader>

      <!-- Shops -->
      <div class="mt-6 space-y-3">
        <p class="text-[10px] font-black uppercase tracking-widest opacity-60">
          Shops ({{ mockFavoriteShops.length }})
        </p>
        <div class="flex gap-4 overflow-x-auto pb-2 custom-scrollbar">
          <NuxtLink
            v-for="(shop, idx) in mockFavoriteShops"
            :key="shop.id"
            to="/favorites"
            class="shrink-0 w-32 brutalist-card p-4 bg-white dark:bg-dark-surface text-center animate-fade-in"
            :style="{ animationDelay: `${idx * 40}ms` }"
          >
            <div class="w-16 h-16 mx-auto border-2 border-black dark:border-white bg-page-bg dark:bg-dark-bg flex items-center justify-center mb-2">
              <Store class="w-8 h-8 opacity-60" />
            </div>
            <p class="text-sm italic font-black leading-tight truncate">{{ shop.name }}</p>
            <p class="text-[9px] font-black uppercase tracking-widest opacity-40 mt-1 truncate">{{ shop.tag }}</p>
          </NuxtLink>
        </div>
      </div>

      <!-- Drinks -->
      <div class="mt-8 space-y-3">
        <p class="text-[10px] font-black uppercase tracking-widest opacity-60">
          Drinks ({{ mockFavoriteDrinks.length }})
        </p>
        <div class="flex gap-4 overflow-x-auto pb-2 custom-scrollbar">
          <NuxtLink
            v-for="(drink, idx) in mockFavoriteDrinks"
            :key="drink.id"
            to="/favorites"
            class="shrink-0 w-56 brutalist-card p-4 bg-white dark:bg-dark-surface flex items-center gap-3 animate-fade-in"
            :style="{ animationDelay: `${idx * 40}ms` }"
          >
            <div class="w-12 h-12 border-2 border-black dark:border-white bg-page-bg dark:bg-dark-bg flex items-center justify-center shrink-0">
              <Coffee class="w-6 h-6 opacity-60" />
            </div>
            <div class="flex-1 min-w-0 text-left">
              <p class="text-sm italic font-black leading-tight truncate">{{ drink.name }}</p>
              <p class="text-[9px] font-black uppercase tracking-widest opacity-40 mt-1 truncate">
                {{ drink.shop }} · {{ drink.spec }}
              </p>
            </div>
            <ChevronRight class="w-4 h-4 opacity-30 shrink-0" />
          </NuxtLink>
        </div>
      </div>

      <p class="text-[10px] font-black uppercase tracking-widest opacity-40 italic mt-6">
        Mockup data — coming soon
      </p>
    </section>
  </div>
</template>
