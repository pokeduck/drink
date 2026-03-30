<script setup lang="ts">
import { useApi } from '~/composable/useApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiError } from '~/composable/useApiError'
import { useLoading } from '~/composable/useLoading'

interface MenuCrudItem {
  menu_id: number
  menu_name: string
  can_read: boolean
  can_create: boolean
  can_update: boolean
  can_delete: boolean
}

interface RoleDetail {
  id: number
  name: string
  is_system: boolean
  menus: MenuCrudItem[]
}

interface ApiResponse<T> {
  data: T
  code: number
}

const api = useApi()
const router = useRouter()
const route = useRoute()
const roleId = Number(route.params.roleId)
const { labelPosition } = useFormLayout()
const { handleError } = useApiError()

const formRef = ref()
const { loading, start: startLoading, stop: stopLoading } = useLoading()
const fetchLoading = ref(true)
const isSystem = ref(false)

const form = reactive({
  name: '',
})

const rules = {
  name: [
    { required: true, message: '請輸入角色名稱', trigger: 'blur' },
    { max: 50, message: '角色名稱最多 50 字', trigger: 'blur' },
  ],
}

const menuCrudList = ref<MenuCrudItem[]>([])

const fetchRole = async () => {
  fetchLoading.value = true
  try {
    const res = await api.get<ApiResponse<RoleDetail>>(`/admin/roles/${roleId}`)
    const role = res.data
    form.name = role.name
    isSystem.value = role.is_system
    menuCrudList.value = role.menus
  } catch (err: any) {
    ElMessage.error('載入角色資料失敗')
    router.push('/admin-account/role')
  } finally {
    fetchLoading.value = false
  }
}

const handleSubmit = async () => {
  if (isSystem.value) return

  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  try {
    await api.put(`/admin/roles/${roleId}`, {
      name: form.name,
      menus: menuCrudList.value.map((m) => ({
        menu_id: m.menu_id,
        can_read: m.can_read,
        can_create: m.can_create,
        can_update: m.can_update,
        can_delete: m.can_delete,
      })),
    })
    ElMessage.success('角色更新成功')
    router.push('/admin-account/role')
  } catch (err: any) {
    handleError(err, formRef.value, '更新失敗')
  } finally {
    stopLoading()
  }
}

onMounted(() => {
  fetchRole()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/admin-account/role')">
      <template #content>{{ isSystem ? '檢視角色' : '編輯角色' }}</template>
    </el-page-header>

    <el-card v-loading="fetchLoading || loading" shadow="never" style="margin-top: 16px">
      <el-alert
        v-if="isSystem"
        type="info"
        :closable="false"
        show-icon
        style="margin-bottom: 16px"
      >
        <template #title>系統角色無法修改</template>
      </el-alert>

      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large" @submit.prevent>
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="角色名稱" prop="name">
              <el-input
                v-model="form.name"
                placeholder="請輸入角色名稱"
                maxlength="50"
                :disabled="isSystem"
              />
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
            <el-checkbox v-model="row.can_read" :disabled="isSystem" />
          </template>
        </el-table-column>
        <el-table-column label="新增" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_create" :disabled="isSystem" />
          </template>
        </el-table-column>
        <el-table-column label="修改" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_update" :disabled="isSystem" />
          </template>
        </el-table-column>
        <el-table-column label="刪除" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_delete" :disabled="isSystem" />
          </template>
        </el-table-column>
      </el-table>

      <div v-if="!isSystem" style="margin-top: 24px">
        <el-button type="primary" @click="handleSubmit">儲存</el-button>
        <el-button @click="router.push('/admin-account/role')">取消</el-button>
      </div>
      <div v-else style="margin-top: 24px">
        <el-button @click="router.push('/admin-account/role')">返回</el-button>
      </div>
    </el-card>
  </div>
</template>
