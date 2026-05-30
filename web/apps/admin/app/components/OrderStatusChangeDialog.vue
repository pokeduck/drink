<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'

const props = defineProps<{
  visible: boolean
  orderId: number
  currentStatus: number | undefined
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  success: []
}>()

const api = useAdminApi()
const { handleError, showSuccess, startLoading, stopLoading } = useApiFeedback()

const statusLabels: Record<number, string> = {
  1: '進行中',
  2: '已截止',
  3: '配送中',
  4: '已完成',
  5: '已取消',
}

// 對應 openspec/specs/admin-order/spec.md 的白名單
// Active(1) -> Closed(2) / Delivered(3) / Cancelled(5)
// Closed(2) -> Active(1) / Delivered(3) / Cancelled(5)
// Delivered(3) -> Active(1) / Closed(2) / Completed(4)
// Completed(4) / Cancelled(5) 為終態
const validTransitions: Record<number, number[]> = {
  1: [2, 3, 5],
  2: [1, 3, 5],
  3: [1, 2, 4],
  4: [],
  5: [],
}

const targets = computed(() => validTransitions[props.currentStatus ?? 0] ?? [])

const selected = ref<number | null>(null)
const submitting = ref(false)

const dialogVisible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val),
})

watch(
  () => props.visible,
  (open) => {
    if (open) selected.value = null
  },
)

const close = () => {
  dialogVisible.value = false
}

const handleConfirm = async () => {
  if (!selected.value) return
  submitting.value = true
  startLoading()
  try {
    const { error } = await api.PUT('/api/admin/orders/{orderId}/status', {
      params: { path: { orderId: props.orderId } },
      // openapi-fetch 型別接受 enum literal 數字
      body: { status: selected.value as 1 | 2 | 3 | 4 | 5 },
    })
    await stopLoading()
    if (error) {
      handleError(error, '狀態變更失敗')
      return
    }
    showSuccess('狀態變更成功')
    emit('success')
    close()
  } catch (err) {
    await stopLoading()
    handleError(err, '狀態變更失敗')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <el-dialog
    v-model="dialogVisible"
    title="變更訂單狀態"
    width="420px"
    :close-on-click-modal="false"
  >
    <div v-if="targets.length === 0" style="text-align: center; color: var(--el-text-color-secondary)">
      無可變更狀態
    </div>
    <el-form v-else label-position="top">
      <el-form-item label="目前狀態">
        <OrderStatusTag :status="currentStatus" />
      </el-form-item>
      <el-form-item label="變更為">
        <el-radio-group v-model="selected">
          <el-radio v-for="target in targets" :key="target" :value="target" style="display: block; margin-bottom: 8px">
            {{ statusLabels[target] }}
          </el-radio>
        </el-radio-group>
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="close">取消</el-button>
      <el-button
        v-if="targets.length > 0"
        type="primary"
        :disabled="!selected || submitting"
        @click="handleConfirm"
      >
        確認變更
      </el-button>
    </template>
  </el-dialog>
</template>
