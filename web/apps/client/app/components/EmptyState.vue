<script setup lang="ts">
defineProps<{
  title: string
  subtitle?: string
}>()
</script>

<template>
  <div class="bg-white dark:bg-dark-surface border-4 border-black dark:border-white border-dashed p-16 md:p-24 text-center">
    <div v-if="$slots.default" class="mb-6 flex justify-center opacity-20">
      <slot />
    </div>
    <p class="text-dark dark:text-white font-black tracking-widest uppercase italic text-2xl md:text-4xl">{{ title }}</p>
    <p v-if="subtitle" class="text-xs md:text-sm font-bold opacity-40 dark:text-dark-text/40 mt-4 uppercase tracking-widest">{{ subtitle }}</p>
  </div>
</template>
