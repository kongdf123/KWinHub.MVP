import type { ScientificScenario, ScientificTrialInput, ScientificSignalChannel, ScientificResultRevision, ScientificCalculationJob } from './dto';
import {
  generateCmjForceTime,
  generateSprintForceSingleStep,
  CMJ_METRIC_DEFINITIONS,
  SPRINT_METRIC_DEFINITIONS,
} from './fixture';

function cmjChannel(trialNumber: number, seed: number, side: 'total' | 'left' | 'right'): ScientificSignalChannel {
  const curve = generateCmjForceTime(800, 1000, seed, side === 'left' ? { baselineForce: 380 } : side === 'right' ? { baselineForce: 400 } : undefined);
  return {
    label: side === 'total' ? 'GRF-Z Bilateral' : `GRF-Z ${side === 'left' ? 'Left' : 'Right'}`,
    labelCn: side === 'total' ? '双侧垂直地反力' : `${side === 'left' ? '左' : '右'}侧垂直地反力`,
    deviceCode: 'KW-FP-00124',
    side,
    sampleRateHz: 1000,
    durationMs: 800,
    samples: curve.samples,
    events: curve.events.map((e) => ({
      code: e.code,
      codeCn: {
        onset: '起点',
        peak_force: '峰值力',
        takeoff: '起跳',
        landing: '落地',
      }[e.code] ?? e.code,
      offsetMs: e.offsetMs,
      detectionMethod: 'force_threshold',
      confidence: 0.98,
    })),
  };
}

function sprintFrontBlockChannel(seed: number, totalMs: number = 650): ScientificSignalChannel {
  const curve = generateSprintForceSingleStep(seed, totalMs, 1000, 'front');
  return {
    label: 'GRF Front Block',
    labelCn: '前蹬板力',
    deviceCode: 'KW-FP-0042-F',
    side: 'front',
    sampleRateHz: 1000,
    durationMs: totalMs,
    samples: curve.samples,
    events: curve.events.map((e) => ({
      code: e.code,
      codeCn: { contact: '接触', peak_force: '峰值力', toe_off: '离地' }[e.code] ?? e.code,
      offsetMs: e.offsetMs,
      detectionMethod: 'force_threshold',
      confidence: 0.97,
    })),
  };
}

function sprintRearBlockChannel(seed: number, totalMs: number = 550): ScientificSignalChannel {
  const curve = generateSprintForceSingleStep(seed + 999, totalMs, 1000, 'rear');
  return {
    label: 'GRF Rear Block',
    labelCn: '后蹬板力',
    deviceCode: 'KW-FP-0042-R',
    side: 'rear',
    sampleRateHz: 1000,
    durationMs: totalMs,
    samples: curve.samples,
    events: curve.events.map((e) => ({
      code: e.code,
      codeCn: { contact: '接触', peak_force: '峰值力', toe_off: '离地' }[e.code] ?? e.code,
      offsetMs: e.offsetMs,
      detectionMethod: 'force_threshold',
      confidence: 0.97,
    })),
  };
}

