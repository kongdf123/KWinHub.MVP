import { RotateCcw, Play, RefreshCw, Calculator, FlaskConical, AlertTriangle } from 'lucide-react';
import { useScientificDemo } from '../hooks/useScientificDemo';
import { LIFECYCLE_STAGES, LIFECYCLE_STAGE_LABELS, type LifecycleStage } from '../services/scientific-demo-store';
import { SCENARIOS } from '@/lib/scientific/scenarios';

const STATUS_STYLES: Record<string, string> = {
  local: 'bg-ink-100 text-ink-700 border-ink-200',
  uploaded: 'bg-teal-50 text-teal-700 border-teal-200',
  upload_failed: 'bg-invalid-50 text-invalid-700 border-invalid-200',
  validating: 'bg-teal-50 text-teal-700 border-teal-200',
  succeeded: 'bg-teal-50 text-teal-700 border-teal-200',
  failed: 'bg-invalid-50 text-invalid-700 border-invalid-200',
  published: 'bg-success-50 text-success-700 border-success-200',
};

function stageIndex(stage: LifecycleStage): number {
  return LIFECYCLE_STAGES.indexOf(stage);
}

export function DemoScenarioControls() {
  const {
    scenario,
    activeScenarioId,
    projection,
    selectScenario,
    reset,
    advanceTrial,
    retryUpload,
    requestRecalculation,
    apiMode,
  } = useScientificDemo();

  if (apiMode) {
    return (
      <div className="bg-white rounded-xl border border-ink-200 p-5">
        <div className="flex items-center gap-2 mb-3">
          <FlaskConical className="w-4 h-4 text-ink-400" />
          <h3 className="text-sm font-semibold text-ink-900">Scientific Demo Controls</h3>
        </div>
        <div className="flex items-start gap-2 p-3 bg-ink-50 border border-ink-200 rounded-lg">
          <AlertTriangle className="w-4 h-4 text-ink-500 shrink-0 mt-0.5" />
          <p className="text-xs text-ink-600 leading-relaxed">
            Scenario controls are not available in API mode. They drive deterministic demo fixtures only and must not simulate successful operations against a live backend.
          </p>
        </div>
      </div>
    );
  }

  if (!scenario) return null;

  return (
    <div className="bg-white rounded-xl border border-ink-200 p-5">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <FlaskConical className="w-4 h-4 text-teal-600" />
          <h3 className="text-sm font-semibold text-ink-900">Scientific Demo Controls</h3>
          <span className="text-[10px] font-semibold uppercase tracking-wide px-1.5 py-0.5 rounded bg-teal-100 text-teal-700 border border-teal-200">
            Demo
          </span>
        </div>
        <button
          onClick={reset}
          className="flex items-center gap-1.5 text-xs font-medium text-ink-600 hover:text-ink-900 px-2.5 py-1.5 rounded-lg hover:bg-ink-50 transition-colors"
          title="Reset scenario to initial deterministic state"
        >
          <RotateCcw className="w-3.5 h-3.5" />
          Reset
        </button>
      </div>

      <div className="mb-4">
        <label className="block text-xs font-medium text-ink-500 mb-1.5">Featured scenario</label>
        <select
          value={activeScenarioId}
          onChange={(e) => selectScenario(e.target.value)}
          className="w-full text-sm bg-white border border-ink-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-teal-400"
        >
          {SCENARIOS.map((s) => (
            <option key={s.id} value={s.id}>
              {s.label}
            </option>
          ))}
        </select>
        <p className="mt-1.5 text-xs text-ink-500 leading-relaxed">{scenario.description}</p>
      </div>

      <div className="space-y-3">
        {projection.trials.map((tp) => {
          const currentIdx = stageIndex(tp.stage);
          return (
            <div key={tp.trialNumber} className="border border-ink-100 rounded-lg p-3">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center gap-2">
                  <span className="text-xs font-mono font-medium text-ink-400">T{tp.trialNumber}</span>
                  <span className="text-sm font-medium text-ink-900">{tp.trial.input.channels[0]?.side ? `${tp.trial.input.channels[0].side} channel` : `Trial ${tp.trialNumber}`}</span>
                </div>
                <span className={`text-[11px] font-medium px-2 py-0.5 rounded-full border ${STATUS_STYLES[tp.status] ?? STATUS_STYLES.local}`}>
                  {tp.status}
                </span>
              </div>

              <div className="flex items-center gap-1 mb-3">
                {LIFECYCLE_STAGES.map((stage, idx) => {
                  const isActive = idx === currentIdx;
                  const isPast = idx < currentIdx;
                  const isFailureHere = tp.failed && isActive;
                  return (
                    <div key={stage} className="flex-1 flex flex-col items-center gap-1">
                      <div
                        className={`w-full h-1.5 rounded-full ${
                          isFailureHere
                            ? 'bg-invalid-500'
                            : isActive
                            ? 'bg-teal-500'
                            : isPast
                            ? 'bg-success-400'
                            : 'bg-ink-200'
                        }`}
                      />
                      <span className={`text-[10px] ${isActive ? 'text-ink-700 font-medium' : 'text-ink-400'}`}>
                        {LIFECYCLE_STAGE_LABELS[stage]}
                      </span>
                    </div>
                  );
                })}
              </div>

              {tp.failed && (
                <div className="mb-2 p-2 bg-invalid-50 border border-invalid-100 rounded text-xs text-invalid-700">
                  <span className="font-mono">{tp.failureCode}</span>
                  {tp.failureMessage && <span className="ml-1.5">— {tp.failureMessage}</span>}
                </div>
              )}

              <div className="flex flex-wrap items-center gap-2">
                <button
                  onClick={() => advanceTrial(tp.trialNumber)}
                  disabled={!tp.canAdvance}
                  className="flex items-center gap-1.5 text-xs font-medium text-white bg-teal-600 hover:bg-teal-700 disabled:bg-ink-200 disabled:text-ink-400 disabled:cursor-not-allowed px-2.5 py-1.5 rounded-lg transition-colors"
                >
                  <Play className="w-3 h-3" />
                  Advance
                </button>
                {tp.canRetryUpload && (
                  <button
                    onClick={() => retryUpload(tp.trialNumber)}
                    className="flex items-center gap-1.5 text-xs font-medium text-white bg-attention-600 hover:bg-attention-700 px-2.5 py-1.5 rounded-lg transition-colors"
                  >
                    <RefreshCw className="w-3 h-3" />
                    Retry upload
                  </button>
                )}
                {tp.canRequestRecalculation && (
                  <button
                    onClick={() => requestRecalculation(tp.trialNumber)}
                    className="flex items-center gap-1.5 text-xs font-medium text-white bg-ink-700 hover:bg-ink-800 px-2.5 py-1.5 rounded-lg transition-colors"
                  >
                    <Calculator className="w-3 h-3" />
                    Request recalculation
                  </button>
                )}
                {tp.runtime.recalculationRequested && (
                  <span className="text-[11px] font-mono text-ink-500 bg-ink-100 px-2 py-1 rounded">
                    candidate revision {tp.runtime.recalculationRevisionNumber}
                  </span>
                )}
              </div>
            </div>
          );
        })}
      </div>

      <div className="mt-4 pt-3 border-t border-ink-100">
        <p className="text-[11px] text-ink-400 leading-relaxed">
          Deterministic demo state. Reset reproduces identical output. Advance/retry/recalculate do not call a backend.
        </p>
      </div>
    </div>
  );
}