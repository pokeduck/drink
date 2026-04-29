<script setup lang="ts">
import { Save, X, Lock, LogOut, AtSign, Bell, Moon, Sun, Monitor, ImagePlus, Loader2 } from 'lucide-vue-next'
import { useAuthStore } from '~/stores/auth'
import type { components } from '@app/api-types/user'

useHead({ title: 'Settings · DRINK.UP' })

type NotificationType = components['schemas']['NotificationType']

const authStore = useAuthStore()
const userApi = useUserApi()
const colorMode = useColorMode()
const toast = useToast()

const profileForm = reactive({
  name: '',
  avatar: '' as string | null
})
const profileSubmitting = ref(false)
const profileError = ref<string | null>(null)
const profileFieldErrors = ref<Record<string, string[]>>({})

const passwordForm = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})
const passwordSubmitting = ref(false)
const passwordError = ref<string | null>(null)
const passwordFieldErrors = ref<Record<string, string[]>>({})

const prefForm = reactive({
  theme: colorMode.preference,
  notification: 0 as NotificationType
})
const prefSubmitting = ref(false)

const logoutAllSubmitting = ref(false)

const avatar = useAvatarUpload()
const dragOver = ref(false)
const fileInput = ref<HTMLInputElement>()

onMounted(async () => {
  if (!authStore.currentUser) await authStore.fetchProfile()
  syncFromStore()
})

watch(() => authStore.currentUser, syncFromStore)

function syncFromStore() {
  const u = authStore.currentUser
  if (!u) return
  profileForm.name = u.name ?? ''
  profileForm.avatar = u.avatar ?? ''
  prefForm.notification = (u.notification_type ?? 0) as NotificationType
  prefForm.theme = colorMode.preference
}

const currentAvatarSrc = computed(() => {
  if (avatar.previewUrl.value) return avatar.previewUrl.value
  const u = authStore.currentUser
  if (!u) return null
  return u.avatar || `https://api.dicebear.com/7.x/avataaars/svg?seed=${u.email}`
})

function onDrop(e: DragEvent) {
  e.preventDefault()
  dragOver.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) avatar.stageFile(file)
}

function onSelectFile(e: Event) {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) avatar.stageFile(file)
  target.value = ''
}

async function saveProfile() {
  profileSubmitting.value = true
  profileError.value = null
  profileFieldErrors.value = {}

  try {
    await withMinDelay((async () => {
      let finalAvatar: string | null = profileForm.avatar ?? null

      // 有 staged 圖才實際上傳
      if (avatar.stagedFile.value) {
        const uploadedPath = await avatar.commit()
        if (!uploadedPath) {
          profileError.value = avatar.error.value ?? '頭像上傳失敗'
          return
        }
        finalAvatar = uploadedPath
      }

      const { data: res, error } = await userApi.PUT('/api/user/profile', {
        body: {
          name: profileForm.name.trim(),
          avatar: finalAvatar,
          notification_type: (authStore.currentUser?.notification_type ?? 0) as NotificationType
        }
      })
      if (error || res?.code !== 0 || !res.data) {
        const e = (error ?? res) as { message?: string; errors?: Record<string, string[]> } | null
        if (e?.errors) profileFieldErrors.value = e.errors
        profileError.value = e?.message ?? '更新失敗'
        toast.error(e?.message ?? '更新失敗')
        return
      }
      authStore.currentUser = res.data
      avatar.reset()
      toast.success('個人資料已儲存')
    })())
  } finally {
    profileSubmitting.value = false
  }
}

function cancelAvatarPreview() {
  avatar.reset()
}

async function changePassword() {
  passwordError.value = null
  passwordFieldErrors.value = {}

  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    passwordFieldErrors.value = { confirm_password: ['兩次密碼輸入不一致'] }
    return
  }

  passwordSubmitting.value = true
  try {
    await withMinDelay(authStore.changePassword({
      oldPassword: passwordForm.oldPassword,
      newPassword: passwordForm.newPassword
    }))
    passwordForm.oldPassword = ''
    passwordForm.newPassword = ''
    passwordForm.confirmPassword = ''
    toast.success('密碼已變更')
  } catch (err: unknown) {
    const e = err as { message?: string; errors?: Record<string, string[]> } | null
    if (e?.errors) passwordFieldErrors.value = e.errors
    passwordError.value = e?.message ?? '變更密碼失敗'
    toast.error(e?.message ?? '變更密碼失敗')
  } finally {
    passwordSubmitting.value = false
  }
}

