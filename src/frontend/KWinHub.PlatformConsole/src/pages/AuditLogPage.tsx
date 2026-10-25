import { useEffect, useMemo, useState } from 'react';
import { Download } from 'lucide-react';
import { api, type AuditLogResponse } from '../lib/api';
import { useI18n, type TranslationKey } from '../lib/i18n';
import {
  Button,
  Card,
  EmptyState,
  ErrorBanner,
  Input,
  PageHeader,
  Select,
  Spinner,
  Table,
  formatDateTime,
} from '../components/ui';

const ENTITY_OPTIONS: { value: string; labelKey: TranslationKey }[] = [
  { value: '', labelKey: 'audit.allEntityTypes' },
  { value: 'Organization', labelKey: 'audit.entity.organization' },
  { value: 'User', labelKey: 'audit.entity.user' },
  { value: 'GlobalProtocol', labelKey: 'audit.entity.globalProtocol' },
  { value: 'GlobalMetric', labelKey: 'audit.entity.globalMetric' },
  { value: 'DeviceModel', labelKey: 'audit.entity.deviceModel' },
  { value: 'AlgorithmVersion', labelKey: 'audit.entity.algorithmVersion' },
];

const ENTITY_LABELS: Record<string, TranslationKey> = {
  Organization: 'audit.entity.organization',
  User: 'audit.entity.user',
  GlobalProtocol: 'audit.entity.globalProtocol',
  GlobalMetric: 'audit.entity.globalMetric',
  DeviceModel: 'audit.entity.deviceModel',
  AlgorithmVersion: 'audit.entity.algorithmVersion',
};

export function AuditLogPage() {
  const { t } = useI18n();

  const [logs, setLogs] = useState<AuditLogResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [entityType, setEntityType] = useState('');
  const [search, setSearch] = useState('');
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    const params = new URLSearchParams();
    params.set('take', '200');
    if (entityType) params.set('entityType', entityType);

    api
      .get<AuditLogResponse[]>(`/platform/audit-logs?${params.toString()}`)
      .then((response) => {
        if (cancelled) return;
        setLogs(response);
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
  }, [entityType, reloadKey]);

  const filteredLogs = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return logs;
    return logs.filter(
      (log) =>
        (log.actorEmail ?? '').toLowerCase().includes(q) ||
        log.action.toLowerCase().includes(q) ||
        (log.summary ?? '').toLowerCase().includes(q) ||
        (log.entityId ?? '').toLowerCase().includes(q)
    );
  }, [logs, search]);

  function exportCsv() {
    if (filteredLogs.length === 0) return;
    const headers = ['Time', 'Actor', 'Action', 'Entity Type', 'Entity ID', 'Summary'];
    const rows = filteredLogs.map((log) => [
      formatDateTime(log.createdAt),
      log.actorEmail ?? t('audit.system'),
      log.action,
      log.entityType,
      log.entityId ?? '',
      (log.summary ?? '').replace(/"/g, '""'),
    ]);
    const csv = [
      headers.join(','),
      ...rows.map((row) => row.map((cell) => `"${cell}"`).join(',')),
    ].join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `audit-log-${new Date().toISOString().slice(0, 10)}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  return (
    <div>
      <PageHeader title={t('audit.title')} subtitle={t('audit.subtitle')} />

      {error ? (
        <div className="mb-4">
          <ErrorBanner message={error} onRetry={() => setReloadKey((key) => key + 1)} />
        </div>
      ) : null}

      <Card>
        <div className="flex flex-wrap items-center gap-2 border-b border-ink-200 p-4">
          <Input
            className="max-w-xs"
            placeholder={t('audit.search')}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <Select
            className="max-w-[220px]"
            value={entityType}
            onChange={(event) => setEntityType(event.target.value)}
          >
            {ENTITY_OPTIONS.map((option) => (
              <option key={option.value} value={option.value}>
                {t(option.labelKey)}
              </option>
            ))}
          </Select>
          <div className="ml-auto">
            <Button
              variant="secondary"
              onClick={exportCsv}
              disabled={filteredLogs.length === 0}
            >
              <Download size={14} />
              {t('audit.export')}
            </Button>
          </div>
        </div>

        {loading ? (
          <Spinner />
        ) : filteredLogs.length === 0 ? (
          <EmptyState title={search ? t('common.noResults') : t('audit.empty')} />
        ) : (
          <Table
            headers={[
              t('audit.time'),
              t('audit.actor'),
              t('audit.action'),
              t('audit.entityType'),
              t('audit.entityId'),
              t('audit.summary'),
            ]}
          >
            {filteredLogs.map((log) => {
              const entityLabelKey = ENTITY_LABELS[log.entityType];
              return (
                <tr key={log.id}>
                  <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                    {formatDateTime(log.createdAt)}
                  </td>
                  <td className="px-4 py-3 text-ink-700">
                    {log.actorEmail ?? t('audit.system')}
                  </td>
                  <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-ink-700">
                    {log.action}
                  </td>
                  <td className="whitespace-nowrap px-4 py-3 text-ink-600">
                    {entityLabelKey ? t(entityLabelKey) : log.entityType}
                  </td>
                  <td className="max-w-[180px] truncate px-4 py-3 font-mono text-xs text-ink-500">
                    {log.entityId ?? '—'}
                  </td>
                  <td className="px-4 py-3 text-ink-600">{log.summary}</td>
                </tr>
              );
            })}
          </Table>
        )}
      </Card>
    </div>
  );
}