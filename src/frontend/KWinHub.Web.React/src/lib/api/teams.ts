import { http } from '@/lib/api/http';

export interface TeamDto {
  id: string;
  organizationId: string;
  name: string;
  description: string | null;
  sportId: string | null;
  season: string | null;
  status: string;
  memberCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface TeamMemberDto {
  membershipId: string;
  teamId: string;
  subjectId: string;
  role: string | null;
}

export interface CreateTeamRequest {
  name: string;
  description?: string | null;
  sportId?: string | null;
  season?: string | null;
}

export interface UpdateTeamRequest extends CreateTeamRequest {}

export interface AddTeamMemberRequest {
  subjectId: string;
  role?: string | null;
}

export interface TeamListParams {
  search?: string;
  status?: string;
}

function buildQuery(params: TeamListParams): string {
  const q = new URLSearchParams();
  if (params.search) q.set('search', params.search);
  if (params.status) q.set('status', params.status);
  const s = q.toString();
  return s ? `?${s}` : '';
}

export const teamsApi = {
  list: (params: TeamListParams = {}) =>
    http.get<TeamDto[]>(`/teams${buildQuery(params)}`),
  getById: (id: string) =>
    http.get<TeamDto>(`/teams/${id}`),
  create: (request: CreateTeamRequest) =>
    http.post<TeamDto>('/teams', request),
  update: (id: string, request: UpdateTeamRequest) =>
    http.put<TeamDto>(`/teams/${id}`, request),
  archive: (id: string) =>
    http.post<void>(`/teams/${id}/archive`),
  restore: (id: string) =>
    http.post<void>(`/teams/${id}/restore`),
  listMembers: (id: string) =>
    http.get<TeamMemberDto[]>(`/teams/${id}/members`),
  addMember: (id: string, request: AddTeamMemberRequest) =>
    http.post<TeamMemberDto>(`/teams/${id}/members`, request),
  removeMember: (id: string, subjectId: string) =>
    http.delete(`/teams/${id}/members/${subjectId}`),
};