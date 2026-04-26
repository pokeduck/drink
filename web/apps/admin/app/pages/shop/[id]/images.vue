<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ArrowLeft, Plus, Upload as UploadIcon } from '@element-plus/icons-vue'
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useLoading } from '~/composable/useLoading'
import { useAssetHost } from '~/composable/useAssetHost'
import { useImageUploadQueue } from '~/composable/useImageUploadQueue'
import ShopImagePoolGrid from '~/components/ShopImagePoolGrid.vue'
import ShopImageDrawer from '~/components/ShopImageDrawer.vue'

const route = useRoute()
const router = useRouter()
const shopId = computed(() => Number(route.params.id))

const api = useAdminApi()
const { handleError, showSuccess, startLoading, stopLoading } = useApiFeedback()
const { loading, start: startListLoading, stop: stopListLoading } = useLoading()
const { ensureBaseUrl } = useAssetHost()

interface ShopImage {
  id: number
  path: string
  hash?: string
  width: number
  height: number
  file_size: number
  original_file_name?: string | null
  is_cover: boolean
  sort: number
  drink_item?: { id: number; name: string } | null
  created_at: string
}

const shopName = ref('')
const filter = ref<'all' | 'orphan' | 'assigned'>('all')
const drinkItemFilter = ref<number | null>(null)
const keyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const images = ref<ShopImage[]>([])

const drinkItemOptions = ref<{ id: number; name: string }[]>([])
const selectedIds = ref<number[]>([])

const drawerVisible = ref(false)
const drawerImage = ref<ShopImage | null>(null)

// ===== Upload =====

const uploadInput = ref<HTMLInputElement | null>(null)
const queue = useImageUploadQueue({ uploadEndpoint: `/admin/shops/${shopId.value}/images` })

function onPickFiles() {
  uploadInput.value?.click()
}

async function onFileSelected(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return
  queue.enqueue(Array.from(input.files))
  input.value = ''
}

watch(() => queue.progress.value.done, async (done, prev) => {
  if (done > prev) {
    // 有新上傳完成 → reload
    await fetchList()
  }
})

// ===== Data =====

async function fetchShop() {
  const { data: res } = await api.GET('/api/admin/shops/{id}', {
    params: { path: { id: shopId.value } },
  })
  shopName.value = res?.data?.name ?? ''
}

async function fetchDrinkItems() {
  const { data: res } = await api.GET('/api/admin/shops/{shopId}/menu', {
    params: { path: { shopId: shopId.value } },
  })
  const list: { id: number; name: string }[] = []
  const cats = res?.data?.categories ?? []
  for (const cat of cats) {
    for (const item of cat.items ?? []) {
      if (item.drink_item_id && item.drink_item_name) {
        list.push({ id: item.drink_item_id, name: item.drink_item_name })
      }
    }
  }
  // 去重（同 drinkItemId 在不同分類可能重複）
  const seen = new Set<number>()
  drinkItemOptions.value = list.filter((x) => {
    if (seen.has(x.id)) return false
    seen.add(x.id); return true
  })
}

async function fetchList() {
  startListLoading()
  try {
    const { data: res } = await api.GET('/api/admin/shops/{shopId}/images', {
      params: {
        path: { shopId: shopId.value },
        query: {
          filter: filter.value,
          drink_item_id: drinkItemFilter.value ?? undefined,
          keyword: keyword.value || undefined,
          page: page.value,
          page_size: pageSize.value,
        },
      },
    })
    images.value = (res?.data?.items ?? []) as ShopImage[]
    total.value = res?.data?.total ?? 0
  } catch (err) {
    console.error('Failed to fetch shop images:', err)
  } finally {
    stopListLoading()
  }
}

onMounted(async () => {
  await ensureBaseUrl()
  await Promise.all([fetchShop(), fetchDrinkItems()])
  await fetchList()
})

// ===== Drawer =====

function openDrawer(image: ShopImage) {
  drawerImage.value = image
  drawerVisible.value = true
}

