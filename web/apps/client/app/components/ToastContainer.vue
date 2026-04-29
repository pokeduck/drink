<script setup lang="ts">
import { CheckCircle2, XCircle, Info } from 'lucide-vue-next'

const { toasts } = useToast()

const toneClass = {
  success: 'bg-green-500 text-white border-black dark:border-white',
  error: 'bg-red-500 text-white border-black dark:border-white',
  info: 'bg-black text-white dark:bg-white dark:text-black border-black dark:border-white'
}
const iconFor = {
  success: CheckCircle2,
  error: XCircle,
  info: Info
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed bottom-24 md:bottom-12 left-1/2 -translate-x-1/2 z-[80] flex flex-col items-center gap-2 pointer-events-none">
      <TransitionGroup name="dropdown">
        <div
          v-for="t in toasts"
          :key="t.id"
          :class="['flex items-center gap-3 px-5 py-3 border-2 shadow-brutalist-lg dark:shadow-brutalist-dark-lg pointer-events-auto', toneClass[t.type]]"
        >
          <component :is="iconFor[t.type]" class="w-4 h-4 shrink-0" />
          <p class="text-xs font-black uppercase tracking-widest italic">{{ t.message }}</p>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>
