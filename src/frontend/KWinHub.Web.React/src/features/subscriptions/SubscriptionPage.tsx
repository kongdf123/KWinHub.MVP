import { useCallback, useEffect, useState } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { getCurrentOrgId } from '@/lib/api/http';
import { subscriptionApi } from './api';
import type { SubscriptionSummary, UsageSummary, EntitlementsResponse, EntitlementItem, UsageItem } from './api';
import {
  AlertCircle, ArrowRight, CalendarDays, CheckCircle2, CircleHelp, Gauge,
  LockKeyhole, RefreshCw, ShieldCheck, Sparkles,
} from 'lucide-react';

type Tab = 'overview' | 'usage' | 'features';
type ViewData = { summary: SubscriptionSummary | null; usage: UsageSummary | null; entitlements: EntitlementsResponse | null };

const STATUS_STYLES: Record<string, { color: string; bg: string; border: string }> = {
  Trialing: { color: 'text-teal-700', bg: 'bg-teal-50', border: 'border-teal-200' },
  Active: { color: 'text-success-700', bg: 'bg-success-50', border: 'border-success-200' },
  PastDue: { color: 'text-attention-700', bg: 'bg-attention-50', border: 'border-attention-200' },
  GracePeriod: { color: 'text-warning-700', bg: 'bg-warning-50', border: 'border-warning-200' },
  Suspended: { color: 'text-invalid-700', bg: 'bg-invalid-50', border: 'border-invalid-200' },
  Expired: { color: 'text-ink-600', bg: 'bg-ink-100', border: 'border-ink-200' },
  Canceled: { color: 'text-ink-600', bg: 'bg-ink-100', border: 'border-ink-200' },
};

const DEMO_ORG = '00000000-0000-0000-0000-000000000001';
const demoData: ViewData = {
  summary: {
    organizationId: DEMO_ORG,
    subscriptionId: 'demo-sub-001',
    plan: { code: 'PERFORMANCE_PRO', name: 'Performance Pro', version: 2 },
    status: 'Active',
    startsAt: '2026-10-01T00:00:00Z',
    currentPeriodStart: '2026-10-01T00:00:00Z',
    currentPeriodEnd: '2026-11-01T00:00:00Z',
    graceUntil: null,
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    currency: 'USD',
    amountMinor: 24900,
    revision: 3,
  },
  usage: {
    organizationId: DEMO_ORG,
    asOf: '2026-10-29T00:00:00Z',
    items: [
      { key: 'sessions.monthly', consumed: 428, limit: 1000, unit: 'sessions', periodStart: '2026-10-01', periodEnd: '2026-11-01' },
      { key: 'members', consumed: 8, limit: 25, unit: 'members', periodStart: null, periodEnd: null },
      { key: 'devices', consumed: 3, limit: 10, unit: 'devices', periodStart: null, periodEnd: null },
      { key: 'storage.gb', consumed: 78.4, limit: 100, unit: 'GB', periodStart: null, periodEnd: null },
      { key: 'athletes', consumed: 183, limit: 500, unit: 'athletes', periodStart: null, periodEnd: null },
    ],
  },
  entitlements: {
    organizationId: DEMO_ORG,
    items: [
      { featureKey: 'testing.forceplate', enabled: true, source: 'Plan', effectiveUntil: null },
      { featureKey: 'testing.sprint', enabled: true, source: 'Plan', effectiveUntil: null },
      { featureKey: 'testing.video', enabled: false, source: 'Plan', effectiveUntil: null },
      { featureKey: 'analytics.advanced', enabled: true, source: 'Plan', effectiveUntil: null },
      { featureKey: 'reports.export', enabled: true, source: 'Plan', effectiveUntil: null },
      { featureKey: 'integrations.api', enabled: false, source: 'Plan', effectiveUntil: null },
    ],
  },
};

const METRIC_LABELS: Record<string, { en: string; cn: string }> = {
  'sessions.monthly': { en: 'Monthly tests', cn: '本月测试' },
  members: { en: 'Team members', cn: '团队成员' },
  devices: { en: 'Registered devices', cn: '注册设备' },
  'storage.gb': { en: 'Cloud storage', cn: '云存储' },
  athletes: { en: 'Athletes', cn: '运动员' },
};

