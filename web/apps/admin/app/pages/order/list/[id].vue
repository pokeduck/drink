<script setup lang="ts">
import { ArrowLeft } from '@element-plus/icons-vue'
import { formatDateTime } from '~/utils/format'
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useLoading } from '~/composable/useLoading'
import { useAuthStore } from '~/stores/auth'
import type { components } from '@app/api-types/admin'

type AdminOrderDetail = components['schemas']['AdminOrderDetailResponse']

const route = useRoute()
const router = useRouter()
const api = useAdminApi()
const authStore = useAuthStore()
const config = useRuntimeConfig()
const { handleError, showSuccess, startLoading, stopLoading } = useApiFeedback()
const { loading, start: startPageLoading, stop: stopPageLoading } = useLoading()

const orderId = computed(() => Number(route.params.id))

const detail = ref<AdminOrderDetail | null>(null)
const notFound = ref(false)
const statusDialogVisible = ref(false)
const notifying = ref(false)

const TERMINAL_STATUSES = [4, 5]
const CANCELLABLE_STATUSES = [1, 2]

const isTerminal = computed(() => detail.value && TERMINAL_STATUSES.includes(detail.value.status ?? 0))
const isCancellable = computed(() => detail.value && CANCELLABLE_STATUSES.includes(detail.value.status ?? 0))

const fetchDetail = async () => {
  startPageLoading()
  notFound.value = false
  try {
    const { data: res, error, response } = await api.GET('/api/admin/orders/{orderId}', {
      params: { path: { orderId: orderId.value } },
    })
    if (response?.status === 404 || (error && (error as any)?.error === 'ORDER_NOT_FOUND')) {
      notFound.value = true
      detail.value = null
      return
    }
    if (error) {
      handleError(error, '載入訂單失敗')
      return
    }
    detail.value = res?.data ?? null
  } catch (err) {
    handleError(err, '載入訂單失敗')
  } finally {
    stopPageLoading()
  }
}

const handleStatusChangeSuccess = () => {
  fetchDetail()
}

const handleCancel = async () => {
  if (!detail.value) return
  try {
    await ElMessageBox.confirm(`確定要取消訂單「${detail.value.title}」嗎?此動作不可逆。`, '取消訂單確認', {
      type: 'warning',
      confirmButtonText: '確定取消',
      cancelButtonText: '返回',
    })
    startLoading()
    const { error } = await api.PUT('/api/admin/orders/{orderId}/cancel', {
      params: { path: { orderId: orderId.value } },
    })
    await stopLoading()
    if (error) {
      handleError(error, '取消訂單失敗')
      return
    }
    showSuccess('訂單已取消')
    await fetchDetail()
  } catch (err: any) {
    if (err !== 'cancel') {
      handleError(err, '取消訂單失敗')
    }
  }
}

const handleExport = async () => {
  if (!detail.value) return
  startLoading()
  try {
    const baseUrl = (config.public.apiBase as string).replace(/\/api\/?$/, '')
    const res = await fetch(`${baseUrl}/api/admin/orders/${orderId.value}/export`, {
      headers: { Authorization: `Bearer ${authStore.accessToken ?? ''}` },
    })
    if (!res.ok) {
      await stopLoading()
      let msg = '匯出失敗'
      try {
        const errBody = await res.json()
        msg = errBody?.message ?? msg
      } catch {
        // binary or non-json error
      }
      handleError({ data: { message: msg } }, '匯出失敗')
      return
    }
    const disposition = res.headers.get('Content-Disposition') ?? ''
    const blob = await res.blob()
    const fileName = parseFileName(disposition) ?? defaultFileName(orderId.value)

    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = fileName
    document.body.appendChild(anchor)
    anchor.click()
    document.body.removeChild(anchor)
    URL.revokeObjectURL(url)
    await stopLoading()
    showSuccess('匯出成功')
  } catch (err) {
    await stopLoading()
    handleError(err, '匯出失敗')
  }
}

