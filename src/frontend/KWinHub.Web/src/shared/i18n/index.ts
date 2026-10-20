import { createI18n } from 'vue-i18n'

const messages = {
  en: {
    nav: {
      dashboard: 'Dashboard',
      subjects: 'Subjects',
      devices: 'Devices',
      testSessions: 'Test Sessions',
      analytics: 'Analytics',
      reports: 'Reports',
      organizations: 'Organizations'
    },
    common: {
      search: 'Search',
      create: 'Create',
      edit: 'Edit',
      delete: 'Delete',
      cancel: 'Cancel',
      confirm: 'Confirm',
      actions: 'Actions'
    }
  }
}

const i18n = createI18n({
  legacy: false,
  locale: 'en',
  fallbackLocale: 'en',
  messages
})

export default i18n