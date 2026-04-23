<script setup lang="ts">
import { ChevronRight } from 'lucide-vue-next'
import type { Drink } from '~/composables/useMockData'

const route = useRoute()
const { groups, stores, currentUser } = useMockData()

const group = computed(() => groups.find(g => g.id === route.params.id))
const store = computed(() => stores.find(s => s.name === group.value?.storeName))
const isHost = computed(() => group.value?.hostId === currentUser.id)

const selectedDrink = ref<Drink | null>(null)
const isOrderModalOpen = computed({
  get: () => !!selectedDrink.value,
  set: (v) => { if (!v) selectedDrink.value = null }
})
const specs = reactive({ size: '大杯', sugar: '全糖', ice: '正常冰' })
const isOrdered = ref(false)

function handlePlaceOrder() {
  isOrdered.value = true
  setTimeout(() => {
    navigateTo('/my-orders')
  }, 2000)
}

function openOrderModal(drink: Drink) {
  selectedDrink.value = drink
  isOrdered.value = false
}

const sizeOptions = ['Large', 'Medium']
const sugarOptions = ['100%', '75%', '50%', '25%', '0%']
</script>

<template>
  <div v-if="group && store" class="pb-24">
    <!-- Group Info Header -->
    <div class="flex items-center gap-6 mb-8">
      <BackButton />
      <div class="w-20 h-20 brutalist-card p-1">
        <img :src="store.logo" :alt="store.name" class="w-full h-full object-cover">
      </div>
      <div class="flex-1">
        <h1 class="text-5xl italic leading-none truncate">{{ store.name }}</h1>
        <p class="text-xs font-black uppercase tracking-widest opacity-60 mt-2 italic">{{ group.description }}</p>
      </div>
    </div>

    <!-- Host Controls -->
    <div v-if="isHost" class="mb-8 p-6 brutalist-card bg-brand/10 flex flex-col gap-4">
      <div class="flex items-center gap-3">
        <h3 class="text-xl italic text-brand">Host Controls</h3>
      </div>
      <NuxtLink :to="`/admin/${group.id}`" class="brutalist-button brutalist-button-primary w-full text-center">
        Management Panel
      </NuxtLink>
    </div>

    <!-- Menu -->
    <div class="space-y-8">
      <SectionHeader title="Menu">
        <template #right>
          <span class="hidden md:block text-[10px] font-black uppercase tracking-widest opacity-40">Select a drink to customize</span>
        </template>
      </SectionHeader>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <button
          v-for="drink in store.menu"
          :key="drink.id"
          class="brutalist-card flex items-center justify-between p-6 text-left group bg-white active:scale-[0.98] transition-transform"
          @click="openOrderModal(drink)"
        >
          <div class="flex flex-col gap-1">
            <h3 class="text-xl italic">{{ drink.name }}</h3>
            <p class="text-[9px] font-black uppercase tracking-widest opacity-40">{{ drink.category }}</p>
          </div>
          <div class="flex flex-col items-end gap-2">
            <span class="text-2xl font-black italic text-brand">{{ formatPrice(drink.price) }}</span>
            <div class="text-[9px] font-black uppercase tracking-widest text-dark/30 group-hover:text-dark transition-colors flex items-center gap-1">
              ORDER <ChevronRight class="w-3 h-3" />
            </div>
          </div>
        </button>
      </div>
    </div>

    <!-- Order Modal -->
    <BrutalistModal v-model="isOrderModalOpen" size="sm">
      <template v-if="selectedDrink">
        <div class="p-10">
          <div class="flex justify-between items-start mb-8 pb-6 border-b-2 border-black dark:border-white/20">
            <div>
              <h3 class="text-4xl italic">{{ selectedDrink.name }}</h3>
              <p class="text-xs font-black uppercase tracking-widest opacity-40 mt-1 italic">{{ selectedDrink.category }}</p>
            </div>
            <div class="text-3xl font-black italic text-brand">
              {{ formatPrice(selectedDrink.price) }}
            </div>
          </div>

          <div class="space-y-8 mb-10">
            <div>
              <label class="text-[10px] font-black uppercase tracking-widest mb-3 block">Size</label>
              <div class="grid grid-cols-2 gap-3">
                <button
                  v-for="size in sizeOptions"
                  :key="size"
                  :class="[
                    'brutalist-button py-3 text-sm italic transition-colors',
                    specs.size === size
                      ? 'bg-dark text-white dark:bg-white dark:text-dark'
                      : 'bg-white text-dark dark:bg-white/5 dark:text-white'
                  ]"
                  @click="specs.size = size"
                >
                  {{ size }}
                </button>
              </div>
            </div>

            <div>
              <label class="text-[10px] font-black uppercase tracking-widest mb-3 block">Sugar</label>
              <div class="grid grid-cols-4 gap-2">
                <button
                  v-for="sugar in sugarOptions"
                  :key="sugar"
                  :class="[
                    'brutalist-button py-2 px-1 text-[10px] italic transition-colors',
                    specs.sugar === sugar
                      ? 'bg-dark text-white dark:bg-white dark:text-dark'
                      : 'bg-white text-dark dark:bg-white/5 dark:text-white'
                  ]"
                  @click="specs.sugar = sugar"
                >
                  {{ sugar }}
                </button>
              </div>
            </div>
          </div>

          <div class="flex gap-4">
            <button
              :disabled="isOrdered"
              class="flex-1 brutalist-button brutalist-button-primary py-4 text-lg"
              @click="handlePlaceOrder"
            >
              {{ isOrdered ? 'PROCESSING...' : 'ADD TO ORDER' }}
            </button>
          </div>
        </div>
      </template>
    </BrutalistModal>
  </div>

  <div v-else>找不到此揪團</div>
</template>
