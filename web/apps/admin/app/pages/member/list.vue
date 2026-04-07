<script setup lang="ts">
import { useApi } from '~/composable/useApi'
import { useApiError } from '~/composable/useApiError'

interface Member {
  id: number
  name: string
  email: string
  avatar: string | null
  notification_type: number
  status: number
  email_verified: boolean
  is_google_connected: boolean
  last_login_at: string | null
  created_at: string
}

interface PaginationList {
  items: Member[]
  total: number
  page: number
  page_size: number
}

interface ApiResponse<T> {
  data: T
  code: number
}

const api = useApi()
const router = useRouter()
const { handleError } = useApiError()

// 搜尋 & 篩選
const keyword = ref('')
const filterStatus = ref<number | undefined>(undefined)
const filterEmailVerified = ref<boolean | undefined>(undefined)
const filterGoogleConnected = ref<boolean | undefined>(undefined)

// 分頁
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

// 排序
const sortBy = ref('created_at')
const sortOrder = ref('desc')

// 資料
const tableData = ref<Member[]>([])
const loading = ref(false)

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
    if (filterStatus.value !== undefined) params.status = filterStatus.value
    if (filterEmailVerified.value !== undefined) params.email_verified = filterEmailVerified.value
    if (filterGoogleConnected.value !== undefined) params.is_google_connected = filterGoogleConnected.value

    const res = await api.get<ApiResponse<PaginationList>>('/admin/members', { params })
    tableData.value = res.data.items
    total.value = res.data.total
  } catch (err) {
    console.error('Failed to fetch members:', err)
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
    sortBy.value = 'created_at'
    sortOrder.value = 'desc'
  }
  page.value = 1
  fetchList()
}

const statusLabel = (status: number) => {
  return status === 1 ? '啟用' : '停用'
}

const statusType = (status: number) => {
  return status === 1 ? 'success' : 'danger'
}

const notificationLabel = (type: number) => {
  const map: Record<number, string> = { 0: '不接收', 1: 'WebPush', 2: 'Email', 3: '全部' }
  return map[type] ?? '未知'
}

// 重設密碼
const resetPasswordDialogVisible = ref(false)
const resetPasswordMemberId = ref<number | null>(null)
const resetPasswordEmail = ref('')
const newPassword = ref('')
const resetPasswordLoading = ref(false)

const openResetPasswordDialog = (member: Member) => {
  resetPasswordMemberId.value = member.id
  resetPasswordEmail.value = member.email
  newPassword.value = ''
  resetPasswordDialogVisible.value = true
}

const handleResetPassword = async () => {
  if (!newPassword.value) {
    ElMessage.warning('請輸入新密碼')
    return
  }
  resetPasswordLoading.value = true
  try {
    await api.put(`/admin/members/${resetPasswordMemberId.value}/password`, {
      new_password: newPassword.value,
    })
    ElMessage.success('密碼重設成功')
    resetPasswordDialogVisible.value = false
  } catch (err: any) {
    handleError(err, undefined, '密碼重設失敗')
  } finally {
    resetPasswordLoading.value = false
  }
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
          <el-select v-model="filterStatus" placeholder="狀態" clearable style="width: 120px" @change="handleSearch">
            <el-option label="啟用" :value="1" />
            <el-option label="停用" :value="2" />
          </el-select>
          <el-select v-model="filterEmailVerified" placeholder="Email 驗證" clearable style="width: 140px" @change="handleSearch">
            <el-option label="已驗證" :value="true" />
            <el-option label="未驗證" :value="false" />
          </el-select>
          <el-select v-model="filterGoogleConnected" placeholder="Google 綁定" clearable style="width: 140px" @change="handleSearch">
            <el-option label="已綁定" :value="true" />
            <el-option label="未綁定" :value="false" />
          </el-select>
          <el-button type="primary" @click="handleSearch">查詢</el-button>
        </div>
        <div class="toolbar-right">
          <el-button type="primary" :icon="Plus" @click="router.push('/member/create')">
            新增會員
          </el-button>
        </div>
      </div>

      <!-- 表格 -->
      <el-table :data="tableData" v-loading="loading" stripe style="width: 100%" @sort-change="handleSortChange">
        <el-table-column prop="id" label="ID" width="80" sortable="custom" />
        <el-table-column prop="name" label="名稱" min-width="120" sortable="custom" />
        <el-table-column prop="email" label="Email" min-width="200" sortable="custom" />
        <el-table-column label="狀態" width="100" sortable="custom" prop="status">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)" size="small">
              {{ statusLabel(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="Email 驗證" width="110">
          <template #default="{ row }">
            <el-tag :type="row.email_verified ? 'success' : 'info'" size="small">
              {{ row.email_verified ? '已驗證' : '未驗證' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="Google" width="100">
          <template #default="{ row }">
            <el-tag :type="row.is_google_connected ? 'success' : 'info'" size="small">
              {{ row.is_google_connected ? '已綁定' : '未綁定' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="通知方式" width="100">
          <template #default="{ row }">
            {{ notificationLabel(row.notification_type) }}
          </template>
        </el-table-column>
        <el-table-column prop="created_at" label="建立時間" width="180" sortable="custom">
          <template #default="{ row }">
            {{ new Date(row.created_at).toLocaleString('zh-TW') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="router.push(`/member/${row.id}/edit`)">
              編輯
            </el-button>
            <el-button size="small" type="warning" @click="openResetPasswordDialog(row)">
              重設密碼
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分頁 -->
      <AppPagination v-model:page="page" v-model:page-size="pageSize" :total="total" @change="fetchList" />
    </el-card>

    <!-- 重設密碼 Dialog -->
    <el-dialog v-model="resetPasswordDialogVisible" title="重設密碼" width="420" :close-on-click-modal="false">
      <el-form label-position="top">
        <el-form-item label="Email">
          <el-input :model-value="resetPasswordEmail" disabled />
        </el-form-item>
        <el-form-item label="新密碼">
          <el-input v-model="newPassword" type="password" placeholder="請輸入新密碼" show-password />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="resetPasswordDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="resetPasswordLoading" @click="handleResetPassword">確認</el-button>
      </template>
    </el-dialog>
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
</style>
