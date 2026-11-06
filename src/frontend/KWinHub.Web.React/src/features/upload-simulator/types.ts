import type { SessionStatus } from '@/lib/types';

export type FailureMode = 'none' | 'upload' | 'validation' | 'processing';

export interface SessionDevice {
  id: string;
  externalDeviceId: string;
  type: string;
  serialNumber: string;
}

export interface SessionTrial {
  externalTrialId: string;
  trialNumber: number;
  status: 'valid' | 'invalid';
  reason?: string;
}

export interface DeviceTrialGroup {
  device: SessionDevice;
  trials: SessionTrial[];
}

export interface UploadedFile {
  id: string;
  name: string;
  sizeBytes: number;
  type: string;
  previewUrl?: string;
  dataUrl?: string;
}

export interface CsvPreview {
  headers: string[];
  rows: string[][];
  rowCount: number;
}

export interface WizardState {
  clientId: string;
  clientApplication: string;
  clientVersion: string;
  externalSessionId: string;
  sessionName: string;
  date: string;
  environment: 'production' | 'staging' | 'development';
  operatorName: string;
  athleteId: string;
  protocolId: string;
  devices: DeviceTrialGroup[];
  csvFile: UploadedFile | null;
  csvPreview: CsvPreview | null;
  videoFile: UploadedFile | null;
  notes: string;
  failureMode: FailureMode;
}

export interface DeviceAsset {
  externalDeviceId: string;
  deviceType: string;
  trials: {
    externalTrialId: string;
    trialNumber: number;
    assets: AssetRef[];
  }[];
}

export interface AssetRef {
  kind: 'csv' | 'video';
  fileName: string;
  sizeBytes: number;
  mimeType: string;
}

export interface SessionManifest {
  externalSessionId: string;
  client: {
    id: string;
    application: string;
    version: string;
  };
  subject: {
    id: string;
  };
  protocol: {
    id: string;
  };
  operator: {
    name: string;
  };
  environment: string;
  startedAt: string;
  devices: DeviceAsset[];
  notes: string;
}

export interface LifecycleStage {
  key: string;
  label: string;
  labelCn: string;
  status: SessionStatus;
}

export type LifecycleResult = 'pending' | 'running' | 'success' | 'failed' | 'skipped';

export interface LifecycleStageState {
  stage: LifecycleStage;
  result: LifecycleResult;
  startedAt: string | null;
  completedAt: string | null;
  detail: string | null;
}

export interface SimulationResult {
  sessionId: string;
  idempotencyKey: string;
  correlationId: string;
  statusUrl: string;
  currentStatus: SessionStatus;
  stages: LifecycleStageState[];
  log: SimulationLogEntry[];
  startedAt: string;
  completedAt: string | null;
  errorMessage: string | null;
  isDuplicate: boolean;
}

export interface SimulationLogEntry {
  id: string;
  timestamp: string;
  level: 'info' | 'success' | 'warning' | 'error';
  message: string;
  messageCn: string;
}

export const LIFECYCLE_STAGES: LifecycleStage[] = [
  { key: 'create', label: 'Create Session', labelCn: '创建会话', status: 'RECEIVED' },
  { key: 'asset-auth', label: 'Asset Authorization', labelCn: '资源授权', status: 'RECEIVED' },
  { key: 'asset-upload', label: 'Asset Upload', labelCn: '资源上传', status: 'UPLOADING' },
  { key: 'session-update', label: 'Session Update', labelCn: '会话更新', status: 'RECEIVED' },
  { key: 'processing', label: 'Processing', labelCn: '处理计算', status: 'PROCESSING' },
  { key: 'publish', label: 'Publish', labelCn: '发布', status: 'PUBLISHED' },
];

export const FAILURE_STAGE: Record<Exclude<FailureMode, 'none'>, string> = {
  upload: 'asset-upload',
  validation: 'processing',
  processing: 'processing',
};

export const FAILURE_STATUS: Record<Exclude<FailureMode, 'none'>, SessionStatus> = {
  upload: 'UPLOAD_FAILED',
  validation: 'VALIDATION_FAILED',
  processing: 'PROCESSING_FAILED',
};