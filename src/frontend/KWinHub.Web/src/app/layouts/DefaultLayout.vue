<script setup lang="ts">
import { ref } from 'vue'

const activeMenu = ref('dashboard')

const menuItems = [
  { index: 'dashboard', label: 'Dashboard', icon: 'Odometer' },
  { index: 'subjects', label: 'Subjects', icon: 'User' },
  { index: 'devices', label: 'Devices', icon: 'Cpu' },
  { index: 'test-sessions', label: 'Test Sessions', icon: 'DataLine' },
  { index: 'analytics', label: 'Analytics', icon: 'TrendCharts' },
  { index: 'reports', label: 'Reports', icon: 'Document' },
  { index: 'organizations', label: 'Organizations', icon: 'OfficeBuilding' }
]
</script>

<template>
  <el-container class="layout-container">
    <el-aside width="240px" class="sidebar">
      <div class="logo">
        <h2>KWinHub</h2>
      </div>
      <el-menu
        :default-active="activeMenu"
        router
        class="sidebar-menu"
      >
        <el-menu-item
          v-for="item in menuItems"
          :key="item.index"
          :index="`/${item.index}`"
        >
          <el-icon><component :is="item.icon" /></el-icon>
          <span>{{ item.label }}</span>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <el-container>
      <el-header class="header">
        <div class="header-content">
          <span class="page-title">KWinHub</span>
          <div class="header-right">
            <el-dropdown>
              <span class="user-info">
                <el-icon><User /></el-icon>
                User
              </span>
            </el-dropdown>
          </div>
        </div>
      </el-header>

      <el-main class="main-content">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.layout-container {
  height: 100vh;
}

.sidebar {
  background-color: var(--kwin-sidebar-bg, #1e293b);
  color: #fff;
}

.logo {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.logo h2 {
  color: #fff;
  margin: 0;
}

.sidebar-menu {
  border-right: none;
  background-color: transparent;
}

.sidebar-menu .el-menu-item {
  color: #cbd5e1;
}

.sidebar-menu .el-menu-item.is-active {
  background-color: var(--kwin-primary, #6366f1);
  color: #fff;
}

.header {
  background-color: #fff;
  border-bottom: 1px solid var(--kwin-border, #e2e8f0);
  display: flex;
  align-items: center;
}

.header-content {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.page-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--kwin-text, #1e293b);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}

.main-content {
  background-color: var(--kwin-page-bg, #f8fafc);
}
</style>