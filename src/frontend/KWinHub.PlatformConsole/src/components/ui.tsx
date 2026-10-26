import type {
  ButtonHTMLAttributes,
  InputHTMLAttributes,
  ReactNode,
  SelectHTMLAttributes,
  TextareaHTMLAttributes,
} from 'react';
import { AlertTriangle, Inbox, Loader2, X } from 'lucide-react';
import { useI18n, type TranslationKey } from '../lib/i18n';

// --- Layout primitives -----------------------------------------------------

export function Card({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <section className={`rounded-xl border border-ink-200 bg-white shadow-sm ${className}`}>
      {children}
    </section>
  );
}

export type StatAccent = 'teal' | 'success' | 'warning' | 'invalid' | 'ink';

const STAT_ACCENTS: Record<StatAccent, string> = {
  teal: 'bg-teal-50 text-teal-700',
  success: 'bg-success-50 text-success-700',
  warning: 'bg-warning-50 text-warning-700',
  invalid: 'bg-invalid-50 text-invalid-700',
  ink: 'bg-ink-100 text-ink-600',
};

export function StatCard({
  label,
  value,
  icon,
  accent = 'ink',
}: {
  label: string;
  value: number | string;
  icon?: ReactNode;
  accent?: StatAccent;
}) {
  return (
    <Card className="p-5">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="text-xs font-medium uppercase tracking-wide text-ink-500">{label}</div>
          <div className="mt-2 text-3xl font-semibold text-ink-900">{value}</div>
        </div>
        {icon ? <div className={`shrink-0 rounded-lg p-2 ${STAT_ACCENTS[accent]}`}>{icon}</div> : null}
      </div>
    </Card>
  );
}

export function PageHeader({
  title,
  subtitle,
  actions,
}: {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
}) {
  return (
    <div className="mb-5 flex flex-wrap items-start justify-between gap-3">
      <div>
        <h1 className="text-xl font-semibold text-ink-900">{title}</h1>
        {subtitle ? <p className="mt-1 text-sm text-ink-500">{subtitle}</p> : null}
      </div>
      {actions ? <div className="flex flex-wrap items-center gap-2">{actions}</div> : null}
    </div>
  );
}

// --- Status ----------------------------------------------------------------

const STATUS_STYLES: Record<string, string> = {
  Active: 'border-success-200 bg-success-50 text-success-700',
  Suspended: 'border-warning-200 bg-warning-50 text-warning-700',
  Disabled: 'border-invalid-200 bg-invalid-50 text-invalid-700',
  Draft: 'border-ink-200 bg-ink-100 text-ink-600',
  Published: 'border-teal-200 bg-teal-50 text-teal-700',
  Archived: 'border-ink-200 bg-ink-100 text-ink-500',
  PendingEmailVerification: 'border-warning-200 bg-warning-50 text-warning-700',
  Individual: 'border-teal-200 bg-teal-50 text-teal-700',
  Standard: 'border-ink-200 bg-ink-100 text-ink-700',
};

const STATUS_KEYS: Record<string, TranslationKey> = {
  Active: 'status.active',
  Suspended: 'status.suspended',
  Disabled: 'status.disabled',
  Draft: 'status.draft',
  Published: 'status.published',
  Archived: 'status.archived',
  PendingEmailVerification: 'status.pendingEmailVerification',
  Individual: 'status.individual',
  Standard: 'status.standard',
};

export function StatusPill({ status }: { status: string }) {
  const { t } = useI18n();
  const key = STATUS_KEYS[status];
  const label = key ? t(key) : status;
  const style = STATUS_STYLES[status] ?? 'border-ink-200 bg-ink-100 text-ink-600';

  return (
    <span
      className={`inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium ${style}`}
    >
      {label}
    </span>
  );
}

// --- Form controls ---------------------------------------------------------

export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost';

const BUTTON_VARIANTS: Record<ButtonVariant, string> = {
  primary: 'bg-accent text-white hover:bg-accent-hover',
  secondary: 'border border-ink-300 bg-white text-ink-700 hover:bg-ink-50',
  danger: 'bg-invalid-600 text-white hover:bg-invalid-700',
  ghost: 'text-ink-600 hover:bg-ink-100 hover:text-ink-900',
};

