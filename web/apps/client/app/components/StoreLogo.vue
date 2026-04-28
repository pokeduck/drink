<script setup lang="ts">
const props = withDefaults(defineProps<{
  src: string
  alt?: string
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl'
}>(), {
  alt: '',
  size: 'md'
})

const sizeClass = computed(() => {
  switch (props.size) {
    case 'xs': return 'w-12 h-12'
    case 'sm': return 'w-14 h-14'
    case 'lg': return 'w-20 h-20'
    case 'xl': return 'w-24 h-24'
    case 'md':
    default: return 'w-16 h-16'
  }
})
</script>

<template>
  <div
    :class="[
      sizeClass,
      'border-2 border-black dark:border-white bg-slate-50 dark:bg-white/5 p-1 shrink-0 overflow-hidden'
    ]"
  >
    <img :src="src" :alt="alt" class="w-full h-full object-cover">
  </div>
</template>
