import { useState, type ReactNode } from 'react';
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

// Flat primary navigation — one item per product area. Areas with sub-sections
// (e.g. Scientific Catalogs) expose them as route-aware tabs in the main layout.
const NAV_ITEMS: NavItem[] = [
  { path: '/', labelKey: 'nav.dashboard', icon: LayoutDashboard },
  { path: '/organizations', labelKey: 'nav.organizations', icon: Building2 },
  { path: '/users', labelKey: 'nav.users', icon: Users },
  { path: '/catalogs/protocols', labelKey: 'nav.catalogs', icon: ClipboardList },
  { path: '/audit-logs', labelKey: 'nav.security', icon: ScrollText },
];

// Section tabs for the Scientific Catalogs area. Each tab is a real route so
// refresh, deep links, and browser Back/Forward keep working.
const CATALOG_TABS: NavItem[] = [
  { path: '/catalogs/protocols', labelKey: 'nav.protocols', icon: ClipboardList },
  { path: '/catalogs/metrics', labelKey: 'nav.metrics', icon: Ruler },
  { path: '/catalogs/devices', labelKey: 'nav.devices', icon: Cpu },
  { path: '/catalogs/algorithms', labelKey: 'nav.algorithms', icon: Binary },
];

function isActive(itemPath: string, currentPath: string): boolean {
  if (itemPath === '/') return currentPath === '/';
  return currentPath === itemPath || currentPath.startsWith(`${itemPath}/`);
}

// A primary item is active when its own route matches, or when the current
// route belongs to one of its sections (e.g. any /catalogs/* route activates
// the Scientific Catalogs item).
function isNavItemActive(item: NavItem, currentPath: string): boolean {
  if (item.labelKey === 'nav.catalogs') return currentPath.startsWith('/catalogs');
  return isActive(item.path, currentPath);
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
  const [compact, setCompact] = useState(false);

  const collapseLabel = compact
    ? locale === 'zh-CN'
      ? '展开侧边栏'
      : 'Expand sidebar'
    : locale === 'zh-CN'
      ? '收起侧边栏'
      : 'Collapse sidebar';
  const signOutLabel = t('layout.signOut');
  const showCatalogTabs = route.path.startsWith('/catalogs');

  const renderItem = (item: NavItem) => {
    const Icon = item.icon;
    const active = isNavItemActive(item, route.path);
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
          <div className="space-y-1">
            {NAV_ITEMS.map(renderItem)}
          </div>
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
        <div className="flex h-screen flex-col">
          <header className="shrink-0 border-b border-default bg-surface-header px-4 sm:px-6">
            <div className="flex h-14 items-center justify-between gap-3">
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
            </div>

            {/* Section tabs for the Scientific Catalogs area — real routes */}
            {showCatalogTabs && (
              <nav
                aria-label={t('nav.catalogs')}
                className="flex items-center gap-1 overflow-x-auto scrollbar-thin"
              >
                {CATALOG_TABS.map((tab) => {
                  const active = isActive(tab.path, route.path);
                  return (
                    <a
                      key={tab.path}
                      href={`#${tab.path}`}
                      aria-current={active ? 'page' : undefined}
                      className={`px-4 py-2.5 -mb-px text-sm font-medium border-b-2 whitespace-nowrap transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent ${
                        active
                          ? 'border-accent text-accent'
                          : 'border-transparent text-content-secondary hover:text-content-primary'
                      }`}
                    >
                      {t(tab.labelKey)}
                    </a>
                  );
                })}
              </nav>
            )}
          </header>

          <main className="scrollbar-thin flex-1 overflow-y-auto bg-surface-app p-4 sm:p-6">
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}