const handleNotify = async () => {
  if (!detail.value || notifying.value) return
  notifying.value = true
  startLoading()
  try {
    const { data: res, error } = await api.POST('/api/admin/orders/{orderId}/notify', {
      params: { path: { orderId: orderId.value } },
    })
    await stopLoading()
    if (error) {
      handleError(error, '發送通知失敗')
      return
    }
    const stats = res?.data
    ElNotification({
      type: 'success',
      title: '通知已發送',
      message: `已通知 ${stats?.total_recipients ?? 0} 人 (Email: ${stats?.email_sent ?? 0}, Push 略過: ${stats?.push_skipped ?? 0}, 未訂閱: ${stats?.none_skipped ?? 0}, 失敗: ${stats?.failed ?? 0})`,
      position: 'top-right',
      duration: 6000,
      offset: 76,
    })
  } catch (err) {
    await stopLoading()
    handleError(err, '發送通知失敗')
  } finally {
    notifying.value = false
  }
}

const parseFileName = (disposition: string): string | null => {
  if (!disposition) return null
  const utfMatch = disposition.match(/filename\*=(?:UTF-8'')?([^;]+)/i)
  if (utfMatch?.[1]) {
    try {
      return decodeURIComponent(utfMatch[1].trim().replace(/^"|"$/g, ''))
    } catch {
      // fallthrough
    }
  }
  const asciiMatch = disposition.match(/filename="?([^";]+)"?/i)
  if (asciiMatch?.[1]) return asciiMatch[1].trim()
  return null
}

const defaultFileName = (id: number): string => {
  const now = new Date()
  const y = now.getFullYear()
  const m = String(now.getMonth() + 1).padStart(2, '0')
  const d = String(now.getDate()).padStart(2, '0')
  return `order_${id}_${y}${m}${d}.xlsx`
}

const formatAmount = (value: number | undefined) => {
  if (value == null) return '$ 0'
  return `$ ${value.toFixed(0)}`
}

const toppingsLabel = (toppings: { topping_name?: string | null; price?: number }[] | null | undefined) => {
  if (!toppings || toppings.length === 0) return '—'
  return toppings.map((t) => `${t.topping_name ?? ''} +$${(t.price ?? 0).toFixed(0)}`).join('、')
}

