import { useState, useEffect, useCallback } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { getCurrentOrgId } from '@/lib/api/http';
import { subscriptionApi } from './api';
import type { SubscriptionSummary, UsageSummary, EntitlementsResponse } from './api';
import { CreditCard, Gauge, CheckCircle2, XCircle, RefreshCw, AlertCircle, Clock, Calendar, Shield } from 'lucide-react';

type Tab = 'overview' | 'usage' | 'features';

const STATUS_STYLES: Record<string, { color: string; bg: string; border: string }> = {
  Trialing: { color: 'text-teal-700', bg: 'bg-teal-50', border: 'border-teal-200' },
  Active: { color: 'text-success-700', bg: 'bg-success-50', border: 'border-success-200' },
  PastDue: { color: 'text-attention-700', bg: 'bg-attention-50', border: 'border-attention-200' },
  GracePeriod: { color: 'text-warning-700', bg: 'bg-warning-50', border: 'border-warning-200' },
  Suspended: { color: 'text-invalid-700', bg: 'bg-invalid-50', border: 'border-invalid-200' },
  Expired: { color: 'text-ink-600', bg: 'bg-ink-100', border: 'border-ink-200' },
  Canceled: { color: 'text-ink-600', bg: 'bg-ink-100', border: 'border-ink-200' },
};

const FEATURE_LABELS: Record<string, { en: string; cn: string }> = {
  'testing.sprint': { en: 'Sprint Testing', cn: '短跑测试' },
  'testing.forceplate': { en: 'Force Plate Testing', cn: '测力台测试' },
  'analytics.advanced': { en: 'Advanced Analytics', cn: '高级分析' },
  'integrations.api': { en: 'API Integrations', cn: 'API 集成' },
  'reports.export': { en: 'Reports Export', cn: '报告导出' },
  'subscription.view': { en: 'Subscription View', cn: '订阅查看' },
  'subscription.manage': { en: 'Subscription Management', cn: '订阅管理' },
};

function formatDate(value: string | null, lang: 'en' | 'cn'): string {
  if (!value) return '—';
  try {
    return new Date(value).toLocaleDateString(lang === 'cn' ? 'zh-CN' : 'en-US', {
      year: 'numeric', month: 'short', day: 'numeric',
    });
  } catch {
    return value;
  }
}

function formatDateTime(value: string | null, lang: 'en' | 'cn'): string {
  if (!value) return '—';
  try {
    return new Date(value).toLocaleString(lang === 'cn' ? 'zh-CN' : 'en-US', {
      year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit',
    });
  } catch {
    return value;
  }
}

function usagePct(consumed: number, limit: number | null): number {
  if (!limit || limit <= 0) return 0;
  return Math.min(100, Math.round((consumed / limit) * 100));
}

export function SubscriptionPage() {
  const { lang, navigate, canAccess, addToast } = useApp();
  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['subscription'] ?? []).filter((tab) => canAccess(tab.view));

  const [tab, setTab] = useState<Tab>('overview');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [summary, setSummary] = useState<SubscriptionSummary | null>(null);
  const [usage, setUsage] = useState<UsageSummary | null>(null);
  const [entitlements, setEntitlements] = useState<EntitlementsResponse | null>(null);

  const orgId = getCurrentOrgId();

  const loadData = useCallback(async () => {
    if (!orgId) {
      setError(lang === 'cn' ? '无法确定当前组织 ID' : 'Unable to determine current organization id');
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const [s, u, e] = await Promise.all([
        subscriptionApi.getSummary(orgId).catch(() => null),
        subscriptionApi.getUsage(orgId).catch(() => null),
        subscriptionApi.getEntitlements(orgId).catch(() => null),
      ]);
      setSummary(s);
      setUsage(u);
      setEntitlements(e);
    } catch (err) {
      const msg = err instanceof Error ? err.message : String(err);
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [orgId, lang]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const onRetry = () => void loadData();

  const statusCfg = summary ? (STATUS_STYLES[summary.status] ?? STATUS_STYLES.Active) : null;

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold text-content-primary">{tr('nav.subscription', lang)}</h2>
            <p className="text-sm text-content-secondary mt-0.5">{tr('subscription.subtitle', lang)}</p>
          </div>
          <button
            onClick={onRetry}
            disabled={loading}
            className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-ink-700 bg-white border border-ink-200 rounded-lg hover:bg-ink-50 disabled:opacity-50 transition-colors"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            {tr('common.retry', lang)}
          </button>
        </div>

        {workspaceTabs.length > 1 && (
          <nav aria-label={tr('nav.subscription', lang)} className="flex items-center gap-1 mt-4 border-b border-default">
            {workspaceTabs.map((t) => {
              const active = t.view === 'subscription';
              return (
                <button
                  key={t.view}
                  type="button"
                  onClick={() => navigate(t.view)}
                  aria-current={active ? 'page' : undefined}
                  className={`px-4 py-2.5 -mb-px text-sm font-medium border-b-2 whitespace-nowrap transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent ${
                    active ? 'border-accent text-accent' : 'border-transparent text-content-secondary hover:text-content-primary'
                  }`}
                >
                  {tr(t.labelKey, lang)}
                </button>
              );
            })}
          </nav>
        )}
      </div>

      <div className="px-6 py-6">
        {/* Inner tab switch */}
        <div className="flex items-center gap-1 mb-6 border-b border-ink-200">
          {(['overview', 'usage', 'features'] as Tab[]).map((t) => {
            const active = tab === t;
            const labelKey = `subscription.tab.${t}` as const;
            return (
              <button
                key={t}
                type="button"
                onClick={() => setTab(t)}
                aria-current={active ? 'page' : undefined}
                className={`px-4 py-2.5 -mb-px text-sm font-medium border-b-2 whitespace-nowrap transition-colors ${
                  active ? 'border-accent text-accent' : 'border-transparent text-ink-500 hover:text-ink-800'
                }`}
              >
                {tr(labelKey, lang)}
              </button>
            );
          })}
        </div>

        {loading ? (
          <LoadingState lang={lang} />
        ) : error ? (
          <ErrorState message={error} onRetry={onRetry} lang={lang} />
        ) : !summary ? (
          <EmptyState lang={lang} />
        ) : (
          <>
            {tab === 'overview' && <OverviewTab summary={summary} statusCfg={statusCfg!} lang={lang} />}
            {tab === 'usage' && <UsageTab usage={usage} lang={lang} />}
            {tab === 'features' && <FeaturesTab entitlements={entitlements} lang={lang} addToast={addToast} />}
          </>
        )}
      </div>
    </div>
  );
}

function LoadingState({ lang }: { lang: 'en' | 'cn' }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-ink-400">
      <RefreshCw className="w-8 h-8 animate-spin mb-3" />
      <p className="text-sm">{tr('common.loading', lang)}</p>
    </div>
  );
}

