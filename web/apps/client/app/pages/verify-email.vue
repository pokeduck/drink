<script setup lang="ts">
import { CheckCircle2, XCircle, Loader2 } from 'lucide-vue-next'
import { useAuthStore } from '~/stores/auth'

definePageMeta({ layout: false })
useHead({ title: 'Verify Email · DRINK.UP' })

const route = useRoute()
const authStore = useAuthStore()
const status = ref<'pending' | 'success' | 'error'>('pending')
const errorMessage = ref<string | null>(null)

onMounted(async () => {
  const token = route.query.token
  if (typeof token !== 'string' || !token) {
    status.value = 'error'
    errorMessage.value = '驗證連結無效'
    return
  }

  try {
    await authStore.verifyEmail(token)
    status.value = 'success'
  } catch (err: unknown) {
    const e = err as { message?: string } | null
    status.value = 'error'
    errorMessage.value = e?.message ?? '驗證失敗'
  }
})
</script>

<template>
  <div class="min-h-screen bg-page-bg dark:bg-dark-bg flex items-center justify-center px-4 py-10 transition-colors">
    <div class="w-full max-w-md">
      <h1 class="text-5xl md:text-6xl italic leading-none mb-10 text-dark dark:text-dark-text">DRINK.UP</h1>

      <section class="brutalist-card p-10 bg-white dark:bg-dark-surface space-y-6 text-center">
        <div v-if="status === 'pending'" class="flex flex-col items-center gap-4">
          <Loader2 class="w-12 h-12 animate-spin text-brand" />
          <p class="text-2xl italic">驗證中…</p>
        </div>

        <div v-else-if="status === 'success'" class="flex flex-col items-center gap-5">
          <div class="w-16 h-16 border-2 border-black dark:border-white flex items-center justify-center">
            <CheckCircle2 class="w-8 h-8 text-green-500" />
          </div>
          <p class="text-[10px] font-black uppercase tracking-widest opacity-40">Verified</p>
          <h2 class="text-3xl italic leading-tight">Email 驗證成功！</h2>
          <p class="text-sm font-bold italic opacity-60">現在你可以使用此帳號登入。</p>
          <NuxtLink to="/login" class="brutalist-button brutalist-button-primary w-full py-4 text-base flex items-center justify-center mt-4">
            前往登入
          </NuxtLink>
        </div>

        <div v-else class="flex flex-col items-center gap-5">
          <div class="w-16 h-16 border-2 border-black dark:border-white flex items-center justify-center">
            <XCircle class="w-8 h-8 text-red-500" />
          </div>
          <p class="text-[10px] font-black uppercase tracking-widest opacity-40">Failed</p>
          <h2 class="text-3xl italic leading-tight">驗證失敗</h2>
          <p class="text-sm font-bold italic text-red-500">{{ errorMessage }}</p>
          <NuxtLink to="/login" class="brutalist-button w-full py-4 text-base flex items-center justify-center mt-4">
            返回登入
          </NuxtLink>
        </div>
      </section>
    </div>
  </div>
</template>
