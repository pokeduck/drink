<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import { useLoading } from '~/composable/useLoading'

definePageMeta({
  layout: 'blank',
})

const authStore = useAuthStore()
const router = useRouter()

const form = reactive({
  username: '',
  password: '',
})

const { loading, start: startLoading, stop: stopLoading } = useLoading()
const errorMsg = ref('')
const formRef = ref()

const rules = {
  username: [{ required: true, message: '請輸入帳號', trigger: 'blur' }],
  password: [{ required: true, message: '請輸入密碼', trigger: 'blur' }],
}

// 已登入直接跳轉
if (authStore.isLoggedIn) {
  navigateTo('/')
}

const handleLogin = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  errorMsg.value = ''

  try {
    await authStore.login(form)
    await router.push('/')
  } catch (err: any) {
    errorMsg.value = err.message || '登入失敗'
  } finally {
    stopLoading()
  }
}
</script>

<template>
  <div class="login-container">
    <el-card v-loading="loading" class="login-card" shadow="hover">
      <div class="login-header">
        <el-icon :size="36" color="#409eff">
          <ColdDrink />
        </el-icon>
        <h2>DRINK ADMIN</h2>
      </div>

      <el-alert v-if="errorMsg" :title="errorMsg" type="error" show-icon :closable="false" class="login-error" />

      <el-form ref="formRef" :model="form" :rules="rules" label-position="top" @keyup.enter="handleLogin">
        <el-form-item label="帳號" prop="username">
          <el-input v-model="form.username" prefix-icon="User" placeholder="請輸入帳號" size="large" />
        </el-form-item>

        <el-form-item label="密碼" prop="password">
          <el-input v-model="form.password" type="password" prefix-icon="Lock" placeholder="請輸入密碼" size="large" show-password />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" size="large" class="login-btn" @click="handleLogin">
            登入
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  width: 400px;
  padding: 20px;
}

.login-header {
  text-align: center;
  margin-bottom: 24px;
}

.login-header h2 {
  margin: 12px 0 0;
  color: #303133;
  letter-spacing: 2px;
}

.login-error {
  margin-bottom: 16px;
}

.login-btn {
  width: 100%;
}
</style>
