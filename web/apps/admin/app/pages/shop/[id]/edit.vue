<script setup lang="ts">
import draggable from 'vuedraggable'
import { useAdminApi } from '~/composable/useAdminApi'
import { useFormLayout } from '~/composable/useFormLayout'
import { useApiFeedback } from '~/composable/useApiFeedback'
import { useUnsavedGuard } from '~/composable/useUnsavedGuard'
import { useLoading } from '~/composable/useLoading'
import { usePermission } from '~/composable/usePermission'
import { MENU } from '@app/core'
import type { components } from '@app/api-types/admin'

type MenuCategory = components['schemas']['AdminShopMenuCategoryResponse']
type MenuItem = components['schemas']['AdminShopMenuItemResponse']
type DrinkItem = components['schemas']['DrinkItemListResponse']
type SugarItem = components['schemas']['SugarListResponse']
type IceItem = components['schemas']['IceListResponse']
type ToppingItem = components['schemas']['ToppingListResponse']
type SizeItem = components['schemas']['SizeListResponse']

const api = useAdminApi()
const router = useRouter()
const route = useRoute()
const shopId = Number(route.params.id)
const { labelPosition } = useFormLayout()
const { serverErrors, handleError, clearErrors, showSuccess, startLoading, stopLoading } = useApiFeedback()
const { serverErrors: itemServerErrors, handleError: handleItemError, clearErrors: clearItemErrors } = useApiFeedback()
const { can } = usePermission()

const formRef = ref()
const fetchLoading = ref(true)
const createdAt = ref('')
const updatedAt = ref('')

// ==================== 店家基本資訊 ====================

const form = reactive({
  name: '',
  phone: '',
  address: '',
  note: '',
  status: 1,
  sort: 0,
  max_topping_per_item: 1,
})

const { takeSnapshot } = useUnsavedGuard(form)

const rules = {
  name: [{ required: true, message: '請輸入店家名稱', trigger: 'blur' }],
  status: [{ required: true, message: '請選擇狀態', trigger: 'change' }],
}

const fetchShop = async () => {
  const { data: res, error } = await api.GET('/api/admin/shops/{id}', {
    params: { path: { id: shopId } },
  })
  if (error) {
    handleError(error, '載入店家資料失敗')
    router.push('/shop/list')
    return
  }
  const item = res!.data!
  form.name = item.name!
  form.phone = item.phone ?? ''
  form.address = item.address ?? ''
  form.note = item.note ?? ''
  form.status = item.status!
  form.sort = item.sort!
  form.max_topping_per_item = item.max_topping_per_item!
  createdAt.value = item.created_at!
  updatedAt.value = item.updated_at!
  takeSnapshot()
}

const handleSubmitShop = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  clearErrors()
  startLoading()
  const { error } = await api.PUT('/api/admin/shops/{id}', {
    params: { path: { id: shopId } },
    body: {
      name: form.name,
      phone: form.phone || undefined,
      address: form.address || undefined,
      note: form.note || undefined,
      status: form.status,
      sort: form.sort,
      max_topping_per_item: form.max_topping_per_item,
    },
  })
  await stopLoading()

  if (error) {
    handleError(error, '更新失敗')
    return
  }
  showSuccess('店家資訊更新成功')
  takeSnapshot()
}

// ==================== 菜單管理 ====================

const categories = ref<MenuCategory[]>([])
const { loading: menuLoading, start: startMenuLoading, stop: stopMenuLoading } = useLoading()

const expandedCategories = ref<Set<number>>(new Set())
const expandedItems = ref<Set<number>>(new Set())

const toggleCategory = (id: number) => {
  if (expandedCategories.value.has(id)) expandedCategories.value.delete(id)
  else expandedCategories.value.add(id)
}

const toggleItem = (id: number) => {
  if (expandedItems.value.has(id)) expandedItems.value.delete(id)
  else expandedItems.value.add(id)
}

const fetchMenu = async () => {
  startMenuLoading()
  try {
    const { data: res, error } = await api.GET('/api/admin/shops/{shopId}/menu', {
      params: { path: { shopId } },
    })
    if (error) {
      handleError(error, '載入菜單失敗')
      return
    }
    categories.value = res?.data?.categories ?? []
    // 預設展開所有分類
    expandedCategories.value = new Set(categories.value.map(c => c.id!))
  } finally {
    stopMenuLoading()
  }
}

