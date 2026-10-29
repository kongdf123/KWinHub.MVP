import { useEffect, useState } from 'react';
import {
  api,
  type PlanResponse,
  type PlatformSubscriptionResponse,
  type AssignSubscriptionRequest,
} from '../lib/api';
import { useI18n } from '../lib/i18n';
import { navigate } from '../lib/router';
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
  formatDate,
} from '../components/ui';

const SUBSCRIPTION_STATUSES = ['Active', 'Trialing', 'PastDue', 'GracePeriod', 'Suspended', 'Expired', 'Canceled'];

export function SubscriptionsPage() {
  const { t } = useI18n();
  const [subscriptions, setSubscriptions] = useState<PlatformSubscriptionResponse[]>([]);
  const [plans, setPlans] = useState<PlanResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  const [assignOpen, setAssignOpen] = useState(false);
  const [assignTarget, setAssignTarget] = useState<PlatformSubscriptionResponse | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionBusy, setActionBusy] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    Promise.all([
      api.get<PlatformSubscriptionResponse[]>('/platform/subscriptions'),
      api.get<PlanResponse[]>('/platform/subscription-plans').catch(() => [] as PlanResponse[]),
    ])
      .then(([subs, pl]) => {
        if (cancelled) return;
        setSubscriptions(subs);
        setPlans(pl.filter((p) => p.status === 'Published'));
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

  async function assignSubscription(
    orgId: string,
    payload: AssignSubscriptionRequest,
    expectedRevision?: number,
  ) {
    setActionError(null);
    setActionBusy(true);
    try {
      const headers =
        expectedRevision !== undefined ? { 'If-Match': String(expectedRevision) } : undefined;
      await api.put<PlatformSubscriptionResponse>(
        `/platform/organizations/${orgId}/subscription`,
        payload,
        headers,
      );
      setAssignOpen(false);
      setAssignTarget(null);
      setReloadKey((k) => k + 1);
    } catch (err) {
      const msg = err instanceof Error ? err.message : t('common.error');
      // 409 from the backend surfaces as ApiError with status 409.
      if (err instanceof Error && /409|conflict|revision/i.test(msg)) {
        setActionError(t('subs.staleRevision'));
      } else {
        setActionError(msg);
      }
    } finally {
      setActionBusy(false);
    }
  }

  return (
    <div>
      <PageHeader title={t('subs.title')} subtitle={t('subs.subtitle')} />

      <SubscriptionTabs />

      {actionError && !assignOpen ? (
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
        ) : subscriptions.length === 0 ? (
          <EmptyState title={t('subs.empty')} />
        ) : (
          <Table
            headers={[
              t('subs.organization'),
              t('subs.plan'),
              t('subs.status'),
              t('subs.startsAt'),
              t('subs.endsAt'),
              t('subs.revision'),
              t('common.actions'),
            ]}
          >
            {subscriptions.map((sub) => (
              <tr key={sub.id}>
                <td className="px-4 py-3 font-medium text-ink-900">
                  <button
                    type="button"
                    onClick={() => navigate(`/organizations/${sub.organizationId}`)}
                    className="text-teal-600 hover:underline"
                  >
                    {sub.organizationId}
                  </button>
                </td>
                <td className="px-4 py-3 text-ink-700">
                  <div className="font-medium text-ink-900">{sub.planName}</div>
                  <div className="text-xs text-ink-500">
                    {sub.planCode} · v{sub.planVersion}
                  </div>
                </td>
                <td className="px-4 py-3">
                  <StatusPill status={sub.status} />
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-ink-600">{formatDate(sub.startsAt)}</td>
                <td className="whitespace-nowrap px-4 py-3 text-ink-600">{formatDate(sub.endsAt)}</td>
                <td className="px-4 py-3 text-ink-500">#{sub.revision}</td>
                <td className="px-4 py-3">
                  <Button
                    variant="secondary"
                    onClick={() => {
                      setActionError(null);
                      setAssignTarget(sub);
                      setAssignOpen(true);
                    }}
                  >
                    {t('subs.assign')}
                  </Button>
                </td>
              </tr>
            ))}
          </Table>
        )}
      </Card>

      <AssignSubscriptionModal
        open={assignOpen}
        target={assignTarget}
        plans={plans}
        busy={actionBusy}
        error={actionError}
        onClose={() => { setAssignOpen(false); setAssignTarget(null); }}
        onSubmit={(payload) => {
          if (!assignTarget) return;
          void assignSubscription(
            assignTarget.organizationId,
            payload,
            assignTarget.revision,
          );
        }}
      />
    </div>
  );
}

function AssignSubscriptionModal({
  open,
  target,
  plans,
  busy,
  error,
  onClose,
  onSubmit,
}: {
  open: boolean;
  target: PlatformSubscriptionResponse | null;
  plans: PlanResponse[];
  busy: boolean;
  error: string | null;
  onClose: () => void;
  onSubmit: (payload: AssignSubscriptionRequest) => void;
}) {
  const { t } = useI18n();
  const [planId, setPlanId] = useState('');
  const [status, setStatus] = useState('Active');
  const [reason, setReason] = useState('');

  useEffect(() => {
    if (open && target) {
      setPlanId(target.planId);
      setStatus(target.status);
      setReason('');
    }
  }, [open, target]);

  const canSubmit = !!planId && reason.trim().length > 0;

  function handleSubmit() {
    if (!canSubmit) return;
    const now = new Date();
    const ends = new Date(now.getTime() + 365 * 86_400_000);
    onSubmit({
      planId,
      status,
      startsAt: now.toISOString(),
      endsAt: ends.toISOString(),
      reason: reason.trim(),
    });
  }

  return (
    <Modal
      open={open}
      title={t('subs.assignTitle')}
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={busy}>
            {t('common.cancel')}
          </Button>
          <Button variant="primary" onClick={handleSubmit} disabled={busy || !canSubmit}>
            {t('common.save')}
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        {target ? (
          <div className="rounded-md bg-ink-50 px-3 py-2 text-xs text-ink-500">
            {target.organizationId} · {t('subs.revision')} #{target.revision}
          </div>
        ) : null}

        <label className="block">
          <span className="mb-1 block text-xs font-medium uppercase tracking-wide text-ink-500">
            {t('subs.selectPlan')} *
          </span>
          <Select value={planId} onChange={(e) => setPlanId(e.target.value)}>
            <option value="">—</option>
            {plans.map((p) => (
              <option key={p.id} value={p.id}>
                {p.name} ({p.code} v{p.version})
              </option>
            ))}
          </Select>
        </label>

        <label className="block">
          <span className="mb-1 block text-xs font-medium uppercase tracking-wide text-ink-500">
            {t('subs.status')}
          </span>
          <Select value={status} onChange={(e) => setStatus(e.target.value)}>
            {SUBSCRIPTION_STATUSES.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </Select>
        </label>

        <label className="block">
          <span className="mb-1 block text-xs font-medium uppercase tracking-wide text-ink-500">
            {t('subs.reason')}
          </span>
          <Input
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder={t('subs.reasonPlaceholder')}
          />
        </label>

        {error ? <p className="text-sm text-invalid-600">{error}</p> : null}
      </div>
    </Modal>
  );
}