import { useState, type FormEvent } from 'react';
import {
  Activity,
  ArrowRight,
  Loader2,
  LockKeyhole,
  Mail,
  ShieldCheck,
} from 'lucide-react';
import {
  ApiError,
  clearToken,
  isMockApiEnabled,
  login,
  setToken,
  type LoginResponse,
} from '../lib/api';
import { LanguageSwitcher, useI18n, type TranslationKey } from '../lib/i18n';
import { Button, Input } from '../components/ui';

const PLATFORM_FEATURES: TranslationKey[] = [
  'login.feature.organizations',
  'login.feature.catalogs',
  'login.feature.audit',
];

export function LoginPage({
  onLogin,
  notice,
}: {
  onLogin: (session: LoginResponse) => void;
  notice?: string | null;
}) {
  const { t } = useI18n();

  // Credentials are only prefilled for the prototype mock backend. With the
  // real API enabled the fields stay blank.
  const [email, setEmail] = useState(isMockApiEnabled ? 'admin@kwinhub.com' : '');
  const [password, setPassword] = useState(isMockApiEnabled ? 'Admin@12345' : '');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (submitting) return;

    setError(null);
    setSubmitting(true);

    try {
      const result = await login(email.trim(), password);

      if (!result.isPlatformAdmin) {
        clearToken();
        setError(t('login.notAdmin'));
        return;
      }

      setToken(result.token);
      onLogin(result);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'));
    } finally {
      setSubmitting(false);
    }
  }

  const alertMessage = error ?? notice ?? null;

  return (
    <main className="min-h-screen bg-ink-50 lg:flex">
      {/* Brand panel */}
      <aside className="relative hidden min-h-screen overflow-hidden bg-ink-950 lg:flex lg:w-[46%] lg:flex-col lg:justify-between lg:p-12 xl:p-16">
        <div aria-hidden="true" className="pointer-events-none absolute inset-0 overflow-hidden">
          <div className="absolute -left-24 top-1/4 h-80 w-80 rounded-full bg-teal-600/20 blur-3xl" />
          <div className="absolute -bottom-24 right-0 h-96 w-96 rounded-full bg-kunwei-800/30 blur-3xl" />
        </div>

        <div className="relative z-10 flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-teal-500 text-white">
            <Activity size={24} aria-hidden="true" />
          </div>
          <div>
            <div className="text-xl font-semibold tracking-tight text-white">KWinHub</div>
            <div className="mt-0.5 text-xs text-ink-400">{t('login.productLabel')}</div>
          </div>
        </div>

        <div className="relative z-10 max-w-xl py-12">
          <div className="mb-5 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-3 py-1.5 text-xs font-medium text-teal-300">
            <ShieldCheck size={14} aria-hidden="true" />
            {t('login.restricted')}
          </div>

          <h1 className="text-4xl font-semibold leading-tight tracking-tight text-white xl:text-5xl">
            {t('login.brandHeadline')}
          </h1>

          <p className="mt-5 max-w-lg text-base leading-7 text-ink-300">
            {t('login.brandDescription')}
          </p>

          <ul className="mt-8 space-y-4">
            {PLATFORM_FEATURES.map((key) => (
              <li key={key} className="flex items-start gap-3 text-sm text-ink-200">
                <span className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-teal-500/15 text-teal-300">
                  <ShieldCheck size={13} aria-hidden="true" />
                </span>
                {t(key)}
              </li>
            ))}
          </ul>
        </div>

        <p className="relative z-10 text-xs text-ink-500">{t('login.footer')}</p>
      </aside>

      {/* Login area */}
      <section className="flex min-h-screen flex-1 flex-col px-5 py-6 sm:px-8 lg:px-12">
        <header className="flex items-center justify-between gap-4 lg:justify-end">
          <div className="flex items-center gap-2 lg:hidden">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-teal-600 text-white">
              <Activity size={20} aria-hidden="true" />
            </div>
            <span className="font-semibold text-ink-900">KWinHub</span>
          </div>

          <LanguageSwitcher />
        </header>

        <div className="flex flex-1 items-center justify-center py-10">
          <div className="w-full max-w-md">
            <div className="mb-7">
              <p className="mb-3 text-xs font-semibold uppercase tracking-[0.16em] text-teal-700">
                {t('login.productLabel')}
              </p>

              <h2 className="text-2xl font-semibold tracking-tight text-ink-900 sm:text-3xl">
                {t('login.title')}
              </h2>

              <p className="mt-2 text-sm leading-6 text-ink-500">{t('login.subtitle')}</p>
            </div>

            <div className="rounded-xl border border-ink-200 bg-white p-5 shadow-sm sm:p-8">
              <div className="mb-6 flex items-start gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-teal-50 text-teal-700">
                  <ShieldCheck size={20} aria-hidden="true" />
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-ink-900">{t('login.formTitle')}</h3>
                  <p className="mt-1 text-xs leading-5 text-ink-500">
                    {t('login.accessDescription')}
                  </p>
                </div>
              </div>

              {alertMessage ? (
                <div
                  role="alert"
                  aria-live="polite"
                  className="mb-5 rounded-lg border border-invalid-200 bg-invalid-50 px-3 py-3 text-sm leading-5 text-invalid-700"
                >
                  {alertMessage}
                </div>
              ) : null}

              <form onSubmit={handleSubmit} className="space-y-5">
                <div>
                  <label
                    htmlFor="login-email"
                    className="mb-1.5 block text-sm font-medium text-ink-700"
                  >
                    {t('login.email')}
                  </label>

                  <div className="relative">
                    <Mail
                      size={17}
                      aria-hidden="true"
                      className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400"
                    />
                    <Input
                      id="login-email"
                      name="email"
                      type="email"
                      autoComplete="username"
                      autoCapitalize="none"
                      spellCheck={false}
                      placeholder={t('login.emailPlaceholder')}
                      className="py-2.5 pl-10"
                      value={email}
                      onChange={(event) => setEmail(event.target.value)}
                      disabled={submitting}
                      required
                    />
                  </div>
                </div>

                <div>
                  <label
                    htmlFor="login-password"
                    className="mb-1.5 block text-sm font-medium text-ink-700"
                  >
                    {t('login.password')}
                  </label>

                  <div className="relative">
                    <LockKeyhole
                      size={17}
                      aria-hidden="true"
                      className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-ink-400"
                    />
                    <Input
                      id="login-password"
                      name="password"
                      type="password"
                      autoComplete="current-password"
                      placeholder={t('login.passwordPlaceholder')}
                      className="py-2.5 pl-10"
                      value={password}
                      onChange={(event) => setPassword(event.target.value)}
                      disabled={submitting}
                      required
                    />
                  </div>
                </div>

                <Button type="submit" className="w-full !py-2.5" disabled={submitting}>
                  {submitting ? (
                    <>
                      <Loader2 size={16} className="animate-spin" aria-hidden="true" />
                      {t('login.signingIn')}
                    </>
                  ) : (
                    <>
                      {t('login.signIn')}
                      <ArrowRight size={16} aria-hidden="true" />
                    </>
                  )}
                </Button>
              </form>

              <div className="mt-6 border-t border-ink-100 pt-4">
                <p className="text-xs leading-5 text-ink-500">{t('login.audienceNote')}</p>
              </div>
            </div>

            <p className="mt-6 text-center text-xs text-ink-400">{t('login.footer')}</p>
          </div>
        </div>
      </section>
    </main>
  );
}
