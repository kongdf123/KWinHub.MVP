import { useState, useEffect, useMemo, type ReactNode } from 'react';
import {
  Binary,
  Building2,
  ChevronDown,
  ClipboardList,
  Cpu,
  LayoutDashboard,
  LogOut,
  PanelLeftClose,
  PanelLeftOpen,
  Ruler,
  ScrollText,
  ShieldCheck,
  UserRound,
  Users,
  type LucideIcon,
} from 'lucide-react';
import { LanguageSwitcher, useI18n, type TranslationKey } from '../lib/i18n';
import { useHashRoute } from '../lib/router';
import { ThemeSwitcher } from './ThemeSwitcher';

type NavItem = {
  path: string;
  labelKey: TranslationKey;
  icon: LucideIcon;
};

type NavGroup = {
  labelKey: TranslationKey;
  items: NavItem[];
};

const NAV_GROUPS: NavGroup[] = [
  {
    labelKey: 'nav.group.overview',
    items: [{ path: '/', labelKey: 'nav.dashboard', icon: LayoutDashboard }],
  },
  {
    labelKey: 'nav.group.tenants',
    items: [
      { path: '/organizations', labelKey: 'nav.organizations', icon: Building2 },
      { path: '/users', labelKey: 'nav.users', icon: Users },
    ],
  },
  {
    labelKey: 'nav.group.catalogs',
    items: [
      { path: '/catalogs/protocols', labelKey: 'nav.protocols', icon: ClipboardList },
      { path: '/catalogs/metrics', labelKey: 'nav.metrics', icon: Ruler },
      { path: '/catalogs/devices', labelKey: 'nav.devices', icon: Cpu },
      { path: '/catalogs/algorithms', labelKey: 'nav.algorithms', icon: Binary },
    ],
  },
  {
    labelKey: 'nav.group.security',
    items: [{ path: '/audit-logs', labelKey: 'nav.auditLogs', icon: ScrollText }],
  },
];

function isActive(itemPath: string, currentPath: string): boolean {
  if (itemPath === '/') return currentPath === '/';
  return currentPath === itemPath || currentPath.startsWith(`${itemPath}/`);
}

function isGroupActive(group: NavGroup, currentPath: string): boolean {
  return group.items.some((item) => isActive(item.path, currentPath));
}

const SIDEBAR_EXPANDED = 'w-60';
const SIDEBAR_COMPACT = 'w-[72px]';
const CONTENT_OFFSET_EXPANDED = 'pl-60';
const CONTENT_OFFSET_COMPACT = 'pl-[72px]';

