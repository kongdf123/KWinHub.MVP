// ============================================================================
// KWinHub Service Layer — Dual Source (Demo ↔ API)
// When VITE_DATA_MODE === 'api', delegates to the real backend via http client.
// Otherwise uses the in-memory DEMO_* dataset (delay-wrapped for realism).
// ============================================================================

import {
  DEMO_ATHLETES, DEMO_PROTOCOLS, DEMO_SESSIONS, DEMO_DEVICES, DEMO_DATA_SOURCES,
  DEMO_DATASETS, DEMO_RAW_MEASUREMENTS, DEMO_PROCESSED_MEASUREMENTS,
  DEMO_SYNC_JOBS, DEMO_SYNC_LOGS, DEMO_HEARTBEATS, DEMO_RESULTS,
  DEMO_REPORTS, DEMO_DASHBOARD_WIDGETS, DEMO_TEAMS, DEMO_SPORTS,
  DEMO_IMPORTS, DEMO_INTEGRATIONS, DEMO_USERS, DEMO_GROUPS, DEMO_ORG,
  getSessionsForAthlete, getTimelineForAthlete, getSport, getProtocol, getAthlete, getTeam,
  getResultForSession, getDataSource,
} from './demo-data';
import { getMetric } from './metrics';
import { http } from './api/http';
import type {
  Athlete, TestProtocol, TestSession, Device, DataSource, Dataset,
  RawMeasurement, ProcessedMeasurement, SyncJob, SyncLog, Heartbeat,
  Result, ReportDefinition, DashboardWidget, Team, Sport, ImportJob,
  Integration, User, Group, Organization, TimelineEvent, SessionStatus,
  Status,
} from './types';

// --- Async helper (simulates network latency) -----------------------------
function delay<T>(data: T, ms = 120): Promise<T> {
  return new Promise((resolve) => setTimeout(() => resolve(data), ms));
}

// --- API / Demo mode flag -------------------------------------------------
const API_MODE = import.meta.env.VITE_DATA_MODE === 'api';

// --- Backend → Frontend mappers ------------------------------------------
// These map the backend's camelCase JSON (PascalCase C# record fields serialised
// as camelCase by System.Text.Json) onto the richer frontend domain models,
// filling gaps with sensible defaults so the UI still renders.

interface ApiSubject {
  id: string; organizationId: string; firstName: string; lastName: string;
  subjectType: string; externalId?: string | null; displayName?: string | null;
  dateOfBirth?: string | null; sex?: string | null; status: string;
  createdAt: string; updatedAt?: string | null;
}

interface ApiProtocol {
  id: string; organizationId: string; name: string; code: string; version: string;
  category?: string | null; sport?: string | null; status: string;
  description?: string | null; createdAt: string; updatedAt?: string | null;
}

interface ApiSession {
  id: string; organizationId: string; subjectId: string; protocolId: string;
  protocolVersion: string; sourceId?: string | null; deviceId?: string | null;
  status: string; startedAt?: string | null; completedAt?: string | null;
  clientApplication?: string | null; clientVersion?: string | null; externalId?: string | null;
  timezone?: string | null; createdAt: string; updatedAt?: string | null;
}

interface ApiMetricValue {
  id: string; metricCode: string; metricName: string; value: number; unit: string; side?: string | null;
}

interface ApiResult {
  id: string; organizationId: string; sessionId: string; protocolId: string;
  protocolVersion: string; algorithmVersion: string; status: string;
  qualityStatus?: string | null; trialId?: string | null; metrics: ApiMetricValue[];
  createdAt: string; updatedAt?: string | null;
}

function mapSubject(s: ApiSubject): Athlete {
  const statusMap: Record<string, Status> = {
    ACTIVE: 'normal', INACTIVE: 'offline', ARCHIVED: 'offline', SUSPENDED: 'warning',
  };
  return {
    id: s.id,
    subjectType: (s.subjectType?.toUpperCase() as Athlete['subjectType']) ?? 'ATHLETE',
    firstName: s.firstName ?? '',
    lastName: s.lastName ?? '',
    dateOfBirth: s.dateOfBirth ?? '',
    sex: (s.sex?.toUpperCase() as 'M' | 'F') ?? 'M',
    sportId: '', position: '', teamId: '', groupIds: [],
    status: statusMap[s.status?.toUpperCase()] ?? 'normal',
    lastTestDate: null, lastTestProtocol: null, photoUrl: null,
    height: 0, weight: 0, baseline: {}, personalBest: {}, tags: [],
  };
}

