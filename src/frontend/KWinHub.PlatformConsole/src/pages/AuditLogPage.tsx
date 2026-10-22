import { useEffect, useState } from 'react';
import { api, type AuditLogResponse } from '../lib/api';
import { useI18n, type TranslationKey } from '../lib/i18n';
import {
  Card,
  EmptyState,
  ErrorBanner,
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

  return (
    <div>
      <PageHeader title={t('audit.title')} subtitle={t('audit.subtitle')} />

      {error ? (
        <div className="mb-4">
          <ErrorBanner message={error} onRetry={() => setReloadKey((key) => key + 1)} />
        </div>
      ) : null}

      <Card>
        <div className="border-b border-ink-200 p-4">
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
        </div>

        {loading ? (
          <Spinner />
        ) : logs.length === 0 ? (
          <EmptyState title={t('audit.empty')} />
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
            {logs.map((log) => {
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