async function rebind(payload: { drinkItemId: number | null }) {
  if (!drawerImage.value) return
  startLoading()
  try {
    const { error } = await api.PATCH('/api/admin/shops/{shopId}/images/{imageId}', {
      params: { path: { shopId: shopId.value, imageId: drawerImage.value.id } },
      body: { change_drink_item: true, drink_item_id: payload.drinkItemId ?? null },
    })
    await stopLoading()
    if (error) { handleError(error, '改綁失敗'); return }
    showSuccess('已更新綁定')
    drawerVisible.value = false
    await fetchList()
  } catch { await stopLoading() }
}

async function setCover() {
  if (!drawerImage.value) return
  startLoading()
  try {
    const { error } = await api.PATCH('/api/admin/shops/{shopId}/images/{imageId}', {
      params: { path: { shopId: shopId.value, imageId: drawerImage.value.id } },
      body: { is_cover: !drawerImage.value.is_cover },
    })
    await stopLoading()
    if (error) { handleError(error, '設定封面失敗'); return }
    showSuccess('已更新封面')
    await fetchList()
    // 重新指向更新後的 image
    const updated = images.value.find(i => i.id === drawerImage.value?.id)
    if (updated) drawerImage.value = updated
  } catch { await stopLoading() }
}

async function makeOrphan() {
  if (!drawerImage.value) return
  startLoading()
  try {
    const { error } = await api.PATCH('/api/admin/shops/{shopId}/images/{imageId}', {
      params: { path: { shopId: shopId.value, imageId: drawerImage.value.id } },
      body: { change_drink_item: true, drink_item_id: null },
    })
    await stopLoading()
    if (error) { handleError(error, '改為孤兒失敗'); return }
    showSuccess('已改為孤兒')
    drawerVisible.value = false
    await fetchList()
  } catch { await stopLoading() }
}

async function deleteOne() {
  if (!drawerImage.value) return
  try {
    await ElMessageBox.confirm('確定要刪除此圖片？', '刪除確認', {
      type: 'warning', confirmButtonText: '刪除', cancelButtonText: '取消',
    })
    startLoading()
    const { error } = await api.DELETE('/api/admin/shops/{shopId}/images', {
      params: { path: { shopId: shopId.value } },
      body: { ids: [drawerImage.value.id] },
    })
    await stopLoading()
    if (error) { handleError(error, '刪除失敗'); return }
    showSuccess('已刪除')
    drawerVisible.value = false
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') handleError(err, '刪除失敗')
  }
}

// ===== Batch =====

async function batchDelete() {
  if (selectedIds.value.length === 0) return
  try {
    await ElMessageBox.confirm(`確定要刪除選取的 ${selectedIds.value.length} 張圖片？`, '批量刪除確認', {
      type: 'warning', confirmButtonText: '刪除', cancelButtonText: '取消',
    })
    startLoading()
    const { error } = await api.DELETE('/api/admin/shops/{shopId}/images', {
      params: { path: { shopId: shopId.value } },
      body: { ids: selectedIds.value },
    })
    await stopLoading()
    if (error) { handleError(error, '批量刪除失敗'); return }
    showSuccess(`已刪除 ${selectedIds.value.length} 張`)
    selectedIds.value = []
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') handleError(err, '批量刪除失敗')
  }
}

const orphanCount = computed(() => images.value.filter(i => !i.drink_item).length)

async function clearOrphans() {
  try {
    await ElMessageBox.confirm(
      `將刪除此店所有「孤兒」圖片。請確認後執行。`,
      '一鍵清孤兒',
      { type: 'warning', confirmButtonText: '清除', cancelButtonText: '取消' }
    )
    startLoading()
    const { data: res, error } = await api.DELETE('/api/admin/shops/{shopId}/images/orphans', {
      params: { path: { shopId: shopId.value } },
    })
    await stopLoading()
    if (error) { handleError(error, '清孤兒失敗'); return }
    showSuccess(`已刪除 ${res?.data?.deleted ?? 0} 張孤兒圖`)
    selectedIds.value = []
    await fetchList()
  } catch (err: any) {
    if (err !== 'cancel') handleError(err, '清孤兒失敗')
  }
}
</script>

