<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useUnsavedGuard } from '~/composable/useUnsavedGuard'

const api = useAdminApi()
const router = useRouter()
const route = useRoute()
const toppingId = Number(route.params.id)
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors, showSuccess, startLoading, stopLoading } = useApiFeedback()

const formRef = ref()
const fetchLoading = ref(true)
const createdAt = ref('')
const updatedAt = ref('')

const form = reactive({
  name: '',
  default_price: 0,
  sort: 0,
})

const { takeSnapshot } = useUnsavedGuard(form)

const rules = {
  name: [{ required: true, message: '請輸入名稱', trigger: 'blur' }],
}

const fetchTopping = async () => {
  fetchLoading.value = true
  const { data: res, error } = await api.GET('/api/admin/toppings/{id}', {
    params: { path: { id: toppingId } },
  })
  fetchLoading.value = false
  if (error) {
    handleError(error, '載入加料資料失敗')
    router.push('/drink-option/topping/list')
    return
  }
  const topping = res!.data!
  form.name = topping.name!
  form.default_price = topping.default_price!
  form.sort = topping.sort!
  createdAt.value = topping.created_at!
  updatedAt.value = topping.updated_at!
  takeSnapshot()
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  clearErrors()
  const { error } = await api.PUT('/api/admin/toppings/{id}', {
    params: { path: { id: toppingId } },
    body: form,
  })
  await stopLoading()
  if (error) { handleError(error, '更新失敗'); return }
  showSuccess('更新成功')
  takeSnapshot()
  router.push('/drink-option/topping/list')
}

onMounted(() => {
  fetchTopping()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card v-loading="fetchLoading" shadow="never">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <div style="display: flex; align-items: center; gap: 8px">
            <el-button text @click="router.push('/drink-option/topping/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
            <span>編輯加料</span>
          </div>
          <AppTimestamp v-if="createdAt" :created-at="createdAt" :updated-at="updatedAt" />
        </div>
      </template>
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入名稱" maxlength="50" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="預設價格" prop="default_price">
              <el-input-number v-model="form.default_price" :min="0" :precision="0" style="width: 180px; max-width: 100%" />
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
