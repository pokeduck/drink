import { useAuthStore } from '~/stores/auth'

let cachedBaseUrl: string | null = null
let inflight: Promise<string> | null = null

interface AssetHostResponse {
  data?: { base_url?: string }
  code?: number
  message?: string | null
}

/**
 * 取得圖片資產的 base URL，並提供 path → 完整 URL 的 helper。
 * 啟動後第一次呼叫會打 Admin.API /upload/asset-host，後續使用 module-level cache。
 *
 * 用 $fetch 而非 useAdminApi（openapi-fetch）以避免相依於尚未產生的 admin.d.ts 型別。
 */
export function useAssetHost() {
  const config = useRuntimeConfig()
  const apiBase = (config.public.apiBase as string).replace(/\/$/, '')

  async function fetchBaseUrl(): Promise<string> {
    if (cachedBaseUrl) return cachedBaseUrl
    if (inflight) return inflight

    inflight = (async () => {
      const authStore = useAuthStore()
      const headers: Record<string, string> = {}
      if (authStore.accessToken) {
        headers['Authorization'] = `Bearer ${authStore.accessToken}`
      }

      const res = await $fetch<AssetHostResponse>(`${apiBase}/admin/upload/asset-host`, { headers })
      if (!res?.data?.base_url) {
        throw new Error('取得 asset host 失敗')
      }
      cachedBaseUrl = res.data.base_url
      return cachedBaseUrl
    })()

    try {
      return await inflight
    }
    finally {
      inflight = null
    }
  }

  /**
   * 把後端回傳的相對 path（如 `/assets/images/ab/abc.webp`）拼成完整 URL。
   * 必須先呼叫 fetchBaseUrl 一次，否則 fallback 為原 path（dev 同 origin 也能 work）。
   */
  function assetUrl(path: string): string {
    if (!cachedBaseUrl) return path
    // base_url 是 ".../assets"；後端回傳的 path 是 "/assets/..."
    // 直接拼會重複 → 用 base_url 去掉 trailing /assets
    const baseOrigin = cachedBaseUrl.replace(/\/assets\/?$/, '')
    return `${baseOrigin}${path}`
  }

  return {
    fetchBaseUrl,
    ensureBaseUrl: fetchBaseUrl,
    assetUrl,
  }
}
