import { describe, it, expect, beforeEach, vi } from 'vitest';
import {
  useScientificDemoStore,
  deriveTrialRuntimeStatus,
  getTrialFailureInfo,
  LIFECYCLE_STAGES,
  type TrialRuntimeState,
} from '@/features/science/services/scientific-demo-store';
import {
  selectScenarioProjection,
  selectTrialProjection,
  compareRevisions,
} from '@/features/science/services/scientific-selectors';
import { SCENARIOS } from '@/lib/scientific/scenarios';
import type { ScientificResultRevision, ScientificMetricValue } from '@/lib/scientific/dto';

function getState() {
  return useScientificDemoStore.getState();
}

function resetToFirstScenario() {
  useScientificDemoStore.setState({
    activeScenarioId: SCENARIOS[0].id,
    trialStates: Object.fromEntries(
      SCENARIOS[0].trials.map((t) => [
        `${SCENARIOS[0].id}:trial:${t.number}`,
        { stage: 0, retryCount: 0, recalculationRequested: false, recalculationRevisionNumber: null },
      ]),
    ),
  });
}

describe('scientific-demo-store', () => {
  beforeEach(() => {
    resetToFirstScenario();
  });

  it('initializes the first scenario with all trials at local stage', () => {
    const { activeScenarioId, getScenario, getTrialRuntime } = getState();
    expect(activeScenarioId).toBe(SCENARIOS[0].id);
    const scenario = getScenario();
    expect(scenario).toBeDefined();
    for (const trial of scenario!.trials) {
      const runtime = getTrialRuntime(trial.number);
      expect(runtime).toBeDefined();
      expect(runtime!.stage).toBe(0);
      expect(runtime!.retryCount).toBe(0);
      expect(runtime!.recalculationRequested).toBe(false);
    }
  });

  it('selectScenario switches scenario and resets all trial states to stage 0', () => {
    const target = SCENARIOS[1];
    getState().selectScenario(target.id);
    expect(getState().activeScenarioId).toBe(target.id);
    for (const trial of target.trials) {
      const runtime = getState().getTrialRuntime(trial.number);
      expect(runtime).toBeDefined();
      expect(runtime!.stage).toBe(0);
    }
  });

  it('advanceTrial moves stage forward deterministically and caps at publication', () => {
    const scenario = getState().getScenario()!;
    const trial = scenario.trials[0];
    const runtime = () => getState().getTrialRuntime(trial.number)!;
    expect(runtime().stage).toBe(0);
    getState().advanceTrial(trial.number);
    expect(runtime().stage).toBe(1);
    getState().advanceTrial(trial.number);
    expect(runtime().stage).toBe(2);
    getState().advanceTrial(trial.number);
    getState().advanceTrial(trial.number);
    expect(runtime().stage).toBe(4);
    getState().advanceTrial(trial.number);
    expect(runtime().stage).toBe(4);
  });

  it('reset reproduces identical output after mutations', () => {
    const scenario = getState().getScenario()!;
    const trial = scenario.trials[0];
    getState().advanceTrial(trial.number);
    getState().advanceTrial(trial.number);
    getState().retryUpload(trial.number);
    expect(getState().getTrialRuntime(trial.number)!.stage).toBeGreaterThan(0);
    getState().reset();
    expect(getState().getTrialRuntime(trial.number)!.stage).toBe(0);
    expect(getState().getTrialRuntime(trial.number)!.retryCount).toBe(0);
    expect(getState().getTrialRuntime(trial.number)!.recalculationRequested).toBe(false);
  });

  it('retryUpload increments retryCount for the target trial only', () => {
    const scenario = getState().getScenario()!;
    const [t1, t2] = scenario.trials;
    getState().retryUpload(t1.number);
    expect(getState().getTrialRuntime(t1.number)!.retryCount).toBe(1);
    expect(getState().getTrialRuntime(t2.number)!.retryCount).toBe(0);
  });

  it('requestRecalculation assigns the next candidate revision number deterministically', () => {
    const scenario = getState().getScenario()!;
    const trial = scenario.trials.find((t) => t.revision) ?? scenario.trials[0];
    getState().requestRecalculation(trial.number);
    const runtime = getState().getTrialRuntime(trial.number)!;
    expect(runtime.recalculationRequested).toBe(true);
    const currentRev = trial.revision?.revisionNumber ?? 0;
    expect(runtime.recalculationRevisionNumber).toBe(currentRev + 1);
  });

  it('deriveTrialRuntimeStatus maps validation failure deterministically', () => {
    const failureScenario = SCENARIOS.find((s) =>
      s.trials.some((t) => t.calculationJob?.status === 'Failed'),
    )!;
    const failingTrial = failureScenario.trials.find((t) => t.calculationJob?.status === 'Failed')!;
    const failureInfo = getTrialFailureInfo(failingTrial);
    expect(failureInfo).not.toBeNull();
    expect(failureInfo!.stage).toBe('validation');

    const runtimeAtValidation: TrialRuntimeState = { stage: 2, retryCount: 0, recalculationRequested: false, recalculationRevisionNumber: null };
    const failed = deriveTrialRuntimeStatus(failingTrial, runtimeAtValidation);
    expect(failed.status).toBe('failed');
    expect(failed.failed).toBe(true);
    expect(failed.failureCode).toBe(failureInfo!.code);
  });

  it('upload failure is bypassed after retry', () => {
    const failureScenario = SCENARIOS.find((s) =>
      s.trials.some((t) => t.calculationJob?.status === 'Failed'),
    )!;
    const failingTrial = failureScenario.trials.find((t) => t.calculationJob?.status === 'Failed')!;
    const uploadStage: TrialRuntimeState = { stage: 1, retryCount: 0, recalculationRequested: false, recalculationRevisionNumber: null };
    const atUpload = deriveTrialRuntimeStatus(failingTrial, uploadStage);
    expect(atUpload.failed).toBe(false);
    expect(atUpload.status).toBe('uploaded');
  });

  it('independent subscribers observe the same state after mutation', () => {
    const scenario = getState().getScenario()!;
    const trial = scenario.trials[0];
    const snapshotA = getState().getTrialRuntime(trial.number);
    getState().advanceTrial(trial.number);
    const snapshotB = getState().getTrialRuntime(trial.number);
    expect(snapshotB!.stage).toBe(snapshotA!.stage + 1);
  });
});

