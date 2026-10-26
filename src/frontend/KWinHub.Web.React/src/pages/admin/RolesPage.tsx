import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { ShieldCheck, Check, X } from 'lucide-react';

const PERMISSIONS = [
  { key: 'view', labelEn: 'View', labelCn: '查看' },
  { key: 'create', labelEn: 'Create', labelCn: '创建' },
  { key: 'edit', labelEn: 'Edit', labelCn: '编辑' },
  { key: 'delete', labelEn: 'Delete', labelCn: '删除' },
  { key: 'export', labelEn: 'Export', labelCn: '导出' },
  { key: 'admin', labelEn: 'Admin', labelCn: '管理' },
];

const ROLES = [
  { key: 'admin', labelEn: 'Organization Admin', labelCn: '组织管理员', permissions: ['view', 'create', 'edit', 'delete', 'export', 'admin'] },
  { key: 'coach', labelEn: 'Coach', labelCn: '教练', permissions: ['view', 'create', 'edit', 'export'] },
  { key: 'analyst', labelEn: 'Analyst', labelCn: '分析师', permissions: ['view', 'export'] },
  { key: 'viewer', labelEn: 'Viewer', labelCn: '观察者', permissions: ['view'] },
];

const RESOURCES = ['subjects', 'sessions', 'results', 'protocols', 'datasets', 'reports', 'integrations', 'settings'];

export function RolesPage() {
  const { lang, navigate, canAccess } = useApp();

  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['roles'] ?? []).filter((tab) => canAccess(tab.view));

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.settings', lang)}</h2>
          <div className="text-sm text-content-secondary">{/* page count or subtitle here */}</div>
        </div>

        {workspaceTabs.length > 1 && (
          <nav aria-label={tr('nav.settings', lang)} className="flex items-center gap-1 mt-4 border-b border-default">
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'roles';
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

      <div className="px-6 py-6 space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {ROLES.map((role) => (
            <div key={role.key} className="bg-white rounded-xl border border-ink-200 p-5">
              <div className="flex items-center gap-2 mb-3">
                <ShieldCheck className="w-5 h-5 text-teal-600" />
                <h4 className="text-sm font-semibold text-ink-900">{lang === 'cn' ? role.labelCn : role.labelEn}</h4>
              </div>
              <p className="text-xs text-ink-500 mb-3">{role.permissions.length} {lang === 'cn' ? '项权限' : 'permissions'}</p>
              <div className="flex flex-wrap gap-1.5">
                {PERMISSIONS.map((p) => (
                  <span
                    key={p.key}
                    className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                      role.permissions.includes(p.key) ? 'bg-teal-50 text-teal-700' : 'bg-ink-100 text-ink-400'
                    }`}
                  >
                    {lang === 'cn' ? p.labelCn : p.labelEn}
                  </span>
                ))}
              </div>
            </div>
          ))}
        </div>

        <div className="bg-white rounded-xl border border-ink-200 overflow-hidden">
          <h4 className="text-sm font-semibold text-ink-900 px-5 py-4 border-b border-ink-100">
            {lang === 'cn' ? '权限矩阵' : 'Permission Matrix'}
          </h4>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-ink-100">
                  <th className="text-left text-xs font-semibold text-ink-500 uppercase tracking-wider px-5 py-3">
                    {lang === 'cn' ? '资源' : 'Resource'}
                  </th>
                  {ROLES.map((role) => (
                    <th key={role.key} className="text-center text-xs font-semibold text-ink-500 uppercase tracking-wider px-4 py-3">
                      {lang === 'cn' ? role.labelCn : role.labelEn}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-50">
                {RESOURCES.map((resource) => (
                  <tr key={resource} className="hover:bg-ink-50">
                    <td className="px-5 py-3 text-sm font-medium text-ink-900 capitalize">{resource}</td>
                    {ROLES.map((role) => {
                      const hasPermission = role.permissions.includes('admin') || role.permissions.includes('view') || role.permissions.includes('edit');
                      return (
                        <td key={role.key} className="text-center px-4 py-3">
                          {hasPermission ? (
                            <Check className="w-4 h-4 text-success-500 mx-auto" />
                          ) : (
                            <X className="w-4 h-4 text-ink-300 mx-auto" />
                          )}
                        </td>
                      );
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
