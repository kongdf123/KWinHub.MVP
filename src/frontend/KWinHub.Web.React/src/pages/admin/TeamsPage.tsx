import { useState, useEffect } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { DataTable } from '@/components/ui/DataTable';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { DEMO_TEAMS, getSport } from '@/lib/demo-data';
import { Search, Plus, Archive, RotateCcw } from 'lucide-react';
import { teamsApi, type TeamDto } from '@/lib/api/teams';
import type { Team } from '@/lib/types';

const API_MODE = import.meta.env.VITE_DATA_MODE === 'api';

export function TeamsPage() {
  const { lang, navigate, canAccess, addToast } = useApp();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [apiTeams, setApiTeams] = useState<TeamDto[]>([]);
  const [loading, setLoading] = useState(API_MODE);

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['teams'] ?? []).filter((tab) => canAccess(tab.view));

  useEffect(() => {
    if (!API_MODE) return;
    let cancelled = false;
    setLoading(true);
    teamsApi.list({ search: search || undefined, status: statusFilter === 'all' ? undefined : statusFilter })
      .then((data) => { if (!cancelled) setApiTeams(data); })
      .catch(() => { if (!cancelled) setApiTeams([]); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [search, statusFilter]);

  const handleArchive = async (id: string) => {
    try {
      await teamsApi.archive(id);
      addToast(lang === 'cn' ? '已归档' : 'Archived', 'success');
      setApiTeams((prev) => prev.map((t) => t.id === id ? { ...t, status: 'Archived' } : t));
    } catch {
      addToast(lang === 'cn' ? '归档失败' : 'Archive failed', 'error');
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await teamsApi.restore(id);
      addToast(lang === 'cn' ? '已恢复' : 'Restored', 'success');
      setApiTeams((prev) => prev.map((t) => t.id === id ? { ...t, status: 'Active' } : t));
    } catch {
      addToast(lang === 'cn' ? '恢复失败' : 'Restore failed', 'error');
    }
  };

  const teams: Array<Team | TeamDto> = API_MODE ? apiTeams : DEMO_TEAMS;

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.people', lang)}</h2>
          <div className="flex items-center gap-2 text-sm text-content-secondary">
            <span>{teams.length} {lang === 'cn' ? '个队伍' : 'teams'}</span>
          </div>
        </div>

        {workspaceTabs.length > 1 && (
          <nav
            aria-label={tr('nav.people', lang)}
            className="flex items-center gap-1 mt-4 border-b border-default"
          >
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'teams';
              return (
                <button
                  key={tab.view}
                  type="button"
                  onClick={() => navigate(tab.view)}
                  aria-current={active ? 'page' : undefined}
                  className={`px-4 py-2.5 -mb-px text-sm font-medium border-b-2 whitespace-nowrap transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent ${
                    active
                      ? 'border-accent text-accent'
                      : 'border-transparent text-content-secondary hover:text-content-primary'
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
          <div className="relative flex-1 min-w-[200px] max-w-sm">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-content-muted" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={lang === 'cn' ? '搜索队伍...' : 'Search teams...'}
              className="w-full pl-9 pr-3 py-2 text-sm bg-surface-panel border border-default rounded-lg focus:outline-none focus:ring-2 focus:ring-accent focus:border-accent transition-all"
            />
          </div>
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="text-sm bg-surface-panel border border-default rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-accent transition-all">
            <option value="all">{lang === 'cn' ? '全部状态' : 'All statuses'}</option>
            <option value="Active">{lang === 'cn' ? '活跃' : 'Active'}</option>
            <option value="Archived">{lang === 'cn' ? '已归档' : 'Archived'}</option>
          </select>
          <div className="flex-1" />
          <button
            onClick={() => addToast(lang === 'cn' ? '创建队伍（演示）' : 'Create team (demo)', 'info')}
            className="flex items-center gap-2 px-3 py-2 bg-accent hover:bg-accent-hover text-white text-sm font-medium rounded-lg transition-colors"
          >
            <Plus className="w-4 h-4" /> {lang === 'cn' ? '新建队伍' : 'New Team'}
          </button>
        </div>

        <div className="bg-surface-panel rounded-xl border border-default overflow-hidden">
          {loading && API_MODE ? (
            <div className="px-5 py-8 text-center text-sm text-content-muted">
              {lang === 'cn' ? '加载中...' : 'Loading...'}
            </div>
          ) : (
            <DataTable
              columns={[
                {
                  key: 'name',
                  header: lang === 'cn' ? '队伍' : 'Team',
                  render: (t) => {
                    const name = API_MODE ? (t as TeamDto).name : (lang === 'cn' ? (t as Team).nameCn : (t as Team).name);
                    return <p className="text-sm font-medium text-content-primary">{name}</p>;
                  },
                },
                {
                  key: 'sport',
                  header: lang === 'cn' ? '项目' : 'Sport',
                  render: (t) => {
                    const sportId = API_MODE ? (t as TeamDto).sportId : (t as Team).sportId;
                    const sport = getSport(sportId ?? '');
                    return <span className="text-sm text-content-secondary">{sport ? (lang === 'cn' ? sport.nameCn : sport.name) : '—'}</span>;
                  },
                },
                {
                  key: 'season',
                  header: lang === 'cn' ? '赛季' : 'Season',
                  render: (t) => {
                    const season = API_MODE ? (t as TeamDto).season : (t as Team).season;
                    return <span className="text-sm text-content-secondary">{season || '—'}</span>;
                  },
                },
                {
                  key: 'count',
                  header: lang === 'cn' ? '人数' : 'Members',
                  align: 'right',
                  render: (t) => {
                    const count = API_MODE ? (t as TeamDto).memberCount : (t as Team).athleteCount;
                    return <span className="text-sm font-mono text-content-secondary">{count}</span>;
                  },
                },
                {
                  key: 'status',
                  header: lang === 'cn' ? '状态' : 'Status',
                  render: (t) => {
                    const status = API_MODE ? (t as TeamDto).status : 'Active';
                    return (
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                        status === 'Archived' ? 'bg-attention-soft text-attention-700' : 'bg-success-soft text-success-700'
                      }`}>
                        {status}
                      </span>
                    );
                  },
                },
                {
                  key: 'action',
                  header: '',
                  align: 'right',
                  render: (t) => {
                    const id = API_MODE ? (t as TeamDto).id : (t as Team).id;
                    const status = API_MODE ? (t as TeamDto).status : 'Active';
                    return (
                      <div className="flex items-center justify-end gap-1">
                        {API_MODE && status === 'Active' && (
                          <button
                            onClick={() => handleArchive(id)}
                            className="p-1.5 text-content-muted hover:text-attention-700 hover:bg-attention-soft rounded-md transition-colors"
                            title={lang === 'cn' ? '归档' : 'Archive'}
                          >
                            <Archive className="w-4 h-4" />
                          </button>
                        )}
                        {API_MODE && status === 'Archived' && (
                          <button
                            onClick={() => handleRestore(id)}
                            className="p-1.5 text-content-muted hover:text-success-700 hover:bg-success-soft rounded-md transition-colors"
                            title={lang === 'cn' ? '恢复' : 'Restore'}
                          >
                            <RotateCcw className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                    );
                  },
                },
              ]}
              data={teams}
              rowKey={(t) => API_MODE ? (t as TeamDto).id : (t as Team).id}
              emptyMessage={lang === 'cn' ? '暂无队伍' : 'No teams'}
            />
          )}
        </div>
      </div>
    </div>
  );
}