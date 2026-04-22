<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiError } from '~/composable/useApiError'
import { useLoading } from '~/composable/useLoading'
import type { components } from '@app/api-types/admin'

type MenuCrudItem = components['schemas']['AdminMenuRoleResponse']

const api = useAdminApi()
const router = useRouter()
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors } = useApiError()

const formRef = ref()
const { loading, start: startLoading, stop: stopLoading } = useLoading()
const fetchLoading = ref(true)

const form = reactive({
  name: '',
})

const rules = {
  name: [
    { required: true, message: '請輸入角色名稱', trigger: 'blur' },
    { max: 50, message: '角色名稱最多 50 字', trigger: 'blur' },
  ],
}

// Menu CRUD 矩陣
const menuCrudList = ref<MenuCrudItem[]>([])

const fetchMenus = async () => {
  fetchLoading.value = true
  const { data: res, error } = await api.GET('/api/admin/roles/{roleId}', {
    params: { path: { roleId: 1 } },
  })

  if (error) {
    ElMessage.error('載入 Menu 資料失敗')
    fetchLoading.value = false
    return
  }

  menuCrudList.value = (res!.data!.menus ?? []).map((m) => ({
    ...m,
    can_read: false,
    can_create: false,
    can_update: false,
    can_delete: false,
  }))
  fetchLoading.value = false
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  clearErrors()
  startLoading()
  const { error } = await api.POST('/api/admin/roles', {
    body: {
      name: form.name,
      menus: menuCrudList.value.map((m) => ({
        menu_id: m.menu_id!,
        can_read: m.can_read,
        can_create: m.can_create,
        can_update: m.can_update,
        can_delete: m.can_delete,
      })),
    },
  })
  stopLoading()

  if (error) {
    handleError(error, '建立失敗')
    return
  }
  ElMessage.success('角色建立成功')
  router.push('/admin-account/role')
}

onMounted(() => {
  fetchMenus()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/admin-account/role')">
      <template #content>新增角色</template>
    </el-page-header>

    <el-card v-loading="fetchLoading || loading" shadow="never" style="margin-top: 16px">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large" @submit.prevent>
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="角色名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入角色名稱" maxlength="50" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>

      <!-- Menu CRUD 矩陣 -->
      <h4 style="margin: 24px 0 12px">Menu 權限設定</h4>
      <el-table :data="menuCrudList" stripe border style="width: 100%">
        <el-table-column prop="menu_name" label="功能頁面" min-width="200" />
        <el-table-column label="讀取" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_read" />
          </template>
        </el-table-column>
        <el-table-column label="新增" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_create" />
          </template>
        </el-table-column>
        <el-table-column label="修改" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_update" />
          </template>
        </el-table-column>
        <el-table-column label="刪除" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_delete" />
          </template>
        </el-table-column>
      </el-table>

      <div style="margin-top: 24px">
        <el-button type="primary" @click="handleSubmit">建立</el-button>
        <el-button @click="router.push('/admin-account/role')">取消</el-button>
      </div>
    </el-card>
  </div>
</template>
