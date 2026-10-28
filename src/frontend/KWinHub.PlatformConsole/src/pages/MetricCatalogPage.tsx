import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { Plus } from 'lucide-react';
import { api, type GlobalMetricResponse } from '../lib/api';
import { useI18n } from '../lib/i18n';
import { METRIC_SCIENTIFIC_META, type MetricScientificMeta } from '../lib/mockApi';
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

const BASE = '/platform/catalogs/metrics';

const EMPTY_FORM = {
  code: '',
  name: '',
  unit: '',
  category: '',
  description: '',
};

export function MetricCatalogPage() {
  const { t } = useI18n();

  const [items, setItems] = useState<GlobalMetricResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);
  const [search, setSearch] = useState('');
  const [domainFilter, setDomainFilter] = useState('');

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<GlobalMetricResponse | null>(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const [deleteTarget, setDeleteTarget] = useState<GlobalMetricResponse | null>(null);
  const [detailTarget, setDetailTarget] = useState<GlobalMetricResponse | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionBusy, setActionBusy] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    api
      .get<GlobalMetricResponse[]>(BASE)
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

  const metaByCode = useMemo(() => METRIC_SCIENTIFIC_META, []);

  function getMeta(code: string): MetricScientificMeta | undefined {
    return metaByCode[code];
  }

  function getDomainLabel(code: string): string {
    const meta = getMeta(code);
    return meta?.domain ?? 'Other';
  }

  function openCreate() {
    setEditing(null);
    setForm(EMPTY_FORM);
    setFormError(null);
    setFormOpen(true);
  }

  function openEdit(item: GlobalMetricResponse) {
    setEditing(item);
    setForm({
      code: item.code,
      name: item.name,
      unit: item.unit,
      category: item.category ?? '',
      description: item.description ?? '',
    });
    setFormError(null);
    setFormOpen(true);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!form.code.trim() || !form.name.trim() || !form.unit.trim()) {
      setFormError(t('catalog.required'));
      return;
    }

    setFormError(null);
    setSaving(true);

    const body = {
      code: form.code.trim(),
      name: form.name.trim(),
      unit: form.unit.trim(),
      category: form.category.trim() || null,
      description: form.description.trim() || null,
    };

    try {
      if (editing) {
        await api.put<GlobalMetricResponse>(`${BASE}/${editing.id}`, body);
      } else {
        await api.post<GlobalMetricResponse>(BASE, body);
      }
      setFormOpen(false);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : t('common.error'));
    } finally {
      setSaving(false);
    }
  }

  async function changeStatus(item: GlobalMetricResponse, action: 'publish' | 'archive') {
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

  const query = search.trim().toLowerCase();
  const filtered = items.filter((item) => {
    const matchesQuery =
      !query ||
      item.code.toLowerCase().includes(query) ||
      item.name.toLowerCase().includes(query);
    const matchesDomain = !domainFilter || getDomainLabel(item.code) === domainFilter;
    return matchesQuery && matchesDomain;
  });

  const domainOptions = useMemo(() => {
    const set = new Set(items.map((item) => getDomainLabel(item.code)));
    return Array.from(set).sort();
  }, [items]);

  return (
    <div>
      <PageHeader
        title={t('metrics.title')}
        subtitle={t('metrics.subtitle')}
        actions={
          <Button onClick={openCreate}>
            <Plus size={16} />
            {t('catalog.new')}
          </Button>
        }
      />

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
          <Select
            className="max-w-[10rem]"
            value={domainFilter}
            onChange={(event) => setDomainFilter(event.target.value)}
          >
            <option value="">{t('metrics.filterByDomain')}</option>
            {domainOptions.map((domain) => (
              <option key={domain} value={domain}>
                {domain}
              </option>
            ))}
          </Select>
        </div>

        {loading ? (
          <Spinner />
        ) : filtered.length === 0 ? (
          <EmptyState title={query || domainFilter ? t('catalog.noResults') : t('catalog.empty')} />
        ) : (
          <Table
            headers={[
              t('catalog.code'),
              t('catalog.name'),
              t('metrics.domain'),
              t('metrics.method'),
              t('metrics.definitionVersion'),
              t('catalog.unit'),
              t('catalog.status'),
              t('catalog.actions'),
            ]}
          >
            {filtered.map((item) => {
              const meta = getMeta(item.code);
              return (
                <tr key={item.id}>
                  <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-ink-700">
                    {item.code}
                  </td>
                  <td className="px-4 py-3 font-medium text-ink-900">{item.name}</td>
                  <td className="px-4 py-3 text-ink-600">{meta?.domain ?? 'Other'}</td>
                  <td className="px-4 py-3 text-ink-600">{meta?.methodLabel ?? '—'}</td>
                  <td className="px-4 py-3 text-ink-600">{meta?.definitionVersion ?? '—'}</td>
                  <td className="px-4 py-3 text-ink-600">{item.unit}</td>
                  <td className="px-4 py-3">
                    <StatusPill status={item.status} />
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex flex-wrap items-center gap-1">
                      <Button variant="ghost" onClick={() => setDetailTarget(item)}>
                        {t('algorithms.details')}
                      </Button>
                      <Button variant="ghost" onClick={() => openEdit(item)}>
                        {t('common.edit')}
                      </Button>
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
            <Button type="submit" form="metric-form" disabled={saving}>
              {saving ? t('catalog.saving') : t('common.save')}
            </Button>
          </>
        }
      >
        <form id="metric-form" onSubmit={handleSubmit} className="space-y-3">
          <div className="grid gap-3 sm:grid-cols-2">
            <label className="block">
              <span className="mb-1 block text-xs font-medium text-ink-600">
                {t('catalog.code')} *
              </span>
              <Input
                value={form.code}
                placeholder={t('catalog.codePlaceholder')}
                onChange={(event) => setForm({ ...form, code: event.target.value })}
                required
              />
            </label>
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
                {t('catalog.unit')} *
              </span>
              <Input
                value={form.unit}
                placeholder={t('catalog.unitPlaceholder')}
                onChange={(event) => setForm({ ...form, unit: event.target.value })}
                required
              />
            </label>
            <label className="block">
              <span className="mb-1 block text-xs font-medium text-ink-600">
                {t('catalog.category')}
              </span>
              <Input
                value={form.category}
                placeholder={t('catalog.categoryPlaceholder')}
                onChange={(event) => setForm({ ...form, category: event.target.value })}
              />
            </label>
          </div>
          <label className="block">
            <span className="mb-1 block text-xs font-medium text-ink-600">
              {t('catalog.description')}
            </span>
            <Textarea
              value={form.description}
              placeholder={t('catalog.descriptionPlaceholder')}
              onChange={(event) => setForm({ ...form, description: event.target.value })}
            />
          </label>
          {formError ? <p className="text-sm text-invalid-600">{formError}</p> : null}
        </form>
      </Modal>

      <Modal
        open={detailTarget !== null}
        title={t('metrics.detail')}
        onClose={() => setDetailTarget(null)}
        footer={
          <Button variant="secondary" onClick={() => setDetailTarget(null)}>
            {t('common.close')}
          </Button>
        }
      >
        {detailTarget ? (
          <>
            <div className="mb-4 space-y-2">
              <p className="font-mono text-sm text-ink-700">{detailTarget.code}</p>
              <p className="text-sm font-semibold text-ink-900">{detailTarget.name}</p>
              {detailTarget.description ? (
                <p className="text-sm text-ink-500">{detailTarget.description}</p>
              ) : null}
            </div>
            {(() => {
              const meta = getMeta(detailTarget.code);
              if (!meta) {
                return <p className="text-sm text-ink-500">{t('metrics.noScientificMeta')}</p>;
              }
              return (
                <div className="space-y-4 text-sm">
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <p className="text-xs font-medium text-ink-500">{t('metrics.domain')}</p>
                      <p className="text-ink-900">{meta.domain}</p>
                    </div>
                    <div>
                      <p className="text-xs font-medium text-ink-500">{t('metrics.physicalQuantity')}</p>
                      <p className="text-ink-900">{meta.physicalQuantity}</p>
                    </div>
                    <div>
                      <p className="text-xs font-medium text-ink-500">{t('metrics.method')}</p>
                      <p className="text-ink-900">{meta.methodLabel}</p>
                    </div>
                    <div>
                      <p className="text-xs font-medium text-ink-500">{t('metrics.definitionVersion')}</p>
                      <p className="text-ink-900">v{meta.definitionVersion}</p>
                    </div>
                  </div>
                  <div>
                    <p className="text-xs font-medium text-ink-500">{t('metrics.formula')}</p>
                    <code className="mt-1 block rounded-md bg-ink-50 px-3 py-2 font-mono text-xs text-ink-800">
                      {meta.formula}
                    </code>
                  </div>
                  <div>
                    <p className="text-xs font-medium text-ink-500">{t('metrics.dependentMetrics')}</p>
                    <p className="text-ink-900">
                      {meta.dependentMetrics.length > 0
                        ? meta.dependentMetrics.map((code) => (
                            <span
                              key={code}
                              className="mr-2 inline-block rounded bg-ink-100 px-2 py-0.5 font-mono text-xs text-ink-700"
                            >
                              {code}
                            </span>
                          ))
                        : t('metrics.noDependents')}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs font-medium text-ink-500">{t('metrics.versionHistory')}</p>
                    <ul className="mt-1 space-y-1">
                      {meta.versionHistory.map((v) => (
                        <li key={v.version} className="flex gap-2 text-ink-700">
                          <span className="font-mono text-xs text-ink-500">{v.version}</span>
                          <span className="text-xs">{v.changeSummary}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                  <div>
                    <p className="text-xs font-medium text-ink-500">{t('metrics.notes')}</p>
                    <p className="text-ink-700">{meta.notes}</p>
                  </div>
                </div>
              );
            })()}
          </>
        ) : null}
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
            {deleteTarget.code} — {deleteTarget.name}
          </p>
        ) : null}
        {actionError ? <p className="mt-3 text-sm text-invalid-600">{actionError}</p> : null}
      </Modal>
    </div>
  );
}