import { useEffect, useState } from 'react';
import { ArrowLeft } from 'lucide-react';
import {
  api,
  type FeatureOverrideResponse,
  type PlatformOrganizationDetailResponse,
  type QuotaOverrideResponse,
  type SubscriptionAuditEventResponse,
} from '../lib/api';
import { useI18n } from '../lib/i18n';
import { navigate } from '../lib/router';
import {
  Button,
  Card,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  Select,
  Spinner,
  StatusPill,
  Table,
  Textarea,
  formatDate,
  formatDateTime,
} from '../components/ui';

type Tab = 'overview' | 'members' | 'entitlements';

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

export function OrganizationDetailPage({ id }: { id: string }) {
  const { t } = useI18n();

  const [organization, setOrganization] = useState<PlatformOrganizationDetailResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);
  const [tab, setTab] = useState<Tab>('overview');

  const [confirmOpen, setConfirmOpen] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionBusy, setActionBusy] = useState(false);

  const [featureOverrides, setFeatureOverrides] = useState<FeatureOverrideResponse[]>([]);
  const [quotaOverrides, setQuotaOverrides] = useState<QuotaOverrideResponse[]>([]);
  const [auditEvents, setAuditEvents] = useState<SubscriptionAuditEventResponse[]>([]);
  const [overrideModal, setOverrideModal] = useState<{
    kind: 'feature' | 'quota';
    key: string;
  } | null>(null);
  const [overrideForm, setOverrideForm] = useState({
    enabled: true,
    limitValue: '',
    reason: '',
  });

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    api
      .get<PlatformOrganizationDetailResponse>(`/platform/organizations/${id}`)
      .then((response) => {
        if (cancelled) return;
        setOrganization(response);
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
  }, [id, reloadKey]);

  async function changeStatus(action: 'suspend' | 'reactivate') {
    setActionError(null);
    setActionBusy(true);

    try {
      await api.changeStatus<void>(`/platform/organizations/${id}/status`, action);
      setConfirmOpen(false);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : t('orgs.actionFailed'));
    } finally {
      setActionBusy(false);
    }
  }

  useEffect(() => {
    if (tab !== 'entitlements') return;
    let cancelled = false;
    Promise.all([
      api.get<FeatureOverrideResponse[]>(`/platform/organizations/${id}/entitlements`).catch(() => []),
      api.get<QuotaOverrideResponse[]>(`/platform/organizations/${id}/quotas`).catch(() => []),
      api.get<SubscriptionAuditEventResponse[]>(`/platform/organizations/${id}/subscription-audit`).catch(() => []),
    ]).then(([fo, qo, audit]) => {
      if (cancelled) return;
      setFeatureOverrides(fo);
      setQuotaOverrides(qo);
      setAuditEvents(audit);
    });
    return () => {
      cancelled = true;
    };
  }, [tab, id, reloadKey]);

  async function submitOverride() {
    if (!overrideModal || !overrideForm.reason.trim()) return;
    setActionBusy(true);
    setActionError(null);
    try {
      const now = new Date().toISOString();
      if (overrideModal.kind === 'feature') {
        await api.put(`/platform/organizations/${id}/entitlements/${overrideModal.key}`, {
          enabled: overrideForm.enabled,
          startsAt: now,
          endsAt: null,
          reason: overrideForm.reason.trim(),
        });
      } else {
        const limitValue = overrideForm.limitValue === '' ? null : Number(overrideForm.limitValue);
        await api.put(`/platform/organizations/${id}/quotas/${overrideModal.key}`, {
          limitValue,
          startsAt: now,
          endsAt: null,
          reason: overrideForm.reason.trim(),
        });
      }
      setOverrideModal(null);
      setReloadKey((k) => k + 1);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : t('common.error'));
    } finally {
      setActionBusy(false);
    }
  }

  function openOverrideModal(kind: 'feature' | 'quota', key: string, current: boolean | number | null) {
    setOverrideModal({ kind, key });
    setOverrideForm({
      enabled: kind === 'feature' ? Boolean(current) : true,
      limitValue: kind === 'quota' && current !== null ? String(current) : '',
      reason: '',
    });
    setActionError(null);
  }

  const infoFields = organization
    ? [
        { label: t('orgDetail.country'), value: organization.countryCode },
        { label: t('orgDetail.region'), value: organization.region },
        { label: t('orgDetail.timezone'), value: organization.timezone },
        { label: t('orgDetail.locale'), value: organization.locale },
        { label: t('orgDetail.createdBy'), value: organization.createdByUserId },
      ]
    : [];

  return (
    <div>
      <button
        type="button"
        onClick={() => navigate('/organizations')}
        className="mb-4 inline-flex items-center gap-1.5 text-sm text-ink-500 transition-colors hover:text-ink-900"
      >
        <ArrowLeft size={16} />
        {t('orgDetail.back')}
      </button>

      {actionError && !confirmOpen ? (
        <div className="mb-4">
          <ErrorBanner message={actionError} />
        </div>
      ) : null}

      {loading ? (
        <Spinner />
      ) : error ? (
        <ErrorBanner message={error} onRetry={() => setReloadKey((key) => key + 1)} />
      ) : !organization ? (
        <Card>
          <EmptyState title={t('orgDetail.notFound')} />
        </Card>
      ) : (
        <>
          <Card className="p-5">
            <div className="flex flex-wrap items-start justify-between gap-4">
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <h1 className="text-xl font-semibold text-ink-900">{organization.name}</h1>
                  <StatusPill status={organization.status} />
                  <StatusPill status={organization.type} />
                </div>
                <p className="mt-1 font-mono text-xs text-ink-500">{organization.slug}</p>
                <p className="mt-1 text-xs text-ink-500">
                  {t('orgDetail.created')}: {formatDate(organization.createdAt)}
                </p>
              </div>

              <div>
                {organization.status === 'Active' ? (
                  <Button
                    variant="danger"
                    onClick={() => {
                      setActionError(null);
                      setConfirmOpen(true);
                    }}
                  >
                    {t('orgs.suspend')}
                  </Button>
                ) : organization.status === 'Suspended' ? (
                  <Button
                    variant="secondary"
                    disabled={actionBusy}
                    onClick={() => changeStatus('reactivate')}
                  >
                    {t('orgs.reactivate')}
                  </Button>
                ) : null}
              </div>
            </div>

            <div className="mt-4 flex gap-1 border-t border-ink-100 pt-4">
              <button
                type="button"
                onClick={() => setTab('overview')}
                className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                  tab === 'overview' ? 'bg-teal-50 text-teal-700' : 'text-ink-500 hover:text-ink-900'
                }`}
              >
                {t('orgDetail.tab.overview')}
              </button>
              <button
                type="button"
                onClick={() => setTab('members')}
                className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                  tab === 'members' ? 'bg-teal-50 text-teal-700' : 'text-ink-500 hover:text-ink-900'
                }`}
              >
                {t('orgDetail.tab.members')}
                <span className="ml-1.5 text-xs text-ink-400">{organization.members.length}</span>
              </button>
              <button
                type="button"
                onClick={() => setTab('entitlements')}
                className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                  tab === 'entitlements' ? 'bg-teal-50 text-teal-700' : 'text-ink-500 hover:text-ink-900'
                }`}
              >
                {t('orgDetail.tab.entitlements')}
              </button>
            </div>
          </Card>

          {tab === 'overview' ? (
            <Card className="mt-5 p-5">
              <h2 className="text-sm font-semibold text-ink-900">{t('orgDetail.infoTitle')}</h2>
              <dl className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {infoFields.map((field) => (
                  <div key={field.label}>
                    <dt className="text-xs font-medium uppercase tracking-wide text-ink-500">
                      {field.label}
                    </dt>
                    <dd className="mt-1 break-all text-sm text-ink-900">{field.value ?? '—'}</dd>
                  </div>
                ))}
              </dl>
            </Card>
          ) : tab === 'members' ? (
            <Card className="mt-5">
              <div className="flex items-center justify-between border-b border-ink-200 px-5 py-4">
                <h2 className="text-sm font-semibold text-ink-900">{t('orgDetail.membersTitle')}</h2>
                <span className="text-xs text-ink-500">{organization.members.length}</span>
              </div>
              {organization.members.length === 0 ? (
                <EmptyState title={t('orgDetail.noMembers')} />
              ) : (
                <Table
                  headers={[
                    t('orgDetail.memberEmail'),
                    t('orgDetail.memberName'),
                    t('orgDetail.memberRole'),
                    t('orgDetail.memberStatus'),
                    t('orgDetail.memberJoined'),
                  ]}
                >
                  {organization.members.map((member) => (
                    <tr key={member.userId}>
                      <td className="px-4 py-3 font-medium text-ink-900">{member.email}</td>
                      <td className="px-4 py-3 text-ink-600">{member.displayName}</td>
                      <td className="px-4 py-3 text-ink-600">{member.role}</td>
                      <td className="px-4 py-3">
                        <StatusPill status={member.status} />
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                        {formatDate(member.joinedAt)}
                      </td>
                    </tr>
                  ))}
                </Table>
              )}
            </Card>
          ) : (
            <div className="mt-5 space-y-5">
              <Card>
                <div className="flex items-center justify-between border-b border-ink-200 px-5 py-4">
                  <h2 className="text-sm font-semibold text-ink-900">{t('orgDetail.entitlementsTitle')}</h2>
                </div>
                <Table headers={[t('plans.featureKey'), t('plans.status'), t('orgDetail.effectiveUntil'), '']}>
                  {FEATURE_KEYS.map((key) => {
                    const ov = featureOverrides.find((o) => o.featureKey === key);
                    return (
                      <tr key={key}>
                        <td className="px-4 py-3 font-mono text-xs text-ink-900">{key}</td>
                        <td className="px-4 py-3">
                          {ov ? (
                            <StatusPill status={ov.enabled ? 'Granted' : 'Denied'} />
                          ) : (
                            <span className="text-xs text-ink-400">{t('orgDetail.noOverrides')}</span>
                          )}
                        </td>
                        <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                          {ov ? formatDateTime(ov.endsAt) : '—'}
                        </td>
                        <td className="px-4 py-3 text-right">
                          <Button
                            variant="secondary"
                            onClick={() =>
                              openOverrideModal('feature', key, ov?.enabled ?? true)
                            }
                          >
                            {t('orgDetail.setOverride')}
                          </Button>
                        </td>
                      </tr>
                    );
                  })}
                </Table>
              </Card>

              <Card>
                <div className="flex items-center justify-between border-b border-ink-200 px-5 py-4">
                  <h2 className="text-sm font-semibold text-ink-900">{t('orgDetail.quotasTitle')}</h2>
                </div>
                <Table headers={[t('plans.quotaKey'), t('plans.limit'), t('orgDetail.effectiveUntil'), '']}>
                  {QUOTA_KEYS.map((key) => {
                    const ov = quotaOverrides.find((o) => o.quotaKey === key);
                    return (
                      <tr key={key}>
                        <td className="px-4 py-3 font-mono text-xs text-ink-900">{key}</td>
                        <td className="px-4 py-3 text-ink-900">
                          {ov
                            ? ov.limitValue === null
                              ? t('plans.unlimited')
                              : ov.limitValue.toLocaleString()
                            : '—'}
                        </td>
                        <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                          {ov ? formatDateTime(ov.endsAt) : '—'}
                        </td>
                        <td className="px-4 py-3 text-right">
                          <Button
                            variant="secondary"
                            onClick={() => openOverrideModal('quota', key, ov?.limitValue ?? null)}
                          >
                            {t('orgDetail.setOverride')}
                          </Button>
                        </td>
                      </tr>
                    );
                  })}
                </Table>
              </Card>

              <Card>
                <div className="flex items-center justify-between border-b border-ink-200 px-5 py-4">
                  <h2 className="text-sm font-semibold text-ink-900">{t('orgDetail.auditTitle')}</h2>
                </div>
                {auditEvents.length === 0 ? (
                  <EmptyState title={t('orgDetail.noOverrides')} />
                ) : (
                  <Table
                    headers={[t('orgDetail.action'), t('orgDetail.occurredAt'), t('subs.reason'), t('orgDetail.requestId')]}
                  >
                    {auditEvents.map((evt) => (
                      <tr key={evt.id}>
                        <td className="px-4 py-3 font-medium text-ink-900">{evt.action}</td>
                        <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                          {formatDateTime(evt.occurredAt)}
                        </td>
                        <td className="px-4 py-3 text-ink-600">{evt.reason}</td>
                        <td className="px-4 py-3 font-mono text-xs text-ink-400">{evt.requestId ?? '—'}</td>
                      </tr>
                    ))}
                  </Table>
                )}
              </Card>
            </div>
          )}
        </>
      )}

      <Modal
        open={confirmOpen}
        title={t('orgs.suspendTitle')}
        onClose={() => setConfirmOpen(false)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setConfirmOpen(false)}>
              {t('common.cancel')}
            </Button>
            <Button variant="danger" disabled={actionBusy} onClick={() => changeStatus('suspend')}>
              {t('orgs.suspend')}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-600">{t('orgs.suspendBody')}</p>
        {organization ? (
          <p className="mt-2 text-sm font-semibold text-ink-900">{organization.name}</p>
        ) : null}
        {actionError ? <p className="mt-3 text-sm text-invalid-600">{actionError}</p> : null}
      </Modal>

      <Modal
        open={overrideModal !== null}
        title={overrideModal ? `${t('orgDetail.setOverride')}: ${overrideModal.key}` : ''}
        onClose={() => setOverrideModal(null)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setOverrideModal(null)} disabled={actionBusy}>
              {t('common.cancel')}
            </Button>
            <Button
              disabled={actionBusy || !overrideForm.reason.trim()}
              onClick={submitOverride}
            >
              {t('common.save')}
            </Button>
          </>
        }
      >
        {overrideModal?.kind === 'feature' ? (
          <div className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-ink-700">{t('plans.enabled')}</label>
              <Select
                value={overrideForm.enabled ? 'true' : 'false'}
                onChange={(e) => setOverrideForm((f) => ({ ...f, enabled: e.target.value === 'true' }))}
              >
                <option value="true">{t('orgDetail.overrideGrant')}</option>
                <option value="false">{t('orgDetail.overrideDeny')}</option>
              </Select>
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-ink-700">{t('plans.limit')}</label>
              <Input
                type="number"
                placeholder={t('plans.unlimited')}
                value={overrideForm.limitValue}
                onChange={(e) => setOverrideForm((f) => ({ ...f, limitValue: e.target.value }))}
              />
              <p className="mt-1 text-xs text-ink-400">{t('orgDetail.noOverrides')}</p>
            </div>
          </div>
        )}
        <div className="mt-4">
          <label className="mb-1 block text-sm font-medium text-ink-700">{t('subs.reason')}</label>
          <Textarea
            value={overrideForm.reason}
            onChange={(e) => setOverrideForm((f) => ({ ...f, reason: e.target.value }))}
            placeholder={t('subs.reasonPlaceholder')}
          />
        </div>
        {actionError ? <p className="mt-3 text-sm text-invalid-600">{actionError}</p> : null}
      </Modal>
    </div>
  );
}