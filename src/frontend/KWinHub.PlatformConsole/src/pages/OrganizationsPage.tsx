import { useEffect, useState } from 'react';
import { api, type PlatformOrganizationResponse } from '../lib/api';
import { useI18n } from '../lib/i18n';
import { navigate, useHashRoute } from '../lib/router';
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

export function OrganizationsPage() {
  const { t } = useI18n();
  const route = useHashRoute();

  const [organizations, setOrganizations] = useState<PlatformOrganizationResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [status, setStatus] = useState(route.query.get('status') ?? '');
  const [type, setType] = useState('');
  const [reloadKey, setReloadKey] = useState(0);

  const [pendingSuspend, setPendingSuspend] = useState<PlatformOrganizationResponse | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionBusy, setActionBusy] = useState(false);

  useEffect(() => {
    const timeout = window.setTimeout(() => setDebouncedSearch(search.trim()), 300);
    return () => window.clearTimeout(timeout);
  }, [search]);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    const params = new URLSearchParams();
    if (debouncedSearch) params.set('search', debouncedSearch);
    if (status) params.set('status', status);
    if (type) params.set('type', type);
    const query = params.toString();

    api
      .get<PlatformOrganizationResponse[]>(`/platform/organizations${query ? `?${query}` : ''}`)
      .then((response) => {
        if (cancelled) return;
        setOrganizations(response);
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
  }, [debouncedSearch, status, type, reloadKey]);

  async function changeStatus(organizationId: string, action: 'suspend' | 'reactivate') {
    setActionError(null);
    setActionBusy(true);

    try {
      await api.changeStatus<void>(`/platform/organizations/${organizationId}/status`, action);
      setPendingSuspend(null);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : t('orgs.actionFailed'));
    } finally {
      setActionBusy(false);
    }
  }

  const hasFilters = debouncedSearch !== '' || status !== '' || type !== '';

  return (
    <div>
      <PageHeader title={t('orgs.title')} subtitle={t('orgs.subtitle')} />

      {actionError && !pendingSuspend ? (
        <div className="mb-4">
          <ErrorBanner message={actionError} />
        </div>
      ) : null}

      {error ? (
        <div className="mb-4">
          <ErrorBanner message={error} onRetry={() => setReloadKey((key) => key + 1)} />
        </div>
      ) : null}

      <Card>
        <div className="flex flex-wrap items-center gap-2 border-b border-ink-200 p-4">
          <Input
            className="max-w-xs"
            placeholder={t('orgs.searchPlaceholder')}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <Select className="max-w-[200px]" value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="">{t('orgs.allStatuses')}</option>
            <option value="Active">{t('status.active')}</option>
            <option value="Suspended">{t('status.suspended')}</option>
          </Select>
          <Select className="max-w-[200px]" value={type} onChange={(event) => setType(event.target.value)}>
            <option value="">{t('orgs.allTypes')}</option>
            <option value="Standard">{t('status.standard')}</option>
            <option value="Individual">{t('status.individual')}</option>
          </Select>
        </div>

        {loading ? (
          <Spinner />
        ) : organizations.length === 0 ? (
          <EmptyState
            title={hasFilters ? t('orgs.noResults') : t('orgs.empty')}
            description={hasFilters ? undefined : t('orgs.subtitle')}
          />
        ) : (
          <Table
            headers={[
              t('orgs.name'),
              t('orgs.type'),
              t('orgs.status'),
              t('orgs.country'),
              t('orgs.members'),
              t('orgs.subjects'),
              t('orgs.sessions'),
              t('common.created'),
              t('common.actions'),
            ]}
          >
            {organizations.map((organization) => (
              <tr
                key={organization.id}
                onClick={() => navigate(`/organizations/${organization.id}`)}
                className="cursor-pointer transition-colors hover:bg-ink-50"
              >
                <td className="px-4 py-3 font-medium text-ink-900">{organization.name}</td>
                <td className="px-4 py-3">
                  <StatusPill status={organization.type} />
                </td>
                <td className="px-4 py-3">
                  <StatusPill status={organization.status} />
                </td>
                <td className="px-4 py-3 text-ink-600">{organization.countryCode ?? '—'}</td>
                <td className="px-4 py-3 text-ink-600">{organization.memberCount}</td>
                <td className="px-4 py-3 text-ink-600">{organization.subjectCount}</td>
                <td className="px-4 py-3 text-ink-600">{organization.sessionCount}</td>
                <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                  {formatDate(organization.createdAt)}
                </td>
                <td className="px-4 py-3" onClick={(event) => event.stopPropagation()}>
                  {organization.status === 'Active' ? (
                    <Button
                      variant="danger"
                      onClick={() => {
                        setActionError(null);
                        setPendingSuspend(organization);
                      }}
                    >
                      {t('orgs.suspend')}
                    </Button>
                  ) : organization.status === 'Suspended' ? (
                    <Button
                      variant="secondary"
                      disabled={actionBusy}
                      onClick={() => changeStatus(organization.id, 'reactivate')}
                    >
                      {t('orgs.reactivate')}
                    </Button>
                  ) : (
                    <span className="text-xs text-ink-400">—</span>
                  )}
                </td>
              </tr>
            ))}
          </Table>
        )}
      </Card>

      <Modal
        open={pendingSuspend !== null}
        title={t('orgs.suspendTitle')}
        onClose={() => setPendingSuspend(null)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setPendingSuspend(null)}>
              {t('common.cancel')}
            </Button>
            <Button
              variant="danger"
              disabled={actionBusy}
              onClick={() => {
                if (pendingSuspend) changeStatus(pendingSuspend.id, 'suspend');
              }}
            >
              {t('orgs.suspend')}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-600">{t('orgs.suspendBody')}</p>
        {pendingSuspend ? (
          <p className="mt-2 text-sm font-semibold text-ink-900">{pendingSuspend.name}</p>
        ) : null}
        {actionError ? <p className="mt-3 text-sm text-invalid-600">{actionError}</p> : null}
      </Modal>
    </div>
  );
}