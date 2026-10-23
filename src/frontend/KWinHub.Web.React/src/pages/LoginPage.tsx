import { useState } from 'react';
import { Activity, ArrowRight, Loader2, Lock, Mail } from 'lucide-react';
import { http, setToken, clearToken } from '@/lib/api/http';
import { LanguageSwitcher, useI18n } from '@/lib/i18n-context';

interface LoginPageProps {
  onLogin: () => void;
  onGoToRegister: () => void;
}

interface LoginResponse {
  token: string;
  type: string;
  userId: string;
  email: string;
  displayName: string;
  organizationId: string;
  organizationName: string;
  organizationRole: string;
  isPlatformAdmin: boolean;
  emailVerified: boolean;
  status: string;
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function getErrorMessage(err: unknown, fallback: string): string {
  if (typeof err === 'object' && err !== null) {
    const candidate = err as { message?: unknown; detail?: unknown };
    if (typeof candidate.message === 'string' && candidate.message.trim()) return candidate.message;
    if (typeof candidate.detail === 'string' && candidate.detail.trim()) return candidate.detail;
  }
  return fallback;
}

export function LoginPage({ onLogin, onGoToRegister }: LoginPageProps) {
  const { t } = useI18n();
  const [email, setEmail] = useState('demo@kwinhub.com');
  const [password, setPassword] = useState('Demo@12345');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [platformAdmin, setPlatformAdmin] = useState(false);

  // Inside the Tauri desktop shell, the console is bundled next to this app
  // (../console/) and the link switches views in-app instead of opening a tab.
  const isTauri = typeof window !== 'undefined' && '__TAURI_INTERNALS__' in window;
  const consoleUrl = isTauri
    ? '../console/index.html'
    : (import.meta.env.VITE_PLATFORM_CONSOLE_URL ?? 'http://localhost:5174');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setPlatformAdmin(false);

    if (!email.trim() || !password) {
      setError(t('login.validation.required'));
      return;
    }
    if (!EMAIL_PATTERN.test(email.trim())) {
      setError(t('login.validation.email'));
      return;
    }

    setLoading(true);
    try {
      if (import.meta.env.VITE_DATA_MODE === 'api') {
        const response = await http.post<LoginResponse>('/auth/login', {
          email: email.trim(),
          password,
        });
        setToken(response.token);
        if (response.isPlatformAdmin) {
          // Platform admins have no organization workspace — keep them on this screen
          // with a shortcut into the console instead of opening the org app.
          setPlatformAdmin(true);
          return;
        }
      } else {
        // Demo mode: fake token, real check so App.tsx's isLoggedIn() picks it up
        setToken('demo-token');
      }
      onLogin();
    } catch (err) {
      setError(getErrorMessage(err, t('login.error')));
      clearToken();
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex">
      <div className="hidden lg:flex lg:w-1/2 bg-ink-950 relative overflow-hidden">
        <div className="absolute inset-0 opacity-30">
          <div className="absolute top-1/4 left-1/4 w-96 h-96 rounded-full bg-teal-600 blur-3xl" />
          <div className="absolute bottom-1/4 right-1/4 w-80 h-80 rounded-full bg-kunwei-800 blur-3xl" />
        </div>
        <div className="relative z-10 flex flex-col justify-between p-12 text-white">
          <div className="flex items-center gap-3">
            <div className="w-11 h-11 rounded-xl bg-teal-500 flex items-center justify-center">
              <Activity className="w-6 h-6 text-white" />
            </div>
            <span className="text-xl font-semibold">KWinHub</span>
          </div>
          <div>
            <h1 className="text-4xl font-bold leading-tight mb-4">{t('brand.tagline')}</h1>
            <p className="text-lg text-ink-300 max-w-md leading-relaxed">{t('brand.description')}</p>
            <div className="mt-8 space-y-3">
              {['brand.feature1', 'brand.feature2', 'brand.feature3', 'brand.feature4'].map((key) => (
                <div key={key} className="flex items-center gap-3 text-ink-200">
                  <div className="w-5 h-5 rounded-full bg-success-500/20 flex items-center justify-center">
                    <div className="w-2 h-2 rounded-full bg-success-400" />
                  </div>
                  <span className="text-sm">{t(key)}</span>
                </div>
              ))}
            </div>
          </div>
          <p className="text-xs text-ink-500">{t('brand.copyright')}</p>
        </div>
      </div>

      <div className="flex-1 flex items-center justify-center bg-ink-50 px-6 py-10">
        <div className="w-full max-w-sm">
          <div className="flex items-center justify-between mb-8">
            <div className="lg:hidden flex items-center gap-2.5">
              <div className="w-10 h-10 rounded-xl bg-teal-500 flex items-center justify-center">
                <Activity className="w-5 h-5 text-white" />
              </div>
              <span className="text-lg font-semibold text-ink-900">KWinHub</span>
            </div>
            <LanguageSwitcher className="ml-auto" />
          </div>

          <h2 className="text-2xl font-semibold text-ink-900 mb-1">{t('login.title')}</h2>
          <p className="text-sm text-ink-500 mb-8">{t('login.subtitle')}</p>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">{t('login.email')}</label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full pl-10 pr-3 py-2.5 text-sm bg-white border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400 focus:border-teal-400 transition-all"
                  required
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">{t('login.password')}</label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full pl-10 pr-3 py-2.5 text-sm bg-white border border-ink-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-400 focus:border-teal-400 transition-all"
                  required
                />
              </div>
            </div>
            <div className="flex items-center justify-between text-sm">
              <label className="flex items-center gap-2 text-ink-600">
                <input type="checkbox" defaultChecked className="rounded border-ink-300 text-teal-600 focus:ring-teal-400" />
                {t('login.remember')}
              </label>
              <button type="button" className="text-teal-600 hover:text-teal-700 font-medium">
                {t('login.forgot')}
              </button>
            </div>
            {error && (
              <div className="rounded-lg border border-invalid-200 bg-invalid-50 px-3 py-2 text-xs text-invalid-700">
                {error}
              </div>
            )}
            {platformAdmin && (
              <div className="rounded-lg border border-teal-200 bg-teal-50 px-3 py-2 text-xs text-teal-800">
                <a href={consoleUrl} className="font-medium underline" {...(isTauri ? {} : { target: '_blank', rel: 'noreferrer' })}>
                  {t('login.platformAdmin')}
                </a>
              </div>
            )}
            <button
              type="submit"
              disabled={loading}
              className="w-full flex items-center justify-center gap-2 py-2.5 bg-teal-600 hover:bg-teal-700 text-white font-medium text-sm rounded-lg transition-colors disabled:opacity-60"
            >
              {loading ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  {t('login.submitting')}
                </>
              ) : (
                <>
                  {t('login.submit')}
                  <ArrowRight className="w-4 h-4" />
                </>
              )}
            </button>
          </form>

          <p className="text-xs text-ink-400 text-center mt-6">{t('login.demoHint')}</p>

          <p className="text-sm text-ink-600 text-center mt-4">
            {t('login.noAccount')}{' '}
            <button
              type="button"
              onClick={onGoToRegister}
              className="font-medium text-teal-600 hover:text-teal-700"
            >
              {t('login.createAccount')}
            </button>
          </p>
        </div>
      </div>
    </div>
  );
}
