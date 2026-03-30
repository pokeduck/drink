<script setup lang="ts">
import { useApi } from '~/composable/useApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiError } from '~/composable/useApiError'
import { useLoading } from '~/composable/useLoading'

interface MenuCrudItem {
  menu_id: number
  menu_name: string
  can_read: boolean
  can_create: boolean
  can_update: boolean
  can_delete: boolean
}

interface ApiResponse<T> {
  data: T
  code: number
}

const api = useApi()
const router = useRouter()
const { labelPosition } = useFormLayout()
const { handleError } = useApiError()

const formRef = ref()
const { loading, start: startLoading, stop: stopLoading } = useLoading()
const fetchLoading = ref(true)

const form = reactive({
  name: '',
})

const rules = {
  name: [
    { required: true, message: '請輸入角色名稱', trigger: 'blur' },
    { max: 50, message: '角色名稱最多 50 字', trigger: 'blur' },
  ],
}

// Menu CRUD 矩陣
const menuCrudList = ref<MenuCrudItem[]>([])

const fetchMenus = async () => {
  fetchLoading.value = true
  try {
    // 用一個不存在的 roleId=0 或新建時取得空白矩陣
    // 透過取得任意角色的 menus 結構，只要 menu list
    // 但 spec 說 GET /api/admin/roles/{roleId} 回傳所有 Menu CRUD
    // 新建時可以先 GET 一個現有 role 取 menu 結構，或後端提供 menu list API
    // 最簡方式：GET /api/admin/roles/1 取得 menus 結構，全部設 false
    const res = await api.get<ApiResponse<{ menus: MenuCrudItem[] }>>('/admin/roles/1')
    menuCrudList.value = res.data.menus.map((m) => ({
      ...m,
      can_read: false,
      can_create: false,
      can_update: false,
      can_delete: false,
    }))
  } catch {
    ElMessage.error('載入 Menu 資料失敗')
  } finally {
    fetchLoading.value = false
  }
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startLoading()
  try {
    await api.post('/admin/roles', {
      name: form.name,
      menus: menuCrudList.value.map((m) => ({
        menu_id: m.menu_id,
        can_read: m.can_read,
        can_create: m.can_create,
        can_update: m.can_update,
        can_delete: m.can_delete,
      })),
    })
    ElMessage.success('角色建立成功')
    router.push('/admin-account/role')
  } catch (err: any) {
    handleError(err, formRef.value, '建立失敗')
  } finally {
    stopLoading()
  }
}

onMounted(() => {
  fetchMenus()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-page-header title="返回上一頁" @back="router.push('/admin-account/role')">
      <template #content>新增角色</template>
    </el-page-header>

    <el-card v-loading="fetchLoading || loading" shadow="never" style="margin-top: 16px">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="100px" size="large" @submit.prevent>
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="角色名稱" prop="name">
              <el-input v-model="form.name" placeholder="請輸入角色名稱" maxlength="50" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>

      <!-- Menu CRUD 矩陣 -->
      <h4 style="margin: 24px 0 12px">Menu 權限設定</h4>
      <el-table :data="menuCrudList" stripe border style="width: 100%">
        <el-table-column prop="menu_name" label="功能頁面" min-width="200" />
        <el-table-column label="讀取" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_read" />
          </template>
        </el-table-column>
        <el-table-column label="新增" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_create" />
          </template>
        </el-table-column>
        <el-table-column label="修改" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_update" />
          </template>
        </el-table-column>
        <el-table-column label="刪除" width="80" align="center">
          <template #default="{ row }">
            <el-checkbox v-model="row.can_delete" />
          </template>
        </el-table-column>
      </el-table>

      <div style="margin-top: 24px">
        <el-button type="primary" @click="handleSubmit">建立</el-button>
        <el-button @click="router.push('/admin-account/role')">取消</el-button>
      </div>
    </el-card>
  </div>
</template>
