<script setup lang="ts">
import { DrinkStatus, type DrinkOrder } from '@app/models'

// TODO: 後續呼叫 useApi()
// const { data, pending, error, refresh } = await useApi('/api/orders')

// 產生 20 筆 mock data
const mockData: DrinkOrder[] = Array.from({ length: 20 }, (_, index) => {
  const statuses = [DrinkStatus.PREPARING, DrinkStatus.READY, DrinkStatus.PICKED_UP]
  return {
    id: index + 1,
    name: `訂單 #${1000 + index + 1}`,
    status: statuses[index % statuses.length],
    price: Math.floor(Math.random() * 100) + 50,
    createdAt: new Date(Date.now() - index * 3600000).toLocaleString()
  }
})

const tableData = ref<DrinkOrder[]>(mockData)

const getStatusType = (status: DrinkStatus) => {
  switch (status) {
    case DrinkStatus.PREPARING:
      return 'warning'
    case DrinkStatus.READY:
      return 'success'
    case DrinkStatus.PICKED_UP:
      return 'info'
    default:
      return ''
  }
}
</script>

<template>
  <div class="page-container">
    <AppBreadcrumb />
    <el-card shadow="never" class="table-card">
      <template #header>
        <div class="card-header">
          <span>訂單列表 (DrinkOrder)</span>
          <el-button type="primary" @click="navigateTo('/products/create')">新增產品</el-button>
        </div>
      </template>

      <el-table :data="tableData" style="width: 100%" border stripe>
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="name" label="訂單名稱" min-width="150" />
        <el-table-column prop="status" label="狀態" width="120">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="price" label="價格" width="100">
          <template #default="{ row }">
            ${{ row.price }}
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="建立時間" min-width="180" />
        <el-table-column label="操作" width="150">
          <template #default>
            <el-button size="small">編輯</el-button>
            <el-button size="small" type="danger">刪除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-container">
        <el-pagination
          layout="prev, pager, next"
          :total="100"
          background
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.page-container {
  padding: 20px;
}

.table-card {
  margin-top: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}
</style>
