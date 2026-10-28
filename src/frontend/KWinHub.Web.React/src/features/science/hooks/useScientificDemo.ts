import { useMemo } from 'react';
import { useScientificDemoStore } from '../services/scientific-demo-store';
import {
  selectScenarioProjection,
  selectActiveScenario,
  selectActiveScenarioId,
  selectTrialProjection,
  type ScenarioProjection,
  type TrialProjection,
} from '../services/scientific-selectors';
import type { ScientificScenario } from '@/lib/scientific/dto';

export function useScientificDemo(): {
  scenario: ScientificScenario | undefined;
  activeScenarioId: string;
  projection: ScenarioProjection;
  trialProjection: (trialNumber: number) => TrialProjection | undefined;
  selectScenario: (id: string) => void;
  reset: () => void;
  resetTrial: (trialNumber: number) => void;
  advanceTrial: (trialNumber: number) => void;
  retryUpload: (trialNumber: number) => void;
  requestRecalculation: (trialNumber: number) => void;
  apiMode: boolean;
} {
  const activeScenarioId = useScientificDemoStore((s) => s.activeScenarioId);
  const trialStates = useScientificDemoStore((s) => s.trialStates);
  const selectScenario = useScientificDemoStore((s) => s.selectScenario);
  const reset = useScientificDemoStore((s) => s.reset);
  const resetTrial = useScientificDemoStore((s) => s.resetTrial);
  const advanceTrial = useScientificDemoStore((s) => s.advanceTrial);
  const retryUpload = useScientificDemoStore((s) => s.retryUpload);
  const requestRecalculation = useScientificDemoStore((s) => s.requestRecalculation);

  const projection = useMemo(() => selectScenarioProjection(), [activeScenarioId, trialStates]);
  const scenario = useMemo(() => selectActiveScenario(), [activeScenarioId]);
  const apiMode = import.meta.env.VITE_DATA_MODE === 'api';

  const trialProjection = useMemo(
    () => (trialNumber: number) => selectTrialProjection(trialNumber),
    [activeScenarioId, trialStates],
  );

  return {
    scenario,
    activeScenarioId,
    projection,
    trialProjection,
    selectScenario,
    reset,
    resetTrial,
    advanceTrial,
    retryUpload,
    requestRecalculation,
    apiMode,
  };
}

export { useScientificDemoStore, selectActiveScenarioId };