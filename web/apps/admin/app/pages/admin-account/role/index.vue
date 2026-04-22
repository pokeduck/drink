<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiError } from '~/composable/useApiError'
import { usePermission } from '~/composable/usePermission'
import { MENU } from '@app/core'
import type { components } from '@app/api-types/admin'

type AdminRole = components['schemas']['AdminRoleListResponse']

const api = useAdminApi()
const router = useRouter()
const { handleError } = useApiError()
const { can } = usePermission()

const tableData = ref<AdminRole[]>([])
const loading = ref(false)

const fetchList = async () => {
  loading.value = true
  try {
    const { data: res } = await api.GET('/api/admin/roles')
    tableData.value = res?.data ?? []
  } catch (err) {
    console.error('Failed to fetch roles:', err)
  } finally {
    loading.value = false
  }
}

// 刪除
const deleteDialogVisible = ref(false)
const deleteTarget = ref<AdminRole | null>(null)
const reassignRoleId = ref<number | null>(null)
const deleteLoading = ref(false)

const openDeleteDialog = (role: AdminRole) => {
  deleteTarget.value = role
  reassignRoleId.value = null
  deleteDialogVisible.value = true
}

const availableRolesForReassign = computed(() => {
  if (!deleteTarget.value) return []
  return tableData.value.filter((r) => r.id !== deleteTarget.value!.id)
})

const handleDelete = async () => {
  if (!deleteTarget.value) return

  if ((deleteTarget.value.staff_count ?? 0) > 0 && !reassignRoleId.value) {
    ElMessage.warning('請選擇要遷移 Staff 的目標角色')
    return
  }

  deleteLoading.value = true
  try {
    const body: { reassign_role_id?: number | null } = {}
    if ((deleteTarget.value.staff_count ?? 0) > 0) {
      body.reassign_role_id = reassignRoleId.value
    }
    await api.DELETE('/api/admin/roles/{roleId}', {
      params: { path: { roleId: deleteTarget.value.id! } },
      body,
    })
    ElMessage.success('刪除成功')
    deleteDialogVisible.value = false
    await fetchList()
  } catch (err: any) {
    handleError(err, undefined, '刪除失敗')
  } finally {
    deleteLoading.value = false
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
        <div />
        <div>
          <el-button v-if="can(MENU.AdminRole, 'create')" type="primary" icon="Plus" @click="router.push('/admin-account/role/create')">
            新增角色
          </el-button>
        </div>
      </div>

      <!-- 表格 -->
      <el-table :data="tableData" v-loading="loading" stripe style="width: 100%">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="name" label="角色名稱" min-width="200" />
        <el-table-column label="類型" width="120">
          <template #default="{ row }">
            <el-tag v-if="row.is_system" type="warning" size="small">系統</el-tag>
            <el-tag v-else size="small">自訂</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="staff_count" label="帳號數量" width="120" />
        <el-table-column label="建立時間" width="180">
          <template #default="{ row }">
            {{ new Date(row.created_at).toLocaleString('zh-TW') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="router.push(`/admin-account/role/${row.id}/edit`)">
              {{ row.is_system ? '檢視' : '編輯' }}
            </el-button>
            <el-button
              v-if="!row.is_system && can(MENU.AdminRole, 'delete')"
              size="small"
              type="danger"
              @click="openDeleteDialog(row)"
            >
              刪除
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 刪除確認 Dialog -->
    <el-dialog v-model="deleteDialogVisible" title="刪除角色" width="480" :close-on-click-modal="false">
      <template v-if="deleteTarget">
        <p>確定要刪除角色「{{ deleteTarget.name }}」嗎？</p>
        <template v-if="(deleteTarget.staff_count ?? 0) > 0">
          <el-alert
            type="warning"
            :closable="false"
            show-icon
            style="margin: 12px 0"
          >
            <template #title>
              此角色下有 <strong>{{ deleteTarget.staff_count }}</strong> 個帳號，刪除前必須將這些帳號遷移至其他角色。
            </template>
          </el-alert>
          <el-form label-position="top">
            <el-form-item label="遷移至角色">
              <el-select v-model="reassignRoleId" placeholder="請選擇目標角色" style="width: 100%">
                <el-option
                  v-for="role in availableRolesForReassign"
                  :key="role.id!"
                  :label="role.name!"
                  :value="role.id!"
                />
              </el-select>
            </el-form-item>
          </el-form>
        </template>
      </template>
      <template #footer>
        <el-button @click="deleteDialogVisible = false">取消</el-button>
        <el-button type="danger" :loading="deleteLoading" @click="handleDelete">確認刪除</el-button>
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
</style>