export function Button({
  variant = 'primary',
  className = '',
  type = 'button',
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: ButtonVariant }) {
  return (
    <button
      type={type}
      className={`inline-flex items-center justify-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${BUTTON_VARIANTS[variant]} ${className}`}
      {...props}
    />
  );
}

const CONTROL_CLASS =
  'w-full rounded-md border border-ink-300 bg-white px-3 py-2 text-sm text-ink-900 shadow-sm placeholder:text-ink-400 focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent-soft disabled:bg-ink-50';

export function Input({ className = '', ...props }: InputHTMLAttributes<HTMLInputElement>) {
  return <input className={`${CONTROL_CLASS} ${className}`} {...props} />;
}

export function Select({
  className = '',
  children,
  ...props
}: SelectHTMLAttributes<HTMLSelectElement>) {
  return (
    <select className={`${CONTROL_CLASS} ${className}`} {...props}>
      {children}
    </select>
  );
}

export function Textarea({
  className = '',
  ...props
}: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea className={`${CONTROL_CLASS} min-h-[80px] ${className}`} {...props} />;
}

// --- Table -----------------------------------------------------------------

export function Table({ headers, children }: { headers: ReactNode[]; children: ReactNode }) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead className="bg-ink-50 text-xs uppercase tracking-wide text-ink-500">
          <tr>
            {headers.map((header, index) => (
              <th key={index} className="whitespace-nowrap px-4 py-3 font-semibold">
                {header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-ink-100">{children}</tbody>
      </table>
    </div>
  );
}

// --- Feedback --------------------------------------------------------------

export function EmptyState({ title, description }: { title: string; description?: string }) {
  return (
    <div className="flex flex-col items-center justify-center px-6 py-12 text-center">
      <div className="mb-3 rounded-full bg-ink-100 p-3 text-ink-400">
        <Inbox size={20} />
      </div>
      <p className="text-sm font-medium text-ink-700">{title}</p>
      {description ? <p className="mt-1 text-xs text-ink-500">{description}</p> : null}
    </div>
  );
}

export function Spinner({ className = '' }: { className?: string }) {
  return (
    <div className={`flex items-center justify-center py-12 ${className}`}>
      <Loader2 className="h-6 w-6 animate-spin text-accent" />
    </div>
  );
}

export function ErrorBanner({ message, onRetry }: { message: string; onRetry?: () => void }) {
  const { t } = useI18n();

  return (
    <div className="flex items-start gap-3 rounded-lg border border-invalid-200 bg-invalid-50 px-4 py-3 text-sm text-invalid-700">
      <AlertTriangle size={18} className="mt-0.5 shrink-0" />
      <div className="flex-1">{message}</div>
      {onRetry ? (
        <Button variant="ghost" className="!px-2 !py-1 text-invalid-700" onClick={onRetry}>
          {t('common.retry')}
        </Button>
      ) : null}
    </div>
  );
}

// --- Modal -----------------------------------------------------------------

export function Modal({
  open,
  title,
  onClose,
  children,
  footer,
}: {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  footer?: ReactNode;
}) {
  const { t } = useI18n();

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-ink-950/50 p-4">
      <div className="w-full max-w-lg animate-slide-up rounded-xl bg-white shadow-xl">
        <div className="flex items-center justify-between border-b border-ink-200 px-5 py-4">
          <h2 className="text-base font-semibold text-ink-900">{title}</h2>
          <button
            type="button"
            onClick={onClose}
            aria-label={t('common.close')}
            className="rounded-md p-1 text-ink-400 transition-colors hover:bg-ink-100 hover:text-ink-700"
          >
            <X size={18} />
          </button>
        </div>
        <div className="px-5 py-4">{children}</div>
        {footer ? (
          <div className="flex justify-end gap-2 border-t border-ink-200 px-5 py-3">{footer}</div>
        ) : null}
      </div>
    </div>
  );
}

// --- Formatting ------------------------------------------------------------

export function formatDate(value?: string | null): string {
  if (!value) return '—';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleDateString();
}

export function formatDateTime(value?: string | null): string {
  if (!value) return '—';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleString();
}