onMounted(() => {
  fetchDetail()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card v-if="notFound" shadow="never">
      <el-empty description="找不到此訂單">
        <el-button type="primary" @click="router.push('/order/list')">返回列表</el-button>
      </el-empty>
    </el-card>

    <template v-else>
      <el-card shadow="never" v-loading="loading">
        <template #header>
          <div style="display: flex; justify-content: space-between; align-items: center">
            <div style="display: flex; align-items: center; gap: 8px">
              <el-button text @click="router.push('/order/list')">
                <el-icon><ArrowLeft /></el-icon>返回
              </el-button>
              <span>訂單詳情 #{{ orderId }}</span>
            </div>
            <AppTimestamp
              v-if="detail?.created_at && detail?.updated_at"
              :created-at="detail.created_at"
              :updated-at="detail.updated_at"
            />
          </div>
        </template>

        <!-- 訂單基本資訊 -->
        <el-descriptions :column="2" border>
          <el-descriptions-item label="標題">{{ detail?.title ?? '—' }}</el-descriptions-item>
          <el-descriptions-item label="店家">{{ detail?.shop_name ?? '—' }}</el-descriptions-item>
          <el-descriptions-item label="發起人">{{ detail?.initiator_name ?? '—' }}</el-descriptions-item>
          <el-descriptions-item label="狀態">
            <OrderStatusTag :status="detail?.status" />
          </el-descriptions-item>
          <el-descriptions-item label="截止時間">
            {{ detail?.deadline ? formatDateTime(detail.deadline) : '—' }}
          </el-descriptions-item>
          <el-descriptions-item label="備註">{{ detail?.note || '—' }}</el-descriptions-item>
        </el-descriptions>

        <!-- Summary -->
        <div class="summary-grid">
          <div class="summary-card">
            <div class="summary-label">飲料數</div>
            <div class="summary-value">{{ detail?.summary?.total_items ?? 0 }}</div>
          </div>
          <div class="summary-card">
            <div class="summary-label">總金額</div>
            <div class="summary-value">{{ formatAmount(detail?.summary?.total_amount) }}</div>
          </div>
          <div class="summary-card">
            <div class="summary-label">收件人數</div>
            <div class="summary-value">{{ detail?.summary?.recipient_count ?? 0 }}</div>
          </div>
        </div>

        <!-- 操作區 -->
        <div class="actions">
          <el-tooltip content="終態無法變更" :disabled="!isTerminal">
            <span>
              <el-button type="primary" :disabled="isTerminal || !detail" @click="statusDialogVisible = true">
                變更狀態
              </el-button>
            </span>
          </el-tooltip>
          <el-button v-if="isCancellable" type="danger" @click="handleCancel">取消訂單</el-button>
          <el-button :disabled="!detail" @click="handleExport">匯出 Excel</el-button>
          <el-button :disabled="!detail || notifying" @click="handleNotify">發送通知</el-button>
        </div>
      </el-card>

      <!-- 飲料明細 -->
      <el-card shadow="never" style="margin-top: 16px" v-loading="loading">
        <template #header>飲料明細</template>
        <el-table :data="detail?.order_items ?? []" stripe style="width: 100%">
          <el-table-column prop="recipient_name" label="收件人" width="120" show-overflow-tooltip />
          <el-table-column prop="user_name" label="填單人" width="120" show-overflow-tooltip />
          <el-table-column prop="menu_item_name" label="品項" min-width="140" show-overflow-tooltip />
          <el-table-column prop="size_name" label="尺寸" width="90" />
          <el-table-column prop="sugar_name" label="甜度" width="90" />
          <el-table-column prop="ice_name" label="冰塊" width="90" />
          <el-table-column label="加料" min-width="180">
            <template #default="{ row }">
              <span>{{ toppingsLabel(row.toppings) }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="item_price" label="品項價" width="100" align="right">
            <template #default="{ row }">{{ formatAmount(row.item_price) }}</template>
          </el-table-column>
          <el-table-column prop="sugar_price" label="甜度價" width="100" align="right">
            <template #default="{ row }">{{ formatAmount(row.sugar_price) }}</template>
          </el-table-column>
          <el-table-column prop="topping_price" label="加料價" width="100" align="right">
            <template #default="{ row }">{{ formatAmount(row.topping_price) }}</template>
          </el-table-column>
          <el-table-column prop="total_price" label="單筆總價" width="110" align="right">
            <template #default="{ row }">{{ formatAmount(row.total_price) }}</template>
          </el-table-column>
          <el-table-column prop="quantity" label="數量" width="80" align="center" />
          <el-table-column prop="note" label="備註" min-width="150" show-overflow-tooltip />
          <el-table-column prop="created_at" label="建立時間" width="160">
            <template #default="{ row }">{{ formatDateTime(row.created_at) }}</template>
          </el-table-column>
        </el-table>
      </el-card>

      <!-- 變更狀態 dialog -->
      <OrderStatusChangeDialog
        v-model:visible="statusDialogVisible"
        :order-id="orderId"
        :current-status="detail?.status"
        @success="handleStatusChangeSuccess"
      />
    </template>
  </div>
</template>

<style scoped>
.summary-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-top: 16px;
}

.summary-card {
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  padding: 16px;
  text-align: center;
  background: var(--el-fill-color-light);
}

.summary-label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-bottom: 8px;
}

.summary-value {
  font-size: 24px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.actions {
  margin-top: 24px;
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}
</style>
