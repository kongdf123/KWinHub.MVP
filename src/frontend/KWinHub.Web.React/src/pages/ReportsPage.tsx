import { useEffect, useState } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { reportsApi, type ReportResponse, type GenerateReportRequest } from '@/lib/api/reports';
import { plansApi, type AssessmentPlanResponse } from '@/lib/api/plans';
import { subjectsApi, type SubjectDto } from '@/lib/api/subjects';
import { sessionsApi, type SessionDto } from '@/lib/api/sessions';
import { FileText, Plus, CheckCircle, Clock, Loader2, X, ChevronRight, AlertTriangle } from 'lucide-react';

const STATUS_MAP: Record<string, { label: string; color: string; dot: string }> = {
  Draft: { label: 'Draft', color: 'text-amber-700 bg-amber-50', dot: 'bg-amber-500' },
  Complete: { label: 'Complete', color: 'text-success-700 bg-success-50', dot: 'bg-success-500' },
  Archived: { label: 'Archived', color: 'text-ink-600 bg-ink-100', dot: 'bg-ink-400' },
};

export function ReportsPage() {
  const { lang, addToast, navigate, canAccess } = useApp();
  const [reports, setReports] = useState<ReportResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [showGenerate, setShowGenerate] = useState(false);

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['reports'] ?? []).filter((tab) => canAccess(tab.view));

  const loadReports = async () => {
    setLoading(true);
    try {
      setReports(await reportsApi.list());
    } catch {
      addToast(lang === 'cn' ? '加载报告失败' : 'Failed to load reports', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadReports();
  }, []);

  const handleReportGenerated = (reportId: string) => {
    setShowGenerate(false);
    void loadReports();
    navigate('report-detail', { reportId });
  };

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.reports', lang)}</h2>
          <button
            onClick={() => setShowGenerate(true)}
            className="flex items-center gap-1.5 px-3 py-2 bg-teal-600 hover:bg-teal-700 text-white text-sm font-medium rounded-lg transition-colors"
          >
            <Plus className="w-4 h-4" /> {lang === 'cn' ? '生成报告' : 'Generate Report'}
          </button>
        </div>

        {workspaceTabs.length > 1 && (
          <nav aria-label={tr('nav.reports', lang)} className="flex items-center gap-1 mt-4 border-b border-default">
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'reports';
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

      <div className="px-6 py-6">
        {loading ? (
          <div className="flex items-center justify-center py-20">
            <Loader2 className="w-6 h-6 animate-spin text-ink-400" />
          </div>
        ) : reports.length === 0 ? (
          <div className="text-center py-20">
            <FileText className="w-12 h-12 text-ink-300 mx-auto mb-4" />
            <p className="text-sm text-ink-500 mb-4">{lang === 'cn' ? '暂无报告，点击上方按钮生成。' : 'No reports yet. Generate one above.'}</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {reports.map((report) => {
              const cfg = STATUS_MAP[report.status] ?? STATUS_MAP.Draft;
              return (
                <button
                  key={report.id}
                  onClick={() => navigate('report-detail', { reportId: report.id })}
                  className="text-left bg-white rounded-xl border border-ink-200 p-5 hover:border-ink-300 hover:shadow-md transition-all"
                >
                  <div className="flex items-start justify-between mb-3">
                    <div className="w-11 h-11 rounded-xl bg-ink-50 flex items-center justify-center">
                      <FileText className="w-5 h-5 text-ink-600" />
                    </div>
                    <span className={`inline-flex items-center gap-1.5 text-xs px-2 py-0.5 rounded-full font-medium ${cfg.color}`}>
                      <span className={`w-1.5 h-1.5 rounded-full ${cfg.dot}`} />
                      {cfg.label}
                    </span>
                  </div>
                  <h3 className="text-base font-semibold text-ink-900 mb-1">{report.title}</h3>
                  <p className="text-xs text-ink-500 mb-3">
                    {report.planCode} v{report.planVersion} · {report.metricValueCount} {lang === 'cn' ? '指标' : 'metrics'} · {report.assetStateCount} {lang === 'cn' ? '视频' : 'videos'}
                  </p>
                  <div className="flex items-center gap-2 pt-3 border-t border-ink-100">
                    <span className="text-xs text-ink-400">
                      {new Date(report.generatedAt).toLocaleDateString(lang === 'cn' ? 'zh-CN' : 'en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
                    </span>
                    <ChevronRight className="w-4 h-4 text-ink-400 ml-auto" />
                  </div>
                </button>
              );
            })}
          </div>
        )}
      </div>

      {showGenerate && (
        <GenerateReportDialog
          onClose={() => setShowGenerate(false)}
          onGenerated={handleReportGenerated}
        />
      )}
    </div>
  );
}

function GenerateReportDialog({
  onClose,
  onGenerated,
}: {
  onClose: () => void;
  onGenerated: (reportId: string) => void;
}) {
  const { lang, addToast } = useApp();
  const [plans, setPlans] = useState<AssessmentPlanResponse[]>([]);
  const [subjects, setSubjects] = useState<SubjectDto[]>([]);
  const [sessions, setSessions] = useState<SessionDto[]>([]);
  const [selectedPlanId, setSelectedPlanId] = useState('');
  const [selectedSubjectId, setSelectedSubjectId] = useState('');
  const [selectedSessionIds, setSelectedSessionIds] = useState<string[]>([]);
  const [title, setTitle] = useState('');
  const [notes, setNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    void Promise.all([
      plansApi.list().catch(() => []),
      subjectsApi.list().catch(() => []),
    ]).then(([p, s]) => {
      setPlans(p.filter((pl) => pl.status === 'Published'));
      setSubjects(s);
    });
  }, []);

  useEffect(() => {
    if (selectedSubjectId) {
      sessionsApi.listBySubject(selectedSubjectId).catch(() => []).then(setSessions);
    } else {
      setSessions([]);
    }
    setSelectedSessionIds([]);
  }, [selectedSubjectId]);

  const toggleSession = (id: string) => {
    setSelectedSessionIds((prev) => prev.includes(id) ? prev.filter((s) => s !== id) : [...prev, id]);
  };

  const canGenerate = selectedPlanId && selectedSubjectId && selectedSessionIds.length > 0 && !submitting;

  const handleGenerate = async () => {
    if (!canGenerate) return;
    setSubmitting(true);
    try {
      const request: GenerateReportRequest = {
        planId: selectedPlanId,
        subjectId: selectedSubjectId,
        sessionIds: selectedSessionIds,
        title: title.trim() || null,
        notes: notes.trim() || null,
      };
      const report = await reportsApi.generate(request);
      addToast(lang === 'cn' ? '报告生成成功' : 'Report generated successfully', 'success');
      onGenerated(report.id);
    } catch (e) {
      addToast(e instanceof Error ? e.message : (lang === 'cn' ? '生成失败' : 'Generation failed'), 'error');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-hidden flex flex-col">
        <div className="flex items-center justify-between px-6 py-4 border-b border-ink-100">
          <h3 className="text-lg font-semibold text-ink-900">{lang === 'cn' ? '生成报告' : 'Generate Report'}</h3>
          <button onClick={onClose} className="text-ink-400 hover:text-ink-600"><X className="w-5 h-5" /></button>
        </div>

        <div className="px-6 py-5 space-y-5 overflow-y-auto">
          <div>
            <label className="block text-xs font-medium text-ink-500 uppercase tracking-wider mb-2">
              {lang === 'cn' ? '评估计划' : 'Assessment Plan'}
            </label>
            <select
              value={selectedPlanId}
              onChange={(e) => setSelectedPlanId(e.target.value)}
              className="w-full px-3 py-2 text-sm bg-ink-50 border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400"
            >
              <option value="">{lang === 'cn' ? '选择计划...' : 'Select plan...'}</option>
              {plans.map((p) => (
                <option key={p.id} value={p.id}>{p.name} ({p.code} v{p.version})</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-500 uppercase tracking-wider mb-2">
              {lang === 'cn' ? '受试者' : 'Subject'}
            </label>
            <select
              value={selectedSubjectId}
              onChange={(e) => setSelectedSubjectId(e.target.value)}
              className="w-full px-3 py-2 text-sm bg-ink-50 border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400"
            >
              <option value="">{lang === 'cn' ? '选择受试者...' : 'Select subject...'}</option>
              {subjects.map((s) => (
                <option key={s.id} value={s.id}>{s.firstName} {s.lastName}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-500 uppercase tracking-wider mb-2">
              {lang === 'cn' ? '会话' : 'Sessions'} ({selectedSessionIds.length} {lang === 'cn' ? '已选' : 'selected'})
            </label>
            {sessions.length === 0 ? (
              <p className="text-sm text-ink-400 py-2">{lang === 'cn' ? '无可用会话' : 'No sessions available'}</p>
            ) : (
              <div className="space-y-2 max-h-48 overflow-y-auto">
                {sessions.map((s) => (
                  <label key={s.id} className={`flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${selectedSessionIds.includes(s.id) ? 'border-teal-500 bg-teal-50' : 'border-ink-200 bg-ink-50 hover:bg-ink-100'}`}>
                    <input
                      type="checkbox"
                      checked={selectedSessionIds.includes(s.id)}
                      onChange={() => toggleSession(s.id)}
                      className="w-4 h-4 rounded text-teal-600 focus:ring-teal-400"
                    />
                    <div className="flex-1 min-w-0">
                      <div className="text-sm font-medium text-ink-800">{s.externalId || s.id.slice(0, 8)}</div>
                      <div className="text-xs text-ink-500">
                        {s.status} · {s.startedAt ? new Date(s.startedAt).toLocaleDateString() : lang === 'cn' ? '未开始' : 'not started'}
                      </div>
                    </div>
                  </label>
                ))}
              </div>
            )}
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-500 uppercase tracking-wider mb-2">
              {lang === 'cn' ? '标题（可选）' : 'Title (optional)'}
            </label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder={lang === 'cn' ? '自动生成标题' : 'Auto-generated title'}
              className="w-full px-3 py-2 text-sm bg-ink-50 border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-500 uppercase tracking-wider mb-2">
              {lang === 'cn' ? '备注（可选）' : 'Notes (optional)'}
            </label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
              className="w-full px-3 py-2 text-sm bg-ink-50 border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400"
            />
          </div>

          <div className="flex items-start gap-2 p-3 rounded-lg bg-amber-50 text-amber-700">
            <AlertTriangle className="w-4 h-4 shrink-0 mt-0.5" />
            <p className="text-xs">{lang === 'cn' ? '报告生成后将成为不可变的快照，源数据的后续修改不会影响已生成的报告。' : 'Once generated, the report is an immutable snapshot. Subsequent changes to source data will not affect it.'}</p>
          </div>
        </div>

        <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-ink-100">
          <button onClick={onClose} className="px-4 py-2 text-sm font-medium text-ink-700 bg-white border border-ink-200 hover:bg-ink-50 rounded-lg transition-colors">
            {lang === 'cn' ? '取消' : 'Cancel'}
          </button>
          <button
            onClick={handleGenerate}
            disabled={!canGenerate}
            className="flex items-center gap-2 px-4 py-2 bg-teal-600 hover:bg-teal-700 disabled:opacity-40 text-white text-sm font-medium rounded-lg transition-colors"
          >
            {submitting && <Loader2 className="w-4 h-4 animate-spin" />}
            {lang === 'cn' ? '生成' : 'Generate'}
          </button>
        </div>
      </div>
    </div>
  );
}