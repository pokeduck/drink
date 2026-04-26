<script setup lang="ts">
import { ref, watch } from 'vue'
import { useAssetHost } from '~/composable/useAssetHost'
import { formatDateTime } from '~/utils/format'

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

interface DrinkItemOption {
  id: number
  name: string
}

const props = defineProps<{
  modelValue: boolean
  image: ShopImageItem | null
  drinkItemOptions: DrinkItemOption[]
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', v: boolean): void
  (e: 'rebind', payload: { drinkItemId: number | null }): void
  (e: 'setCover'): void
  (e: 'orphan'): void
  (e: 'delete'): void
}>()

const { assetUrl } = useAssetHost()
const selectedDrinkItemId = ref<number | null>(null)

watch(() => props.image?.id, () => {
  selectedDrinkItemId.value = props.image?.drink_item?.id ?? null
}, { immediate: true })

function close() {
  emit('update:modelValue', false)
}

function formatBytes(n: number): string {
  if (n < 1024) return `${n} B`
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`
  return `${(n / 1024 / 1024).toFixed(2)} MB`
}

function rebind() {
  emit('rebind', { drinkItemId: selectedDrinkItemId.value })
}
</script>

<template>
  <el-drawer
    :model-value="modelValue"
    :before-close="(done) => { close(); done() }"
    title="圖片詳情"
    direction="rtl"
    size="420px"
  >
    <div v-if="image" class="drawer-body">
      <div class="preview">
        <img :src="assetUrl(image.path)" :alt="image.original_file_name ?? ''" />
      </div>

      <el-descriptions :column="1" size="small" border>
        <el-descriptions-item label="檔名">
          {{ image.original_file_name ?? '—' }}
        </el-descriptions-item>
        <el-descriptions-item label="尺寸">
          {{ image.width }} × {{ image.height }}
        </el-descriptions-item>
        <el-descriptions-item label="大小">
          {{ formatBytes(image.file_size) }}
        </el-descriptions-item>
        <el-descriptions-item label="上傳時間">
          {{ formatDateTime(image.created_at) }}
        </el-descriptions-item>
      </el-descriptions>

      <div class="section">
        <div class="section-title">綁定品項</div>
        <div class="row">
          <el-select
            v-model="selectedDrinkItemId"
            placeholder="未綁定（孤兒）"
            clearable
            style="flex: 1"
          >
            <el-option
              v-for="item in drinkItemOptions"
              :key="item.id"
              :label="`#${item.id} ${item.name}`"
              :value="item.id"
            />
          </el-select>
          <el-button type="primary" @click="rebind">儲存</el-button>
        </div>
      </div>

      <div class="section">
        <el-checkbox
          :model-value="image.is_cover"
          :disabled="!image.drink_item"
          @change="emit('setCover')"
        >
          設為封面
        </el-checkbox>
        <FormHint v-if="!image.drink_item">孤兒圖不可設為封面，請先綁定品項</FormHint>
      </div>

      <div class="section actions">
        <el-button v-if="image.drink_item" @click="emit('orphan')">改為孤兒</el-button>
        <el-button type="danger" @click="emit('delete')">刪除</el-button>
      </div>
    </div>
  </el-drawer>
</template>

<style scoped>
.drawer-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.preview {
  width: 100%;
  aspect-ratio: 1 / 1;
  background: var(--el-fill-color-light);
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  overflow: hidden;
}

.preview img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

.section {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.section-title {
  font-size: 14px;
  font-weight: 500;
  color: var(--el-text-color-primary);
}

.row {
  display: flex;
  gap: 8px;
}

.actions {
  flex-direction: row;
  justify-content: flex-end;
}
</style>
