import { describe, it, expect, beforeEach } from 'vitest';
import {
  useScientificDemoStore,
} from '@/features/science/services/scientific-demo-store';
import {
  findScenarioForSession,
  deriveScenarioFixtureStatus,
  getSessionScientificInfo,
  getSourceApplication,
  SOURCE_APPLICATIONS,
} from '@/features/science/services/scientific-selectors';
import { SCENARIOS } from '@/lib/scientific/scenarios';

function resetStore() {
  useScientificDemoStore.setState({
    activeScenarioId: null,
    trialStates: {},
  });
}

describe('session-to-scenario mapping', () => {
  beforeEach(resetStore);

  describe('findScenarioForSession', () => {
    it('maps CMJ sessions of athlete a-1 to scenario-cmj-success', () => {
      const scenario = findScenarioForSession('a-1', 'p-cmj-bilateral');
      expect(scenario).toBeDefined();
      expect(scenario?.id).toBe('scenario-cmj-success');
    });

    it('maps Sprint sessions of athlete a-2 to scenario-sprint-multidevice', () => {
      const scenario = findScenarioForSession('a-2', 'p-sprint-30m');
      expect(scenario).toBeDefined();
      expect(scenario?.id).toBe('scenario-sprint-multidevice');
    });

    it('returns undefined for sessions without a scenario', () => {
      expect(findScenarioForSession('a-1', 'p-imtp-legacy')).toBeUndefined();
      expect(findScenarioForSession('a-99', 'p-cmj-bilateral')).toBeUndefined();
    });
  });

  describe('deriveScenarioFixtureStatus', () => {
    it('returns published when all trials are published', () => {
      const cmjSuccess = SCENARIOS.find((s) => s.id === 'scenario-cmj-success')!;
      expect(deriveScenarioFixtureStatus(cmjSuccess)).toBe('published');
    });

    it('returns failed when any trial has failed status', () => {
      const uploadFailure = SCENARIOS.find((s) => s.id === 'scenario-upload-validation-failure')!;
      expect(deriveScenarioFixtureStatus(uploadFailure)).toBe('failed');
    });

    it('returns published for the recalc-diff scenario (published baseline with revisions)', () => {
      const recalcDiff = SCENARIOS.find((s) => s.id === 'scenario-recalc-diff')!;
      expect(deriveScenarioFixtureStatus(recalcDiff)).toBe('published');
    });
  });

  describe('getSessionScientificInfo', () => {
    it('returns null for non-featured sessions', () => {
      expect(getSessionScientificInfo('a-1', 'p-imtp-legacy')).toBeNull();
    });

    it('returns fixture status when no scenario is active', () => {
      const info = getSessionScientificInfo('a-1', 'p-cmj-bilateral');
      expect(info).not.toBeNull();
      expect(info?.scenario.id).toBe('scenario-cmj-success');
      expect(info?.isActive).toBe(false);
      expect(info?.summaryStatus).toBe('published');
      expect(info?.trialCount).toBe(2);
      expect(info?.publishedTrials).toBe(2);
      expect(info?.failedTrials).toBe(0);
    });

    it('returns isActive true when the matching scenario is active', () => {
      useScientificDemoStore.getState().selectScenario('scenario-cmj-success');
      const info = getSessionScientificInfo('a-1', 'p-cmj-bilateral');
      expect(info?.isActive).toBe(true);
    });

    it('reflects live projection status for the active scenario and stays consistent with store', () => {
      useScientificDemoStore.getState().selectScenario('scenario-cmj-success');
      const before = getSessionScientificInfo('a-1', 'p-cmj-bilateral');
      expect(before?.summaryStatus).toBe('local');

      useScientificDemoStore.getState().advanceTrial(1);
      useScientificDemoStore.getState().advanceTrial(1);
      useScientificDemoStore.getState().advanceTrial(1);
      useScientificDemoStore.getState().advanceTrial(1);

      const after = getSessionScientificInfo('a-1', 'p-cmj-bilateral');
      expect(after?.summaryStatus).toBe('published');
    });

    it('returns failed fixture status for the upload-validation-failure scenario', () => {
      const info = getSessionScientificInfo('a-4', 'p-cmj-bilateral');
      expect(info?.summaryStatus).toBe('failed');
      expect(info?.failedTrials).toBeGreaterThan(0);
    });
  });

  describe('getSourceApplication', () => {
    it('maps CMJ/IMTP/HJ to KWinPerformance', () => {
      expect(getSourceApplication('sport-cmj')?.en).toBe('KWinPerformance');
      expect(getSourceApplication('sport-imtp')?.en).toBe('KWinPerformance');
      expect(getSourceApplication('sport-hj')?.en).toBe('KWinPerformance');
    });

    it('maps Sprint/COD to KWinSprint', () => {
      expect(getSourceApplication('sport-sprint')?.en).toBe('KWinSprint');
      expect(getSourceApplication('sport-cod')?.en).toBe('KWinSprint');
    });

    it('maps Swim to KWinVideo', () => {
      expect(getSourceApplication('sport-swim')?.en).toBe('KWinVideo');
    });

    it('returns undefined for unknown sports', () => {
      expect(getSourceApplication('sport-unknown')).toBeUndefined();
    });

    it('covers every known sport id in the source app table', () => {
      expect(Object.keys(SOURCE_APPLICATIONS)).toEqual(
        expect.arrayContaining(['sport-cmj', 'sport-imtp', 'sport-hj', 'sport-sprint', 'sport-cod', 'sport-swim']),
      );
    });
  });
});