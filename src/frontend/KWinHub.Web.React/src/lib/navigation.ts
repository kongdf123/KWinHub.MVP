import {
  LayoutDashboard, Users, ClipboardList, BarChart3, FileText, Plug, Settings,
  type LucideIcon,
} from 'lucide-react';

/**
 * Primary navigation model for the Hub.
 *
 * The sidebar exposes a flat list of primary destinations. Each destination owns
 * an ordered set of sub-views (`tabs`) that are rendered as a contextual tab group
 * in the main layout (see TopBar). `matchViews` includes detail views so the
 * sidebar destination stays highlighted on pages such as session-detail.
 */
export interface WorkspaceTab {
  view: string;
  labelKey: string;
}

export interface PrimaryNavItem {
  id: string;
  labelKey: string;
  icon: LucideIcon;
  tabs: WorkspaceTab[];
  matchViews: string[];
}

export const PRIMARY_NAV: PrimaryNavItem[] = [
  {
    id: 'overview',
    labelKey: 'nav.home',
    icon: LayoutDashboard,
    tabs: [{ view: 'home', labelKey: 'nav.home' }],
    matchViews: ['home'],
  },
  {
    id: 'people',
    labelKey: 'nav.people',
    icon: Users,
    tabs: [
      { view: 'athletes', labelKey: 'nav.athletes' },
      { view: 'teams', labelKey: 'nav.teams' },
    ],
    matchViews: ['athletes', 'athlete-profile', 'teams'],
  },
  {
    id: 'testing',
    labelKey: 'nav.workspace.testing',
    icon: ClipboardList,
    tabs: [
      { view: 'sessions', labelKey: 'nav.sessions' },
      { view: 'protocols', labelKey: 'nav.protocols' },
      { view: 'devices', labelKey: 'nav.devices' },
      { view: 'sync-center', labelKey: 'nav.syncCenter' },
    ],
    matchViews: ['sessions', 'session-detail', 'protocols', 'protocol-detail', 'devices', 'sync-center'],
  },
  {
    id: 'analysis',
    labelKey: 'nav.workspace.analysis',
    icon: BarChart3,
    tabs: [
      { view: 'results', labelKey: 'nav.results' },
      { view: 'performance', labelKey: 'nav.performance' },
      { view: 'trends', labelKey: 'nav.trends' },
      { view: 'comparisons', labelKey: 'nav.comparisons' },
      { view: 'biomechanics', labelKey: 'nav.biomechanics' },
      { view: 'dashboards', labelKey: 'nav.dashboards' },
    ],
    matchViews: ['results', 'result-detail', 'performance', 'trends', 'comparisons', 'biomechanics', 'dashboards'],
  },
  {
    id: 'reports',
    labelKey: 'nav.reports',
    icon: FileText,
    tabs: [{ view: 'reports', labelKey: 'nav.reports' }],
    matchViews: ['reports'],
  },
  {
    id: 'data',
    labelKey: 'nav.workspace.data',
    icon: Plug,
    tabs: [
      { view: 'data-sources', labelKey: 'nav.dataSources' },
      { view: 'integrations', labelKey: 'nav.integrations' },
      { view: 'datasets', labelKey: 'nav.datasets' },
    ],
    matchViews: ['data-sources', 'data-source-detail', 'integrations', 'datasets', 'dataset-detail'],
  },
  {
    id: 'settings',
    labelKey: 'nav.settings',
    icon: Settings,
    tabs: [
      { view: 'organization', labelKey: 'nav.organization' },
      { view: 'users', labelKey: 'nav.users' },
      { view: 'roles', labelKey: 'nav.roles' },
      { view: 'settings', labelKey: 'nav.preferences' },
    ],
    matchViews: ['settings', 'organization', 'users', 'roles'],
  },
];

/**
 * Maps a top-level list view to the workspace tab group it belongs to.
 * Detail views (session-detail, athlete-profile, …) are intentionally excluded:
 * they keep their own contextual tabs instead of the workspace tab group.
 */
export const WORKSPACE_TABS_BY_VIEW: Record<string, WorkspaceTab[]> = (() => {
  const map: Record<string, WorkspaceTab[]> = {};
  for (const item of PRIMARY_NAV) {
    if (item.tabs.length < 2) continue;
    for (const tab of item.tabs) {
      map[tab.view] = item.tabs;
    }
  }
  return map;
})();
