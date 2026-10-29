import { useEffect, useState } from 'react';
import { api, type PlanResponse, type CreatePlanRequest } from '../lib/api';
import { useI18n } from '../lib/i18n';
import { SubscriptionTabs } from '../components/SubscriptionTabs';
import {
  Button,
  Card,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  PageHeader,
  Select,
  Spinner,
  StatusPill,
  Table,
  Textarea,
  formatDate,
} from '../components/ui';

const FEATURE_KEYS = [
  'testing.sprint',
  'testing.forceplate',
  'analytics.advanced',
  'integrations.api',
  'reports.export',
  'subscription.view',
  'subscription.manage',
];

const QUOTA_KEYS = [
  'members.active',
  'devices.registered',
  'sessions.monthly',
  'storage.bytes',
];

export function SubscriptionPlansPage() {
  const { t } = useI18n();
  const [plans, setPlans] = useState<PlanResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  const [createOpen, setCreateOpen] = useState(false);
  const [publishTarget, setPublishTarget] = useState<PlanResponse | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionBusy, setActionBusy] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    api
      .get<PlanResponse[]>('/platform/subscription-plans')
      .then((response) => {
        if (cancelled) return;
        setPlans(response);
        setError(null);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : t('common.error'));
      })
      .finally(() => {
        if (cancelled) return;
        setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [reloadKey, t]);

  async function publishPlan(plan: PlanResponse) {
    setActionError(null);
    setActionBusy(true);
    try {
      await api.post<PlanResponse>(`/platform/subscription-plans/${plan.id}/publish`);
      setPublishTarget(null);
      setReloadKey((k) => k + 1);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : t('common.error'));
    } finally {
      setActionBusy(false);
    }
  }

  return (
    <div>
      <PageHeader
        title={t('plans.title')}
        subtitle={t('plans.subtitle')}
        actions={
          <Button onClick={() => { setActionError(null); setCreateOpen(true); }}>
            {t('plans.create')}
          </Button>
        }
      />

      <SubscriptionTabs />

      {actionError && !createOpen && !publishTarget ? (
        <div className="mb-4">
          <ErrorBanner message={actionError} />
        </div>
      ) : null}

      {error ? (
        <div className="mb-4">
          <ErrorBanner message={error} onRetry={() => setReloadKey((k) => k + 1)} />
        </div>
      ) : null}

      <Card>
        {loading ? (
          <Spinner />
        ) : plans.length === 0 ? (
          <EmptyState title={t('plans.empty')} />
        ) : (
          <Table
            headers={[
              t('plans.code'),
              t('plans.name'),
              t('plans.version'),
              t('plans.status'),
              t('plans.features'),
              t('plans.quotas'),
              t('plans.created'),
              t('common.actions'),
            ]}
          >
            {plans.map((plan) => (
              <tr key={plan.id}>
                <td className="px-4 py-3 font-mono text-sm font-medium text-ink-900">{plan.code}</td>
                <td className="px-4 py-3 text-ink-700">
                  <div className="font-medium text-ink-900">{plan.name}</div>
                  {plan.description ? (
                    <div className="text-xs text-ink-500 mt-0.5 line-clamp-1">{plan.description}</div>
                  ) : null}
                </td>
                <td className="px-4 py-3 text-ink-600">v{plan.version}</td>
                <td className="px-4 py-3">
                  <StatusPill status={plan.status} />
                </td>
                <td className="px-4 py-3 text-sm text-ink-600">
                  {plan.features.filter((f) => f.enabled).length}/{plan.features.length}
                </td>
                <td className="px-4 py-3 text-sm text-ink-600">{plan.quotas.length}</td>
                <td className="whitespace-nowrap px-4 py-3 text-ink-500">{formatDate(plan.createdAt)}</td>
                <td className="px-4 py-3">
                  {plan.status === 'Draft' ? (
                    <Button
                      variant="primary"
                      onClick={() => { setActionError(null); setPublishTarget(plan); }}
                    >
                      {t('plans.publish')}
                    </Button>
                  ) : (
                    <span className="text-xs text-ink-400">{t('plans.alreadyPublished')}</span>
                  )}
                </td>
              </tr>
            ))}
          </Table>
        )}
      </Card>

      <CreatePlanModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={() => { setCreateOpen(false); setReloadKey((k) => k + 1); }}
        onError={(msg) => setActionError(msg)}
        busy={actionBusy}
        setBusy={setActionBusy}
      />

      <Modal
        open={publishTarget !== null}
        title={t('plans.publish')}
        onClose={() => setPublishTarget(null)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setPublishTarget(null)} disabled={actionBusy}>
              {t('common.cancel')}
            </Button>
            <Button
              variant="primary"
              disabled={actionBusy}
              onClick={() => { if (publishTarget) void publishPlan(publishTarget); }}
            >
              {t('plans.publish')}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-600">{t('plans.publishConfirm')}</p>
        {publishTarget ? (
          <p className="mt-2 text-sm font-semibold text-ink-900">
            {publishTarget.name} <span className="font-mono text-ink-500">({publishTarget.code})</span>
          </p>
        ) : null}
        {actionError ? <p className="mt-3 text-sm text-invalid-600">{actionError}</p> : null}
      </Modal>
    </div>
  );
}

