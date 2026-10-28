import { useMemo, useState } from 'react';
import { useApp } from '@/lib/app-context';
import { TopBar } from '@/components/layout/TopBar';
import { ORG_WORKFLOWS } from '@/features/workflows/workflow-definitions';
import { WorkflowDiagram } from '@/features/workflows/WorkflowDiagram';
import { WorkflowDetails } from '@/features/workflows/WorkflowDetails';
import { WorkflowWalkthrough } from '@/features/workflows/WorkflowWalkthrough';
import {
  DECISION_STATUS_LABELS,
  DECISION_STATUS_STYLES,
  IMPLEMENTATION_STATUS_LABELS,
  IMPLEMENTATION_STATUS_STYLES,
  DEMO_BEHAVIOR_LABELS,
  DEMO_BEHAVIOR_STYLES,
} from '@/features/workflows/status-helpers';

export function WorkflowExplorerPage() {
  const { navigate, lang } = useApp();
  const locale: 'en' | 'zh-CN' = lang === 'cn' ? 'zh-CN' : 'en';
  const L = locale === 'zh-CN' ? 'zhCN' : 'en';
  const [workflowId, setWorkflowId] = useState(ORG_WORKFLOWS[0].id);
  const [selectedStepId, setSelectedStepId] = useState<string | null>(null);
  const [walkIndex, setWalkIndex] = useState(0);
  const [walkMode, setWalkMode] = useState<'happy' | 'recovery'>('happy');

  const workflow = useMemo(
    () => ORG_WORKFLOWS.find((w) => w.id === workflowId) ?? ORG_WORKFLOWS[0],
    [workflowId],
  );

  const activePath = walkMode === 'recovery' && workflow.recoveryBranch ? workflow.recoveryBranch : workflow.happyPath;
  const selectedStep = workflow.steps.find((s) => s.id === selectedStepId) ?? null;

  const handleWorkflowChange = (id: string) => {
    setWorkflowId(id);
    setSelectedStepId(null);
    setWalkIndex(0);
    setWalkMode('happy');
  };

  const handleWalkthroughIndex = (index: number) => {
    setWalkIndex(index);
    const path = walkMode === 'recovery' && workflow.recoveryBranch ? workflow.recoveryBranch : workflow.happyPath;
    if (path[index]) setSelectedStepId(path[index]);
  };

  const handleSelectStep = (stepId: string) => {
    setSelectedStepId(stepId);
    const path = walkMode === 'recovery' && workflow.recoveryBranch ? workflow.recoveryBranch : workflow.happyPath;
    const idx = path.indexOf(stepId);
    if (idx >= 0) setWalkIndex(idx);
  };

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <h2 className="text-xl font-semibold text-content-primary">
          {locale === 'zh-CN' ? '工作流探索器' : 'Workflow Explorer'}
        </h2>
        <p className="mt-1 text-sm text-content-secondary">
          {locale === 'zh-CN' ? '探索产品工作流、用户故事和原型状态' : 'Explore product workflows, user stories, and prototype status'}
        </p>
      </div>

      <div className="px-6 py-6 space-y-4">
        <div className="rounded-lg border border-default bg-surface-panel p-4">
          <div className="flex flex-wrap gap-2">
            {ORG_WORKFLOWS.map((w) => (
              <button
                key={w.id}
                type="button"
                onClick={() => handleWorkflowChange(w.id)}
                className={`rounded-md border px-3 py-1.5 text-sm font-medium transition-colors ${
                  w.id === workflowId
                    ? 'border-accent bg-accent text-white'
                    : 'border-default bg-surface-panel text-ink-700 hover:bg-surface-hover'
                }`}
              >
                <span className="mr-1.5 text-xs opacity-70">{w.id}</span>
                {w.name[L]}
              </button>
            ))}
          </div>
        </div>

        <div className="grid gap-4 lg:grid-cols-3">
          <div className="rounded-lg border border-default bg-surface-panel p-4 lg:col-span-2">
            <WorkflowDiagram
              workflow={workflow}
              selectedStepId={selectedStepId}
              activePath={activePath}
              onSelect={handleSelectStep}
              locale={locale}
            />
            <details className="mt-3 text-xs text-ink-500">
              <summary className="cursor-pointer font-medium text-ink-600">
                {locale === 'zh-CN' ? '键盘可访问的步骤列表' : 'Keyboard-accessible step list'}
              </summary>
              <ol className="mt-2 space-y-1">
                {workflow.steps.map((step, i) => (
                  <li key={step.id}>
                    <button
                      type="button"
                      onClick={() => handleSelectStep(step.id)}
                      className={`w-full rounded px-2 py-1 text-left ${step.id === selectedStepId ? 'bg-accent-soft text-accent' : 'hover:bg-surface-hover'}`}
                    >
                      <span className="mr-2 text-ink-400">{i + 1}.</span>
                      {step.title[L]}
                    </button>
                  </li>
                ))}
              </ol>
            </details>
          </div>

          <WorkflowDetails step={selectedStep} locale={locale} onOpenRoute={(r) => navigate(r)} />
        </div>

        <div className="grid gap-4 lg:grid-cols-2">
          <WorkflowWalkthrough
            workflow={workflow}
            currentIndex={walkIndex}
            onIndexChange={handleWalkthroughIndex}
            mode={walkMode}
            onModeChange={(m) => { setWalkMode(m); setWalkIndex(0); }}
            locale={locale}
            onOpenRoute={(r) => navigate(r)}
          />

          <div className="rounded-lg border border-default bg-surface-panel p-4">
            <h3 className="mb-3 text-sm font-semibold text-ink-900">
              {locale === 'zh-CN' ? '状态图例' : 'Status legend'}
            </h3>
            <div className="space-y-3 text-xs">
              <div>
                <p className="mb-1.5 font-semibold uppercase tracking-wide text-ink-500">
                  {locale === 'zh-CN' ? '需求决策状态' : 'Requirement decision status'}
                </p>
                <div className="flex flex-wrap gap-1.5">
                  {(Object.keys(DECISION_STATUS_LABELS) as Array<keyof typeof DECISION_STATUS_LABELS>).map((s) => (
                    <span key={s} className={`inline-flex items-center rounded-full border px-2 py-0.5 font-medium ${DECISION_STATUS_STYLES[s]}`}>
                      {DECISION_STATUS_LABELS[s][L]}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <p className="mb-1.5 font-semibold uppercase tracking-wide text-ink-500">
                  {locale === 'zh-CN' ? '实现/验证状态' : 'Implementation / verification status'}
                </p>
                <div className="flex flex-wrap gap-1.5">
                  {(Object.keys(IMPLEMENTATION_STATUS_LABELS) as Array<keyof typeof IMPLEMENTATION_STATUS_LABELS>).map((s) => (
                    <span key={s} className={`inline-flex items-center rounded-full border px-2 py-0.5 font-medium ${IMPLEMENTATION_STATUS_STYLES[s]}`}>
                      {IMPLEMENTATION_STATUS_LABELS[s][L]}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <p className="mb-1.5 font-semibold uppercase tracking-wide text-ink-500">
                  {locale === 'zh-CN' ? '模拟行为' : 'Simulation behavior'}
                </p>
                <div className="flex flex-wrap gap-1.5">
                  {(Object.keys(DEMO_BEHAVIOR_LABELS) as Array<keyof typeof DEMO_BEHAVIOR_LABELS>).map((s) => (
                    <span key={s} className={`inline-flex items-center rounded-full border px-2 py-0.5 font-medium ${DEMO_BEHAVIOR_STYLES[s]}`}>
                      {DEMO_BEHAVIOR_LABELS[s][L]}
                    </span>
                  ))}
                </div>
              </div>
            </div>
            <p className="mt-3 text-xs italic text-ink-400">
              {locale === 'zh-CN'
                ? '注意：模拟状态独立于实现状态。模拟数据绝不表示已验证的生产能力。'
                : 'Note: Simulation status is independent of implementation status. Mock behavior never implies a verified production capability.'}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}