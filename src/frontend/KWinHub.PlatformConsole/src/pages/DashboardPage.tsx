import { useEffect, useState } from 'react';
import {
  AlertTriangle,
  ArrowRight,
  Ban,
  Building2,
  CheckCircle2,
  ShieldAlert,
  UserRound,
  Users,
} from 'lucide-react';
import { api, type PlatformDashboardResponse } from '../lib/api';
import { useI18n } from '../lib/i18n';
import { navigate } from '../lib/router';
import {
  Card,
  EmptyState,
  ErrorBanner,
  PageHeader,
  Spinner,
  StatCard,
  StatusPill,
  Table,
  formatDate,
  formatDateTime,
} from '../components/ui';

export function DashboardPage() {
  const { t } = useI18n();
  const [data, setData] = useState<PlatformDashboardResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    api
      .get<PlatformDashboardResponse>('/platform/dashboard')
      .then((response) => {
        if (cancelled) return;
        setData(response);
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
  }, [reloadKey]);

  return (
    <div>
      <PageHeader title={t('dashboard.title')} subtitle={t('dashboard.subtitle')} />

      {loading ? (
        <Spinner />
      ) : error ? (
        <ErrorBanner message={error} onRetry={() => setReloadKey((key) => key + 1)} />
      ) : data ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            <StatCard
              label={t('dashboard.totalOrganizations')}
              value={data.totalOrganizations}
              icon={<Building2 size={18} />}
              accent="teal"
            />
            <StatCard
              label={t('dashboard.activeOrganizations')}
              value={data.activeOrganizations}
              icon={<CheckCircle2 size={18} />}
              accent="success"
            />
            <StatCard
              label={t('dashboard.suspendedOrganizations')}
              value={data.suspendedOrganizations}
              icon={<Ban size={18} />}
              accent="warning"
            />
            <StatCard
              label={t('dashboard.individualOrganizations')}
              value={data.individualOrganizations}
              icon={<UserRound size={18} />}
              accent="ink"
            />
            <StatCard
              label={t('dashboard.totalUsers')}
              value={data.totalUsers}
              icon={<Users size={18} />}
              accent="teal"
            />
            <StatCard
              label={t('dashboard.pendingVerification')}
              value={data.pendingVerificationUsers}
              icon={<AlertTriangle size={18} />}
              accent="warning"
            />
          </div>

          <div className="mt-5 grid gap-4 sm:grid-cols-2">
            {(data.suspendedOrganizations > 0 || data.pendingVerificationUsers > 0) && (
              <Card className="p-5">
                <div className="flex items-center gap-2">
                  <AlertTriangle size={16} className="text-attention-600" />
                  <h2 className="text-sm font-semibold text-ink-900">{t('dashboard.needsAttention')}</h2>
                </div>
                <div className="mt-4 space-y-3">
                  {data.suspendedOrganizations > 0 && (
                    <button
                      type="button"
                      onClick={() => navigate('/organizations?status=Suspended')}
                      className="flex w-full items-center justify-between rounded-md border border-ink-200 px-4 py-3 text-left transition-colors hover:bg-ink-50"
                    >
                      <span className="flex items-center gap-2 text-sm text-ink-700">
                        <Ban size={15} className="text-attention-600" />
                        <span className="font-semibold text-ink-900">{data.suspendedOrganizations}</span>
                        {t('dashboard.suspendedOrgs')}
                      </span>
                      <ArrowRight size={15} className="text-ink-400" />
                    </button>
                  )}
                  {data.pendingVerificationUsers > 0 && (
                    <button
                      type="button"
                      onClick={() => navigate('/users?status=PendingEmailVerification')}
                      className="flex w-full items-center justify-between rounded-md border border-ink-200 px-4 py-3 text-left transition-colors hover:bg-ink-50"
                    >
                      <span className="flex items-center gap-2 text-sm text-ink-700">
                        <Users size={15} className="text-attention-600" />
                        <span className="font-semibold text-ink-900">{data.pendingVerificationUsers}</span>
                        {t('dashboard.pendingUsers')}
                      </span>
                      <ArrowRight size={15} className="text-ink-400" />
                    </button>
                  )}
                </div>
              </Card>
            )}

            <Card className="p-5">
              <div className="flex items-center gap-2">
                <ShieldAlert size={16} className="text-teal-600" />
                <h2 className="text-sm font-semibold text-ink-900">{t('dashboard.securityEvents')}</h2>
              </div>
              {data.recentAuditLogs.length === 0 ? (
                <EmptyState title={t('dashboard.noAuditEvents')} />
              ) : (
                <ul className="mt-3 space-y-2">
                  {data.recentAuditLogs
                    .filter((log) =>
                      /(status|suspend|admin|delete|create|grant|revoke)/i.test(log.action)
                    )
                    .slice(0, 4)
                    .map((log) => (
                      <li key={log.id} className="flex items-start gap-3 rounded-md px-2 py-1.5 hover:bg-ink-50">
                        <span className="mt-0.5 h-1.5 w-1.5 shrink-0 rounded-full bg-teal-500" />
                        <div className="min-w-0 flex-1">
                          <p className="truncate font-mono text-xs text-ink-700">{log.action}</p>
                          <p className="mt-0.5 truncate text-xs text-ink-400">
                            {log.actorEmail ?? t('audit.system')} · {formatDateTime(log.createdAt)}
                          </p>
                        </div>
                      </li>
                    ))}
                  {data.recentAuditLogs.some((log) =>
                    /(status|suspend|admin|delete|create|grant|revoke)/i.test(log.action)
                  ) ? null : (
                    <li className="px-2 py-1.5 text-xs text-ink-400">{t('dashboard.noAuditEvents')}</li>
                  )}
                </ul>
              )}
            </Card>
          </div>

          <div className="mt-5 grid gap-5 xl:grid-cols-2">
            <Card>
              <div className="border-b border-ink-200 px-5 py-4">
                <h2 className="text-sm font-semibold text-ink-900">
                  {t('dashboard.recentOrganizations')}
                </h2>
              </div>
              {data.recentOrganizations.length === 0 ? (
                <EmptyState title={t('dashboard.noOrganizations')} />
              ) : (
                <Table
                  headers={[
                    t('orgs.name'),
                    t('orgs.type'),
                    t('orgs.status'),
                    t('common.created'),
                  ]}
                >
                  {data.recentOrganizations.map((organization) => (
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
                      <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                        {formatDate(organization.createdAt)}
                      </td>
                    </tr>
                  ))}
                </Table>
              )}
            </Card>

            <Card>
              <div className="border-b border-ink-200 px-5 py-4">
                <h2 className="text-sm font-semibold text-ink-900">
                  {t('dashboard.recentAuditEvents')}
                </h2>
              </div>
              {data.recentAuditLogs.length === 0 ? (
                <EmptyState title={t('dashboard.noAuditEvents')} />
              ) : (
                <ul className="divide-y divide-ink-100">
                  {data.recentAuditLogs.map((log) => (
                    <li key={log.id} className="px-5 py-3">
                      <div className="flex items-center justify-between gap-3">
                        <span className="truncate font-mono text-xs text-ink-700">{log.action}</span>
                        <span className="shrink-0 text-xs text-ink-400">
                          {formatDateTime(log.createdAt)}
                        </span>
                      </div>
                      <p className="mt-1 text-sm text-ink-600">{log.summary}</p>
                      <p className="mt-0.5 text-xs text-ink-400">
                        {log.actorEmail ?? t('audit.system')}
                      </p>
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          </div>
        </>
      ) : null}
    </div>
  );
}