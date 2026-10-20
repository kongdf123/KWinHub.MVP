import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAppStore = defineStore('app', () => {
  const activeOrganizationId = ref<string>('00000000-0000-0000-0000-000000000000')
  const locale = ref<string>('en')

  function setActiveOrganization(id: string) {
    activeOrganizationId.value = id
  }

  function setLocale(lang: string) {
    locale.value = lang
  }

  return {
    activeOrganizationId,
    locale,
    setActiveOrganization,
    setLocale
  }
})