function cmjRevision(
  trialNumber: number,
  seed: number,
  algorithmVersion: string,
  idSuffix: string,
  status: ScientificResultRevision['status'],
  supersedesId: string | null = null,
  diff?: Record<string, number>,
): ScientificResultRevision {
  const h = 0.52 + (seed % 7) * 0.008;
  const peakForce = 2100 + (seed % 13) * 30;
  const rsiMod = 0.40 + (seed % 5) * 0.005;
  const asymmetry = 5.5 + (seed % 9) * 0.5;
  const takeoffV = 3.2 + (seed % 5) * 0.08;
  const flightTime = Math.sqrt((2 * h) / 9.81);

  const finalH = diff && diff.CMJ_JUMP_HEIGHT_FLIGHT_TIME !== undefined
    ? h + diff.CMJ_JUMP_HEIGHT_FLIGHT_TIME
    : h;
  const finalPeak = diff && diff.CMJ_PEAK_FORCE !== undefined
    ? peakForce + diff.CMJ_PEAK_FORCE
    : peakForce;
  const finalRsi = diff && diff.CMJ_RSI_MOD !== undefined
    ? rsiMod + diff.CMJ_RSI_MOD
    : rsiMod;

  return {
    id: `rev-cmj-${trialNumber}-${idSuffix}`,
    trialId: `trial-cmj-1-${trialNumber}`,
    revisionNumber: Number(idSuffix.replace(/^v/, '')) || 1,
    status,
    algorithmCode: 'CMJ_BILATERAL',
    algorithmVersion,
    metricDefinitions: CMJ_METRIC_DEFINITIONS,
    metricValues: [
      { metricCode: 'CMJ_JUMP_HEIGHT_FLIGHT_TIME', definitionVersion: '1.0', method: 'flight_time', value: Number(finalH.toFixed(3)), unit: 'm', side: 'total', confidence: 0.97, qualityFlags: [] },
      { metricCode: 'CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM', definitionVersion: '1.0', method: 'impulse_momentum', value: Number((finalH * 0.99).toFixed(3)), unit: 'm', side: 'total', confidence: 0.96, qualityFlags: [] },
      { metricCode: 'CMJ_PEAK_FORCE', definitionVersion: '1.0', method: 'peak_detection', value: finalPeak, unit: 'N', side: 'total', confidence: 0.99, qualityFlags: [] },
      { metricCode: 'CMJ_RSI_MOD', definitionVersion: '1.0', method: 'modified', value: Number(finalRsi.toFixed(3)), unit: 'm/s', side: 'total', confidence: 0.95, qualityFlags: [] },
      { metricCode: 'CMJ_ASYMMETRY_INDEX', definitionVersion: '1.0', method: 'bilateral_comparison', value: Number(asymmetry.toFixed(1)), unit: '%', side: 'bilateral', confidence: 0.94, qualityFlags: [] },
      { metricCode: 'CMJ_TAKEOFF_VELOCITY', definitionVersion: '1.0', method: 'impulse_integration', value: Number(takeoffV.toFixed(2)), unit: 'm/s', side: 'total', confidence: 0.95, qualityFlags: [] },
    ],
    events: [],
    segments: [],
    inputFingerprint: `sha256:cmj1-t${trialNumber}-v${algorithmVersion}-${seed}`,
    supersedesId,
    calculatedAt: `2026-09-29T09:44:${10 + trialNumber}Z`,
    publishedAt: status === 'Published' ? `2026-09-29T09:44:${25 + trialNumber}Z` : null,
    publishedBy: status === 'Published' ? 'System' : null,
    qualityFlags: [],
    provenance: {
      parameters: { bodyMassKg: 75.0, takeoffForceThresholdN: 100, eventDetectionMethod: 'force_threshold' },
      inputSchemaVersion: '1.0',
    },
  };
}

function sprintRevision(
  trialNumber: number,
  seed: number,
  algorithmVersion: string,
  status: ScientificResultRevision['status'],
): ScientificResultRevision {
  const peakV = 10.2 + (seed % 3) * 0.1;
  const reactionTime = 180 + (seed % 7) * 8;
  const gct = 88 - (seed % 4) * 2;

  return {
    id: `rev-sprint-${trialNumber}-v1`,
    trialId: `trial-sprint-1-${trialNumber}`,
    revisionNumber: 1,
    status,
    algorithmCode: 'SPRINT_30M_MULTIDEVICE',
    algorithmVersion,
    metricDefinitions: SPRINT_METRIC_DEFINITIONS,
    metricValues: [
      { metricCode: 'SPRINT_REACTION_TIME_BLOCK', definitionVersion: '1.0', method: 'force_threshold', value: reactionTime, unit: 'ms', side: 'rear', confidence: 0.96, qualityFlags: [] },
      { metricCode: 'SPRINT_PEAK_VELOCITY', definitionVersion: '1.0', method: 'radar_direct', value: Number(peakV.toFixed(2)), unit: 'm/s', side: 'total', confidence: 0.99, qualityFlags: [] },
      { metricCode: 'SPRINT_GROUND_CONTACT_TIME', definitionVersion: '1.0', method: 'force_bilateral', value: gct, unit: 'ms', side: 'total', confidence: 0.93, qualityFlags: [] },
      { metricCode: 'SPRINT_PEAK_GROUND_REACTION_FORCE', definitionVersion: '1.0', method: 'force_plate', value: 920 + (seed % 5) * 15, unit: 'N', side: 'front', confidence: 0.97, qualityFlags: [] },
      { metricCode: 'SPRINT_POWER_TRANSFER', definitionVersion: '1.0', method: 'impulse_rate', value: 5400 + (seed % 11) * 30, unit: 'N/s', side: 'rear', confidence: 0.91, qualityFlags: [] },
    ],
    events: [],
    segments: [],
    inputFingerprint: `sha256:sprint1-t${trialNumber}-v${algorithmVersion}-${seed}`,
    supersedesId: null,
    calculatedAt: `2026-09-29T09:52:${20 + trialNumber}Z`,
    publishedAt: status === 'Published' ? `2026-09-29T09:52:${35 + trialNumber}Z` : null,
    publishedBy: status === 'Published' ? 'System' : null,
    qualityFlags: [],
    provenance: {
      parameters: { radarPeakSmoothingMs: 150, blockForceThresholdN: 50, gctWindowMs: 200 },
      inputSchemaVersion: '1.0',
    },
  };
}

