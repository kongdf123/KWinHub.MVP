import { http } from '@/lib/api/http';

export interface SubjectDto {
  id: string;
  organizationId: string;
  firstName: string;
  lastName: string;
  subjectType: string;
  externalId: string | null;
  displayName: string | null;
  dateOfBirth: string | null;
  sex: string | null;
  status: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateSubjectRequest {
  firstName: string;
  lastName: string;
  subjectType: string;
  externalId?: string | null;
  displayName?: string | null;
  dateOfBirth?: string | null;
  sex?: string | null;
}

export interface UpdateSubjectRequest extends CreateSubjectRequest {}

export interface SubjectListParams {
  search?: string;
  status?: string;
  subjectType?: string;
}

function buildQuery(params: SubjectListParams): string {
  const q = new URLSearchParams();
  if (params.search) q.set('search', params.search);
  if (params.status) q.set('status', params.status);
  if (params.subjectType) q.set('subjectType', params.subjectType);
  const s = q.toString();
  return s ? `?${s}` : '';
}

export const subjectsApi = {
  list: (params: SubjectListParams = {}) =>
    http.get<SubjectDto[]>(`/subjects${buildQuery(params)}`),
  getById: (id: string) =>
    http.get<SubjectDto>(`/subjects/${id}`),
  create: (request: CreateSubjectRequest) =>
    http.post<SubjectDto>('/subjects', request),
  update: (id: string, request: UpdateSubjectRequest) =>
    http.put<SubjectDto>(`/subjects/${id}`, request),
  archive: (id: string) =>
    http.post<void>(`/subjects/${id}/archive`),
  restore: (id: string) =>
    http.post<void>(`/subjects/${id}/restore`),
};