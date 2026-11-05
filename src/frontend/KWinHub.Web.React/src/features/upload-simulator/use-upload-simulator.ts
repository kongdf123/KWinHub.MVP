import { useCallback, useEffect, useRef, useState } from 'react';
import { advanceJob, createJob, stageDelayFor } from './engine';
import type { UploadConfig, UploadJob } from './types';

export interface UseUploadSimulatorReturn {
  job: UploadJob | null;
  isRunning: boolean;
  start: (config: UploadConfig) => void;
  reset: () => void;
  history: UploadJob[];
}

export function useUploadSimulator(): UseUploadSimulatorReturn {
  const [job, setJob] = useState<UploadJob | null>(null);
  const [isRunning, setIsRunning] = useState(false);
  const [history, setHistory] = useState<UploadJob[]>([]);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const runningRef = useRef(false);

  const clearTimer = useCallback(() => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }
  }, []);

  useEffect(() => () => clearTimer(), [clearTimer]);

  const tick = useCallback((current: UploadJob) => {
    const { job: next, done } = advanceJob(current);
    setJob(next);

    if (done) {
      runningRef.current = false;
      setIsRunning(false);
      setHistory((prev) => [next, ...prev].slice(0, 12));
      return;
    }

    const activeStage = next.stages.find((s) => s.result === 'running');
    const delay = activeStage ? stageDelayFor(activeStage.stage.status) : 500;
    timerRef.current = setTimeout(() => tick(next), delay);
  }, []);

  const start = useCallback(
    (config: UploadConfig) => {
      clearTimer();
      const fresh = createJob(config);
      setJob(fresh);
      setIsRunning(true);
      runningRef.current = true;
      const first = advanceJob(fresh);
      setJob(first.job);
      if (first.done) {
        setIsRunning(false);
        runningRef.current = false;
        setHistory((prev) => [first.job, ...prev].slice(0, 12));
        return;
      }
      const activeStage = first.job.stages.find((s) => s.result === 'running');
      const delay = activeStage ? stageDelayFor(activeStage.stage.status) : 500;
      timerRef.current = setTimeout(() => tick(first.job), delay);
    },
    [clearTimer, tick],
  );

  const reset = useCallback(() => {
    clearTimer();
    runningRef.current = false;
    setIsRunning(false);
    setJob(null);
  }, [clearTimer]);

  return { job, isRunning, start, reset, history };
}