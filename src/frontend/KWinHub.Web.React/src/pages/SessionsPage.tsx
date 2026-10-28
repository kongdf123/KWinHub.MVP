import { useState, useMemo } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { DEMO_SESSIONS, DEMO_ATHLETES, DEMO_PROTOCOLS, DEMO_DATA_SOURCES, getSport, getProtocol, getDataSource } from '@/lib/demo-data';
// metric labels handled inline via protocol definitions
import { TopBar } from '@/components/layout/TopBar';
import { DataTable } from '@/components/ui/DataTable';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { Upload, Search, Activity, Zap, Waves, TrendingUp, Dumbbell, Shuffle, ChevronRight, CheckCircle, XCircle, RefreshCw, Download, MoreVertical, FileText, BarChart3 } from 'lucide-react';
import type { TestSession, SessionStatus } from '@/lib/types';
import {
  getSessionScientificInfo,
  getSourceApplication,
  type ScientificFixtureSummaryStatus,
} from '@/features/science';

const SPORT_ICONS: Record<string, typeof Activity> = {
  'sport-cmj': Activity,
  'sport-sprint': Zap,
  'sport-swim': Waves,
  'sport-hj': TrendingUp,
  'sport-imtp': Dumbbell,
  'sport-cod': Shuffle,
};

const SESSION_STATUS_CONFIG: Record<SessionStatus, { en: string; cn: string; color: string; icon: typeof CheckCircle }> = {
  LOCAL: { en: 'Local', cn: '本地', color: 'text-ink-500', icon: CheckCircle },
  QUEUED: { en: 'Queued', cn: '排队中', color: 'text-ink-500', icon: RefreshCw },
  UPLOADING: { en: 'Uploading', cn: '上传中', color: 'text-teal-600', icon: RefreshCw },
  RECEIVED: { en: 'Received', cn: '已接收', color: 'text-teal-600', icon: CheckCircle },
  VALIDATING: { en: 'Validating', cn: '验证中', color: 'text-teal-600', icon: RefreshCw },
  PROCESSING: { en: 'Processing', cn: '处理中', color: 'text-teal-600', icon: RefreshCw },
  ANALYZED: { en: 'Analyzed', cn: '已分析', color: 'text-success-600', icon: CheckCircle },
  PUBLISHED: { en: 'Published', cn: '已发布', color: 'text-success-600', icon: CheckCircle },
  UPLOAD_FAILED: { en: 'Upload Failed', cn: '上传失败', color: 'text-invalid-600', icon: XCircle },
  VALIDATION_FAILED: { en: 'Validation Failed', cn: '验证失败', color: 'text-invalid-600', icon: XCircle },
  PROCESSING_FAILED: { en: 'Processing Failed', cn: '处理失败', color: 'text-invalid-600', icon: XCircle },
};

const SCI_STATUS_CONFIG: Record<ScientificFixtureSummaryStatus, { en: string; cn: string; color: string; bg: string }> = {
  local: { en: 'Local', cn: '本地', color: 'text-ink-600', bg: 'bg-ink-100' },
  uploading: { en: 'Uploading', cn: '上传中', color: 'text-teal-700', bg: 'bg-teal-50' },
  validating: { en: 'Validating', cn: '校验中', color: 'text-teal-700', bg: 'bg-teal-50' },
  processing: { en: 'Calculating', cn: '计算中', color: 'text-teal-700', bg: 'bg-teal-50' },
  published: { en: 'Published', cn: '已发布', color: 'text-success-700', bg: 'bg-success-50' },
  failed: { en: 'Failed', cn: '失败', color: 'text-invalid-700', bg: 'bg-invalid-50' },
};

