<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { Plus, Picture as PictureIcon } from '@element-plus/icons-vue'
import { useAdminApi } from '~/composable/useAdminApi'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useAssetHost } from '~/composable/useAssetHost'
import { useImageUploadQueue } from '~/composable/useImageUploadQueue'
import ShopImageItemPicker from './ShopImageItemPicker.vue'

interface ShopImageItem {
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

const props = defineProps<{
  shopId: number
  drinkItemId: number
}>()

const api = useAdminApi()
const { handleError, showSuccess, startLoading, stopLoading } = useApiFeedback()
const { ensureBaseUrl, assetUrl } = useAssetHost()

const images = ref<ShopImageItem[]>([])
const orphans = ref<ShopImageItem[]>([])
const pickerVisible = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const loading = ref(false)
const selectedIds = ref<number[]>([])

const selectedSet = computed(() => new Set(selectedIds.value))
function toggleSelect(image: ShopImageItem) {
  const next = new Set(selectedSet.value)
  if (next.has(image.id)) next.delete(image.id)
  else next.add(image.id)
  selectedIds.value = Array.from(next)
}
function clearSelection() {
  selectedIds.value = []
}

const MAX_IMAGES = 10
const remaining = computed(() => MAX_IMAGES - images.value.length)
const isFull = computed(() => images.value.length >= MAX_IMAGES)

// ===== Upload queue =====
const queue = useImageUploadQueue({
  uploadEndpoint: `/admin/shops/${props.shopId}/images`,
  query: { drink_item_id: props.drinkItemId },
})

/**
 * 把 MessageBox 掛到當前開啟的 dialog 內，避免被 dialog 蓋住。
 * 邏輯與 useApiFeedback.showErrorAlert 一致。
 */
function messageBoxOverlayOptions() {
  const dialogEl = document.querySelector('.el-dialog') as HTMLElement | null
  if (!dialogEl) return {}
  return {
    appendTo: dialogEl,
    modal: false,
  }
}

watch(() => queue.progress.value.done, async (done, prev) => {
  if (done > prev) await fetchImages()
})

// ===== Data =====
async function fetchImages() {
  loading.value = true
  try {
    const { data: res } = await api.GET(
      '/api/admin/shops/{shopId}/drink-items/{drinkItemId}/images',
      { params: { path: { shopId: props.shopId, drinkItemId: props.drinkItemId } } },
    )
    images.value = (res?.data ?? []) as ShopImageItem[]
    // 清掉已選但本次回傳沒有的 id（避免 stale 選取）
    if (selectedIds.value.length > 0) {
      const validIds = new Set(images.value.map(i => i.id))
      selectedIds.value = selectedIds.value.filter(id => validIds.has(id))
    }
  }
  finally {
    loading.value = false
  }
}

async function fetchOrphans() {
  const { data: res } = await api.GET('/api/admin/shops/{shopId}/images', {
    params: {
      path: { shopId: props.shopId },
      query: { filter: 'orphan', page: 1, page_size: 200 },
    },
  })
  orphans.value = (res?.data?.items ?? []) as ShopImageItem[]
}

onMounted(async () => {
  await ensureBaseUrl()
  await fetchImages()
})

watch(() => [props.shopId, props.drinkItemId], async () => {
  queue.clear()
  await fetchImages()
})

// ===== Actions =====
function pickFiles() {
  if (isFull.value) return
  fileInput.value?.click()
}

function onFileSelected(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files || input.files.length === 0) return
  const files = Array.from(input.files)
  // 限制不超過剩餘量
  const allowed = files.slice(0, remaining.value)
  if (allowed.length < files.length) {
    ElMessage.warning(`此品項剩餘 ${remaining.value} 張可上傳，已忽略 ${files.length - allowed.length} 張`)
  }
  if (allowed.length > 0) queue.enqueue(allowed)
  input.value = ''
}

async function setCover(image: ShopImageItem) {
  if (image.is_cover) return
  startLoading()
  try {
    const { error } = await api.PATCH('/api/admin/shops/{shopId}/images/{imageId}', {
      params: { path: { shopId: props.shopId, imageId: image.id } },
      body: { is_cover: true },
    })
    await stopLoading()
    if (error) { handleError(error, '設定封面失敗'); return }
    showSuccess('已設為封面')
    await fetchImages()
  }
  catch { await stopLoading() }
}

