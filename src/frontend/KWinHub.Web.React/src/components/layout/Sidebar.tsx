import { useState, useCallback } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { DEMO_ORG } from '@/lib/demo-data';
import { PRIMARY_NAV, type PrimaryNavItem } from '@/lib/navigation';
import { Activity, LogOut, PanelLeftClose, PanelLeftOpen } from 'lucide-react';

export function Sidebar({ onLogout }: { onLogout?: () => void }) {
  const { currentView, navigate, lang, canAccess } = useApp();
  const [compact, setCompact] = useState(false);

  // Flat primary navigation: each destination navigates to its first accessible
  // sub-view; destinations with no accessible sub-view are hidden for the role.
  const navItems = PRIMARY_NAV.map((item) => ({
    item,
    target: item.tabs.find((tab) => canAccess(tab.view))?.view,
  })).filter((entry): entry is { item: PrimaryNavItem; target: string } => !!entry.target);

  const isItemActive = useCallback(
    (item: PrimaryNavItem) => item.matchViews.includes(currentView),
    [currentView],
  );

  const renderNavItem = (item: PrimaryNavItem, target: string) => {
    const Icon = item.icon;
    const label = tr(item.labelKey, lang);
    const active = isItemActive(item);
    const base =
      'relative w-full flex items-center gap-3 rounded-lg font-medium transition-colors duration-150 motion-reduce:transition-none focus:outline-none focus-visible:ring-2 focus-visible:ring-accent focus-visible:ring-offset-2 focus-visible:ring-offset-surface-sidebar';

    if (compact) {
      return (
        <button
          key={item.id}
          type="button"
          onClick={() => navigate(target)}
          aria-label={label}
          title={label}
          aria-current={active ? 'page' : undefined}
          className={`${base} h-9 w-full justify-center ${
            active
              ? 'bg-surface-sidebar-active text-content-sidebar-active'
              : 'text-content-sidebar hover:bg-surface-sidebar-hover hover:text-content-sidebar'
          }`}
        >
          <Icon className="w-[18px] h-[18px] shrink-0" strokeWidth={1.75} />
        </button>
      );
    }

    return (
      <button
        key={item.id}
        type="button"
        onClick={() => navigate(target)}
        aria-current={active ? 'page' : undefined}
        className={`${base} h-9 px-3 border-l-[3px] ${
          active
            ? 'bg-surface-sidebar-active text-content-sidebar-active border-accent-highlight'
            : 'border-transparent text-content-sidebar hover:bg-surface-sidebar-hover hover:text-content-sidebar'
        }`}
      >
        <Icon className="w-[18px] h-[18px] shrink-0" strokeWidth={1.75} />
        <span className="truncate text-[13px]">{label}</span>
      </button>
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

      {/* Flat primary navigation — direct links, no expandable groups */}
      <nav className="flex-1 px-1.5 py-3 overflow-y-auto scrollbar-thin">
        <div className="space-y-1">
          {navItems.map(({ item, target }) => renderNavItem(item, target))}
        </div>
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
