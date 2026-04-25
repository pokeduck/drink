<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'

const api = useAdminApi()
const router = useRouter()
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors, showSuccess, startLoading, stopLoading } = useApiFeedback()

const formRef = ref()

const form = reactive({
  name: '',
  sort: 0,
})

const rules = {
  name: [
    { required: true, message: '請輸入名稱', trigger: 'blur' },
    { max: 100, message: '名稱最多 100 字', trigger: 'blur' },
  ],
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  clearErrors()
  const { error } = await api.POST('/api/admin/sizes', { body: form })
  await stopLoading()
  if (error) { handleError(error, '新增失敗'); return }
  showSuccess('新增成功')
  router.push('/drink-option/size/list')
}
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never">
      <template #header>
        <div style="display: flex; align-items: center; gap: 8px">
          <el-button text @click="router.push('/drink-option/size/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
          <span>新增容量</span>
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
          <el-button type="primary" @click="handleSubmit">建立</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
