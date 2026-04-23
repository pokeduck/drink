<script setup lang="ts">
withDefaults(defineProps<{
  title: string
  muted?: boolean
}>(), {
  muted: false
})
</script>

<template>
  <div
    :class="[
      'flex items-center justify-between mb-8 pb-4',
      muted ? 'border-b-4 border-black/10 dark:border-white/10' : 'border-b-4 border-black dark:border-white'
    ]"
  >
    <h2
      :class="[
        'italic leading-none',
        muted
          ? 'text-2xl md:text-4xl font-black uppercase tracking-widest opacity-40 dark:text-dark-text/40'
          : 'text-4xl md:text-6xl'
      ]"
    >
      {{ title }}
    </h2>
    <slot name="right" />
  </div>
</template>
