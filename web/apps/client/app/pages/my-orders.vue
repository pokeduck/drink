<script setup lang="ts">
import { ShoppingBag, Clock, Coffee } from 'lucide-vue-next'

const { groups, currentUser } = useMockData()

const userOrders = computed(() =>
  groups.flatMap(group =>
    group.items
      .filter(item => item.userId === currentUser.id)
      .map(item => ({
        ...item,
        groupStatus: group.status,
        storeName: group.storeName,
        storeLogo: group.storeLogo,
        deadline: group.deadline
      }))
  )
)
</script>

<template>
  <div class="space-y-10">
    <h1 class="text-5xl italic leading-none">Orders</h1>

    <div v-if="userOrders.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
      <div
        v-for="(order, index) in userOrders"
        :key="order.id"
        class="brutalist-card p-6 bg-white dark:bg-dark-surface animate-slide-in"
        :style="{ animationDelay: `${index * 50}ms` }"
      >
        <div class="flex justify-between items-start mb-6 pb-4 border-b-2 border-black dark:border-white/20 border-dashed">
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 border-2 border-black dark:border-white bg-slate-50 dark:bg-white/5 flex-shrink-0">
              <img :src="order.storeLogo" :alt="order.storeName" class="w-full h-full object-cover rounded-sm">
            </div>
            <div>
              <h3 class="text-xl italic">{{ order.storeName }}</h3>
              <div class="flex items-center gap-1 text-[10px] font-black uppercase tracking-widest opacity-40">
                <Clock class="w-3 h-3" />
                <span>{{ new Date(order.deadline).toLocaleDateString() }}</span>
              </div>
            </div>
          </div>
          <StatusBadge :status="order.groupStatus" variant="simple" />
        </div>

        <div class="flex items-center gap-5 py-4">
          <div class="w-10 h-10 border-2 border-black dark:border-white bg-sidebar-bg dark:bg-white/5 flex items-center justify-center">
            <Coffee class="w-5 h-5 text-dark dark:text-white" />
          </div>
          <div class="flex-1">
            <p class="text-lg font-black italic leading-none">{{ order.drinkName }}</p>
            <p class="text-[10px] font-bold opacity-60 uppercase mt-1">{{ order.specifications }}</p>
          </div>
          <span class="text-2xl font-black italic">{{ formatPrice(order.price) }}</span>
        </div>

        <div class="mt-6 pt-6 border-t-2 border-black dark:border-white flex items-center justify-between">
          <div class="flex items-center gap-2">
            <div
              :class="[
                'w-3 h-3 border-2 border-black dark:border-white',
                order.paid ? 'bg-green-400' : 'bg-red-400 animate-pulse'
              ]"
            />
            <span class="text-[10px] font-black uppercase tracking-widest">{{ order.paid ? 'PAID' : 'UNPAID' }}</span>
          </div>
          <button v-if="!order.paid" class="brutalist-button brutalist-button-primary py-2 px-6">
            PAY NOW
          </button>
        </div>
      </div>
    </div>

    <EmptyState v-else title="NO ORDERS YET" subtitle="Go find a group buy!">
      <ShoppingBag class="w-16 h-16" />
    </EmptyState>
  </div>
</template>

<style scoped>
@keyframes slide-in {
  from {
    opacity: 0;
    transform: translateX(-10px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}
.animate-slide-in {
  animation: slide-in 0.2s ease both;
}
</style>
