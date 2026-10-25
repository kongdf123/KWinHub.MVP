import { useEffect, useState } from 'react';
import { ArrowLeft } from 'lucide-react';
import { api, type PlatformOrganizationDetailResponse } from '../lib/api';
import { useI18n } from '../lib/i18n';
import { navigate } from '../lib/router';
import {
  Button,
  Card,
  EmptyState,
  ErrorBanner,
  Modal,
  Spinner,
  StatusPill,
  Table,
  formatDate,
} from '../components/ui';

type Tab = 'overview' | 'members';

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
          ) : (
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
    </div>
  );
}