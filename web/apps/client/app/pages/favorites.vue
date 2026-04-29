<script setup lang="ts">
import { Store, Coffee, X, Star } from 'lucide-vue-next'

useHead({ title: '我的收藏 · DRINK.UP' })

const mockShops = [
  { id: 1, name: '五十嵐', tag: '經典珍奶' },
  { id: 2, name: '可不可', tag: '茶飲專門' },
  { id: 3, name: '清心', tag: '老字號' },
  { id: 4, name: 'CoCo', tag: '國民飲料' },
  { id: 5, name: '麻古', tag: '芋頭專門' }
]

const mockDrinks = [
  { id: 1, shop: '五十嵐', name: '蜂蜜檸檬綠', spec: '半糖少冰' },
  { id: 2, shop: '可不可', name: '鐵觀音奶茶', spec: '微糖去冰' },
  { id: 3, shop: '清心', name: '紅茶拿鐵', spec: '無糖' }
]

const tab = ref<'shops' | 'drinks'>('shops')
const toast = ref<string | null>(null)
let toastTimer: ReturnType<typeof setTimeout> | null = null

function fakeRemove() {
  toast.value = 'Mockup 階段尚未支援'
  if (toastTimer) clearTimeout(toastTimer)
  toastTimer = setTimeout(() => { toast.value = null }, 2000)
}
</script>