function ErrorState({ message, onRetry, lang }: { message: string; onRetry: () => void; lang: 'en' | 'cn' }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle className="w-10 h-10 text-invalid-500 mb-3" />
      <p className="text-sm font-medium text-ink-800 mb-1">{tr('common.error', lang)}</p>
      <p className="text-xs text-ink-500 mb-4 max-w-md">{message}</p>
      <button
        onClick={onRetry}
        className="flex items-center gap-2 px-4 py-2 bg-accent hover:bg-accent-hover text-white text-sm font-medium rounded-lg transition-colors"
      >
        <RefreshCw className="w-4 h-4" />
        {tr('common.retry', lang)}
      </button>
    </div>
  );
}

function EmptyState({ lang }: { lang: 'en' | 'cn' }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center">
      <div className="w-16 h-16 rounded-2xl bg-ink-100 flex items-center justify-center mb-4">
        <CreditCard className="w-8 h-8 text-ink-400" />
      </div>
      <p className="text-base font-semibold text-ink-800 mb-1">{tr('subscription.noSubscription', lang)}</p>
      <p className="text-sm text-ink-500">{tr('subscription.noSubscriptionDetail', lang)}</p>
    </div>
  );
}

function OverviewTab({
  summary,
  statusCfg,
  lang,
}: {
  summary: SubscriptionSummary;
  statusCfg: { color: string; bg: string; border: string };
  lang: 'en' | 'cn';
}) {
  return (
    <div className="space-y-6">
      {/* Plan summary card */}
      <div className="bg-white rounded-xl border border-ink-200 p-6">
        <div className="flex items-start justify-between gap-4 flex-wrap">
          <div className="flex items-center gap-4">
            <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-teal-500 to-teal-700 flex items-center justify-center">
              <Shield className="w-7 h-7 text-white" />
            </div>
            <div>
              <p className="text-xs text-ink-500 uppercase tracking-wider">{tr('subscription.plan', lang)}</p>
              <h3 className="text-xl font-semibold text-ink-900">{summary.plan.name}</h3>
              <p className="text-xs text-ink-500">
                {summary.plan.code} · v{summary.plan.version} · {tr('subscription.revision', lang)} #{summary.revision}
              </p>
            </div>
          </div>
          <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium border ${statusCfg.bg} ${statusCfg.color} ${statusCfg.border}`}>
            {summary.status}
          </span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mt-6">
          <DetailField label={tr('subscription.renewalMode', lang)} value={summary.renewalMode === 'SalesManaged' ? tr('subscription.salesManaged', lang) : tr('subscription.autoRenew', lang)} />
          <DetailField label={tr('subscription.startsAt', lang)} value={formatDate(summary.startsAt, lang)} />
          <DetailField label={tr('subscription.currentPeriod', lang)} value={`${formatDate(summary.currentPeriodStart, lang)} → ${formatDate(summary.currentPeriodEnd, lang)}`} />
          <DetailField label={tr('subscription.graceUntil', lang)} value={formatDate(summary.graceUntil, lang)} />
        </div>

        {summary.cancelAtPeriodEnd && (
          <div className="mt-4 flex items-center gap-2 px-3 py-2 bg-attention-50 border border-attention-200 rounded-lg">
            <Clock className="w-4 h-4 text-attention-600" />
            <span className="text-sm text-attention-700">{tr('subscription.cancelAtPeriodEnd', lang)}</span>
          </div>
        )}
      </div>
    </div>
  );
}

function DetailField({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs text-ink-500 uppercase tracking-wider mb-1">{label}</p>
      <p className="text-sm font-medium text-ink-900 break-words">{value}</p>
    </div>
  );
}

function UsageTab({ usage, lang }: { usage: UsageSummary | null; lang: 'en' | 'cn' }) {
  if (!usage || !usage.items || usage.items.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <Gauge className="w-10 h-10 text-ink-300 mb-3" />
        <p className="text-sm text-ink-500">{tr('usage.noData', lang)}</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      {usage.items.map((item) => {
        const pct = usagePct(item.consumed, item.limit);
        const nearLimit = item.limit !== null && pct >= 80;
        const barColor = nearLimit ? 'bg-attention-500' : 'bg-teal-500';
        return (
          <div key={item.key} className="bg-white rounded-xl border border-ink-200 p-5">
            <div className="flex items-start justify-between gap-3 mb-3">
              <div>
                <p className="text-sm font-semibold text-ink-900">{item.key}</p>
                <p className="text-xs text-ink-500">{item.unit}</p>
              </div>
              <div className="text-right">
                <p className="text-lg font-semibold text-ink-900">
                  {item.consumed}
                  <span className="text-sm font-normal text-ink-500">
                    {' / '}
                    {item.limit === null ? tr('usage.unlimited', lang) : item.limit}
                  </span>
                </p>
              </div>
            </div>

            {item.limit !== null && item.limit > 0 && (
              <div className="w-full h-2 bg-ink-100 rounded-full overflow-hidden">
                <div className={`h-full ${barColor} rounded-full transition-all`} style={{ width: `${pct}%` }} />
              </div>
            )}

            <div className="flex items-center justify-between mt-3 text-xs text-ink-500">
              <span className="flex items-center gap-1">
                <Calendar className="w-3 h-3" />
                {item.periodStart ? `${formatDate(item.periodStart, lang)} → ${formatDate(item.periodEnd, lang)}` : tr('usage.period', lang)}
              </span>
              {item.limit !== null && <span>{pct}%</span>}
            </div>
          </div>
        );
      })}
    </div>
  );
}

function FeaturesTab({
  entitlements,
  lang,
  addToast,
}: {
  entitlements: EntitlementsResponse | null;
  lang: 'en' | 'cn';
  addToast: (msg: string, type?: 'success' | 'warning' | 'attention' | 'info') => void;
}) {
  if (!entitlements || !entitlements.items || entitlements.items.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <Shield className="w-10 h-10 text-ink-300 mb-3" />
        <p className="text-sm text-ink-500">{tr('entitlements.noData', lang)}</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl border border-ink-200 overflow-hidden">
      <div className="divide-y divide-ink-100">
        {entitlements.items.map((item) => {
          const label = FEATURE_LABELS[item.featureKey] ?? { en: item.featureKey, cn: item.featureKey };
          const enabled = item.enabled;
          return (
            <div key={item.featureKey} className="flex items-center justify-between gap-4 px-5 py-4">
              <div className="flex items-center gap-3 min-w-0">
                <div className={`w-9 h-9 rounded-lg flex items-center justify-center shrink-0 ${enabled ? 'bg-success-50' : 'bg-ink-100'}`}>
                  {enabled ? (
                    <CheckCircle2 className="w-5 h-5 text-success-600" />
                  ) : (
                    <XCircle className="w-5 h-5 text-ink-400" />
                  )}
                </div>
                <div className="min-w-0">
                  <p className="text-sm font-medium text-ink-900 truncate">{lang === 'cn' ? label.cn : label.en}</p>
                  <p className="text-xs text-ink-500 truncate">
                    {item.featureKey} · {item.source}
                    {item.effectiveUntil ? ` · ${tr('entitlements.effectiveUntil', lang)}: ${formatDateTime(item.effectiveUntil, lang)}` : ''}
                  </p>
                  {item.reason && <p className="text-xs text-ink-400 mt-0.5 truncate">{item.reason}</p>}
                </div>
              </div>
              <span
                className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium border shrink-0 ${
                  enabled
                    ? 'bg-success-50 text-success-700 border-success-200'
                    : 'bg-ink-100 text-ink-600 border-ink-200'
                }`}
              >
                {enabled ? tr('entitlements.enabled', lang) : tr('entitlements.disabled', lang)}
              </span>
            </div>
          );
        })}
      </div>
      <div className="px-5 py-3 bg-ink-50 border-t border-ink-100">
        <button
          onClick={() => addToast(lang === 'cn' ? '请联系客户经理调整功能权限。' : 'Contact your account manager to adjust feature access.', 'info')}
          className="text-xs font-medium text-teal-600 hover:text-teal-700"
        >
          {lang === 'cn' ? '需要调整功能权限？' : 'Need to adjust feature access?'}
        </button>
      </div>
    </div>
  );
}
