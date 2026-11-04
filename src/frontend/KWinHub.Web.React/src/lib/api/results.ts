import { http } from '@/lib/api/http';

export interface MetricValueDto {
  id: string;
  metricDefinitionId: string | null;
  metricCode: string;
  metricName: string;
  valueType: string;
  scalarValue: number | null;
  leftValue: number | null;
  rightValue: number | null;
  categoricalValue: string | null;
  structuredValue: string | null;
  unit: string;
  side: string | null;
}

export interface ResultDto {
  id: string;
  organizationId: string;
  sessionId: string;
  protocolId: string;
  protocolVersion: string;
  algorithmVersion: string;
  status: string;
  qualityStatus: string | null;
  trialId: string | null;
  metrics: MetricValueDto[];
  createdAt: string;
  updatedAt: string | null;
}

export const resultsApi = {
  list: () =>
    http.get<ResultDto[]>('/results'),
  getById: (id: string) =>
    http.get<ResultDto>(`/results/${id}`),
  getBySession: (sessionId: string) =>
    http.get<ResultDto>(`/results/session/${sessionId}`),
};