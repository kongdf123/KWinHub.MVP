import { useState, useEffect, useMemo, useCallback } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { DEMO_ORG } from '@/lib/demo-data';
import {
  Activity, LayoutDashboard, Users, ClipboardList, BarChart3,
  TrendingUp, GitCompare, Waves, FileText, Database, Server,
  HardDrive, Plug, RefreshCw, ChevronDown,
  Building2, UserCog, ShieldCheck, UsersRound, Cpu, Settings,
  LogOut, PanelLeftClose, PanelLeftOpen, type LucideIcon,
} from 'lucide-react';

interface NavItem {
  view: string;
  label: string;
  icon: LucideIcon;
  matchViews?: string[];
}

interface NavGroup {
  id: string;
  labelKey: string;
  items: NavItem[];
}

export function Sidebar({ onLogout }: { onLogout?: () => void }) {
  const { currentView, navigate, lang, canAccess } = useApp();
  const [collapsedGroups, setCollapsedGroups] = useState<Record<string, boolean>>({});
  const [compact, setCompact] = useState(false);

  const toggleGroup = useCallback((id: string) => {
    setCollapsedGroups((prev) => ({ ...prev, [id]: !prev[id] }));
  }, []);

  const topItem: NavItem = {
    view: 'home',
    label: tr('nav.home', lang),
    icon: LayoutDashboard,
  };

  const allItems: NavItem[] = [
    { view: 'athletes', label: tr('nav.athletes', lang), icon: Users, matchViews: ['athletes', 'athlete-profile'] },
    { view: 'teams', label: tr('nav.teams', lang), icon: UsersRound, matchViews: ['teams'] },
    { view: 'sessions', label: tr('nav.sessions', lang), icon: ClipboardList, matchViews: ['sessions', 'session-detail'] },
    { view: 'protocols', label: tr('nav.protocols', lang), icon: FileText, matchViews: ['protocols', 'protocol-detail'] },
    { view: 'devices', label: tr('nav.devices', lang), icon: Cpu, matchViews: ['devices'] },
    { view: 'sync-center', label: tr('nav.syncCenter', lang), icon: RefreshCw, matchViews: ['sync-center'] },
    { view: 'results', label: tr('nav.results', lang), icon: BarChart3, matchViews: ['results', 'result-detail'] },
    { view: 'performance', label: tr('nav.performance', lang), icon: TrendingUp, matchViews: ['performance'] },
    { view: 'trends', label: tr('nav.trends', lang), icon: TrendingUp, matchViews: ['trends'] },
    { view: 'comparisons', label: tr('nav.comparisons', lang), icon: GitCompare, matchViews: ['comparisons'] },
    { view: 'biomechanics', label: tr('nav.biomechanics', lang), icon: Waves, matchViews: ['biomechanics'] },
    { view: 'dashboards', label: tr('nav.dashboards', lang), icon: LayoutDashboard, matchViews: ['dashboards'] },
    { view: 'reports', label: tr('nav.reports', lang), icon: FileText, matchViews: ['reports'] },
    { view: 'datasets', label: tr('nav.datasets', lang), icon: Database, matchViews: ['datasets', 'dataset-detail'] },
    { view: 'raw-data', label: tr('nav.rawData', lang), icon: HardDrive, matchViews: ['raw-data'] },
    { view: 'data-sources', label: tr('nav.dataSources', lang), icon: Server, matchViews: ['data-sources', 'data-source-detail'] },
    { view: 'integrations', label: tr('nav.integrations', lang), icon: Plug, matchViews: ['integrations'] },
    { view: 'organization', label: tr('nav.organization', lang), icon: Building2, matchViews: ['organization'] },
    { view: 'users', label: tr('nav.users', lang), icon: UserCog, matchViews: ['users'] },
    { view: 'roles', label: tr('nav.roles', lang), icon: ShieldCheck, matchViews: ['roles'] },
    { view: 'settings', label: tr('nav.settings', lang), icon: Settings, matchViews: ['settings'] },
  ];

  const byView = new Map(allItems.map((i) => [i.view, i]));
  const pick = (views: string[]): NavItem[] =>
    views.map((v) => byView.get(v)).filter((i): i is NavItem => !!i && canAccess(i.view));

  const groups = useMemo<NavGroup[]>(
    () =>
      [
        { id: 'people', labelKey: 'nav.group.people', items: pick(['athletes', 'teams']) },
        { id: 'testing', labelKey: 'nav.group.testing', items: pick(['sessions', 'protocols', 'devices', 'sync-center']) },
        {
          id: 'analysis',
          labelKey: 'nav.group.analysis',
          items: pick(['results', 'performance', 'trends', 'comparisons', 'biomechanics', 'dashboards', 'reports']),
        },
        {
          id: 'data',
          labelKey: 'nav.group.data',
          items: pick(['datasets', 'raw-data', 'data-sources', 'integrations']),
        },
        { id: 'admin', labelKey: 'nav.group.admin', items: pick(['organization', 'users', 'roles', 'settings']) },
      ].filter((g) => g.items.length > 0),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [lang, canAccess],
  );

  const isItemActive = useCallback(
    (item: NavItem) => {
      const matchViews = item.matchViews || [item.view];
      return matchViews.includes(currentView);
    },
    [currentView],
  );

  const activeGroupId = useMemo(() => {
    for (const g of groups) {
      if (g.items.some(isItemActive)) return g.id;
    }
    return null;
  }, [groups, isItemActive]);

  // Auto-expand the group that contains the active page without disturbing the
  // user's other collapse choices. Uses a functional state update so it never
  // reads a stale `collapsedGroups` closure, and depends only on `activeGroupId`.
  useEffect(() => {
    if (!activeGroupId) return;
    setCollapsedGroups((prev) =>
      prev[activeGroupId] ? { ...prev, [activeGroupId]: false } : prev,
    );
  }, [activeGroupId]);

  const renderNavItem = (item: NavItem) => {
    const Icon = item.icon;
    const active = isItemActive(item);
    const base =
      'relative w-full flex items-center gap-3 rounded-lg font-medium transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent focus-visible:ring-offset-2 focus-visible:ring-offset-surface-sidebar';

    if (compact) {
      return (
        <button
          key={item.view}
          type="button"
          onClick={() => navigate(item.view)}
          aria-label={item.label}
          title={item.label}
          aria-current={active ? 'page' : undefined}
          className={`${base} h-9 w-full justify-center ${
            active
              ? 'bg-accent-soft text-accent'
              : 'text-content-sidebar hover:bg-surface-sidebar-hover hover:text-content-sidebar'
          }`}
        >
          <Icon className="w-[18px] h-[18px] shrink-0" strokeWidth={1.75} />
        </button>
      );
    }

    return (
      <button
        key={item.view}
        type="button"
        onClick={() => navigate(item.view)}
        aria-current={active ? 'page' : undefined}
        className={`${base} h-9 px-3 border-l-2 ${
          active
            ? 'bg-accent-soft text-accent border-accent'
            : 'border-transparent text-content-sidebar hover:bg-surface-sidebar-hover hover:text-content-sidebar'
        }`}
      >
        <Icon className="w-[18px] h-[18px] shrink-0" strokeWidth={1.75} />
        <span className="truncate text-[13px]">{item.label}</span>
      </button>
    );
  };

  const renderGroup = (group: NavGroup) => {
    const collapsed = collapsedGroups[group.id];
    const groupActive = group.id === activeGroupId;

    if (compact) {
      return (
        <div key={group.id} className="space-y-1">
          {group.items.map(renderNavItem)}
        </div>
      );
    }

    return (
      <div key={group.id}>
        <button
          type="button"
          onClick={() => toggleGroup(group.id)}
          aria-expanded={!collapsed}
          className={`w-full flex items-center gap-2 px-3 pt-4 pb-1.5 text-[11px] font-semibold uppercase tracking-wider transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-accent rounded ${
            groupActive ? 'text-accent' : 'text-content-sidebar-muted hover:text-content-sidebar'
          }`}
        >
          <ChevronDown className={`w-3 h-3 transition-transform duration-150 motion-reduce:transition-none shrink-0 ${collapsed ? '-rotate-90' : ''}`} />
          <span className="truncate">{tr(group.labelKey, lang)}</span>
        </button>
        {!collapsed && (
          <div className="space-y-1">
            {group.items.map(renderNavItem)}
          </div>
        )}
      </div>
    );
  };

  const orgName = lang === 'cn' ? DEMO_ORG.nameCn : DEMO_ORG.name;
  const collapseLabel = compact
    ? (lang === 'cn' ? '展开侧边栏' : 'Expand sidebar')
    : (lang === 'cn' ? '收起侧边栏' : 'Collapse sidebar');
  const signOutLabel = lang === 'cn' ? '退出登录' : 'Sign out';

  return (
    <aside
      className={`${compact ? 'w-[68px]' : 'w-60'} bg-surface-sidebar text-content-sidebar flex flex-col h-screen sticky top-0 shrink-0 transition-[width] duration-200 motion-reduce:transition-none`}
    >
      {/* Brand header — aligned with the app top bar (h-16) */}
      <div className="h-16 px-3 flex items-center gap-2.5 shrink-0">
        <div className="w-9 h-9 rounded-lg bg-accent flex items-center justify-center shrink-0">
          <Activity className="w-5 h-5 text-surface-sidebar" />
        </div>
        {!compact && (
          <div className="min-w-0 flex-1">
            <h1 className="text-sm font-semibold text-content-sidebar truncate">Kunwei Hub</h1>
            <p className="text-xs text-content-sidebar-muted truncate">{orgName}</p>
          </div>
        )}
      </div>

      {/* Independently scrollable navigation */}
      <nav className="flex-1 px-1.5 py-3 overflow-y-auto scrollbar-thin">
        {canAccess(topItem.view) && (
          <div className="mb-2">
            {renderNavItem(topItem)}
          </div>
        )}
        {groups.map(renderGroup)}
      </nav>

      {/* Compact footer — no live-telemetry claims for static demo data */}
      <div className="border-t border-sidebar px-2.5 py-2.5 space-y-1 shrink-0">
        {!compact && (
          <div className="px-2 py-1 text-[11px] text-content-sidebar-muted flex items-center gap-1.5">
            <span className="w-1.5 h-1.5 rounded-full bg-success-500 shrink-0" />
            <span className="truncate">v2.1 · {lang === 'cn' ? '演示环境' : 'Demo'}</span>
          </div>
        )}
        <div className="flex items-center gap-1">
          {onLogout && (
            <button
              type="button"
              onClick={onLogout}
              aria-label={signOutLabel}
              title={signOutLabel}
              className={`flex items-center gap-2 rounded-lg text-content-sidebar-muted hover:bg-surface-sidebar-hover hover:text-content-sidebar transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent ${
                compact ? 'h-9 w-full justify-center' : 'h-9 px-3 text-[13px] font-medium flex-1'
              }`}
            >
              <LogOut className="w-[18px] h-[18px] shrink-0" strokeWidth={1.75} />
              {!compact && <span>{signOutLabel}</span>}
            </button>
          )}
          <button
            type="button"
            onClick={() => setCompact((c) => !c)}
            aria-label={collapseLabel}
            title={collapseLabel}
            aria-expanded={!compact}
            className={`flex items-center justify-center rounded-lg text-content-sidebar-muted hover:bg-surface-sidebar-hover hover:text-content-sidebar transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent h-9 ${
              compact ? 'w-full' : 'w-9 shrink-0'
            }`}
          >
            {compact ? <PanelLeftOpen className="w-[18px] h-[18px]" strokeWidth={1.75} /> : <PanelLeftClose className="w-[18px] h-[18px]" strokeWidth={1.75} />}
          </button>
        </div>
      </div>
    </aside>
  );
}