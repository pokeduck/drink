<script setup lang="ts">
import { formatDateTime } from '~/utils/format'
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useLoading } from '~/composable/useLoading'
import { useFormLayout } from '~/composable/useFormLayout'
import type { components } from '@app/api-types/admin'

type OrderListItem = components['schemas']['AdminOrderListItemResponse']
type ShopListItem = components['schemas']['ShopListResponse']

const api = useAdminApi()
const router = useRouter()
const { labelPosition } = useFormLayout()
const { loading, start: startListLoading, stop: stopListLoading } = useLoading()

// 搜尋 / 篩選 / 分頁
const keyword = ref('')
const statusFilter = ref<number | undefined>(undefined)
const shopIdFilter = ref<number | undefined>(undefined)
const createdRange = ref<[string, string] | null>(null)
const deadlineRange = ref<[string, string] | null>(null)

const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

// 排序
const sortBy = ref<string | undefined>(undefined)
const sortOrder = ref<string | undefined>(undefined)

const tableData = ref<OrderListItem[]>([])
const tableKey = ref(0)

const statusOptions = [
  { label: '進行中', value: 1 },
  { label: '已截止', value: 2 },
  { label: '配送中', value: 3 },
  { label: '已完成', value: 4 },
  { label: '已取消', value: 5 },
]

// shop 下拉
const shopOptions = ref<ShopListItem[]>([])
const loadShops = async () => {
  try {
    const { data: res } = await api.GET('/api/admin/shops', {
      params: { query: { page: 1, page_size: 100 } as any },
    })
    shopOptions.value = res?.data?.items ?? []
  } catch (err) {
    console.error('Failed to load shops:', err)
  }
}

const fetchList = async () => {
  startListLoading()
  try {
    const query: Record<string, unknown> = {
      page: page.value,
      page_size: pageSize.value,
      keyword: keyword.value || undefined,
      status: statusFilter.value,
      shop_id: shopIdFilter.value,
    }
    if (sortBy.value && sortOrder.value) {
      query.sort_by = sortBy.value
      query.sort_order = sortOrder.value
    }
    if (createdRange.value) {
      query.created_from = createdRange.value[0]
      query.created_to = createdRange.value[1]
    }
    if (deadlineRange.value) {
      query.deadline_from = deadlineRange.value[0]
      query.deadline_to = deadlineRange.value[1]
    }
    const { data: res } = await api.GET('/api/admin/orders', {
      params: { query: query as any },
    })
    tableData.value = res?.data?.items ?? []
    total.value = res?.data?.total ?? 0
    tableKey.value++
  } catch (err) {
    console.error('Failed to fetch orders:', err)
  } finally {
    stopListLoading()
  }
}

const handleSearch = () => {
  page.value = 1
  fetchList()
}

const handleReset = () => {
  keyword.value = ''
  statusFilter.value = undefined
  shopIdFilter.value = undefined
  createdRange.value = null
  deadlineRange.value = null
  sortBy.value = undefined
  sortOrder.value = undefined
  page.value = 1
  fetchList()
}

const handleSortChange = ({ prop, order }: { prop: string; order: string | null }) => {
  if (order) {
    sortBy.value = prop
    sortOrder.value = order === 'ascending' ? 'asc' : 'desc'
  } else {
    sortBy.value = undefined
    sortOrder.value = undefined
  }
  page.value = 1
  fetchList()
}

const formatAmount = (value: number | undefined) => {
  if (value == null) return '$ 0'
  return `$ ${value.toFixed(0)}`
}

onMounted(() => {
  loadShops()
  fetchList()
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never" v-loading="loading">
      <template #header>訂單列表</template>

      <!-- 篩選列 -->
      <el-form inline :label-position="labelPosition" :label-width="labelPosition === 'right' ? '90px' : undefined" class="filter-form">
        <el-form-item label="關鍵字">
          <el-input
            v-model="keyword"
            placeholder="標題 / 發起人"
            clearable
            style="width: 200px"
            @keyup.enter="handleSearch"
            @clear="handleSearch"
          />
        </el-form-item>
        <el-form-item label="狀態">
          <el-select
            v-model="statusFilter"
            placeholder="全部"
            clearable
            style="width: 140px"
          >
            <el-option v-for="opt in statusOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="店家">
          <el-select
            v-model="shopIdFilter"
            placeholder="全部"
            clearable
            filterable
            style="width: 180px"
          >
            <el-option v-for="shop in shopOptions" :key="shop.id" :label="shop.name" :value="shop.id!" />
          </el-select>
        </el-form-item>
        <el-form-item label="建立時間">
          <el-date-picker
            v-model="createdRange"
            type="daterange"
            range-separator="至"
            start-placeholder="開始日"
            end-placeholder="結束日"
            value-format="YYYY-MM-DD"
            style="width: 260px"
          />
        </el-form-item>
        <el-form-item label="截止時間">
          <el-date-picker
            v-model="deadlineRange"
            type="daterange"
            range-separator="至"
            start-placeholder="開始日"
            end-placeholder="結束日"
            value-format="YYYY-MM-DD"
            style="width: 260px"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查詢</el-button>
          <el-button @click="handleReset">重設</el-button>
        </el-form-item>
      </el-form>

      <!-- 表格 -->
      <el-table
        :key="tableKey"
        :data="tableData"
        stripe
        style="width: 100%"
        row-key="id"
        @sort-change="handleSortChange"
      >
        <el-table-column prop="id" label="ID" width="80" sortable="custom" />
        <el-table-column prop="title" label="標題" min-width="180" show-overflow-tooltip />
        <el-table-column prop="shop_name" label="店家" min-width="140" show-overflow-tooltip />
        <el-table-column prop="initiator_name" label="發起人" width="120" show-overflow-tooltip />
        <el-table-column label="狀態" width="110" align="center">
          <template #default="{ row }">
            <OrderStatusTag :status="row.status" />
          </template>
        </el-table-column>
        <el-table-column prop="deadline" label="截止時間" width="160" sortable="custom">
          <template #default="{ row }">
            {{ formatDateTime(row.deadline) }}
          </template>
        </el-table-column>
        <el-table-column prop="order_item_count" label="飲料數" width="90" align="center" />
        <el-table-column prop="total_amount" label="總金額" width="110" align="right">
          <template #default="{ row }">
            {{ formatAmount(row.total_amount) }}
          </template>
        </el-table-column>
        <el-table-column prop="created_at" label="建立時間" width="160" sortable="custom">
          <template #default="{ row }">
            {{ formatDateTime(row.created_at) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="100" fixed="right" align="center">
          <template #default="{ row }">
            <el-button size="small" @click="router.push(`/order/list/${row.id}`)">檢視</el-button>
          </template>
        </el-table-column>
      </el-table>

      <AppPagination v-model:page="page" v-model:page-size="pageSize" :total="total" @change="fetchList" />
    </el-card>
  </div>
</template>

<style scoped>
.filter-form {
  margin-bottom: 8px;
}

.filter-form :deep(.el-form-item) {
  margin-bottom: 12px;
}
</style>
