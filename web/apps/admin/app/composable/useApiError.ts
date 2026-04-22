/**
 * 處理 API 錯誤：
 * - 有 errors（欄位驗證）→ 設定到 serverErrors，透過 :error prop 顯示在 el-form-item
 * - 有 message → ElMessageBox alert 顯示
 * - 都沒有 → 顯示預設訊息
 */
export function useApiError() {
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
        serverErrors[field] = messages[0]
      }
    }
  }

  const handleError = (err: any, fallbackMsg = '操作失敗') => {
    // openapi-fetch 的 error 物件直接就是 response body
    const data = err?.data || err?.response?._data || err || {}

    // 欄位驗證錯誤 → 設定 serverErrors
    if (data.errors && typeof data.errors === 'object') {
      setErrors(data.errors)
      return
    }

    // 一般錯誤訊息 → MessageBox alert
    const msg = data.message || fallbackMsg
    ElMessageBox.alert(msg, '錯誤提示', {
      confirmButtonText: '確定',
      type: 'error',
    })
  }

  return { serverErrors, handleError, clearErrors }
}