const FEATURE_LABELS: Record<string, { en: { name: string; desc: string }; cn: { name: string; desc: string } }> = {
  'testing.forceplate': { en: { name: 'Force Plate Analysis', desc: 'Force-time curves and jump metrics' }, cn: { name: '测力台分析', desc: '测力曲线与跳跃指标' } },
  'testing.sprint': { en: { name: 'Sprint Testing', desc: 'Sprint starts and split timing' }, cn: { name: '短跑测试', desc: '起跑与分段计时' } },
  'testing.video': { en: { name: 'Video Analysis', desc: 'Synchronized video review' }, cn: { name: '视频分析', desc: '同步视频分析' } },
  'analytics.advanced': { en: { name: 'Advanced Analytics', desc: 'Longitudinal and comparative analysis' }, cn: { name: '高级分析', desc: '长期趋势与对比分析' } },
  'reports.export': { en: { name: 'Report Exports', desc: 'PDF / CSV test reports' }, cn: { name: '报告导出', desc: 'PDF / CSV 测试报告' } },
  'integrations.api': { en: { name: 'API Integrations', desc: 'Third-party data integrations' }, cn: { name: 'API 集成', desc: '第三方数据对接' } },
  'subscription.view': { en: { name: 'Subscription View', desc: 'View plan and usage details' }, cn: { name: '订阅查看', desc: '查看套餐与用量' } },
  'subscription.manage': { en: { name: 'Subscription Management', desc: 'Manage plan and billing' }, cn: { name: '订阅管理', desc: '管理套餐与账单' } },
};

function fmtDate(value: string | null, cn: boolean): string {
  if (!value) return '—';
  try {
    return new Intl.DateTimeFormat(cn ? 'zh-CN' : 'en-US', {
      year: 'numeric', month: 'short', day: 'numeric', timeZone: 'UTC',
    }).format(new Date(value));
  } catch {
    return value;
  }
}

function usagePct(consumed: number, limit: number | null): number {
  if (!limit || limit <= 0) return 0;
  return Math.min(100, (consumed / limit) * 100);
}

function metricLabel(key: string, cn: boolean): string {
  const label = METRIC_LABELS[key];
  return label ? (cn ? label.cn : label.en) : key;
}

function featureLabel(key: string, cn: boolean): { name: string; desc: string } {
  const label = FEATURE_LABELS[key];
  if (!label) return { name: key, desc: '' };
  return cn ? label.cn : label.en;
}