// --- 分類 CRUD ---

const newCategoryName = ref('')

const handleAddCategory = async () => {
  if (!newCategoryName.value.trim()) return
  startLoading()
  const maxSort = categories.value.length > 0
    ? Math.max(...categories.value.map(c => c.sort ?? 0))
    : 0
  const { error } = await api.POST('/api/admin/shops/{shopId}/categories', {
    params: { path: { shopId } },
    body: { name: newCategoryName.value.trim(), sort: maxSort + 1 },
  })
  await stopLoading()
  if (error) { handleError(error, '新增分類失敗'); return }
  showSuccess('分類新增成功')
  newCategoryName.value = ''
  await fetchMenu()
}

const editingCategoryId = ref<number | null>(null)
const editingCategoryName = ref('')

const startEditCategory = (cat: MenuCategory) => {
  editingCategoryId.value = cat.id!
  editingCategoryName.value = cat.name!
}

const handleUpdateCategory = async (cat: MenuCategory) => {
  if (!editingCategoryName.value.trim()) return
  startLoading()
  const { error } = await api.PUT('/api/admin/shops/{shopId}/categories/{categoryId}', {
    params: { path: { shopId, categoryId: cat.id! } },
    body: { name: editingCategoryName.value.trim(), sort: cat.sort! },
  })
  await stopLoading()
  if (error) { handleError(error, '更新分類失敗'); return }
  showSuccess('分類更新成功')
  editingCategoryId.value = null
  await fetchMenu()
}

const cancelEditCategory = () => {
  editingCategoryId.value = null
}

const handleDeleteCategory = async (cat: MenuCategory) => {
  try {
    await ElMessageBox.confirm(`確定要刪除分類「${cat.name}」嗎？底下品項也會一併刪除。`, '刪除確認', {
      type: 'warning',
      confirmButtonText: '刪除',
      cancelButtonText: '取消',
    })
    startLoading()
    const { error } = await api.DELETE('/api/admin/shops/{shopId}/categories/{categoryId}', {
      params: { path: { shopId, categoryId: cat.id! } },
    })
    await stopLoading()
    if (error) { handleError(error, '刪除分類失敗'); return }
    showSuccess('分類刪除成功')
    await fetchMenu()
  } catch (err: any) {
    if (err !== 'cancel') handleError(err, '刪除分類失敗')
  }
}

// --- 品項刪除 ---

const handleDeleteMenuItem = async (cat: MenuCategory, item: MenuItem) => {
  try {
    await ElMessageBox.confirm(`確定要刪除品項「${item.drink_item_name}」嗎？`, '刪除確認', {
      type: 'warning',
      confirmButtonText: '刪除',
      cancelButtonText: '取消',
    })
    startLoading()
    const { error } = await api.DELETE('/api/admin/shops/{shopId}/categories/{categoryId}/items/{itemId}', {
      params: { path: { shopId, categoryId: cat.id!, itemId: item.id! } },
    })
    await stopLoading()
    if (error) { handleError(error, '刪除品項失敗'); return }
    showSuccess('品項刪除成功')
    await fetchMenu()
  } catch (err: any) {
    if (err !== 'cancel') handleError(err, '刪除品項失敗')
  }
}

// --- 拖拉排序 ---

const handleCategoryDragEnd = async () => {
  const items = categories.value.map((c, i) => ({ id: c.id!, sort: i + 1 }))
  const { error } = await api.PUT('/api/admin/shops/{shopId}/categories/sort', {
    params: { path: { shopId } },
    body: { items },
  })
  if (error) { handleError(error, '分類排序失敗'); await fetchMenu(); return }
  // 更新本地 sort 值
  categories.value.forEach((c, i) => { c.sort = i + 1 })
}

const handleItemDragEnd = async (cat: MenuCategory) => {
  if (!cat.items) return
  const items = cat.items.map((m, i) => ({ id: m.id!, sort: i + 1 }))
  const { error } = await api.PUT('/api/admin/shops/{shopId}/categories/{categoryId}/items/sort', {
    params: { path: { shopId, categoryId: cat.id! } },
    body: { items },
  })
  if (error) { handleError(error, '品項排序失敗'); await fetchMenu(); return }
  cat.items.forEach((m, i) => { m.sort = i + 1 })
}

