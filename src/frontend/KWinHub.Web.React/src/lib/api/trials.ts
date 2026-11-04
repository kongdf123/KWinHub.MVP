import { http } from '@/lib/api/http';

export interface TrialDto {
  id: string;
  organizationId: string;
  sessionId: string;
  trialIndex: number;
  status: string;
  externalId: string | null;
  startedAt: string | null;
  completedAt: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateTrialRequest {
  sessionId: string;
  trialIndex: number;
  externalId?: string | null;
  notes?: string | null;
}

export const trialsApi = {
  listBySession: (sessionId: string) =>
    http.get<TrialDto[]>(`/trials/by-session/${sessionId}`),
  getById: (id: string) =>
    http.get<TrialDto>(`/trials/${id}`),
  create: (request: CreateTrialRequest) =>
    http.post<TrialDto>('/trials', request),
  complete: (id: string) =>
    http.post<TrialDto>(`/trials/${id}/complete`),
  fail: (id: string) =>
    http.post<TrialDto>(`/trials/${id}/fail`),
};