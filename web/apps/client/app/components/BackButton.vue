<script setup lang="ts">
import { ChevronLeft, X } from 'lucide-vue-next'

const props = withDefaults(defineProps<{
  icon?: 'back' | 'close'
}>(), {
  icon: 'back'
})

const emit = defineEmits<{
  click: [e: MouseEvent]
}>()

const router = useRouter()

function handleClick(e: MouseEvent) {
  if (emit('click', e)) return
  router.back()
}
</script>

<template>
  <button
    class="p-1 border-2 border-black dark:border-white brutalist-card bg-white dark:bg-dark-surface block"
    @click="handleClick"
  >
    <X v-if="icon === 'close'" class="w-6 h-6" />
    <ChevronLeft v-else class="w-6 h-6" />
  </button>
</template>