// ==================== 品項編輯 Dialog ====================

// 全域選項資料
const allDrinkItems = ref<DrinkItem[]>([])
const allSugars = ref<SugarItem[]>([])
const allIces = ref<IceItem[]>([])
const allToppings = ref<ToppingItem[]>([])
const allSizes = ref<SizeItem[]>([])

const fetchOptions = async () => {
  const [drinkRes, sugarRes, iceRes, toppingRes, sizeRes] = await Promise.all([
    api.GET('/api/admin/drink-items', { params: { query: { page: 1, page_size: 999 } } }),
    api.GET('/api/admin/sugars', { params: { query: { page: 1, page_size: 999 } } }),
    api.GET('/api/admin/ices', { params: { query: { page: 1, page_size: 999 } } }),
    api.GET('/api/admin/toppings', { params: { query: { page: 1, page_size: 999 } } }),
    api.GET('/api/admin/sizes', { params: { query: { page: 1, page_size: 999 } } }),
  ])
  allDrinkItems.value = drinkRes.data?.data?.items ?? []
  allSugars.value = sugarRes.data?.data?.items ?? []
  allIces.value = iceRes.data?.data?.items ?? []
  allToppings.value = toppingRes.data?.data?.items ?? []
  allSizes.value = sizeRes.data?.data?.items ?? []
}

// Dialog 狀態
const itemDialogVisible = ref(false)
const itemDialogMode = ref<'create' | 'edit'>('create')
const itemDialogCategoryId = ref<number>(0)
const itemDialogItemId = ref<number>(0)
const itemFormRef = ref()

const itemForm = reactive({
  drink_item_id: null as number | null,
  drink_item_name: '',
  description: '',
  sort: 0,
  max_topping_count: 5,
  sizes: [] as { size_id: number; size_name: string; enabled: boolean; price: number }[],
  sugar_ids: [] as number[],
  ice_ids: [] as number[],
  topping_ids: [] as number[],
})

// DrinkItem autocomplete
const drinkItemSuggestions = ref<DrinkItem[]>([])

const handleDrinkItemSearch = (query: string, cb: (items: any[]) => void) => {
  const results = query
    ? allDrinkItems.value.filter(d => d.name?.toLowerCase().includes(query.toLowerCase()))
    : allDrinkItems.value
  cb(results.map(d => ({ value: d.name, id: d.id })))
}

const handleDrinkItemSelect = (selected: { value: string; id: number }) => {
  itemForm.drink_item_id = selected.id
  itemForm.drink_item_name = selected.value
}

const handleDrinkItemInput = (val: string) => {
  // 如果手動修改了文字，清除已選的 ID
  const match = allDrinkItems.value.find(d => d.name === val)
  if (match) {
    itemForm.drink_item_id = match.id!
  } else {
    itemForm.drink_item_id = null
  }
}

const openItemDialog = (categoryId: number, item?: MenuItem) => {
  clearItemErrors()
  itemDialogCategoryId.value = categoryId
  itemDialogMode.value = item ? 'edit' : 'create'
  itemDialogItemId.value = item?.id ?? 0

  if (item) {
    itemForm.drink_item_id = item.drink_item_id ?? null
    itemForm.drink_item_name = item.drink_item_name ?? ''
    itemForm.description = item.description ?? ''
    itemForm.sort = item.sort ?? 0
    itemForm.max_topping_count = item.max_topping_count ?? 5
    itemForm.sizes = allSizes.value.map(s => ({
      size_id: s.id!,
      size_name: s.name!,
      enabled: item.sizes?.some(is => is.size_id === s.id) ?? false,
      price: item.sizes?.find(is => is.size_id === s.id)?.price ?? 0,
    }))
    itemForm.sugar_ids = [...(item.sugar_ids ?? [])]
    itemForm.ice_ids = [...(item.ice_ids ?? [])]
    itemForm.topping_ids = [...(item.topping_ids ?? [])]
  } else {
    itemForm.drink_item_id = null
    itemForm.drink_item_name = ''
    itemForm.description = ''
    itemForm.sort = 0
    itemForm.max_topping_count = 5
    itemForm.sizes = allSizes.value.map(s => ({
      size_id: s.id!,
      size_name: s.name!,
      enabled: false,
      price: 0,
    }))
    itemForm.sugar_ids = allSugars.value.map(s => s.id!)
    itemForm.ice_ids = allIces.value.map(i => i.id!)
    itemForm.topping_ids = allToppings.value.map(t => t.id!)
  }

  itemDialogVisible.value = true
}

