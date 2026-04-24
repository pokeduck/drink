<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'

const api = useAdminApi()
const router = useRouter()
const route = useRoute()
const memberId = Number(route.params.id)
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors, showSuccess, startLoading, stopLoading } = useApiFeedback()

const formRef = ref()
const fetchLoading = ref(true)

const form = reactive({
  name: '',
  avatar: '' as string | null,
  notification_type: 0,
  status: 1,
})

const email = ref('')
const emailVerified = ref(false)
const isGoogleConnected = ref(false)
const lastLoginAt = ref('')
const createdAt = ref('')
const updatedAt = ref('')

const rules = {
  name: [
    { required: true, message: '請輸入名稱', trigger: 'blur' },
    { max: 100, message: '名稱最多 100 字', trigger: 'blur' },
  ],
  notification_type: [{ required: true, message: '請選擇通知方式', trigger: 'change' }],
  status: [{ required: true, message: '請選擇狀態', trigger: 'change' }],
}

const notificationOptions = [
  { label: '不接收', value: 0 },
  { label: 'WebPush', value: 1 },
  { label: 'Email', value: 2 },
  { label: '全部', value: 3 },
]

const statusOptions = [
  { label: '啟用', value: 1 },
  { label: '停用', value: 2 },
]

const fetchMember = async () => {
  fetchLoading.value = true
  const { data: res, error } = await api.GET('/api/admin/members/{memberId}', {
    params: { path: { memberId } },
  })
  fetchLoading.value = false
  if (error) {
    handleError(error, '載入會員資料失敗')
    router.push('/member/list')
    return
  }
  const member = res!.data!
  form.name = member.name!
  form.avatar = member.avatar ?? null
  form.notification_type = member.notification_type!
  form.status = member.status!
  email.value = member.email!
  emailVerified.value = member.email_verified!
  isGoogleConnected.value = member.is_google_connected!
  lastLoginAt.value = member.last_login_at ? new Date(member.last_login_at).toLocaleString('zh-TW') : '從未登入'
  createdAt.value = new Date(member.created_at!).toLocaleString('zh-TW')
  updatedAt.value = new Date(member.updated_at!).toLocaleString('zh-TW')
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  clearErrors()
  const { error } = await api.PUT('/api/admin/members/{memberId}', {
    params: { path: { memberId } },
    body: form,
  })
  await stopLoading()
  if (error) { handleError(error, '更新失敗'); return }
  showSuccess('更新成功')
  router.push('/member/list')
}

onMounted(() => {
  fetchMember()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/member/list')">
      <template #content>編輯會員</template>
    </el-page-header>

    <el-card v-loading="fetchLoading" shadow="never" style="margin-top: 16px">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large">
        <el-row :gutter="24">
          <!-- 唯讀欄位 -->
          <el-col :span="24">
            <el-form-item label="ID">
              <el-input :model-value="String(memberId)" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="Email">
              <el-input :model-value="email" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="Email 驗證">
              <el-tag :type="emailVerified ? 'success' : 'info'" size="default">
                {{ emailVerified ? '已驗證' : '未驗證' }}
              </el-tag>
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="Google 綁定">
              <el-tag :type="isGoogleConnected ? 'success' : 'info'" size="default">
                {{ isGoogleConnected ? '已綁定' : '未綁定' }}
              </el-tag>
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="最後登入">
              <el-input :model-value="lastLoginAt" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="建立時間">
              <el-input :model-value="createdAt" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="更新時間">
              <el-input :model-value="updatedAt" disabled />
            </el-form-item>
          </el-col>

          <!-- 可編輯欄位 -->
          <el-col :span="24">
            <el-form-item label="名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入名稱" maxlength="100" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="頭像 URL">
              <el-input v-model="form.avatar" placeholder="頭像 URL（可留空）" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="通知方式" prop="notification_type">
              <el-select v-model="form.notification_type" style="width: 100%">
                <el-option v-for="opt in notificationOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="狀態" prop="status">
              <el-select v-model="form.status" style="width: 100%">
                <el-option v-for="opt in statusOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item>
          <el-button type="primary" @click="handleSubmit">儲存</el-button>
          <el-button @click="router.push('/member/list')">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>