function cmjInput(trialNumber: number, seed: number): ScientificTrialInput {
  return {
    trialNumber,
    bodyMassKg: 75.0,
    bodyMassSource: 'KW-PROFILE-001',
    channels: [
      cmjChannel(trialNumber, seed, 'total'),
      cmjChannel(trialNumber, seed + 11, 'left'),
      cmjChannel(trialNumber, seed + 22, 'right'),
    ],
  };
}

function sprintInput(trialNumber: number, seed: number): ScientificTrialInput {
  return {
    trialNumber,
    bodyMassKg: 72.5,
    bodyMassSource: 'KW-PROFILE-002',
    channels: [
      sprintFrontBlockChannel(seed),
      sprintRearBlockChannel(seed),
    ],
  };
}

function cmjJob(trialNumber: number, queue: number, status: ScientificCalculationJob['status'] = 'Queued'): ScientificCalculationJob {
  return {
    id: `job-cmj-${trialNumber}`,
    trialId: `trial-cmj-1-${trialNumber}`,
    algorithmCode: 'CMJ_BILATERAL',
    algorithmVersion: queue === 0 ? '2.1' : queue === 1 ? '2.2' : '2.2',
    status,
    reason: '',
    retryCount: 0,
    maxRetries: 2,
    createdAt: `2026-09-29T09:43:${10 + queue}.000Z`,
    leaseExpiresAt: null,
    errorCode: null,
    resultRevisionId: null,
  };
}

const JOB_SUCCESS: ScientificCalculationJob['status'] = 'Succeeded';

export const SCENARIO_CMJ_SUCCESS: ScientificScenario = {
  id: 'scenario-cmj-success',
  label: 'CMJ – Full Success Pipeline',
  labelCn: 'CMJ – 全流程成功场景',
  description: 'Athlete a-1 performs 2 valid bilateral countermovement jumps. All trials uploaded, validated, calculated with CMJ_BILATERAL v2.1, published.',
  descriptionCn: '运动员 a-1 完成 2 次有效双侧反向纵跳。所有试次上传成功 → 校验通过 → v2.1 计算 → 发布。',
  athleteId: 'a-1',
  protocolCode: 'p-cmj-bilateral',
  trials: [
    {
      number: 1,
      status: 'published',
      input: cmjInput(1, 100),
      revision: cmjRevision(1, 100, '2.1', 'v1', 'Published'),
      calculationJob: { ...cmjJob(1, 0), status: JOB_SUCCESS, resultRevisionId: 'rev-cmj-1-v1' },
    },
    {
      number: 2,
      status: 'published',
      input: cmjInput(2, 200),
      revision: cmjRevision(2, 200, '2.1', 'v1', 'Published'),
      calculationJob: { ...cmjJob(2, 1), status: JOB_SUCCESS, resultRevisionId: 'rev-cmj-2-v1' },
    },
  ],
  timelineSummary: {
    upload: '2026-09-29T09:42:30Z',
    validation: '2026-09-29T09:43:05Z',
    calculation: '2026-09-29T09:43:45Z',
    publication: '2026-09-29T09:44:25Z',
  },
};

export const SCENARIO_SPRINT_MULTIDEVICE: ScientificScenario = {
  id: 'scenario-sprint-multidevice',
  label: 'Sprint 30m – Multi-Device',
  labelCn: '30m 短跑 – 多设备数据',
  description: '30m sprint combining two starting-block force plates (front + rear) and a radar gun. All devices upload individually, merged by trial number. Calculated with SPRINT_30M_MULTIDEVICE v1.1.',
  descriptionCn: '结合两块起跑台测力台（前+后）与雷达枪的 30m 短跑。各设备独立上传，按试次编号合并。用 SPRINT_30M_MULTIDEVICE v1.1 计算。',
  athleteId: 'a-2',
  protocolCode: 'p-sprint-30m',
  trials: [
    {
      number: 1,
      status: 'published',
      input: sprintInput(1, 300),
      revision: sprintRevision(1, 300, '1.1', 'Published'),
      calculationJob: { ...cmjJob(1, 0), algorithmCode: 'SPRINT_30M_MULTIDEVICE', algorithmVersion: '1.1', trialId: 'trial-sprint-1-1', status: JOB_SUCCESS, resultRevisionId: 'rev-sprint-1-v1' },
    },
    {
      number: 2,
      status: 'published',
      input: sprintInput(2, 400),
      revision: sprintRevision(2, 400, '1.1', 'Published'),
      calculationJob: { ...cmjJob(2, 1), algorithmCode: 'SPRINT_30M_MULTIDEVICE', algorithmVersion: '1.1', trialId: 'trial-sprint-1-2', status: JOB_SUCCESS, resultRevisionId: 'rev-sprint-2-v1' },
    },
  ],
  timelineSummary: {
    upload: '2026-09-29T09:40:10Z',
    validation: '2026-09-29T09:41:30Z',
    calculation: '2026-09-29T09:42:10Z',
    publication: '2026-09-29T09:43:05Z',
  },
};

