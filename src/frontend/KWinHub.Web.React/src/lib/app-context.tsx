import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import type { Toast } from '@/components/ui/Toast';
import type { Lang } from '@/lib/types';
import { subscriptionApi } from '@/features/subscriptions/api';

export type UserRole = 'coach' | 'org-admin' | 'athlete' | 'support';

/**
 * Stable catalog of feature keys (mirrors KWinHub.Domain.Subscriptions.SubscriptionCatalog).
 * Used by VIEW_FEATURES to gate UI views against server-side feature entitlements.
 */
export const FEATURE_KEYS = {
  testingSprint: 'testing.sprint',
  testingForceplate: 'testing.forceplate',
  analyticsAdvanced: 'analytics.advanced',
  integrationsApi: 'integrations.api',
  reportsExport: 'reports.export',
  subscriptionView: 'subscription.view',
  subscriptionManage: 'subscription.manage',
} as const;

/**
 * Maps a UI view name to the feature key that gates it. Views not present here
 * are gated only by role permissions, not by feature entitlements.
 */
export const VIEW_FEATURES: Record<string, string> = {
  subscription: FEATURE_KEYS.subscriptionView,
  integrations: FEATURE_KEYS.integrationsApi,
  reports: FEATURE_KEYS.reportsExport,
  biomechanics: FEATURE_KEYS.analyticsAdvanced,
};

export const ROLE_PERMISSIONS: Record<UserRole, string[]> = {
  coach: [
    'home', 'athletes', 'athlete-profile', 'teams',
    'sessions', 'session-detail', 'protocols', 'protocol-detail', 'devices', 'sync-center',
    'results', 'result-detail', 'performance', 'trends', 'comparisons', 'biomechanics',
    'dashboards', 'reports',
    'organization',
    'workflows',
  ],
  'org-admin': [
    'home', 'athletes', 'athlete-profile', 'teams',
    'sessions', 'session-detail', 'protocols', 'protocol-detail', 'devices', 'sync-center',
    'results', 'result-detail', 'performance', 'trends', 'comparisons', 'biomechanics',
    'dashboards', 'reports',
    'datasets', 'raw-data', 'data-sources', 'data-source-detail', 'integrations',
    'organization', 'users', 'roles', 'settings',
    'subscription',
    'workflows',
  ],
  athlete: [
    'home', 'athletes', 'athlete-profile',
    'sessions', 'session-detail',
    'results', 'result-detail', 'performance', 'trends',
    'workflows',
  ],
  support: [
    'home', 'athletes', 'athlete-profile', 'teams',
    'sessions', 'session-detail', 'protocols', 'protocol-detail',
    'results', 'result-detail', 'performance', 'trends', 'comparisons', 'biomechanics',
    'dashboards', 'reports',
    'datasets', 'raw-data', 'data-sources', 'data-source-detail', 'integrations', 'sync-center',
    'workflows',
  ],
};

interface AppContextValue {
  currentView: string;
  navigate: (view: string, params?: Record<string, string>) => void;
  viewParams: Record<string, string>;
  toasts: Toast[];
  addToast: (message: string, type?: Toast['type']) => void;
  dismissToast: (id: string) => void;
  selectedTeamId: string;
  setSelectedTeamId: (id: string) => void;
  lang: Lang;
  setLang: (lang: Lang) => void;
  t: (key: string) => string;
  role: UserRole;
  setRole: (role: UserRole) => void;
  canAccess: (view: string) => boolean;
  /** True when the view passes both role permission and (if applicable) the
   *  server-side feature entitlement. */
  canOpenView: (view: string) => boolean;
  serverFeatures: Record<string, boolean>;
  loadAccess: () => Promise<void>;
  logout: () => void;
}

const AppContext = createContext<AppContextValue | null>(null);

export function AppProvider({ children, onLogout }: { children: ReactNode; onLogout?: () => void }) {
  const [currentView, setCurrentView] = useState('home');
  const [viewParams, setViewParams] = useState<Record<string, string>>({});
  const [toasts, setToasts] = useState<Toast[]>([]);
  const [selectedTeamId, setSelectedTeamId] = useState('team-1');
  const [lang, setLang] = useState<Lang>('cn');
  const [role, setRole] = useState<UserRole>('org-admin');
  const [serverFeatures, setServerFeatures] = useState<Record<string, boolean>>({});

  const navigate = useCallback((view: string, params: Record<string, string> = {}) => {
    setCurrentView(view);
    setViewParams(params);
  }, []);

  const addToast = useCallback((message: string, type: Toast['type'] = 'success') => {
    const id = `toast-${Date.now()}-${Math.random().toString(36).slice(2)}`;
    setToasts((prev) => [...prev, { id, message, type }]);
  }, []);

  const dismissToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const tFn = useCallback((key: string) => {
    return key;
  }, []);

  const canAccess = useCallback((view: string) => {
    if (!ROLE_PERMISSIONS[role].includes(view)) return false;
    const featureKey = VIEW_FEATURES[view];
    if (!featureKey) return true; // not feature-gated
    // Fail-open until the access response arrives so the UI stays usable.
    if (!(featureKey in serverFeatures)) return true;
    return serverFeatures[featureKey] === true;
  }, [role, serverFeatures]);

  // Explicit alias for the combined role + feature check.
  const canOpenView = useCallback((view: string) => canAccess(view), [canAccess]);

  const loadAccess = useCallback(async () => {
    try {
      const access = await subscriptionApi.getAccess();
      setServerFeatures(access.features ?? {});
    } catch {
      // Network/auth errors leave serverFeatures empty; canOpenView fails open.
    }
  }, []);

  const logout = useCallback(() => {
    if (onLogout) onLogout();
  }, [onLogout]);

  return (
    <AppContext.Provider value={{ currentView, navigate, viewParams, toasts, addToast, dismissToast, selectedTeamId, setSelectedTeamId, lang, setLang, t: tFn, role, setRole, canAccess, canOpenView, serverFeatures, loadAccess, logout }}>
      {children}
    </AppContext.Provider>
  );
}

export function useApp() {
  const ctx = useContext(AppContext);
  if (!ctx) throw new Error('useApp must be used within AppProvider');
  return ctx;
}