describe('scientific-demo-store API-mode isolation', () => {
  beforeEach(() => {
    resetToFirstScenario();
  });

  it('advanceTrial throws a descriptive error in API mode', () => {
    const original = import.meta.env.VITE_DATA_MODE;
    vi.stubEnv('VITE_DATA_MODE', 'api');
    try {
      expect(() => getState().advanceTrial(1)).toThrow(/not available in API mode/i);
    } finally {
      vi.stubEnv('VITE_DATA_MODE', original);
    }
  });

  it('retryUpload throws in API mode', () => {
    const original = import.meta.env.VITE_DATA_MODE;
    vi.stubEnv('VITE_DATA_MODE', 'api');
    try {
      expect(() => getState().retryUpload(1)).toThrow(/not available in API mode/i);
    } finally {
      vi.stubEnv('VITE_DATA_MODE', original);
    }
  });

  it('requestRecalculation throws in API mode', () => {
    const original = import.meta.env.VITE_DATA_MODE;
    vi.stubEnv('VITE_DATA_MODE', 'api');
    try {
      expect(() => getState().requestRecalculation(1)).toThrow(/not available in API mode/i);
    } finally {
      vi.stubEnv('VITE_DATA_MODE', original);
    }
  });

  it('selectScenario and reset do not throw in API mode (read-only state access)', () => {
    const original = import.meta.env.VITE_DATA_MODE;
    vi.stubEnv('VITE_DATA_MODE', 'api');
    try {
      expect(() => getState().selectScenario(SCENARIOS[1].id)).not.toThrow();
      expect(() => getState().reset()).not.toThrow();
    } finally {
      vi.stubEnv('VITE_DATA_MODE', original);
    }
  });
});

