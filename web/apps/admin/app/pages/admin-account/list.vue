<script setup lang="ts">
import { useApi } from '~/composable/useApi'

interface AdminUser {
  id: number
  username: string
  role_id: number
  role_name: string
  is_active: boolean
  created_at: string
}

interface PaginationList {
  items: AdminUser[]
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

// 搜尋 & 篩選
const keyword = ref('')
const filterActive = ref<boolean | undefined>(undefined)

// 分頁
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

// 資料
const tableData = ref<AdminUser[]>([])
const loading = ref(false)

const fetchList = async () => {
  loading.value = true
  try {
    const params: Record<string, any> = {
      page: page.value,
      pageSize: pageSize.value,
    }
    if (keyword.value) params.keyword = keyword.value
    if (filterActive.value !== undefined) params.isActive = filterActive.value

    const res = await api.get<ApiResponse<PaginationList>>('/admin/users', { params })
    tableData.value = res.data.items
    total.value = res.data.total
  } catch (err) {
    console.error('Failed to fetch users:', err)
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  page.value = 1
  fetchList()
}

const handlePageChange = (newPage: number) => {
  page.value = newPage
  fetchList()
}

const handleSizeChange = (newSize: number) => {
  pageSize.value = newSize
  page.value = 1
  fetchList()
}

// 刪除
const handleDelete = async (user: AdminUser) => {
  try {
    await ElMessageBox.confirm(`確定要刪除帳號「${user.username}」嗎？`, '刪除確認', {
      type: 'warning',
      confirmButtonText: '刪除',
      cancelButtonText: '取消',
    })
    await api.delete(`/admin/users/${user.id}`)
    ElMessage.success('刪除成功')
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') {
      const msg = err?.data?.message || '刪除失敗'
      ElMessage.error(msg)
    }
  }
}

// 重設密碼
const resetPasswordDialogVisible = ref(false)
const resetPasswordUserId = ref<number | null>(null)
const resetPasswordUsername = ref('')
const newPassword = ref('')
const resetPasswordLoading = ref(false)

const openResetPasswordDialog = (user: AdminUser) => {
  resetPasswordUserId.value = user.id
  resetPasswordUsername.value = user.username
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
    await api.put(`/admin/users/${resetPasswordUserId.value}/password`, {
      new_password: newPassword.value,
    })
    ElMessage.success('密碼重設成功')
    resetPasswordDialogVisible.value = false
  } catch (err: any) {
    ElMessage.error(err?.data?.message || '密碼重設失敗')
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
          <el-input v-model="keyword" placeholder="搜尋帳號" clearable style="width: 240px" @keyup.enter="handleSearch" @clear="handleSearch">
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <el-select v-model="filterActive" placeholder="狀態" clearable style="width: 120px" @change="handleSearch">
            <el-option label="啟用" :value="true" />
            <el-option label="停用" :value="false" />
          </el-select>
          <el-button type="primary" @click="handleSearch">查詢</el-button>
        </div>
        <div class="toolbar-right">
          <el-button type="primary" :icon="Plus" @click="router.push('/admin-account/create')">
            新增帳號
          </el-button>
        </div>
      </div>

      <!-- 表格 -->
      <el-table :data="tableData" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="username" label="帳號" min-width="150" />
        <el-table-column prop="role_name" label="角色" width="150" />
        <el-table-column label="狀態" width="100">
          <template #default="{ row }">
            <el-tag :type="row.is_active ? 'success' : 'danger'" size="small">
              {{ row.is_active ? '啟用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="建立時間" width="180">
          <template #default="{ row }">
            {{ new Date(row.created_at).toLocaleString('zh-TW') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="250" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="router.push(`/admin-account/${row.id}/edit`)">
              編輯
            </el-button>
            <el-button size="small" type="warning" @click="openResetPasswordDialog(row)">
              重設密碼
            </el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)">
              刪除
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分頁 -->
      <div class="pagination-wrapper">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next, jumper"
          background
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </el-card>

    <!-- 重設密碼 Dialog -->
    <el-dialog v-model="resetPasswordDialogVisible" title="重設密碼" width="420" :close-on-click-modal="false">
      <el-form label-position="top">
        <el-form-item label="帳號">
          <el-input :model-value="resetPasswordUsername" disabled />
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
}

.toolbar-left {
  display: flex;
  gap: 12px;
  align-items: center;
}

</style>
