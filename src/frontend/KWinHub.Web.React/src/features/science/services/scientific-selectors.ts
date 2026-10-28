import {
  useScientificDemoStore,
  deriveTrialRuntimeStatus,
  getActiveRevision,
  getTrialFailureInfo,
  type TrialRuntimeState,
  type LifecycleStage,
  type TrialDisplayStatus,
  LIFECYCLE_STAGES,
} from './scientific-demo-store';
import type { ScientificScenario, ScientificResultRevision, ScientificMetricValue } from '@/lib/scientific/dto';
import { SCENARIOS } from '@/lib/scientific/scenarios';

type ScenarioTrial = ScientificScenario['trials'][number];

export interface TrialProjection {
  trialNumber: number;
  trial: ScenarioTrial;
  runtime: TrialRuntimeState;
  stage: LifecycleStage;
  status: TrialDisplayStatus;
  failed: boolean;
  failureCode: string | null;
  failureMessage: string | null;
  canAdvance: boolean;
  canRetryUpload: boolean;
  canRequestRecalculation: boolean;
  activeRevision: ScientificResultRevision | undefined;
  publishedRevisions: ScientificResultRevision[];
}

export interface ScenarioProjection {
  scenario: ScientificScenario;
  trials: TrialProjection[];
  allPublished: boolean;
  anyFailed: boolean;
  summaryStatus: 'local' | 'uploading' | 'validating' | 'processing' | 'published' | 'failed';
}

export function selectScenarioProjection(): ScenarioProjection {
  const scenario = useScientificDemoStore.getState().getScenario();
  if (!scenario) {
    throw new Error('[scientific-selectors] No active scenario');
  }
  const getTrialRuntime = useScientificDemoStore.getState().getTrialRuntime;

  const trials: TrialProjection[] = scenario.trials.map((trial) => {
    const runtime = getTrialRuntime(trial.number);
    if (!runtime) {
      throw new Error(`[scientific-selectors] Missing runtime state for trial ${trial.number}`);
    }
    const derived = deriveTrialRuntimeStatus(trial, runtime);
    const activeRevision = getActiveRevision(trial);
    const publishedRevisions = activeRevision ? [activeRevision] : [];

    const canAdvance =
      derived.stage !== 'publication' &&
      !derived.failed;

    const failureInfo = getTrialFailureInfo(trial);
    const canRetryUpload =
      derived.stage === 'upload' && derived.failed && failureInfo?.recoverable === true;

    const canRequestRecalculation =
      derived.stage === 'publication' && !runtime.recalculationRequested;

    return {
      trialNumber: trial.number,
      trial,
      runtime,
      stage: derived.stage,
      status: derived.status,
      failed: derived.failed,
      failureCode: derived.failureCode,
      failureMessage: derived.failureMessage,
      canAdvance,
      canRetryUpload,
      canRequestRecalculation,
      activeRevision,
      publishedRevisions,
    };
  });

  const allPublished = trials.every((t) => t.stage === 'publication');
  const anyFailed = trials.some((t) => t.failed);

  let summaryStatus: ScenarioProjection['summaryStatus'] = 'local';
  if (anyFailed) {
    summaryStatus = 'failed';
  } else if (allPublished) {
    summaryStatus = 'published';
  } else {
    const maxStage = Math.max(...trials.map((t) => LIFECYCLE_STAGES.indexOf(t.stage)));
    summaryStatus =
      (['local', 'uploading', 'validating', 'processing', 'published'] as const)[maxStage] ?? 'local';
  }

  return { scenario, trials, allPublished, anyFailed, summaryStatus };
}

export function selectActiveScenario(): ScientificScenario | undefined {
  return useScientificDemoStore.getState().getScenario();
}

export function selectActiveScenarioId(): string {
  return useScientificDemoStore.getState().activeScenarioId;
}

export function selectTrialProjection(trialNumber: number): TrialProjection | undefined {
  const projection = selectScenarioProjection();
  return projection.trials.find((t) => t.trialNumber === trialNumber);
}

export function selectRevisionForComparison(
  trial: ScenarioTrial,
  revisionNumberA: number,
  revisionNumberB: number,
): { revisionA?: ScientificResultRevision; revisionB?: ScientificResultRevision } {
  const revisionA = trial.revision && trial.revision.revisionNumber === revisionNumberA ? trial.revision : undefined;
  const revisionB = trial.revision && trial.revision.revisionNumber === revisionNumberB ? trial.revision : undefined;
  return { revisionA, revisionB };
}

export interface MetricComparison {
  metricCode: string;
  definitionVersionA: string;
  definitionVersionB: string;
  methodA: string;
  methodB: string;
  valueA: number;
  valueB: number;
  unitA: string;
  unitB: string;
  comparable: boolean;
  delta: number | null;
  reason: string | null;
}

