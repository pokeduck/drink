<script setup lang="ts">
import { User, Mail, Lock, Send, Mailbox } from 'lucide-vue-next'
import { useAuthStore } from '~/stores/auth'

definePageMeta({ layout: false })
useHead({ title: 'Create account · DRINK.UP' })

const authStore = useAuthStore()
const name = ref('')
const email = ref('')
const password = ref('')
const passwordConfirm = ref('')
const submitting = ref(false)
const submittedEmail = ref<string | null>(null)
const generalError = ref<string | null>(null)
const fieldErrors = ref<Record<string, string[]>>({})

async function handleSubmit() {
  generalError.value = null
  fieldErrors.value = {}

  if (password.value !== passwordConfirm.value) {
    fieldErrors.value = { password_confirm: ['兩次密碼輸入不一致'] }
    return
  }

  submitting.value = true
  try {
    await authStore.register({
      name: name.value.trim(),
      email: email.value.trim(),
      password: password.value
    })
    submittedEmail.value = email.value.trim()
  } catch (err: unknown) {
    const e = err as { message?: string; errors?: Record<string, string[]> } | null
    if (e?.errors && typeof e.errors === 'object') {
      fieldErrors.value = e.errors
    }
    generalError.value = e?.message ?? '註冊失敗'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-page-bg dark:bg-dark-bg flex items-center justify-center px-4 py-10 transition-colors">
    <div class="w-full max-w-md">
      <h1 class="text-5xl md:text-6xl italic leading-none mb-10 text-dark dark:text-dark-text">DRINK.UP</h1>

      <section v-if="!submittedEmail" class="brutalist-card p-8 bg-white dark:bg-dark-surface space-y-6">
        <header class="space-y-1">
          <p class="text-[10px] font-black uppercase tracking-widest opacity-40">New here?</p>
          <h2 class="text-3xl italic leading-tight">Create Account</h2>
        </header>

        <form class="space-y-5" @submit.prevent="handleSubmit">
          <div>
            <FormLabel for="name" text="Name" />
            <div class="relative">
              <User class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 opacity-50" />
              <input
                id="name"
                v-model="name"
                required
                maxlength="100"
                class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-4 pl-12 pr-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
              >
            </div>
            <p v-if="fieldErrors.name" class="text-xs font-black text-red-500 mt-2 italic">
              {{ fieldErrors.name[0] }}
            </p>
          </div>

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
            <FormLabel for="password" text="Password (≥6)" />
            <div class="relative">
              <Lock class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 opacity-50" />
              <input
                id="password"
                v-model="password"
                type="password"
                required
                minlength="6"
                autocomplete="new-password"
                class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-4 pl-12 pr-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
              >
            </div>
            <p v-if="fieldErrors.password" class="text-xs font-black text-red-500 mt-2 italic">
              {{ fieldErrors.password[0] }}
            </p>
          </div>

          <div>
            <FormLabel for="password-confirm" text="Confirm Password" />
            <div class="relative">
              <Lock class="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 opacity-50" />
              <input
                id="password-confirm"
                v-model="passwordConfirm"
                type="password"
                required
                minlength="6"
                autocomplete="new-password"
                class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-4 pl-12 pr-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
              >
            </div>
            <p v-if="fieldErrors.password_confirm" class="text-xs font-black text-red-500 mt-2 italic">
              {{ fieldErrors.password_confirm[0] }}
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
            <Send class="w-5 h-5" />
            <span>{{ submitting ? 'Creating…' : 'Create Account' }}</span>
          </button>
        </form>

        <p class="text-xs font-bold italic opacity-60 text-center">
          已有帳號？
          <NuxtLink to="/login" class="text-brand underline underline-offset-2 font-black">登入</NuxtLink>
        </p>
      </section>

      <section v-else class="brutalist-card p-8 bg-white dark:bg-dark-surface space-y-6">
        <div class="w-16 h-16 border-2 border-black dark:border-white flex items-center justify-center">
          <Mailbox class="w-8 h-8 text-brand" />
        </div>
        <header class="space-y-1">
          <p class="text-[10px] font-black uppercase tracking-widest opacity-40">Verify your email</p>
          <h2 class="text-3xl italic leading-tight">Check your inbox</h2>
        </header>
        <p class="text-sm font-bold italic leading-relaxed">
          我們已寄出驗證信至 <span class="text-brand">{{ submittedEmail }}</span>。請點擊信件中的連結完成驗證後再登入。
        </p>
        <p class="text-[10px] font-mono uppercase tracking-widest opacity-40 leading-relaxed">
          [DEV] 目前驗證信僅輸出至 server log。請從 <code>api/User.API/logs/user-api-*.log</code> 找出 token，並開啟 <code>/verify-email?token=...</code>
        </p>
        <NuxtLink
          to="/login"
          class="brutalist-button w-full py-4 text-base flex items-center justify-center"
        >
          Back to Sign In
        </NuxtLink>
      </section>
    </div>
  </div>
</template>
