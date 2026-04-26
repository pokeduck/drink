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
  external_url: '',
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
  const { data: res, error } = await api.POST('/api/admin/shops', {
    body: {
      name: form.name,
      phone: form.phone || undefined,
      address: form.address || undefined,
      note: form.note || undefined,
      external_url: form.external_url || null,
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
  const newId = res?.data?.id
  if (newId) router.push(`/shop/${newId}/edit`)
  else router.push('/shop/list')
}
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never">
      <template #header>
        <div style="display: flex; align-items: center; gap: 8px">
          <el-button text @click="router.push('/shop/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
          <span>新增店家</span>
        </div>
      </template>
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
            <el-form-item label="封面圖片">
              <FormHint>店家建立後可在編輯頁上傳封面圖片</FormHint>
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="外部連結" prop="external_url" :error="serverErrors.external_url">
              <el-input v-model="form.external_url" placeholder="例如 https://maps.google.com/..." maxlength="500" />
              <FormHint>僅接受 http 或 https 連結（例如 Google Map 分享連結）</FormHint>
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
              <el-input-number v-model="form.sort" :min="0" :precision="0" style="width: 180px; max-width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="每種加料上限" prop="max_topping_per_item">
              <el-input-number v-model="form.max_topping_per_item" :min="1" :precision="0" style="width: 180px; max-width: 100%" />
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
