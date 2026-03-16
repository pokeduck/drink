<script setup lang="ts">
import { DrinkStatus } from '@app/models'
import { ArrowLeft } from '@element-plus/icons-vue'

const router = useRouter()

const form = reactive({
  name: '',
  price: 0,
  status: DrinkStatus.PREPARING,
  description: ''
})

const rules = {
  name: [{ required: true, message: '請輸入產品名稱', trigger: 'blur' }],
  price: [{ required: true, message: '請輸入產品價格', trigger: 'blur' }]
}

const formRef = ref()

const handleSubmit = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate((valid: boolean) => {
    if (valid) {
      console.log('提交資料:', form)
      ElMessage.success('產品建立成功 (Mock)')
      router.push('/products/list')
    }
  })
}

const handleBack = () => {
  router.back()
}
</script>

<template>
  <div class="page-container">
    <AppBreadcrumb />
    
    <el-page-header :icon="ArrowLeft" @back="handleBack" class="page-header">
      <template #content>
        <span class="text-large font-600 mr-3"> 新增產品 </span>
      </template>
    </el-page-header>

    <el-card shadow="never" class="form-card">
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="100px"
        label-position="left"
        style="max-width: 600px"
      >
        <el-form-item label="產品名稱" prop="name">
          <el-input v-model="form.name" placeholder="例如: 珍珠奶茶" />
        </el-form-item>

        <el-form-item label="產品價格" prop="price">
          <el-input-number v-model="form.price" :min="0" style="width: 200px" />
        </el-form-item>

        <el-form-item label="初始狀態" prop="status">
          <el-select v-model="form.status" placeholder="選擇狀態">
            <el-option label="準備中" :value="DrinkStatus.PREPARING" />
            <el-option label="可取餐" :value="DrinkStatus.READY" />
            <el-option label="已取餐" :value="DrinkStatus.PICKED_UP" />
          </el-select>
        </el-form-item>

        <el-form-item label="備註內容" prop="description">
          <el-input v-model="form.description" type="textarea" rows="4" placeholder="產品詳細描述..." />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="handleSubmit">立即建立</el-button>
          <el-button @click="handleBack">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.page-container {
  padding: 20px;
}

.page-header {
  margin-bottom: 20px;
}

.form-card {
  padding: 20px;
}

.text-large {
  font-size: 18px;
}

.font-600 {
  font-weight: 600;
}
</style>
