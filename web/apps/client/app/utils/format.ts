export function formatPrice(price: number): string {
  return `$${price}`
}

export function formatTime(isoString: string): string {
  const date = new Date(isoString)
  return date.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })
}
