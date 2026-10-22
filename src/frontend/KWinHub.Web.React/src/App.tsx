import { useState } from 'react';
import { AppProvider, useApp } from '@/lib/app-context';
import { ToastContainer } from '@/components/ui/Toast';
import { Sidebar } from '@/components/layout/Sidebar';
import { LoginPage } from '@/pages/LoginPage';
import { RegisterPage } from '@/pages/RegisterPage';
import { VerifyEmailPage } from '@/pages/VerifyEmailPage';
import { HomePage } from '@/pages/HomePage';
import { AthletesPage } from '@/pages/AthletesPage';
import { AthleteProfilePage } from '@/pages/AthleteProfilePage';
import { SessionsPage } from '@/pages/SessionsPage';
import { SessionDetailPage } from '@/pages/SessionDetailPage';
import { ProtocolsPage } from '@/pages/ProtocolsPage';
import { ProtocolDetailPage } from '@/pages/ProtocolDetailPage';
import { ResultsPage } from '@/pages/ResultsPage';
import { ResultDetailPage } from '@/pages/ResultDetailPage';
import { PerformancePage } from '@/pages/PerformancePage';
import { TrendsPage } from '@/pages/TrendsPage';
import { ComparisonsPage } from '@/pages/ComparisonsPage';
import { BiomechanicsPage } from '@/pages/BiomechanicsPage';
import { DatasetsPage } from '@/pages/DatasetsPage';
import { RawDataPage } from '@/pages/RawDataPage';
import { DataSourcesPage } from '@/pages/DataSourcesPage';
import { DashboardsPage } from '@/pages/DashboardsPage';
import { IntegrationsPage } from '@/pages/IntegrationsPage';
import { SyncCenterPage } from '@/pages/SyncCenterPage';
import { ReportsPage } from '@/pages/ReportsPage';
import { OrganizationPage } from '@/pages/admin/OrganizationPage';
import { UsersPage } from '@/pages/admin/UsersPage';
import { RolesPage } from '@/pages/admin/RolesPage';
import { TeamsPage } from '@/pages/admin/TeamsPage';
import { DevicesPage } from '@/pages/admin/DevicesPage';
import { SettingsPage } from '@/pages/admin/SettingsPage';
import { isLoggedIn, clearToken } from '@/lib/api/http';
import { LocaleProvider } from '@/lib/i18n-context';

type AuthView = 'login' | 'register' | 'verify';

function AppContent({ onLogout }: { onLogout: () => void }) {
  const { currentView, toasts, dismissToast } = useApp();

  const renderView = () => {
    switch (currentView) {
      case 'home': return <HomePage />;
      case 'athletes': return <AthletesPage />;
      case 'athlete-profile': return <AthleteProfilePage />;
      case 'sessions': return <SessionsPage />;
      case 'session-detail': return <SessionDetailPage />;
      case 'protocols': return <ProtocolsPage />;
      case 'protocol-detail': return <ProtocolDetailPage />;
      case 'results': return <ResultsPage />;
      case 'result-detail': return <ResultDetailPage />;
      case 'performance': return <PerformancePage />;
      case 'trends': return <TrendsPage />;
      case 'comparisons': return <ComparisonsPage />;
      case 'biomechanics': return <BiomechanicsPage />;
      case 'datasets': return <DatasetsPage />;
      case 'raw-data': return <RawDataPage />;
      case 'data-sources': return <DataSourcesPage />;
      case 'dashboards': return <DashboardsPage />;
      case 'integrations': return <IntegrationsPage />;
      case 'sync-center': return <SyncCenterPage />;
      case 'reports': return <ReportsPage />;
      case 'organization': return <OrganizationPage />;
      case 'users': return <UsersPage />;
      case 'roles': return <RolesPage />;
      case 'teams': return <TeamsPage />;
      case 'devices': return <DevicesPage />;
      case 'settings': return <SettingsPage />;
      default: return <HomePage />;
    }
  };

  return (
    <div className="flex min-h-screen bg-ink-50">
      <Sidebar onLogout={onLogout} />
      <main className="flex-1 min-w-0">
        {renderView()}
      </main>
      <ToastContainer toasts={toasts} onDismiss={dismissToast} />
    </div>
  );
}

function App() {
  const [loggedIn, setLoggedIn] = useState(isLoggedIn());
  const [authView, setAuthView] = useState<AuthView>('login');
  const [pendingEmail, setPendingEmail] = useState('');
  const [pendingToken, setPendingToken] = useState<string | null>(null);

  const onLogout = () => {
    clearToken();
    setLoggedIn(false);
    setAuthView('login');
    setPendingEmail('');
    setPendingToken(null);
  };

  const renderAuth = () => {
    switch (authView) {
      case 'register':
        return (
          <RegisterPage
            onRegistered={(email, verificationToken) => {
              setPendingEmail(email);
              setPendingToken(verificationToken);
              setAuthView('verify');
            }}
            onGoToLogin={() => setAuthView('login')}
          />
        );
      case 'verify':
        return (
          <VerifyEmailPage
            email={pendingEmail}
            initialToken={pendingToken}
            onDone={() => setAuthView('login')}
            onGoToLogin={() => setAuthView('login')}
          />
        );
      default:
        return <LoginPage onLogin={() => setLoggedIn(true)} onGoToRegister={() => setAuthView('register')} />;
    }
  };

  return (
    <LocaleProvider>
      {!loggedIn ? (
        renderAuth()
      ) : (
        <AppProvider>
          <AppContent onLogout={onLogout} />
        </AppProvider>
      )}
    </LocaleProvider>
  );
}

export default App;
