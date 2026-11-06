import {
  FAILURE_STAGE,
  FAILURE_STATUS,
  LIFECYCLE_STAGES,
  type FailureMode,
  type LifecycleStageState,
  type SessionManifest,
  type SimulationLogEntry,
  type SimulationResult,
  type WizardState,
} from './types';
import type { TestSession } from '@/lib/types';
import { DEMO_PROTOCOLS, DEMO_DEVICES, DEMO_SPORTS } from '@/lib/demo-data';

let counter = 0;
const nextId = (prefix = 'sim') => `${prefix}-${Date.now().toString(36)}-${++counter}`;
const nowIso = () => new Date().toISOString();

export function buildManifest(wizard: WizardState): SessionManifest {
  const devices = wizard.devices.map((group) => ({
    externalDeviceId: group.device.externalDeviceId,
    deviceType: group.device.type,
    trials: group.trials.map((trial) => ({
      externalTrialId: trial.externalTrialId,
      trialNumber: trial.trialNumber,
      assets: [
        ...(wizard.csvFile
          ? [{ kind: 'csv' as const, fileName: wizard.csvFile.name, sizeBytes: wizard.csvFile.sizeBytes, mimeType: wizard.csvFile.type }]
          : []),
        ...(wizard.videoFile
          ? [{ kind: 'video' as const, fileName: wizard.videoFile.name, sizeBytes: wizard.videoFile.sizeBytes, mimeType: wizard.videoFile.type }]
          : []),
      ],
    })),
  }));

  return {
    externalSessionId: wizard.externalSessionId,
    client: {
      id: wizard.clientId,
      application: wizard.clientApplication,
      version: wizard.clientVersion,
    },
    subject: { id: wizard.athleteId },
    protocol: { id: wizard.protocolId },
    operator: { name: wizard.operatorName },
    environment: wizard.environment,
    startedAt: wizard.date,
    devices,
    notes: wizard.notes,
  };
}

export function buildIdempotencyKey(wizard: WizardState): string {
  return `kw-${wizard.clientId}-${wizard.externalSessionId}`;
}

export function buildCorrelationId(): string {
  return `corr-${nextId()}`;
}

export function initialStages(): LifecycleStageState[] {
  return LIFECYCLE_STAGES.map((stage) => ({
    stage,
    result: 'pending' as const,
    startedAt: null,
    completedAt: null,
    detail: null,
  }));
}

export function createInitialResult(wizard: WizardState, isDuplicate: boolean): SimulationResult {
  const sessionId = nextId('sess');
  return {
    sessionId,
    idempotencyKey: buildIdempotencyKey(wizard),
    correlationId: buildCorrelationId(),
    statusUrl: `/api/v1/test-sessions/${sessionId}/status`,
    currentStatus: 'LOCAL',
    stages: initialStages(),
    log: [
      makeLog('info', 'Preparing submission payload', '正在构建提交载荷'),
      makeLog('info', `Idempotency key: ${buildIdempotencyKey(wizard)}`, `幂等键: ${buildIdempotencyKey(wizard)}`),
      ...(isDuplicate ? [makeLog('warning', 'Duplicate idempotency key detected — reusing existing session', '检测到重复幂等键 — 复用已有会话')] : []),
    ],
    startedAt: nowIso(),
    completedAt: null,
    errorMessage: null,
    isDuplicate,
  };
}

export function makeLog(
  level: SimulationLogEntry['level'],
  message: string,
  messageCn: string,
): SimulationLogEntry {
  return { id: nextId('log'), timestamp: nowIso(), level, message, messageCn };
}

function failureDetail(mode: FailureMode): { message: string; messageCn: string } {
  switch (mode) {
    case 'upload':
      return { message: 'Network timeout during asset upload (ECONNRESET)', messageCn: '资源上传时网络超时 (ECONNRESET)' };
    case 'validation':
      return { message: 'Protocol validity rule rejected: minimum jump height not reached (18.2 cm < 20 cm)', messageCn: '协议有效性规则驳回: 最小跳跃高度未达标 (18.2 cm < 20 cm)' };
    case 'processing':
      return { message: 'Processing pipeline error: force-time event detection failed (no clear takeoff onset)', messageCn: '处理管线错误: 力-时间事件检测失败 (未识别到清晰起跳点)' };
    default:
      return { message: '', messageCn: '' };
  }
}

