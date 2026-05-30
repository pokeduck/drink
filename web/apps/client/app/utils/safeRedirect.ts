/**
 * 驗證 redirect query value 為相對路徑：
 * - MUST 以 `/` 開頭
 * - MUST NOT 以 `//` 開頭（protocol-relative URL）
 *
 * 不通過則回傳 fallback（預設 `/`），避免 open redirect 攻擊。
 */
export function safeRedirect(value: unknown, fallback = '/'): string {
  if (typeof value !== 'string') return fallback
  if (!value.startsWith('/')) return fallback
  if (value.startsWith('//')) return fallback
  return value
}
