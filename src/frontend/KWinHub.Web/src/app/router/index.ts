import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import DefaultLayout from '@/app/layouts/DefaultLayout.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: DefaultLayout,
    children: [
      {
        path: '',
        name: 'dashboard',
        component: () => import('@/features/dashboard/DashboardView.vue')
      },
      {
        path: 'subjects',
        name: 'subjects',
        component: () => import('@/features/subjects/SubjectsView.vue')
      },
      {
        path: 'devices',
        name: 'devices',
        component: () => import('@/features/devices/DevicesView.vue')
      },
      {
        path: 'test-sessions',
        name: 'test-sessions',
        component: () => import('@/features/test-sessions/TestSessionsView.vue')
      },
      {
        path: 'analytics',
        name: 'analytics',
        component: () => import('@/features/analytics/AnalyticsView.vue')
      },
      {
        path: 'reports',
        name: 'reports',
        component: () => import('@/features/reports/ReportsView.vue')
      },
      {
        path: 'organizations',
        name: 'organizations',
        component: () => import('@/features/organizations/OrganizationsView.vue')
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router