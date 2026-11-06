import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { DEMO_ATHLETES, DEMO_DEVICES, DEMO_PROTOCOLS } from '@/lib/demo-data';
import { appendSession, clearGeneratedSessions, findByIdempotencyKey, generatedSessionCount } from '@/lib/ingestion-demo-store';
import {
  advanceResult,
  buildIdempotencyKey,
  buildManifest,
  buildMockSession,
  createInitialResult,
  stageDelay,
} from './engine';
import type { CsvPreview, DeviceTrialGroup, FailureMode, SimulationResult, UploadedFile, WizardState } from './types';

const genTrialId = (deviceExt: string, n: number) => `trial-${deviceExt}-${n}`;

function defaultDevices(): DeviceTrialGroup[] {
  const d = DEMO_DEVICES[0];
  return [
    {
      device: {
        id: d.id,
        externalDeviceId: d.serialNumber,
        type: d.type,
        serialNumber: d.serialNumber,
      },
      trials: [
        { externalTrialId: genTrialId(d.serialNumber, 1), trialNumber: 1, status: 'valid' },
        { externalTrialId: genTrialId(d.serialNumber, 2), trialNumber: 2, status: 'valid' },
        { externalTrialId: genTrialId(d.serialNumber, 3), trialNumber: 3, status: 'valid' },
      ],
    },
  ];
}

function initialWizard(): WizardState {
  return {
    clientId: 'kwinhub-desktop-client',
    clientApplication: 'KWinHub Desktop',
    clientVersion: '1.4.2',
    externalSessionId: `sess-${Date.now().toString(36)}`,
    sessionName: '',
    date: new Date().toISOString().slice(0, 16),
    environment: 'staging',
    operatorName: 'Demo Operator',
    athleteId: DEMO_ATHLETES[0].id,
    protocolId: DEMO_PROTOCOLS[0].id,
    devices: defaultDevices(),
    csvFile: null,
    csvPreview: null,
    videoFile: null,
    notes: '',
    failureMode: 'none',
  };
}

