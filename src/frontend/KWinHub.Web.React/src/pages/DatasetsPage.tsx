import { useState, useMemo } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { DataTable } from '@/components/ui/DataTable';
import { DEMO_DATASETS, DEMO_PROTOCOLS, getDataSource } from '@/lib/demo-data';
import { Search, Database, Download } from 'lucide-react';
import type { Dataset } from '@/lib/types';

const TYPE_LABELS: Record<string, { en: string; cn: string }> = {
  session: { en: 'Session', cn: '会话' },
  longitudinal: { en: 'Longitudinal', cn: '纵向' },
  research: { en: 'Research', cn: '研究' },
  import: { en: 'Import', cn: '导入' },
  device_stream: { en: 'Device Stream', cn: '设备流' },
};

const STATUS_LABELS: Record<string, { en: string; cn: string; color: string }> = {
  active: { en: 'Active', cn: '活跃', color: 'text-success-600' },
  archived: { en: 'Archived', cn: '已归档', color: 'text-ink-500' },
  processing: { en: 'Processing', cn: '处理中', color: 'text-teal-600' },
};

export function DatasetsPage() {
  const { lang, addToast, navigate, canAccess } = useApp();
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('all');

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['datasets'] ?? []).filter((tab) => canAccess(tab.view));

  const filtered = useMemo(() => {
    return DEMO_DATASETS.filter((d) => {
      const name = lang === 'cn' ? d.nameCn : d.name;
      const matchesSearch = name.toLowerCase().includes(search.toLowerCase());
      const matchesType = typeFilter === 'all' || d.type === typeFilter;
      return matchesSearch && matchesType;
    });
  }, [search, typeFilter, lang]);

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.workspace.data', lang)}</h2>
          <div className="text-sm text-content-secondary">{/* page count or subtitle here */}</div>
        </div>

        {workspaceTabs.length > 1 && (
          <nav aria-label={tr('nav.workspace.data', lang)} className="flex items-center gap-1 mt-4 border-b border-default">
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'datasets';
              return (
                <button
                  key={tab.view}
                  type="button"
                  onClick={() => navigate(tab.view)}
                  aria-current={active ? 'page' : undefined}
                  className={`px-4 py-2.5 -mb-px text-sm font-medium border-b-2 whitespace-nowrap transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent ${
                    active ? 'border-accent text-accent' : 'border-transparent text-content-secondary hover:text-content-primary'
                  }`}
                >
                  {tr(tab.labelKey, lang)}
                </button>
              );
            })}
          </nav>
        )}
      </div>

      <div className="px-6 py-6 space-y-4">
        <div className="flex flex-wrap items-center gap-3">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={lang === 'cn' ? '搜索数据集...' : 'Search datasets...'}
              className="w-full pl-9 pr-3 py-2 text-sm bg-white border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400 focus:border-teal-400 transition-all"
            />
          </div>
          <select value={typeFilter} onChange={(e) => setTypeFilter(e.target.value)} className="text-sm bg-white border border-ink-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-teal-400 transition-all">
            <option value="all">{tr('common.allTypes', lang)}</option>
            {Object.entries(TYPE_LABELS).map(([key, val]) => (
              <option key={key} value={key}>{lang === 'cn' ? val.cn : val.en}</option>
            ))}
          </select>
        </div>

        <div className="bg-white rounded-xl border border-ink-200 overflow-hidden">
          <DataTable
            columns={[
              {
                key: 'name',
                header: lang === 'cn' ? '数据集' : 'Dataset',
                render: (d: Dataset) => (
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-lg bg-ink-100 flex items-center justify-center shrink-0">
                      <Database className="w-4 h-4 text-ink-500" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-ink-900">{lang === 'cn' ? d.nameCn : d.name}</p>
                      <p className="text-xs text-ink-400">{d.subjectIds.length} {tr('datasets.subjects', lang)} · {d.protocolIds.length} {tr('nav.protocols', lang)}</p>
                    </div>
                  </div>
                ),
              },
              {
                key: 'type',
                header: tr('datasets.type', lang),
                render: (d: Dataset) => {
                  const t = TYPE_LABELS[d.type];
                  return <span className="text-sm text-ink-700">{t ? (lang === 'cn' ? t.cn : t.en) : d.type}</span>;
                },
              },
              {
                key: 'source',
                header: tr('datasets.source', lang),
                render: (d: Dataset) => {
                  const ds = getDataSource(d.sourceId);
                  return <span className="text-sm text-ink-700">{ds ? (lang === 'cn' ? ds.nameCn : ds.name) : d.sourceId}</span>;
                },
              },
              {
                key: 'measurements',
                header: tr('datasets.measurements', lang),
                align: 'right',
                render: (d: Dataset) => <span className="text-sm font-mono text-ink-700">{d.measurementCount}</span>,
              },
              {
                key: 'createdAt',
                header: tr('datasets.created', lang),
                align: 'right',
                render: (d: Dataset) => <span className="text-sm text-ink-600">{new Date(d.createdAt).toLocaleDateString(lang === 'cn' ? 'zh-CN' : 'en-US', { month: 'short', day: 'numeric', year: 'numeric' })}</span>,
              },
              {
                key: 'status',
                header: tr('dataSources.status', lang),
                render: (d: Dataset) => {
                  const s = STATUS_LABELS[d.status];
                  return <span className={`text-sm font-medium ${s?.color || 'text-ink-600'}`}>{s ? (lang === 'cn' ? s.cn : s.en) : d.status}</span>;
                },
              },
            ]}
            data={filtered}
            rowKey={(d) => d.id}
            emptyMessage={tr('datasets.noMatch', lang)}
          />
        </div>
      </div>
    </div>
  );
}