function mapProtocol(p: ApiProtocol): TestProtocol {
  return {
    id: p.id,
    key: p.code ?? p.name?.toLowerCase().replace(/\s+/g, '-') ?? p.id,
    name: p.name ?? '',
    nameCn: p.name ?? '',
    version: p.version ?? '1.0',
    sportId: p.sport ?? '',
    deviceType: '',
    category: p.category ?? '',
    input: { dataTypes: [], captureMode: 'force_plate', trials: 3, trialIntervalSec: 10 },
    processing: { steps: [], eventDetection: [], formulaRefs: [], processingVersion: '1.0' },
    metrics: [],
    validityRules: [], validityRulesCn: [],
    visualization: [], comparison: [],
    description: p.description ?? '', descriptionCn: p.description ?? '',
    durationMin: 5, difficulty: 'standard',
  };
}

function mapSessionStatus(s: string): SessionStatus {
  const up = s?.toUpperCase() ?? '';
  // Backend stores uppercase status strings that map cleanly onto our enum.
  if (up === 'PUBLISHED') return 'PUBLISHED';
  if (up === 'ANALYZED') return 'ANALYZED';
  if (up === 'PROCESSING') return 'PROCESSING';
  if (up === 'VALIDATING') return 'VALIDATING';
  if (up === 'RECEIVED') return 'RECEIVED';
  if (up === 'QUEUED') return 'QUEUED';
  if (up === 'UPLOADING') return 'UPLOADING';
  if (up === 'UPLOAD_FAILED') return 'UPLOAD_FAILED';
  if (up === 'VALIDATION_FAILED') return 'VALIDATION_FAILED';
  if (up === 'PROCESSING_FAILED') return 'PROCESSING_FAILED';
  if (up === 'LOCAL') return 'LOCAL';
  return 'RECEIVED';
}

function mapSession(s: ApiSession): TestSession {
  const sessionStatus = mapSessionStatus(s.status);
  const status: Status = (sessionStatus === 'PUBLISHED' || sessionStatus === 'ANALYZED') ? 'normal'
    : (sessionStatus.includes('FAILED') || sessionStatus === 'UPLOAD_FAILED' || sessionStatus === 'PROCESSING_FAILED') ? 'warning'
    : 'processing';
  return {
    id: s.id,
    athleteId: s.subjectId,
    protocolId: s.protocolId,
    protocolName: '', // enriched later if needed
    sportId: '',
    date: s.startedAt ?? s.createdAt,
    deviceSerial: '',
    operatorName: '',
    status,
    sessionStatus,
    trials: [],
    summary: {},
    qualityFlag: 'valid',
    dataQuality: { status: 'complete', issues: [], validationPassed: true },
    processingVersion: s.protocolVersion ?? '1.0',
    source: s.clientApplication ? 'api' : 'device',
    sourceId: s.sourceId ?? '',
    dataSourceId: '',
    idempotencyKey: s.externalId ?? '',
    startedAt: s.startedAt ?? s.createdAt,
    completedAt: s.completedAt ?? null,
    statusUrl: `/api/v1/sessions/${s.id}/status`,
  };
}

function mapResult(r: ApiResult): Result {
  const metrics = r.metrics.map((m) => ({
    key: m.metricCode,
    name: m.metricName,
    nameCn: m.metricName,
    unit: m.unit,
    value: m.value,
    primary: false,
    higherIsBetter: true,
  }));
  return {
    sessionId: r.sessionId,
    protocolId: r.protocolId,
    subjectId: '',
    primaryMetric: metrics[0] ?? { key: '', name: '', nameCn: '', unit: '', value: 0, primary: true, higherIsBetter: true },
    metrics,
    visualizations: [],
    trialResults: [],
    analysisNotes: [],
    rawDataRef: { measurementIds: [], datasetId: null },
  };
}