export function useUploadSimulator() {
  const [step, setStep] = useState(0);
  const [wizard, setWizard] = useState<WizardState>(initialWizard);
  const [result, setResult] = useState<SimulationResult | null>(null);
  const [isRunning, setIsRunning] = useState(false);
  const [history, setHistory] = useState<SimulationResult[]>([]);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const runningRef = useRef(false);

  const clearTimer = useCallback(() => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }
  }, []);

  useEffect(() => () => clearTimer(), [clearTimer]);

  const update = useCallback(<K extends keyof WizardState>(key: K, value: WizardState[K]) => {
    setWizard((w) => ({ ...w, [key]: value }));
  }, []);

  const regenerateExternalSessionId = useCallback(() => {
    setWizard((w) => ({ ...w, externalSessionId: `sess-${Date.now().toString(36)}` }));
  }, []);

  const addDevice = useCallback(() => {
    setWizard((w) => {
      const d = DEMO_DEVICES[w.devices.length % DEMO_DEVICES.length];
      const extId = `${d.serialNumber}-${w.devices.length + 1}`;
      return {
        ...w,
        devices: [
          ...w.devices,
          {
            device: { id: d.id, externalDeviceId: extId, type: d.type, serialNumber: d.serialNumber },
            trials: [{ externalTrialId: genTrialId(extId, 1), trialNumber: 1, status: 'valid' }],
          },
        ],
      };
    });
  }, []);

  const removeDevice = useCallback((idx: number) => {
    setWizard((w) => ({ ...w, devices: w.devices.filter((_, i) => i !== idx) }));
  }, []);

  const addTrial = useCallback((deviceIdx: number) => {
    setWizard((w) => {
      const devices = w.devices.map((g, i) => {
        if (i !== deviceIdx) return g;
        const n = g.trials.length + 1;
        return {
          ...g,
          trials: [...g.trials, { externalTrialId: genTrialId(g.device.externalDeviceId, n), trialNumber: n, status: 'valid' }],
        };
      });
      return { ...w, devices };
    });
  }, []);

  const removeTrial = useCallback((deviceIdx: number, trialIdx: number) => {
    setWizard((w) => {
      const devices = w.devices.map((g, i) => {
        if (i !== deviceIdx) return g;
        return { ...g, trials: g.trials.filter((_, ti) => ti !== trialIdx) };
      });
      return { ...w, devices };
    });
  }, []);

  const updateTrialStatus = useCallback((deviceIdx: number, trialIdx: number, status: 'valid' | 'invalid') => {
    setWizard((w) => {
      const devices = w.devices.map((g, i) => {
        if (i !== deviceIdx) return g;
        const trials = g.trials.map((t, ti) => (ti === trialIdx ? { ...t, status } : t));
        return { ...g, trials };
      });
      return { ...w, devices };
    });
  }, []);

  const parseCsv = useCallback(async (file: File): Promise<CsvPreview> => {
    const text = await file.text();
    const lines = text.split(/\r?\n/).filter((l) => l.trim().length > 0);
    const headers = lines[0]?.split(',').map((h) => h.trim()) ?? [];
    const rows = lines.slice(1, 11).map((l) => l.split(',').map((c) => c.trim()));
    return { headers, rows, rowCount: Math.max(0, lines.length - 1) };
  }, []);

  const setCsvFile = useCallback(async (file: File | null) => {
    if (!file) {
      setWizard((w) => ({ ...w, csvFile: null, csvPreview: null }));
      return;
    }
    const uploaded: UploadedFile = {
      id: `f-${Date.now()}`,
      name: file.name,
      sizeBytes: file.size,
      type: file.type || 'text/csv',
    };
    const preview = await parseCsv(file);
    setWizard((w) => ({ ...w, csvFile: uploaded, csvPreview: preview }));
  }, [parseCsv]);

  const setVideoFile = useCallback((file: File | null) => {
    if (!file) {
      setWizard((w) => ({ ...w, videoFile: null }));
      return;
    }
    const uploaded: UploadedFile = {
      id: `f-${Date.now()}`,
      name: file.name,
      sizeBytes: file.size,
      type: file.type || 'video/mp4',
      previewUrl: URL.createObjectURL(file),
    };
    setWizard((w) => ({ ...w, videoFile: uploaded }));
  }, []);

  const manifest = useMemo(() => buildManifest(wizard), [wizard]);
  const idempotencyKey = useMemo(() => buildIdempotencyKey(wizard), [wizard]);

  const tick = useCallback((current: SimulationResult, failureMode: FailureMode) => {
    const next = advanceResult(current, failureMode);
    setResult(next);

    const done = next.completedAt !== null;
    if (done) {
      runningRef.current = false;
      setIsRunning(false);
      setHistory((prev) => [next, ...prev].slice(0, 12));
      if (next.currentStatus === 'PUBLISHED') {
        // Build and register mock session so it appears in Sessions / athlete timeline
        const session = buildMockSession(wizard, next);
        appendSession(session);
      }
      return;
    }

    const active = next.stages.find((s) => s.result === 'running');
    const delay = active ? stageDelay(active.stage.key) : 500;
    timerRef.current = setTimeout(() => tick(next, failureMode), delay);
  }, [wizard]);

  const submit = useCallback(() => {
    clearTimer();
    const duplicate = findByIdempotencyKey(idempotencyKey) !== undefined;
    const initial = createInitialResult(wizard, duplicate);
    setResult(initial);
    setStep(4);
    setIsRunning(true);
    runningRef.current = true;
    const first = advanceResult(initial, wizard.failureMode);
    setResult(first);

    if (first.completedAt !== null) {
      setIsRunning(false);
      runningRef.current = false;
      setHistory((prev) => [first, ...prev].slice(0, 12));
      if (first.currentStatus === 'PUBLISHED') {
        appendSession(buildMockSession(wizard, first));
      }
      return;
    }

    const active = first.stages.find((s) => s.result === 'running');
    const delay = active ? stageDelay(active.stage.key) : 500;
    timerRef.current = setTimeout(() => tick(first, wizard.failureMode), delay);
  }, [clearTimer, idempotencyKey, wizard, tick]);

  const reset = useCallback(() => {
    clearTimer();
    runningRef.current = false;
    setIsRunning(false);
    setResult(null);
    setStep(0);
    setWizard(initialWizard());
  }, [clearTimer]);

  const clearDemoData = useCallback(() => {
    clearGeneratedSessions();
  }, []);

  const next = useCallback(() => setStep((s) => Math.min(4, s + 1)), []);
  const back = useCallback(() => setStep((s) => Math.max(0, s - 1)), []);
  const goTo = useCallback((s: number) => setStep(s), []);

  return {
    step,
    wizard,
    update,
    regenerateExternalSessionId,
    addDevice,
    removeDevice,
    addTrial,
    removeTrial,
    updateTrialStatus,
    setCsvFile,
    setVideoFile,
    manifest,
    idempotencyKey,
    result,
    isRunning,
    history,
    submit,
    reset,
    next,
    back,
    goTo,
    clearDemoData,
    generatedCount: generatedSessionCount(),
  };
}