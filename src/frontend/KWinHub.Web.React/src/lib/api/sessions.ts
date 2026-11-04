import { http } from '@/lib/api/http';

export interface SessionDto {
  id: string;
  organizationId: string;
  subjectId: string;
  protocolId: string;
  protocolVersion: string;
  sourceId: string | null;
  deviceId: string | null;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  clientApplication: string | null;
  clientVersion: string | null;
  externalId: string | null;
  timezone: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface SessionStatusDto {
  sessionId: string;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateSessionRequest {
  subjectId: string;
  protocolId: string;
  protocolVersion: string;
  sourceId?: string | null;
  deviceId?: string | null;
  clientApplication?: string | null;
  clientVersion?: string | null;
  externalId?: string | null;
  timezone?: string | null;
  startedAt?: string | null;
}

export const sessionsApi = {
  list: () =>
    http.get<SessionDto[]>('/sessions'),
  listBySubject: (subjectId: string) =>
    http.get<SessionDto[]>(`/sessions/by-subject/${subjectId}`),
  getById: (id: string) =>
    http.get<SessionDto>(`/sessions/${id}`),
  getStatus: (id: string) =>
    http.get<SessionStatusDto>(`/sessions/${id}/status`),
  create: (request: CreateSessionRequest) =>
    http.post<SessionDto>('/sessions', request),
};