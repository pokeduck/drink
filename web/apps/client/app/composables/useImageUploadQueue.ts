import { computed, reactive, ref } from 'vue'

export type UploadStatus = 'pending' | 'uploading' | 'done' | 'error'

export interface UploadItem {
  id: string
  file: File
  status: UploadStatus
  progress: number
  result?: UploadResult
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
}

/**
 * Client 圖片佇列上傳：一次一張慢慢傳（concurrency=1）。
 * Server 端永遠單檔，因此前端用 queue 控制節奏。
 */
export function useImageUploadQueue(options: UseImageUploadQueueOptions = {}) {
  const concurrency = options.concurrency ?? 1
  const uploads = ref<UploadItem[]>([])
  const config = useRuntimeConfig()
  const apiBase = (config.public.apiBase as string).replace(/\/$/, '')

  const progress = computed(() => {
    const total = uploads.value.length
    const done = uploads.value.filter((u) => u.status === 'done').length
    const failed = uploads.value.filter((u) => u.status === 'error').length
    return { done, failed, total }
  })

  let active = 0
  const queue: UploadItem[] = []

  function enqueue(files: File[]) {
    for (const file of files) {
      const item: UploadItem = reactive({
        id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
        file,
        status: 'pending',
        progress: 0,
      })
      uploads.value.push(item)
      queue.push(item)
    }
    pump()
  }

  function retry(item: UploadItem) {
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

  async function run(item: UploadItem) {
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

  function uploadOne(item: UploadItem): Promise<UploadResult> {
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest()
      const form = new FormData()
      form.append('file', item.file)

      xhr.open('POST', `${apiBase}/user/upload`)
      const token = useCookie('auth_token').value
      if (token) {
        xhr.setRequestHeader('Authorization', `Bearer ${token}`)
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
            resolve(body.data as UploadResult)
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
