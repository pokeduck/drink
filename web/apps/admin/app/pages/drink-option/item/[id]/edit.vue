<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useUnsavedGuard } from '~/composable/useUnsavedGuard'

const api = useAdminApi()
const router = useRouter()
const route = useRoute()
const itemId = Number(route.params.id)
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
  name: [{ required: true, message: '請輸入名稱', trigger: 'blur' }],
}

const fetchItem = async () => {
  fetchLoading.value = true
  const { data: res, error } = await api.GET('/api/admin/drink-items/{id}', {
    params: { path: { id: itemId } },
  })

  if (error) {
    handleError(error, '載入品名資料失敗')
    router.push('/drink-option/item/list')
    return
  }

  const item = res!.data!
  form.name = item.name!
  form.sort = item.sort!
  createdAt.value = item.created_at!
  updatedAt.value = item.updated_at!
  fetchLoading.value = false
  takeSnapshot()
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  clearErrors()
  startLoading()
  const { error } = await api.PUT('/api/admin/drink-items/{id}', {
    params: { path: { id: itemId } },
    body: {
      name: form.name,
      sort: form.sort,
    },
  })
  await stopLoading()

  if (error) {
    handleError(error, '更新失敗')
    return
  }
  showSuccess('更新成功')
  takeSnapshot()
  router.push('/drink-option/item/list')
}

onMounted(() => {
  fetchItem()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card v-loading="fetchLoading" shadow="never">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <div style="display: flex; align-items: center; gap: 8px">
            <el-button text @click="router.push('/drink-option/item/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
            <span>編輯通用品名</span>
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