async function removeImage(image: ShopImageItem) {
  try {
    await ElMessageBox.confirm(`確定要將此圖移除（改為孤兒）？\n圖片仍會保留在圖庫中。`, '移除圖片', {
      type: 'warning', confirmButtonText: '移除', cancelButtonText: '取消',
      ...messageBoxOverlayOptions(),
    })
    startLoading()
    const { error } = await api.PATCH('/api/admin/shops/{shopId}/images/{imageId}', {
      params: { path: { shopId: props.shopId, imageId: image.id } },
      body: { change_drink_item: true, drink_item_id: null },
    })
    await stopLoading()
    if (error) { handleError(error, '移除失敗'); return }
    showSuccess('已移為孤兒')
    await fetchImages()
  }
  catch (err: any) {
    if (err !== 'cancel') handleError(err, '移除失敗')
  }
}

async function batchUnbind() {
  if (selectedIds.value.length === 0) return
  const count = selectedIds.value.length
  try {
    await ElMessageBox.confirm(
      `確定要將選取的 ${count} 張圖移除綁定（改為孤兒）？\n圖片仍會保留在圖庫中。`,
      '批量移除綁定',
      {
        type: 'warning', confirmButtonText: '移除綁定', cancelButtonText: '取消',
        ...messageBoxOverlayOptions(),
      },
    )
    startLoading()
    try {
      for (const imageId of selectedIds.value) {
        const { error } = await api.PATCH('/api/admin/shops/{shopId}/images/{imageId}', {
          params: { path: { shopId: props.shopId, imageId } },
          body: { change_drink_item: true, drink_item_id: null },
        })
        if (error) {
          await stopLoading()
          handleError(error, '批量移除失敗')
          await fetchImages()
          return
        }
      }
      await stopLoading()
      showSuccess(`已移除 ${count} 張綁定`)
      clearSelection()
      await fetchImages()
    }
    catch { await stopLoading() }
  }
  catch (err: any) {
    if (err !== 'cancel') handleError(err, '批量移除失敗')
  }
}

async function openPicker() {
  // 同時刷新本品項圖片數與孤兒池，避免 currentCount 取到舊值
  // 造成 picker 內 maxSelectable 計算錯誤
  await Promise.all([fetchImages(), fetchOrphans()])
  pickerVisible.value = true
}

async function bindOrphans(ids: number[]) {
  const slots = remaining.value
  if (ids.length > slots) {
    ElMessage.warning(`此品項剩餘 ${slots} 張可綁定，將只綁定前 ${slots} 張`)
    ids = ids.slice(0, slots)
  }
  if (ids.length === 0) return
  startLoading()
  try {
    for (const imageId of ids) {
      const { error } = await api.PATCH('/api/admin/shops/{shopId}/images/{imageId}', {
        params: { path: { shopId: props.shopId, imageId } },
        body: { change_drink_item: true, drink_item_id: props.drinkItemId },
      })
      if (error) {
        handleError(error, '綁定失敗')
        break
      }
    }
    await stopLoading()
    showSuccess(`已綁定 ${ids.length} 張`)
    await fetchImages()
  }
  catch { await stopLoading() }
}
</script>

