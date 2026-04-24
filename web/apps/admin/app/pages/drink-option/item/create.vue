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
  name: [{ required: true, message: '請輸入名稱', trigger: 'blur' }],
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  clearErrors()
  startLoading()
  const { error } = await api.POST('/api/admin/drink-items', {
    body: {
      name: form.name,
      sort: form.sort,
    },
  })
  await stopLoading()

  if (error) {
    handleError(error, '新增失敗')
    return
  }
  showSuccess('新增成功')
  router.push('/drink-option/item/list')
}
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/drink-option/item/list')">
      <template #content>新增通用品名</template>
    </el-page-header>

    <el-card shadow="never" style="margin-top: 16px">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="80px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入名稱" maxlength="100" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="排序" prop="sort">
              <el-input-number v-model="form.sort" :min="0" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item>
          <el-button type="primary" @click="handleSubmit">建立</el-button>
          <el-button @click="router.push('/drink-option/item/list')">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
