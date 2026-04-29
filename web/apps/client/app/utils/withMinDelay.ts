/**
 * 包裝 async 操作，確保至少 X ms 後才 resolve。
 * 用來避免「太快回來 → loading spinner 閃一下」的視覺體驗問題。
 *
 * 用法：
 *   await withMinDelay(api.save(), 1000)
 *   // 或 const result = await withMinDelay(api.fetch())
 */
export async function withMinDelay<T>(promise: Promise<T>, ms = 1000): Promise<T> {
  const [result] = await Promise.all([
    promise,
    new Promise<void>((resolve) => setTimeout(resolve, ms))
  ])
  return result
}
