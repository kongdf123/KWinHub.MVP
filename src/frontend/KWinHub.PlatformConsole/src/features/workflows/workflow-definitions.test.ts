import { describe, it, expect } from 'vitest';
import { PLATFORM_WORKFLOWS } from './workflow-definitions';
import { validateAllWorkflows } from './workflow-validation';
import { matchRoute } from '@/lib/router';

const VALID_ROUTE_PATTERNS = [
  '/',
  '/organizations',
  '/organizations/:id',
  '/users',
  '/catalogs/protocols',
  '/catalogs/metrics',
  '/catalogs/devices',
  '/catalogs/algorithms',
  '/audit-logs',
  '/workflows',
];

function isValidRoute(path: string): boolean {
  return VALID_ROUTE_PATTERNS.some((pattern) => matchRoute(pattern, path) !== null || pattern === path);
}

describe('workflow definitions', () => {
  it('have unique workflow ids', () => {
    const ids = PLATFORM_WORKFLOWS.map((w) => w.id);
    expect(new Set(ids).size).toBe(ids.length);
  });

  it('have unique step ids across all workflows', () => {
    const stepIds = PLATFORM_WORKFLOWS.flatMap((w) => w.steps.map((s) => s.id));
    expect(new Set(stepIds).size).toBe(stepIds.length);
  });

  it('have unique edge ids across all workflows', () => {
    const edgeIds = PLATFORM_WORKFLOWS.flatMap((w) => w.edges.map((e) => e.id));
    expect(new Set(edgeIds).size).toBe(edgeIds.length);
  });

  it('pass structural validation (edges, happy paths, recovery branches, spec refs)', () => {
    const errors = validateAllWorkflows(PLATFORM_WORKFLOWS);
    expect(errors).toEqual([]);
  });

  it('have non-empty happy paths', () => {
    for (const workflow of PLATFORM_WORKFLOWS) {
      expect(workflow.happyPath.length).toBeGreaterThan(0);
    }
  });

  it('reference only valid demo routes', () => {
    for (const workflow of PLATFORM_WORKFLOWS) {
      for (const step of workflow.steps) {
        if (step.demoRoute) {
          expect(isValidRoute(step.demoRoute)).toBe(true);
        }
      }
    }
  });

  it('have valid specification references on every step', () => {
    for (const workflow of PLATFORM_WORKFLOWS) {
      for (const step of workflow.steps) {
        expect(step.specificationRefs.length).toBeGreaterThan(0);
        for (const ref of step.specificationRefs) {
          expect(ref.documentPath.length).toBeGreaterThan(0);
        }
      }
    }
  });

  it('have bilingual content for every localized field', () => {
    for (const workflow of PLATFORM_WORKFLOWS) {
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