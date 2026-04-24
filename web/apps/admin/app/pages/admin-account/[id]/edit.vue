<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'
import type { components } from '@app/api-types/admin'

type AdminRole = components['schemas']['AdminRoleListResponse']

const api = useAdminApi()
const router = useRouter()
const route = useRoute()
const userId = Number(route.params.id)
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors, showSuccess, startLoading, stopLoading } = useApiFeedback()

const formRef = ref()
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
  const { data: res } = await api.GET('/api/admin/roles')
  roles.value = res?.data ?? []
}

const fetchUser = async () => {
  fetchLoading.value = true
  const { data: res, error } = await api.GET('/api/admin/users/{userId}', {
    params: { path: { userId } },
  })

  if (error) {
    handleError(error, '載入帳號資料失敗')
    router.push('/admin-account/list')
    return
  }

  const user = res!.data!
  username.value = user.username!
  form.role_id = user.role_id!
  form.is_active = user.is_active!
  createdAt.value = new Date(user.created_at!).toLocaleString('zh-TW')
  updatedAt.value = new Date(user.updated_at!).toLocaleString('zh-TW')
  fetchLoading.value = false
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  clearErrors()
  startLoading()
  const { error } = await api.PUT('/api/admin/users/{userId}', {
    params: { path: { userId } },
    body: {
      role_id: form.role_id!,
      is_active: form.is_active,
    },
  })
  await stopLoading()

  if (error) {
    handleError(error, '更新失敗')
    return
  }
  showSuccess('更新成功')
  router.push('/admin-account/list')
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

    <el-card v-loading="fetchLoading" shadow="never" style="margin-top: 16px">
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
            <el-form-item label="角色" prop="role_id" :error="serverErrors.role_id">
              <el-select v-model="form.role_id" placeholder="請選擇角色" style="width: 100%">
                <el-option v-for="role in roles" :key="role.id!" :label="role.name!" :value="role.id!" />
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
