import { useEffect, useState, type FormEvent } from 'react';
import { Plus } from 'lucide-react';
import { api, type GlobalProtocolResponse } from '../lib/api';
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

const BASE = '/platform/catalogs/protocols';

const EMPTY_FORM = {
  code: '',
  name: '',
  version: '1.0.0',
  category: '',
  sport: '',
  description: '',
};

export function ProtocolCatalogPage() {
  const { t } = useI18n();

  const [items, setItems] = useState<GlobalProtocolResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);
  const [search, setSearch] = useState('');

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<GlobalProtocolResponse | null>(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const [deleteTarget, setDeleteTarget] = useState<GlobalProtocolResponse | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [actionBusy, setActionBusy] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    api
      .get<GlobalProtocolResponse[]>(BASE)
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

  function openEdit(item: GlobalProtocolResponse) {
    setEditing(item);
    setForm({
      code: item.code,
      name: item.name,
      version: item.version,
      category: item.category ?? '',
      sport: item.sport ?? '',
      description: item.description ?? '',
    });
    setFormError(null);
    setFormOpen(true);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!form.code.trim() || !form.name.trim() || !form.version.trim()) {
      setFormError(t('catalog.required'));
      return;
    }

    setFormError(null);
    setSaving(true);

    const body = {
      code: form.code.trim(),
      name: form.name.trim(),
      version: form.version.trim(),
      category: form.category.trim() || null,
      sport: form.sport.trim() || null,
      description: form.description.trim() || null,
    };

    try {
      if (editing) {
        await api.put<GlobalProtocolResponse>(`${BASE}/${editing.id}`, body);
      } else {
        await api.post<GlobalProtocolResponse>(BASE, body);
      }
      setFormOpen(false);
      setReloadKey((key) => key + 1);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : t('common.error'));
    } finally {
      setSaving(false);
    }
  }

  async function changeStatus(item: GlobalProtocolResponse, action: 'publish' | 'archive') {
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
          item.code.toLowerCase().includes(query) || item.name.toLowerCase().includes(query)
      )
    : items;

  return (
    <div>
      <PageHeader
        title={t('protocols.title')}
        subtitle={t('protocols.subtitle')}
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
              t('catalog.code'),
              t('catalog.name'),
              t('catalog.version'),
              t('catalog.status'),
              t('catalog.created'),
              t('catalog.actions'),
            ]}
          >
            {filtered.map((item) => (
              <tr key={item.id}>
                <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-ink-700">
                  {item.code}
                </td>
                <td className="px-4 py-3 font-medium text-ink-900">{item.name}</td>
                <td className="px-4 py-3 text-ink-600">{item.version}</td>
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
            <Button type="submit" form="protocol-form" disabled={saving}>
              {saving ? t('catalog.saving') : t('common.save')}
            </Button>
          </>
        }
      >
        <form id="protocol-form" onSubmit={handleSubmit} className="space-y-3">
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
                {t('catalog.version')} *
              </span>
              <Input
                value={form.version}
                placeholder={t('catalog.versionPlaceholder')}
                onChange={(event) => setForm({ ...form, version: event.target.value })}
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
            <label className="block">
              <span className="mb-1 block text-xs font-medium text-ink-600">
                {t('catalog.sport')}
              </span>
              <Input
                value={form.sport}
                placeholder={t('catalog.sportPlaceholder')}
                onChange={(event) => setForm({ ...form, sport: event.target.value })}
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