// --- Ingestion response (202 Accepted semantics) --------------------------
export interface IngestionResponse {
  sessionId: string;
  status: 'RECEIVED';
  statusUrl: string;
  retryAfter: number;
  idempotencyKey: string;
}

// ============================================================================
// Athlete / Subject Service
// ============================================================================
export const athleteService = {
  async getAll(): Promise<Athlete[]> {
    if (API_MODE) {
      const list = await http.get<ApiSubject[]>('/subjects');
      return list.map(mapSubject);
    }
    return delay([...DEMO_ATHLETES]);
  },
  async getById(id: string): Promise<Athlete | undefined> {
    if (API_MODE) {
      try {
        const item = await http.get<ApiSubject>(`/subjects/${id}`);
        return mapSubject(item);
      } catch (e: unknown) {
        if ((e as { status?: number })?.status === 404) return undefined;
        throw e;
      }
    }
    return delay(getAthlete(id));
  },
  getByTeam(teamId: string): Promise<Athlete[]> {
    if (API_MODE) {
      return athleteService.getAll().then((list) => list.filter((a) => a.teamId === teamId));
    }
    return delay(DEMO_ATHLETES.filter((a) => a.teamId === teamId));
  },
  getTimeline(athleteId: string): Promise<TimelineEvent[]> {
    return delay(getTimelineForAthlete(athleteId));
  },
  async getSessions(athleteId: string): Promise<TestSession[]> {
    if (API_MODE) {
      const list = await http.get<ApiSession[]>(`/sessions/by-subject/${athleteId}`);
      return list.map(mapSession);
    }
    return delay(getSessionsForAthlete(athleteId));
  },
};

// ============================================================================
// Protocol Service
// ============================================================================
export const protocolService = {
  async getAll(): Promise<TestProtocol[]> {
    if (API_MODE) {
      const list = await http.get<ApiProtocol[]>('/protocols');
      return list.map(mapProtocol);
    }
    return delay([...DEMO_PROTOCOLS]);
  },
  async getById(id: string): Promise<TestProtocol | undefined> {
    if (API_MODE) {
      try {
        const item = await http.get<ApiProtocol>(`/protocols/${id}`);
        return mapProtocol(item);
      } catch (e: unknown) {
        if ((e as { status?: number })?.status === 404) return undefined;
        throw e;
      }
    }
    return delay(getProtocol(id));
  },
  getBySport(sportId: string): Promise<TestProtocol[]> {
    if (API_MODE) {
      return protocolService.getAll().then((list) => list.filter((p) => p.sportId === sportId));
    }
    return delay(DEMO_PROTOCOLS.filter((p) => p.sportId === sportId));
  },
};

