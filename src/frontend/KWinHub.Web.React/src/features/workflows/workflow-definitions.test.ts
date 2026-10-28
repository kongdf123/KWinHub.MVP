import { describe, it, expect } from 'vitest';
import { ORG_WORKFLOWS } from './workflow-definitions';
import { validateAllWorkflows } from './workflow-validation';

const VALID_VIEWS = new Set([
  'home', 'athletes', 'athlete-profile', 'sessions', 'session-detail',
  'protocols', 'protocol-detail', 'results', 'result-detail', 'performance',
  'trends', 'comparisons', 'biomechanics', 'datasets', 'raw-data',
  'data-sources', 'dashboards', 'integrations', 'sync-center', 'reports',
  'organization', 'users', 'roles', 'teams', 'devices', 'settings', 'workflows',
]);

describe('workflow definitions', () => {
  it('have unique workflow ids', () => {
    const ids = ORG_WORKFLOWS.map((w) => w.id);
    expect(new Set(ids).size).toBe(ids.length);
  });

  it('have unique step ids across all workflows', () => {
    const stepIds = ORG_WORKFLOWS.flatMap((w) => w.steps.map((s) => s.id));
    expect(new Set(stepIds).size).toBe(stepIds.length);
  });

  it('have unique edge ids across all workflows', () => {
    const edgeIds = ORG_WORKFLOWS.flatMap((w) => w.edges.map((e) => e.id));
    expect(new Set(edgeIds).size).toBe(edgeIds.length);
  });

  it('pass structural validation (edges, happy paths, recovery branches, spec refs)', () => {
    const errors = validateAllWorkflows(ORG_WORKFLOWS);
    expect(errors).toEqual([]);
  });

  it('have non-empty happy paths', () => {
    for (const workflow of ORG_WORKFLOWS) {
      expect(workflow.happyPath.length).toBeGreaterThan(0);
    }
  });

  it('reference only valid demo routes', () => {
    for (const workflow of ORG_WORKFLOWS) {
      for (const step of workflow.steps) {
        if (step.demoRoute) {
          expect(VALID_VIEWS.has(step.demoRoute)).toBe(true);
        }
      }
    }
  });

  it('have valid specification references on every step', () => {
    for (const workflow of ORG_WORKFLOWS) {
      for (const step of workflow.steps) {
        expect(step.specificationRefs.length).toBeGreaterThan(0);
        for (const ref of step.specificationRefs) {
          expect(ref.documentPath.length).toBeGreaterThan(0);
        }
      }
    }
  });

  it('have bilingual content for every localized field', () => {
    for (const workflow of ORG_WORKFLOWS) {
      expect(workflow.name.en.length).toBeGreaterThan(0);
      expect(workflow.name.zhCN.length).toBeGreaterThan(0);
      for (const step of workflow.steps) {
        expect(step.title.en.length).toBeGreaterThan(0);
        expect(step.title.zhCN.length).toBeGreaterThan(0);
        expect(step.description.en.length).toBeGreaterThan(0);
        expect(step.description.zhCN.length).toBeGreaterThan(0);
        expect(step.actor.en.length).toBeGreaterThan(0);
        expect(step.actor.zhCN.length).toBeGreaterThan(0);
      }
    }
  });
});