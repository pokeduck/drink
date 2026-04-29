<script setup lang="ts">
import { Mail, Lock, LogIn } from 'lucide-vue-next'
import { useAuthStore } from '~/stores/auth'

definePageMeta({ layout: false })
useHead({ title: 'Sign In · DRINK.UP' })

const authStore = useAuthStore()
const email = ref('')
const password = ref('')
const submitting = ref(false)
const generalError = ref<string | null>(null)
const fieldErrors = ref<Record<string, string[]>>({})

async function handleSubmit() {
  submitting.value = true
  generalError.value = null
  fieldErrors.value = {}

  try {
    await authStore.login({ email: email.value.trim(), password: password.value })
    await navigateTo('/')
  } catch (err: unknown) {
    const e = err as { message?: string; errors?: Record<string, string[]> } | null
    if (e?.errors && typeof e.errors === 'object') {
      fieldErrors.value = e.errors
    }
    generalError.value = e?.message ?? '登入失敗'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-page-bg dark:bg-dark-bg flex items-center justify-center px-4 py-10 transition-colors">
    <div class="w-full max-w-md">
      <h1 class="text-5xl md:text-6xl italic leading-none mb-10 text-dark dark:text-dark-text">DRINK.UP</h1>

      <section class="brutalist-card p-8 bg-white dark:bg-dark-surface space-y-6">
        <header class="space-y-1">
          <p class="text-[10px] font-black uppercase tracking-widest opacity-40">Welcome back</p>
          <h2 class="text-3xl italic leading-tight">Sign In</h2>
        </header>

        <form class="space-y-5" @submit.prevent="handleSubmit">
          <div>
            <FormLabel for="email" text="Email" />
            <div class="relative">
              <Mail class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 opacity-50" />
              <input
                id="email"
                v-model="email"
                type="email"
                required
                autocomplete="email"
                class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-4 pl-12 pr-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
              >
            </div>
            <p v-if="fieldErrors.email" class="text-xs font-black text-red-500 mt-2 italic">
              {{ fieldErrors.email[0] }}
            </p>
          </div>

          <div>
            <FormLabel for="password" text="Password" />
            <div class="relative">
              <Lock class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 opacity-50" />
              <input
                id="password"
                v-model="password"
                type="password"
                required
                autocomplete="current-password"
                class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-4 pl-12 pr-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
              >
            </div>
            <p v-if="fieldErrors.password" class="text-xs font-black text-red-500 mt-2 italic">
              {{ fieldErrors.password[0] }}
            </p>
          </div>

          <p v-if="generalError" class="text-sm font-black italic text-red-500 border-l-4 border-red-500 pl-3 py-2">
            {{ generalError }}
          </p>

          <button
            type="submit"
            :disabled="submitting"
            class="w-full brutalist-button brutalist-button-primary py-5 text-lg flex items-center justify-center gap-3 disabled:opacity-50"
          >
            <LogIn class="w-5 h-5" />
            <span>{{ submitting ? 'Signing in…' : 'Launch Session' }}</span>
          </button>
        </form>

        <p class="text-xs font-bold italic opacity-60 text-center">
          沒有帳號？
          <NuxtLink to="/register" class="text-brand underline underline-offset-2 font-black">建立新帳號</NuxtLink>
        </p>
      </section>
    </div>
  </div>
</template>
