<script setup lang="ts">
import { CheckCircle2, User, Coffee, Send } from 'lucide-vue-next'
import { OrderStatus } from '~/composables/useMockData'

const route = useRoute()
const { groups } = useMockData()

const initialGroup = groups.find(g => g.id === route.params.id)
const group = ref(initialGroup ? { ...initialGroup, items: initialGroup.items.map(i => ({ ...i })) } : null)

const totalAmount = computed(() =>
  group.value?.items.reduce((sum, item) => sum + item.price, 0) ?? 0
)

const paidAmount = computed(() =>
  group.value?.items.filter(i => i.paid).reduce((sum, item) => sum + item.price, 0) ?? 0
)

const statusFlow = [
  { status: OrderStatus.OPEN, label: '揪團中', icon: User },
  { status: OrderStatus.ORDERED, label: '已下單', icon: Coffee },
  { status: OrderStatus.READY, label: '可取餐', icon: CheckCircle2 }
]

function handleStatusChange(newStatus: OrderStatus) {
  if (group.value) {
    group.value.status = newStatus
  }
}

function togglePaid(itemId: string) {
  if (!group.value) return
  const item = group.value.items.find(i => i.id === itemId)
  if (item) {
    item.paid = !item.paid
  }
}

function currentStepIndex() {
  return statusFlow.findIndex(s => s.status === group.value?.status)
}

function progressWidth() {
  if (group.value?.status === OrderStatus.ORDERED) return 'w-1/2'
  if (group.value?.status === OrderStatus.READY) return 'w-full'
  return 'w-0'
}
</script>

<template>
  <div v-if="group" class="pb-10">
    <div class="flex items-center gap-4 mb-8">
      <BackButton />
      <h1 class="text-4xl italic leading-none">Management</h1>
    </div>

    <!-- Progress Tracker -->
    <div class="brutalist-card p-8 bg-white dark:bg-dark-surface mb-8">
      <div class="flex justify-between relative mb-12">
        <div class="absolute top-5 left-8 right-8 h-1 bg-dark/10 dark:bg-white/10 -z-0" />
        <div :class="['absolute top-5 left-8 h-1 bg-brand transition-all duration-500 -z-0', progressWidth()]" />

        <div v-for="(step, index) in statusFlow" :key="step.status" class="relative z-10 flex flex-col items-center">
          <button
            :class="[
              'w-12 h-12 border-2 border-black dark:border-white flex items-center justify-center transition-all duration-300',
              group.status === step.status
                ? 'bg-brand text-white shadow-brutalist dark:shadow-brutalist-dark-sm scale-110'
                : currentStepIndex() > index
                  ? 'bg-brand/20 text-brand'
                  : 'bg-white dark:bg-dark-surface text-dark/20 dark:text-white/20'
            ]"
            @click="handleStatusChange(step.status)"
          >
            <component :is="step.icon" class="w-6 h-6" />
          </button>
          <span
            :class="[
              'text-[9px] mt-3 font-black uppercase tracking-widest',
              group.status === step.status ? 'text-dark' : 'text-dark/40'
            ]"
          >
            {{ step.label }}
          </span>
        </div>
      </div>

      <!-- Financial Summary -->
      <div class="flex border-4 border-black dark:border-white border-double p-5 bg-page-bg dark:bg-dark-bg">
        <div class="flex-1 text-center">
          <p class="text-[10px] font-black text-dark dark:text-white/60 uppercase tracking-widest mb-1">Total</p>
          <p class="text-2xl font-black italic">{{ formatPrice(totalAmount) }}</p>
        </div>
        <div class="w-1 bg-black dark:bg-white mx-4" />
        <div class="flex-1 text-center">
          <p class="text-[10px] font-black text-dark dark:text-white/60 uppercase tracking-widest mb-1">Paid</p>
          <p class="text-2xl font-black text-green-600 dark:text-green-400 italic">{{ formatPrice(paidAmount) }}</p>
        </div>
      </div>
    </div>

    <!-- Order List -->
    <SectionHeader title="Order List" />

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="item in group.items" :key="item.id" class="brutalist-card p-6 bg-white dark:bg-dark-surface flex items-center justify-between">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 border-2 border-black dark:border-white p-0.5 overflow-hidden">
            <img :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${item.userName}`" :alt="item.userName">
          </div>
          <div>
            <h4 class="font-black italic text-lg leading-none">{{ item.userName }}</h4>
            <p class="text-[10px] font-bold opacity-60 uppercase mt-1">{{ item.drinkName }} · {{ item.specifications }}</p>
          </div>
        </div>
        <div class="flex items-center gap-4">
          <span class="font-mono font-black italic text-lg">{{ formatPrice(item.price) }}</span>
          <button
            :class="[
              'w-8 h-8 border-2 border-black dark:border-white flex items-center justify-center transition-colors shadow-brutalist-sm dark:shadow-brutalist-dark-sm',
              item.paid ? 'bg-green-400 text-dark' : 'bg-white dark:bg-white/5'
            ]"
            @click="togglePaid(item.id)"
          >
            <CheckCircle2 class="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>

    <div class="mt-12 space-y-6">
      <button class="w-full brutalist-button brutalist-button-primary py-5 text-lg flex items-center justify-center gap-3">
        <Send class="w-6 h-6" />
        <span>Broadcast Status</span>
      </button>
    </div>
  </div>

  <div v-else>找不到此揪團</div>
</template>
