import { useAuthStore } from '~/stores/auth'

const ALLOWED_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.gif', '.webp']
const MAX_BYTES = 10 * 1024 * 1024

export function useAvatarUpload() {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()

  const stagedFile = ref<File | null>(null)
  const previewUrl = ref<string | null>(null)
  const uploading = ref(false)
  const error = ref<string | null>(null)

  function reset() {
    stagedFile.value = null
    if (previewUrl.value) {
      URL.revokeObjectURL(previewUrl.value)
      previewUrl.value = null
    }
    uploading.value = false
    error.value = null
  }

  function stageFile(file: File) {
    error.value = null

    const ext = '.' + (file.name.split('.').pop() ?? '').toLowerCase()
    if (!ALLOWED_EXTENSIONS.includes(ext)) {
      error.value = `不支援的檔案格式（${ext}）`
      return
    }
    if (file.size > MAX_BYTES) {
      error.value = '檔案超過 10MB 上限'
      return
    }

    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
    stagedFile.value = file
    previewUrl.value = URL.createObjectURL(file)
  }

  /**
   * 真正上傳：傳完回傳 server 端 path（要寫進 User.Avatar）。
   * 沒有 staged file 時回 null（呼叫端應跳過上傳改沿用既有 avatar）。
   */
  async function commit(): Promise<string | null> {
    if (!stagedFile.value) return null

    uploading.value = true
    error.value = null
    try {
      const baseUrl = (config.public.apiBase as string).replace(/\/api\/?$/, '')
      const formData = new FormData()
      formData.append('file', stagedFile.value)

      const res = await fetch(`${baseUrl}/api/user/upload/avatar`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${authStore.accessToken}` },
        body: formData
      })

      const json = await res.json()
      if (!res.ok || json?.code !== 0 || !json?.data) {
        error.value = json?.message ?? '上傳失敗'
        return null
      }

      return json.data.path as string
    } catch (e: unknown) {
      error.value = (e as Error)?.message ?? '上傳失敗'
      return null
    } finally {
      uploading.value = false
    }
  }

  return {
    stagedFile,
    previewUrl,
    uploading,
    error,
    stageFile,
    commit,
    reset
  }
}
