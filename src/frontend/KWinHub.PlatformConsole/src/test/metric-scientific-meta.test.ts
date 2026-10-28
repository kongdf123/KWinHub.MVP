import { describe, it, expect } from 'vitest';
import {
  METRIC_SCIENTIFIC_META,
  ALGORITHM_SCIENTIFIC_META,
  getMetrics,
  getAlgorithms,
} from '../lib/mockApi';

describe('SCI-FE-005 metric scientific metadata consistency', () => {
  it('registers meta for every metric code that appears in a scientific algorithm', () => {
    const metricCodes = new Set(getMetrics().map((m) => m.code));
    const scientificAlgos = getAlgorithms().filter(
      (a) => a.code && ALGORITHM_SCIENTIFIC_META[a.code]
    );
    const referencedCodes = new Set<string>();
    scientificAlgos.forEach((algo) => {
      if (!algo.supportedMetricCodes) return;
      algo.supportedMetricCodes
        .split(',')
        .map((c) => c.trim())
        .filter(Boolean)
        .forEach((c) => referencedCodes.add(c));
    });

    referencedCodes.forEach((code) => {
      expect(metricCodes.has(code)).toBe(true);
      expect(METRIC_SCIENTIFIC_META[code]).toBeDefined();
    });
  });

  it('flight-time and impulse-momentum jump height are distinct methods', () => {
    const ft = METRIC_SCIENTIFIC_META.CMJ_JUMP_HEIGHT_FLIGHT_TIME;
    const im = METRIC_SCIENTIFIC_META.CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM;

    expect(ft).toBeDefined();
    expect(im).toBeDefined();
    expect(ft.method).not.toBe(im.method);
    expect(ft.methodLabel).not.toBe(im.methodLabel);
    expect(ft.formula).not.toBe(im.formula);
    expect(ft.physicalQuantity).toBe(im.physicalQuantity);
  });

  it('every registered metric meta has a definition version and a non-empty formula', () => {
    Object.values(METRIC_SCIENTIFIC_META).forEach((meta) => {
      expect(meta.definitionVersion).toMatch(/^\d+\.\d+$/);
      expect(meta.formula.length).toBeGreaterThan(0);
      expect(meta.method.length).toBeGreaterThan(0);
      expect(meta.methodLabel.length).toBeGreaterThan(0);
      expect(meta.physicalQuantity.length).toBeGreaterThan(0);
    });
  });

  it('dependentMetrics reference codes that exist in the metric catalog', () => {
    const allCodes = new Set(Object.keys(METRIC_SCIENTIFIC_META));
    Object.values(METRIC_SCIENTIFIC_META).forEach((meta) => {
      meta.dependentMetrics.forEach((code) => {
        expect(allCodes.has(code)).toBe(true);
      });
    });
  });

  it('every metric referenced by a scientific algorithm has scientific meta registered', () => {
    const referencedCodes = new Set<string>();
    getAlgorithms().forEach((algo) => {
      if (!algo.code || !ALGORITHM_SCIENTIFIC_META[algo.code]) return;
      if (!algo.supportedMetricCodes) return;
      algo.supportedMetricCodes
        .split(',')
        .map((c) => c.trim())
        .filter(Boolean)
        .forEach((c) => referencedCodes.add(c));
    });

    referencedCodes.forEach((code) => {
      expect(METRIC_SCIENTIFIC_META[code]).toBeDefined();
    });
  });

  it('method codes match the values used by Web.React scientific fixtures', () => {
    const expectedMethods = new Set([
      'flight_time',
      'impulse_momentum',
      'peak_detection',
      'modified',
      'bilateral_comparison',
      'impulse_integration',
      'force_threshold',
      'radar_direct',
      'force_bilateral',
      'force_plate',
      'impulse_rate',
    ]);
    Object.values(METRIC_SCIENTIFIC_META).forEach((meta) => {
      expect(expectedMethods.has(meta.method)).toBe(true);
    });
  });
});

describe('SCI-FE-005 algorithm-metric link consistency', () => {
  it('every algorithm output metric code exists in the metric catalog', () => {
    const metricCodes = new Set(getMetrics().map((m) => m.code));
    getAlgorithms().forEach((algo) => {
      if (!algo.supportedMetricCodes) return;
      const codes = algo.supportedMetricCodes
        .split(',')
        .map((c) => c.trim())
        .filter(Boolean);
      codes.forEach((code) => {
        expect(metricCodes.has(code)).toBe(true);
      });
    });
  });

  it('algorithm outputMetricCount matches the actual number of supported metric codes', () => {
    getAlgorithms().forEach((algo) => {
      if (!algo.code) return;
      const meta = ALGORITHM_SCIENTIFIC_META[algo.code];
      if (!meta) return;
      const codes = algo.supportedMetricCodes
        ? algo.supportedMetricCodes.split(',').map((c) => c.trim()).filter(Boolean)
        : [];
      expect(codes.length).toBe(meta.outputMetricCount);
    });
  });
});