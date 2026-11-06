import type { SessionStatus } from '@/lib/types';

export type SimulatedFailureMode = 'none' | 'upload' | 'validation' | 'processing';

export interface UploadConfig {
  athleteId: string;
  protocolId: string;
  deviceId: string;
  trials: number;
  failureMode: SimulatedFailureMode;
  operatorName: string;
}

export interface PipelineStage {
  status: SessionStatus;
  label: string;
  labelCn: string;
  description: string;
  descriptionCn: string;
}

export type StageResult = 'pending' | 'running' | 'success' | 'failed' | 'skipped';

export interface PipelineStageState {
  stage: PipelineStage;
  result: StageResult;
  startedAt: string | null;
  completedAt: string | null;
  detail: string | null;
}

export interface UploadJob {
  id: string;
  config: UploadConfig;
  idempotencyKey: string;
  statusUrl: string;
  currentStatus: SessionStatus;
  stages: PipelineStageState[];
  log: LogEntry[];
  startedAt: string;
  completedAt: string | null;
  errorMessage: string | null;
}

export interface LogEntry {
  id: string;
  timestamp: string;
  level: 'info' | 'success' | 'warning' | 'error';
  status: SessionStatus;
  message: string;
  messageCn: string;
}

export const PIPELINE_STAGES: PipelineStage[] = [
  {
    status: 'LOCAL',
    label: 'Local Capture',
    labelCn: '本地采集',
    description: 'Raw measurement captured on the client device',
    descriptionCn: '在客户端设备上完成原始数据采集',
  },
  {
    status: 'QUEUED',
    label: 'Queue',
    labelCn: '上传队列',
    description: 'Session added to the client upload queue',
    descriptionCn: '会话已加入客户端上传队列',
  },
  {
    status: 'UPLOADING',
    label: 'Upload',
    labelCn: '数据上传',
    description: 'Transferring raw payload to KWinHub API',
    descriptionCn: '正在传输原始数据至 KWinHub API',
  },
  {
    status: 'RECEIVED',
    label: 'Received',
    labelCn: '服务端接收',
    description: 'Server acknowledged and persisted the session',
    descriptionCn: '服务端已确认接收并持久化会话',
  },
  {
    status: 'VALIDATING',
    label: 'Validation',
    labelCn: '数据校验',
    description: 'Protocol validity rules applied to raw measurements',
    descriptionCn: '对原始测量应用协议有效性规则',
  },
  {
    status: 'PROCESSING',
    label: 'Processing',
    labelCn: '指标计算',
    description: 'Computing metrics via the protocol processing graph',
    descriptionCn: '通过协议处理图谱计算指标',
  },
  {
    status: 'ANALYZED',
    label: 'Analyzed',
    labelCn: '分析完成',
    description: 'Metrics and visualizations ready',
    descriptionCn: '指标与可视化结果已就绪',
  },
  {
    status: 'PUBLISHED',
    label: 'Published',
    labelCn: '已发布',
    description: 'Session published to the athlete record',
    descriptionCn: '会话已发布至运动员记录',
  },
];

export function failureStatusFor(mode: SimulatedFailureMode): SessionStatus | null {
  switch (mode) {
    case 'upload':
      return 'UPLOAD_FAILED';
    case 'validation':
      return 'VALIDATION_FAILED';
    case 'processing':
      return 'PROCESSING_FAILED';
    default:
      return null;
  }
}

export function terminalStatusFor(mode: SimulatedFailureMode): SessionStatus {
  return failureStatusFor(mode) ?? 'PUBLISHED';
}