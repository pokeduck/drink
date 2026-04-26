<script setup lang="ts">
import { useRoute } from "vue-router"
import { useMenuStore, type MenuModel } from "~/stores/menu"
import { ArrowRight } from "@element-plus/icons-vue"

const route = useRoute()
const menuStore = useMenuStore()

interface BreadcrumbItem {
  title: string
  path: string
}

/**
 * 從 menu tree 中找到目標路徑對應的葉節點，回傳完整祖先鏈
 */
function findAncestorChain(
  items: MenuModel[],
  targetPath: string,
  ancestors: BreadcrumbItem[] = [],
): BreadcrumbItem[] | null {
  for (const item of items) {
    const current: BreadcrumbItem = { title: item.title, path: item.index }

    if (!item.children?.length && item.index === targetPath) {
      return [...ancestors, current]
    }

    if (item.children?.length) {
      const found = findAncestorChain(item.children, targetPath, [
        ...ancestors,
        current,
      ])
      if (found) return found
    }
  }
  return null
}

/**
 * 收集所有葉節點的 endpoint，用於找「最接近的 menu 頁面」
 */
function collectLeafPaths(items: MenuModel[]): string[] {
  const paths: string[] = []
  for (const item of items) {
    if (item.children?.length) {
      paths.push(...collectLeafPaths(item.children))
    } else {
      paths.push(item.index)
    }
  }
  return paths
}

/** 子頁面自動附加的標題對照 */
const subPageTitles: Record<string, string> = {
  create: "新增",
  edit: "編輯",
  images: "圖庫管理",
}

const breadcrumbs = computed(() => {
  const home: BreadcrumbItem = { title: "首頁", path: "/" }

  if (route.path === "/") return [home]

  // 1. 精確匹配 menu tree 中的葉節點
  const exactChain = findAncestorChain(menuStore.menuData, route.path)
  if (exactChain) return [home, ...exactChain]

  // 2. 子頁面匹配：找當前路徑最長前綴匹配的 menu 葉節點
  //    例如 /admin-account/create → 匹配 /admin-account/list 的祖先鏈
  //    例如 /admin-account/1/edit → 匹配 /admin-account/list 的祖先鏈
  const leafPaths = collectLeafPaths(menuStore.menuData)
  const currentSegments = route.path.split("/").filter(Boolean)

  // 找最長前綴匹配的 menu 葉節點
  // e.g. /admin-account/role/create → /admin-account/role（而非 /admin-account/list）
  let parentMenuPath: string | undefined
  let bestMatchLen = 0
  for (const lp of leafPaths) {
    const prefix = lp.replace(/\/list$/, '')
    if (route.path.startsWith(prefix) && prefix.length > bestMatchLen) {
      bestMatchLen = prefix.length
      parentMenuPath = lp
    }
  }

  if (parentMenuPath) {
    const parentChain = findAncestorChain(menuStore.menuData, parentMenuPath)
    if (parentChain) {
      // 取得最後一個 URL segment 作為子頁面標題
      const lastSegment = currentSegments[currentSegments.length - 1]
      const subTitle = subPageTitles[lastSegment] || lastSegment

      return [
        home,
        ...parentChain,
        { title: subTitle, path: route.path },
      ]
    }
  }

  // 3. fallback：URL segments（給完全不在 menu 中的頁面）
  const segments = route.path.split("/").filter(Boolean)
  let currentPath = ""
  const items = segments.map((seg) => {
    currentPath += `/${seg}`
    return { title: seg, path: currentPath }
  })
  return [home, ...items]
})
</script>

<template>
  <el-breadcrumb :separator-icon="ArrowRight" class="app-breadcrumb">
    <el-breadcrumb-item
      v-for="(item, idx) in breadcrumbs"
      :key="item.path"
      :to="idx === 0 ? { path: '/' } : undefined"
    >
      {{ item.title }}
    </el-breadcrumb-item>
  </el-breadcrumb>
</template>

<style scoped>
.app-breadcrumb {
  margin-bottom: 16px;
}
</style>
