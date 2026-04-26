<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import ShopImagePoolGrid from './ShopImagePoolGrid.vue'

interface ShopImageItem {
  id: number
  path: string
  hash?: string
  is_cover: boolean
  drink_item?: { id: number; name: string } | null
  original_file_name?: string | null
}

const props = defineProps<{
  modelValue: boolean
  orphans: ShopImageItem[]
  /** 已使用張數（含已綁定的）。picker 會用 max - currentCount 推算可再加多少張。 */
  currentCount?: number
  /** 上限。預設 10。 */
  max?: number
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', v: boolean): void
  (e: 'confirm', ids: number[]): void
}>()

const selectedIds = ref<number[]>([])

const maxSelectable = computed(() => {
  const max = props.max ?? 10
  const used = props.currentCount ?? 0
  return Math.max(0, max - used)
})

const isFullyBlocked = computed(() => maxSelectable.value === 0)
const reachedLimit = computed(() => selectedIds.value.length >= maxSelectable.value)

watch(() => props.modelValue, (open) => {
  if (open) selectedIds.value = []
})

// 若 maxSelectable 因 props 變動而縮減（例：父層的 currentCount 後到），
// 自動截斷 selectedIds 到上限內，避免「已選 3 / 1」這種超量狀態。
watch(maxSelectable, (max) => {
  if (selectedIds.value.length > max) {
    selectedIds.value = selectedIds.value.slice(0, max)
  }
})

function close() {
  emit('update:modelValue', false)
}

function confirm() {
  emit('confirm', selectedIds.value)
  close()
}
</script>

<template>
  <el-dialog
    :model-value="modelValue"
    title="從孤兒池挑選圖片"
    width="720px"
    @update:model-value="(v) => emit('update:modelValue', !!v)"
  >
    <div class="quota-banner" :class="{ blocked: isFullyBlocked }">
      <template v-if="isFullyBlocked">
        此品項圖片數已達上限（{{ max ?? 10 }} 張），無法再加入
      </template>
      <template v-else>
        此品項已有 {{ currentCount ?? 0 }} / {{ max ?? 10 }} 張，最多可再加入 <strong>{{ maxSelectable }}</strong> 張
      </template>
    </div>

    <div v-if="orphans.length === 0" class="empty">
      目前沒有孤兒圖片
    </div>
    <ShopImagePoolGrid
      v-else
      :images="orphans"
      :selectable="true"
      :select-on-click="true"
      :selected-ids="selectedIds"
      :max-selectable="maxSelectable"
      @update:selected-ids="selectedIds = $event"
    />
    <template #footer>
      <span class="footer-info">
        已選 {{ selectedIds.length }} / {{ maxSelectable }} 張
        <span v-if="reachedLimit && !isFullyBlocked" class="hint-reached">已達可加入上限</span>
      </span>
      <el-button @click="close">取消</el-button>
      <el-button type="primary" :disabled="selectedIds.length === 0" @click="confirm">
        綁定到此品項
      </el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.quota-banner {
  margin-bottom: 12px;
  padding: 8px 12px;
  background: var(--el-color-info-light-9);
  border-left: 3px solid var(--el-color-info);
  border-radius: 4px;
  font-size: 13px;
  color: var(--el-text-color-regular);
}

.quota-banner.blocked {
  background: var(--el-color-warning-light-9);
  border-left-color: var(--el-color-warning);
  color: var(--el-color-warning);
}

.empty {
  text-align: center;
  padding: 40px;
  color: var(--el-text-color-secondary);
}

.footer-info {
  margin-right: auto;
  padding-right: 16px;
  color: var(--el-text-color-secondary);
  font-size: 14px;
}

.hint-reached {
  margin-left: 8px;
  color: var(--el-color-warning);
  font-weight: 500;
}
</style>
