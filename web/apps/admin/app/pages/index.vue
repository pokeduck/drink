<script setup lang="ts">
import { DrinkStatus } from "@app/models";
import { formatPrice } from "@app/core";

const productPrice = 1200;
const testPrice = formatPrice(productPrice);
</script>

<template>
  <div>
    <AppBreadcrumb />
    <h1>Admin Dashboard</h1>
    <p>原始數據：{{ productPrice }}</p>
    <p>格式化價格：{{ testPrice }}</p>
    <p>狀態：{{ DrinkStatus.PREPARING }}</p>
  </div>
</template>