async function handleLogoutAll() {
  if (!confirm('確認要登出所有裝置（包含本機）嗎？')) return
  logoutAllSubmitting.value = true
  try {
    await withMinDelay(authStore.logoutAll())
    toast.success('已登出所有裝置')
    await navigateTo('/login')
  } finally {
    logoutAllSubmitting.value = false
  }
}

const prefDirty = computed(() => {
  const u = authStore.currentUser
  return prefForm.theme !== colorMode.preference
    || prefForm.notification !== ((u?.notification_type ?? 0) as NotificationType)
})

async function savePreferences() {
  prefSubmitting.value = true
  try {
    await withMinDelay((async () => {
      const themeChanged = prefForm.theme !== colorMode.preference
      const notifChanged = prefForm.notification !== ((authStore.currentUser?.notification_type ?? 0) as NotificationType)

      // 1. Apply theme locally（color-mode 持久化到 cookie）
      if (themeChanged) {
        colorMode.preference = prefForm.theme
      }

      // 2. PUT notification 只在有變動時打
      if (notifChanged) {
        const { data: res } = await userApi.PUT('/api/user/profile', {
          body: {
            name: authStore.currentUser?.name ?? profileForm.name,
            avatar: authStore.currentUser?.avatar ?? null,
            notification_type: prefForm.notification
          }
        })
        if (res?.code === 0 && res.data) {
          authStore.currentUser = res.data
        } else {
          toast.error('偏好設定更新失敗')
          return
        }
      }

      toast.success('偏好設定已儲存')
    })())
  } finally {
    prefSubmitting.value = false
  }
}

const themeOptions = [
  { label: 'Light', value: 'light', icon: Sun },
  { label: 'Dark', value: 'dark', icon: Moon },
  { label: 'System', value: 'system', icon: Monitor }
]
</script>