<template>
  <div class="space-y-12">
    <div class="flex items-center gap-4">
      <BackButton />
      <h1 class="text-4xl italic leading-none">我的收藏</h1>
    </div>

    <!-- Mobile: Tab switcher -->
    <div class="md:hidden">
      <div class="flex border-2 border-black dark:border-white shadow-brutalist dark:shadow-brutalist-dark">
        <button
          :class="[
            'flex-1 py-3 text-sm font-black uppercase tracking-widest italic transition-colors',
            tab === 'shops' ? 'bg-brand text-white' : 'bg-white dark:bg-dark-surface'
          ]"
          @click="tab = 'shops'"
        >
          店家 ({{ mockShops.length }})
        </button>
        <button
          :class="[
            'flex-1 py-3 text-sm font-black uppercase tracking-widest italic transition-colors border-l-2 border-black dark:border-white',
            tab === 'drinks' ? 'bg-brand text-white' : 'bg-white dark:bg-dark-surface'
          ]"
          @click="tab = 'drinks'"
        >
          飲料 ({{ mockDrinks.length }})
        </button>
      </div>

      <!-- Mobile shops tab -->
      <ul v-if="tab === 'shops'" class="space-y-3 mt-6">
        <li
          v-for="(shop, idx) in mockShops"
          :key="shop.id"
          class="brutalist-card p-4 bg-white dark:bg-dark-surface flex items-center gap-4 animate-fade-in"
          :style="{ animationDelay: `${idx * 30}ms` }"
        >
          <div class="w-14 h-14 border-2 border-black dark:border-white bg-page-bg dark:bg-dark-bg flex items-center justify-center">
            <Store class="w-7 h-7 opacity-60" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-lg italic leading-tight truncate">{{ shop.name }}</p>
            <p class="text-[10px] font-black uppercase tracking-widest opacity-40">{{ shop.tag }}</p>
          </div>
          <button
            class="w-9 h-9 border-2 border-black dark:border-white flex items-center justify-center hover:bg-red-500 hover:text-white transition-colors"
            aria-label="移除"
            @click="fakeRemove"
          >
            <X class="w-4 h-4" />
          </button>
        </li>
      </ul>

      <!-- Mobile drinks tab -->
      <ul v-else class="space-y-3 mt-6">
        <li
          v-for="(drink, idx) in mockDrinks"
          :key="drink.id"
          class="brutalist-card p-4 bg-white dark:bg-dark-surface flex items-center gap-4 animate-fade-in"
          :style="{ animationDelay: `${idx * 30}ms` }"
        >
          <div class="w-14 h-14 border-2 border-black dark:border-white bg-page-bg dark:bg-dark-bg flex items-center justify-center">
            <Coffee class="w-7 h-7 opacity-60" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-lg italic leading-tight truncate">{{ drink.name }}</p>
            <p class="text-[10px] font-black uppercase tracking-widest opacity-40">
              {{ drink.shop }} · {{ drink.spec }}
            </p>
          </div>
          <button
            class="w-9 h-9 border-2 border-black dark:border-white flex items-center justify-center hover:bg-red-500 hover:text-white transition-colors"
            aria-label="移除"
            @click="fakeRemove"
          >
            <X class="w-4 h-4" />
          </button>
        </li>
      </ul>
    </div>

    <!-- Desktop: Two sections side by side -->
    <div class="hidden md:grid md:grid-cols-2 gap-12">
      <section>
        <SectionHeader title="Shops">
          <template #right>
            <span class="text-xs font-black uppercase tracking-widest opacity-40">
              {{ mockShops.length }} 家店
            </span>
          </template>
        </SectionHeader>
        <ul class="space-y-3 mt-6">
          <li
            v-for="(shop, idx) in mockShops"
            :key="shop.id"
            class="brutalist-card p-4 bg-white dark:bg-dark-surface flex items-center gap-4 animate-fade-in"
            :style="{ animationDelay: `${idx * 30}ms` }"
          >
            <div class="w-14 h-14 border-2 border-black dark:border-white bg-page-bg dark:bg-dark-bg flex items-center justify-center">
              <Store class="w-7 h-7 opacity-60" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-lg italic leading-tight truncate">{{ shop.name }}</p>
              <p class="text-[10px] font-black uppercase tracking-widest opacity-40">{{ shop.tag }}</p>
            </div>
            <button
              class="w-9 h-9 border-2 border-black dark:border-white flex items-center justify-center hover:bg-red-500 hover:text-white transition-colors"
              aria-label="移除"
              @click="fakeRemove"
            >
              <X class="w-4 h-4" />
            </button>
          </li>
        </ul>
      </section>

      <section>
        <SectionHeader title="Drinks">
          <template #right>
            <span class="text-xs font-black uppercase tracking-widest opacity-40">
              {{ mockDrinks.length }} 杯
            </span>
          </template>
        </SectionHeader>
        <ul class="space-y-3 mt-6">
          <li
            v-for="(drink, idx) in mockDrinks"
            :key="drink.id"
            class="brutalist-card p-4 bg-white dark:bg-dark-surface flex items-center gap-4 animate-fade-in"
            :style="{ animationDelay: `${idx * 30}ms` }"
          >
            <div class="w-14 h-14 border-2 border-black dark:border-white bg-page-bg dark:bg-dark-bg flex items-center justify-center">
              <Coffee class="w-7 h-7 opacity-60" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-lg italic leading-tight truncate">{{ drink.name }}</p>
              <p class="text-[10px] font-black uppercase tracking-widest opacity-40">
                {{ drink.shop }} · {{ drink.spec }}
              </p>
            </div>
            <button
              class="w-9 h-9 border-2 border-black dark:border-white flex items-center justify-center hover:bg-red-500 hover:text-white transition-colors"
              aria-label="移除"
              @click="fakeRemove"
            >
              <X class="w-4 h-4" />
            </button>
          </li>
        </ul>
      </section>
    </div>

    <!-- Mockup notice -->
    <div class="border-2 border-dashed border-black/30 dark:border-white/30 p-6 text-center">
      <Star class="w-6 h-6 mx-auto mb-2 opacity-40" />
      <p class="text-xs font-black uppercase tracking-widest opacity-60">Mockup data — coming soon</p>
      <p class="text-xs font-bold italic opacity-50 mt-2">真實功能將於店家瀏覽功能上線後啟用</p>
    </div>

    <!-- Toast -->
    <Transition name="dropdown">
      <div
        v-if="toast"
        class="fixed bottom-24 md:bottom-12 left-1/2 -translate-x-1/2 bg-black dark:bg-white text-white dark:text-black px-6 py-3 border-2 border-black dark:border-white shadow-brutalist-lg dark:shadow-brutalist-dark-lg z-[80]"
      >
        <p class="text-xs font-black uppercase tracking-widest italic">{{ toast }}</p>
      </div>
    </Transition>
  </div>
</template>