const V1_PUBLISHED: ScientificResultRevision['status'] = 'Published';
const V2_SUPERSEDED: ScientificResultRevision['status'] = 'Superseded';
const V3_PUBLISHED: ScientificResultRevision['status'] = 'Published';

export const SCENARIO_RECALC_DIFF: ScientificScenario = {
  id: 'scenario-recalc-diff',
  label: 'CMJ – Algorithm Version Upgrade & Recalculation Diff',
  labelCn: 'CMJ – 算法升级 + 重算差异',
  description: 'Trial re-calculated twice to surface version delta: v2.1 (Published baseline) → v2.2 (refined takeoff detection, supersedes v1) → v3.0 (new asymmetric impulse method, supersedes v2). Diff table shows per-metric delta and provenance parameters.',
  descriptionCn: '同一试次被重算两次以暴露版本差异：v2.1（基线发布）→ v2.2（改进起跳检测，取代 v1）→ v3.0（新的不对称冲量法，取代 v2）。差异表展示每个 metric 的增量和算法参数来源。',
  athleteId: 'a-3',
  protocolCode: 'p-cmj-bilateral',
  trials: [
    {
      number: 1,
      status: 'published',
      input: cmjInput(1, 500),
      revision: cmjRevision(1, 500, '3.0', 'v3', V3_PUBLISHED, 'rev-cmj-1-v2', {
        CMJ_JUMP_HEIGHT_FLIGHT_TIME: 0.006,
        CMJ_PEAK_FORCE: 28,
        CMJ_RSI_MOD: 0.014,
      }),
      calculationJob: { ...cmjJob(1, 2), algorithmVersion: '3.0', trialId: 'trial-cmj-3-1', status: JOB_SUCCESS, resultRevisionId: 'rev-cmj-1-v3' },
    },
  ],
  timelineSummary: {
    upload: '2026-09-20T09:30:00Z',
    validation: '2026-09-20T09:30:30Z',
    calculation: '2026-09-20T09:31:00Z',
    publication: '2026-09-20T09:45:00Z',
  },
};

export const SCENARIO_UPLOAD_VALIDATION_FAILURE: ScientificScenario = {
  id: 'scenario-upload-validation-failure',
  label: 'CMJ – Upload Validation Failure (Missing Required Side)',
  labelCn: 'CMJ – 上传校验失败（缺少必需通道）',
  description: 'Athlete a-4 uploads CMJ data from a single force plate (right side only). Validation rejects because CMJ_BILATERAL requires both left and right channels. The scenario captures the exact validation error code, the rejected trial, and the corrective guidance shown to the operator.',
  descriptionCn: '运动员 a-4 只上传了右侧测力台的 CMJ 数据。校验拒绝，因为 CMJ_BILATERAL 要求双侧通道。此场景捕获精确的校验错误码、被拒绝的试次，以及给操作员显示的纠正指引。',
  athleteId: 'a-4',
  protocolCode: 'p-cmj-bilateral',
  trials: [
    {
      number: 1,
      status: 'failed',
      input: cmjInput(1, 600).channels.filter((c) => c.side !== 'left' && c.side !== 'total').length === 1
        ? cmjInput(1, 600)
        : {
            trialNumber: 1,
            bodyMassKg: 68.2,
            bodyMassSource: 'KW-PROFILE-004',
            channels: [cmjChannel(1, 600, 'right')],
          },
      revision: null,
      calculationJob: { ...cmjJob(1, 3), status: 'Failed', errorCode: 'SCHEMA_VALIDATION_ERROR', reason: 'Missing required channel: left bilateral channel required for CMJ_BILATERAL', resultRevisionId: null },
    },
    {
      number: 2,
      status: 'validating',
      input: cmjInput(2, 700),
      revision: null,
      calculationJob: null,
    },
  ],
  timelineSummary: {
    upload: '2026-09-30T14:05:00Z',
    validation: '2026-09-30T14:05:30Z',
    calculation: '—',
    publication: '—',
  },
};