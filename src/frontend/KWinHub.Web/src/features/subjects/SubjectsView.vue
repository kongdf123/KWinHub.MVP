<script setup lang="ts">
import { ref, computed } from 'vue'
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { ElMessage } from 'element-plus'
import { listSubjects, createSubject } from '@/shared/api/subjects'
import type { CreateSubjectRequest } from '@/shared/api/types'

const organizationId = '00000000-0000-0000-0000-000000000000'
const queryClient = useQueryClient()

const dialogVisible = ref(false)
const form = ref<CreateSubjectRequest>({
  firstName: '',
  lastName: '',
  subjectType: 'Athlete'
})

const { data: subjects, isLoading } = useQuery({
  queryKey: ['subjects'],
  queryFn: () => listSubjects(organizationId)
})

const createMutation = useMutation({
  mutationFn: (req: CreateSubjectRequest) => createSubject(organizationId, req),
  onSuccess: () => {
    ElMessage.success('Subject created successfully')
    dialogVisible.value = false
    queryClient.invalidateQueries({ queryKey: ['subjects'] })
  },
  onError: () => {
    ElMessage.error('Failed to create subject')
  }
})

const subjectTypes = ['Athlete', 'Patient', 'Participant', 'ResearchSubject', 'Other']

const tableData = computed(() => subjects.value ?? [])

function handleCreate() {
  createMutation.mutate(form.value)
}
</script>

<template>
  <div class="subjects-view">
    <div class="page-header">
      <h2>Subjects</h2>
      <el-button type="primary" @click="dialogVisible = true">
        <el-icon><Plus /></el-icon>
        Add Subject
      </el-button>
    </div>

    <el-card>
      <el-table :data="tableData" v-loading="isLoading" stripe>
        <el-table-column prop="firstName" label="First Name" />
        <el-table-column prop="lastName" label="Last Name" />
        <el-table-column prop="subjectType" label="Type" />
        <el-table-column prop="sex" label="Sex" />
        <el-table-column prop="status" label="Status" />
        <el-table-column prop="createdAt" label="Created" width="180" />
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" title="Add Subject" width="500px">
      <el-form :model="form" label-width="120px">
        <el-form-item label="First Name" required>
          <el-input v-model="form.firstName" />
        </el-form-item>
        <el-form-item label="Last Name" required>
          <el-input v-model="form.lastName" />
        </el-form-item>
        <el-form-item label="Type" required>
          <el-select v-model="form.subjectType">
            <el-option
              v-for="type in subjectTypes"
              :key="type"
              :label="type"
              :value="type"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Sex">
          <el-select v-model="form.sex" clearable>
            <el-option label="Male" value="Male" />
            <el-option label="Female" value="Female" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">Cancel</el-button>
        <el-button type="primary" :loading="createMutation.isPending" @click="handleCreate">
          Create
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.subjects-view {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.page-header h2 {
  margin: 0;
}
</style>