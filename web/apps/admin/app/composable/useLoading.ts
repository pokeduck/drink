/**
 * 帶最低顯示時間的 loading 狀態
 * @param minDuration 最低顯示毫秒數，預設 1000
 */
export function useLoading(minDuration = 1000) {
  const loading = ref(false)
  let startTime = 0

  const start = () => {
    loading.value = true
    startTime = Date.now()
  }

  const stop = () => {
    const elapsed = Date.now() - startTime
    const remaining = Math.max(0, minDuration - elapsed)
    setTimeout(() => {
      loading.value = false
    }, remaining)
  }

  return { loading, start, stop }
}
