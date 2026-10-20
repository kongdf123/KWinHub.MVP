<script setup lang="ts">
</script>

<template>
  <div>
    <h2>Dashboard</h2>
    <el-row :gutter="16">
      <el-col :span="6">
        <el-card>
          <el-statistic title="Organizations" :value="1" />
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <el-statistic title="Subjects" :value="0" />
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <el-statistic title="Sessions" :value="0" />
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <el-statistic title="Devices" :value="0" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>