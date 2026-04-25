<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useLoading } from '~/composable/useLoading'
import { usePermission } from '~/composable/usePermission'
import { MENU } from '@app/core'
import type { components } from '@app/api-types/admin'

type Shop = components['schemas']['ShopListResponse']
type SugarOverride = components['schemas']['ShopSugarOverrideDetailResponse']
type ToppingOverride = components['schemas']['ShopToppingOverrideDetailResponse']

const api = useAdminApi()
const router = useRouter()
const { handleError, showSuccess, startLoading, stopLoading } = useApiFeedback()
const { can } = usePermission()

// 店家選擇
const shops = ref<Shop[]>([])
const selectedShopId = ref<number | null>(null)
const { loading: shopLoading, start: startShopLoading, stop: stopShopLoading } = useLoading()

// 覆寫資料
const sugarOverrides = ref<SugarOverride[]>([])
const toppingOverrides = ref<ToppingOverride[]>([])
const { loading: overrideLoading, start: startOverrideLoading, stop: stopOverrideLoading } = useLoading()

const fetchShops = async () => {
  startShopLoading()
  try {
    const { data: res } = await api.GET('/api/admin/shops', {
      params: { query: { page: 1, page_size: 999 } },
    })
    shops.value = res?.data?.items ?? []
  } finally {
    stopShopLoading()
  }
}

const fetchOverrides = async () => {
  if (!selectedShopId.value) return
  startOverrideLoading()
  try {
    const { data: res, error } = await api.GET('/api/admin/shops/{shopId}/overrides', {
      params: { path: { shopId: selectedShopId.value } },
    })
    if (error) {
      handleError(error, '載入覆寫設定失敗')
      return
    }
    sugarOverrides.value = res?.data?.sugar_overrides ?? []
    toppingOverrides.value = res?.data?.topping_overrides ?? []
  } finally {
    stopOverrideLoading()
  }
}

const handleShopChange = () => {
  if (selectedShopId.value) fetchOverrides()
  else {
    sugarOverrides.value = []
    toppingOverrides.value = []
  }
}

const handleSave = async () => {
  if (!selectedShopId.value) return
  startLoading()

  const sugarItems = sugarOverrides.value
    .filter(s => s.override_price !== null || s.override_sort !== null)
    .map(s => ({
      sugar_id: s.sugar_id!,
      price: s.override_price ?? undefined,
      sort: s.override_sort ?? undefined,
    }))

  const toppingItems = toppingOverrides.value
    .filter(t => t.override_price !== null || t.override_sort !== null)
    .map(t => ({
      topping_id: t.topping_id!,
      price: t.override_price ?? undefined,
      sort: t.override_sort ?? undefined,
    }))

  const { error } = await api.PUT('/api/admin/shops/{shopId}/overrides', {
    params: { path: { shopId: selectedShopId.value } },
    body: {
      sugar_overrides: sugarItems,
      topping_overrides: toppingItems,
    },
  })
  await stopLoading()

  if (error) {
    handleError(error, '儲存失敗')
    return
  }
  showSuccess('覆寫設定儲存成功')
  await fetchOverrides()
}

onMounted(() => {
  fetchShops()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never" v-loading="shopLoading">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <div style="display: flex; align-items: center; gap: 8px">
            <el-button text @click="router.push('/shop/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
            <span>店家覆寫設定</span>
          </div>
          <el-button
            v-if="can(MENU.ShopOverride, 'update') && selectedShopId"
            type="primary"
            @click="handleSave"
          >
            儲存覆寫設定
          </el-button>
        </div>
      </template>

      <!-- 店家選擇 -->
      <div style="margin-bottom: 20px">
        <el-select
          v-model="selectedShopId"
          placeholder="請選擇店家"
          filterable
          clearable
          style="width: 300px"
          @change="handleShopChange"
        >
          <el-option
            v-for="shop in shops"
            :key="shop.id"
            :label="shop.name"
            :value="shop.id"
          />
        </el-select>
      </div>

      <template v-if="selectedShopId">
        <div v-loading="overrideLoading">
          <!-- 甜度覆寫 -->
          <h4 style="margin-bottom: 12px">甜度覆寫</h4>
          <el-table :data="sugarOverrides" stripe style="width: 100%; margin-bottom: 24px">
            <el-table-column prop="sugar_name" label="名稱" min-width="150" />
            <el-table-column label="價格" width="280">
              <template #header>
                價格 <span style="color: var(--el-text-color-placeholder); font-weight: normal">（留空 = 全域）</span>
              </template>
              <template #default="{ row }">
                <el-input-number
                  v-model="row.override_price"
                  :min="0"
                  :precision="0"
                  :placeholder="`全域 ${row.default_price}`"

                  style="width: 240px"
                />
              </template>
            </el-table-column>
            <el-table-column label="排序" width="220">
              <template #header>
                排序 <span style="color: var(--el-text-color-placeholder); font-weight: normal">（留空 = 全域）</span>
              </template>
              <template #default="{ row }">
                <el-input-number
                  v-model="row.override_sort"
                  :min="0"
                  :precision="0"
                  :placeholder="`全域 ${row.default_sort}`"

                  style="width: 180px"
                />
              </template>
            </el-table-column>
          </el-table>

          <!-- 加料覆寫 -->
          <h4 style="margin-bottom: 12px">加料覆寫</h4>
          <el-table :data="toppingOverrides" stripe style="width: 100%">
            <el-table-column prop="topping_name" label="名稱" min-width="150" />
            <el-table-column label="價格" width="280">
              <template #header>
                價格 <span style="color: var(--el-text-color-placeholder); font-weight: normal">（留空 = 全域）</span>
              </template>
              <template #default="{ row }">
                <el-input-number
                  v-model="row.override_price"
                  :min="0"
                  :precision="0"
                  :placeholder="`全域 ${row.default_price}`"

                  style="width: 240px"
                />
              </template>
            </el-table-column>
            <el-table-column label="排序" width="220">
              <template #header>
                排序 <span style="color: var(--el-text-color-placeholder); font-weight: normal">（留空 = 全域）</span>
              </template>
              <template #default="{ row }">
                <el-input-number
                  v-model="row.override_sort"
                  :min="0"
                  :precision="0"
                  :placeholder="`全域 ${row.default_sort}`"

                  style="width: 180px"
                />
              </template>
            </el-table-column>
          </el-table>
        </div>
      </template>

      <el-result v-else icon="info" title="" sub-title="請先選擇店家" />
    </el-card>
  </div>
</template>
