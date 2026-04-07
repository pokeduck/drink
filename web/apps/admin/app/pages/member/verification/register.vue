<script setup lang="ts">
import { useApi } from '~/composable/useApi'
import { useApiError } from '~/composable/useApiError'

interface Verification {
  id: number
  user_id: number
  user_name: string
  user_email: string
  is_success: boolean
  is_used: boolean
  sent_at: string
  expires_at: string
}

interface PaginationList {
  items: Verification[]
  total: number
  page: number
  page_size: number
}

interface BatchResendResult {
  success_count: number
  skip_count: number
  skipped_ids: number[]
}

interface ApiResponse<T> {
  data: T
  code: number
}

const api = useApi()
const { handleError } = useApiError()

// 搜尋 & 篩選
const keyword = ref('')
const filterSuccess = ref<boolean | undefined>(undefined)
const filterUsed = ref<boolean | undefined>(undefined)

// 分頁
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

// 排序
const sortBy = ref('sent_at')
const sortOrder = ref('desc')

// 資料
const tableData = ref<Verification[]>([])
const loading = ref(false)

// 多選
const selectedRows = ref<Verification[]>([])

const fetchList = async () => {
  loading.value = true
  try {
    const params: Record<string, any> = {
      page: page.value,
      page_size: pageSize.value,
      sort_by: sortBy.value,
      sort_order: sortOrder.value,
    }
    if (keyword.value) params.keyword = keyword.value
    if (filterSuccess.value !== undefined) params.is_success = filterSuccess.value
    if (filterUsed.value !== undefined) params.is_used = filterUsed.value

    const res = await api.get<ApiResponse<PaginationList>>('/admin/verifications/register', { params })
    tableData.value = res.data.items
    total.value = res.data.total
  } catch (err) {
    console.error('Failed to fetch verifications:', err)
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  page.value = 1
  fetchList()
}

const handleSortChange = ({ prop, order }: { prop: string; order: string | null }) => {
  if (order) {
    sortBy.value = prop
    sortOrder.value = order === 'ascending' ? 'asc' : 'desc'
  } else {
    sortBy.value = 'sent_at'
    sortOrder.value = 'desc'
  }
  page.value = 1
  fetchList()
}

const handleSelectionChange = (rows: Verification[]) => {
  selectedRows.value = rows
}

// 重發單筆
const handleResend = async (row: Verification) => {
  try {
    await ElMessageBox.confirm(`確定要重發驗證信給「${row.user_email}」嗎？`, '重發確認', {
      type: 'warning',
      confirmButtonText: '確認重發',
      cancelButtonText: '取消',
    })
    await api.post(`/admin/verifications/${row.id}/resend`)
    ElMessage.success('重發成功')
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') {
      handleError(err, undefined, '重發失敗')
    }
  }
}

// 批量重發
const batchResendLoading = ref(false)

const handleBatchResend = async () => {
  if (selectedRows.value.length === 0) {
    ElMessage.warning('請先選擇要重發的紀錄')
    return
  }
  try {
    await ElMessageBox.confirm(`確定要批量重發 ${selectedRows.value.length} 筆驗證信嗎？`, '批量重發確認', {
      type: 'warning',
      confirmButtonText: '確認重發',
      cancelButtonText: '取消',
    })
    batchResendLoading.value = true
    const ids = selectedRows.value.map(r => r.id)
    const res = await api.post<ApiResponse<BatchResendResult>>('/admin/verifications/register/resend', { ids })
    const data = res.data
    if (data.skip_count > 0) {
      ElMessage.warning(`成功 ${data.success_count} 筆，跳過 ${data.skip_count} 筆（已驗證或 10 分鐘內已重發）`)
    } else {
      ElMessage.success(`成功重發 ${data.success_count} 筆`)
    }
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') {
      handleError(err, undefined, '批量重發失敗')
    }
  } finally {
    batchResendLoading.value = false
  }
}

const isExpired = (expiresAt: string) => {
  return new Date(expiresAt) < new Date()
}

onMounted(() => {
  fetchList()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never">
      <!-- 工具列 -->
      <div class="toolbar">
        <div class="toolbar-left">
          <el-input v-model="keyword" placeholder="搜尋名稱 / Email" clearable style="width: 240px" @keyup.enter="handleSearch" @clear="handleSearch">
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <el-select v-model="filterSuccess" placeholder="發送狀態" clearable style="width: 130px" @change="handleSearch">
            <el-option label="成功" :value="true" />
            <el-option label="失敗" :value="false" />
          </el-select>
          <el-select v-model="filterUsed" placeholder="使用狀態" clearable style="width: 130px" @change="handleSearch">
            <el-option label="已使用" :value="true" />
            <el-option label="未使用" :value="false" />
          </el-select>
          <el-button type="primary" @click="handleSearch">查詢</el-button>
        </div>
        <div class="toolbar-right">
          <el-button type="warning" :loading="batchResendLoading" :disabled="selectedRows.length === 0" @click="handleBatchResend">
            批量重發 ({{ selectedRows.length }})
          </el-button>
        </div>
      </div>

      <!-- 表格 -->
      <el-table :data="tableData" v-loading="loading" stripe style="width: 100%" @sort-change="handleSortChange" @selection-change="handleSelectionChange">
        <el-table-column type="selection" width="50" />
        <el-table-column prop="id" label="ID" width="80" sortable="custom" />
        <el-table-column prop="user_name" label="名稱" min-width="120" />
        <el-table-column prop="user_email" label="Email" min-width="200" />
        <el-table-column label="發送狀態" width="100">
          <template #default="{ row }">
            <el-tag :type="row.is_success ? 'success' : 'danger'" size="small">
              {{ row.is_success ? '成功' : '失敗' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="使用狀態" width="100">
          <template #default="{ row }">
            <el-tag :type="row.is_used ? 'primary' : 'info'" size="small">
              {{ row.is_used ? '已使用' : '未使用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="sent_at" label="發送時間" width="180" sortable="custom">
          <template #default="{ row }">
            {{ new Date(row.sent_at).toLocaleString('zh-TW') }}
          </template>
        </el-table-column>
        <el-table-column label="過期時間" width="180" sortable="custom" prop="expires_at">
          <template #default="{ row }">
            <span :class="{ 'text-danger': isExpired(row.expires_at) }">
              {{ new Date(row.expires_at).toLocaleString('zh-TW') }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="warning" @click="handleResend(row)">
              重發
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分頁 -->
      <AppPagination v-model:page="page" v-model:page-size="pageSize" :total="total" @change="fetchList" />
    </el-card>
  </div>
</template>

<style scoped>
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  flex-wrap: wrap;
  gap: 12px;
}

.toolbar-left {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}

.text-danger {
  color: var(--el-color-danger);
}
</style>
