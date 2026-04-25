/**
 * 全域規則：el-input-number
 * - 只允許整數（可負數），不允許小數點、千分位
 * - 清空或非數字時自動補回 0
 * 透過事件委派監聯所有 .el-input-number 內的 input 事件
 */
export default defineNuxtPlugin(() => {
  // 輸入時即時過濾：只保留數字和開頭的負號
  document.addEventListener('input', (e) => {
    const target = e.target as HTMLInputElement
    if (
      target.tagName === 'INPUT' &&
      target.closest('.el-input-number')
    ) {
      // 只保留負號（僅限開頭）和數字
      const cleaned = target.value.replace(/[^0-9-]/g, '').replace(/(?!^)-/g, '')
      if (cleaned !== target.value) {
        target.value = cleaned
        target.dispatchEvent(new Event('input', { bubbles: true }))
      }
    }
  }, true)

  // blur 時：清空或非整數補回 0
  document.addEventListener('blur', (e) => {
    const target = e.target as HTMLInputElement
    if (
      target.tagName === 'INPUT' &&
      target.closest('.el-input-number')
    ) {
      const value = target.value.trim()
      if (value === '' || value === '-' || !/^-?\d+$/.test(value)) {
        target.value = '0'
        target.dispatchEvent(new Event('input', { bubbles: true }))
        target.dispatchEvent(new Event('change', { bubbles: true }))
      }
    }
  }, true)
})
