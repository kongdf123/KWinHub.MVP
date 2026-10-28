import { describe, it, expect, beforeEach } from 'vitest';
import {
  ALGORITHM_SCIENTIFIC_META,
  getAlgorithms,
  getProtocols,
  mockRequest,
} from '../lib/mockApi';

describe('SCI-FE-004 algorithm scientific metadata consistency', () => {
  it('registers meta for both CMJ and Sprint algorithm families', () => {
    expect(ALGORITHM_SCIENTIFIC_META.CMJ_BILATERAL).toBeDefined();
    expect(ALGORITHM_SCIENTIFIC_META.SPRINT_30M_MULTIDEVICE).toBeDefined();
    expect(ALGORITHM_SCIENTIFIC_META.CMJ_BILATERAL.domain).toBe('CMJ');
    expect(ALGORITHM_SCIENTIFIC_META.SPRINT_30M_MULTIDEVICE.domain).toBe('Sprint');
  });

  it('every algorithm with a code has matching scientific meta or is explicitly Other', () => {
    const algos = getAlgorithms();
    algos.forEach((algo) => {
      if (!algo.code) return;
      const meta = ALGORITHM_SCIENTIFIC_META[algo.code];
      if (meta) {
        expect(meta.domain).toMatch(/^(CMJ|Sprint|Force|Other)$/);
        expect(meta.requiredChannels.length).toBeGreaterThan(0);
        expect(meta.parameters.length).toBeGreaterThan(0);
        expect(meta.supportedProtocols.length).toBeGreaterThan(0);
        expect(meta.outputMetricCount).toBeGreaterThan(0);
        expect(meta.releaseNotes.length).toBeGreaterThan(0);
      }
    });
  });

  it('outputMetricCount matches the number of supported metric codes', () => {
    const algos = getAlgorithms();
    algos.forEach((algo) => {
      if (!algo.code) return;
      const meta = ALGORITHM_SCIENTIFIC_META[algo.code];
      if (!meta) return;
      const codes = algo.supportedMetricCodes
        ? algo.supportedMetricCodes.split(',').map((c) => c.trim()).filter(Boolean)
        : [];
      expect(codes.length).toBe(meta.outputMetricCount);
    });
  });

  it('supportedProtocols reference protocol codes that exist in the catalog', () => {
    const protocolCodes = new Set(getProtocols().map((p) => p.code));
    Object.values(ALGORITHM_SCIENTIFIC_META).forEach((meta) => {
      meta.supportedProtocols.forEach((code) => {
        expect(protocolCodes.has(code)).toBe(true);
      });
    });
  });

  it('both CMJ and Sprint algorithm versions appear in the list', () => {
    const algos = getAlgorithms();
    const cmj = algos.filter((a) => a.code === 'CMJ_BILATERAL');
    const sprint = algos.filter((a) => a.code === 'SPRINT_30M_MULTIDEVICE');
    expect(cmj.length).toBeGreaterThan(0);
    expect(sprint.length).toBeGreaterThan(0);
  });
});

describe('SCI-FE-004 existing API status behavior preserved', () => {
  beforeEach(async () => {
    const algos = getAlgorithms();
    const draft = algos.find((a) => a.status === 'Draft');
    if (draft) {
      await mockRequest(`/platform/catalogs/algorithms/${draft.id}/status`, {
        method: 'POST',
        body: JSON.stringify({ action: 'archive' }),
      });
    }
  });

  it('publish action sets status to Published (no Validated state in API)', async () => {
    const algos = getAlgorithms();
    const draft = algos.find((a) => a.status === 'Draft') ?? algos[0];
    await mockRequest(`/platform/catalogs/algorithms/${draft.id}/status`, {
      method: 'POST',
      body: JSON.stringify({ action: 'publish' }),
    });
    const updated = getAlgorithms().find((a) => a.id === draft.id)!;
    expect(updated.status).toBe('Published');
  });

  it('archive action sets status to Archived', async () => {
    const algos = getAlgorithms();
    const published = algos.find((a) => a.status === 'Published') ?? algos[0];
    await mockRequest(`/platform/catalogs/algorithms/${published.id}/status`, {
      method: 'POST',
      body: JSON.stringify({ action: 'archive' }),
    });
    const updated = getAlgorithms().find((a) => a.id === published.id)!;
    expect(updated.status).toBe('Archived');
  });

  it('rejects unknown status actions', async () => {
    const algos = getAlgorithms();
    const target = algos[0];
    await expect(
      mockRequest(`/platform/catalogs/algorithms/${target.id}/status`, {
        method: 'POST',
        body: JSON.stringify({ action: 'validate' }),
      })
    ).rejects.toThrow();
  });
});