const itemRules = {
  drink_item_name: [{
    validator: (_rule: any, _value: any, callback: any) => {
      if (!itemForm.drink_item_id && !itemForm.drink_item_name.trim()) {
        callback(new Error('請選擇或輸入品名'))
      } else {
        callback()
      }
    },
    trigger: 'blur',
  }],
  sizes: [{
    validator: (_rule: any, _value: any, callback: any) => {
      const enabled = itemForm.sizes.filter(s => s.enabled)
      if (enabled.length === 0) {
        callback(new Error('請至少勾選一個尺寸'))
      } else {
        const invalid = enabled.find(s => s.price <= 0)
        if (invalid) {
          callback(new Error(`尺寸「${invalid.size_name}」的價格必須大於 0`))
        } else {
          callback()
        }
      }
    },
    trigger: 'change',
  }],
}

const handleSubmitItem = async () => {
  const valid = await itemFormRef.value?.validate().catch(() => false)
  if (!valid) return

  const enabledSizes = itemForm.sizes.filter(s => s.enabled)

  const body = {
    drink_item_id: itemForm.drink_item_id ?? undefined,
    drink_item_name: itemForm.drink_item_id ? undefined : itemForm.drink_item_name.trim() || undefined,
    description: itemForm.description || undefined,
    sort: itemForm.sort,
    max_topping_count: itemForm.max_topping_count,
    sizes: enabledSizes.map(s => ({ size_id: s.size_id, price: s.price })),
    sugar_ids: itemForm.sugar_ids,
    ice_ids: itemForm.ice_ids,
    topping_ids: itemForm.topping_ids,
  }

  clearItemErrors()
  startLoading()

  if (itemDialogMode.value === 'create') {
    const { error } = await api.POST('/api/admin/shops/{shopId}/categories/{categoryId}/items', {
      params: { path: { shopId, categoryId: itemDialogCategoryId.value } },
      body,
    })
    await stopLoading()
    if (error) { handleItemError(error, '新增品項失敗'); return }
    showSuccess('品項新增成功')
  } else {
    const { error } = await api.PUT('/api/admin/shops/{shopId}/categories/{categoryId}/items/{itemId}', {
      params: { path: { shopId, categoryId: itemDialogCategoryId.value, itemId: itemDialogItemId.value } },
      body,
    })
    await stopLoading()
    if (error) { handleItemError(error, '更新品項失敗'); return }
    showSuccess('品項更新成功')
  }

  itemDialogVisible.value = false
  await fetchMenu()
  // 重新載入選項（可能新增了 DrinkItem）
  await fetchOptions()
}

// ==================== 初始化 ====================

onMounted(async () => {
  await fetchShop()
  await Promise.all([fetchMenu(), fetchOptions()])
  fetchLoading.value = false
})
</script>

