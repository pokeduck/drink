export function useFormLayout() {
  const isMobile = ref(false)

  const check = () => {
    isMobile.value = window.innerWidth < 768
  }

  onMounted(() => {
    check()
    window.addEventListener('resize', check)
  })

  onUnmounted(() => {
    window.removeEventListener('resize', check)
  })

  const labelPosition = computed(() => (isMobile.value ? 'top' : 'right'))

  return { labelPosition }
}
