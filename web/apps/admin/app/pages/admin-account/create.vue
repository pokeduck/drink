<script setup lang="ts">
import { useApi } from '~/composable/useApi'
import { useFormLayout } from '~/composable/useFormLayout'

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
const { labelPosition } = useFormLayout()

const formRef = ref()
const loading = ref(false)

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
  try {
    const res = await api.get<ApiResponse<AdminRole[]>>('/admin/roles')
    roles.value = res.data
  } catch {
    ElMessage.error('載入角色清單失敗')
  }
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    await api.post('/admin/users', form)
    ElMessage.success('帳號建立成功')
    router.push('/admin-account/list')
  } catch (err: any) {
    const msg = err?.data?.message || '建立失敗'
    ElMessage.error(msg)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchRoles()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/admin-account/list')">
      <template #content>新增帳號</template>
    </el-page-header>

    <el-card shadow="never" style="margin-top: 16px">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="帳號" prop="username">
              <el-input v-model="form.username" placeholder="請輸入帳號" maxlength="50" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="密碼" prop="password">
              <el-input v-model="form.password" type="password" placeholder="請輸入密碼" show-password />
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
          <el-button type="primary" :loading="loading" @click="handleSubmit">建立</el-button>
          <el-button @click="router.push('/admin-account/list')">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
