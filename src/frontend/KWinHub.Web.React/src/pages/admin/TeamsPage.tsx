import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { DataTable } from '@/components/ui/DataTable';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { DEMO_TEAMS, DEMO_GROUPS, getSport } from '@/lib/demo-data';
import type { Team, Group } from '@/lib/types';

export function TeamsPage() {
  const { lang, navigate, canAccess } = useApp();

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['teams'] ?? []).filter((tab) => canAccess(tab.view));

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.people', lang)}</h2>
          <div className="flex items-center gap-2 text-sm text-content-secondary">
            <span>{DEMO_TEAMS.length} {lang === 'cn' ? '个队伍' : 'teams'}</span>
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

      <div className="px-6 py-6 space-y-6">
        <div className="bg-surface-panel rounded-xl border border-default overflow-hidden">
          <h4 className="text-sm font-semibold text-content-primary px-5 py-4 border-b border-default">
            {lang === 'cn' ? '队伍' : 'Teams'}
          </h4>
          <DataTable
            columns={[
              {
                key: 'name',
                header: lang === 'cn' ? '队伍' : 'Team',
                render: (t: Team) => <p className="text-sm font-medium text-content-primary">{lang === 'cn' ? t.nameCn : t.name}</p>,
              },
              {
                key: 'sport',
                header: lang === 'cn' ? '项目' : 'Sport',
                render: (t: Team) => {
                  const sport = getSport(t.sportId);
                  return <span className="text-sm text-content-secondary">{sport ? (lang === 'cn' ? sport.nameCn : sport.name) : '—'}</span>;
                },
              },
              { key: 'season', header: lang === 'cn' ? '赛季' : 'Season', render: (t: Team) => <span className="text-sm text-content-secondary">{t.season}</span> },
              { key: 'count', header: lang === 'cn' ? '人数' : 'Athletes', align: 'right', render: (t: Team) => <span className="text-sm font-mono text-content-secondary">{t.athleteCount}</span> },
            ]}
            data={DEMO_TEAMS}
            rowKey={(t) => t.id}
          />
        </div>

        <div className="bg-surface-panel rounded-xl border border-default overflow-hidden">
          <h4 className="text-sm font-semibold text-content-primary px-5 py-4 border-b border-default">
            {lang === 'cn' ? '分组' : 'Groups'}
          </h4>
          <DataTable
            columns={[
              {
                key: 'name',
                header: lang === 'cn' ? '分组' : 'Group',
                render: (g: Group) => <p className="text-sm font-medium text-content-primary">{lang === 'cn' ? g.nameCn : g.name}</p>,
              },
              { key: 'type', header: lang === 'cn' ? '类型' : 'Type', render: (g: Group) => <span className="text-sm text-content-secondary">{g.type}</span> },
              { key: 'desc', header: lang === 'cn' ? '描述' : 'Description', render: (g: Group) => <span className="text-sm text-content-muted">{g.description}</span> },
              { key: 'count', header: lang === 'cn' ? '人数' : 'Athletes', align: 'right', render: (g: Group) => <span className="text-sm font-mono text-content-secondary">{g.athleteCount}</span> },
            ]}
            data={DEMO_GROUPS}
            rowKey={(g) => g.id}
          />
        </div>
      </div>
    </div>
  );
}
