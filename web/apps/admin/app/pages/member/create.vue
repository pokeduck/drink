<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiError } from '~/composable/useApiError'
import { useLoading } from '~/composable/useLoading'

const api = useAdminApi()
const router = useRouter()
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors } = useApiError()

const formRef = ref()
const { loading, start: startLoading, stop: stopLoading } = useLoading()

const form = reactive({
  name: '',
  email: '',
  password: '',
})

const rules = {
  name: [
    { required: true, message: '請輸入名稱', trigger: 'blur' },
    { max: 100, message: '名稱最多 100 字', trigger: 'blur' },
  ],
  email: [
    { required: true, message: '請輸入 Email', trigger: 'blur' },
    { type: 'email' as const, message: '請輸入有效的 Email', trigger: 'blur' },
    { max: 200, message: 'Email 最多 200 字', trigger: 'blur' },
  ],
  password: [{ required: true, message: '請輸入密碼', trigger: 'blur' }],
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  clearErrors()
  const { error } = await api.POST('/api/admin/members', { body: form })
  stopLoading()
  if (error) { handleError(error, '建立失敗'); return }
  ElMessage.success('會員建立成功')
  router.push('/member/list')
}
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/member/list')">
      <template #content>新增會員</template>
    </el-page-header>

    <el-card v-loading="loading" shadow="never" style="margin-top: 16px">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入名稱" maxlength="100" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="Email" prop="email" :error="serverErrors.email">
              <el-input v-model="form.email" placeholder="請輸入 Email" maxlength="200" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="密碼" prop="password" :error="serverErrors.password">
              <el-input v-model="form.password" type="password" placeholder="請輸入密碼" show-password />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item>
          <el-button type="primary" @click="handleSubmit">建立</el-button>
          <el-button @click="router.push('/member/list')">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
