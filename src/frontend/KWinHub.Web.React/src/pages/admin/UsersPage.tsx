import { useState, useMemo } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { DataTable } from '@/components/ui/DataTable';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { DEMO_USERS, DEMO_TEAMS } from '@/lib/demo-data';
import { Search, UserPlus } from 'lucide-react';
import type { User } from '@/lib/types';

export function UsersPage() {
  const { lang, addToast, navigate, canAccess } = useApp();
  const [search, setSearch] = useState('');

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['users'] ?? []).filter((tab) => canAccess(tab.view));

  const filtered = useMemo(() => {
    return DEMO_USERS.filter((u) => {
      const matchesSearch = u.name.toLowerCase().includes(search.toLowerCase()) || u.email.toLowerCase().includes(search.toLowerCase());
      return matchesSearch;
    });
  }, [search]);

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.settings', lang)}</h2>
          <div className="text-sm text-content-secondary">{/* page count or subtitle here */}</div>
        </div>

        {workspaceTabs.length > 1 && (
          <nav aria-label={tr('nav.settings', lang)} className="flex items-center gap-1 mt-4 border-b border-default">
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'users';
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
        <div className="relative flex-1 min-w-[200px]">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={lang === 'cn' ? '搜索用户...' : 'Search users...'}
            className="w-full pl-9 pr-3 py-2 text-sm bg-white border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400 focus:border-teal-400 transition-all"
          />
        </div>

        <div className="bg-white rounded-xl border border-ink-200 overflow-hidden">
          <DataTable
            columns={[
              {
                key: 'name',
                header: lang === 'cn' ? '用户' : 'User',
                render: (u: User) => (
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-full bg-ink-100 flex items-center justify-center text-sm font-medium text-ink-600 shrink-0">
                      {u.name[0]}{u.name.split(' ')[1]?.[0] || ''}
                    </div>
                    <div>
                      <p className="text-sm font-medium text-ink-900">{u.name}</p>
                      <p className="text-xs text-ink-500">{u.email}</p>
                    </div>
                  </div>
                ),
              },
              {
                key: 'role',
                header: lang === 'cn' ? '角色' : 'Role',
                render: (u: User) => <span className="text-sm text-ink-700">{lang === 'cn' ? u.roleCn : u.role}</span>,
              },
              {
                key: 'teamAccess',
                header: lang === 'cn' ? '队伍权限' : 'Team Access',
                render: (u: User) => (
                  <span className="text-sm text-ink-600">
                    {u.teamAccess.length} {lang === 'cn' ? '个队伍' : 'teams'}
                  </span>
                ),
              },
              {
                key: 'lastActive',
                header: lang === 'cn' ? '最近活跃' : 'Last Active',
                align: 'right',
                render: (u: User) => <span className="text-sm text-ink-600">{new Date(u.lastActive).toLocaleDateString(lang === 'cn' ? 'zh-CN' : 'en-US', { month: 'short', day: 'numeric' })}</span>,
              },
              {
                key: 'status',
                header: tr('dataSources.status', lang),
                render: (u: User) => (
                  <span className={`text-sm font-medium ${u.status === 'active' ? 'text-success-600' : 'text-ink-500'}`}>
                    {u.status === 'active' ? (lang === 'cn' ? '活跃' : 'Active') : (lang === 'cn' ? '未活跃' : 'Inactive')}
                  </span>
                ),
              },
            ]}
            data={filtered}
            rowKey={(u) => u.id}
            emptyMessage={lang === 'cn' ? '没有符合条件的用户' : 'No users match your search'}
          />
        </div>
      </div>
    </div>
  );
}
