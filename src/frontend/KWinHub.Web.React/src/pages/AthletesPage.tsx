import { useState, useMemo } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { DEMO_ATHLETES, DEMO_TEAMS, DEMO_GROUPS, DEMO_SPORTS, getSessionsForAthlete, getSport } from '@/lib/demo-data';
import { TopBar } from '@/components/layout/TopBar';
import { DataTable } from '@/components/ui/DataTable';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { Sparkline } from '@/components/ui/Sparkline';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { Search, Filter, Download, UserPlus, ChevronRight, Activity, Zap, Waves, TrendingUp, Dumbbell, Shuffle } from 'lucide-react';
import type { Athlete } from '@/lib/types';

const SPORT_ICONS: Record<string, typeof Activity> = {
  'sport-cmj': Activity,
  'sport-sprint': Zap,
  'sport-swim': Waves,
  'sport-hj': TrendingUp,
  'sport-imtp': Dumbbell,
  'sport-cod': Shuffle,
};

export function AthletesPage() {
  const { navigate, selectedTeamId, addToast, lang, canAccess } = useApp();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [groupFilter, setGroupFilter] = useState('all');
  const [sportFilter, setSportFilter] = useState('all');

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['athletes'] ?? []).filter((tab) => canAccess(tab.view));

  const teamAthletes = useMemo(
    () => DEMO_ATHLETES.filter((a) => a.teamId === selectedTeamId),
    [selectedTeamId],
  );

  const filteredAthletes = useMemo(() => {
    return teamAthletes.filter((a) => {
      const fullName = `${a.lastName} ${a.firstName}`.toLowerCase();
      const matchesSearch = fullName.includes(search.toLowerCase()) || a.position.toLowerCase().includes(search.toLowerCase());
      const matchesStatus = statusFilter === 'all' || a.status === statusFilter;
      const matchesGroup = groupFilter === 'all' || a.groupIds.includes(groupFilter);
      const matchesSport = sportFilter === 'all' || a.sportId === sportFilter;
      return matchesSearch && matchesStatus && matchesGroup && matchesSport;
    });
  }, [teamAthletes, search, statusFilter, groupFilter, sportFilter]);

  const team = DEMO_TEAMS.find((t) => t.id === selectedTeamId);

  const formatDate = (dateStr: string | null) => {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return d.toLocaleDateString(lang === 'cn' ? 'zh-CN' : 'en-US', { month: 'short', day: 'numeric' });
  };

  const getTrendData = (athlete: Athlete) => {
    const sessions = getSessionsForAthlete(athlete.id).filter((s) => s.sportId === athlete.sportId);
    return sessions.reverse().map((s) => Object.values(s.summary)[0] || 0);
  };

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        {/* Section heading — VALD-style: workspace name, not the active tab */}
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.people', lang)}</h2>
          <div className="flex items-center gap-2 text-sm text-content-secondary">
            {team && <span>{lang === 'cn' ? team.nameCn : team.name}</span>}
            <span className="text-content-muted">·</span>
            <span>{filteredAthletes.length} / {teamAthletes.length}</span>
          </div>
        </div>

        {/* Workspace tabs — direct row under the heading, no extra container */}
        {workspaceTabs.length > 1 && (
          <nav
            aria-label={tr('nav.people', lang)}
            className="flex items-center gap-1 mt-4 border-b border-default"
          >
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'athletes';
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

      {/* Content toolbar + table — VALD-style single-surface body */}
      <div className="px-6 py-6 space-y-4">
        <div className="flex flex-wrap items-center gap-3">
          <div className="relative flex-1 min-w-[200px] max-w-sm">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-content-muted" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={tr('athletes.searchPlaceholder', lang)}
              className="w-full pl-9 pr-3 py-2 text-sm bg-surface-panel border border-default rounded-lg focus:outline-none focus:ring-2 focus:ring-accent focus:border-accent transition-all"
            />
          </div>
          <div className="flex items-center gap-2">
            <Filter className="w-4 h-4 text-content-muted" />
            <select value={sportFilter} onChange={(e) => setSportFilter(e.target.value)} className="text-sm bg-surface-panel border border-default rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-accent transition-all">
              <option value="all">{tr('library.allSports', lang)}</option>
              {DEMO_SPORTS.map((s) => (
                <option key={s.id} value={s.id}>{lang === 'cn' ? s.nameCn : s.name}</option>
              ))}
            </select>
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="text-sm bg-surface-panel border border-default rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-accent transition-all">
              <option value="all">{tr('athletes.allStatuses', lang)}</option>
              <option value="normal">{tr('common.normal', lang)}</option>
              <option value="warning">{tr('common.warning', lang)}</option>
              <option value="attention">{tr('common.attention', lang)}</option>
            </select>
            <select value={groupFilter} onChange={(e) => setGroupFilter(e.target.value)} className="text-sm bg-surface-panel border border-default rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-accent transition-all">
              <option value="all">{tr('athletes.allGroups', lang)}</option>
              {DEMO_GROUPS.map((g) => (
                <option key={g.id} value={g.id}>{lang === 'cn' ? g.nameCn : g.name}</option>
              ))}
            </select>
          </div>
          <div className="flex-1" />
          <div className="flex items-center gap-2">
            <button
              onClick={() => addToast(lang === 'cn' ? '导出已开始（演示）' : 'Export started (demo)', 'info')}
              className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-content-primary bg-surface-panel border border-default hover:bg-surface-hover rounded-lg transition-colors"
            >
              <Download className="w-4 h-4" /> {tr('common.export', lang)}
            </button>
            <button
              onClick={() => addToast(lang === 'cn' ? '添加运动员（演示）' : 'Add athlete (demo)', 'info')}
              className="flex items-center gap-2 px-3 py-2 bg-accent hover:bg-accent-hover text-white text-sm font-medium rounded-lg transition-colors"
            >
              <UserPlus className="w-4 h-4" /> {tr('common.addAthlete', lang)}
            </button>
          </div>
        </div>

        <div className="bg-surface-panel rounded-xl border border-default overflow-hidden">
          <DataTable
            columns={[
              {
                key: 'lastName',
                header: lang === 'cn' ? '运动员' : 'Athlete',
                sortable: true,
                render: (a) => {
                  const sport = getSport(a.sportId);
                  const SportIcon = sport ? SPORT_ICONS[sport.id] || Activity : Activity;
                  return (
                    <div className="flex items-center gap-3">
                      <div className="w-9 h-9 rounded-full bg-surface-hover flex items-center justify-center text-sm font-medium text-content-secondary shrink-0">
                        {a.firstName[0]}{a.lastName[0]}
                      </div>
                      <div>
                        <p className="font-medium text-content-primary">{a.lastName} {a.firstName}</p>
                        <p className="text-xs text-content-muted flex items-center gap-1">
                          <SportIcon className="w-3 h-3" />
                          {a.position} · {a.height}cm · {a.weight}kg
                        </p>
                      </div>
                    </div>
                  );
                },
              },
              {
                key: 'sportId',
                header: lang === 'cn' ? '项目' : 'Sport',
                sortable: true,
                render: (a) => {
                  const sport = getSport(a.sportId);
                  return <span className="text-sm text-content-primary">{sport ? (lang === 'cn' ? sport.nameCn : sport.name) : '—'}</span>;
                },
              },
              {
                key: 'status',
                header: lang === 'cn' ? '状态' : 'Status',
                sortable: true,
                render: (a) => <StatusBadge status={a.status} lang={lang} />,
              },
              {
                key: 'lastTestDate',
                header: tr('athletes.lastTest', lang),
                sortable: true,
                render: (a) => (
                  <div>
                    <p className="text-content-primary">{formatDate(a.lastTestDate)}</p>
                    <p className="text-xs text-content-muted">{a.lastTestProtocol || '—'}</p>
                  </div>
                ),
              },
              {
                key: 'trend',
                header: lang === 'cn' ? '趋势' : 'Trend',
                align: 'right',
                render: (a) => {
                  const trend = getTrendData(a);
                  if (trend.length < 2) return <span className="text-content-muted">—</span>;
                  return (
                    <div className="flex items-center justify-end gap-2">
                      <Sparkline data={trend} width={50} height={20} color={a.status === 'attention' ? '#f97316' : a.status === 'warning' ? '#fbbf24' : '#06b56b'} />
                    </div>
                  );
                },
              },
              {
                key: 'tags',
                header: lang === 'cn' ? '标签' : 'Tags',
                render: (a) => (
                  <div className="flex flex-wrap gap-1">
                    {a.tags.length === 0 ? <span className="text-content-muted text-xs">—</span> :
                      a.tags.map((tag) => (
                        <span key={tag} className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                          tag === 'Flagged' || tag === 'RTP' ? 'bg-attention-soft text-attention-700' : 'bg-surface-hover text-content-secondary'
                        }`}>{tag}</span>
                      ))
                    }
                  </div>
                ),
              },
              {
                key: 'action',
                header: '',
                align: 'right',
                render: (a) => (
                  <button
                    onClick={(e) => { e.stopPropagation(); navigate('athlete-profile', { athleteId: a.id }); }}
                    className="p-1.5 text-content-muted hover:text-accent hover:bg-accent-soft rounded-md transition-colors"
                    title={tr('common.viewAll', lang)}
                  >
                    <ChevronRight className="w-4 h-4" />
                  </button>
                ),
              },
            ]}
            data={filteredAthletes}
            rowKey={(a) => a.id}
            onRowClick={(a) => navigate('athlete-profile', { athleteId: a.id })}
            initialSort={{ key: 'lastName', direction: 'asc' }}
            emptyMessage={tr('athletes.noMatch', lang)}
          />
        </div>
      </div>
    </div>
  );
}
