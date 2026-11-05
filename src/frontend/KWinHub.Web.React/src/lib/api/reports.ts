import { http } from '@/lib/api/http';

export interface ReportMetricValueResponse {
  id: string;
  sessionId: string;
  trialId: string | null;
  resultId: string | null;
  sessionStartedAt: string | null;
  metricCode: string;
  metricName: string;
  unit: string;
  value: number | null;
  required: boolean;
  planItemSortOrder: number;
}

export interface ReportAssetStateResponse {
  id: string;
  assetId: string;
  sessionId: string;
  trialId: string | null;
  assetType: string;
  status: string;
  mediaProviderId: string | null;
  playbackUrl: string | null;
  failureReason: string | null;
  processedAt: string | null;
  playbackAuthorized: boolean;
}

export interface ReportResponse {
  id: string;
  organizationId: string;
  planId: string | null;
  planCode: string;
  planVersion: number;
  planName: string;
  subjectId: string;
  generatedByUserId: string;
  title: string;
  status: 'Draft' | 'Complete' | 'Archived';
  notes: string | null;
  generatedAt: string;
  metricValueCount: number;
  assetStateCount: number;
}

export interface ReportDetailResponse {
  id: string;
  organizationId: string;
  planId: string | null;
  planCode: string;
  planVersion: number;
  planName: string;
  subjectId: string;
  generatedByUserId: string;
  title: string;
  status: 'Draft' | 'Complete' | 'Archived';
  notes: string | null;
  generatedAt: string;
  metricValues: ReportMetricValueResponse[];
  assetStates: ReportAssetStateResponse[];
}

export interface GenerateReportRequest {
  planId: string;
  subjectId: string;
  sessionIds: string[];
  title: string | null;
  notes: string | null;
}

export interface PlaybackAuthorizationResponse {
  assetId: string;
  authorized: boolean;
  playbackUrl: string | null;
  expiresAt: string | null;
  reason: string | null;
}

export const reportsApi = {
  list: () => http.get<ReportResponse[]>('/reports'),
  getById: (id: string) => http.get<ReportDetailResponse>(`/reports/${id}`),
  generate: (request: GenerateReportRequest) =>
    http.post<ReportDetailResponse>('/reports', request),
  requestPlayback: (reportId: string, assetId: string) =>
    http.post<PlaybackAuthorizationResponse>(`/reports/${reportId}/playback/${assetId}`, null),
};