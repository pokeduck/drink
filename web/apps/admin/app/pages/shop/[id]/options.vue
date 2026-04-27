<script setup lang="ts">
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useLoading } from '~/composable/useLoading'
import { useUnsavedGuard } from '~/composable/useUnsavedGuard'
import { usePermission } from '~/composable/usePermission'
import { useShopOptionsApi } from '~/composable/useShopOptionsApi'
import { MENU } from '@app/core'

const route = useRoute()
const shopId = computed(() => Number(route.params.id))
const { showSuccess, handleError } = useApiFeedback()
const { loading, start: startLoading, stop: stopLoading } = useLoading()
const { can } = usePermission()
const { getOptions, previewOptions, updateOptions } = useShopOptionsApi()

interface Row {
  id: number
  name: string
  defaultPrice?: number | null
  isEnabled: boolean
  sort: number
}

const sugars = ref<Row[]>([])
const ices = ref<Row[]>([])
const toppings = ref<Row[]>([])
const sizes = ref<Row[]>([])
const activeTab = ref('sugars')

const formState = reactive({ sugars, ices, toppings, sizes })
const { takeSnapshot } = useUnsavedGuard(formState)

async function fetchOptions() {
  startLoading()
  try {
    const { data: res, error } = await getOptions(shopId.value)
    if (error) { handleError(error, '載入選項失敗'); return }
    const data = res?.data
    sugars.value = (data?.sugars ?? []).map(s => ({
      id: s.sugar_id!, name: s.sugar_name!, defaultPrice: s.default_price ?? 0,
      isEnabled: s.is_enabled ?? false, sort: s.sort ?? 0,
    }))
    ices.value = (data?.ices ?? []).map(s => ({
      id: s.ice_id!, name: s.ice_name!, isEnabled: s.is_enabled ?? false, sort: s.sort ?? 0,
    }))
    toppings.value = (data?.toppings ?? []).map(s => ({
      id: s.topping_id!, name: s.topping_name!, defaultPrice: s.default_price ?? 0,
      isEnabled: s.is_enabled ?? false, sort: s.sort ?? 0,
    }))
    sizes.value = (data?.sizes ?? []).map(s => ({
      id: s.size_id!, name: s.size_name!, isEnabled: s.is_enabled ?? false, sort: s.sort ?? 0,
    }))
    takeSnapshot()
  } finally {
    await stopLoading()
  }
}

function buildRequestBody() {
  return {
    sugars: sugars.value.filter(r => r.isEnabled).map(r => ({ sugar_id: r.id, sort: r.sort })),
    ices: ices.value.filter(r => r.isEnabled).map(r => ({ ice_id: r.id, sort: r.sort })),
    toppings: toppings.value.filter(r => r.isEnabled).map(r => ({ topping_id: r.id, sort: r.sort })),
    sizes: sizes.value.filter(r => r.isEnabled).map(r => ({ size_id: r.id, sort: r.sort })),
  }
}

async function handleSave() {
  if (!can(MENU.ShopOptions, 'update')) return

  startLoading()
  const body = buildRequestBody()

  // Step 1: dry-run preview
  const { data: previewRes, error: previewErr } = await previewOptions(shopId.value, body)
  if (previewErr) {
    await stopLoading()
    handleError(previewErr, '預覽失敗')
    return
  }

  const preview = previewRes?.data
  const affected = preview?.affected_menu_items ?? []
  const count = preview?.affected_menu_items_count ?? 0

  // Step 2: confirmation if there's impact
  if (count > 0) {
    await stopLoading()
    try {
      const itemList = affected.slice(0, 10)
        .map(it => `<li>${it.name}（移除 ${(it.removed_options?.sugars?.length ?? 0)
          + (it.removed_options?.ices?.length ?? 0)
          + (it.removed_options?.toppings?.length ?? 0)
          + (it.removed_options?.sizes?.length ?? 0)} 個選項）</li>`)
        .join('')
      const more = affected.length > 10 ? `<li>...還有 ${affected.length - 10} 個品項</li>` : ''
      await ElMessageBox.confirm(
        `<div>本次儲存會連帶移除 ${count} 個品項中的選項：</div><ul style="margin: 8px 0; padding-left: 20px">${itemList}${more}</ul><div>確定要儲存嗎？</div>`,
        '確認儲存',
        { type: 'warning', dangerouslyUseHTMLString: true, confirmButtonText: '儲存', cancelButtonText: '取消' },
      )
    } catch (err: any) {
      if (err === 'cancel') return
      throw err
    }
    startLoading()
  }

  // Step 3: actual update
  const { data: updateRes, error: updateErr } = await updateOptions(shopId.value, body)
  await stopLoading()
  if (updateErr) {
    handleError(updateErr, '儲存失敗')
    return
  }
  const actualCount = updateRes?.data?.affected_menu_items_count ?? 0
  showSuccess(actualCount > 0
    ? `儲存成功，實際移除 ${actualCount} 個品項中的選項`
    : '儲存成功')
  takeSnapshot()
}

onMounted(() => {
  fetchOptions()
})
</script>

<template>
  <div v-loading="loading">
    <h3 style="margin: 0 0 16px 0; font-weight: 600">選項啟用</h3>

    <el-tabs v-model="activeTab">
      <el-tab-pane label="糖度" name="sugars">
        <ShopOptionTab v-model="sugars" :rows="sugars" :has-price="true" />
      </el-tab-pane>
      <el-tab-pane label="冰塊" name="ices">
        <ShopOptionTab v-model="ices" :rows="ices" />
      </el-tab-pane>
      <el-tab-pane label="加料" name="toppings">
        <ShopOptionTab v-model="toppings" :rows="toppings" :has-price="true" />
      </el-tab-pane>
      <el-tab-pane label="尺寸" name="sizes">
        <ShopOptionTab v-model="sizes" :rows="sizes" />
      </el-tab-pane>
    </el-tabs>

    <div style="margin-top: 16px">
      <el-button v-if="can(MENU.ShopOptions, 'update')" type="primary" @click="handleSave">儲存</el-button>
    </div>
  </div>
</template>