<template>
  <div>
    <AppBreadcrumb />

    <el-card shadow="never" v-loading="loading">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <div style="display: flex; align-items: center; gap: 8px">
            <el-button text @click="router.push('/shop/list')">
              <el-icon><ArrowLeft /></el-icon>返回
            </el-button>
            <span>店家圖庫: {{ shopName }}</span>
          </div>
          <el-button size="small" @click="router.push(`/shop/${shopId}/edit`)">編輯店家</el-button>
        </div>
      </template>

      <!-- 篩選列 -->
      <div class="toolbar">
        <div class="toolbar-left">
          <el-select v-model="filter" style="width: 130px" @change="(page = 1, fetchList())">
            <el-option label="全部" value="all" />
            <el-option label="已使用" value="assigned" />
            <el-option label="孤兒" value="orphan" />
          </el-select>
          <el-select
            v-model="drinkItemFilter"
            placeholder="指定品項"
            clearable
            filterable
            style="width: 220px"
            @change="(page = 1, fetchList())"
          >
            <el-option
              v-for="item in drinkItemOptions"
              :key="item.id"
              :label="`#${item.id} ${item.name}`"
              :value="item.id"
            />
          </el-select>
          <el-input
            v-model="keyword"
            placeholder="搜尋檔名"
            clearable
            style="width: 200px"
            @keyup.enter="(page = 1, fetchList())"
            @clear="(page = 1, fetchList())"
          />
          <el-button type="primary" @click="(page = 1, fetchList())">查詢</el-button>
        </div>
        <div class="toolbar-right">
          <input
            ref="uploadInput"
            type="file"
            accept="image/*"
            multiple
            style="display: none"
            @change="onFileSelected"
          />
          <el-button v-if="selectedIds.length > 0" type="danger" @click="batchDelete">
            批量刪除 ({{ selectedIds.length }})
          </el-button>
          <el-button v-if="orphanCount > 0" @click="clearOrphans">一鍵清孤兒</el-button>
          <el-button type="primary" :icon="Plus" @click="onPickFiles">上傳圖片</el-button>
        </div>
      </div>

      <!-- 上傳進度 -->
      <div v-if="queue.uploads.value.length > 0" class="upload-status">
        <div class="upload-summary">
          <el-icon><UploadIcon /></el-icon>
          <span>進度 {{ queue.progress.value.done }} / {{ queue.progress.value.total }}（失敗 {{ queue.progress.value.failed }}）</span>
          <el-button v-if="queue.progress.value.done === queue.progress.value.total" size="small" text @click="queue.clear()">清除</el-button>
        </div>
        <div class="upload-list">
          <div v-for="item in queue.uploads.value" :key="item.id" class="upload-item">
            <span class="upload-name">{{ item.file.name }}</span>
            <el-progress :percentage="item.progress" :status="item.status === 'error' ? 'exception' : item.status === 'done' ? 'success' : ''" style="flex: 1" />
            <el-button v-if="item.status === 'error'" size="small" @click="queue.retry(item)">重試</el-button>
          </div>
        </div>
      </div>

      <!-- 縮圖網格 -->
      <ShopImagePoolGrid
        v-if="images.length > 0"
        :images="images"
        :selectable="true"
        :selected-ids="selectedIds"
        @update:selected-ids="selectedIds = $event"
        @click="openDrawer"
      />
      <div v-else class="empty">目前沒有圖片</div>

      <AppPagination v-model:page="page" v-model:page-size="pageSize" :total="total" @change="fetchList" />
    </el-card>

    <ShopImageDrawer
      v-model="drawerVisible"
      :image="drawerImage"
      :drink-item-options="drinkItemOptions"
      @rebind="rebind"
      @set-cover="setCover"
      @orphan="makeOrphan"
      @delete="deleteOne"
    />
  </div>
</template>

<style scoped>
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  gap: 12px;
  flex-wrap: wrap;
}

.toolbar-left, .toolbar-right {
  display: flex;
  gap: 12px;
  align-items: center;
}

.upload-status {
  background: var(--el-fill-color-light);
  border-radius: 6px;
  padding: 12px 16px;
  margin-bottom: 16px;
}

.upload-summary {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
  margin-bottom: 8px;
}

.upload-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.upload-item {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 13px;
}

.upload-name {
  width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.empty {
  text-align: center;
  padding: 60px;
  color: var(--el-text-color-secondary);
}
</style>
