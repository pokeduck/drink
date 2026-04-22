<script setup lang="ts">
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiError } from '~/composable/useApiError'
import { useFormLayout } from '~/composable/useFormLayout'
import { useLoading } from '~/composable/useLoading'
import type { components } from '@app/api-types/admin'

type Ice = components['schemas']['IceListResponse']

const api = useAdminApi()
const { serverErrors, handleError, clearErrors } = useApiError()
const { labelPosition } = useFormLayout()

// 搜尋 & 分頁
const keyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const tableKey = ref(0)

// 排序
const sortBy = ref('sort')
const sortOrder = ref('asc')

// 資料
const tableData = ref<Ice[]>([])
const { loading, start: startLoading, stop: stopLoading } = useLoading()

// 多選
const selectedRows = ref<Ice[]>([])

const fetchList = async () => {
  startLoading()
  try {
    const { data: res } = await api.GET('/api/admin/ices', {
      params: {
        query: {
          page: page.value,
          page_size: pageSize.value,
          sort_by: sortBy.value,
          sort_order: sortOrder.value,
          keyword: keyword.value || undefined,
        },
      },
    })
    tableData.value = res?.data?.items ?? []
    total.value = res?.data?.total ?? 0
    tableKey.value++
  } catch (err) {
    console.error('Failed to fetch ices:', err)
  } finally {
    stopLoading()
  }
}

const handleSearch = () => {
  page.value = 1
  fetchList()
}

const handleSortChange = ({ prop, order }: { prop: string; order: string | null }) => {
  if (order) {
    sortBy.value = prop
    sortOrder.value = order === 'ascending' ? 'asc' : 'desc'
  } else {
    sortBy.value = 'sort'
    sortOrder.value = 'asc'
  }
  page.value = 1
  fetchList()
}

const handleSelectionChange = (rows: Ice[]) => {
  selectedRows.value = rows
}

// ---- 新增 / 編輯 Dialog ----
const dialogVisible = ref(false)
const dialogTitle = ref('新增冰塊')
const editingId = ref<number | null>(null)
const formRef = ref()
const { loading: formLoading, start: startFormLoading, stop: stopFormLoading } = useLoading()
const form = reactive({
  name: '',
  sort: 0,
})
const rules = {
  name: [{ required: true, message: '請輸入名稱', trigger: 'blur' }],
}

const openCreate = () => {
  editingId.value = null
  dialogTitle.value = '新增冰塊'
  form.name = ''
  form.sort = 0
  dialogVisible.value = true
  nextTick(() => formRef.value?.clearValidate())
}

const openEdit = (row: Ice) => {
  editingId.value = row.id!
  dialogTitle.value = '編輯冰塊'
  form.name = row.name!
  form.sort = row.sort!
  dialogVisible.value = true
  nextTick(() => formRef.value?.clearValidate())
}

const handleSubmit = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  startFormLoading()
  clearErrors()
  try {
    if (editingId.value) {
      const { error } = await api.PUT('/api/admin/ices/{id}', {
        params: { path: { id: editingId.value } },
        body: form,
      })
      if (error) { handleError(error, '更新失敗'); stopFormLoading(); return }
      ElMessage.success('更新成功')
    } else {
      const { error } = await api.POST('/api/admin/ices', { body: form })
      if (error) { handleError(error, '新增失敗'); stopFormLoading(); return }
      ElMessage.success('新增成功')
    }
    dialogVisible.value = false
    await fetchList()
  } finally {
    stopFormLoading()
  }
}

// ---- 刪除 ----
const handleDelete = async (row: Ice) => {
  try {
    await ElMessageBox.confirm(`確定要刪除「${row.name}」嗎？`, '刪除確認', {
      type: 'warning',
      confirmButtonText: '刪除',
      cancelButtonText: '取消',
    })
    const { error } = await api.DELETE('/api/admin/ices/{id}', {
      params: { path: { id: row.id! } },
    })
    if (error) { handleError(error, '刪除失敗'); return }
    ElMessage.success('刪除成功')
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') {
      handleError(err, '刪除失敗')
    }
  }
}

