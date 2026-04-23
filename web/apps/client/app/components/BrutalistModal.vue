<script setup lang="ts">
const props = withDefaults(defineProps<{
  modelValue: boolean
  size?: 'full' | 'md' | 'sm'
}>(), {
  size: 'md'
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

function close() {
  emit('update:modelValue', false)
}

const sizeClass = computed(() => {
  switch (props.size) {
    case 'sm': return 'md:w-[480px] md:max-h-[70vh]'
    case 'full': return 'md:w-[700px] md:h-[80vh]'
    case 'md':
    default: return 'md:w-[700px] md:h-[80vh]'
  }
})
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modelValue" class="fixed inset-0 z-[90]">
        <!-- Backdrop (desktop) -->
        <div
          class="absolute inset-0 bg-black/60 backdrop-blur-sm hidden md:block"
          @click="close"
        />

        <!-- Modal -->
        <div
          :class="[
            'fixed z-[100] bg-page-bg dark:bg-dark-bg flex flex-col',
            'inset-0',
            'md:inset-auto md:top-1/2 md:left-1/2 md:-translate-x-1/2 md:-translate-y-1/2',
            'md:border-4 md:border-black md:dark:border-white md:shadow-brutalist-2xl md:dark:shadow-brutalist-dark-2xl',
            sizeClass
          ]"
        >
          <!-- Header -->
          <div v-if="$slots.header" class="p-6 border-b-4 border-black dark:border-white shrink-0 bg-white dark:bg-dark-surface">
            <slot name="header" :close="close" />
          </div>

          <!-- Content -->
          <div class="flex-1 overflow-y-auto custom-scrollbar">
            <slot :close="close" />
          </div>

          <!-- Footer -->
          <div v-if="$slots.footer" class="shrink-0">
            <slot name="footer" :close="close" />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
