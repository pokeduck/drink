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
  phone: '',
  address: '',
  note: '',
  status: 1,
  sort: 0,
  max_topping_per_item: 1,
})

const rules = {
  name: [{ required: true, message: '請輸入店家名稱', trigger: 'blur' }],
  status: [{ required: true, message: '請選擇狀態', trigger: 'change' }],
  max_topping_per_item: [{ required: true, message: '請輸入每種加料上限', trigger: 'blur' }],
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  clearErrors()
  startLoading()
  const { error } = await api.POST('/api/admin/shops', {
    body: {
      name: form.name,
      phone: form.phone || undefined,
      address: form.address || undefined,
      note: form.note || undefined,
      status: form.status,
      sort: form.sort,
      max_topping_per_item: form.max_topping_per_item,
    },
  })
  await stopLoading()

  if (error) {
    handleError(error, '新增失敗')
    return
  }
  showSuccess('新增成功')
  router.push('/shop/list')
}
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/shop/list')">
      <template #content>新增店家</template>
    </el-page-header>

    <el-card shadow="never" style="margin-top: 16px">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="120px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="店家名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入店家名稱" maxlength="100" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="電話" prop="phone">
              <el-input v-model="form.phone" placeholder="請輸入聯絡電話" maxlength="20" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="地址" prop="address">
              <el-input v-model="form.address" placeholder="請輸入地址" maxlength="200" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="備註" prop="note">
              <el-input v-model="form.note" type="textarea" :rows="3" placeholder="請輸入備註" maxlength="500" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="狀態" prop="status">
              <el-radio-group v-model="form.status">
                <el-radio :value="1">上架</el-radio>
                <el-radio :value="2">下架</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="排序" prop="sort">
              <el-input-number v-model="form.sort" :min="0" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="每種加料上限" prop="max_topping_per_item">
              <el-input-number v-model="form.max_topping_per_item" :min="1" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item>
          <el-button type="primary" @click="handleSubmit">建立</el-button>
          <el-button @click="router.push('/shop/list')">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
