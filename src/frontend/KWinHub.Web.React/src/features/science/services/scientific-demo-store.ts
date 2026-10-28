import { create } from 'zustand';
import { SCENARIOS, type ScientificScenario } from '@/lib/scientific/scenarios';
import type { ScientificResultRevision } from '@/lib/scientific/dto';

type ScenarioTrial = ScientificScenario['trials'][number];

function isApiMode(): boolean {
  return import.meta.env.VITE_DATA_MODE === 'api';
}

function ensureDemoMode(action: string): void {
  if (isApiMode()) {
    throw new Error(
      `[scientific-demo] ${action} is not available in API mode. ` +
        'Scenario controls (advance/retry/recalculate) are demo-only and must not fabricate successful operations against a live backend.',
    );
  }
}

export const LIFECYCLE_STAGES = ['local', 'upload', 'validation', 'calculation', 'publication'] as const;
export type LifecycleStage = (typeof LIFECYCLE_STAGES)[number];
export const LIFECYCLE_STAGE_LABELS: Record<LifecycleStage, string> = {
  local: 'Local capture',
  upload: 'Upload',
  validation: 'Validation',
  calculation: 'Calculation',
  publication: 'Publication',
};

export type TrialDisplayStatus =
  | 'local'
  | 'uploaded'
  | 'upload_failed'
  | 'validating'
  | 'succeeded'
  | 'failed'
  | 'published';

export interface TrialRuntimeState {
  stage: number;
  retryCount: number;
  recalculationRequested: boolean;
  recalculationRevisionNumber: number | null;
}

export interface TrialFailureInfo {
  stage: LifecycleStage;
  code: string;
  message: string;
  recoverable: boolean;
}

export function getTrialFailureInfo(trial: ScenarioTrial): TrialFailureInfo | null {
  const job = trial.calculationJob;
  if (!job || job.status !== 'Failed') return null;
  const code = job.errorCode ?? 'CALCULATION_ERROR';
  const message = job.reason ?? 'Calculation failed';
  let stage: LifecycleStage = 'calculation';
  if (code.includes('VALIDATION') || code.includes('SCHEMA')) stage = 'validation';
  if (code.includes('UPLOAD') || code.includes('TRANSFER')) stage = 'upload';
  const recoverable = stage === 'upload';
  return { stage, code, message, recoverable };
}

function trialKey(scenarioId: string, trialNumber: number): string {
  return `${scenarioId}:trial:${trialNumber}`;
}

function buildInitialTrialStates(scenario: ScientificScenario): Record<string, TrialRuntimeState> {
  const states: Record<string, TrialRuntimeState> = {};
  for (const trial of scenario.trials) {
    states[trialKey(scenario.id, trial.number)] = {
      stage: 0,
      retryCount: 0,
      recalculationRequested: false,
      recalculationRevisionNumber: null,
    };
  }
  return states;
}

export interface ScientificDemoState {
  activeScenarioId: string;
  trialStates: Record<string, TrialRuntimeState>;
  selectScenario: (id: string) => void;
  reset: () => void;
  resetTrial: (trialNumber: number) => void;
  advanceTrial: (trialNumber: number) => void;
  retryUpload: (trialNumber: number) => void;
  requestRecalculation: (trialNumber: number) => void;
  getScenario: () => ScientificScenario | undefined;
  getTrialRuntime: (trialNumber: number) => TrialRuntimeState | undefined;
}

