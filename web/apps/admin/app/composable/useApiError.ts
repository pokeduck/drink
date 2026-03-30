import type { FormInstance } from 'element-plus'

interface ApiErrorData {
  message?: string
  error?: string
  errors?: Record<string, string[]>
}

/**
 * 處理 API 錯誤：
 * - 有 errors（欄位驗證）→ 設定到 el-form 對應欄位
 * - 有 message → ElMessageBox alert 顯示
 * - 都沒有 → 顯示預設訊息
 */
export function useApiError() {
  const handleError = (err: any, formRef?: FormInstance, fallbackMsg = '操作失敗') => {
    const data: ApiErrorData = err?.data || err?.response?._data || {}

    // 欄位驗證錯誤 → 對應到 form fields
    if (data.errors && formRef) {
      const fields: Record<string, { message: string }[]> = {}
      for (const [field, messages] of Object.entries(data.errors)) {
        fields[field] = messages.map((msg) => ({ message: msg }))
      }
      // 透過 el-form 的 scrollToField + validateField 顯示錯誤
      for (const [field, messages] of Object.entries(data.errors)) {
        formRef.fields?.find((f) => f.prop === field)?.validateState
        // 使用內部方法設置欄位錯誤狀態
      }
      // 用更可靠的方式：直接呼叫 validateField 後覆蓋錯誤
      nextTick(() => {
        for (const [field, messages] of Object.entries(data.errors!)) {
          const formItem = formRef.fields?.find((f) => f.prop === field)
          if (formItem) {
            formItem.validateState = 'error'
            formItem.validateMessage = messages[0]
          }
        }
      })
      return
    }

    // 一般錯誤訊息 → MessageBox alert
    const msg = data.message || fallbackMsg
    ElMessageBox.alert(msg, '錯誤提示', {
      confirmButtonText: '確定',
      type: 'error',
    })
  }

  return { handleError }
}
