<script setup lang="ts">
import { useApi } from '~/composable/useApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiError } from '~/composable/useApiError'
import { useLoading } from '~/composable/useLoading'

interface AdminUserDetail {
  id: number
  username: string
  role_id: number
  role_name: string
  is_active: boolean
  created_at: string
  updated_at: string
}

interface AdminRole {
  id: number
  name: string
}

interface ApiResponse<T> {
  data: T
  code: number
  error?: string
  message?: string
}

const api = useApi()
const router = useRouter()
const route = useRoute()
const userId = Number(route.params.id)
const { labelPosition } = useFormLayout()
const { handleError } = useApiError()

const formRef = ref()
const { loading, start: startLoading, stop: stopLoading } = useLoading()
const fetchLoading = ref(true)

const form = reactive({
  role_id: null as number | null,
  is_active: true,
})

const username = ref('')
const createdAt = ref('')
const updatedAt = ref('')

const rules = {
  role_id: [{ required: true, message: '請選擇角色', trigger: 'change' }],
}

// 載入角色清單
const roles = ref<AdminRole[]>([])
const fetchRoles = async () => {
  try {
    const res = await api.get<ApiResponse<AdminRole[]>>('/admin/roles')
    roles.value = res.data
  } catch {
    ElMessage.error('載入角色清單失敗')
  }
}

const fetchUser = async () => {
  fetchLoading.value = true
  try {
    const res = await api.get<ApiResponse<AdminUserDetail>>(`/admin/users/${userId}`)
    const user = res.data
    username.value = user.username
    form.role_id = user.role_id
    form.is_active = user.is_active
    createdAt.value = new Date(user.created_at).toLocaleString('zh-TW')
    updatedAt.value = new Date(user.updated_at).toLocaleString('zh-TW')
  } catch (err: any) {
    ElMessage.error('載入帳號資料失敗')
    router.push('/admin-account/list')
  } finally {
    fetchLoading.value = false
  }
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  try {
    await api.put(`/admin/users/${userId}`, form)
    ElMessage.success('更新成功')
    router.push('/admin-account/list')
  } catch (err: any) {
    handleError(err, formRef.value, '更新失敗')
  } finally {
    stopLoading()
  }
}

onMounted(async () => {
  await Promise.all([fetchRoles(), fetchUser()])
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/admin-account/list')">
      <template #content>編輯帳號</template>
    </el-page-header>

    <el-card v-loading="fetchLoading || loading" shadow="never" style="margin-top: 16px">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="帳號">
              <el-input :model-value="username" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="ID">
              <el-input :model-value="String(userId)" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="建立時間">
              <el-input :model-value="createdAt" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="更新時間">
              <el-input :model-value="updatedAt" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="角色" prop="role_id">
              <el-select v-model="form.role_id" placeholder="請選擇角色" style="width: 100%">
                <el-option v-for="role in roles" :key="role.id" :label="role.name" :value="role.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="狀態">
              <el-switch v-model="form.is_active" active-text="啟用" inactive-text="停用" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item>
          <el-button type="primary" @click="handleSubmit">儲存</el-button>
          <el-button @click="router.push('/admin-account/list')">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
