<script setup lang="ts">
import { formatDateTime } from '~/utils/format'

const props = defineProps<{
  createdAt: string
  updatedAt: string
}>()
</script>

<template>
  <span style="font-size: 12px; color: var(--el-text-color-secondary)">
    建立：{{ formatDateTime(props.createdAt) }}　｜　更新：{{ formatDateTime(props.updatedAt) }}
  </span>
</template>
