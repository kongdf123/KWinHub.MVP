import { useEffect, useState, type FormEvent } from 'react';
import { Plus } from 'lucide-react';
import { api, type AlgorithmVersionResponse } from '../lib/api';
import { useI18n } from '../lib/i18n';
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

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<AlgorithmVersionResponse | null>(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const [deleteTarget, setDeleteTarget] = useState<AlgorithmVersionResponse | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionBusy, setActionBusy] = useState(false);

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

  const query = search.trim().toLowerCase();
  const filtered = query
    ? items.filter(
        (item) =>
          item.name.toLowerCase().includes(query) || item.version.toLowerCase().includes(query)
      )
    : items;

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
        <div className="border-b border-ink-200 p-4">
          <Input
            className="max-w-xs"
            placeholder={t('catalog.searchPlaceholder')}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </div>

        {loading ? (
          <Spinner />
        ) : filtered.length === 0 ? (
          <EmptyState title={query ? t('catalog.noResults') : t('catalog.empty')} />
        ) : (
          <Table
            headers={[
              t('catalog.name'),
              t('catalog.version'),
              t('catalog.supportedMetrics'),
              t('catalog.status'),
              t('catalog.created'),
              t('catalog.actions'),
            ]}
          >
            {filtered.map((item) => (
              <tr key={item.id}>
                <td className="px-4 py-3 font-medium text-ink-900">{item.name}</td>
                <td className="px-4 py-3 text-ink-600">{item.version}</td>
                <td className="max-w-xs truncate px-4 py-3 font-mono text-xs text-ink-600">
                  {item.supportedMetricCodes ?? '—'}
                </td>
                <td className="px-4 py-3">
                  <StatusPill status={item.status} />
                </td>
                <td className="whitespace-nowrap px-4 py-3 text-ink-500">
                  {formatDate(item.createdAt)}
                </td>
                <td className="px-4 py-3">
                  <div className="flex flex-wrap items-center gap-1">
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
            ))}
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
    </div>
  );
}
