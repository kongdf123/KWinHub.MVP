import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { Plus } from 'lucide-react';
import { api, type AlgorithmVersionResponse } from '../lib/api';
import { ALGORITHM_SCIENTIFIC_META, type AlgorithmScientificMeta } from '../lib/mockApi';
import { useI18n } from '../lib/i18n';
import { CatalogTabs } from '../components/CatalogTabs';
import {
  Button,
  Card,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  PageHeader,
  Spinner,
  StatusPill,
  Table,
  Textarea,
  formatDate,
} from '../components/ui';

const BASE = '/platform/catalogs/algorithms';

const EMPTY_FORM = {
  name: '',
  version: '1.0.0',
  supportedMetricCodes: '',
  notes: '',
};

export function AlgorithmVersionsPage() {
  const { t } = useI18n();

  const [items, setItems] = useState<AlgorithmVersionResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);
  const [search, setSearch] = useState('');
  const [protocolFilter, setProtocolFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<AlgorithmVersionResponse | null>(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const [deleteTarget, setDeleteTarget] = useState<AlgorithmVersionResponse | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionBusy, setActionBusy] = useState(false);

  const [detailTarget, setDetailTarget] = useState<AlgorithmVersionResponse | null>(null);
  const [validatedIds, setValidatedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    api
      .get<AlgorithmVersionResponse[]>(BASE)
      .then((response) => {
        if (cancelled) return;
        setItems(response);
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

  function openCreate() {
    setEditing(null);
    setForm(EMPTY_FORM);
    setFormError(null);
    setFormOpen(true);
  }

  function openEdit(item: AlgorithmVersionResponse) {
    setEditing(item);
    setForm({
      name: item.name,
      version: item.version,
      supportedMetricCodes: item.supportedMetricCodes ?? '',
      notes: item.notes ?? '',
    });
    setFormError(null);
    setFormOpen(true);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!form.name.trim() || !form.version.trim()) {
      setFormError(t('catalog.required'));
      return;
    }

    setFormError(null);
    setSaving(true);

    const body = {
      name: form.name.trim(),
      version: form.version.trim(),
      supportedMetricCodes: form.supportedMetricCodes.trim() || null,
      notes: form.notes.trim() || null,
    };

    try {
      if (editing) {
        await api.put<AlgorithmVersionResponse>(`${BASE}/${editing.id}`, body);
      } else {
        await api.post<AlgorithmVersionResponse>(BASE, body);
      }
      setFormOpen(false);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : t('common.error'));
    } finally {
      setSaving(false);
    }
  }

  async function changeStatus(item: AlgorithmVersionResponse, action: 'publish' | 'archive') {
    setActionError(null);
    setActionBusy(true);

    try {
      await api.changeStatus<void>(`${BASE}/${item.id}/status`, action);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : t('catalog.actionFailed'));
    } finally {
      setActionBusy(false);
    }
  }

  async function handleDelete() {
    if (!deleteTarget) return;
    setActionError(null);
    setActionBusy(true);

    try {
      await api.del(`${BASE}/${deleteTarget.id}`);
      setDeleteTarget(null);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : t('catalog.actionFailed'));
    } finally {
      setActionBusy(false);
    }
  }

  function getMeta(item: AlgorithmVersionResponse): AlgorithmScientificMeta | undefined {
    return item.code ? ALGORITHM_SCIENTIFIC_META[item.code] : undefined;
  }

  function getDomainLabel(item: AlgorithmVersionResponse): string {
    const meta = getMeta(item);
    if (meta) return meta.domain;
    return 'Other';
  }

  function getProtocolCodes(item: AlgorithmVersionResponse): string[] {
    return getMeta(item)?.supportedProtocols ?? [];
  }

  function getDisplayStatus(item: AlgorithmVersionResponse): string {
    if (item.status === 'Draft' && validatedIds.has(item.id)) {
      return t('algorithms.validated');
    }
    return item.status;
  }

  function handleDemoValidate(item: AlgorithmVersionResponse) {
    setActionError(null);
    setValidatedIds((prev) => new Set(prev).add(item.id));
  }

  async function handleDemoApprovePublish(item: AlgorithmVersionResponse) {
    setActionError(null);
    setActionBusy(true);
    try {
      await api.changeStatus<void>(`${BASE}/${item.id}/status`, 'publish');
      setValidatedIds((prev) => {
        const next = new Set(prev);
        next.delete(item.id);
        return next;
      });
      setReloadKey((key) => key + 1);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : t('catalog.actionFailed'));
    } finally {
      setActionBusy(false);
    }
  }

  const availableProtocols = useMemo(() => {
    const set = new Set<string>();
    items.forEach((item) => {
      getProtocolCodes(item).forEach((code) => set.add(code));
    });
    return Array.from(set).sort();
  }, [items]);

  const query = search.trim().toLowerCase();
  const filtered = items.filter((item) => {
    if (query) {
      const matchesQuery =
        (item.code ?? '').toLowerCase().includes(query) ||
        item.name.toLowerCase().includes(query) ||
        item.version.toLowerCase().includes(query);
      if (!matchesQuery) return false;
    }
    if (protocolFilter) {
      const protocols = getProtocolCodes(item);
      if (!protocols.includes(protocolFilter)) return false;
    }
    if (statusFilter) {
      if (getDisplayStatus(item) !== statusFilter) return false;
    }
    return true;
  });

  return (
    <div>
      <PageHeader
        title={t('algorithms.title')}
        subtitle={t('algorithms.subtitle')}
        actions={
          <Button onClick={openCreate}>
            <Plus size={16} />
            {t('catalog.new')}
          </Button>
        }
      />

      <CatalogTabs />

      {actionError && !deleteTarget ? (
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
        <div className="flex flex-wrap items-center gap-3 border-b border-ink-200 p-4">
          <Input
            className="max-w-xs"
            placeholder={t('catalog.searchPlaceholder')}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <select
            className="rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm text-ink-700 focus:outline-none focus:ring-2 focus:ring-accent"
            value={protocolFilter}
            onChange={(event) => setProtocolFilter(event.target.value)}
          >
            <option value="">{t('algorithms.filterByProtocol')}</option>
            {availableProtocols.map((code) => (
              <option key={code} value={code}>{code}</option>
            ))}
          </select>
          <select
            className="rounded-lg border border-ink-200 bg-white px-3 py-2 text-sm text-ink-700 focus:outline-none focus:ring-2 focus:ring-accent"
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value)}
          >
            <option value="">{t('algorithms.filterByStatus')}</option>
            {['Draft', t('algorithms.validated'), 'Published', 'Archived'].map((status) => (
              <option key={status} value={status}>{status}</option>
            ))}
          </select>
        </div>

        {loading ? (
          <Spinner />
        ) : filtered.length === 0 ? (
          <EmptyState title={query || protocolFilter || statusFilter ? t('catalog.noResults') : t('catalog.empty')} />
        ) : (
          <Table
            headers={[
              t('catalog.code'),
              t('algorithms.domain'),
              t('catalog.name'),
              t('catalog.version'),
              t('algorithms.protocol'),
              t('algorithms.outputs'),
              t('catalog.status'),
              t('catalog.created'),
              t('catalog.actions'),
            ]}
          >
            {filtered.map((item) => {
              const meta = getMeta(item);
              const protocols = getProtocolCodes(item);
              const isDraft = item.status === 'Draft';
              const isValidated = validatedIds.has(item.id);
              const displayStatus = getDisplayStatus(item);
              return (
                <tr key={item.id}>
                  <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-ink-700">
                    {item.code ?? '—'}
                  </td>
                  <td className="whitespace-nowrap px-4 py-3">
                    {meta ? (
                      <span className="inline-flex items-center rounded-full border border-teal-200 bg-teal-50 px-2 py-0.5 text-xs font-medium text-teal-700">
                        {meta.domain}
                      </span>
                    ) : (
                      <span className="text-xs text-ink-400">—</span>
                    )}
                  </td>
                  <td className="px-4 py-3 font-medium text-ink-900">{item.name}</td>
                  <td className="px-4 py-3 text-ink-600">{item.version}</td>
                  <td className="max-w-[12rem] truncate px-4 py-3 font-mono text-xs text-ink-600">
                    {protocols.length > 0 ? protocols.join(', ') : '—'}
                  </td>
                  <td className="px-4 py-3 text-center text-sm text-ink-600">
                    {meta ? meta.outputMetricCount : '—'}
                  </td>
                  <td className="px-4 py-3">
                    {isValidated ? (
                      <span className="inline-flex items-center rounded-full border border-warning-200 bg-warning-50 px-2 py-0.5 text-xs font-medium text-warning-700">
                        {t('algorithms.validated')}
                      </span>
                    ) : (
                      <StatusPill status={item.status} />
                    )}
                  </td>
                  <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                    {formatDate(item.createdAt)}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex flex-wrap items-center gap-1">
                      <Button variant="ghost" onClick={() => setDetailTarget(item)}>
                        {t('algorithms.details')}
                      </Button>
                      <Button variant="ghost" onClick={() => openEdit(item)}>
                        {t('common.edit')}
                      </Button>
                      {isDraft && !isValidated ? (
                        <Button
                          variant="ghost"
                          className="text-warning-600 hover:bg-warning-50 hover:text-warning-700"
                          onClick={() => handleDemoValidate(item)}
                        >
                          {t('algorithms.demoValidate')}
                        </Button>
                      ) : null}
                      {isDraft && isValidated ? (
                        <Button
                          variant="ghost"
                          className="text-teal-600 hover:bg-teal-50 hover:text-teal-700"
                          disabled={actionBusy}
                          onClick={() => handleDemoApprovePublish(item)}
                        >
                          {t('algorithms.demoApprovePublish')}
                        </Button>
                      ) : null}
                      {item.status !== 'Published' ? (
                        <Button
                          variant="ghost"
                          disabled={actionBusy}
                          onClick={() => changeStatus(item, 'publish')}
                        >
                          {t('catalog.publish')}
                        </Button>
                      ) : null}
                      {item.status !== 'Archived' ? (
                        <Button
                          variant="ghost"
                          disabled={actionBusy}
                          onClick={() => changeStatus(item, 'archive')}
                        >
                          {t('catalog.archive')}
                        </Button>
                      ) : null}
                      <Button
                        variant="ghost"
                        className="text-invalid-600 hover:bg-invalid-50 hover:text-invalid-700"
                        onClick={() => {
                          setActionError(null);
                          setDeleteTarget(item);
                        }}
                      >
                        {t('common.delete')}
                      </Button>
                    </div>
                  </td>
                </tr>
              );
            })}
          </Table>
        )}
      </Card>

      <Modal
        open={formOpen}
        title={editing ? t('catalog.editTitle') : t('catalog.createTitle')}
        onClose={() => setFormOpen(false)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setFormOpen(false)}>
              {t('common.cancel')}
            </Button>
            <Button type="submit" form="algorithm-form" disabled={saving}>
              {saving ? t('catalog.saving') : t('common.save')}
            </Button>
          </>
        }
      >
        <form id="algorithm-form" onSubmit={handleSubmit} className="space-y-3">
          <div className="grid gap-3 sm:grid-cols-2">
            <label className="block">
              <span className="mb-1 block text-xs font-medium text-ink-600">
                {t('catalog.name')} *
              </span>
              <Input
                value={form.name}
                placeholder={t('catalog.namePlaceholder')}
                onChange={(event) => setForm({ ...form, name: event.target.value })}
                required
              />
            </label>
            <label className="block">
              <span className="mb-1 block text-xs font-medium text-ink-600">
                {t('catalog.version')} *
              </span>
              <Input
                value={form.version}
                placeholder={t('catalog.versionPlaceholder')}
                onChange={(event) => setForm({ ...form, version: event.target.value })}
                required
              />
            </label>
          </div>
          <label className="block">
            <span className="mb-1 block text-xs font-medium text-ink-600">
              {t('catalog.supportedMetrics')}
            </span>
            <Input
              value={form.supportedMetricCodes}
              placeholder={t('catalog.supportedMetricsPlaceholder')}
              onChange={(event) =>
                setForm({ ...form, supportedMetricCodes: event.target.value })
              }
            />
          </label>
          <label className="block">
            <span className="mb-1 block text-xs font-medium text-ink-600">
              {t('catalog.notes')}
            </span>
            <Textarea
              value={form.notes}
              placeholder={t('catalog.notesPlaceholder')}
              onChange={(event) => setForm({ ...form, notes: event.target.value })}
            />
          </label>
          {formError ? <p className="text-sm text-invalid-600">{formError}</p> : null}
        </form>
      </Modal>

      <Modal
        open={deleteTarget !== null}
        title={t('catalog.deleteTitle')}
        onClose={() => setDeleteTarget(null)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setDeleteTarget(null)}>
              {t('common.cancel')}
            </Button>
            <Button variant="danger" disabled={actionBusy} onClick={handleDelete}>
              {t('common.delete')}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-600">{t('catalog.deleteBody')}</p>
        {deleteTarget ? (
          <p className="mt-2 text-sm font-semibold text-ink-900">
            {deleteTarget.name} — {deleteTarget.version}
          </p>
        ) : null}
        {actionError ? <p className="mt-3 text-sm text-invalid-600">{actionError}</p> : null}
      </Modal>

      <Modal
        open={detailTarget !== null}
        title={t('algorithms.scientificDetail')}
        onClose={() => setDetailTarget(null)}
        footer={
          <Button variant="secondary" onClick={() => setDetailTarget(null)}>
            {t('common.close')}
          </Button>
        }
      >
        {detailTarget ? (() => {
          const meta = getMeta(detailTarget);
          const metricCodes = detailTarget.supportedMetricCodes
            ? detailTarget.supportedMetricCodes.split(',').map((c) => c.trim()).filter(Boolean)
            : [];
          return (
            <div className="space-y-5">
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                <div>
                  <p className="text-xs font-medium text-ink-500">{t('catalog.code')}</p>
                  <p className="mt-1 font-mono text-sm text-ink-900">{detailTarget.code ?? '—'}</p>
                </div>
                <div>
                  <p className="text-xs font-medium text-ink-500">{t('algorithms.domain')}</p>
                  <p className="mt-1 text-sm text-ink-900">{meta ? meta.domain : 'Other'}</p>
                </div>
                <div>
                  <p className="text-xs font-medium text-ink-500">{t('catalog.version')}</p>
                  <p className="mt-1 text-sm text-ink-900">{detailTarget.version}</p>
                </div>
                <div>
                  <p className="text-xs font-medium text-ink-500">{t('catalog.status')}</p>
                  <div className="mt-1">
                    {validatedIds.has(detailTarget.id) ? (
                      <span className="inline-flex items-center rounded-full border border-warning-200 bg-warning-50 px-2 py-0.5 text-xs font-medium text-warning-700">
                        {t('algorithms.validated')}
                      </span>
                    ) : (
                      <StatusPill status={detailTarget.status} />
                    )}
                  </div>
                </div>
              </div>

              {meta ? (
                <>
                  <div>
                    <h4 className="mb-2 text-sm font-semibold text-ink-900">{t('algorithms.requiredChannels')}</h4>
                    <div className="space-y-1.5">
                      {meta.requiredChannels.map((ch) => (
                        <div key={ch.name} className="flex items-start gap-2 text-sm">
                          <span className="font-mono text-ink-800">{ch.name}</span>
                          {ch.required ? (
                            <span className="inline-flex items-center rounded-full border border-warning-200 bg-warning-50 px-1.5 py-0.5 text-[10px] font-medium text-warning-700">
                              required
                            </span>
                          ) : null}
                          <span className="text-ink-500">{ch.description}</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  <div>
                    <h4 className="mb-2 text-sm font-semibold text-ink-900">{t('algorithms.parameters')}</h4>
                    <div className="overflow-hidden rounded-lg border border-ink-200">
                      <table className="w-full text-sm">
                        <thead className="bg-ink-50">
                          <tr>
                            <th className="px-3 py-2 text-left font-medium text-ink-600">Name</th>
                            <th className="px-3 py-2 text-left font-medium text-ink-600">Default</th>
                            <th className="px-3 py-2 text-left font-medium text-ink-600">Description</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-ink-100">
                          {meta.parameters.map((p) => (
                            <tr key={p.name}>
                              <td className="px-3 py-2 font-mono text-xs text-ink-800">{p.name}</td>
                              <td className="px-3 py-2 text-ink-600">{p.defaultValue}</td>
                              <td className="px-3 py-2 text-ink-500">{p.description}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>

                  <div>
                    <h4 className="mb-2 text-sm font-semibold text-ink-900">{t('algorithms.supportedProtocols')}</h4>
                    <div className="flex flex-wrap gap-1.5">
                      {meta.supportedProtocols.map((code) => (
                        <span key={code} className="inline-flex items-center rounded-full border border-teal-200 bg-teal-50 px-2 py-0.5 font-mono text-xs text-teal-700">
                          {code}
                        </span>
                      ))}
                    </div>
                  </div>
                </>
              ) : (
                <p className="text-sm text-ink-500">{t('algorithms.noScientificMeta')}</p>
              )}

              <div>
                <h4 className="mb-2 text-sm font-semibold text-ink-900">{t('catalog.supportedMetrics')}</h4>
                {metricCodes.length > 0 ? (
                  <div className="flex flex-wrap gap-1.5">
                    {metricCodes.map((code) => (
                      <span key={code} className="inline-flex items-center rounded-full border border-ink-200 bg-ink-50 px-2 py-0.5 font-mono text-xs text-ink-700">
                        {code}
                      </span>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-ink-400">—</p>
                )}
              </div>

              <div>
                <h4 className="mb-2 text-sm font-semibold text-ink-900">{t('algorithms.releaseNotes')}</h4>
                <p className="text-sm leading-relaxed text-ink-600">
                  {meta?.releaseNotes ?? detailTarget.notes ?? '—'}
                </p>
              </div>

              <div className="rounded-lg border border-warning-200 bg-warning-50 px-3 py-2">
                <p className="text-xs text-warning-700">{t('algorithms.demoLifecycleNote')}</p>
              </div>
            </div>
          );
        })() : null}
      </Modal>
    </div>
  );
}