<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiError } from '~/composable/useApiError'
import { usePermission } from '~/composable/usePermission'
import { MENU } from '@app/core'
import type { components } from '@app/api-types/admin'

type AdminUser = components['schemas']['AdminUserListResponse']

const api = useAdminApi()
const router = useRouter()
const { handleError } = useApiError()
const { can } = usePermission()

// 搜尋 & 篩選
const keyword = ref('')
const filterActive = ref<boolean | undefined>(undefined)

// 分頁
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

// 排序
const sortBy = ref('created_at')
const sortOrder = ref('desc')

// 資料
const tableData = ref<AdminUser[]>([])
const loading = ref(false)

const fetchList = async () => {
  loading.value = true
  const { data: res } = await api.GET('/api/admin/users', {
    params: {
      query: {
        page: page.value,
        pageSize: pageSize.value,
        sortBy: sortBy.value,
        sortOrder: sortOrder.value,
        ...(keyword.value ? { keyword: keyword.value } : {}),
        ...(filterActive.value !== undefined ? { isActive: filterActive.value } : {}),
      },
    },
  })
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
    sortBy.value = 'created_at'
    sortOrder.value = 'desc'
  }
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
  } catch {
    return // 使用者取消
  }

  const { error } = await api.DELETE('/api/admin/users/{userId}', {
    params: { path: { userId: user.id! } },
  })
  if (error) {
    handleError(error, '刪除失敗')
    return
  }
  ElMessage.success('刪除成功')
  await fetchList()
}

// 重設密碼
const resetPasswordDialogVisible = ref(false)
const resetPasswordUserId = ref<number | null>(null)
const resetPasswordUsername = ref('')
const newPassword = ref('')
const resetPasswordLoading = ref(false)

const openResetPasswordDialog = (user: AdminUser) => {
  resetPasswordUserId.value = user.id!
  resetPasswordUsername.value = user.username!
  newPassword.value = ''
  resetPasswordDialogVisible.value = true
}

const handleResetPassword = async () => {
  if (!newPassword.value) {
    ElMessage.warning('請輸入新密碼')
    return
  }
  resetPasswordLoading.value = true
  const { error } = await api.PUT('/api/admin/users/{userId}/password', {
    params: { path: { userId: resetPasswordUserId.value! } },
    body: { new_password: newPassword.value },
  })
  resetPasswordLoading.value = false

  if (error) {
    handleError(error, '密碼重設失敗')
    return
  }
  ElMessage.success('密碼重設成功')
  resetPasswordDialogVisible.value = false
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
          <el-button v-if="can(MENU.AdminAccountList, 'create')" type="primary" icon="Plus" @click="router.push('/admin-account/create')">
            新增帳號
          </el-button>
        </div>
      </div>

      <!-- 表格 -->
      <el-table :data="tableData" v-loading="loading" stripe style="width: 100%" @sort-change="handleSortChange">
        <el-table-column prop="id" label="ID" width="80" sortable="custom" />
        <el-table-column prop="username" label="帳號" min-width="150" />
        <el-table-column prop="role_name" label="角色" width="150" />
        <el-table-column label="狀態" width="100">
          <template #default="{ row }">
            <el-tag :type="row.is_active ? 'success' : 'danger'" size="small">
              {{ row.is_active ? '啟用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="created_at" label="建立時間" width="180" sortable="custom">
          <template #default="{ row }">
            {{ new Date(row.created_at).toLocaleString('zh-TW') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="250" fixed="right">
          <template #default="{ row }">
            <el-button v-if="can(MENU.AdminAccountList, 'update')" size="small" @click="router.push(`/admin-account/${row.id}/edit`)">
              編輯
            </el-button>
            <el-button v-if="can(MENU.AdminAccountList, 'update')" size="small" type="warning" @click="openResetPasswordDialog(row)">
              重設密碼
            </el-button>
            <el-button v-if="can(MENU.AdminAccountList, 'delete')" size="small" type="danger" @click="handleDelete(row)">
              刪除
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
