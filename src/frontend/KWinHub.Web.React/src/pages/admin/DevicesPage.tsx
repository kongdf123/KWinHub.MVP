import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { DataTable } from '@/components/ui/DataTable';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { WORKSPACE_TABS_BY_VIEW } from '@/lib/navigation';
import { DEMO_DEVICES } from '@/lib/demo-data';
import type { Device } from '@/lib/types';

export function DevicesPage() {
  const { lang, navigate, canAccess } = useApp();
  const workspaceTabs = (WORKSPACE_TABS_BY_VIEW['devices'] ?? []).filter((tab) => canAccess(tab.view));

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold text-content-primary">{tr('nav.workspace.testing', lang)}</h2>
          <div className="text-sm text-content-secondary">{DEMO_DEVICES.length}</div>
        </div>

        {workspaceTabs.length > 1 && (
          <nav aria-label={tr('nav.workspace.testing', lang)} className="flex items-center gap-1 mt-4 border-b border-default">
            {workspaceTabs.map((tab) => {
              const active = tab.view === 'devices';
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
        <div className="bg-surface-panel rounded-xl border border-default overflow-hidden">
          <DataTable
            columns={[
              {
                key: 'serial',
                header: lang === 'cn' ? '设备' : 'Device',
                render: (d: Device) => (
                  <div>
                    <p className="text-sm font-medium text-ink-900">{d.serialNumber}</p>
                    <p className="text-xs text-ink-500">{d.type}</p>
                  </div>
                ),
              },
              {
                key: 'location',
                header: lang === 'cn' ? '位置' : 'Location',
                render: (d: Device) => <span className="text-sm text-ink-700">{d.location}</span>,
              },
              {
                key: 'firmware',
                header: lang === 'cn' ? '固件' : 'Firmware',
                render: (d: Device) => <span className="text-sm font-mono text-ink-600">{d.firmware}</span>,
              },
              {
                key: 'battery',
                header: lang === 'cn' ? '电量' : 'Battery',
                align: 'right',
                render: (d: Device) => (
                  <span className={`text-sm font-mono ${d.battery > 50 ? 'text-success-600' : d.battery > 20 ? 'text-warning-600' : 'text-invalid-600'}`}>
                    {d.battery > 0 ? `${d.battery}%` : '—'}
                  </span>
                ),
              },
              {
                key: 'lastSync',
                header: lang === 'cn' ? '上次同步' : 'Last Sync',
                align: 'right',
                render: (d: Device) => <span className="text-sm text-ink-600">{new Date(d.lastSync).toLocaleDateString(lang === 'cn' ? 'zh-CN' : 'en-US', { month: 'short', day: 'numeric' })}</span>,
              },
              {
                key: 'status',
                header: tr('dataSources.status', lang),
                render: (d: Device) => <StatusBadge status={d.status} lang={lang} />,
              },
            ]}
            data={DEMO_DEVICES}
            rowKey={(d) => d.id}
          />
        </div>
      </div>
    </div>
  );
}
