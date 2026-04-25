<script setup lang="ts">
import { formatDateTime } from '~/utils/format'
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useLoading } from '~/composable/useLoading'
import { usePermission } from '~/composable/usePermission'
import { MENU } from '@app/core'
import type { components } from '@app/api-types/admin'

type Shop = components['schemas']['ShopListResponse']

const api = useAdminApi()
const router = useRouter()
const { handleError, showSuccess, startLoading, stopLoading } = useApiFeedback()
const { can } = usePermission()

// 搜尋 & 分頁
const keyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const tableKey = ref(0)

// 排序
const sortBy = ref('sort')
const sortOrder = ref('asc')

// 篩選
const statusFilter = ref<number | undefined>(undefined)

// 資料
const tableData = ref<Shop[]>([])
const { loading, start: startListLoading, stop: stopListLoading } = useLoading()

// 多選
const selectedRows = ref<Shop[]>([])

const statusOptions = [
  { label: '全部', value: undefined },
  { label: '上架', value: 1 },
  { label: '下架', value: 2 },
]

const statusMap: Record<number, string> = {
  1: '上架',
  2: '下架',
}

const fetchList = async () => {
  startListLoading()
  try {
    const { data: res } = await api.GET('/api/admin/shops', {
      params: {
        query: {
          page: page.value,
          page_size: pageSize.value,
          sort_by: sortBy.value,
          sort_order: sortOrder.value,
          keyword: keyword.value || undefined,
          status: statusFilter.value,
        },
      },
    })
    tableData.value = res?.data?.items ?? []
    total.value = res?.data?.total ?? 0
    tableKey.value++
  } catch (err) {
    console.error('Failed to fetch shops:', err)
  } finally {
    stopListLoading()
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

// 多選
const handleSelectionChange = (rows: Shop[]) => {
  selectedRows.value = rows
}

// ---- 刪除 ----
const handleDelete = async (row: Shop) => {
  try {
    await ElMessageBox.confirm(`確定要刪除「${row.name}」嗎？`, '刪除確認', {
      type: 'warning',
      confirmButtonText: '刪除',
      cancelButtonText: '取消',
    })
    startLoading()
    const { error } = await api.DELETE('/api/admin/shops/{id}', {
      params: { path: { id: row.id! } },
    })
    await stopLoading()
    if (error) { handleError(error, '刪除失敗'); return }
    showSuccess('刪除成功')
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
    startLoading()
    const { error } = await api.DELETE('/api/admin/shops/batch', {
      body: { ids: selectedRows.value.map((r) => r.id!) },
    })
    await stopLoading()
    if (error) { handleError(error, '批次刪除失敗'); return }
    showSuccess('批次刪除成功')
    selectedRows.value = []
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') {
      handleError(err, '批次刪除失敗')
    }
  }
}

// ---- 儲存排序 ----
const handleSaveSort = async () => {
  startLoading()
  try {
    const items = tableData.value.map((row) => ({
      id: row.id!,
      sort: row.sort!,
    }))
    const { error } = await api.PUT('/api/admin/shops/sort', { body: { items } })
    await stopLoading()
    if (error) { handleError(error, '排序失敗'); return }
    showSuccess('排序儲存成功')
    await fetchList()
  } catch {
    await stopLoading()
  }
}

onMounted(() => {
  fetchList()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never" v-loading="loading">
      <!-- 工具列 -->
      <div class="toolbar">
        <div class="toolbar-left">
          <el-input
            v-model="keyword"
            placeholder="搜尋店家名稱"
            clearable
            style="width: 240px"
            @keyup.enter="handleSearch"
            @clear="handleSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <el-select
            v-model="statusFilter"
            placeholder="狀態"
            clearable
            style="width: 120px"
            @change="handleSearch"
          >
            <el-option
              v-for="opt in statusOptions"
              :key="String(opt.value)"
              :label="opt.label"
              :value="opt.value"
            />
          </el-select>
          <el-button type="primary" @click="handleSearch">查詢</el-button>
        </div>
        <div class="toolbar-right">
          <el-button
            v-if="selectedRows.length && can(MENU.ShopList, 'delete')"
            type="danger"
            @click="handleBatchDelete"
          >
            批次刪除 ({{ selectedRows.length }})
          </el-button>
          <el-button v-if="can(MENU.ShopList, 'update')" @click="handleSaveSort">
            儲存排序
          </el-button>
          <el-button v-if="can(MENU.ShopList, 'create')" type="primary" icon="Plus" @click="router.push('/shop/create')">
            新增店家
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
        <el-table-column prop="name" label="名稱" min-width="150" />
        <el-table-column prop="phone" label="電話" width="150" />
        <el-table-column prop="address" label="地址" min-width="200" show-overflow-tooltip />
        <el-table-column prop="status" label="狀態" width="90">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'info'">
              {{ statusMap[row.status] ?? '未知' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="category_count" label="分類數" width="90" align="center" />
        <el-table-column prop="menu_item_count" label="品項數" width="90" align="center" />
        <el-table-column label="排序" width="150">
          <template #default="{ row }">
            <el-input-number v-if="can(MENU.ShopList, 'update')" v-model="row.sort" :min="0" :precision="0" style="width: 120px" />
            <span v-else>{{ row.sort }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="created_at" label="建立時間" width="160" sortable="custom">
          <template #default="{ row }">
            {{ formatDateTime(row.created_at) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button v-if="can(MENU.ShopList, 'update')" size="small" @click="router.push(`/shop/${row.id}/edit`)">編輯</el-button>
            <el-button v-if="can(MENU.ShopList, 'delete')" size="small" type="danger" @click="handleDelete(row)">刪除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分頁 -->
      <AppPagination v-model:page="page" v-model:page-size="pageSize" :total="total" @change="fetchList" />
    </el-card>
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