export function compareRevisions(
  revisionA: ScientificResultRevision,
  revisionB: ScientificResultRevision,
): MetricComparison[] {
  const byCodeA = new Map(revisionA.metricValues.map((m) => [m.metricCode, m]));
  const comparisons: MetricComparison[] = [];

  for (const metricB of revisionB.metricValues) {
    const metricA = byCodeA.get(metricB.metricCode);
    if (!metricA) continue;
    const sameDefinition = metricA.definitionVersion === metricB.definitionVersion;
    const sameMethod = metricA.method === metricB.method;
    const comparable = sameDefinition && sameMethod;
    let delta: number | null = null;
    let reason: string | null = null;
    if (comparable) {
      delta = metricB.value - metricA.value;
    } else {
      const parts: string[] = [];
      if (!sameDefinition) parts.push(`definition ${metricA.definitionVersion} vs ${metricB.definitionVersion}`);
      if (!sameMethod) parts.push(`method ${metricA.method} vs ${metricB.method}`);
      reason = parts.join(', ');
    }
    comparisons.push({
      metricCode: metricB.metricCode,
      definitionVersionA: metricA.definitionVersion,
      definitionVersionB: metricB.definitionVersion,
      methodA: metricA.method,
      methodB: metricB.method,
      valueA: metricA.value,
      valueB: metricB.value,
      unitA: metricA.unit,
      unitB: metricB.unit,
      comparable,
      delta,
      reason,
    });
  }

  return comparisons;
}

export function flattenMetricValues(metrics: ScientificMetricValue[]): Record<string, number> {
  const out: Record<string, number> = {};
  for (const m of metrics) {
    out[m.metricCode] = m.value;
  }
  return out;
}

export type ScientificFixtureSummaryStatus = 'local' | 'uploading' | 'validating' | 'processing' | 'published' | 'failed';

export function findScenarioForSession(athleteId: string, protocolId: string): ScientificScenario | undefined {
  return SCENARIOS.find((s) => s.athleteId === athleteId && s.protocolCode === protocolId);
}

export function deriveScenarioFixtureStatus(scenario: ScientificScenario): ScientificFixtureSummaryStatus {
  const statuses = scenario.trials.map((t) => t.status);
  if (statuses.some((s) => s === 'failed')) return 'failed';
  if (statuses.every((s) => s === 'published')) return 'published';
  if (statuses.some((s) => s === 'calculating' || s === 'publishing')) return 'processing';
  if (statuses.some((s) => s === 'validating')) return 'validating';
  if (statuses.some((s) => s === 'uploading')) return 'uploading';
  return 'local';
}

export interface SessionScientificInfo {
  scenario: ScientificScenario;
  isActive: boolean;
  summaryStatus: ScientificFixtureSummaryStatus;
  trialCount: number;
  publishedTrials: number;
  failedTrials: number;
}

export function getSessionScientificInfo(
  athleteId: string,
  protocolId: string,
): SessionScientificInfo | null {
  const scenario = findScenarioForSession(athleteId, protocolId);
  if (!scenario) return null;
  const activeId = selectActiveScenarioId();
  const isActive = scenario.id === activeId;
  let summaryStatus: ScientificFixtureSummaryStatus;
  if (isActive) {
    const projection = selectScenarioProjection();
    summaryStatus = projection.summaryStatus;
  } else {
    summaryStatus = deriveScenarioFixtureStatus(scenario);
  }
  const trialCount = scenario.trials.length;
  const publishedTrials = scenario.trials.filter((t) => t.status === 'published').length;
  const failedTrials = scenario.trials.filter((t) => t.status === 'failed').length;
  return { scenario, isActive, summaryStatus, trialCount, publishedTrials, failedTrials };
}

export const SOURCE_APPLICATIONS: Record<string, { en: string; cn: string; color: string }> = {
  'sport-cmj': { en: 'KWinPerformance', cn: '坤维表现', color: 'text-teal-700' },
  'sport-imtp': { en: 'KWinPerformance', cn: '坤维表现', color: 'text-teal-700' },
  'sport-hj': { en: 'KWinPerformance', cn: '坤维表现', color: 'text-teal-700' },
  'sport-sprint': { en: 'KWinSprint', cn: '坤维冲刺', color: 'text-accent' },
  'sport-cod': { en: 'KWinSprint', cn: '坤维冲刺', color: 'text-accent' },
  'sport-swim': { en: 'KWinVideo', cn: '坤维视频', color: 'text-ink-600' },
};

export function getSourceApplication(sportId: string): { en: string; cn: string; color: string } | undefined {
  return SOURCE_APPLICATIONS[sportId];
}