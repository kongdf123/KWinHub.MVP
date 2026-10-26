import { useState, useRef, useEffect, useMemo } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { DEMO_TEAMS, DEMO_ATHLETES, DEMO_SESSIONS, getProtocol } from '@/lib/demo-data';
import { Search, Bell, ChevronDown, User, Settings, LogOut, HelpCircle, Languages } from 'lucide-react';
import { ThemeSwitcher } from './ThemeSwitcher';

interface TopBarProps {
  title: string;
  subtitle?: string;
  actions?: React.ReactNode;
}

type SearchResult = {
  id: string;
  label: string;
  sub: string;
  view: string;
  params: Record<string, string>;
  kind: 'athlete' | 'session';
};

export function TopBar({ title, subtitle, actions }: TopBarProps) {
  const { selectedTeamId, setSelectedTeamId, lang, setLang, navigate, logout, addToast, currentView, canAccess } = useApp();
  const [teamMenuOpen, setTeamMenuOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const [notifOpen, setNotifOpen] = useState(false);
  const [searchOpen, setSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const teamRef = useRef<HTMLDivElement>(null);
  const userRef = useRef<HTMLDivElement>(null);
  const notifRef = useRef<HTMLDivElement>(null);
  const searchRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (teamRef.current && !teamRef.current.contains(e.target as Node)) setTeamMenuOpen(false);
      if (userRef.current && !userRef.current.contains(e.target as Node)) setUserMenuOpen(false);
      if (notifRef.current && !notifRef.current.contains(e.target as Node)) setNotifOpen(false);
      if (searchRef.current && !searchRef.current.contains(e.target as Node)) setSearchOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const selectedTeam = DEMO_TEAMS.find((t) => t.id === selectedTeamId) || DEMO_TEAMS[0];

  const searchResults = useMemo<SearchResult[]>(() => {
    const q = searchQuery.trim().toLowerCase();
    if (!q) return [];
    const athletes: SearchResult[] = DEMO_ATHLETES
      .filter((a) => `${a.firstName} ${a.lastName}`.toLowerCase().includes(q) || a.position.toLowerCase().includes(q))
      .slice(0, 5)
      .map((a) => ({
        id: `ath-${a.id}`,
        label: `${a.lastName} ${a.firstName}`,
        sub: a.position,
        view: 'athlete-profile',
        params: { athleteId: a.id },
        kind: 'athlete' as const,
      }));
    const sessions: SearchResult[] = DEMO_SESSIONS
      .filter((s) => s.protocolName.toLowerCase().includes(q))
      .slice(0, 5)
      .map((s) => ({
        id: `ses-${s.id}`,
        label: getProtocol(s.protocolId)?.name || s.protocolName,
        sub: new Date(s.date).toLocaleDateString(lang === 'cn' ? 'zh-CN' : 'en-US'),
        view: 'session-detail',
        params: { sessionId: s.id },
        kind: 'session' as const,
      }));
    return [...athletes, ...sessions].slice(0, 8);
  }, [searchQuery, lang]);

  const notifications = lang === 'cn'
    ? [
        { msg: '张伟 需关注 — 不对称性 >15%', time: '10分钟前', action: () => navigate('athlete-profile', { athleteId: 'a-1' }) },
        { msg: '设备 KW-FP-00125 已离线', time: '2小时前', action: () => navigate('devices') },
        { msg: '3名运动员本周未测试', time: '5小时前', action: () => navigate('athletes') },
        { msg: '赛季前测试窗口将在3天后关闭', time: '1天前', action: () => navigate('sessions') },
      ]
    : [
        { msg: 'Zhang Wei flagged for attention — asymmetry >15%', time: '10 min ago', action: () => navigate('athlete-profile', { athleteId: 'a-1' }) },
        { msg: 'Device KW-FP-00125 went offline', time: '2 hours ago', action: () => navigate('devices') },
        { msg: '3 athletes have not tested this week', time: '5 hours ago', action: () => navigate('athletes') },
        { msg: 'Pre-season testing window closes in 3 days', time: '1 day ago', action: () => navigate('sessions') },
      ];

  // Contextual workspace tab group for the current primary destination.
  // Only top-level list views belong to a workspace; detail pages render their own tabs.
  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW[currentView] ?? []).filter((tab) => canAccess(tab.view));

  return (
    <header className="sticky top-0 z-30 bg-surface-header border-b border-default px-6 pt-3">
      <div className="flex items-center justify-between gap-4 pb-3">
      <div className="min-w-0 flex-1">
        <h2 className="text-lg font-semibold text-content-primary truncate">{title}</h2>
        {subtitle && <p className="text-sm text-content-secondary truncate">{subtitle}</p>}
      </div>

      <div className="flex items-center gap-3 shrink-0">
        {actions}

        <div className="relative hidden md:block" ref={searchRef}>
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-content-secondary" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => { setSearchQuery(e.target.value); setSearchOpen(true); }}
            onFocus={() => setSearchOpen(true)}
            placeholder={tr('common.search', lang)}
            className="w-56 pl-9 pr-3 py-2 text-sm bg-surface-hover border border-default rounded-lg focus:outline-none focus:ring-2 focus:ring-accent focus:border-accent transition-all"
          />
          {searchOpen && searchResults.length > 0 && (
            <div className="absolute right-0 top-full mt-1 w-72 bg-surface-panel rounded-lg shadow-lg border border-default py-1 z-20 animate-slide-up max-h-80 overflow-y-auto scrollbar-thin">
              {searchResults.map((r) => (
                <button
                  key={r.id}
                  onClick={() => { navigate(r.view, r.params); setSearchOpen(false); setSearchQuery(''); }}
                  className="w-full flex flex-col items-start px-3 py-2 hover:bg-surface-hover transition-colors text-left"
                >
                  <span className="text-sm text-content-primary">{r.label}</span>
                  <span className="text-xs text-content-secondary">{r.kind === 'athlete' ? (lang === 'cn' ? '运动员' : 'Athlete') : (lang === 'cn' ? '测试记录' : 'Session')} · {r.sub}</span>
                </button>
              ))}
            </div>
          )}
        </div>

        <button
          onClick={() => setLang(lang === 'cn' ? 'en' : 'cn')}
          className="flex items-center gap-1.5 px-2.5 py-2 text-sm font-medium text-content-secondary hover:bg-surface-hover rounded-lg transition-colors"
          title={lang === 'cn' ? 'Switch to English' : '切换中文'}
        >
          <Languages className="w-4 h-4" />
          {lang === 'cn' ? 'EN' : '中'}
        </button>

        <ThemeSwitcher />

        <div className="relative" ref={teamRef}>
          <button
            onClick={() => setTeamMenuOpen(!teamMenuOpen)}
            className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-content-primary bg-surface-hover hover:bg-surface-hover rounded-lg transition-colors"
          >
            {lang === 'cn' ? selectedTeam.nameCn : selectedTeam.name}
            <ChevronDown className="w-4 h-4 text-content-secondary" />
          </button>
          {teamMenuOpen && (
            <div className="absolute right-0 top-full mt-1 w-56 bg-surface-panel rounded-lg shadow-lg border border-default py-1 z-20 animate-slide-up">
              {DEMO_TEAMS.map((team) => (
                <button
                  key={team.id}
                  onClick={() => { setSelectedTeamId(team.id); setTeamMenuOpen(false); }}
                  className={`w-full flex items-center justify-between px-3 py-2 text-sm hover:bg-surface-hover transition-colors ${team.id === selectedTeamId ? 'text-accent font-medium' : 'text-content-primary'}`}
                >
                  <span>{lang === 'cn' ? team.nameCn : team.name}</span>
                  <span className="text-xs text-content-secondary">{team.athleteCount}</span>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="relative" ref={notifRef}>
          <button
            onClick={() => setNotifOpen(!notifOpen)}
            className="relative p-2 text-content-secondary hover:text-content-primary hover:bg-surface-hover rounded-lg transition-colors"
          >
            <Bell className="w-5 h-5" />
            <span className="absolute top-1 right-1 w-2 h-2 bg-attention-500 rounded-full" />
          </button>
          {notifOpen && (
            <div className="absolute right-0 top-full mt-1 w-80 bg-surface-panel rounded-lg shadow-lg border border-default py-2 z-20 animate-slide-up">
              <div className="px-3 py-2 border-b border-default">
                <h3 className="text-sm font-semibold text-content-primary">{lang === 'cn' ? '通知' : 'Notifications'}</h3>
              </div>
              <div className="max-h-72 overflow-y-auto scrollbar-thin">
                {notifications.map((n, i) => (
                  <button
                    key={i}
                    onClick={() => { n.action(); setNotifOpen(false); }}
                    className="w-full text-left px-3 py-2.5 hover:bg-surface-hover border-b border-default last:border-0 transition-colors"
                  >
                    <p className="text-sm text-content-primary">{n.msg}</p>
                    <p className="text-xs text-content-secondary mt-0.5">{n.time}</p>
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>

        <div className="relative" ref={userRef}>
          <button
            onClick={() => setUserMenuOpen(!userMenuOpen)}
            className="flex items-center gap-2 p-1 pr-2 hover:bg-surface-hover rounded-lg transition-colors"
          >
            <div className="w-8 h-8 rounded-full bg-accent flex items-center justify-center text-surface-sidebar text-sm font-medium">
              LW
            </div>
            <ChevronDown className="w-4 h-4 text-content-secondary" />
          </button>
          {userMenuOpen && (
            <div className="absolute right-0 top-full mt-1 w-48 bg-surface-panel rounded-lg shadow-lg border border-default py-1 z-20 animate-slide-up">
              <div className="px-3 py-2 border-b border-default">
                <p className="text-sm font-medium text-content-primary">Li Wei</p>
                <p className="text-xs text-content-secondary">{lang === 'cn' ? '组织管理员' : 'Organization Admin'}</p>
              </div>
              <button onClick={() => { addToast(lang === 'cn' ? '个人资料功能即将推出' : 'Profile coming soon', 'info'); setUserMenuOpen(false); }} className="w-full flex items-center gap-2 px-3 py-2 text-sm text-content-primary hover:bg-surface-hover transition-colors">
                <User className="w-4 h-4" /> {lang === 'cn' ? '个人资料' : 'My Profile'}
              </button>
              <button onClick={() => { navigate('settings'); setUserMenuOpen(false); }} className="w-full flex items-center gap-2 px-3 py-2 text-sm text-content-primary hover:bg-surface-hover transition-colors">
                <Settings className="w-4 h-4" /> {lang === 'cn' ? '设置' : 'Settings'}
              </button>
              <button onClick={() => { addToast(lang === 'cn' ? '帮助功能即将推出' : 'Help coming soon', 'info'); setUserMenuOpen(false); }} className="w-full flex items-center gap-2 px-3 py-2 text-sm text-content-primary hover:bg-surface-hover transition-colors">
                <HelpCircle className="w-4 h-4" /> {lang === 'cn' ? '帮助' : 'Help & Support'}
              </button>
              <div className="border-t border-default mt-1 pt-1">
                <button onClick={() => { logout(); }} className="w-full flex items-center gap-2 px-3 py-2 text-sm text-invalid-600 hover:bg-invalid-50 transition-colors">
                  <LogOut className="w-4 h-4" /> {lang === 'cn' ? '退出登录' : 'Sign Out'}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
      </div>

      {workspaceTabs.length > 1 && (
        <nav
          aria-label={lang === 'cn' ? '工作区导航' : 'Workspace navigation'}
          className="flex items-center gap-1 overflow-x-auto scrollbar-thin"
        >
          {workspaceTabs.map((tab) => {
            const active = tab.view === currentView;
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
    </header>
  );
}