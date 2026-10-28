import type { ScientificMetricDefinition } from './dto';

export function mulberry32(seed: number): () => number {
  let s = seed | 0;
  return () => {
    s = (s + 0x6d2b79f5) | 0;
    let t = Math.imul(s ^ (s >>> 15), 1 | s);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export interface CmjProfile {
  baselineForce: number;
  onsetRatio: number;
  peakForceRatio: number;
  takeoffRatio: number;
  landingRatio: number;
  eccentricDepth: number;
  noiseAmplitude: number;
}

export interface SprintProfile {
  sampleRateHz: number;
  durationMs: number;
  reactionTimeMs: number;
  blockOffMs: number;
  peakVelocityRatio: number;
  velocityPeakMs: number;
  accelerationShape: 'linear' | 'quadratic' | 'sigmoid';
  noiseAmplitude: number;
}

export interface ForceTimeCurve {
  sampleRateHz: number;
  durationMs: number;
  samples: number[];
  events: { code: string; offsetMs: number }[];
}

const DEFAULT_CMJ: CmjProfile = {
  baselineForce: 780,
  onsetRatio: 0.08,
  peakForceRatio: 0.42,
  takeoffRatio: 0.62,
  landingRatio: 0.9,
  eccentricDepth: 160,
  noiseAmplitude: 18,
};

export function generateCmjForceTime(
  totalMs: number,
  sampleRateHz: number,
  seed: number,
  profile: Partial<CmjProfile> = {},
): ForceTimeCurve {
  const p: CmjProfile = { ...DEFAULT_CMJ, ...profile };
  const rand = mulberry32(seed);
  const n = Math.round((totalMs / 1000) * sampleRateHz);
  const samples: number[] = [];

  const onsetIdx = Math.round(n * p.onsetRatio);
  const peakIdx = Math.round(n * p.peakForceRatio);
  const takeoffIdx = Math.round(n * p.takeoffRatio);
  const landingIdx = Math.round(n * p.landingRatio);

  for (let i = 0; i < n; i++) {
    let force = p.baselineForce;
    const noise = (rand() - 0.5) * p.noiseAmplitude;

    if (i < onsetIdx) {
      force = p.baselineForce;
    } else if (i < peakIdx) {
      const localT = (i - onsetIdx) / (peakIdx - onsetIdx);
      force = p.baselineForce - p.eccentricDepth * Math.sin(localT * Math.PI * 0.5);
    } else if (i < takeoffIdx) {
      const localT = (i - peakIdx) / (takeoffIdx - peakIdx);
      const eccentricEnd = p.baselineForce - p.eccentricDepth * Math.sin(
        ((peakIdx - onsetIdx) / (peakIdx - onsetIdx)) * Math.PI * 0.5,
      );
      const peakForceTarget = p.baselineForce + p.eccentricDepth * 10;
      force = eccentricEnd + (peakForceTarget - eccentricEnd) * Math.pow(localT, 0.4);
    } else if (i < landingIdx) {
      const localT = (i - takeoffIdx) / (landingIdx - takeoffIdx);
      const peakForce = p.baselineForce + p.eccentricDepth * 10;
      const landingForce = p.baselineForce * 0.95;
      force = peakForce * (1 - Math.pow(localT, 1.5)) + landingForce * localT;
      if (force < p.baselineForce * 0.15) force = 0;
    } else {
      const localT = (i - landingIdx) / (n - landingIdx);
      force = p.baselineForce + p.baselineForce * 0.15 * Math.exp(-localT * 5);
    }

    samples.push(Math.max(0, force + noise));
  }

  return {
    sampleRateHz,
    durationMs: totalMs,
    samples,
    events: [
      { code: 'onset', offsetMs: Math.round((onsetIdx / n) * totalMs) },
      { code: 'peak_force', offsetMs: Math.round((peakIdx / n) * totalMs) },
      { code: 'takeoff', offsetMs: Math.round((takeoffIdx / n) * totalMs) },
      { code: 'landing', offsetMs: Math.round((landingIdx / n) * totalMs) },
    ],
  };
}

const DEFAULT_SPRINT: SprintProfile = {
  sampleRateHz: 500,
  durationMs: 4500,
  reactionTimeMs: 220,
  blockOffMs: 420,
  peakVelocityRatio: 0.95,
  velocityPeakMs: 2800,
  accelerationShape: 'quadratic',
  noiseAmplitude: 0.25,
};

export function generateSprintVelocityCurve(
  seed: number,
  profile: Partial<SprintProfile> = {},
): ForceTimeCurve {
  const p: SprintProfile = { ...DEFAULT_SPRINT, ...profile };
  const rand = mulberry32(seed);
  const n = Math.round((p.durationMs / 1000) * p.sampleRateHz);
  const samples: number[] = [];

  for (let i = 0; i < n; i++) {
    const t = (i / p.sampleRateHz) * 1000;
    let v = 0;
    const noise = (rand() - 0.5) * p.noiseAmplitude;

    if (t < p.reactionTimeMs) {
      v = 0;
    } else if (t < p.blockOffMs) {
      const localT = (t - p.reactionTimeMs) / (p.blockOffMs - p.reactionTimeMs);
      v = 0.5 * localT;
    } else {
      const localT = t / p.durationMs;
      let accel: number;
      switch (p.accelerationShape) {
        case 'linear':
          accel = localT;
          break;
        case 'sigmoid':
          accel = 1 / (1 + Math.exp(-(localT - 0.4) * 10));
          break;
        default:
          accel = localT * localT;
      }
      v = accel * p.peakVelocityRatio + noise;
    }

    samples.push(Math.max(0, v));
  }

  return {
    sampleRateHz: p.sampleRateHz,
    durationMs: p.durationMs,
    samples,
    events: [
      { code: 'reaction', offsetMs: p.reactionTimeMs },
      { code: 'block_off', offsetMs: p.blockOffMs },
      { code: 'peak_velocity', offsetMs: p.velocityPeakMs },
      { code: 'finish_30m', offsetMs: p.durationMs },
    ],
  };
}

export function generateSprintForceSingleStep(
  seed: number,
  totalMs: number,
  sampleRateHz: number,
  side: 'front' | 'rear',
): ForceTimeCurve {
  const rand = mulberry32(seed);
  const n = Math.round((totalMs / 1000) * sampleRateHz);
  const samples: number[] = [];

  const baseline = side === 'rear' ? 420 : 360;
  const peak = side === 'rear' ? 1180 : 920;
  const peakRatio = 0.45;

  for (let i = 0; i < n; i++) {
    const localT = i / n;
    let force = baseline * 0.05;

    if (localT < 0.05) {
      force = 0;
    } else if (localT < peakRatio) {
      const t = (localT - 0.05) / (peakRatio - 0.05);
      force = (peak - baseline * 0.05) * Math.pow(t, 0.5) + baseline * 0.05;
    } else if (localT < 0.85) {
      const t = (localT - peakRatio) / (0.85 - peakRatio);
      force = peak * (1 - t * t);
    } else {
      const t = (localT - 0.85) / 0.15;
      force = peak * 0.15 * (1 - t);
    }

    const noise = (rand() - 0.5) * 12;
    samples.push(Math.max(0, force + noise));
  }

  const peakIdx = Math.round(n * peakRatio);
  return {
    sampleRateHz,
    durationMs: totalMs,
    samples,
    events: [
      { code: 'contact', offsetMs: Math.round(0.05 * totalMs) },
      { code: 'peak_force', offsetMs: Math.round((peakIdx / n) * totalMs) },
      { code: 'toe_off', offsetMs: Math.round(0.85 * totalMs) },
    ],
  };
}

export const CMJ_METRIC_DEFINITIONS: ScientificMetricDefinition[] = [
  {
    code: 'CMJ_JUMP_HEIGHT_FLIGHT_TIME',
    version: '1.0',
    name: 'Jump Height (Flight Time)',
    nameCn: '跳跃高度（腾空时间法）',
    unit: 'm',
    method: 'flight_time',
    methodCn: '腾空时间法',
    side: 'total',
    description: 'Jump height derived from flight time using impulse-momentum relationship.',
    descriptionCn: '基于腾空时间计算的跳跃高度。',
    category: 'Power',
  },
  {
    code: 'CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM',
    version: '1.0',
    name: 'Jump Height (Impulse-Momentum)',
    nameCn: '跳跃高度（冲量动量法）',
    unit: 'm',
    method: 'impulse_momentum',
    methodCn: '冲量动量法',
    side: 'total',
    description: 'Jump height derived from net vertical impulse during eccentric + concentric phase.',
    descriptionCn: '基于净垂直冲量计算的跳跃高度。',
    category: 'Power',
  },
  {
    code: 'CMJ_PEAK_FORCE',
    version: '1.0',
    name: 'Peak Vertical Ground Reaction Force',
    nameCn: '峰值垂直地面反力',
    unit: 'N',
    method: 'peak_detection',
    methodCn: '峰值检测',
    side: 'total',
    description: 'Maximum vertical force measured during the concentric push-off phase.',
    descriptionCn: '同心蹬伸阶段测量到的最大垂直力。',
    category: 'Force',
  },
  {
    code: 'CMJ_RSI_MOD',
    version: '1.0',
    name: 'RSI (Modified)',
    nameCn: '反应力量指数（修正）',
    unit: 'm/s',
    method: 'modified',
    methodCn: '修正法',
    side: 'total',
    description: 'Jump height divided by eccentric + concentric phase time.',
    descriptionCn: '跳跃高度除以离心+同心阶段总时间。',
    category: 'Rate',
  },
  {
    code: 'CMJ_ASYMMETRY_INDEX',
    version: '1.0',
    name: 'Asymmetry Index',
    nameCn: '左右不对称指数',
    unit: '%',
    method: 'bilateral_comparison',
    methodCn: '双侧比较',
    side: 'bilateral',
    description: 'Percentage difference between left and right peak forces.',
    descriptionCn: '左右峰值力之间的百分比差异。',
    category: 'Symmetry',
  },
  {
    code: 'CMJ_TAKEOFF_VELOCITY',
    version: '1.0',
    name: 'Takeoff Velocity',
    nameCn: '起跳速度',
    unit: 'm/s',
    method: 'impulse_integration',
    methodCn: '冲量积分',
    side: 'total',
    description: 'Vertical velocity at takeoff computed from net impulse and body mass.',
    descriptionCn: '通过净冲量和体重计算的起跳垂直速度。',
    category: 'Kinematics',
  },
];

export const SPRINT_METRIC_DEFINITIONS: ScientificMetricDefinition[] = [
  {
    code: 'SPRINT_REACTION_TIME_BLOCK',
    version: '1.0',
    name: 'Block Reaction Time',
    nameCn: '起跑反应时间',
    unit: 'ms',
    method: 'force_threshold',
    methodCn: '力阈值法',
    side: 'rear',
    description: 'Time from signal to rear foot force exceeding threshold on starting block.',
    descriptionCn: '从发令信号到后脚测力超过阈值的时间。',
    category: 'Timing',
  },
  {
    code: 'SPRINT_PEAK_VELOCITY',
    version: '1.0',
    name: 'Peak Velocity',
    nameCn: '峰值速度',
    unit: 'm/s',
    method: 'radar_direct',
    methodCn: '雷达直接测量',
    side: 'total',
    description: 'Maximum horizontal velocity achieved during 30m sprint.',
    descriptionCn: '30米短跑过程中达到的最大水平速度。',
    category: 'Kinematics',
  },
  {
    code: 'SPRINT_GROUND_CONTACT_TIME',
    version: '1.0',
    name: 'Ground Contact Time',
    nameCn: '地面接触时间',
    unit: 'ms',
    method: 'force_bilateral',
    methodCn: '双侧力检测',
    side: 'total',
    description: 'Average foot-ground contact time across all steps.',
    descriptionCn: '所有步幅的平均脚-地接触时间。',
    category: 'Contact',
  },
  {
    code: 'SPRINT_PEAK_GROUND_REACTION_FORCE',
    version: '1.0',
    name: 'Peak GRF (Single Step)',
    nameCn: '单步峰值地面反力',
    unit: 'N',
    method: 'force_plate',
    methodCn: '测力台',
    side: 'front',
    description: 'Maximum force recorded on front block plate during push-off.',
    descriptionCn: '前蹬板在蹬伸阶段的最大力值。',
    category: 'Force',
  },
  {
    code: 'SPRINT_POWER_TRANSFER',
    version: '1.0',
    name: 'Power Transfer Index',
    nameCn: '力量传递指数',
    unit: 'N/s',
    method: 'impulse_rate',
    methodCn: '冲量率',
    side: 'rear',
    description: 'Rate of force development on rear block as proxy for block efficiency.',
    descriptionCn: '后蹬板力发展率，作为起跑效率的代理指标。',
    category: 'Rate',
  },
];