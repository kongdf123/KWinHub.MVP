import { http } from '@/lib/api/http';

export interface GroupDto {
  id: string;
  organizationId: string;
  name: string;
  description: string | null;
  groupType: string | null;
  status: string;
  memberCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface GroupMemberDto {
  membershipId: string;
  groupId: string;
  subjectId: string;
  role: string | null;
}

export interface CreateGroupRequest {
  name: string;
  description?: string | null;
  groupType?: string | null;
}

export interface UpdateGroupRequest extends CreateGroupRequest {}

export interface AddGroupMemberRequest {
  subjectId: string;
  role?: string | null;
}

export interface GroupListParams {
  search?: string;
  status?: string;
  groupType?: string;
}

function buildQuery(params: GroupListParams): string {
  const q = new URLSearchParams();
  if (params.search) q.set('search', params.search);
  if (params.status) q.set('status', params.status);
  if (params.groupType) q.set('groupType', params.groupType);
  const s = q.toString();
  return s ? `?${s}` : '';
}

export const groupsApi = {
  list: (params: GroupListParams = {}) =>
    http.get<GroupDto[]>(`/groups${buildQuery(params)}`),
  getById: (id: string) =>
    http.get<GroupDto>(`/groups/${id}`),
  create: (request: CreateGroupRequest) =>
    http.post<GroupDto>('/groups', request),
  update: (id: string, request: UpdateGroupRequest) =>
    http.put<GroupDto>(`/groups/${id}`, request),
  archive: (id: string) =>
    http.post<void>(`/groups/${id}/archive`),
  restore: (id: string) =>
    http.post<void>(`/groups/${id}/restore`),
  listMembers: (id: string) =>
    http.get<GroupMemberDto[]>(`/groups/${id}/members`),
  addMember: (id: string, request: AddGroupMemberRequest) =>
    http.post<GroupMemberDto>(`/groups/${id}/members`, request),
  removeMember: (id: string, subjectId: string) =>
    http.delete(`/groups/${id}/members/${subjectId}`),
};