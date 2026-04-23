<script setup lang="ts">
import { Clock, Users, ArrowRight } from 'lucide-vue-next'
import type { GroupOrder } from '~/composables/useMockData'

defineProps<{
  group: GroupOrder
}>()
</script>

<template>
  <NuxtLink
    :to="`/group/${group.id}`"
    class="brutalist-card block p-6 relative overflow-hidden"
  >
    <div class="absolute top-0 right-0">
      <StatusBadge :status="group.status" />
    </div>

    <div class="flex gap-4 mb-4">
      <div class="w-14 h-14 border-2 border-black dark:border-white bg-sidebar-bg dark:bg-white/10 flex-shrink-0 flex items-center justify-center overflow-hidden">
        <img :src="group.storeLogo" :alt="group.storeName" class="w-full h-full object-cover">
      </div>
      <div>
        <h3 class="text-xl italic">{{ group.storeName }}</h3>
        <div class="flex items-center gap-1.5 text-[10px] font-mono uppercase font-black tracking-widest mt-1 opacity-60">
          <Clock class="w-3 h-3" />
          <span>Until {{ formatTime(group.deadline) }}</span>
        </div>
      </div>
    </div>

    <p class="text-xs font-bold leading-relaxed opacity-70 mb-6 italic">
      {{ group.description }}
    </p>

    <div class="flex items-center justify-between pt-4 border-t-2 border-black dark:border-white/20 border-dashed">
      <div class="flex items-center gap-1 text-[10px] font-black uppercase tracking-widest">
        <Users class="w-4 h-4" />
        <span>{{ group.items.length }} Joined</span>
      </div>
      <div class="text-brand text-xs font-black uppercase tracking-widest italic flex items-center gap-1">
        Join Now <ArrowRight class="w-4 h-4 group-hover:translate-x-1 transition-transform" />
      </div>
    </div>
  </NuxtLink>
</template>