// ============================================================================
// Test Session Service (with async ingestion semantics)
// ============================================================================
export const sessionService = {
  async getAll(): Promise<TestSession[]> {
    if (API_MODE) {
      const list = await http.get<ApiSession[]>('/sessions');
      return list.map(mapSession);
    }
    return delay([...DEMO_SESSIONS]);
  },
  async getById(id: string): Promise<TestSession | undefined> {
    if (API_MODE) {
      try {
        const item = await http.get<ApiSession>(`/sessions/${id}`);
        return mapSession(item);
      } catch (e: unknown) {
        if ((e as { status?: number })?.status === 404) return undefined;
        throw e;
      }
    }
    return delay(DEMO_SESSIONS.find((s) => s.id === id));
  },
  async getByAthlete(athleteId: string): Promise<TestSession[]> {
    if (API_MODE) {
      const list = await http.get<ApiSession[]>(`/sessions/by-subject/${athleteId}`);
      return list.map(mapSession);
    }
    return delay(getSessionsForAthlete(athleteId));
  },
  getByProtocol(protocolId: string): Promise<TestSession[]> {
    if (API_MODE) {
      return sessionService.getAll().then((list) => list.filter((s) => s.protocolId === protocolId));
    }
    return delay(DEMO_SESSIONS.filter((s) => s.protocolId === protocolId));
  },
  getByTeam(teamId: string): Promise<TestSession[]> {
    if (API_MODE) {
      // Fallback: fetch all sessions then filter by athletes on that team
      return Promise.all([
        athleteService.getByTeam(teamId),
        sessionService.getAll(),
      ]).then(([athletes, sessions]) => {
        const ids = new Set(athletes.map((a) => a.id));
        return sessions.filter((s) => ids.has(s.athleteId));
      });
    }
    const athleteIds = new Set(DEMO_ATHLETES.filter((a) => a.teamId === teamId).map((a) => a.id));
    return delay(DEMO_SESSIONS.filter((s) => athleteIds.has(s.athleteId)));
  },

  // Real POST /api/v1/sessions — idempotent via externalId
  async ingest(params: {
    subjectId: string; protocolId: string; sourceId?: string; deviceId?: string;
    idempotencyKey: string; startedAt?: string; trials?: number;
  }): Promise<IngestionResponse> {
    if (API_MODE) {
      const session = await http.post<ApiSession>('/sessions', {
        subjectId: params.subjectId,
        protocolId: params.protocolId,
        protocolVersion: '1.0',
        sourceId: params.sourceId,
        deviceId: params.deviceId,
        externalId: params.idempotencyKey,
        clientApplication: 'KWinHub.Web.React',
        startedAt: params.startedAt,
      });
      return {
        sessionId: session.id,
        status: 'RECEIVED',
        statusUrl: `/api/v1/sessions/${session.id}/status`,
        retryAfter: 0,
        idempotencyKey: params.idempotencyKey,
      };
    }
    const sessionId = `ses-${Date.now()}`;
    const response: IngestionResponse = {
      sessionId,
      status: 'RECEIVED',
      statusUrl: `/api/v1/test-sessions/${sessionId}/status`,
      retryAfter: 2,
      idempotencyKey: params.idempotencyKey,
    };
    return delay(response, 300);
  },

  // Real GET /api/v1/sessions/{id}/status
  async getStatus(sessionId: string): Promise<{ sessionId: string; sessionStatus: SessionStatus; retryAfter: number }> {
    if (API_MODE) {
      const s = await http.get<{ sessionId: string; status: string; updatedAt?: string | null; errorMessage?: string | null }>(`/sessions/${sessionId}/status`);
      return { sessionId: s.sessionId, sessionStatus: mapSessionStatus(s.status), retryAfter: 2 };
    }
    const session = DEMO_SESSIONS.find((s) => s.id === sessionId);
    const job = DEMO_SYNC_JOBS.find((j) => j.statusUrl.includes(sessionId));
    const status = session?.sessionStatus ?? job?.sessionStatus ?? 'RECEIVED';
    return delay({ sessionId, sessionStatus: status, retryAfter: 2 }, 100);
  },
};

// ============================================================================
// Result Service (generic, protocol-driven)
// ============================================================================
export const resultService = {
  async getBySession(sessionId: string): Promise<Result | undefined> {
    if (API_MODE) {
      try {
        const list = await http.get<ApiResult[]>(`/results/session/${sessionId}`);
        if (!list || list.length === 0) return undefined;
        // Backend can emit multiple results per session (different trials / protocols).
        // UI shows the primary one — just pick the first.
        return mapResult(list[0]);
      } catch (e: unknown) {
        if ((e as { status?: number })?.status === 404) return undefined;
        throw e;
      }
    }
    return delay(getResultForSession(sessionId));
  },
  async getByAthlete(athleteId: string): Promise<Result[]> {
    if (API_MODE) {
      const sessions = await sessionService.getByAthlete(athleteId);
      const results = await Promise.all(sessions.map(async (s) => {
        try { return await resultService.getBySession(s.id); } catch { return undefined; }
      }));
      return results.filter((r): r is Result => !!r);
    }
    const sessionIds = new Set(getSessionsForAthlete(athleteId).map((s) => s.id));
    return delay(DEMO_RESULTS.filter((r) => sessionIds.has(r.sessionId)));
  },
  getByProtocol(protocolId: string): Promise<Result[]> {
    return delay(DEMO_RESULTS.filter((r) => r.protocolId === protocolId));
  },
};

