import { http } from '@/lib/api/http';

export interface AssessmentPlanItemResponse {
  id: string;
  planId: string;
  metricCode: string;
  metricName: string;
  unit: string;
  valueType: string;
  required: boolean;
  minValue: number | null;
  maxValue: number | null;
  sortOrder: number;
}

export interface AssessmentPlanResponse {
  id: string;
  organizationId: string;
  name: string;
  code: string;
  description: string | null;
  version: number;
  status: 'Draft' | 'Published' | 'Retired';
  protocolId: string | null;
  protocolCode: string | null;
  createdByUserId: string;
  items: AssessmentPlanItemResponse[];
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateAssessmentPlanItemRequest {
  metricCode: string;
  metricName: string;
  unit: string;
  valueType: string;
  required: boolean;
  minValue: number | null;
  maxValue: number | null;
}

export interface CreateAssessmentPlanRequest {
  name: string;
  code: string;
  description: string | null;
  protocolId: string | null;
  protocolCode: string | null;
  items: CreateAssessmentPlanItemRequest[];
}

export interface UpdateAssessmentPlanRequest {
  name: string;
  description: string | null;
  protocolId: string | null;
  protocolCode: string | null;
  items: CreateAssessmentPlanItemRequest[];
}

export interface CandidateSessionResponse {
  sessionId: string;
  subjectId: string;
  protocolId: string;
  protocolVersion: string;
  status: string;
  startedAt: string | null;
  hasResult: boolean;
  allRequiredMetricsPresent: boolean;
  missingRequiredMetrics: string[];
  incompatibleMetrics: string[];
}

export interface PlanCompletenessResponse {
  planId: string;
  planVersion: number;
  subjectId: string;
  selectedSessionIds: string[];
  isComplete: boolean;
  blockingReasons: string[];
  candidateSessions: CandidateSessionResponse[];
}

export interface CheckPlanCompletenessRequest {
  subjectId: string;
  sessionIds: string[];
}

export const plansApi = {
  list: () => http.get<AssessmentPlanResponse[]>('/assessmentplans'),
  getById: (id: string) => http.get<AssessmentPlanResponse>(`/assessmentplans/${id}`),
  create: (request: CreateAssessmentPlanRequest) =>
    http.post<AssessmentPlanResponse>('/assessmentplans', request),
  update: (id: string, request: UpdateAssessmentPlanRequest) =>
    http.put<AssessmentPlanResponse>(`/assessmentplans/${id}`, request),
  publish: (id: string) =>
    http.post<AssessmentPlanResponse>(`/assessmentplans/${id}/publish`, null),
  retire: (id: string) =>
    http.post<AssessmentPlanResponse>(`/assessmentplans/${id}/retire`, null),
  createNewVersion: (id: string) =>
    http.post<AssessmentPlanResponse>(`/assessmentplans/${id}/new-version`, null),
  checkCompleteness: (id: string, request: CheckPlanCompletenessRequest) =>
    http.post<PlanCompletenessResponse>(`/assessmentplans/${id}/completeness`, request),
};