import type { WorkflowDefinition, WorkflowStep } from './types';

interface WalkthroughProps {
  workflow: WorkflowDefinition;
  currentIndex: number;
  onIndexChange: (index: number) => void;
  mode: 'happy' | 'recovery';
  onModeChange: (mode: 'happy' | 'recovery') => void;
  locale: 'en' | 'zh-CN';
  onOpenRoute?: (route: string) => void;
}

export function WorkflowWalkthrough({
  workflow,
  currentIndex,
  onIndexChange,
  mode,
  onModeChange,
  locale,
  onOpenRoute,
}: WalkthroughProps) {
  const L = locale === 'zh-CN' ? 'zhCN' : 'en';
  const path = mode === 'recovery' && workflow.recoveryBranch ? workflow.recoveryBranch : workflow.happyPath;
  const stepId = path[currentIndex];
  const step: WorkflowStep | undefined = workflow.steps.find((s) => s.id === stepId);

  const goPrev = () => onIndexChange(Math.max(0, currentIndex - 1));
  const goNext = () => onIndexChange(Math.min(path.length - 1, currentIndex + 1));

  return (
    <div className="rounded-lg border border-default bg-surface-panel p-4">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-semibold text-ink-900">
          {locale === 'zh-CN' ? '引导演示' : 'Guided walkthrough'}
        </h3>
        <div className="flex gap-1">
          <button
            type="button"
            onClick={() => { onModeChange('happy'); onIndexChange(0); }}
            className={`rounded-md px-2.5 py-1 text-xs font-medium ${mode === 'happy' ? 'bg-accent text-white' : 'bg-ink-100 text-ink-600 hover:bg-ink-200'}`}
          >
            {locale === 'zh-CN' ? '主流程' : 'Happy path'}
          </button>
          {workflow.recoveryBranch ? (
            <button
              type="button"
              onClick={() => { onModeChange('recovery'); onIndexChange(0); }}
              className={`rounded-md px-2.5 py-1 text-xs font-medium ${mode === 'recovery' ? 'bg-accent text-white' : 'bg-ink-100 text-ink-600 hover:bg-ink-200'}`}
            >
              {locale === 'zh-CN' ? '恢复分支' : 'Recovery'}
            </button>
          ) : null}
        </div>
      </div>

      <div className="mb-3 flex items-center gap-2">
        {path.map((_, i) => (
          <button
            key={i}
            type="button"
            onClick={() => onIndexChange(i)}
            aria-label={`Step ${i + 1}`}
            className={`h-2 flex-1 rounded-full transition-colors ${i <= currentIndex ? 'bg-accent' : 'bg-ink-200'}`}
          />
        ))}
      </div>

      {step ? (
        <div className="space-y-3">
          <div className="flex items-baseline gap-2">
            <span className="text-xs font-medium text-ink-400">
              {currentIndex + 1} / {path.length}
            </span>
            <h4 className="text-sm font-semibold text-ink-900">{step.title[L]}</h4>
          </div>
          <p className="text-sm leading-relaxed text-ink-600">{step.description[L]}</p>

          {step.demoRoute ? (
            <button
              type="button"
              onClick={() => onOpenRoute?.(step.demoRoute!)}
              className="inline-flex items-center gap-1.5 rounded-md border border-accent bg-accent-soft px-3 py-1.5 text-sm font-medium text-accent hover:bg-accent"
            >
              {locale === 'zh-CN' ? '打开原型页面' : 'Open prototype screen'}
            </button>
          ) : (
            <p className="text-xs italic text-ink-400">
              {locale === 'zh-CN' ? '此步骤尚无已验证的原型页面。' : 'No verified prototype screen exists for this step.'}
            </p>
          )}

          <div className="flex items-center justify-between pt-2">
            <button
              type="button"
              onClick={goPrev}
              disabled={currentIndex === 0}
              className="rounded-md border border-default bg-surface-panel px-3 py-1.5 text-sm font-medium text-ink-600 hover:bg-surface-hover disabled:cursor-not-allowed disabled:opacity-40"
            >
              {locale === 'zh-CN' ? '上一步' : 'Previous'}
            </button>
            <button
              type="button"
              onClick={goNext}
              disabled={currentIndex >= path.length - 1}
              className="rounded-md bg-accent px-3 py-1.5 text-sm font-medium text-white hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-40"
            >
              {locale === 'zh-CN' ? '下一步' : 'Next'}
            </button>
          </div>
        </div>
      ) : null}
    </div>
  );
}