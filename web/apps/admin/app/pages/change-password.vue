<script setup lang="ts">
import { useApi } from '~/composable/useApi'
import { useAuthStore } from '~/stores/auth'

const api = useApi()
const authStore = useAuthStore()
const router = useRouter()

const formRef = ref()
const loading = ref(false)

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

  loading.value = true
  try {
    await api.put('/admin/auth/password', {
      old_password: form.old_password,
      new_password: form.new_password,
    })
    ElMessage.success('密碼修改成功，請重新登入')
    await authStore.logout()
    await router.push('/login')
  } catch (err: any) {
    const msg = err?.data?.message || '密碼修改失敗'
    ElMessage.error(msg)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header @back="router.back()">
      <template #content>修改密碼</template>
    </el-page-header>

    <el-card shadow="never" style="margin-top: 16px; max-width: 500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px" label-position="right">
        <el-form-item label="舊密碼" prop="old_password">
          <el-input v-model="form.old_password" type="password" placeholder="請輸入舊密碼" show-password />
        </el-form-item>

        <el-form-item label="新密碼" prop="new_password">
          <el-input v-model="form.new_password" type="password" placeholder="請輸入新密碼" show-password />
        </el-form-item>

        <el-form-item label="確認新密碼" prop="confirm_password">
          <el-input v-model="form.confirm_password" type="password" placeholder="請再次輸入新密碼" show-password />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :loading="loading" @click="handleSubmit">確認修改</el-button>
          <el-button @click="router.back()">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