export function Layout({
  title,
  email,
  onSignOut,
  children,
}: {
  title: string;
  email: string;
  onSignOut: () => void;
  children: ReactNode;
}) {
  const { t, locale } = useI18n();
  const route = useHashRoute();
  const [collapsedGroups, setCollapsedGroups] = useState<Record<string, boolean>>({});
  const [compact, setCompact] = useState(false);

  const toggleGroup = (key: string) =>
    setCollapsedGroups((prev) => ({ ...prev, [key]: !prev[key] }));

  // Auto-expand the group that contains the active route without disturbing
  // the user's other collapse choices. The active group is derived via useMemo
  // and the effect uses a functional state update, so it never reads a stale
  // `collapsedGroups` closure — only `activeGroupKey` is a dependency.
  const activeGroupKey = useMemo(() => {
    const g = NAV_GROUPS.find((group) => isGroupActive(group, route.path));
    return g ? g.labelKey : null;
  }, [route.path]);

  useEffect(() => {
    if (!activeGroupKey) return;
    setCollapsedGroups((prev) =>
      prev[activeGroupKey] ? { ...prev, [activeGroupKey]: false } : prev,
    );
  }, [activeGroupKey]);

  const collapseLabel = compact
    ? locale === 'zh-CN'
      ? '展开侧边栏'
      : 'Expand sidebar'
    : locale === 'zh-CN'
      ? '收起侧边栏'
      : 'Collapse sidebar';
  const signOutLabel = t('layout.signOut');

  const renderItem = (item: NavItem) => {
    const Icon = item.icon;
    const active = isActive(item.path, route.path);
    const label = t(item.labelKey);
    const base =
      'relative w-full flex items-center gap-3 rounded-md font-medium transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent focus-visible:ring-offset-1 focus-visible:ring-offset-surface-sidebar';

    if (compact) {
      return (
        <a
          key={item.path}
          href={`#${item.path}`}
          aria-label={label}
          title={label}
          aria-current={active ? 'page' : undefined}
          className={`${base} h-9 justify-center ${
            active
              ? 'bg-accent-soft text-accent'
              : 'text-content-sidebar hover:bg-surface-sidebar-hover hover:text-content-sidebar'
          }`}
        >
          <Icon size={18} strokeWidth={1.75} className="shrink-0" />
        </a>
      );
    }

    return (
      <a
        key={item.path}
        href={`#${item.path}`}
        aria-current={active ? 'page' : undefined}
        className={`${base} h-9 px-3 border-l-2 ${
          active
            ? 'bg-accent-soft text-accent border-accent'
            : 'border-transparent text-content-sidebar hover:bg-surface-sidebar-hover hover:text-content-sidebar'
        }`}
      >
        <Icon size={18} strokeWidth={1.75} className="shrink-0" />
        <span className="truncate text-[13px]">{label}</span>
      </a>
    );
  };

  const renderGroup = (group: NavGroup) => {
    const groupActive = group.labelKey === activeGroupKey;
    const isCollapsed = collapsedGroups[group.labelKey];
    const groupId = `nav-group-${group.labelKey}`;

    if (compact) {
      return (
        <div key={group.labelKey} className="space-y-1">
          {group.items.map(renderItem)}
        </div>
      );
    }

    return (
      <div key={group.labelKey} className="space-y-1">
        <button
          type="button"
          onClick={() => toggleGroup(group.labelKey)}
          aria-expanded={!isCollapsed}
          aria-controls={groupId}
          className={`w-full flex items-center gap-2 px-3 pt-3 pb-1 text-[11px] font-semibold uppercase tracking-wider transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-accent rounded ${
            groupActive ? 'text-accent' : 'text-content-sidebar-muted hover:text-content-sidebar'
          }`}
        >
          <ChevronDown
            size={12}
            className={`shrink-0 transition-transform duration-150 motion-reduce:transition-none ${isCollapsed ? '-rotate-90' : ''}`}
          />
          <span className="truncate">{t(group.labelKey)}</span>
        </button>
        {!isCollapsed && (
          <div id={groupId} className="space-y-1">
            {group.items.map(renderItem)}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-surface-app">
      <aside
        className={`fixed inset-y-0 left-0 z-20 flex flex-col bg-surface-sidebar text-content-sidebar border-r border-sidebar transition-[width] duration-200 motion-reduce:transition-none ${
          compact ? SIDEBAR_COMPACT : SIDEBAR_EXPANDED
        }`}
      >
        {/* Brand block — logo, identity, and admin badge in one compact area */}
        <div className="px-3 pt-4 pb-3 shrink-0">
          <div className="flex items-center gap-2.5">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-accent text-surface-sidebar">
              <ShieldCheck size={18} />
            </div>
            {!compact && (
              <div className="min-w-0 flex-1">
                <div className="truncate text-sm font-semibold text-content-sidebar">
                  {t('layout.brand')}
                </div>
                <div className="truncate text-[11px] text-content-sidebar-muted">
                  {t('layout.console')}
                </div>
              </div>
            )}
          </div>
          {!compact && (
            <div className="mt-2 inline-flex rounded bg-accent-soft px-2 py-0.5 text-[10px] font-semibold tracking-widest text-accent">
              {t('layout.badge')}
            </div>
          )}
        </div>

        {/* Independently scrollable navigation */}
        <nav className="flex-1 overflow-y-auto px-2.5 py-2 scrollbar-thin">
          {NAV_GROUPS.map(renderGroup)}
        </nav>

        {/* Account footer — avatar + email, then sign-out and collapse toggle */}
        <div className="border-t border-sidebar px-2.5 py-2.5 space-y-1 shrink-0">
          {!compact && (
            <div className="flex items-center gap-2 px-1 py-1 min-w-0">
              <div className="w-7 h-7 rounded-full bg-accent-soft text-accent flex items-center justify-center shrink-0">
                <UserRound size={15} strokeWidth={1.75} />
              </div>
              <span className="truncate text-xs text-content-sidebar-muted" title={email}>
                {email}
              </span>
            </div>
          )}
          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={onSignOut}
              aria-label={signOutLabel}
              title={signOutLabel}
              className={`flex items-center gap-2 rounded-md text-content-sidebar-muted hover:bg-surface-sidebar-hover hover:text-content-sidebar transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent h-9 ${
                compact ? 'w-full justify-center' : 'px-3 text-[13px] font-medium flex-1'
              }`}
            >
              <LogOut size={18} strokeWidth={1.75} className="shrink-0" />
              {!compact && <span>{signOutLabel}</span>}
            </button>
            <button
              type="button"
              onClick={() => setCompact((c) => !c)}
              aria-label={collapseLabel}
              title={collapseLabel}
              aria-expanded={!compact}
              className={`flex items-center justify-center rounded-md text-content-sidebar-muted hover:bg-surface-sidebar-hover hover:text-content-sidebar transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent h-9 ${
                compact ? 'w-full' : 'w-9 shrink-0'
              }`}
            >
              {compact ? (
                <PanelLeftOpen size={18} strokeWidth={1.75} />
              ) : (
                <PanelLeftClose size={18} strokeWidth={1.75} />
              )}
            </button>
          </div>
        </div>
      </aside>

      {/* Main content — offset derived from the same sidebar state */}
      <div className={compact ? CONTENT_OFFSET_COMPACT : CONTENT_OFFSET_EXPANDED}>
        <header className="sticky top-0 z-10 flex h-14 items-center justify-between gap-3 border-b border-default bg-surface-header px-4 sm:px-6">
          <div className="flex min-w-0 items-center gap-2 text-sm text-content-secondary">
            <span className="shrink-0 whitespace-nowrap text-content-secondary">
              {t('layout.console')}
            </span>
            <ChevronDown size={14} className="shrink-0 -rotate-90 text-content-secondary" />
            <span className="truncate font-semibold text-content-primary">{title}</span>
          </div>
          <div className="flex items-center gap-2 sm:gap-3 shrink-0">
            <ThemeSwitcher />
            <LanguageSwitcher />
            <span className="rounded-full bg-surface-hover px-2.5 py-1 text-xs font-medium text-content-primary">
              {t('layout.adminRole')}
            </span>
          </div>
        </header>

        <main className="scrollbar-thin h-[calc(100vh-3.5rem)] overflow-y-auto bg-surface-app p-4 sm:p-6">
          {children}
        </main>
      </div>
    </div>
  );
}