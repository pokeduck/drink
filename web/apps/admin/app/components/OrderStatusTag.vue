<script setup lang="ts">
const props = defineProps<{
  status: number | undefined
}>()

type TagType = 'success' | 'warning' | 'primary' | 'info' | 'danger'

const statusMap: Record<number, { label: string; type: TagType }> = {
  1: { label: '進行中', type: 'success' },
  2: { label: '已截止', type: 'warning' },
  3: { label: '配送中', type: 'primary' },
  4: { label: '已完成', type: 'info' },
  5: { label: '已取消', type: 'danger' },
}

const view = computed(() => statusMap[props.status ?? 0] ?? { label: '未知', type: 'info' as TagType })
</script>

<template>
  <el-tag :type="view.type" disable-transitions>{{ view.label }}</el-tag>
</template>