function CreatePlanModal({
  open,
  onClose,
  onCreated,
  onError,
  busy,
  setBusy,
}: {
  open: boolean;
  onClose: () => void;
  onCreated: () => void;
  onError: (msg: string) => void;
  busy: boolean;
  setBusy: (b: boolean) => void;
}) {
  const { t } = useI18n();
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [features, setFeatures] = useState<Record<string, boolean>>(() =>
    Object.fromEntries(FEATURE_KEYS.map((k) => [k, false])),
  );
  const [quotas, setQuotas] = useState<Record<string, string>>(() =>
    Object.fromEntries(QUOTA_KEYS.map((k) => [k, ''])),
  );

  useEffect(() => {
    if (open) {
      setCode('');
      setName('');
      setDescription('');
      setFeatures(Object.fromEntries(FEATURE_KEYS.map((k) => [k, false])));
      setQuotas(Object.fromEntries(QUOTA_KEYS.map((k) => [k, ''])));
    }
  }, [open]);

  async function handleSubmit() {
    onError('');
    if (!code.trim()) {
      onError(t('plans.code') + ' *');
      return;
    }
    setBusy(true);
    try {
      const payload: CreatePlanRequest = {
        code: code.trim(),
        name: name.trim() || code.trim(),
        version: 1,
        description: description.trim() || null,
        features: FEATURE_KEYS.map((k) => ({ featureKey: k, enabled: !!features[k] })),
        quotas: QUOTA_KEYS.map((k) => ({
          quotaKey: k,
          limitValue: quotas[k] === '' ? null : Number(quotas[k]),
        })),
      };
      await api.post<PlanResponse>('/platform/subscription-plans', payload);
      onCreated();
    } catch (err) {
      onError(err instanceof Error ? err.message : t('common.error'));
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal
      open={open}
      title={t('plans.createTitle')}
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={busy}>
            {t('common.cancel')}
          </Button>
          <Button variant="primary" onClick={() => void handleSubmit()} disabled={busy}>
            {t('common.save')}
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <label className="block">
            <span className="mb-1 block text-xs font-medium uppercase tracking-wide text-ink-500">
              {t('plans.code')} *
            </span>
            <Input value={code} onChange={(e) => setCode(e.target.value)} placeholder="e.g. STARTER" />
          </label>
          <label className="block">
            <span className="mb-1 block text-xs font-medium uppercase tracking-wide text-ink-500">
              {t('plans.name')}
            </span>
            <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="Plan display name" />
          </label>
        </div>
        <label className="block">
          <span className="mb-1 block text-xs font-medium uppercase tracking-wide text-ink-500">
            {t('plans.description')}
          </span>
          <Textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={2}
            placeholder="Optional plan description"
          />
        </label>

        <div>
          <span className="mb-2 block text-xs font-medium uppercase tracking-wide text-ink-500">
            {t('plans.features')}
          </span>
          <div className="grid gap-2 sm:grid-cols-2">
            {FEATURE_KEYS.map((key) => (
              <label key={key} className="flex items-center gap-2 rounded-md border border-ink-200 px-3 py-2">
                <input
                  type="checkbox"
                  checked={!!features[key]}
                  onChange={(e) => setFeatures((prev) => ({ ...prev, [key]: e.target.checked }))}
                  className="h-4 w-4 rounded border-ink-300 text-teal-600 focus:ring-teal-400"
                />
                <span className="text-sm text-ink-700">{key}</span>
              </label>
            ))}
          </div>
        </div>

        <div>
          <span className="mb-2 block text-xs font-medium uppercase tracking-wide text-ink-500">
            {t('plans.quotas')} <span className="normal-case text-ink-400">({t('plans.unlimited')} if blank)</span>
          </span>
          <div className="grid gap-2 sm:grid-cols-2">
            {QUOTA_KEYS.map((key) => (
              <label key={key} className="block">
                <span className="mb-1 block text-xs text-ink-500">{key}</span>
                <Input
                  type="number"
                  value={quotas[key]}
                  onChange={(e) => setQuotas((prev) => ({ ...prev, [key]: e.target.value }))}
                  placeholder={t('plans.unlimited')}
                />
              </label>
            ))}
          </div>
        </div>
      </div>
    </Modal>
  );
}