export function advanceResult(result: SimulationResult, failureMode: FailureMode): SimulationResult {
  const stages = result.stages.map((s) => ({ ...s }));
  const log = [...result.log];
  const idx = stages.findIndex((s) => s.result === 'pending' || s.result === 'running');

  if (idx === -1) {
    return { ...result, completedAt: nowIso() };
  }

  const stage = stages[idx];
  const isFailure = failureMode !== 'none' && FAILURE_STAGE[failureMode] === stage.stage.key;

  if (isFailure) {
    const detail = failureDetail(failureMode);
    stages[idx] = { ...stage, result: 'failed', startedAt: stage.startedAt ?? nowIso(), completedAt: nowIso(), detail: detail.message };
    log.push(makeLog('warning', `Stage "${stage.stage.label}" failed`, `阶段 "${stage.stage.labelCn}" 失败`));
    log.push(makeLog('error', detail.message, detail.messageCn));
    for (let i = idx + 1; i < stages.length; i++) {
      stages[i].result = 'skipped';
    }
    return {
      ...result,
      stages,
      log,
      currentStatus: FAILURE_STATUS[failureMode],
      completedAt: nowIso(),
      errorMessage: detail.message,
    };
  }

  stages[idx] = { ...stage, result: 'success', startedAt: stage.startedAt ?? nowIso(), completedAt: nowIso(), detail: null };
  log.push(makeLog('success', `Stage "${stage.stage.label}" completed`, `阶段 "${stage.stage.labelCn}" 已完成`));

  const next = stages[idx + 1];
  if (next) {
    stages[idx + 1] = { ...next, result: 'running', startedAt: nowIso(), completedAt: null, detail: null };
    return { ...result, stages, log, currentStatus: next.stage.status };
  }

  log.push(makeLog('success', 'Session published to athlete record', '会话已发布至运动员记录'));
  return { ...result, stages, log, currentStatus: 'PUBLISHED', completedAt: nowIso() };
}

export function stageDelay(key: string): number {
  switch (key) {
    case 'create': return 350;
    case 'asset-auth': return 300;
    case 'asset-upload': return 900;
    case 'session-update': return 300;
    case 'processing': return 1000;
    case 'publish': return 350;
    default: return 500;
  }
}

export function buildMockSession(wizard: WizardState, result: SimulationResult): TestSession {
  const protocol = DEMO_PROTOCOLS.find((p) => p.id === wizard.protocolId) ?? DEMO_PROTOCOLS[0];
  const device = wizard.devices[0]?.device;
  const sportId = protocol.sportId || DEMO_SPORTS[0]?.id || '';
  const trials = wizard.devices.flatMap((g) => g.trials).length || 3;

  const trialList = wizard.devices.flatMap((g) =>
    g.trials.map((t) => ({
      number: t.trialNumber,
      status: t.status as 'valid' | 'invalid',
      reason: t.reason,
      values: { jump_height: 28 + Math.random() * 12, peak_force: 1800 + Math.random() * 600 },
    })),
  );

  return {
    id: result.sessionId,
    athleteId: wizard.athleteId,
    protocolId: wizard.protocolId,
    protocolName: protocol.name,
    sportId,
    date: wizard.date,
    deviceSerial: device?.serialNumber ?? DEMO_DEVICES[0]?.serialNumber ?? 'SIM-001',
    operatorName: wizard.operatorName,
    status: result.currentStatus === 'PUBLISHED' ? 'normal' : 'warning',
    sessionStatus: result.currentStatus,
    trials: trialList,
    summary: { jump_height: 32 + Math.random() * 8, trials },
    qualityFlag: 'valid',
    dataQuality: { status: 'complete', issues: [], validationPassed: true },
    processingVersion: '1.0.0',
    source: 'api',
    sourceId: wizard.clientId,
    dataSourceId: 'ds-simulator',
    idempotencyKey: result.idempotencyKey,
    startedAt: result.startedAt,
    completedAt: result.completedAt,
    statusUrl: result.statusUrl,
  };
}