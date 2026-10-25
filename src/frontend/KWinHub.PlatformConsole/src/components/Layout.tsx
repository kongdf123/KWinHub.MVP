import { useState, type ReactNode } from 'react';
import {
  Binary,
  Building2,
  ChevronDown,
  ClipboardList,
  Cpu,
  LayoutDashboard,
  LogOut,
  Ruler,
  ScrollText,
  ShieldCheck,
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
  const { t } = useI18n();
  const route = useHashRoute();
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>({});

  const toggle = (label: string) => setCollapsed((prev) => ({ ...prev, [label]: !prev[label] }));

  return (
    <div className="min-h-screen bg-surface-app">
      <aside className="fixed inset-y-0 left-0 z-20 flex w-[240px] flex-col bg-surface-sidebar text-content-sidebar border-r border-sidebar">
        <div className="px-5 pb-3 pt-5">
          <div className="flex items-center gap-2.5">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-accent text-surface-sidebar">
              <ShieldCheck size={18} />
            </div>
            <div className="min-w-0">
              <div className="truncate text-sm font-semibold text-content-sidebar">{t('layout.brand')}</div>
              <div className="truncate text-[11px] text-content-sidebar-muted">{t('layout.console')}</div>
            </div>
          </div>
          <div className="mt-3 inline-flex rounded bg-accent-soft px-2 py-0.5 text-[10px] font-semibold tracking-widest text-accent">
            {t('layout.badge')}
          </div>
        </div>

        <nav className="flex-1 overflow-y-auto px-3 py-2">
          {NAV_GROUPS.map((group) => {
            const groupActive = isGroupActive(group, route.path);
            const isCollapsed = collapsed[group.labelKey];
            return (
              <div key={group.labelKey} className="mb-1">
                <button
                  type="button"
                  onClick={() => toggle(group.labelKey)}
                  className={`flex w-full items-center gap-1.5 px-3 pb-1 pt-3 text-[10px] font-semibold uppercase tracking-wider transition-colors ${
                    groupActive ? 'text-accent' : 'text-content-sidebar-muted hover:text-content-sidebar'
                  }`}
                >
                  <ChevronDown
                    size={12}
                    className={`shrink-0 transition-transform ${isCollapsed ? '-rotate-90' : ''}`}
                  />
                  {t(group.labelKey)}
                </button>
                {!isCollapsed &&
                  group.items.map((item) => {
                    const Icon = item.icon;
                    const active = isActive(item.path, route.path);
                    return (
                      <a
                        key={item.path}
                        href={`#${item.path}`}
                        className={`mb-0.5 flex items-center gap-2.5 rounded-md border-l-2 px-3 py-2 text-sm transition-colors ${
                          active
                            ? 'border-accent bg-accent-soft font-medium text-accent'
                            : 'border-transparent text-content-sidebar hover:bg-surface-sidebar-hover hover:text-content-primary'
                        }`}
                      >
                        <Icon size={17} />
                        <span className="truncate">{t(item.labelKey)}</span>
                      </a>
                    );
                  })}
              </div>
            );
          })}
        </nav>

        <div className="border-t border-sidebar p-4">
          <div className="truncate text-xs text-content-sidebar-muted" title={email}>
            {email}
          </div>
          <button
            type="button"
            onClick={onSignOut}
            className="mt-2 flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-content-sidebar transition-colors hover:bg-surface-sidebar-hover hover:text-content-primary"
          >
            <LogOut size={16} />
            {t('layout.signOut')}
          </button>
        </div>
      </aside>

      <div className="pl-[240px]">
        <header className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-default bg-surface-header px-6">
          <div className="flex min-w-0 items-center gap-2 text-sm text-content-secondary">
            <span className="whitespace-nowrap">{t('layout.console')}</span>
            <ChevronDown size={14} className="shrink-0 -rotate-90" />
            <span className="truncate font-semibold text-content-primary">{title}</span>
          </div>
          <div className="flex items-center gap-3">
            <ThemeSwitcher />
            <LanguageSwitcher />
            <span className="rounded-full bg-surface-hover px-2.5 py-1 text-xs font-medium text-content-primary">
              {t('layout.adminRole')}
            </span>
          </div>
        </header>

        <main className="scrollbar-thin h-[calc(100vh-3.5rem)] overflow-y-auto bg-surface-app p-6">
          {children}
        </main>
      </div>
    </div>
  );
}