<template>
  <div>
    <AppBreadcrumb />

    <!-- 店家基本資訊 -->
    <el-card v-loading="fetchLoading" shadow="never">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <div style="display: flex; align-items: center; gap: 8px">
            <el-button text @click="router.push('/shop/list')"><el-icon><ArrowLeft /></el-icon>返回</el-button>
            <span>店家基本資訊</span>
          </div>
          <div style="display: flex; align-items: center; gap: 12px">
            <el-button size="small" @click="router.push(`/shop/${shopId}/images`)">圖庫管理</el-button>
            <AppTimestamp v-if="createdAt" :created-at="createdAt" :updated-at="updatedAt" />
          </div>
        </div>
      </template>

      <el-form ref="formRef" :model="form" :rules="rules" :label-position="labelPosition" label-width="120px" size="large">
        <el-row :gutter="24">
          <el-col :span="24">
            <el-form-item label="店家名稱" prop="name" :error="serverErrors.name">
              <el-input v-model="form.name" placeholder="請輸入店家名稱" maxlength="100" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="電話" prop="phone">
              <el-input v-model="form.phone" placeholder="請輸入聯絡電話" maxlength="20" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="地址" prop="address">
              <el-input v-model="form.address" placeholder="請輸入地址" maxlength="200" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="備註" prop="note">
              <el-input v-model="form.note" type="textarea" :rows="3" placeholder="請輸入備註" maxlength="500" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="狀態" prop="status">
              <el-radio-group v-model="form.status">
                <el-radio :value="1">上架</el-radio>
                <el-radio :value="2">下架</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="排序" prop="sort">
              <el-input-number v-model="form.sort" :min="0" :precision="0" style="width: 180px; max-width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="每種加料上限" prop="max_topping_per_item">
              <el-input-number v-model="form.max_topping_per_item" :min="1" :precision="0" style="width: 180px; max-width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item>
          <el-button type="primary" @click="handleSubmitShop">儲存店家資訊</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 菜單管理 -->
    <el-card v-loading="menuLoading" shadow="never" style="margin-top: 16px">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <span>菜單管理</span>
        </div>
      </template>

      <!-- 新增分類 -->
      <div v-if="can(MENU.ShopList, 'create')" style="display: flex; gap: 8px; margin-bottom: 16px">
        <el-input
          v-model="newCategoryName"
          placeholder="輸入新分類名稱"
          style="width: 240px"
          @keyup.enter="handleAddCategory"
        />
        <el-button type="primary" @click="handleAddCategory">新增分類</el-button>
      </div>

      <!-- 分類列表 -->
      <div v-if="categories.length === 0" style="text-align: center; color: #999; padding: 40px 0">
        尚未建立分類，請先新增分類
      </div>

      <draggable
        v-model="categories"
        item-key="id"
        handle=".drag-handle-category"
        :animation="200"
        @end="handleCategoryDragEnd"
      >
        <template #item="{ element: cat }">
          <div class="category-block">
            <!-- 分類標題 -->
            <div class="category-header" @click="toggleCategory(cat.id!)">
              <div class="category-header-left">
                <el-icon class="drag-handle-category" @click.stop style="cursor: grab">
                  <Rank />
                </el-icon>
                <el-icon>
                  <ArrowRight v-if="!expandedCategories.has(cat.id!)" />
                  <ArrowDown v-else />
                </el-icon>

                <template v-if="editingCategoryId === cat.id">
                  <el-input
                    v-model="editingCategoryName"
                    size="small"
                    style="width: 200px"
                    @click.stop
                    @keyup.enter="handleUpdateCategory(cat)"
                    @keyup.escape="cancelEditCategory"
                  />
                  <el-button size="small" type="primary" @click.stop="handleUpdateCategory(cat)">確認</el-button>
                  <el-button size="small" @click.stop="cancelEditCategory">取消</el-button>
                </template>
                <template v-else>
                  <span class="category-name">{{ cat.name }}</span>
                  <el-tag size="small" type="info">{{ cat.items?.length ?? 0 }} 品項</el-tag>
                </template>
              </div>
              <div class="category-header-right" @click.stop>
                <el-button v-if="can(MENU.ShopList, 'update') && editingCategoryId !== cat.id" size="small" @click="startEditCategory(cat)">編輯</el-button>
                <el-button v-if="can(MENU.ShopList, 'delete')" size="small" type="danger" @click="handleDeleteCategory(cat)">刪除</el-button>
              </div>
            </div>

            <!-- 品項列表 -->
            <div v-if="expandedCategories.has(cat.id!)" class="category-items">
              <div v-if="can(MENU.ShopList, 'create')" style="margin-bottom: 8px">
                <el-button size="small" type="primary" icon="Plus" @click="openItemDialog(cat.id!)">新增品項</el-button>
              </div>

              <div v-if="!cat.items?.length" style="color: #999; padding: 12px 0; text-align: center">
                此分類尚無品項
              </div>

              <draggable
                v-model="cat.items"
                item-key="id"
                handle=".drag-handle-item"
                :animation="200"
                @end="handleItemDragEnd(cat)"
              >
                <template #item="{ element: item }">
                  <div class="menu-item">
                    <div class="menu-item-header" @click="toggleItem(item.id!)">
                      <div class="menu-item-left">
                        <el-icon class="drag-handle-item" @click.stop style="cursor: grab">
                          <Rank />
                        </el-icon>
                        <el-icon>
                          <ArrowRight v-if="!expandedItems.has(item.id!)" />
                          <ArrowDown v-else />
                        </el-icon>
                        <span class="item-name">{{ item.drink_item_name }}</span>
                        <span class="item-sizes">
                          {{ item.sizes?.map(s => `${s.size_name} $${s.price}`).join(' / ') }}
                        </span>
                      </div>
                      <div class="menu-item-right" @click.stop>
                        <el-button v-if="can(MENU.ShopList, 'update')" size="small" @click="openItemDialog(cat.id!, item)">編輯</el-button>
                        <el-button v-if="can(MENU.ShopList, 'delete')" size="small" type="danger" @click="handleDeleteMenuItem(cat, item)">刪除</el-button>
                      </div>
                    </div>

                    <!-- 品項細項 -->
                    <div v-if="expandedItems.has(item.id!)" class="menu-item-detail">
                      <div v-if="item.description" class="detail-row">
                        <span class="detail-label">描述：</span>{{ item.description }}
                      </div>
                      <div class="detail-row">
                        <span class="detail-label">尺寸：</span>
                        <el-tag v-for="s in item.sizes" :key="s.size_id" size="small" style="margin-right: 4px">
                          {{ s.size_name }} ${{ s.price }}
                        </el-tag>
                      </div>
                      <div class="detail-row">
                        <span class="detail-label">甜度：</span>
                        <span>{{ item.sugar_ids?.length ?? 0 }} 種</span>
                      </div>
                      <div class="detail-row">
                        <span class="detail-label">冰塊：</span>
                        <span>{{ item.ice_ids?.length ?? 0 }} 種</span>
                      </div>
                      <div class="detail-row">
                        <span class="detail-label">加料：</span>
                        <span>{{ item.topping_ids?.length ?? 0 }} 種（上限 {{ item.max_topping_count ?? '-' }} 份）</span>
                      </div>
                    </div>
                  </div>
                </template>
              </draggable>
            </div>
          </div>
        </template>
      </draggable>
    </el-card>

    <!-- 品項編輯 Dialog -->
    <el-dialog
      v-model="itemDialogVisible"
      :title="itemDialogMode === 'create' ? '新增品項' : '編輯品項'"
      width="700px"
      destroy-on-close
    >
      <el-form ref="itemFormRef" :model="itemForm" :rules="itemRules" label-width="100px" size="default">
        <!-- 品名 -->
        <el-form-item label="品名" prop="drink_item_name" :error="itemServerErrors.drink_item_name || itemServerErrors.drink_item_id">
          <el-autocomplete
            v-model="itemForm.drink_item_name"
            :fetch-suggestions="handleDrinkItemSearch"
            placeholder="搜尋或輸入新品名"
            style="width: 100%"
            @select="handleDrinkItemSelect"
            @input="handleDrinkItemInput"
          >
            <template #default="{ item }">
              <span>{{ item.value }}</span>
            </template>
          </el-autocomplete>
          <div v-if="!itemForm.drink_item_id && itemForm.drink_item_name" style="font-size: 12px; color: var(--el-color-warning); margin-top: 4px">
            將自動建立新的通用品名「{{ itemForm.drink_item_name }}」
          </div>
        </el-form-item>

        <!-- 描述 -->
        <el-form-item label="描述">
          <el-input v-model="itemForm.description" type="textarea" :rows="2" placeholder="品項描述" maxlength="200" />
        </el-form-item>

        <!-- 排序 -->
        <el-form-item label="排序">
          <el-input-number v-model="itemForm.sort" :min="0" :precision="0" style="width: 180px" />
        </el-form-item>

        <!-- 加料總數上限 -->
        <el-form-item label="加料上限">
          <el-input-number v-model="itemForm.max_topping_count" :min="1" :precision="0" style="width: 180px" />
          <FormHint>單杯飲料最多可加的加料總份數</FormHint>
        </el-form-item>

        <!-- 尺寸價格 -->
        <el-form-item label="尺寸價格" prop="sizes">
          <el-table :data="itemForm.sizes" stripe size="default" style="width: 100%">
            <el-table-column label="啟用" width="70" align="center">
              <template #default="{ row }">
                <el-checkbox v-model="row.enabled" />
              </template>
            </el-table-column>
            <el-table-column prop="size_name" label="尺寸名稱" width="120" />
            <el-table-column label="價格">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.price"
                  :min="0"
                  :precision="0"
                  :disabled="!row.enabled"
                  style="width: 180px"
                />
              </template>
            </el-table-column>
          </el-table>
        </el-form-item>

        <!-- 甜度 -->
        <el-form-item label="甜度">
          <div style="width: 100%; display: flex; gap: 8px; align-items: center; margin-top: 4px">
            <el-button size="small" @click="itemForm.sugar_ids = allSugars.map(s => s.id!)">全選</el-button>
            <el-button size="small" @click="itemForm.sugar_ids = []">取消全選</el-button>
          </div>
          <el-checkbox-group v-model="itemForm.sugar_ids">
            <el-checkbox v-for="s in allSugars" :key="s.id" :value="s.id!">{{ s.name }}</el-checkbox>
          </el-checkbox-group>
          <FormHint>這是測試提示字</FormHint>
        </el-form-item>

        <!-- 冰塊 -->
        <el-form-item label="冰塊">
          <div style="width: 100%; display: flex; gap: 8px; align-items: center; margin-top: 4px">
            <el-button size="small" @click="itemForm.ice_ids = allIces.map(i => i.id!)">全選</el-button>
            <el-button size="small" @click="itemForm.ice_ids = []">取消全選</el-button>
          </div>
          <el-checkbox-group v-model="itemForm.ice_ids">
            <el-checkbox v-for="i in allIces" :key="i.id" :value="i.id!">{{ i.name }}</el-checkbox>
          </el-checkbox-group>
        </el-form-item>

        <!-- 加料 -->
        <el-form-item label="加料">
          <div style="width: 100%; display: flex; gap: 8px; align-items: center; margin-top: 4px">
            <el-button size="small" @click="itemForm.topping_ids = allToppings.map(t => t.id!)">全選</el-button>
            <el-button size="small" @click="itemForm.topping_ids = []">取消全選</el-button>
          </div>
          <el-checkbox-group v-model="itemForm.topping_ids">
            <el-checkbox v-for="t in allToppings" :key="t.id" :value="t.id!">{{ t.name }}</el-checkbox>
          </el-checkbox-group>
        </el-form-item>
      </el-form>

      <!-- 圖片管理（僅編輯模式 + 已有 drink_item_id 時顯示） -->
      <div v-if="itemDialogMode === 'edit' && itemForm.drink_item_id" style="margin-top: 16px">
        <ShopMenuItemImageStrip
          :key="`${shopId}-${itemForm.drink_item_id}`"
          :shop-id="shopId"
          :drink-item-id="itemForm.drink_item_id"
        />
      </div>

      <template #footer>
        <el-button @click="itemDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmitItem">
          {{ itemDialogMode === 'create' ? '新增' : '儲存' }}
        </el-button>
      </template>
    </el-dialog>

  </div>
