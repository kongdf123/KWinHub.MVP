import { useEffect, useState } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { plansApi, type AssessmentPlanResponse, type AssessmentPlanItemResponse } from '@/lib/api/plans';
import { FileText, Plus, CheckCircle, XCircle, Copy, AlertTriangle, Loader2, RotateCcw } from 'lucide-react';

type StatusColor = { label: string; color: string; dot: string };

const STATUS_MAP: Record<string, StatusColor> = {
  Draft: { label: 'Draft', color: 'text-amber-700 bg-amber-50', dot: 'bg-amber-500' },
  Published: { label: 'Published', color: 'text-success-700 bg-success-50', dot: 'bg-success-500' },
  Retired: { label: 'Retired', color: 'text-ink-600 bg-ink-100', dot: 'bg-ink-400' },
};

export function PlansPage() {
  const { lang, addToast, navigate, canAccess } = useApp();
  const [plans, setPlans] = useState<AssessmentPlanResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['plans'] ?? []).filter((tab) => canAccess(tab.view));

  const loadPlans = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await plansApi.list();
      setPlans(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load plans');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadPlans();
  }, []);

  const handleAction = async (action: () => Promise<unknown>, successMsg: string) => {
    try {
      await action();
      addToast(successMsg, 'success');
      void loadPlans();
    } catch (e) {
      addToast(e instanceof Error ? e.message : 'Action failed', 'error');
    }
  };

  const handlePublish = (id: string) =>
    handleAction(() => plansApi.publish(id), lang === 'cn' ? '计划已发布' : 'Plan published');

  const handleRetire = (id: string) =>
    handleAction(() => plansApi.retire(id), lang === 'cn' ? '计划已停用' : 'Plan retired');

  const handleNewVersion = (id: string) =>
    handleAction(() => plansApi.createNewVersion(id), lang === 'cn' ? '已创建新版本' : 'New version created');

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.plans', lang)}</h2>
          <button
            onClick={() => addToast(lang === 'cn' ? '创建计划（M5-3 实现）' : 'Create plan (M5-3)', 'info')}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-teal-600 hover:bg-teal-700 text-white text-sm font-medium rounded-lg transition-colors"
          >
            <Plus className="w-4 h-4" /> {lang === 'cn' ? '新建计划' : 'New Plan'}
          </button>
        </div>

        {workspaceTabs.length > 1 && (
          <nav aria-label={tr('nav.plans', lang)} className="flex items-center gap-1 mt-4 border-b border-default">
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'plans';
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
        {loading && (
          <div className="flex items-center gap-2 text-sm text-ink-500 py-8">
            <Loader2 className="w-4 h-4 animate-spin" />
            {lang === 'cn' ? '加载中...' : 'Loading...'}
          </div>
        )}

        {error && (
          <div className="flex items-center gap-2 p-4 bg-red-50 text-red-700 rounded-lg text-sm">
            <AlertTriangle className="w-4 h-4" />
            {error}
          </div>
        )}

        {!loading && !error && plans.length === 0 && (
          <div className="bg-white rounded-xl border border-dashed border-ink-300 p-12 text-center">
            <FileText className="w-12 h-12 text-ink-300 mx-auto mb-3" />
            <h3 className="text-sm font-semibold text-ink-900 mb-1">
              {lang === 'cn' ? '暂无评估计划' : 'No assessment plans'}
            </h3>
            <p className="text-xs text-ink-500">
              {lang === 'cn' ? '创建计划来定义评估所需的指标集合。' : 'Create a plan to define the metrics required for an assessment.'}
            </p>
          </div>
        )}

        {!loading && !error && plans.length > 0 && (
          <div className="space-y-3">
            {plans.map((plan) => {
              const status = STATUS_MAP[plan.status] ?? STATUS_MAP.Draft;
              const expanded = expandedId === plan.id;
              const requiredCount = plan.items.filter((i) => i.required).length;
              return (
                <div key={plan.id} className="bg-white rounded-xl border border-ink-200 overflow-hidden">
                  <div className="p-4 flex items-center gap-4">
                    <div className="w-10 h-10 rounded-lg bg-ink-50 flex items-center justify-center shrink-0">
                      <FileText className="w-5 h-5 text-ink-600" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <h3 className="text-sm font-semibold text-ink-900 truncate">{plan.name}</h3>
                        <span className={`inline-flex items-center gap-1.5 text-xs px-2 py-0.5 rounded-full font-medium ${status.color}`}>
                          <span className={`w-1.5 h-1.5 rounded-full ${status.dot}`} />
                          {status.label}
                        </span>
                        <span className="text-xs text-ink-400">v{plan.version}</span>
                      </div>
                      <p className="text-xs text-ink-500 truncate mt-0.5">
                        {plan.code} · {plan.items.length} {lang === 'cn' ? '项指标' : 'metrics'} ({requiredCount} {lang === 'cn' ? '必需' : 'required'})
                      </p>
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                      {plan.status === 'Draft' && (
                        <button
                          onClick={() => handlePublish(plan.id)}
                          className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-success-700 bg-success-50 hover:bg-success-100 rounded-lg transition-colors"
                        >
                          <CheckCircle className="w-3.5 h-3.5" /> {lang === 'cn' ? '发布' : 'Publish'}
                        </button>
                      )}
                      {plan.status === 'Published' && (
                        <>
                          <button
                            onClick={() => handleNewVersion(plan.id)}
                            className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-teal-700 bg-teal-50 hover:bg-teal-100 rounded-lg transition-colors"
                          >
                            <Copy className="w-3.5 h-3.5" /> {lang === 'cn' ? '新版本' : 'New Version'}
                          </button>
                          <button
                            onClick={() => handleRetire(plan.id)}
                            className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-ink-600 bg-ink-100 hover:bg-ink-200 rounded-lg transition-colors"
                          >
                            <XCircle className="w-3.5 h-3.5" /> {lang === 'cn' ? '停用' : 'Retire'}
                          </button>
                        </>
                      )}
                      <button
                        onClick={() => setExpandedId(expanded ? null : plan.id)}
                        className="text-xs text-ink-500 hover:text-ink-700 px-2 py-1"
                      >
                        {expanded ? (lang === 'cn' ? '收起' : 'Collapse') : (lang === 'cn' ? '展开' : 'Expand')}
                      </button>
                    </div>
                  </div>

                  {expanded && (
                    <div className="border-t border-ink-100 bg-ink-50/50 p-4">
                      {plan.items.length === 0 ? (
                        <p className="text-xs text-ink-500">{lang === 'cn' ? '无指标项' : 'No metric items'}</p>
                      ) : (
                        <div className="overflow-x-auto">
                          <table className="w-full text-xs">
                            <thead>
                              <tr className="text-left text-ink-500 uppercase tracking-wider">
                                <th className="py-2 px-3 font-medium">{lang === 'cn' ? '指标代码' : 'Code'}</th>
                                <th className="py-2 px-3 font-medium">{lang === 'cn' ? '名称' : 'Name'}</th>
                                <th className="py-2 px-3 font-medium">{lang === 'cn' ? '单位' : 'Unit'}</th>
                                <th className="py-2 px-3 font-medium">{lang === 'cn' ? '类型' : 'Type'}</th>
                                <th className="py-2 px-3 font-medium">{lang === 'cn' ? '必需' : 'Required'}</th>
                                <th className="py-2 px-3 font-medium">{lang === 'cn' ? '范围' : 'Range'}</th>
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-ink-100">
                              {plan.items.map((item: AssessmentPlanItemResponse) => (
                                <tr key={item.id} className="text-ink-700">
                                  <td className="py-2 px-3 font-mono">{item.metricCode}</td>
                                  <td className="py-2 px-3">{item.metricName}</td>
                                  <td className="py-2 px-3">{item.unit}</td>
                                  <td className="py-2 px-3">{item.valueType}</td>
                                  <td className="py-2 px-3">
                                    {item.required ? (
                                      <span className="text-success-600">{lang === 'cn' ? '是' : 'Yes'}</span>
                                    ) : (
                                      <span className="text-ink-400">{lang === 'cn' ? '否' : 'No'}</span>
                                    )}
                                  </td>
                                  <td className="py-2 px-3 text-ink-500">
                                    {item.minValue !== null || item.maxValue !== null
                                      ? `${item.minValue ?? '−∞'} – ${item.maxValue ?? '+∞'}`
                                      : '—'}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}