function UsageCard({ item, cn }: { item: UsageItem; cn: boolean }) {
  const percent = usagePct(item.consumed, item.limit);
  const nearLimit = item.limit !== null && percent >= 75;
  const pctLabel = item.limit === null
    ? tr('subscription.unlimited', cn ? 'cn' : 'en')
    : `${Math.round(percent)}%`;
  const periodText = item.periodStart
    ? `${fmtDate(item.periodStart, cn)} – ${fmtDate(item.periodEnd, cn)}`
    : tr('subscription.currentAllocation', cn ? 'cn' : 'en');

  return (
    <div className="rounded-xl border border-ink-200 bg-white p-5">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="font-semibold text-ink-900">{metricLabel(item.key, cn)}</p>
          <p className="mt-1 text-xs text-ink-500">{item.unit}</p>
        </div>
        <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${nearLimit ? 'bg-amber-50 text-amber-800' : 'bg-teal-50 text-teal-700'}`}>
          {pctLabel}
        </span>
      </div>
      <p className="mt-5 text-2xl font-semibold tracking-tight text-ink-900">
        {item.consumed.toLocaleString()}
        <span className="text-sm font-normal text-ink-500">
          {' / '}{item.limit?.toLocaleString() ?? '∞'}
        </span>
      </p>
      {item.limit !== null && (
        <div
          role="progressbar"
          aria-label={metricLabel(item.key, cn)}
          aria-valuenow={item.consumed}
          aria-valuemin={0}
          aria-valuemax={item.limit}
          className="mt-3 h-2.5 overflow-hidden rounded-full bg-ink-100"
        >
          <div
            style={{ width: `${percent}%` }}
            className={`h-full rounded-full ${nearLimit ? 'bg-amber-500' : 'bg-teal-600'}`}
          />
        </div>
      )}
      <p className="mt-3 text-xs text-ink-500">
        {periodText}
        {nearLimit ? ` · ${tr('subscription.nearLimit', cn ? 'cn' : 'en')}` : ''}
      </p>
    </div>
  );
}

function FeatureRows({ items, cn }: { items: EntitlementItem[]; cn: boolean }) {
  return (
    <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
      {items.map((item) => {
        const label = featureLabel(item.featureKey, cn);
        return (
          <div key={item.featureKey} className="flex items-start gap-3 rounded-xl border border-ink-200 bg-white p-4">
            <span className={`mt-0.5 flex h-9 w-9 flex-none items-center justify-center rounded-lg ${item.enabled ? 'bg-teal-50 text-teal-700' : 'bg-ink-100 text-ink-400'}`}>
              {item.enabled ? <CheckCircle2 className="h-5 w-5" /> : <LockKeyhole className="h-5 w-5" />}
            </span>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold text-ink-900">{label.name}</p>
              {label.desc && <p className="mt-1 text-xs text-ink-500">{label.desc}</p>}
              <p className="mt-2 text-[11px] text-ink-400">{item.featureKey}</p>
              {item.reason && <p className="mt-0.5 text-[11px] text-ink-400">{item.reason}</p>}
            </div>
            <span className={`whitespace-nowrap rounded-full px-2 py-1 text-xs ${item.enabled ? 'bg-teal-50 text-teal-700' : 'bg-ink-100 text-ink-500'}`}>
              {item.enabled ? tr('subscription.included', cn ? 'cn' : 'en') : tr('subscription.notIncluded', cn ? 'cn' : 'en')}
            </span>
          </div>
        );
      })}
    </div>
  );
}

function TabButton({ active, onClick, children }: { active: boolean; onClick: () => void; children: string }) {
  return (
    <button
      type="button"
      role="tab"
      aria-selected={active}
      onClick={onClick}
      className={`border-b-2 px-4 py-3 text-sm font-medium transition-colors ${active ? 'border-teal-600 text-teal-700' : 'border-transparent text-ink-500 hover:text-ink-800'}`}
    >
      {children}
    </button>
  );
}

export function SubscriptionPage() {
  const { lang, navigate, canAccess, addToast } = useApp();
  const cn = lang === 'cn';
  const tabs = (WORKSPACE_TABS_BY_VIEW.subscription ?? []).filter((t) => canAccess(t.view));
  const orgId = getCurrentOrgId();

  const [tab, setTab] = useState<Tab>('overview');
  const [mode, setMode] = useState<'live' | 'demo'>(import.meta.env.VITE_DATA_MODE === 'api' ? 'live' : 'demo');
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState<ViewData>({ summary: null, usage: null, entitlements: null });
  const [error, setError] = useState<string | null>(null);

  const demoEnabled = import.meta.env.DEV || import.meta.env.VITE_ENABLE_SUBSCRIPTION_DEMO === 'true';

  const load = useCallback(async () => {
    if (mode === 'demo') {
      setData(demoData);
      setError(null);
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    if (!orgId) {
      setData({ summary: null, usage: null, entitlements: null });
      setError(tr('subscription.noOrg', cn ? 'cn' : 'en'));
      setLoading(false);
      return;
    }
    const results = await Promise.allSettled([
      subscriptionApi.getSummary(orgId),
      subscriptionApi.getUsage(orgId),
      subscriptionApi.getEntitlements(orgId),
    ]);
    const [s, u, e] = results;
    setData({
      summary: s.status === 'fulfilled' ? s.value : null,
      usage: u.status === 'fulfilled' ? u.value : null,
      entitlements: e.status === 'fulfilled' ? e.value : null,
    });
    const errors = results.filter((r) => r.status === 'rejected') as PromiseRejectedResult[];
    if (errors.length) {
      setError(errors.map((x) => (x.reason instanceof Error ? x.reason.message : String(x.reason))).join('; '));
    }
    setLoading(false);
  }, [orgId, mode, cn]);

  useEffect(() => {
    void load();
  }, [load]);

  const s = data.summary;
  const usage = data.usage;
  const features = data.entitlements?.items ?? [];

  const money = s?.amountMinor != null && s.currency
    ? new Intl.NumberFormat(cn ? 'zh-CN' : 'en-US', { style: 'currency', currency: s.currency }).format(s.amountMinor / 100)
    : tr('subscription.contactPricing', cn ? 'cn' : 'en');

  const statusCfg = s ? (STATUS_STYLES[s.status] ?? STATUS_STYLES.Active) : null;

  return (
    <div>
      <TopBar />
      <div className="px-6 pt-6">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="text-xl font-semibold text-content-primary">{tr('nav.subscription', lang)}</h2>
            <p className="mt-1 text-sm text-content-secondary">
              {cn ? '查看套餐、功能权益与资源使用情况' : 'Your plan, enabled features, and resource usage'}
            </p>
          </div>
          <div className="flex gap-2">
            {demoEnabled && (
              <button
                type="button"
                onClick={() => setMode((m) => (m === 'demo' ? 'live' : 'demo'))}
                className="rounded-lg border border-teal-200 bg-teal-50 px-3 py-2 text-sm font-medium text-teal-800 hover:bg-teal-100 transition-colors"
              >
                {mode === 'demo' ? tr('subscription.liveData', lang) : tr('subscription.demoToggle', lang)}
              </button>
            )}
            <button
              type="button"
              onClick={() => void load()}
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm text-ink-700 hover:bg-ink-50 disabled:opacity-50 transition-colors"
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
              {tr('subscription.refresh', lang)}
            </button>
          </div>
        </div>

        {tabs.length > 1 && (
          <nav className="mt-5 flex gap-1 overflow-x-auto border-b border-default">
            {tabs.map((t) => (
              <button
                key={t.view}
                type="button"
                onClick={() => navigate(t.view)}
                className={`whitespace-nowrap border-b-2 px-4 py-2.5 text-sm ${t.view === 'subscription' ? 'border-teal-600 text-teal-700' : 'border-transparent text-ink-500 hover:text-ink-800'}`}
              >
                {tr(t.labelKey, lang)}
              </button>
            ))}
          </nav>
        )}
      </div>

      <div className="space-y-5 px-6 py-6">
        {mode === 'demo' && (
          <div className="flex items-center gap-2 rounded-lg border border-blue-200 bg-blue-50 px-4 py-3 text-sm text-blue-800">
            <Sparkles className="h-4 w-4" />
            {tr('subscription.demoBanner', lang)}
          </div>
        )}

        <div role="tablist" aria-label={tr('nav.subscription', lang)} className="flex overflow-x-auto border-b border-ink-200">
          <TabButton active={tab === 'overview'} onClick={() => setTab('overview')}>{tr('subscription.tab.overview', lang)}</TabButton>
          <TabButton active={tab === 'usage'} onClick={() => setTab('usage')}>{tr('subscription.tab.usage', lang)}</TabButton>
          <TabButton active={tab === 'features'} onClick={() => setTab('features')}>{tr('subscription.tab.features', lang)}</TabButton>
        </div>

        {loading ? (
          <div className="flex justify-center gap-2 py-16 text-ink-500">
            <RefreshCw className="h-5 w-5 animate-spin" />
            {tr('common.loading', lang)}
          </div>
        ) : error && mode === 'live' ? (
          <div className="rounded-xl border border-attention-200 bg-attention-50 p-6">
            <div className="flex items-center gap-2 font-medium text-attention-800">
              <AlertCircle className="h-5 w-5" />
              {tr('subscription.loadFailed', lang)}
            </div>
            <p className="mt-2 text-sm text-attention-700">{error}</p>
            <div className="mt-4 flex flex-wrap gap-2">
              <button onClick={() => void load()} className="rounded-lg bg-white px-3 py-2 text-sm text-ink-800 ring-1 ring-attention-200 hover:bg-ink-50 transition-colors">
                {tr('common.retry', lang)}
              </button>
              {demoEnabled && (
                <button onClick={() => setMode('demo')} className="rounded-lg bg-white px-3 py-2 text-sm text-ink-800 ring-1 ring-attention-200 hover:bg-ink-50 transition-colors">
                  {tr('subscription.viewDemo', lang)}
                </button>
              )}
            </div>
          </div>
        ) : !s ? (
          <div className="rounded-xl border border-ink-200 bg-white p-10 text-center">
            <CircleHelp className="mx-auto h-10 w-10 text-ink-400" />
            <h3 className="mt-3 font-semibold text-ink-900">{tr('subscription.noSubscription', lang)}</h3>
            <p className="mt-2 text-sm text-ink-500">{tr('subscription.noSubscriptionDetail', lang)}</p>
            {demoEnabled && (
              <button onClick={() => setMode('demo')} className="mt-4 rounded-lg bg-teal-700 px-4 py-2 text-sm font-medium text-white hover:bg-teal-800 transition-colors">
                {tr('subscription.viewDemo', lang)}
              </button>
            )}
          </div>
        ) : (
          <>
            {tab === 'overview' && (
              <div className="space-y-5">
                <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
                  <div className="rounded-xl border border-ink-200 bg-white p-6 lg:col-span-2">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-widest text-teal-700">{tr('subscription.currentPlan', lang)}</p>
                        <h3 className="mt-2 text-2xl font-semibold text-ink-900">{s.plan.name}</h3>
                        <p className="mt-1 text-xs text-ink-500">
                          {s.plan.code} · v{s.plan.version} · {tr('subscription.revision', lang)} #{s.revision}
                        </p>
                      </div>
                      {statusCfg && (
                        <span className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold border ${statusCfg.bg} ${statusCfg.color} ${statusCfg.border}`}>
                          {s.status}
                        </span>
                      )}
                    </div>
                    <p className="mt-6 text-3xl font-semibold text-ink-900">
                      {money}
                      <span className="ml-2 text-xs font-normal text-ink-500">
                        {mode === 'demo' && ` · ${tr('subscription.sampleAmount', lang)}`}
                        {s.renewalMode === 'SalesManaged'
                          ? ` · ${tr('subscription.salesManaged', lang)}`
                          : ` · ${tr('subscription.autoRenew', lang)}`}
                      </span>
                    </p>
                    <div className="mt-6 grid grid-cols-1 gap-3 border-t border-ink-100 pt-4 sm:grid-cols-2">
                      <div>
                        <p className="text-xs text-ink-500">{tr('subscription.startsAt', lang)}</p>
                        <p className="mt-1 text-sm font-medium">{fmtDate(s.startsAt, cn)}</p>
                      </div>
                      <div>
                        <p className="text-xs text-ink-500">{tr('subscription.periodEnds', lang)}</p>
                        <p className="mt-1 text-sm font-medium">{fmtDate(s.currentPeriodEnd, cn)}</p>
                      </div>
                    </div>
                  </div>

                  <div className="rounded-xl border border-ink-200 bg-white p-6">
                    <CalendarDays className="h-6 w-6 text-teal-700" />
                    <p className="mt-4 text-sm font-semibold text-ink-900">{tr('subscription.supportTitle', lang)}</p>
                    <p className="mt-2 text-sm text-ink-600">
                      {s.renewalMode === 'SalesManaged'
                        ? tr('subscription.supportSalesManaged', lang)
                        : tr('subscription.supportAutoRenew', lang)}
                    </p>
                    <p className="mt-4 text-xs text-ink-500">
                      {tr('subscription.periodEnds', lang)}: {fmtDate(s.currentPeriodEnd, cn)}
                    </p>
                    <button
                      onClick={() => addToast(tr('subscription.contactUpgrade', lang), 'info')}
                      className="mt-4 inline-flex items-center gap-2 text-sm font-semibold text-teal-700 hover:text-teal-800 transition-colors"
                    >
                      {tr('subscription.learnMore', lang)}
                      <ArrowRight className="h-4 w-4" />
                    </button>
                  </div>
                </div>

                <div className="grid grid-cols-1 gap-5 xl:grid-cols-2">
                  <div className="rounded-xl border border-ink-200 bg-white p-5">
                    <div className="flex items-center justify-between">
                      <h4 className="font-semibold text-ink-900">{tr('subscription.usageSnapshot', lang)}</h4>
                      <button onClick={() => setTab('usage')} className="text-xs font-semibold text-teal-700 hover:text-teal-800 transition-colors">
                        {tr('common.viewAll', lang)}
                      </button>
                    </div>
                    <div className="mt-4 space-y-4">
                      {(usage?.items ?? []).slice(0, 3).map((item) => {
                        const p = usagePct(item.consumed, item.limit);
                        const nearLimit = item.limit !== null && p >= 75;
                        return (
                          <div key={item.key}>
                            <div className="mb-1.5 flex justify-between text-xs">
                              <span>{metricLabel(item.key, cn)}</span>
                              <span>
                                {item.consumed.toLocaleString()} / {item.limit?.toLocaleString() ?? '∞'}
                              </span>
                            </div>
                            <div className="h-2 rounded-full bg-ink-100">
                              <div className={`h-2 rounded-full ${nearLimit ? 'bg-amber-500' : 'bg-teal-600'}`} style={{ width: `${p}%` }} />
                            </div>
                          </div>
                        );
                      })}
                      {!usage?.items?.length && <p className="text-sm text-ink-500">{tr('subscription.noUsageData', lang)}</p>}
                    </div>
                  </div>

                  <div className="rounded-xl border border-ink-200 bg-white p-5">
                    <div className="flex items-center justify-between">
                      <h4 className="font-semibold text-ink-900">{tr('subscription.includedFeatures', lang)}</h4>
                      <button onClick={() => setTab('features')} className="text-xs font-semibold text-teal-700 hover:text-teal-800 transition-colors">
                        {tr('common.viewAll', lang)}
                      </button>
                    </div>
                    <div className="mt-4 space-y-3">
                      {features.filter((f) => f.enabled).slice(0, 4).map((f) => (
                        <div key={f.featureKey} className="flex items-center gap-2 text-sm text-ink-700">
                          <CheckCircle2 className="h-4 w-4 text-teal-600" />
                          {featureLabel(f.featureKey, cn).name}
                        </div>
                      ))}
                      {!features.some((f) => f.enabled) && <p className="text-sm text-ink-500">{tr('subscription.noEnabledFeatures', lang)}</p>}
                    </div>
                  </div>
                </div>
              </div>
            )}

            {tab === 'usage' && (
              <div>
                <div className="mb-4 flex items-center gap-2 text-sm text-ink-600">
                  <Gauge className="h-4 w-4 text-teal-700" />
                  {tr('subscription.usageSubtitle', lang)}
                </div>
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
                  {usage?.items.map((item) => (
                    <UsageCard key={item.key} item={item} cn={cn} />
                  ))}
                </div>
                {!usage?.items?.length && (
                  <div className="rounded-xl border border-ink-200 bg-white p-8 text-sm text-ink-500">
                    {tr('subscription.noUsageRecords', lang)}
                  </div>
                )}
              </div>
            )}

            {tab === 'features' && (
              <div>
                <div className="mb-4 flex items-center gap-2 text-sm text-ink-600">
                  <ShieldCheck className="h-4 w-4 text-teal-700" />
                  {tr('subscription.featuresSubtitle', lang)}
                </div>
                <FeatureRows items={features} cn={cn} />
                {features.length === 0 && (
                  <div className="rounded-xl border border-ink-200 bg-white p-8 text-sm text-ink-500">
                    {tr('subscription.noEntitlements', lang)}
                  </div>
                )}
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}