<template>
  <div class="max-w-3xl mx-auto space-y-12 md:space-y-16">
    <div class="flex items-center gap-4">
      <BackButton />
      <h1 class="text-4xl italic leading-none">Settings</h1>
    </div>

    <!-- PROFILE -->
      <section class="space-y-6">
        <FormLabel text="1. Profile" />

        <div class="brutalist-card p-6 md:p-8 bg-white dark:bg-dark-surface space-y-6">
          <!-- Avatar dropzone -->
          <div>
            <p class="text-[10px] font-black uppercase tracking-widest opacity-40 mb-3">頭像</p>

            <div class="flex flex-col md:flex-row gap-6 items-center md:items-start">
              <div class="w-32 h-32 border-2 border-black dark:border-white overflow-hidden bg-page-bg dark:bg-dark-bg shrink-0">
                <img v-if="currentAvatarSrc" :src="currentAvatarSrc" alt="avatar" class="w-full h-full object-cover">
              </div>

              <div class="flex-1 w-full">
                <div
                  :class="[
                    'border-2 border-dashed border-black dark:border-white p-6 text-center cursor-pointer transition-colors',
                    dragOver ? 'bg-brand text-white' : 'hover:bg-page-bg dark:hover:bg-white/5'
                  ]"
                  @dragover.prevent="dragOver = true"
                  @dragleave.prevent="dragOver = false"
                  @drop="onDrop"
                  @click="fileInput?.click()"
                >
                  <ImagePlus class="w-8 h-8 mx-auto mb-2 opacity-60" />
                  <p class="text-xs font-black uppercase tracking-widest">拖拉圖片或點擊選擇檔案</p>
                  <p class="text-[10px] font-mono uppercase tracking-widest opacity-40 mt-2">
                    JPG / PNG / WebP / GIF · 最大 10MB · 儲存時上傳並壓縮至 512px
                  </p>
                </div>
                <input ref="fileInput" type="file" accept="image/*" class="hidden" @change="onSelectFile">

                <p v-if="avatar.error.value" class="text-xs font-black text-red-500 mt-3 italic">
                  {{ avatar.error.value }}
                </p>

                <div v-if="avatar.stagedFile.value" class="flex gap-3 mt-3 items-center">
                  <p class="text-xs font-bold italic flex-1 truncate">
                    <Loader2 v-if="avatar.uploading.value" class="w-3 h-3 inline animate-spin mr-1" />
                    {{ avatar.uploading.value ? '上傳中…' : `預覽中（${avatar.stagedFile.value.name}），按「儲存」才會上傳` }}
                  </p>
                  <button
                    type="button"
                    class="brutalist-button px-3 py-1.5 text-xs flex items-center gap-1 shrink-0"
                    :disabled="avatar.uploading.value"
                    @click="cancelAvatarPreview"
                  >
                    <X class="w-3 h-3" />
                    取消
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Name -->
          <div>
            <FormLabel for="name" text="顯示名稱" />
            <input
              id="name"
              v-model="profileForm.name"
              required
              maxlength="100"
              class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-3 px-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
            >
            <p v-if="profileFieldErrors.name" class="text-xs font-black text-red-500 mt-2 italic">{{ profileFieldErrors.name[0] }}</p>
          </div>

          <p v-if="profileError" class="text-sm font-black italic text-red-500 border-l-4 border-red-500 pl-3 py-2">
            {{ profileError }}
          </p>

          <button
            type="button"
            class="w-full brutalist-button brutalist-button-primary py-4 text-base flex items-center justify-center gap-2 disabled:opacity-50"
            :disabled="profileSubmitting"
            @click="saveProfile"
          >
            <Save class="w-5 h-5" />
            <span>{{ profileSubmitting ? 'Saving…' : '儲存個人資料' }}</span>
          </button>
        </div>
      </section>

      <!-- ACCOUNT -->
      <section class="space-y-6">
        <FormLabel text="2. Account" />

        <div class="brutalist-card p-6 md:p-8 bg-white dark:bg-dark-surface space-y-6">
          <!-- Email read-only -->
          <div>
            <FormLabel text="Email" />
            <div class="flex items-center gap-3 bg-page-bg dark:bg-dark-bg border-2 border-black/20 dark:border-white/10 py-3 px-4">
              <AtSign class="w-4 h-4 opacity-50" />
              <span class="font-bold italic flex-1 truncate">{{ authStore.currentUser?.email ?? '—' }}</span>
              <span class="text-[10px] font-black uppercase tracking-widest opacity-40">Read only</span>
            </div>
          </div>

          <!-- Password change -->
          <div class="space-y-4 pt-4 border-t-2 border-black/10 dark:border-white/10">
            <FormLabel text="變更密碼" />
            <input
              v-model="passwordForm.oldPassword"
              type="password"
              autocomplete="current-password"
              placeholder="舊密碼"
              class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-3 px-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
            >
            <p v-if="passwordFieldErrors.old_password" class="text-xs font-black text-red-500 italic">{{ passwordFieldErrors.old_password[0] }}</p>
            <input
              v-model="passwordForm.newPassword"
              type="password"
              autocomplete="new-password"
              placeholder="新密碼（≥6 字元）"
              minlength="6"
              class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-3 px-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
            >
            <p v-if="passwordFieldErrors.new_password" class="text-xs font-black text-red-500 italic">{{ passwordFieldErrors.new_password[0] }}</p>
            <input
              v-model="passwordForm.confirmPassword"
              type="password"
              autocomplete="new-password"
              placeholder="確認新密碼"
              minlength="6"
              class="w-full bg-white dark:bg-dark-surface border-2 border-black dark:border-white/20 py-3 px-4 font-bold italic focus:outline-none shadow-brutalist dark:shadow-brutalist-dark"
            >
            <p v-if="passwordFieldErrors.confirm_password" class="text-xs font-black text-red-500 italic">{{ passwordFieldErrors.confirm_password[0] }}</p>
            <p v-if="passwordError && !passwordFieldErrors.old_password" class="text-sm font-black italic text-red-500 border-l-4 border-red-500 pl-3 py-2">
              {{ passwordError }}
            </p>

            <button
              type="button"
              class="w-full brutalist-button py-3 text-sm flex items-center justify-center gap-2 disabled:opacity-50"
              :disabled="passwordSubmitting || !passwordForm.oldPassword || !passwordForm.newPassword"
              @click="changePassword"
            >
              <Lock class="w-4 h-4" />
              <span>{{ passwordSubmitting ? 'Updating…' : '變更密碼' }}</span>
            </button>
            <p class="text-[10px] font-mono uppercase tracking-widest opacity-40 leading-relaxed">
              本機現有 session 不會被中斷；若需踢出其他裝置，請使用下方「登出所有裝置」
            </p>
          </div>

          <!-- Google connect -->
          <div class="pt-4 border-t-2 border-black/10 dark:border-white/10">
            <FormLabel text="Google 帳號連結" />
            <button
              type="button"
              disabled
              class="w-full brutalist-button py-3 text-sm flex items-center justify-center gap-2 opacity-50 cursor-not-allowed"
            >
              <span>{{ authStore.currentUser?.is_google_connected ? '已連結' : '連結 Google 帳號（即將推出）' }}</span>
            </button>
          </div>

          <!-- Logout all -->
          <div class="pt-4 border-t-2 border-black/10 dark:border-white/10">
            <FormLabel text="登出所有裝置" />
            <button
              type="button"
              class="w-full brutalist-button py-3 text-sm flex items-center justify-center gap-2 text-red-500 hover:bg-red-500 hover:text-white transition-colors disabled:opacity-50"
              :disabled="logoutAllSubmitting"
              @click="handleLogoutAll"
            >
              <LogOut class="w-4 h-4" />
              <span>{{ logoutAllSubmitting ? 'Signing out…' : '登出所有裝置' }}</span>
            </button>
          </div>
        </div>
      </section>

      <!-- PREFERENCES -->
      <section class="space-y-6">
        <FormLabel text="3. Preferences" />

        <div class="brutalist-card p-6 md:p-8 bg-white dark:bg-dark-surface space-y-6">
          <!-- Theme -->
          <div>
            <FormLabel text="主題" />
            <div class="grid grid-cols-3 gap-2">
              <button
                v-for="opt in themeOptions"
                :key="opt.value"
                type="button"
                :class="[
                  'brutalist-button py-3 text-xs flex items-center justify-center gap-2',
                  prefForm.theme === opt.value
                    ? 'bg-brand text-white border-brand'
                    : ''
                ]"
                @click="prefForm.theme = opt.value"
              >
                <component :is="opt.icon" class="w-4 h-4" />
                {{ opt.label }}
              </button>
            </div>
          </div>

          <!-- Notification -->
          <div class="pt-4 border-t-2 border-black/10 dark:border-white/10">
            <FormLabel text="通知" />
            <div class="space-y-2">
              <label
                v-for="(label, idx) in ['不接收', 'Web Push', 'Email', 'Web Push + Email']"
                :key="idx"
                :class="[
                  'flex items-center gap-3 brutalist-button py-3 px-4 text-sm cursor-pointer',
                  prefForm.notification === idx
                    ? 'bg-brand text-white border-brand'
                    : ''
                ]"
              >
                <input
                  type="radio"
                  :value="idx"
                  :checked="prefForm.notification === idx"
                  class="hidden"
                  @change="prefForm.notification = idx as NotificationType"
                >
                <Bell class="w-4 h-4" />
                <span class="italic font-bold">{{ label }}</span>
              </label>
            </div>
          </div>

          <button
            type="button"
            class="w-full brutalist-button brutalist-button-primary py-4 text-base flex items-center justify-center gap-2 disabled:opacity-50"
            :disabled="prefSubmitting || !prefDirty"
            @click="savePreferences"
          >
            <Save class="w-5 h-5" />
            <span>{{ prefSubmitting ? 'Saving…' : '儲存偏好設定' }}</span>
          </button>
        </div>
      </section>
  </div>
</template>
