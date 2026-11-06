import { useEffect, useRef } from 'react';
import { Info, CheckCircle2, AlertTriangle, XCircle } from 'lucide-react';
import type { Lang } from '@/lib/types';
import type { LogEntry, UploadJob } from './types';

interface UploadLogPanelProps {
  lang: Lang;
  job: UploadJob | null;
}

const LEVEL_ICON = {
  info: Info,
  success: CheckCircle2,
  warning: AlertTriangle,
  error: XCircle,
} as const;

const LEVEL_COLOR = {
  info: 'text-content-secondary',
  success: 'text-status-success',
  warning: 'text-status-warning',
  error: 'text-status-danger',
} as const;

export function UploadLogPanel({ lang, job }: UploadLogPanelProps) {
  const cn = lang === 'cn';
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [job?.log.length]);

  const log: LogEntry[] = job?.log ?? [];

  return (
    <div className="flex h-full flex-col">
      <div className="mb-2 flex items-center justify-between">
        <h3 className="text-sm font-medium text-content-primary">
          {cn ? '事件日志' : 'Event log'}
        </h3>
        {job && (
          <span className="text-xs text-content-secondary">
            {cn ? '幂等键: ' : 'Key: '}
            <code className="rounded bg-surface-hover px-1.5 py-0.5 text-[11px] text-content-primary">
              {job.idempotencyKey}
            </code>
          </span>
        )}
      </div>
      <div
        ref={scrollRef}
        className="flex-1 overflow-y-auto rounded-md border border-border-default bg-surface-hover/30 p-3 font-mono text-xs"
      >
        {log.length === 0 ? (
          <p className="text-content-secondary">
            {cn ? '等待模拟启动…' : 'Awaiting simulation…'}
          </p>
        ) : (
          <ul className="space-y-1.5">
            {log.map((entry) => {
              const Icon = LEVEL_ICON[entry.level];
              return (
                <li key={entry.id} className="flex items-start gap-2">
                  <Icon className={`mt-0.5 h-3.5 w-3.5 shrink-0 ${LEVEL_COLOR[entry.level]}`} />
                  <div className="min-w-0 flex-1">
                    <div className="flex items-baseline gap-2">
                      <span className="shrink-0 text-[10px] text-content-muted">
                        {formatTime(entry.timestamp)}
                      </span>
                      <span className="shrink-0 rounded bg-surface-panel px-1 text-[10px] uppercase tracking-wide text-content-secondary">
                        {entry.status}
                      </span>
                    </div>
                    <p className="text-content-primary">
                      {cn ? entry.messageCn : entry.message}
                    </p>
                  </div>
                </li>
              );
            })}
          </ul>
        )}
      </div>
    </div>
  );
}

function formatTime(iso: string): string {
  try {
    const d = new Date(iso);
    return d.toLocaleTimeString('en-GB', { hour12: false }) + '.' + String(d.getMilliseconds()).padStart(3, '0');
  } catch {
    return iso;
  }
}