let cachedBaseUrl: string | null = null
let inflight: Promise<string> | null = null

interface AssetHostResponse {
  data?: { base_url?: string }
  code?: number
  message?: string | null
}

/**
 * 取得圖片資產的 base URL，並提供 path → 完整 URL 的 helper。
 * 啟動後第一次呼叫會打 User.API /upload/asset-host，後續使用 module-level cache。
 */
export function useAssetHost() {
  const config = useRuntimeConfig()
  const apiBase = (config.public.apiBase as string).replace(/\/$/, '')

  async function fetchBaseUrl(): Promise<string> {
    if (cachedBaseUrl) return cachedBaseUrl
    if (inflight) return inflight

    inflight = (async () => {
      const headers: Record<string, string> = {}
      const token = useCookie('auth_token').value
      if (token) headers['Authorization'] = `Bearer ${token}`

      const res = await $fetch<AssetHostResponse>(`${apiBase}/user/upload/asset-host`, { headers })
      if (!res?.data?.base_url) {
        throw new Error('取得 asset host 失敗')
      }
      cachedBaseUrl = res.data.base_url
      return cachedBaseUrl
    })()

    try {
      return await inflight
    } finally {
      inflight = null
    }
  }

  function assetUrl(path: string): string {
    if (!cachedBaseUrl) return path
    const baseOrigin = cachedBaseUrl.replace(/\/assets\/?$/, '')
    return `${baseOrigin}${path}`
  }

  return {
    fetchBaseUrl,
    ensureBaseUrl: fetchBaseUrl,
    assetUrl,
  }
}