export function SessionsPage() {
  const { navigate, addToast, lang, canAccess } = useApp();
  const [search, setSearch] = useState('');
  const [protocolFilter, setProtocolFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');
  const [sourceFilter, setSourceFilter] = useState('all');
  const [dateFilter, setDateFilter] = useState('all');

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['sessions'] ?? []).filter((tab) => canAccess(tab.view));

  const filteredSessions = useMemo(() => {
    return DEMO_SESSIONS.filter((s) => {
      const athlete = DEMO_ATHLETES.find((a) => a.id === s.athleteId);
      const name = athlete ? `${athlete.lastName} ${athlete.firstName}` : '';
      const matchesSearch = name.toLowerCase().includes(search.toLowerCase()) || s.protocolName.toLowerCase().includes(search.toLowerCase());
      const matchesProtocol = protocolFilter === 'all' || s.protocolId === protocolFilter;
      const matchesStatus = statusFilter === 'all' || s.sessionStatus === statusFilter;
      const matchesSource = sourceFilter === 'all' || s.dataSourceId === sourceFilter;
      const matchesDate = dateFilter === 'all' || (() => {
        const days = parseInt(dateFilter);
        const sessionDate = new Date(s.date);
        const cutoff = new Date();
        cutoff.setDate(cutoff.getDate() - days);
        return sessionDate >= cutoff;
      })();
      return matchesSearch && matchesProtocol && matchesStatus && matchesSource && matchesDate;
    }).sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
  }, [search, protocolFilter, statusFilter, sourceFilter, dateFilter]);

  const getPrimaryResult = (session: TestSession): string => {
    const protocol = getProtocol(session.protocolId);
    if (!protocol) return '—';
    const primaryMetric = protocol.metrics.find((m) => m.primary);
    if (!primaryMetric) return '—';
    const value = session.summary[primaryMetric.key];
    if (value === undefined) return '—';
    return `${value.toFixed(2)} ${primaryMetric.unit}`;
  };

  const getSessionStatusDisplay = (status: SessionStatus) => {
    const config = SESSION_STATUS_CONFIG[status] || SESSION_STATUS_CONFIG.RECEIVED;
    const Icon = config.icon;
    const isProcessing = status === 'PROCESSING' || status === 'VALIDATING' || status === 'UPLOADING';
    return (
      <span className={`inline-flex items-center gap-1.5 text-xs font-medium ${config.color}`}>
        <Icon className={`w-3.5 h-3.5 ${isProcessing ? 'animate-sync-spin' : ''}`} />
        {lang === 'cn' ? config.cn : config.en}
      </span>
    );
  };

  const sourceOptions = DEMO_DATA_SOURCES.filter((ds) => ds.category === 'APPLICATION' || ds.category === 'DEVICE' || ds.category === 'INTEGRATION');

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.workspace.testing', lang)}</h2>
          <div className="text-sm text-content-secondary">{filteredSessions.length}</div>
        </div>

        {workspaceTabs.length > 1 && (
          <nav aria-label={tr('nav.workspace.testing', lang)} className="flex items-center gap-1 mt-4 border-b border-default">
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'sessions';
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
        <div className="flex flex-wrap items-center gap-3">
          <div className="relative flex-1 min-w-[200px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-content-muted" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={tr('sessions.searchPlaceholder', lang)}
              className="w-full pl-9 pr-3 py-2 text-sm bg-white border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400 focus:border-teal-400 transition-all"
            />
          </div>
          <select value={protocolFilter} onChange={(e) => setProtocolFilter(e.target.value)} className="text-sm bg-white border border-ink-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-teal-400 transition-all">
            <option value="all">{tr('sessions.allProtocols', lang)}</option>
            {DEMO_PROTOCOLS.map((p) => (
              <option key={p.id} value={p.id}>{lang === 'cn' ? p.nameCn : p.name}</option>
            ))}
          </select>
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="text-sm bg-white border border-ink-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-teal-400 transition-all">
            <option value="all">{tr('sessions.allStatuses', lang)}</option>
            {Object.entries(SESSION_STATUS_CONFIG).map(([key, cfg]) => (
              <option key={key} value={key}>{lang === 'cn' ? cfg.cn : cfg.en}</option>
            ))}
          </select>
          <select value={sourceFilter} onChange={(e) => setSourceFilter(e.target.value)} className="text-sm bg-white border border-ink-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-teal-400 transition-all">
            <option value="all">{tr('sessions.allSources', lang)}</option>
            {sourceOptions.map((s) => (
              <option key={s.id} value={s.id}>{lang === 'cn' ? s.nameCn : s.name}</option>
            ))}
          </select>
          <select value={dateFilter} onChange={(e) => setDateFilter(e.target.value)} className="text-sm bg-white border border-ink-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-teal-400 transition-all">
            <option value="all">{tr('common.allTime', lang)}</option>
            <option value="7">{tr('common.last7days', lang)}</option>
            <option value="30">{tr('common.last30days', lang)}</option>
            <option value="90">{tr('common.last90days', lang)}</option>
          </select>
          <div className="flex-1" />
          <button
            onClick={() => { addToast(lang === 'cn' ? '导入数据（演示）' : 'Import data (demo)', 'info'); navigate('data-sources'); }}
            className="flex items-center gap-2 px-3 py-2 bg-accent hover:bg-accent-hover text-white text-sm font-medium rounded-lg transition-colors"
          >
            <Upload className="w-4 h-4" />
            {tr('common.importData', lang)}
          </button>
        </div>

        <div className="bg-white rounded-xl border border-ink-200 overflow-hidden">
          <DataTable
            columns={[
              {
                key: 'subject',
                header: tr('sessions.subject', lang),
                render: (s) => {
                  const athlete = DEMO_ATHLETES.find((a) => a.id === s.athleteId);
                  const sport = getSport(s.sportId);
                  const SportIcon = sport ? SPORT_ICONS[sport.id] || Activity : Activity;
                  return (
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-lg bg-ink-100 flex items-center justify-center shrink-0">
                        <SportIcon className="w-4 h-4 text-ink-500" />
                      </div>
                      <div className="min-w-0">
                        <p className="text-sm font-medium text-ink-900 truncate">{athlete ? `${athlete.lastName} ${athlete.firstName}` : '—'}</p>
                        <p className="text-xs text-ink-500 truncate">{sport ? (lang === 'cn' ? sport.nameCn : sport.name) : ''}</p>
                      </div>
                    </div>
                  );
                },
              },
              {
                key: 'protocol',
                header: tr('sessions.protocol', lang),
                render: (s) => {
                  const protocol = getProtocol(s.protocolId);
                  return (
                    <div>
                      <p className="text-sm font-medium text-ink-900">{lang === 'cn' ? protocol?.nameCn || s.protocolName : s.protocolName}</p>
                      <p className="text-xs text-ink-400">v{protocol?.version || s.processingVersion}</p>
                    </div>
                  );
                },
              },
              {
                key: 'source',
                header: tr('sessions.source', lang),
                render: (s) => {
                  const ds = getDataSource(s.dataSourceId);
                  const app = getSourceApplication(s.sportId);
                  return (
                    <div>
                      <p className="text-sm text-ink-700">{ds ? (lang === 'cn' ? ds.nameCn : ds.name) : s.source}</p>
                      <p className={`text-xs font-medium ${app ? app.color : 'text-ink-400'}`}>{app ? (lang === 'cn' ? app.cn : app.en) : s.deviceSerial}</p>
                    </div>
                  );
                },
              },
              {
                key: 'status',
                header: tr('sessions.status', lang),
                render: (s) => getSessionStatusDisplay(s.sessionStatus),
              },
              {
                key: 'result',
                header: tr('sessions.result', lang),
                render: (s) => {
                  if (s.sessionStatus !== 'PUBLISHED' && s.sessionStatus !== 'ANALYZED') return <span className="text-sm text-ink-400">—</span>;
                  const protocol = getProtocol(s.protocolId);
                  const primaryMetric = protocol?.metrics.find((m) => m.primary);
                  if (!primaryMetric) return <span className="text-sm text-ink-400">—</span>;
                  const value = s.summary[primaryMetric.key];
                  if (value === undefined) return <span className="text-sm text-ink-400">—</span>;
                  return <span className="text-sm font-mono font-semibold text-ink-900">{value.toFixed(2)} <span className="text-xs text-ink-400 font-normal">{primaryMetric.unit}</span></span>;
                },
              },
              {
                key: 'scientific',
                header: tr('sessions.scientificStatus', lang),
                render: (s) => {
                  const info = getSessionScientificInfo(s.athleteId, s.protocolId);
                  if (!info) {
                    return <span className="text-xs text-ink-300">—</span>;
                  }
                  const cfg = SCI_STATUS_CONFIG[info.summaryStatus];
                  const isActive = info.isActive;
                  return (
                    <span
                      className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded text-xs font-medium ${cfg.bg} ${cfg.color} ${isActive ? 'ring-1 ring-teal-400' : ''}`}
                      title={`${info.publishedTrials}/${info.trialCount} trials published${info.failedTrials > 0 ? `, ${info.failedTrials} failed` : ''}${isActive ? ' (active scenario)' : ''}`}
                    >
                      {isActive && <span className="w-1.5 h-1.5 rounded-full bg-teal-500 animate-pulse" />}
                      {lang === 'cn' ? cfg.cn : cfg.en}
                    </span>
                  );
                },
              },
              {
                key: 'date',
                header: tr('sessions.date', lang),
                align: 'right',
                render: (s) => {
                  const date = new Date(s.date);
                  return (
                    <div className="text-right">
                      <p className="text-sm text-ink-700">{date.toLocaleDateString(lang === 'cn' ? 'zh-CN' : 'en-US', { month: 'short', day: 'numeric' })}</p>
                      <p className="text-xs text-ink-400">{date.toLocaleTimeString(lang === 'cn' ? 'zh-CN' : 'en-US', { hour: '2-digit', minute: '2-digit' })}</p>
                    </div>
                  );
                },
              },
              {
                key: 'action',
                header: '',
                align: 'right',
                render: (s) => (
                  <SessionRowActions session={s} lang={lang} navigate={navigate} addToast={addToast} />
                ),
              },
            ]}
            data={filteredSessions}
            rowKey={(s) => s.id}
            onRowClick={(s) => navigate('session-detail', { sessionId: s.id })}
            emptyMessage={tr('sessions.noMatch', lang)}
          />
        </div>
      </div>
    </div>
  );
}

function SessionRowActions({ session, lang, navigate, addToast }: {
  session: TestSession;
  lang: 'en' | 'cn';
  navigate: (view: string, params?: Record<string, string>) => void;
  addToast: (msg: string, type?: 'success' | 'warning' | 'attention' | 'info') => void;
}) {
  const [open, setOpen] = useState(false);
  const isFailed = session.sessionStatus.includes('FAILED');
  const isPublished = session.sessionStatus === 'PUBLISHED' || session.sessionStatus === 'ANALYZED';

  const actions = [
    { icon: ChevronRight, label: tr('common.viewSession', lang), onClick: () => navigate('session-detail', { sessionId: session.id }) },
    { icon: Download, label: tr('common.download', lang), onClick: () => addToast(lang === 'cn' ? '下载（演示）' : 'Download (demo)', 'info') },
    ...(isFailed ? [{ icon: RefreshCw, label: tr('common.retryProcessing', lang), onClick: () => addToast(lang === 'cn' ? '重试处理（演示）' : 'Retry processing (demo)', 'info') }] : []),
    { icon: Activity, label: tr('common.viewRawData', lang), onClick: () => navigate('raw-data') },
    ...(isPublished ? [{ icon: BarChart3, label: tr('common.viewAnalysis', lang), onClick: () => navigate('result-detail', { sessionId: session.id }) }] : []),
    ...(isPublished ? [{ icon: FileText, label: tr('common.generateReport', lang), onClick: () => navigate('reports') }] : []),
  ];

  return (
    <div className="relative" onClick={(e) => e.stopPropagation()}>
      <button
        onClick={() => setOpen(!open)}
        className="p-1.5 text-ink-400 hover:text-teal-600 hover:bg-teal-50 rounded-md transition-colors"
      >
        <MoreVertical className="w-4 h-4" />
      </button>
      {open && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setOpen(false)} />
          <div className="absolute right-0 top-full mt-1 z-20 bg-white rounded-lg border border-ink-200 shadow-lg py-1 min-w-[180px]">
            {actions.map((action, i) => {
              const Icon = action.icon;
              return (
                <button
                  key={i}
                  onClick={() => { action.onClick(); setOpen(false); }}
                  className="w-full flex items-center gap-2 px-3 py-2 text-sm text-ink-700 hover:bg-ink-50 transition-colors text-left"
                >
                  <Icon className="w-3.5 h-3.5 text-ink-400" />
                  {action.label}
                </button>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}