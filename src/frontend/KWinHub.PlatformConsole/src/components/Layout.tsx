import { useState, type ReactNode } from 'react';
import {
  Building2,
  ClipboardList,
  GitBranch,
  LayoutDashboard,
  LogOut,
  PanelLeftClose,
  PanelLeftOpen,
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
// (e.g. Foundations) expose them as route-aware tabs in the page content.
const NAV_ITEMS: NavItem[] = [
  { path: '/', labelKey: 'nav.dashboard', icon: LayoutDashboard },
  { path: '/organizations', labelKey: 'nav.organizations', icon: Building2 },
  { path: '/users', labelKey: 'nav.users', icon: Users },
  { path: '/catalogs/protocols', labelKey: 'nav.catalogs', icon: ClipboardList },
  { path: '/audit-logs', labelKey: 'nav.security', icon: ScrollText },
  { path: '/workflows', labelKey: 'nav.workflows', icon: GitBranch },
];

function isActive(itemPath: string, currentPath: string): boolean {
  if (itemPath === '/') return currentPath === '/';
  return currentPath === itemPath || currentPath.startsWith(`${itemPath}/`);
}

// A primary item is active when its own route matches, or when the current
// route belongs to one of its sections (e.g. any /catalogs/* route activates
// the Foundations item).
function isNavItemActive(item: NavItem, currentPath: string): boolean {
  if (item.labelKey === 'nav.catalogs') return currentPath.startsWith('/catalogs');
  return isActive(item.path, currentPath);
}

const SIDEBAR_EXPANDED = 'w-60';
const SIDEBAR_COMPACT = 'w-[68px]';
const CONTENT_OFFSET_EXPANDED = 'pl-60';
const CONTENT_OFFSET_COMPACT = 'pl-[68px]';

export function Layout({
  email,
  onSignOut,
  children,
}: {
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
              ? 'bg-surface-sidebar-active text-content-sidebar-active'
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
        className={`${base} h-9 px-3 border-l-[3px] ${
          active
            ? 'bg-surface-sidebar-active text-content-sidebar-active border-accent-highlight'
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
        {/* Brand header — aligned with the app top bar (h-16) */}
        <div className="h-16 px-3 flex items-center gap-2.5 shrink-0">
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
          <header className="sticky top-0 z-30 shrink-0 border-b border-default bg-surface-header">
            <div className="flex h-16 items-center justify-between gap-4 px-6">
              <div className="min-w-0 flex-1" />

              <div className="flex shrink-0 items-center gap-3">
                <LanguageSwitcher />
                <ThemeSwitcher />
                <span className="rounded-lg bg-surface-hover px-3 py-2 text-sm font-medium text-content-primary">
                  {t('layout.adminRole')}
                </span>
              </div>
            </div>
          </header>

          <main className="scrollbar-thin flex-1 overflow-y-auto bg-surface-app p-4 sm:p-6">
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}