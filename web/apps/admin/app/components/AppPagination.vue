<script setup lang="ts">
const props = withDefaults(defineProps<{
  total: number
  pageSizes?: number[]
}>(), {
  pageSizes: () => [10, 20, 50, 100],
})

const paginationLocale = {
  el: {
    pagination: {
      pagesize: ' 筆 / 頁',
      goto: '跳至',
      pageClassifier: '頁',
    },
  },
}

const page = defineModel<number>('page', { required: true })
const pageSize = defineModel<number>('pageSize', { required: true })

const emit = defineEmits<{
  change: []
}>()

function onPageChange() {
  emit('change')
}

function onSizeChange() {
  page.value = 1
  emit('change')
}
</script>

<template>
  <div class="app-pagination">
    <el-config-provider :locale="paginationLocale">
    <el-pagination
      v-model:current-page="page"
      v-model:page-size="pageSize"
      :total="props.total"
      :page-sizes="props.pageSizes"
      layout="slot, ->, sizes, prev, pager, next, jumper"
      background
      @current-change="onPageChange"
      @size-change="onSizeChange"
    >
      <span class="app-pagination__total">共 {{ props.total }} 筆</span>
    </el-pagination>
    </el-config-provider>
  </div>
</template>

<style scoped>
.app-pagination {
  margin-top: 16px;
}
</style>