</template>

<style scoped>
.category-block {
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 4px;
  margin-bottom: 12px;
}

.category-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: var(--el-fill-color-light);
  cursor: pointer;
  user-select: none;
}

.category-header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.category-header-right {
  display: flex;
  gap: 4px;
}

.category-name {
  font-weight: 600;
  font-size: 15px;
}

.category-items {
  padding: 8px 16px;
}

.menu-item {
  border-bottom: 1px solid var(--el-border-color-extra-light);
}

.menu-item:last-child {
  border-bottom: none;
}

.menu-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  cursor: pointer;
}

.menu-item-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.menu-item-right {
  display: flex;
  gap: 4px;
}

.item-name {
  font-weight: 500;
}

.item-sizes {
  color: var(--el-text-color-secondary);
  font-size: 13px;
}

.menu-item-detail {
  padding: 8px 0 12px 24px;
  font-size: 13px;
}

.detail-row {
  margin-bottom: 4px;
}

.detail-label {
  color: var(--el-text-color-secondary);
  margin-right: 4px;
}

.drag-handle-category,
.drag-handle-item {
  color: var(--el-text-color-placeholder);
  cursor: grab;
}

.drag-handle-category:hover,
.drag-handle-item:hover {
  color: var(--el-text-color-regular);
}

.sortable-ghost {
  opacity: 0.4;
  background: var(--el-color-primary-light-9);
}

:deep(.el-dialog) .el-form-item {
  margin-bottom: 0;
  margin-top: 16px;
}

:deep(.el-dialog) .el-form-item:first-child {
  margin-top: 0;
}

</style>
