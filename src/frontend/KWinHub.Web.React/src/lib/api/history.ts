import { http } from '@/lib/api/http';

export interface HistoryMetricResponse {
  metricId: string;
  code: string;
  name: string;
  unit: string;
  value: number;
  valueType: string;
}

export interface HistorySessionResponse {
  id: string;
  protocolId: string;
  protocolName: string;
  startedAt: string;
  status: string;
  metrics: HistoryMetricResponse[];
}

export interface HistoryListResponse {
  subjectId: string;
  total: number;
  sessions: HistorySessionResponse[];
}

export interface HistoryFilters {
  protocolId?: string;
  from?: string;
  to?: string;
  status?: string;
}

export interface MetricStatisticsResponse {
  sampleCount: number;
  mean: number | null;
  max: number | null;
}

export interface MetricComparisonPointResponse {
  sessionId: string;
  sessionDate: string;
  value: number;
}

export interface MetricExclusionResponse {
  sessionId: string;
  reason: string;
}

export interface CompatibleMetricGroupResponse {
  metricCode: string;
  metricName: string;
  unit: string;
  valueType: string;
  points: MetricComparisonPointResponse[];
  exclusions: MetricExclusionResponse[];
  statistics: MetricStatisticsResponse;
}

export interface ComparisonResponse {
  subjectId: string;
  sessionIds: string[];
  compatibleMetrics: CompatibleMetricGroupResponse[];
}

function buildHistoryQuery(filters: HistoryFilters): string {
  const q = new URLSearchParams();
  if (filters.protocolId) q.set('protocolId', filters.protocolId);
  if (filters.from) q.set('from', filters.from);
  if (filters.to) q.set('to', filters.to);
  if (filters.status) q.set('status', filters.status);
  const s = q.toString();
  return s ? `?${s}` : '';
}

export const historyApi = {
  listSubjectHistory: (subjectId: string, filters: HistoryFilters = {}) =>
    http.get<HistoryListResponse>(`/history/subjects/${subjectId}${buildHistoryQuery(filters)}`),

  compareSessions: (subjectId: string, sessionIds: string[]) => {
    const qs = sessionIds.map((id) => `sessionIds=${id}`).join('&');
    return http.get<ComparisonResponse>(`/history/subjects/${subjectId}/comparison?${qs}`);
  },
};