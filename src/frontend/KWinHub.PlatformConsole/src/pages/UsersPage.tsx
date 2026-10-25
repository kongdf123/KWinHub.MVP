import { useEffect, useState } from 'react';
import { CheckCircle2, XCircle } from 'lucide-react';
import { api, type PlatformUserResponse } from '../lib/api';
import { useI18n, type TranslationKey } from '../lib/i18n';
import { useHashRoute } from '../lib/router';
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
  formatDateTime,
} from '../components/ui';

type UserAction = 'suspend' | 'activate' | 'grant-admin' | 'revoke-admin';

const ACTION_LABELS: Record<UserAction, TranslationKey> = {
  suspend: 'users.suspend',
  activate: 'users.activate',
  'grant-admin': 'users.grantAdmin',
  'revoke-admin': 'users.revokeAdmin',
};

const ACTION_CONFIRMATIONS: Record<UserAction, TranslationKey> = {
  suspend: 'users.confirmSuspend',
  activate: 'users.confirmActivate',
  'grant-admin': 'users.confirmGrant',
  'revoke-admin': 'users.confirmRevoke',
};

export function UsersPage() {
  const { t } = useI18n();
  const route = useHashRoute();

  const [users, setUsers] = useState<PlatformUserResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [status, setStatus] = useState(route.query.get('status') ?? '');
  const [reloadKey, setReloadKey] = useState(0);

  const [pending, setPending] = useState<{ user: PlatformUserResponse; action: UserAction } | null>(
    null
  );
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
    const query = params.toString();

    api
      .get<PlatformUserResponse[]>(`/platform/users${query ? `?${query}` : ''}`)
      .then((response) => {
        if (cancelled) return;
        setUsers(response);
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
  }, [debouncedSearch, status, reloadKey]);

  async function runAction(userId: string, action: UserAction) {
    setActionError(null);
    setActionBusy(true);

    try {
      await api.changeStatus<void>(`/platform/users/${userId}/status`, action);
      setPending(null);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : t('users.actionFailed'));
    } finally {
      setActionBusy(false);
    }
  }

  const hasFilters = debouncedSearch !== '' || status !== '';

  return (
    <div>
      <PageHeader title={t('users.title')} subtitle={t('users.subtitle')} />

      {actionError && !pending ? (
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
            placeholder={t('users.searchPlaceholder')}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <Select className="max-w-[200px]" value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="">{t('users.allStatuses')}</option>
            <option value="Active">{t('status.active')}</option>
            <option value="Suspended">{t('status.suspended')}</option>
            <option value="Disabled">{t('status.disabled')}</option>
            <option value="PendingEmailVerification">{t('status.pendingEmailVerification')}</option>
          </Select>
        </div>

        {loading ? (
          <Spinner />
        ) : users.length === 0 ? (
          <EmptyState title={hasFilters ? t('users.noResults') : t('users.empty')} />
        ) : (
          <Table
            headers={[
              t('users.email'),
              t('users.displayName'),
              t('users.status'),
              t('users.platformRole'),
              t('users.emailVerified'),
              t('users.organizations'),
              t('users.lastLogin'),
              t('common.created'),
              t('common.actions'),
            ]}
          >
            {users.map((user) => (
              <tr key={user.id}>
                <td className="px-4 py-3 font-medium text-ink-900">{user.email}</td>
                <td className="px-4 py-3 text-ink-600">{user.displayName}</td>
                <td className="px-4 py-3">
                  <StatusPill status={user.status} />
                </td>
                <td className="px-4 py-3">
                  {user.platformRole ? (
                    <span className="rounded-full bg-teal-50 px-2 py-0.5 text-xs font-medium text-teal-700">
                      {user.platformRole === 'PlatformAdmin'
                        ? t('users.platformAdmin')
                        : user.platformRole}
                    </span>
                  ) : (
                    <span className="text-xs text-ink-400">—</span>
                  )}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center gap-1 text-xs font-medium ${
                      user.emailVerified ? 'text-success-700' : 'text-ink-400'
                    }`}
                  >
                    {user.emailVerified ? <CheckCircle2 size={14} /> : <XCircle size={14} />}
                    {user.emailVerified ? t('users.verified') : t('users.unverified')}
                  </span>
                </td>
                <td className="px-4 py-3 text-ink-600">{user.organizationCount}</td>
                <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                  {user.lastLoginAt ? formatDateTime(user.lastLoginAt) : t('users.never')}
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                  {formatDate(user.createdAt)}
                </td>
                <td className="px-4 py-3">
                  <div className="flex flex-wrap items-center gap-1.5">
                    {user.status === 'Active' ? (
                      <Button
                        variant="danger"
                        onClick={() => {
                          setActionError(null);
                          setPending({ user, action: 'suspend' });
                        }}
                      >
                        {t('users.suspend')}
                      </Button>
                    ) : (
                      <Button
                        variant="secondary"
                        onClick={() => {
                          setActionError(null);
                          setPending({ user, action: 'activate' });
                        }}
                      >
                        {t('users.activate')}
                      </Button>
                    )}

                    {user.platformRole ? (
                      <Button
                        variant="danger"
                        onClick={() => {
                          setActionError(null);
                          setPending({ user, action: 'revoke-admin' });
                        }}
                      >
                        {t('users.revokeAdmin')}
                      </Button>
                    ) : (
                      <Button
                        variant="secondary"
                        onClick={() => {
                          setActionError(null);
                          setPending({ user, action: 'grant-admin' });
                        }}
                      >
                        {t('users.grantAdmin')}
                      </Button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </Table>
        )}
      </Card>

      <Modal
        open={pending !== null}
        title={t('users.confirmTitle')}
        onClose={() => setPending(null)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setPending(null)}>
              {t('common.cancel')}
            </Button>
            <Button
              variant={pending?.action === 'suspend' || pending?.action === 'revoke-admin' ? 'danger' : 'primary'}
              disabled={actionBusy}
              onClick={() => {
                if (pending) runAction(pending.user.id, pending.action);
              }}
            >
              {pending ? t(ACTION_LABELS[pending.action]) : t('common.confirm')}
            </Button>
          </>
        }
      >
        {pending ? (
          <>
            <p className="text-sm text-ink-600">{t(ACTION_CONFIRMATIONS[pending.action])}</p>
            <p className="mt-2 text-sm font-semibold text-ink-900">{pending.user.email}</p>
          </>
        ) : null}
        {actionError ? <p className="mt-3 text-sm text-invalid-600">{actionError}</p> : null}
      </Modal>
    </div>
  );
}