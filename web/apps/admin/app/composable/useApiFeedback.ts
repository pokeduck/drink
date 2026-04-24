/**
 * 統一 API 回饋 composable
 *
 * - 成功 → ElNotification（右上角）
 * - 欄位驗證錯誤（400 + errors）→ serverErrors inline 紅字
 * - 通用錯誤 → ElMessageBox.alert（有 Dialog 時 modal:false 避免疊灰）
 * - 寫入操作 → fullscreen loading（最低 1 秒）
 */
const MIN_LOADING_DURATION = 1000

export function useApiFeedback() {
  // ─── 欄位驗證錯誤（inline 紅字）───
  const serverErrors = reactive<Record<string, string>>({})

  const clearErrors = () => {
    for (const key of Object.keys(serverErrors)) {
      delete serverErrors[key]
    }
  }

  const setErrors = (errors: Record<string, string[]>) => {
    clearErrors()
    for (const [field, messages] of Object.entries(errors)) {
      if (messages.length > 0) {
        serverErrors[field] = messages[0]!
      }
    }
  }

  // ─── 成功提示（ElNotification 右上角）───
  const showSuccess = (message: string) => {
    ElNotification({
      type: 'success',
      title: '成功',
      message,
      position: 'top-right',
      offset: 76, // 避開 header 60px + 16px 間距
    })
  }

  // ─── 錯誤提示（ElMessageBox.alert）───
  const showErrorAlert = (msg: string) => {
    // 如果有 Dialog 開啟，將 MessageBox 掛到 Dialog 內部
    // 並關閉 modal 避免多層灰疊加（Dialog 本身已有遮罩）
    const dialogEl = document.querySelector('.el-dialog') as HTMLElement | null
    const hasDialog = !!dialogEl

    ElMessageBox.alert(msg, '錯誤提示', {
      confirmButtonText: '確定',
      type: 'error',
      appendTo: hasDialog ? dialogEl! : document.body,
      modal: !hasDialog,
      customClass: hasDialog ? 'api-error-in-dialog' : '',
    }).catch(() => {})
  }

  /**
   * 處理 API 錯誤
   * - 有 errors → 設定 serverErrors（inline 紅字）
   * - 無 errors → ElMessageBox alert
   */
  const handleError = (err: any, fallbackMsg = '操作失敗') => {
    const data = err?.data || err?.response?._data || err || {}

    if (data.errors && typeof data.errors === 'object') {
      setErrors(data.errors)
      return
    }

    const msg = data.message || fallbackMsg
    showErrorAlert(msg)
  }

  // ─── Fullscreen Loading（最低 1 秒）───
  let loadingInstance: ReturnType<typeof ElLoading.service> | null = null
  let loadingStartTime = 0

  const startLoading = () => {
    loadingStartTime = Date.now()
    loadingInstance = ElLoading.service({
      fullscreen: true,
      lock: true,
      text: '處理中...',
      background: 'rgba(255, 255, 255, 0.8)',
    })
    // 拉高 z-index 確保蓋住 Dialog
    const el = loadingInstance.$el as HTMLElement | undefined
    if (el) {
      el.style.zIndex = '99998'
    }
  }

  const stopLoading = (): Promise<void> => {
    return new Promise((resolve) => {
      const elapsed = Date.now() - loadingStartTime
      const remaining = Math.max(0, MIN_LOADING_DURATION - elapsed)
      setTimeout(() => {
        loadingInstance?.close()
        loadingInstance = null
        resolve()
      }, remaining)
    })
  }

  return {
    serverErrors,
    clearErrors,
    showSuccess,
    handleError,
    startLoading,
    stopLoading,
  }
}
