import {
  PIPELINE_STAGES,
  type LogEntry,
  type PipelineStageState,
  type SimulatedFailureMode,
  type UploadConfig,
  type UploadJob,
} from './types';
import type { SessionStatus } from '@/lib/types';

let counter = 0;
const nextId = () => `sim-${Date.now()}-${++counter}`;

function nowIso() {
  return new Date().toISOString();
}

function makeLog(
  level: LogEntry['level'],
  status: SessionStatus,
  message: string,
  messageCn: string,
): LogEntry {
  return {
    id: nextId(),
    timestamp: nowIso(),
    level,
    status,
    message,
    messageCn,
  };
}

export function createJob(config: UploadConfig): UploadJob {
  const idempotencyKey = `kw-sim-${config.athleteId}-${config.protocolId}-${Date.now()}`;
  const stages: PipelineStageState[] = PIPELINE_STAGES.map((stage) => ({
    stage,
    result: 'pending',
    startedAt: null,
    completedAt: null,
    detail: null,
  }));

  const initialLog: LogEntry[] = [
    makeLog('info', 'LOCAL', 'Local capture session created', '本地采集会话已创建'),
    makeLog(
      'info',
      'LOCAL',
      `Idempotency key: ${idempotencyKey}`,
      `幂等键: ${idempotencyKey}`,
    ),
  ];

  return {
    id: nextId(),
    config,
    idempotencyKey,
    statusUrl: `/api/v1/test-sessions/${nextId()}/status`,
    currentStatus: 'LOCAL',
    stages,
    log: initialLog,
    startedAt: nowIso(),
    completedAt: null,
    errorMessage: null,
  };
}

export interface AdvanceResult {
  job: UploadJob;
  done: boolean;
}

function stageDelayMs(status: SessionStatus): number {
  switch (status) {
    case 'LOCAL':
      return 250;
    case 'QUEUED':
      return 200;
    case 'UPLOADING':
      return 700;
    case 'RECEIVED':
      return 300;
    case 'VALIDATING':
      return 600;
    case 'PROCESSING':
      return 800;
    case 'ANALYZED':
      return 350;
    default:
      return 400;
  }
}

function injectFailureDetail(mode: SimulatedFailureMode): { message: string; messageCn: string } {
  switch (mode) {
    case 'upload':
      return {
        message: 'Network timeout during raw payload transfer (ECONNRESET)',
        messageCn: '原始数据传输过程中网络超时 (ECONNRESET)',
      };
    case 'validation':
      return {
        message: 'Protocol validity rule failed: minimum jump height not reached (18.2 cm < 20 cm)',
        messageCn: '协议有效性规则未通过: 最小跳跃高度未达标 (18.2 cm < 20 cm)',
      };
    case 'processing':
      return {
        message: 'Processing pipeline error: force-time event detection failed (no clear takeoff onset)',
        messageCn: '处理管线错误: 力-时间事件检测失败 (未识别到清晰起跳点)',
      };
    default:
      return { message: '', messageCn: '' };
  }
}

export function advanceJob(job: UploadJob): AdvanceResult {
  const stages = [...job.stages];
  const log = [...job.log];
  const currentIdx = stages.findIndex((s) => s.result === 'pending' || s.result === 'running');

  if (currentIdx === -1) {
    return { job, done: true };
  }

  const stage = stages[currentIdx];
  const failureStatus = failureTargetFor(job.config.failureMode, stage.stage.status);

  if (failureStatus) {
    stages[currentIdx] = {
      ...stage,
      result: 'failed',
      startedAt: stage.startedAt ?? nowIso(),
      completedAt: nowIso(),
      detail: failureStatus.message,
    };
    log.push(
      makeLog('warning', stage.stage.status, `Stage "${stage.stage.label}" failed`, `阶段 "${stage.stage.labelCn}" 失败`),
    );
    log.push(
      makeLog('error', failureStatus.status, failureStatus.message, failureStatus.messageCn),
    );
    stages.forEach((s, idx) => {
      if (idx > currentIdx) {
        s.result = 'skipped';
      }
    });
    return {
      job: {
        ...job,
        stages,
        log,
        currentStatus: failureStatus.status,
        completedAt: nowIso(),
        errorMessage: failureStatus.message,
      },
      done: true,
    };
  }

  stages[currentIdx] = {
    ...stage,
    result: 'success',
    startedAt: stage.startedAt ?? nowIso(),
    completedAt: nowIso(),
    detail: null,
  };
  log.push(
    makeLog('success', stage.stage.status, `Stage "${stage.stage.label}" completed`, `阶段 "${stage.stage.labelCn}" 已完成`),
  );

  const next = stages[currentIdx + 1];
  if (next) {
    stages[currentIdx + 1] = {
      ...next,
      result: 'running',
      startedAt: nowIso(),
      completedAt: null,
      detail: null,
    };
    return {
      job: {
        ...job,
        stages,
        log,
        currentStatus: next.stage.status,
      },
      done: false,
    };
  }

  return {
    job: {
      ...job,
      stages,
      log,
      currentStatus: stage.stage.status,
      completedAt: nowIso(),
      errorMessage: null,
    },
    done: true,
  };
}

function failureTargetFor(
  mode: SimulatedFailureMode,
  currentStageStatus: SessionStatus,
): { status: SessionStatus; message: string; messageCn: string } | null {
  if (mode === 'none') return null;
  const trigger: Record<Exclude<SimulatedFailureMode, 'none'>, SessionStatus> = {
    upload: 'UPLOADING',
    validation: 'VALIDATING',
    processing: 'PROCESSING',
  };
  const failStatus: Record<Exclude<SimulatedFailureMode, 'none'>, SessionStatus> = {
    upload: 'UPLOAD_FAILED',
    validation: 'VALIDATION_FAILED',
    processing: 'PROCESSING_FAILED',
  };
  const m = mode as Exclude<SimulatedFailureMode, 'none'>;
  if (currentStageStatus === trigger[m]) {
    const detail = injectFailureDetail(mode);
    return { status: failStatus[m], message: detail.message, messageCn: detail.messageCn };
  }
  return null;
}

export function stageDelayFor(status: SessionStatus): number {
  return stageDelayMs(status);
}