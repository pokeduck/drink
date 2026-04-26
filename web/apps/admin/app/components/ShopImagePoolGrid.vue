<script setup lang="ts">
import { computed } from 'vue'
import { useAssetHost } from '~/composable/useAssetHost'

interface ShopImageItem {
  id: number
  path: string
  hash?: string
  is_cover: boolean
  drink_item?: { id: number; name: string } | null
  original_file_name?: string | null
}

const props = defineProps<{
  images: ShopImageItem[]
  selectable?: boolean
  selectedIds?: number[]
  /** 點整張卡片就切換選取（不觸發 click 事件）。給 picker 用。 */
  selectOnClick?: boolean
  /** 限制最多可選張數。已選達到上限時，未選的卡會 disabled。 */
  maxSelectable?: number
}>()

const emit = defineEmits<{
  (e: 'click', image: ShopImageItem): void
  (e: 'update:selectedIds', ids: number[]): void
}>()

const { assetUrl } = useAssetHost()

const selectedSet = computed(() => new Set(props.selectedIds ?? []))

function isBlocked(image: ShopImageItem): boolean {
  if (props.maxSelectable === undefined) return false
  if (selectedSet.value.has(image.id)) return false  // 已選不擋（讓使用者能取消）
  return selectedSet.value.size >= props.maxSelectable
}

function toggleSelect(image: ShopImageItem) {
  const next = new Set(selectedSet.value)
  if (next.has(image.id)) {
    next.delete(image.id)
  }
  else {
    // 加選時嚴格檢查上限（防止 isBlocked 因 props 同步延遲而短暫失準）
    if (props.maxSelectable !== undefined && next.size >= props.maxSelectable) return
    next.add(image.id)
  }
  emit('update:selectedIds', Array.from(next))
}

function onCardClick(image: ShopImageItem) {
  if (props.selectOnClick) {
    toggleSelect(image)
  }
  else {
    emit('click', image)
  }
}
</script>

<template>
  <div class="image-grid">
    <div
      v-for="image in images"
      :key="image.id"
      class="image-card"
      :class="{ selected: selectedSet.has(image.id), blocked: isBlocked(image) }"
      @click="onCardClick(image)"
    >
      <div v-if="selectable" class="checkbox-overlay" @click.stop>
        <el-checkbox
          :model-value="selectedSet.has(image.id)"
          :disabled="isBlocked(image)"
          @change="toggleSelect(image)"
        />
      </div>
      <div class="thumb-wrap">
        <img :src="assetUrl(image.path)" :alt="image.original_file_name ?? ''" loading="lazy" />
        <div v-if="image.is_cover" class="cover-badge">★</div>
      </div>
      <div class="caption">
        <span v-if="image.drink_item" class="badge assigned">{{ image.drink_item.name }}</span>
        <span v-else class="badge orphan">未使用</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.image-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 16px;
}

.image-card {
  position: relative;
  border: 2px solid var(--el-border-color);
  border-radius: 6px;
  overflow: hidden;
  cursor: pointer;
  transition: border-color 0.15s ease;
  background: var(--el-bg-color);
}

.image-card:hover {
  border-color: var(--el-color-primary);
}

.image-card.selected {
  border-color: var(--el-color-primary);
  box-shadow: 0 0 0 2px var(--el-color-primary-light-7);
}

.image-card.blocked {
  opacity: 0.4;
  cursor: not-allowed;
  filter: grayscale(0.6);
}

.image-card.blocked:hover {
  border-color: var(--el-border-color);
}

.checkbox-overlay {
  position: absolute;
  top: 6px;
  left: 6px;
  z-index: 2;
  background: rgba(255, 255, 255, 0.9);
  border-radius: 4px;
  padding: 0 6px;
}

.thumb-wrap {
  position: relative;
  aspect-ratio: 1 / 1;
  background: var(--el-fill-color-light);
  display: flex;
  align-items: center;
  justify-content: center;
}

.thumb-wrap img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

.cover-badge {
  position: absolute;
  top: 6px;
  right: 6px;
  background: var(--el-color-warning);
  color: white;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.caption {
  padding: 8px;
  font-size: 12px;
  text-align: center;
  border-top: 1px solid var(--el-border-color-lighter);
}

.badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
}

.badge.assigned {
  background: var(--el-color-primary-light-9);
  color: var(--el-color-primary);
}

.badge.orphan {
  background: var(--el-fill-color);
  color: var(--el-text-color-secondary);
}
</style>
