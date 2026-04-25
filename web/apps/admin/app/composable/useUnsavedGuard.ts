import { onBeforeRouteLeave } from 'vue-router'

/**
 * 監聽表單是否有未儲存的修改，離開頁面時攔截確認
 * @param form - reactive 表單物件
 */
export function useUnsavedGuard(form: Record<string, any>) {
  let snapshot = ''
  const saved = ref(false)

  function takeSnapshot() {
    snapshot = JSON.stringify(form)
  }

  function isDirty() {
    return snapshot !== '' && snapshot !== JSON.stringify(form)
  }

  function markSaved() {
    saved.value = true
  }

  // 路由離開攔截
  onBeforeRouteLeave(async () => {
    if (saved.value || !isDirty()) return true

    try {
      await ElMessageBox.confirm('尚未儲存，確定要離開嗎？', '提示', {
        type: 'warning',
        confirmButtonText: '離開',
        cancelButtonText: '留在此頁',
      })
      return true
    } catch {
      return false
    }
  })

  // 瀏覽器關閉 / 重新整理攔截
  const beforeUnload = (e: BeforeUnloadEvent) => {
    if (!saved.value && isDirty()) {
      e.preventDefault()
    }
  }

  onMounted(() => {
    window.addEventListener('beforeunload', beforeUnload)
  })

  onUnmounted(() => {
    window.removeEventListener('beforeunload', beforeUnload)
  })

  return { takeSnapshot, markSaved }
}
