<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'
import type { components } from '@app/api-types/admin'

type AdminRole = components['schemas']['AdminRoleListResponse']

const api = useAdminApi()
const router = useRouter()
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors, showSuccess, startLoading, stopLoading } = useApiFeedback()

const formRef = ref()

const form = reactive({
  username: '',
  password: '',
  role_id: null as number | null,
  is_active: true,
})

const rules = {
  username: [
    { required: true, message: '請輸入帳號', trigger: 'blur' },
    { max: 50, message: '帳號最多 50 字', trigger: 'blur' },
  ],
  password: [{ required: true, message: '請輸入密碼', trigger: 'blur' }],
  role_id: [{ required: true, message: '請選擇角色', trigger: 'change' }],
}

// 載入角色清單
const roles = ref<AdminRole[]>([])
const fetchRoles = async () => {
  const { data: res } = await api.GET('/api/admin/roles')
  roles.value = res?.data ?? []
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  clearErrors()
  startLoading()
  const { error } = await api.POST('/api/admin/users', {
    body: {
      username: form.username,
      password: form.password,
      role_id: form.role_id!,
      is_active: form.is_active,
    },
  })
  await stopLoading()

  if (error) {
    handleError(error, '建立失敗')
    return
  }
  showSuccess('帳號建立成功')
  router.push('/admin-account/list')
}

onMounted(() => {
  fetchRoles()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never">
      <template #header>
        <div style="display: flex; align-items: center; gap: 8px">
          <el-button text @click="router.push('/admin-account/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
          <span>新增帳號</span>
        </div>
      </template>
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="帳號" prop="username" :error="serverErrors.username">
              <el-input v-model="form.username" placeholder="請輸入帳號" maxlength="50" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="密碼" prop="password" :error="serverErrors.password">
              <el-input v-model="form.password" type="password" placeholder="請輸入密碼" show-password />
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
          <el-button type="primary" @click="handleSubmit">建立</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
