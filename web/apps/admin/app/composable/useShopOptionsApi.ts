import { useAdminApi } from '~/composable/useAdminApi'
import type { components } from '@app/api-types/admin'

export type ShopOptionsResponse = components['schemas']['ShopOptionsResponse']
export type ShopOptionsPreviewResponse = components['schemas']['ShopOptionsPreviewResponse']
export type UpdateShopOptionsRequest = components['schemas']['UpdateShopOptionsRequest']
export type UpdateShopOptionsResponse = components['schemas']['UpdateShopOptionsResponse']

export function useShopOptionsApi() {
  const api = useAdminApi()

  const getOptions = (shopId: number) =>
    api.GET('/api/admin/shops/{shopId}/options', { params: { path: { shopId } } })

  const previewOptions = (shopId: number, body: UpdateShopOptionsRequest) =>
    api.POST('/api/admin/shops/{shopId}/options/preview', {
      params: { path: { shopId } },
      body,
    })

  const updateOptions = (shopId: number, body: UpdateShopOptionsRequest) =>
    api.PUT('/api/admin/shops/{shopId}/options', {
      params: { path: { shopId } },
      body,
    })

  return { getOptions, previewOptions, updateOptions }
}
