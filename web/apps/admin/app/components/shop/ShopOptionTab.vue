<script setup lang="ts">
interface Row {
  id: number
  name: string
  defaultPrice?: number | null
  isEnabled: boolean
  sort: number
}

defineProps<{
  hasPrice?: boolean
}>()

const model = defineModel<Row[]>({ required: true })

const allEnabled = computed(() => model.value.length > 0 && model.value.every(r => r.isEnabled))
const someEnabled = computed(() => model.value.some(r => r.isEnabled))

function toggleAll(checked: boolean) {
  model.value = model.value.map(r => ({ ...r, isEnabled: checked }))
}
</script>

<template>
  <div class="shop-option-tab">
    <div class="actions">
      <el-button
        size="small"
        type="primary"
        :disabled="allEnabled"
        @click="toggleAll(true)"
      >全選</el-button>
      <el-button
        size="small"
        :disabled="!someEnabled"
        @click="toggleAll(false)"
      >取消全選</el-button>
    </div>
    <el-table :data="model" stripe>
      <el-table-column label="啟用" width="80">
        <template #default="{ $index }">
          <el-checkbox v-model="(model[$index] as Row).isEnabled" />
        </template>
      </el-table-column>
      <el-table-column label="名稱" prop="name" />
      <el-table-column v-if="hasPrice" label="全域預設價" width="160">
        <template #default="{ row }">
          {{ (row as Row).defaultPrice ?? 0 }}
        </template>
      </el-table-column>
      <el-table-column label="店內排序" width="180">
        <template #default="{ $index, row }">
          <el-input-number
            v-model="(model[$index] as Row).sort"
            :precision="0"
            :disabled="!(row as Row).isEnabled"
            style="width: 120px; max-width: 100%"
          />
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<style scoped>
.actions {
  margin-bottom: 12px;
  display: flex;
  gap: 8px;
}
</style>
