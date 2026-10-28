import type { WorkflowStep } from './types';
import {
  DECISION_STATUS_LABELS,
  DECISION_STATUS_STYLES,
  IMPLEMENTATION_STATUS_LABELS,
  IMPLEMENTATION_STATUS_STYLES,
  DEMO_BEHAVIOR_LABELS,
  DEMO_BEHAVIOR_STYLES,
} from './status-helpers';

interface DetailsProps {
  step: WorkflowStep | null;
  locale: 'en' | 'zh-CN';
  onOpenRoute?: (route: string) => void;
}

function Badge({ label, style }: { label: string; style: string }) {
  return (
    <span className={`inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium ${style}`}>
      {label}
    </span>
  );
}

export function WorkflowDetails({ step, locale, onOpenRoute }: DetailsProps) {
  const L = locale === 'zh-CN' ? 'zhCN' : 'en';

  if (!step) {
    return (
      <div className="flex h-full min-h-[200px] flex-col items-center justify-center rounded-lg border border-dashed border-default bg-surface-panel p-6 text-center">
        <p className="text-sm text-ink-500">
          {locale === 'zh-CN' ? '选择一个节点以查看步骤详情' : 'Select a node to view step details'}
        </p>
      </div>
    );
  }

  const reqLabel = DECISION_STATUS_LABELS[step.requirementStatus][L];
  const implLabel = IMPLEMENTATION_STATUS_LABELS[step.implementationStatus][L];
  const demoLabel = DEMO_BEHAVIOR_LABELS[step.demoBehavior][L];

  return (
    <div className="flex h-full flex-col gap-4 rounded-lg border border-default bg-surface-panel p-4">
      <div>
        <h3 className="text-base font-semibold text-ink-900">{step.title[L]}</h3>
        <p className="mt-1 text-xs text-ink-500">
          {locale === 'zh-CN' ? '执行者：' : 'Actor: '}
          {step.actor[L]}
        </p>
      </div>

      <div className="flex flex-wrap gap-1.5">
        <Badge label={reqLabel} style={DECISION_STATUS_STYLES[step.requirementStatus]} />
        <Badge label={implLabel} style={IMPLEMENTATION_STATUS_STYLES[step.implementationStatus]} />
        <Badge label={demoLabel} style={DEMO_BEHAVIOR_STYLES[step.demoBehavior]} />
      </div>

      <p className="text-sm leading-relaxed text-ink-700">{step.description[L]}</p>

      {step.preconditions && step.preconditions.length > 0 ? (
        <div>
          <h4 className="mb-1 text-xs font-semibold uppercase tracking-wide text-ink-500">
            {locale === 'zh-CN' ? '前置条件' : 'Preconditions'}
          </h4>
          <ul className="list-inside list-disc space-y-0.5 text-sm text-ink-600">
            {step.preconditions.map((p, i) => (
              <li key={i}>{p[L]}</li>
            ))}
          </ul>
        </div>
      ) : null}

      {step.expectedOutcome ? (
        <div>
          <h4 className="mb-1 text-xs font-semibold uppercase tracking-wide text-ink-500">
            {locale === 'zh-CN' ? '预期结果' : 'Expected outcome'}
          </h4>
          <p className="text-sm text-ink-600">{step.expectedOutcome[L]}</p>
        </div>
      ) : null}

      {step.alternateFlows && step.alternateFlows.length > 0 ? (
        <div>
          <h4 className="mb-1 text-xs font-semibold uppercase tracking-wide text-ink-500">
            {locale === 'zh-CN' ? '替代流程' : 'Alternate flows'}
          </h4>
          <ul className="list-inside list-disc space-y-0.5 text-sm text-ink-600">
            {step.alternateFlows.map((a, i) => (
              <li key={i}>{a[L]}</li>
            ))}
          </ul>
        </div>
      ) : null}

      {step.acceptanceCriteria && step.acceptanceCriteria.length > 0 ? (
        <div>
          <h4 className="mb-1 text-xs font-semibold uppercase tracking-wide text-ink-500">
            {locale === 'zh-CN' ? '验收标准' : 'Acceptance criteria'}
          </h4>
          <ul className="list-inside list-disc space-y-0.5 text-sm text-ink-600">
            {step.acceptanceCriteria.map((a, i) => (
              <li key={i}>{a[L]}</li>
            ))}
          </ul>
        </div>
      ) : null}

      {step.demoRoute ? (
        <button
          type="button"
          onClick={() => onOpenRoute?.(step.demoRoute!)}
          className="mt-auto inline-flex w-fit items-center gap-1.5 rounded-md bg-accent px-3 py-1.5 text-sm font-medium text-white hover:bg-accent-hover"
        >
          {locale === 'zh-CN' ? '打开原型页面' : 'Open prototype screen'}
        </button>
      ) : (
        <p className="mt-auto text-xs italic text-ink-400">
          {locale === 'zh-CN' ? '此步骤尚无已验证的原型页面。' : 'No verified prototype screen exists for this step.'}
        </p>
      )}

      <div className="border-t border-default pt-2">
        <h4 className="mb-1 text-xs font-semibold uppercase tracking-wide text-ink-500">
          {locale === 'zh-CN' ? '规范引用' : 'Specification references'}
        </h4>
        <ul className="space-y-0.5 text-xs text-ink-500">
          {step.specificationRefs.map((ref, i) => (
            <li key={i}>
              <code className="rounded bg-ink-100 px-1 py-0.5">{ref.documentPath}</code>
              {ref.section ? ` · ${ref.section}` : ''}
              {ref.requirementId ? ` · ${ref.requirementId}` : ''}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}