import httpClient from './http'
import type { Subject, CreateSubjectRequest } from './types'

export async function listSubjects(organizationId: string): Promise<Subject[]> {
  const response = await httpClient.get<Subject[]>('/subjects', {
    headers: { 'X-Organization-Id': organizationId }
  })
  return response.data
}

export async function getSubject(id: string): Promise<Subject> {
  const response = await httpClient.get<Subject>(`/subjects/${id}`)
  return response.data
}

export async function createSubject(
  organizationId: string,
  request: CreateSubjectRequest
): Promise<Subject> {
  const response = await httpClient.post<Subject>('/subjects', request, {
    headers: { 'X-Organization-Id': organizationId }
  })
  return response.data
}