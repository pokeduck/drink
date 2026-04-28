<script setup lang="ts">
defineProps<{
  text?: string
  for?: string
}>()
</script>

<template>
  <label
    :for="$props.for"
    class="text-[10px] font-black uppercase tracking-widest mb-3 block opacity-60"
  >
    <slot>{{ text }}</slot>
  </label>
</template>