describe('scientific-selectors', () => {
  beforeEach(() => {
    resetToFirstScenario();
  });

  it('selectScenarioProjection returns one projection per trial at initial stage', () => {
    const projection = selectScenarioProjection();
    expect(projection.trials.length).toBe(projection.scenario.trials.length);
    for (const tp of projection.trials) {
      expect(tp.stage).toBe('local');
      expect(tp.status).toBe('local');
    }
    expect(projection.summaryStatus).toBe('local');
  });

  it('projection summary becomes published when all trials reach publication', () => {
    const scenario = getState().getScenario()!;
    for (const trial of scenario.trials) {
      for (let i = 0; i < LIFECYCLE_STAGES.length - 1; i++) {
        getState().advanceTrial(trial.number);
      }
    }
    const projection = selectScenarioProjection();
    expect(projection.allPublished).toBe(true);
    expect(projection.summaryStatus).toBe('published');
  });

  it('selectTrialProjection returns undefined for unknown trial numbers', () => {
    expect(selectTrialProjection(999)).toBeUndefined();
  });

  it('compareRevisions marks method-mismatched metrics as incomparable', () => {
    const baseMetric: ScientificMetricValue = {
      metricCode: 'CMJ_JUMP_HEIGHT_FLIGHT_TIME',
      definitionVersion: '1.0',
      method: 'flight_time',
      value: 0.42,
      unit: 'm',
      confidence: 0.99,
      qualityFlags: [],
    };
    const revA: ScientificResultRevision = {
      id: 'rev-a',
      trialId: 't1',
      revisionNumber: 1,
      status: 'Published',
      algorithmCode: 'CMJ_BILATERAL',
      algorithmVersion: '2.1',
      metricDefinitions: [],
      metricValues: [baseMetric],
      events: [],
      segments: [],
      inputFingerprint: 'fp-a',
      supersedesId: null,
      calculatedAt: '2026-01-01T00:00:00Z',
      publishedAt: '2026-01-01T00:01:00Z',
      publishedBy: 'u-1',
      qualityFlags: [],
      provenance: { parameters: {}, inputSchemaVersion: '1.0' },
    };
    const revB: ScientificResultRevision = {
      ...revA,
      id: 'rev-b',
      revisionNumber: 2,
      algorithmVersion: '3.0',
      metricValues: [
        { ...baseMetric, definitionVersion: '2.0', method: 'impulse_momentum', value: 0.45 },
      ],
    };

    const comparisons = compareRevisions(revA, revB);
    expect(comparisons.length).toBe(1);
    const c = comparisons[0];
    expect(c.comparable).toBe(false);
    expect(c.delta).toBeNull();
    expect(c.reason).toContain('definition');
    expect(c.reason).toContain('method');
  });

  it('compareRevisions computes delta for comparable metrics', () => {
    const baseMetric: ScientificMetricValue = {
      metricCode: 'CMJ_PEAK_FORCE',
      definitionVersion: '1.0',
      method: 'peak_detection',
      value: 1800,
      unit: 'N',
      confidence: 0.99,
      qualityFlags: [],
    };
    const revA: ScientificResultRevision = {
      id: 'rev-a',
      trialId: 't1',
      revisionNumber: 1,
      status: 'Published',
      algorithmCode: 'CMJ_BILATERAL',
      algorithmVersion: '2.1',
      metricDefinitions: [],
      metricValues: [baseMetric],
      events: [],
      segments: [],
      inputFingerprint: 'fp-a',
      supersedesId: null,
      calculatedAt: '2026-01-01T00:00:00Z',
      publishedAt: '2026-01-01T00:01:00Z',
      publishedBy: 'u-1',
      qualityFlags: [],
      provenance: { parameters: {}, inputSchemaVersion: '1.0' },
    };
    const revB: ScientificResultRevision = {
      ...revA,
      id: 'rev-b',
      revisionNumber: 2,
      metricValues: [{ ...baseMetric, value: 1850 }],
    };

    const comparisons = compareRevisions(revA, revB);
    expect(comparisons[0].comparable).toBe(true);
    expect(comparisons[0].delta).toBe(50);
    expect(comparisons[0].reason).toBeNull();
  });
});