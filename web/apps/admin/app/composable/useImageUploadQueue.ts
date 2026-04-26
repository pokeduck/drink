import { computed, reactive, ref } from 'vue'
import { useAuthStore } from '~/stores/auth'

export type UploadStatus = 'pending' | 'uploading' | 'done' | 'error'

export interface UploadItem<TResult = UploadResult> {
  id: string
  file: File
  status: UploadStatus
  progress: number
  result?: TResult
  error?: string
}

export interface UploadResult {
  path: string
  hash: string
  size: number
  width: number
  height: number
  mime_type: string
}

interface UseImageUploadQueueOptions {
  concurrency?: number
  /** 上傳端點，相對於 apiBase（如 `/admin/upload`、`/admin/shops/1/images`）。預設 `/admin/upload`。 */
  uploadEndpoint?: string
  /** 上傳時附加的 query string，例如 `{ drink_item_id: 12 }` */
  query?: Record<string, string | number | undefined>
}

/**
 * Admin 圖片佇列上傳：一次一張慢慢傳（concurrency=1）。
 * Server 端永遠單檔，因此前端用 queue 控制節奏。
 */
export function useImageUploadQueue<TResult = UploadResult>(options: UseImageUploadQueueOptions = {}) {
  const concurrency = options.concurrency ?? 1
  const endpoint = options.uploadEndpoint ?? '/admin/upload'
  const query = options.query ?? {}
  const uploads = ref<UploadItem<TResult>[]>([])
  const authStore = useAuthStore()
  const config = useRuntimeConfig()
  const apiBase = (config.public.apiBase as string).replace(/\/$/, '')

  const progress = computed(() => {
    const total = uploads.value.length
    const done = uploads.value.filter((u) => u.status === 'done').length
    const failed = uploads.value.filter((u) => u.status === 'error').length
    return { done, failed, total }
  })

  let active = 0
  const queue: UploadItem<TResult>[] = []

  function enqueue(files: File[]) {
    for (const file of files) {
      const item: UploadItem<TResult> = reactive({
        id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
        file,
        status: 'pending' as UploadStatus,
        progress: 0,
      }) as UploadItem<TResult>
      uploads.value.push(item)
      queue.push(item)
    }
    pump()
  }

  function retry(item: UploadItem<TResult>) {
    if (item.status !== 'error') return
    item.status = 'pending'
    item.error = undefined
    item.progress = 0
    queue.push(item)
    pump()
  }

  function clear() {
    uploads.value = []
    queue.length = 0
  }

  function pump() {
    while (active < concurrency && queue.length > 0) {
      const next = queue.shift()!
      void run(next)
    }
  }

  async function run(item: UploadItem<TResult>) {
    active++
    item.status = 'uploading'
    try {
      const result = await uploadOne(item)
      item.result = result
      item.progress = 100
      item.status = 'done'
    }
    catch (err: any) {
      item.error = err?.message || '上傳失敗'
      item.status = 'error'
    }
    finally {
      active--
      pump()
    }
  }

  function buildUrl(): string {
    const ep = endpoint.startsWith('/') ? endpoint : `/${endpoint}`
    const qs = Object.entries(query)
      .filter(([, v]) => v !== undefined && v !== null && v !== '')
      .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`)
      .join('&')
    return `${apiBase}${ep}${qs ? `?${qs}` : ''}`
  }

  function uploadOne(item: UploadItem<TResult>): Promise<TResult> {
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest()
      const form = new FormData()
      form.append('file', item.file)

      xhr.open('POST', buildUrl())
      if (authStore.accessToken) {
        xhr.setRequestHeader('Authorization', `Bearer ${authStore.accessToken}`)
      }

      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable) {
          item.progress = Math.round((e.loaded / e.total) * 100)
        }
      }

      xhr.onload = () => {
        try {
          const body = JSON.parse(xhr.responseText || '{}')
          if (xhr.status >= 200 && xhr.status < 300 && body.code === 0 && body.data) {
            resolve(body.data as TResult)
          }
          else {
            reject(new Error(body.message || `HTTP ${xhr.status}`))
          }
        }
        catch {
          reject(new Error(`Invalid response (HTTP ${xhr.status})`))
        }
      }

      xhr.onerror = () => reject(new Error('網路錯誤'))
      xhr.onabort = () => reject(new Error('已取消'))
      xhr.send(form)
    })
  }

  return {
    uploads,
    progress,
    enqueue,
    retry,
    clear,
  }
}
