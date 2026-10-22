import { useEffect, useState, type ReactNode } from 'react';
import { api, clearToken, isLoggedIn, type AuthProfileResponse, type LoginResponse } from './lib/api';
import { LocaleProvider, useI18n, type TranslationKey } from './lib/i18n';
import { matchRoute, navigate, useHashRoute } from './lib/router';
import { Layout } from './components/Layout';
import { Card, EmptyState, Spinner } from './components/ui';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { OrganizationsPage } from './pages/OrganizationsPage';
import { OrganizationDetailPage } from './pages/OrganizationDetailPage';
import { UsersPage } from './pages/UsersPage';
import { ProtocolCatalogPage } from './pages/ProtocolCatalogPage';
import { MetricCatalogPage } from './pages/MetricCatalogPage';
import { DeviceCatalogPage } from './pages/DeviceCatalogPage';
import { AlgorithmVersionsPage } from './pages/AlgorithmVersionsPage';
import { AuditLogPage } from './pages/AuditLogPage';

type AuthState = 'checking' | 'authenticated' | 'anonymous';

type Session = {
  email: string;
};

function resolveRoute(path: string, t: (key: TranslationKey) => string): { titleKey: TranslationKey; page: ReactNode } {
  if (path === '/') return { titleKey: 'dashboard.title', page: <DashboardPage /> };
  if (path === '/organizations') return { titleKey: 'orgs.title', page: <OrganizationsPage /> };

  const organizationMatch = matchRoute('/organizations/:id', path);
  if (organizationMatch) {
    return {
      titleKey: 'orgDetail.title',
      page: <OrganizationDetailPage id={organizationMatch.id} />,
    };
  }

  if (path === '/users') return { titleKey: 'users.title', page: <UsersPage /> };
  if (path === '/catalogs/protocols') {
    return { titleKey: 'protocols.title', page: <ProtocolCatalogPage /> };
  }
  if (path === '/catalogs/metrics') return { titleKey: 'metrics.title', page: <MetricCatalogPage /> };
  if (path === '/catalogs/devices') return { titleKey: 'devices.title', page: <DeviceCatalogPage /> };
  if (path === '/catalogs/algorithms') {
    return { titleKey: 'algorithms.title', page: <AlgorithmVersionsPage /> };
  }
  if (path === '/audit-logs') return { titleKey: 'audit.title', page: <AuditLogPage /> };

  return {
    titleKey: 'common.notFound',
    page: (
      <Card>
        <EmptyState title={t('common.notFound')} />
      </Card>
    ),
  };
}

function Console() {
  const { t } = useI18n();
  const route = useHashRoute();

  const [authState, setAuthState] = useState<AuthState>(isLoggedIn() ? 'checking' : 'anonymous');
  const [session, setSession] = useState<Session | null>(null);
  const [notAdmin, setNotAdmin] = useState(false);

  useEffect(() => {
    if (!isLoggedIn()) return;

    let cancelled = false;

    api
      .get<AuthProfileResponse>('/auth/me')
      .then((profile) => {
        if (cancelled) return;

        if (!profile.isPlatformAdmin) {
          clearToken();
          setNotAdmin(true);
          setAuthState('anonymous');
          return;
        }

        setSession({ email: profile.email });
        setAuthState('authenticated');
      })
      .catch(() => {
        if (cancelled) return;
        clearToken();
        setAuthState('anonymous');
      });

    return () => {
      cancelled = true;
    };
  }, []);

  function handleLogin(result: LoginResponse) {
    setSession({ email: result.email });
    setNotAdmin(false);
    setAuthState('authenticated');
    navigate('/');
  }

  function handleSignOut() {
    clearToken();
    setSession(null);
    setAuthState('anonymous');
    navigate('/');
  }

  if (authState === 'checking') {
    return (
      <div className="flex min-h-screen items-center justify-center bg-ink-50">
        <Spinner />
      </div>
    );
  }

  if (authState === 'anonymous') {
    return (
      <LoginPage onLogin={handleLogin} notice={notAdmin ? t('login.notAdmin') : null} />
    );
  }

  const { titleKey, page } = resolveRoute(route.path, t);

  return (
    <Layout title={t(titleKey)} email={session?.email ?? ''} onSignOut={handleSignOut}>
      {page}
    </Layout>
  );
}

export default function App() {
  return (
    <LocaleProvider>
      <Console />
    </LocaleProvider>
  );
}