<template>
  <div class="strip" v-loading="loading">
    <div class="header">
      <span class="title">圖片 ({{ images.length }}/{{ MAX_IMAGES }})</span>
      <div class="actions">
        <input
          ref="fileInput"
          type="file"
          accept="image/*"
          multiple
          style="display: none"
          @change="onFileSelected"
        />
        <el-button size="small" :disabled="isFull" @click="openPicker">從孤兒池挑</el-button>
        <el-button size="small" type="primary" :icon="Plus" :disabled="isFull" @click="pickFiles">
          上傳
        </el-button>
      </div>
    </div>

    <!-- 上傳進度 -->
    <div v-if="queue.uploads.value.length > 0" class="upload-progress">
      <div v-for="item in queue.uploads.value" :key="item.id" class="upload-row">
        <span class="upload-name">{{ item.file.name }}</span>
        <el-progress
          :percentage="item.progress"
          :status="item.status === 'error' ? 'exception' : item.status === 'done' ? 'success' : ''"
          :show-text="false"
          style="flex: 1"
        />
        <span v-if="item.status === 'error'" class="upload-error">
          {{ item.error }}
          <el-button size="small" text @click="queue.retry(item)">重試</el-button>
        </span>
      </div>
    </div>

    <!-- 批量操作 bar（選取後浮現） -->
    <div v-if="selectedIds.length > 0" class="batch-bar">
      <span class="batch-info">已選 {{ selectedIds.length }} 張</span>
      <div class="batch-actions">
        <el-button size="small" @click="clearSelection">取消選取</el-button>
        <el-button size="small" type="warning" @click="batchUnbind">批量移除綁定</el-button>
      </div>
    </div>

    <!-- 縮圖橫向列 -->
    <div v-if="images.length > 0" class="thumb-row">
      <div v-for="image in images" :key="image.id" class="thumb" :class="{ selected: selectedSet.has(image.id) }">
        <div class="thumb-img-wrap">
          <div class="checkbox-overlay" @click.stop>
            <el-checkbox
              :model-value="selectedSet.has(image.id)"
              @change="toggleSelect(image)"
            />
          </div>
          <img :src="assetUrl(image.path)" :alt="image.original_file_name ?? ''" loading="lazy" />
          <div v-if="image.is_cover" class="cover-badge">★</div>
        </div>
        <div class="thumb-actions">
          <el-tooltip :content="image.is_cover ? '已是封面' : '設為封面'" placement="top">
            <el-button
              size="small"
              :type="image.is_cover ? 'warning' : 'default'"
              :disabled="image.is_cover"
              @click="setCover(image)"
            >
              ★
            </el-button>
          </el-tooltip>
          <el-tooltip content="移除綁定（保留檔案）" placement="top">
            <el-button size="small" @click="removeImage(image)">移除</el-button>
          </el-tooltip>
        </div>
      </div>
    </div>
    <div v-else class="empty">
      <el-icon class="empty-icon"><PictureIcon /></el-icon>
      <span>尚無圖片，點「上傳」或「從孤兒池挑」加入</span>
    </div>

    <ShopImageItemPicker
      v-model="pickerVisible"
      :orphans="orphans"
      :current-count="images.length"
      :max="MAX_IMAGES"
      @confirm="bindOrphans"
    />
  </div>
</template>

<style scoped>
.strip {
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 6px;
  padding: 12px;
  background: var(--el-fill-color-lighter);
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.title {
  font-size: 14px;
  font-weight: 500;
}

.actions {
  display: flex;
  gap: 8px;
}

.upload-progress {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-bottom: 12px;
  padding: 8px;
  background: var(--el-bg-color);
  border-radius: 4px;
}

.upload-row {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
}

.upload-name {
  width: 140px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.upload-error {
  color: var(--el-color-danger);
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
}

.thumb-row {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding-bottom: 4px;
}

.batch-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  margin-bottom: 12px;
  background: var(--el-color-primary-light-9);
  border-left: 3px solid var(--el-color-primary);
  border-radius: 4px;
}

.batch-info {
  font-size: 13px;
  color: var(--el-color-primary);
  font-weight: 500;
}

.batch-actions {
  display: flex;
  gap: 8px;
}

.thumb {
  flex: 0 0 auto;
  width: 120px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.thumb.selected .thumb-img-wrap {
  border-color: var(--el-color-primary);
  box-shadow: 0 0 0 2px var(--el-color-primary-light-7);
}

.thumb-img-wrap {
  position: relative;
  width: 120px;
  height: 120px;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color 0.15s ease;
}

.checkbox-overlay {
  position: absolute;
  top: 4px;
  left: 4px;
  z-index: 2;
  background: rgba(255, 255, 255, 0.92);
  border-radius: 3px;
  padding: 0 4px;
  line-height: 1;
}

.thumb-img-wrap img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

.cover-badge {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: var(--el-color-warning);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
}

.thumb-actions {
  display: flex;
  gap: 4px;
}

.thumb-actions .el-button {
  padding: 4px 8px;
  font-size: 12px;
}

.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 24px;
  color: var(--el-text-color-secondary);
  font-size: 13px;
}

.empty-icon {
  font-size: 32px;
}
</style>