// ============================================================================
// Data Source Service
// ============================================================================
export const dataSourceService = {
  getAll(): Promise<DataSource[]> {
    return delay([...DEMO_DATA_SOURCES]);
  },
  getByCategory(category: DataSource['category']): Promise<DataSource[]> {
    return delay(DEMO_DATA_SOURCES.filter((s) => s.category === category));
  },
  getById(id: string): Promise<DataSource | undefined> {
    return delay(getDataSource(id));
  },
};

// ============================================================================
// Device Service
// ============================================================================
export const deviceService = {
  getAll(): Promise<Device[]> {
    return delay([...DEMO_DEVICES]);
  },
  getById(id: string): Promise<Device | undefined> {
    return delay(DEMO_DEVICES.find((d) => d.id === id));
  },
};

// ============================================================================
// Dataset Service
// ============================================================================
export const datasetService = {
  getAll(): Promise<Dataset[]> {
    return delay([...DEMO_DATASETS]);
  },
  getBySubject(subjectId: string): Promise<Dataset[]> {
    return delay(DEMO_DATASETS.filter((d) => d.subjectIds.includes(subjectId)));
  },
};

// ============================================================================
// Measurement Service (raw + processed)
// ============================================================================
export const measurementService = {
  getRawBySession(sessionId: string): Promise<RawMeasurement[]> {
    return delay(DEMO_RAW_MEASUREMENTS.filter((m) => m.sessionId === sessionId));
  },
  getProcessedBySession(sessionId: string): Promise<ProcessedMeasurement[]> {
    return delay(DEMO_PROCESSED_MEASUREMENTS.filter((m) => m.sessionId === sessionId));
  },
};

// ============================================================================
// Sync Service (async lifecycle, heartbeat, idempotency)
// ============================================================================
export const syncService = {
  getJobs(): Promise<SyncJob[]> {
    return delay([...DEMO_SYNC_JOBS]);
  },
  getJobById(id: string): Promise<SyncJob | undefined> {
    return delay(DEMO_SYNC_JOBS.find((j) => j.id === id));
  },
  getLogs(): Promise<SyncLog[]> {
    return delay([...DEMO_SYNC_LOGS]);
  },
  getHeartbeats(): Promise<Heartbeat[]> {
    return delay([...DEMO_HEARTBEATS]);
  },
};

// ============================================================================
// Report Service
// ============================================================================
export const reportService = {
  getAll(): Promise<ReportDefinition[]> {
    return delay([...DEMO_REPORTS]);
  },
  getById(id: string): Promise<ReportDefinition | undefined> {
    return delay(DEMO_REPORTS.find((r) => r.id === id));
  },
};

// ============================================================================
// Dashboard Service (widget model)
// ============================================================================
export const dashboardService = {
  getWidgets(): Promise<DashboardWidget[]> {
    return delay([...DEMO_DASHBOARD_WIDGETS]);
  },
};

// ============================================================================
// Team, Sport, Misc Services
// ============================================================================
export const teamService = {
  getAll(): Promise<Team[]> {
    return delay([...DEMO_TEAMS]);
  },
  getById(id: string): Promise<Team | undefined> {
    return delay(getTeam(id));
  },
};

export const sportService = {
  getAll(): Promise<Sport[]> {
    return delay([...DEMO_SPORTS]);
  },
  getById(id: string): Promise<Sport | undefined> {
    return delay(getSport(id));
  },
};

export const importService = {
  getAll(): Promise<ImportJob[]> {
    return delay([...DEMO_IMPORTS]);
  },
};

export const integrationService = {
  getAll(): Promise<Integration[]> {
    return delay([...DEMO_INTEGRATIONS]);
  },
};

export const userService = {
  getAll(): Promise<User[]> {
    return delay([...DEMO_USERS]);
  },
};

export const groupService = {
  getAll(): Promise<Group[]> {
    return delay([...DEMO_GROUPS]);
  },
};

export const orgService = {
  get(): Promise<Organization> {
    return delay(DEMO_ORG);
  },
};

// --- Metric helper (re-exported for convenience) --------------------------
export { getMetric };
