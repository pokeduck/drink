<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { usePermission } from '~/composable/usePermission'
import { MENU } from '@app/core'
import type { components } from '@app/api-types/admin'

type Verification = components['schemas']['VerificationListResponse']

const api = useAdminApi()
const { handleError, showSuccess, startLoading, stopLoading } = useApiFeedback()
const { can } = usePermission()

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
  const query: Record<string, any> = {
    page: page.value,
    page_size: pageSize.value,
    sort_by: sortBy.value,
    sort_order: sortOrder.value,
  }
  if (keyword.value) query.keyword = keyword.value
  if (filterSuccess.value !== undefined) query.is_success = filterSuccess.value
  if (filterUsed.value !== undefined) query.is_used = filterUsed.value

  const { data: res } = await api.GET('/api/admin/verifications/forgot-password', { params: { query } })
  tableData.value = res?.data?.items ?? []
  total.value = res?.data?.total ?? 0
  loading.value = false
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
  } catch { return }

  startLoading()
  const { error } = await api.POST('/api/admin/verifications/{verificationId}/resend', {
    params: { path: { verificationId: row.id! } },
  })
  await stopLoading()
  if (error) { handleError(error, '重發失敗'); return }
  showSuccess('重發成功')
  await fetchList()
}

// 批量重發
const handleBatchResend = async () => {
  if (selectedRows.value.length === 0) {
    handleError({ message: '請先選擇要重發的紀錄' }, '請先選擇要重發的紀錄')
    return
  }
  try {
    await ElMessageBox.confirm(`確定要批量重發 ${selectedRows.value.length} 筆驗證信嗎？`, '批量重發確認', {
      type: 'warning',
      confirmButtonText: '確認重發',
      cancelButtonText: '取消',
    })
  } catch { return }

  startLoading()
  const ids = selectedRows.value.map(r => r.id!)
  const { data: res, error } = await api.POST('/api/admin/verifications/forgot-password/resend', {
    body: { ids },
  })
  await stopLoading()
  if (error) { handleError(error, '批量重發失敗'); return }
  const data = res!.data!
  if (data.skip_count! > 0) {
    showSuccess(`成功 ${data.success_count} 筆，跳過 ${data.skip_count} 筆（已驗證或 10 分鐘內已重發）`)
  } else {
    showSuccess(`成功重發 ${data.success_count} 筆`)
  }
  await fetchList()
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
          <el-button v-if="can(MENU.VerificationForgotPassword, 'create')" type="warning" :disabled="selectedRows.length === 0" @click="handleBatchResend">
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
            <el-button v-if="can(MENU.VerificationForgotPassword, 'create')" size="small" type="warning" @click="handleResend(row)">
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
