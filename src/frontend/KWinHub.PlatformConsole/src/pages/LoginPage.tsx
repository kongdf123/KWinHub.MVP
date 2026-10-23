import { useState, type FormEvent } from 'react';
import { Loader2, ShieldCheck } from 'lucide-react';
import {
  ApiError,
  clearToken,
  isMockApiEnabled,
  login,
  setToken,
  type LoginResponse,
} from '../lib/api';
import { LanguageSwitcher, useI18n } from '../lib/i18n';
import { Button, Input } from '../components/ui';

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
      <aside className="relative flex flex-col justify-between overflow-hidden bg-gradient-to-br from-ink-950 via-kunwei-950 to-ink-900 px-6 py-6 text-ink-300 lg:w-[44%] lg:px-12 lg:py-10">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -right-24 -top-24 h-64 w-64 rounded-full bg-teal-500/10 blur-3xl"
        />
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -bottom-32 -left-16 h-72 w-72 rounded-full bg-kunwei-600/20 blur-3xl"
        />

        <div className="relative">
          <div className="flex items-center justify-between gap-3">
            <div className="flex items-center gap-3">
              <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-teal-500 text-ink-950">
                <ShieldCheck size={22} aria-hidden="true" />
              </span>
              <span className="min-w-0">
                <span className="block text-base font-semibold tracking-tight text-white">
                  KWinHub
                </span>
                <span className="block text-xs text-ink-400">{t('login.productLabel')}</span>
              </span>
            </div>

            <LanguageSwitcher dark />
          </div>

          <div className="mt-8 max-w-md lg:mt-20">
            <div className="inline-flex items-center gap-2 rounded-full border border-teal-500/30 bg-teal-500/10 px-3 py-1 text-xs font-medium text-teal-300">
              <ShieldCheck size={14} aria-hidden="true" />
              {t('login.restricted')}
            </div>

            <h1 className="mt-5 text-2xl font-semibold tracking-tight text-white sm:text-3xl">
              {t('login.title')}
            </h1>

            <p className="mt-2 text-sm leading-6 text-ink-300">{t('login.subtitle')}</p>
          </div>
        </div>

        <p className="relative mt-8 text-xs leading-5 text-ink-500 lg:mt-0">{t('login.footer')}</p>
      </aside>

      <section className="flex min-w-0 flex-1 items-center justify-center px-4 py-10 sm:px-6 lg:px-10">
        <div className="w-full max-w-md">
          <section
            aria-labelledby="login-form-title"
            className="rounded-xl border border-ink-200 bg-white p-6 shadow-sm sm:p-8"
          >
            <div className="mb-6 border-b border-ink-100 pb-5">
              <h2 id="login-form-title" className="text-base font-semibold text-ink-900">
                {t('login.formTitle')}
              </h2>
              <p className="mt-1 text-sm leading-5 text-ink-500">{t('login.accessDescription')}</p>
            </div>

            {alertMessage ? (
              <div
                role="alert"
                className="mb-5 rounded-lg border border-invalid-200 bg-invalid-50 px-3 py-2.5 text-sm leading-5 text-invalid-700"
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

                <Input
                  id="login-email"
                  name="email"
                  type="email"
                  autoComplete="username"
                  autoCapitalize="none"
                  spellCheck={false}
                  autoFocus
                  placeholder={t('login.emailPlaceholder')}
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  disabled={submitting}
                  required
                />
              </div>

              <div>
                <label
                  htmlFor="login-password"
                  className="mb-1.5 block text-sm font-medium text-ink-700"
                >
                  {t('login.password')}
                </label>

                <Input
                  id="login-password"
                  name="password"
                  type="password"
                  autoComplete="current-password"
                  placeholder={t('login.passwordPlaceholder')}
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  disabled={submitting}
                  required
                />
              </div>

              <Button type="submit" className="w-full !py-2.5" disabled={submitting}>
                {submitting ? (
                  <>
                    <Loader2 size={16} className="animate-spin" aria-hidden="true" />
                    {t('login.signingIn')}
                  </>
                ) : (
                  t('login.signIn')
                )}
              </Button>
            </form>

            <div className="mt-6 border-t border-ink-100 pt-4">
              <p className="text-xs leading-5 text-ink-500">{t('login.audienceNote')}</p>
            </div>
          </section>
        </div>
      </section>
    </main>
  );
}
