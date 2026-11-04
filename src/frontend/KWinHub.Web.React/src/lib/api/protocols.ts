import { http } from '@/lib/api/http';

export interface ProtocolDto {
  id: string;
  organizationId: string;
  name: string;
  code: string;
  version: string;
  category: string | null;
  sport: string | null;
  status: string;
  description: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateProtocolRequest {
  name: string;
  code: string;
  version: string;
  category?: string | null;
  sport?: string | null;
  description?: string | null;
}

export const protocolsApi = {
  list: () =>
    http.get<ProtocolDto[]>('/protocols'),
  getById: (id: string) =>
    http.get<ProtocolDto>(`/protocols/${id}`),
  create: (request: CreateProtocolRequest) =>
    http.post<ProtocolDto>('/protocols', request),
};