import { CheckCircle2, Circle, Loader2, XCircle, MinusCircle } from 'lucide-react';
import type { Lang } from '@/lib/types';
import type { PipelineStageState, StageResult, UploadJob } from './types';

interface PipelineVisualizationProps {
  lang: Lang;
  job: UploadJob | null;
}

const RESULT_STYLE: Record<StageResult, { icon: typeof Circle; color: string; label: string; labelCn: string }> = {
  pending: { icon: Circle, color: 'text-content-muted', label: 'Pending', labelCn: '待处理' },
  running: { icon: Loader2, color: 'text-accent-primary animate-spin', label: 'Running', labelCn: '进行中' },
  success: { icon: CheckCircle2, color: 'text-status-success', label: 'Success', labelCn: '成功' },
  failed: { icon: XCircle, color: 'text-status-danger', label: 'Failed', labelCn: '失败' },
  skipped: { icon: MinusCircle, color: 'text-content-muted', label: 'Skipped', labelCn: '已跳过' },
};

export function PipelineVisualization({ lang, job }: PipelineVisualizationProps) {
  const cn = lang === 'cn';

  if (!job) {
    return (
      <div className="flex h-48 items-center justify-center rounded-lg border border-dashed border-border-default text-sm text-content-secondary">
        {cn ? '配置左侧参数并启动模拟上传以查看管线状态' : 'Configure parameters and start a simulated upload to see the pipeline'}
      </div>
    );
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <div className="text-sm font-medium text-content-primary">
          {cn ? '上传管线' : 'Upload pipeline'}
        </div>
        <StatusBadge status={job.currentStatus} cn={cn} />
      </div>

      <ol className="relative space-y-0">
        {job.stages.map((stageState, idx) => (
          <StageRow
            key={stageState.stage.status}
            stageState={stageState}
            isLast={idx === job.stages.length - 1}
            cn={cn}
          />
        ))}
      </ol>

      {job.errorMessage && (
        <div className="rounded-md border border-status-danger/30 bg-status-danger/5 px-3 py-2 text-xs text-status-danger">
          {job.errorMessage}
        </div>
      )}
    </div>
  );
}

function StageRow({
  stageState,
  isLast,
  cn,
}: {
  stageState: PipelineStageState;
  isLast: boolean;
  cn: boolean;
}) {
  const { stage, result } = stageState;
  const style = RESULT_STYLE[result];
  const Icon = style.icon;

  return (
    <li className="relative flex gap-3 pb-4 last:pb-0">
      {!isLast && (
        <span
          className={`absolute left-[11px] top-6 h-[calc(100%-1rem)] w-px ${
            result === 'success' ? 'bg-status-success/40' : 'bg-border-default'
          }`}
        />
      )}
      <div className="relative z-10 flex h-6 w-6 shrink-0 items-center justify-center">
        <Icon className={`h-5 w-5 ${style.color}`} />
      </div>
      <div className="min-w-0 flex-1 pt-0.5">
        <div className="flex items-center justify-between gap-2">
          <span className="text-sm font-medium text-content-primary">
            {cn ? stage.labelCn : stage.label}
          </span>
          <span className={`text-xs font-medium ${style.color}`}>
            {cn ? style.labelCn : style.label}
          </span>
        </div>
        <p className="mt-0.5 text-xs text-content-secondary">
          {cn ? stage.descriptionCn : stage.description}
        </p>
        {stageState.detail && result === 'failed' && (
          <p className="mt-1 text-xs text-status-danger">{stageState.detail}</p>
        )}
      </div>
    </li>
  );
}

function StatusBadge({ status, cn }: { status: string; cn: boolean }) {
  const isError = status.endsWith('_FAILED');
  const isDone = status === 'PUBLISHED';
  const label = cn ? statusCn(status) : status;
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
        isError
          ? 'bg-status-danger/10 text-status-danger'
          : isDone
            ? 'bg-status-success/10 text-status-success'
            : 'bg-accent-soft text-accent-primary'
      }`}
    >
      {label}
    </span>
  );
}

function statusCn(status: string): string {
  const map: Record<string, string> = {
    LOCAL: '本地',
    QUEUED: '队列中',
    UPLOADING: '上传中',
    RECEIVED: '已接收',
    VALIDATING: '校验中',
    PROCESSING: '处理中',
    ANALYZED: '已分析',
    PUBLISHED: '已发布',
    UPLOAD_FAILED: '上传失败',
    VALIDATION_FAILED: '校验失败',
    PROCESSING_FAILED: '处理失败',
  };
  return map[status] ?? status;
}