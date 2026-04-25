<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useAuthStore } from '~/stores/auth'

const api = useAdminApi()
const authStore = useAuthStore()
const router = useRouter()
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors, showSuccess, startLoading, stopLoading } = useApiFeedback()

const formRef = ref()

const form = reactive({
  old_password: '',
  new_password: '',
  confirm_password: '',
})

const validateConfirm = (_rule: any, value: string, callback: (err?: Error) => void) => {
  if (value !== form.new_password) {
    callback(new Error('兩次密碼不一致'))
  } else {
    callback()
  }
}

const rules = {
  old_password: [{ required: true, message: '請輸入舊密碼', trigger: 'blur' }],
  new_password: [{ required: true, message: '請輸入新密碼', trigger: 'blur' }],
  confirm_password: [
    { required: true, message: '請再次輸入新密碼', trigger: 'blur' },
    { validator: validateConfirm, trigger: 'blur' },
  ],
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  clearErrors()
  startLoading()
  const { error } = await api.PUT('/api/admin/auth/password', {
    body: {
      old_password: form.old_password,
      new_password: form.new_password,
    },
  })
  await stopLoading()

  if (error) {
    handleError(error, '密碼修改失敗')
    return
  }
  showSuccess('密碼修改成功，請重新登入')
  await authStore.logout()
  await router.push('/login')
}
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never">
      <template #header>
        <div style="display: flex; align-items: center; gap: 8px">
          <el-button text @click="router.back()"><el-icon><ArrowLeft /></el-icon>返回</el-button>
          <span>修改密碼</span>
        </div>
      </template>
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="舊密碼" prop="old_password" :error="serverErrors.old_password">
              <el-input v-model="form.old_password" type="password" placeholder="請輸入舊密碼" show-password />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="新密碼" prop="new_password" :error="serverErrors.new_password">
              <el-input v-model="form.new_password" type="password" placeholder="請輸入新密碼" show-password />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="確認新密碼" prop="confirm_password">
              <el-input v-model="form.confirm_password" type="password" placeholder="請再次輸入新密碼" show-password />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item>
          <el-button type="primary" @click="handleSubmit">確認修改</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
