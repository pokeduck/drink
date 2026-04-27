<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useLoading } from '~/composable/useLoading'
import { useUnsavedGuard } from '~/composable/useUnsavedGuard'
import { usePermission } from '~/composable/usePermission'
import { MENU } from '@app/core'
import type { components } from '@app/api-types/admin'

type SugarOverride = components['schemas']['ShopSugarOverrideDetailResponse']
type ToppingOverride = components['schemas']['ShopToppingOverrideDetailResponse']

const route = useRoute()
const shopId = computed(() => Number(route.params.id))

const api = useAdminApi()
const { handleError, showSuccess, startLoading, stopLoading } = useApiFeedback()
const { loading, start: startListLoading, stop: stopListLoading } = useLoading()
const { can } = usePermission()

const sugarOverrides = ref<SugarOverride[]>([])
const toppingOverrides = ref<ToppingOverride[]>([])

const formState = reactive({ sugarOverrides, toppingOverrides })
const { takeSnapshot } = useUnsavedGuard(formState)

async function fetchOverrides() {
  startListLoading()
  try {
    const { data: res, error } = await api.GET('/api/admin/shops/{shopId}/overrides', {
      params: { path: { shopId: shopId.value } },
    })
    if (error) {
      handleError(error, '載入覆寫設定失敗')
      return
    }
    sugarOverrides.value = res?.data?.sugar_overrides ?? []
    toppingOverrides.value = res?.data?.topping_overrides ?? []
    takeSnapshot()
  } finally {
    await stopListLoading()
  }
}

async function handleSave() {
  startLoading()

  const sugarItems = sugarOverrides.value
    .filter(s => s.override_price !== null && s.override_price !== undefined)
    .map(s => ({
      sugar_id: s.sugar_id!,
      price: s.override_price ?? undefined,
    }))

  const toppingItems = toppingOverrides.value
    .filter(t => t.override_price !== null && t.override_price !== undefined)
    .map(t => ({
      topping_id: t.topping_id!,
      price: t.override_price ?? undefined,
    }))

  const { error } = await api.PUT('/api/admin/shops/{shopId}/overrides', {
    params: { path: { shopId: shopId.value } },
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
  fetchOverrides()
})
</script>

<template>
  <div v-loading="loading">
    <h3 style="margin: 0 0 16px 0; font-weight: 600">覆寫設定</h3>

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
    </el-table>

    <div style="margin-top: 16px">
      <el-button v-if="can(MENU.ShopOverride, 'update')" type="primary" @click="handleSave">儲存</el-button>
    </div>
  </div>
</template>
