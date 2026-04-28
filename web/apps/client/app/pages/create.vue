<script setup lang="ts">
import { Calendar, Send, Search, Check } from 'lucide-vue-next'
import type { Store } from '~/composables/useMockData'

const { stores } = useMockData()

const selectedStore = ref<Store | null>(null)
const deadline = ref('15:00')
const description = ref('')
const searchQuery = ref('')
const isSelectingShop = ref(false)

const filteredStores = computed(() =>
  stores.filter(store =>
    store.name.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
)

function selectStore(store: Store) {
  selectedStore.value = store
  isSelectingShop.value = false
  searchQuery.value = ''
}

function handleCreate() {
  navigateTo('/')
}
</script>

<template>
  <div class="pb-10">
    <div class="flex items-center gap-4 mb-10">
      <BackButton />
      <h1 class="text-4xl italic leading-none text-dark dark:text-white">New Group Buy</h1>
    </div>

    <div class="space-y-12">
      <!-- Step 1: Shop Selection -->
      <section>
        <FormLabel text="1. Select Shop" />
        <div v-if="selectedStore" class="brutalist-card p-6 bg-white dark:bg-dark-surface flex items-center justify-between border-brand dark:border-brand">
          <div class="flex items-center gap-6">
            <StoreLogo :src="selectedStore.logo" :alt="selectedStore.name" />
            <div>
              <h3 class="text-2xl italic leading-tight">{{ selectedStore.name }}</h3>
              <p class="text-[10px] font-bold opacity-40 uppercase tracking-widest mt-1">Ready to take orders</p>
            </div>
          </div>
          <button
            class="text-xs font-black uppercase tracking-widest text-brand underline underline-offset-4"
            @click="isSelectingShop = true"
          >
            Change Shop
          </button>
        </div>
        <button
          v-else
          class="w-full brutalist-card p-10 bg-white dark:bg-dark-surface border-dashed border-4 border-black/10 dark:border-white/10 flex flex-col items-center justify-center gap-4 hover:border-brand transition-colors group"
          @click="isSelectingShop = true"
        >
          <div class="w-16 h-16 border-2 border-black dark:border-white flex items-center justify-center group-hover:bg-brand group-hover:text-white transition-colors">
            <Search class="w-6 h-6" />
          </div>
          <span class="text-xl font-black italic uppercase tracking-tighter">Click to browse 50+ shops</span>
        </button>
      </section>

      <!-- Step 2: Deadline -->
      <section :class="['transition-opacity duration-300', !selectedStore && 'opacity-30 pointer-events-none']">
        <FormLabel text="2. Deadline Time" />
        <div class="relative">
          <Calendar class="absolute left-5 top-1/2 -translate-y-1/2 w-5 h-5 text-dark dark:text-dark-text pointer-events-none" />
          <input
            v-model="deadline"
            type="time"
            class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-5 pl-14 pr-5 font-black text-2xl italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark transition-colors"
          >
        </div>
      </section>

      <!-- Step 3: Notes -->
      <section :class="['transition-opacity duration-300', !selectedStore && 'opacity-30 pointer-events-none']">
        <FormLabel text="3. Notes for Members" />
        <textarea
          v-model="description"
          placeholder="Let everyone know where to pick up..."
          class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 p-5 font-bold text-sm min-h-[140px] focus:outline-none shadow-brutalist dark:shadow-brutalist-dark resize-none italic transition-colors"
        />
      </section>

      <button
        :disabled="!selectedStore"
        class="w-full brutalist-button brutalist-button-primary py-6 text-xl mt-6 flex items-center justify-center gap-3"
        @click="handleCreate"
      >
        <Send class="w-6 h-6" />
        <span>Launch Group</span>
      </button>
    </div>

    <!-- Shop Selection Modal -->
    <BrutalistModal v-model="isSelectingShop" size="full">
      <template #header="{ close }">
        <div class="flex items-center gap-4">
          <BackButton icon="close" @click="close" />
          <div class="flex-1">
            <h2 class="text-2xl font-black italic leading-none text-dark dark:text-white">Choose Shop</h2>
            <p class="text-[10px] font-black uppercase tracking-widest opacity-40 mt-1">Select from 50+ merchants</p>
          </div>
        </div>
      </template>

      <!-- Search + Shop List -->
      <div>
        <div class="p-6 bg-white dark:bg-dark-surface border-b-2 border-black dark:border-white/10 shrink-0">
          <div class="relative">
            <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 opacity-40 dark:text-dark-text/40" />
            <input
              v-model="searchQuery"
              autofocus
              type="text"
              placeholder="Search by name..."
              class="w-full bg-page-bg dark:bg-dark-bg border-2 border-black dark:border-white/20 py-4 pl-12 pr-4 text-lg font-bold italic focus:border-brand transition-colors text-dark dark:text-white"
            >
          </div>
        </div>

        <div class="p-6 space-y-4">
          <button
            v-for="(store, index) in filteredStores"
            :key="store.id"
            :class="[
              'w-full brutalist-card flex items-center gap-4 p-5 text-left transition-all animate-slide-in',
              selectedStore?.id === store.id
                ? 'bg-brand/10 border-brand dark:border-brand shadow-brutalist-brand-active'
                : 'bg-white dark:bg-dark-surface'
            ]"
            :style="{ animationDelay: `${index * 10}ms` }"
            @click="selectStore(store)"
          >
            <StoreLogo :src="store.logo" :alt="store.name" />
            <div class="flex-1">
              <h3 class="text-xl italic leading-tight">{{ store.name }}</h3>
              <p class="text-[10px] font-bold opacity-40 uppercase tracking-widest mt-1">Drinks · Dessert</p>
            </div>
            <div
              v-if="selectedStore?.id === store.id"
              class="w-8 h-8 border-2 border-black dark:border-white bg-brand flex items-center justify-center"
            >
              <Check class="w-4 h-4 text-white" />
            </div>
          </button>

          <div v-if="filteredStores.length === 0" class="py-20 text-center flex flex-col items-center gap-4">
            <div class="w-16 h-16 border-2 border-black dark:border-white border-dashed rounded-full flex items-center justify-center opacity-20">
              <Search class="w-8 h-8" />
            </div>
            <p class="uppercase font-black italic opacity-40">No shops found matching your search</p>
          </div>
        </div>
      </div>

      <template #footer="{ close }">
        <footer class="p-6 bg-black dark:bg-black text-white flex justify-between items-center">
          <span class="text-xs font-mono tracking-widest uppercase opacity-60">{{ filteredStores.length }} RESULTS</span>
          <button
            class="text-brand font-black uppercase text-xs tracking-widest underline underline-offset-4"
            @click="close"
          >
            CANCEL
          </button>
        </footer>
      </template>
    </BrutalistModal>
  </div>
</template>