export const useScientificDemoStore = create<ScientificDemoState>((set, get) => {
  const initialScenario = SCENARIOS[0];
  const initialStates = buildInitialTrialStates(initialScenario);

  return {
    activeScenarioId: initialScenario.id,
    trialStates: initialStates,

    selectScenario: (id: string) => {
      const scenario = SCENARIOS.find((s) => s.id === id);
      if (!scenario) return;
      set({
        activeScenarioId: id,
        trialStates: buildInitialTrialStates(scenario),
      });
    },

    reset: () => {
      const scenario = get().getScenario();
      if (!scenario) return;
      set({ trialStates: buildInitialTrialStates(scenario) });
    },

    resetTrial: (trialNumber: number) => {
      ensureDemoMode('resetTrial');
      const { activeScenarioId, trialStates } = get();
      const key = trialKey(activeScenarioId, trialNumber);
      if (!(key in trialStates)) return;
      set({
        trialStates: {
          ...trialStates,
          [key]: { stage: 0, retryCount: 0, recalculationRequested: false, recalculationRevisionNumber: null },
        },
      });
    },

    advanceTrial: (trialNumber: number) => {
      ensureDemoMode('advanceTrial');
      const { activeScenarioId, trialStates } = get();
      const key = trialKey(activeScenarioId, trialNumber);
      const current = trialStates[key];
      if (!current) return;
      if (current.stage >= LIFECYCLE_STAGES.length - 1) return;
      set({
        trialStates: {
          ...trialStates,
          [key]: { ...current, stage: current.stage + 1 },
        },
      });
    },

    retryUpload: (trialNumber: number) => {
      ensureDemoMode('retryUpload');
      const { activeScenarioId, trialStates } = get();
      const key = trialKey(activeScenarioId, trialNumber);
      const current = trialStates[key];
      if (!current) return;
      set({
        trialStates: {
          ...trialStates,
          [key]: { ...current, retryCount: current.retryCount + 1 },
        },
      });
    },

    requestRecalculation: (trialNumber: number) => {
      ensureDemoMode('requestRecalculation');
      const { activeScenarioId, trialStates, getScenario } = get();
      const scenario = getScenario();
      if (!scenario) return;
      const trial = scenario.trials.find((t) => t.number === trialNumber);
      if (!trial) return;
      const key = trialKey(activeScenarioId, trialNumber);
      const current = trialStates[key];
      if (!current) return;
      const currentRev = trial.revision?.revisionNumber ?? 0;
      const nextRevision = Math.max(currentRev, current.recalculationRevisionNumber ?? 0) + 1;
      set({
        trialStates: {
          ...trialStates,
          [key]: {
            ...current,
            recalculationRequested: true,
            recalculationRevisionNumber: current.recalculationRevisionNumber ?? nextRevision,
          },
        },
      });
    },

    getScenario: () => SCENARIOS.find((s) => s.id === get().activeScenarioId),

    getTrialRuntime: (trialNumber: number) => {
      const { activeScenarioId, trialStates } = get();
      return trialStates[trialKey(activeScenarioId, trialNumber)];
    },
  };
});

export interface DerivedTrialStatus {
  status: TrialDisplayStatus;
  stage: LifecycleStage;
  failed: boolean;
  failureCode: string | null;
  failureMessage: string | null;
}

export function deriveTrialRuntimeStatus(
  trial: ScenarioTrial,
  runtime: TrialRuntimeState,
): DerivedTrialStatus {
  const stage: LifecycleStage = LIFECYCLE_STAGES[runtime.stage] ?? 'publication';
  const failure = getTrialFailureInfo(trial);
  const failedAtCurrentStage = failure && failure.stage === stage;
  const uploadBypassed = failure?.stage === 'upload' && runtime.retryCount > 0;
  const failed = !!failedAtCurrentStage && !uploadBypassed;

  let status: TrialDisplayStatus = 'local';
  if (runtime.stage === 0) {
    status = 'local';
  } else if (runtime.stage === 1) {
    status = failed ? 'upload_failed' : 'uploaded';
  } else if (runtime.stage === 2) {
    status = failed ? 'failed' : 'validating';
  } else if (runtime.stage === 3) {
    status = failed ? 'failed' : 'succeeded';
  } else {
    status = 'published';
  }

  return {
    status,
    stage,
    failed,
    failureCode: failed ? (failure?.code ?? null) : null,
    failureMessage: failed ? (failure?.message ?? null) : null,
  };
}

export function getActiveRevision(trial: ScenarioTrial): ScientificResultRevision | undefined {
  return trial.revision ?? undefined;
}

export { SCENARIOS };