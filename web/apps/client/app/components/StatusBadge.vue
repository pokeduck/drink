<script setup lang="ts">
import { OrderStatus } from '~/composables/useMockData'

const props = withDefaults(defineProps<{
  status: OrderStatus | string
  variant?: 'default' | 'simple'
}>(), {
  variant: 'default'
})

const statusColors: Record<string, string> = {
  [OrderStatus.OPEN]: 'bg-green-400 text-black',
  [OrderStatus.ORDERED]: 'bg-blue-400 text-black',
  [OrderStatus.ARRIVING]: 'bg-blue-300 text-black',
  [OrderStatus.READY]: 'bg-orange-400 text-black',
  [OrderStatus.COMPLETED]: 'bg-slate-300 text-black opacity-60',
  [OrderStatus.CANCELLED]: 'bg-red-400 text-black'
}

const statusLabels: Record<string, string> = {
  [OrderStatus.OPEN]: 'Active',
  [OrderStatus.ORDERED]: 'Ordered',
  [OrderStatus.ARRIVING]: 'Arriving',
  [OrderStatus.READY]: 'Ready',
  [OrderStatus.COMPLETED]: 'Closed',
  [OrderStatus.CANCELLED]: 'Cancelled'
}

const badgeClass = computed(() => {
  if (props.variant === 'simple') {
    return props.status === OrderStatus.READY
      ? 'bg-brand text-white'
      : 'bg-white dark:bg-dark-surface text-dark/40 dark:text-dark-text/40'
  }
  return statusColors[props.status] ?? ''
})

const badgeLabel = computed(() => {
  if (props.variant === 'simple') {
    return props.status === OrderStatus.READY ? 'READY' : 'WAITING'
  }
  return statusLabels[props.status] ?? ''
})
</script>

<template>
  <span
    :class="[
      'text-[10px] font-black uppercase tracking-widest px-3 py-1 border-2 border-black dark:border-white',
      variant === 'default' && 'border-l-2 border-b-2',
      badgeClass
    ]"
  >
    {{ badgeLabel }}
  </span>
</template>
