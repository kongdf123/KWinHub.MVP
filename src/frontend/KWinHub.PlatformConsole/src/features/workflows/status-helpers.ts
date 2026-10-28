import type { DecisionStatus, ImplementationStatus, DemoBehavior } from './types';

export const DECISION_STATUS_LABELS: Record<DecisionStatus, { en: string; zhCN: string }> = {
  AGREED: { en: 'Agreed', zhCN: '已确认' },
  PROPOSED: { en: 'Proposed', zhCN: '提议' },
  ASSUMPTION: { en: 'Assumption', zhCN: '假设' },
  'OPEN QUESTION': { en: 'Open question', zhCN: '待决问题' },
  'OUT OF SCOPE': { en: 'Out of scope', zhCN: '范围外' },
};

export const DECISION_STATUS_STYLES: Record<DecisionStatus, string> = {
  AGREED: 'bg-success-50 text-success-700 border-success-200',
  PROPOSED: 'bg-teal-50 text-teal-700 border-teal-200',
  ASSUMPTION: 'bg-ink-100 text-ink-600 border-ink-200',
  'OPEN QUESTION': 'bg-warning-50 text-warning-700 border-warning-200',
  'OUT OF SCOPE': 'bg-invalid-50 text-invalid-700 border-invalid-200',
};

export const IMPLEMENTATION_STATUS_LABELS: Record<ImplementationStatus, { en: string; zhCN: string }> = {
  unknown: { en: 'Unknown', zhCN: '未知' },
  'not-started': { en: 'Not started', zhCN: '未开始' },
  prototype: { en: 'Prototype', zhCN: '原型' },
  integrated: { en: 'Integrated', zhCN: '已集成' },
  verified: { en: 'Verified', zhCN: '已验证' },
};

export const IMPLEMENTATION_STATUS_STYLES: Record<ImplementationStatus, string> = {
  unknown: 'bg-ink-100 text-ink-500 border-ink-200',
  'not-started': 'bg-ink-100 text-ink-600 border-ink-200',
  prototype: 'bg-teal-50 text-teal-700 border-teal-200',
  integrated: 'bg-success-50 text-success-700 border-success-200',
  verified: 'bg-success-50 text-success-700 border-success-200',
};

export const DEMO_BEHAVIOR_LABELS: Record<DemoBehavior, { en: string; zhCN: string }> = {
  unknown: { en: 'Simulation: unknown', zhCN: '模拟：未知' },
  mock: { en: 'Simulation: mock', zhCN: '模拟：模拟数据' },
  mixed: { en: 'Simulation: mixed', zhCN: '模拟：混合' },
  live: { en: 'Simulation: live', zhCN: '模拟：实时' },
};

export const DEMO_BEHAVIOR_STYLES: Record<DemoBehavior, string> = {
  unknown: 'bg-ink-100 text-ink-500 border-ink-200',
  mock: 'bg-warning-50 text-warning-700 border-warning-200',
  mixed: 'bg-teal-50 text-teal-700 border-teal-200',
  live: 'bg-success-50 text-success-700 border-success-200',
};