// ---- 批次刪除 ----
const handleBatchDelete = async () => {
  if (!selectedRows.value.length) return
  try {
    const names = selectedRows.value.map((r) => r.name).join('、')
    await ElMessageBox.confirm(`確定要刪除「${names}」嗎？`, '批次刪除確認', {
      type: 'warning',
      confirmButtonText: '刪除',
      cancelButtonText: '取消',
    })
    const { error } = await api.DELETE('/api/admin/ices/batch', {
      body: { ids: selectedRows.value.map((r) => r.id!) },
    })
    if (error) { handleError(error, '批次刪除失敗'); return }
    ElMessage.success('批次刪除成功')
    selectedRows.value = []
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') {
      handleError(err, '批次刪除失敗')
    }
  }
}

// ---- 儲存排序 ----
const { loading: sortLoading, start: startSortLoading, stop: stopSortLoading } = useLoading()

const handleSaveSort = async () => {
  startSortLoading()
  try {
    const items = tableData.value.map((row) => ({
      id: row.id!,
      sort: row.sort!,
    }))
    const { error } = await api.PUT('/api/admin/ices/sort', { body: { items } })
    if (error) { handleError(error, '排序失敗'); return }
    ElMessage.success('排序儲存成功')
    await fetchList()
  } finally {
    stopSortLoading()
  }
}

onMounted(() => {
  fetchList()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never" v-loading="loading || sortLoading">
      <!-- 工具列 -->
      <div class="toolbar">
        <div class="toolbar-left">
          <el-input
            v-model="keyword"
            placeholder="搜尋冰塊"
            clearable
            style="width: 240px"
            @keyup.enter="handleSearch"
            @clear="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <el-button type="primary" @click="handleSearch">查詢</el-button>
        </div>
        <div class="toolbar-right">
          <el-button
            v-if="selectedRows.length"
            type="danger"
            @click="handleBatchDelete"
          >
            批次刪除 ({{ selectedRows.length }})
          </el-button>
          <el-button @click="handleSaveSort">
            儲存排序
          </el-button>
          <el-button type="primary" icon="Plus" @click="openCreate">
            新增冰塊
          </el-button>
        </div>
      </div>

      <!-- 表格 -->
      <el-table
        :key="tableKey"
        :data="tableData"
        stripe
        style="width: 100%"
        row-key="id"
        @selection-change="handleSelectionChange"
        @sort-change="handleSortChange"
      >
        <el-table-column type="selection" width="45" />
        <el-table-column prop="id" label="ID" width="80" sortable="custom" />
        <el-table-column prop="name" label="名稱" min-width="200" />
        <el-table-column label="排序" width="120">
          <template #default="{ row }">
            <el-input-number v-model="row.sort" :min="0" :precision="0" controls-position="right" size="small" style="width: 90px" />
          </template>
        </el-table-column>
        <el-table-column prop="created_at" label="建立時間" width="180" sortable="custom">
          <template #default="{ row }">
            {{ new Date(row.created_at).toLocaleString('zh-TW') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openEdit(row)">編輯</el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)">刪除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <AppPagination v-model:page="page" v-model:page-size="pageSize" :total="total" @change="fetchList" />
    </el-card>

    <!-- 新增 / 編輯 Dialog -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="480" :close-on-click-modal="false">
      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="80px" v-loading="formLoading">
        <el-row :gutter="20">
          <el-col :span="24">
            <el-form-item label="名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入名稱" maxlength="50" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="排序" prop="sort">
              <el-input-number v-model="form.sort" :min="0" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">確認</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.toolbar-left {
  display: flex;
  gap: 12px;
  align-items: center;
}

.toolbar-right {
  display: flex;
  gap: 12px;
  align-items: center;
}
</style>
