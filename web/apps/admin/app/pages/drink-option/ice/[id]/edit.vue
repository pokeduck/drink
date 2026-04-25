<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useUnsavedGuard } from '~/composable/useUnsavedGuard'

const api = useAdminApi()
const router = useRouter()
const route = useRoute()
const id = Number(route.params.id)
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors, showSuccess, startLoading, stopLoading } = useApiFeedback()

const formRef = ref()
const fetchLoading = ref(true)
const createdAt = ref('')
const updatedAt = ref('')

const form = reactive({
  name: '',
  sort: 0,
})

const { takeSnapshot } = useUnsavedGuard(form)

const rules = {
  name: [
    { required: true, message: '請輸入名稱', trigger: 'blur' },
    { max: 100, message: '名稱最多 100 字', trigger: 'blur' },
  ],
}

const fetchData = async () => {
  fetchLoading.value = true
  const { data: res, error } = await api.GET('/api/admin/ices/{id}', {
    params: { path: { id } },
  })
  fetchLoading.value = false
  if (error) {
    handleError(error, '載入資料失敗')
    router.push('/drink-option/ice/list')
    return
  }
  const item = res!.data!
  form.name = item.name!
  form.sort = item.sort!
  createdAt.value = item.created_at!
  updatedAt.value = item.updated_at!
  takeSnapshot()
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  clearErrors()
  const { error } = await api.PUT('/api/admin/ices/{id}', {
    params: { path: { id } },
    body: form,
  })
  await stopLoading()
  if (error) { handleError(error, '更新失敗'); return }
  showSuccess('更新成功')
  takeSnapshot()
  router.push('/drink-option/ice/list')
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card v-loading="fetchLoading" shadow="never">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <div style="display: flex; align-items: center; gap: 8px">
            <el-button text @click="router.push('/drink-option/ice/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
            <span>編輯冰塊</span>
          </div>
          <AppTimestamp v-if="createdAt" :created-at="createdAt" :updated-at="updatedAt" />
        </div>
      </template>
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="80px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入名稱" maxlength="100" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="排序" prop="sort">
              <el-input-number v-model="form.sort" :min="0" :precision="0" style="width: 180px; max-width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item>
          <el-button type="primary" @click="handleSubmit">儲存</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
