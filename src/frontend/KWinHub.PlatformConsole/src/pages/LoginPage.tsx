import { useState, type FormEvent } from 'react';
import { ShieldCheck } from 'lucide-react';
import { ApiError, clearToken, login, setToken, type LoginResponse } from '../lib/api';
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
  const [email, setEmail] = useState('admin@kwinhub.com');
  const [password, setPassword] = useState('Admin@12345');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      const result = await login(email, password);
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
    <div className="relative flex min-h-screen items-center justify-center bg-gradient-to-br from-ink-950 via-kunwei-950 to-ink-900 px-4 py-10">
      <div className="absolute right-5 top-5">
        <LanguageSwitcher dark />
      </div>

      <div className="w-full max-w-md">
        <div className="mb-6 flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-teal-500 text-ink-950">
            <ShieldCheck size={22} />
          </div>
          <div>
            <h1 className="text-lg font-semibold text-white">{t('login.title')}</h1>
            <p className="text-sm text-ink-400">{t('login.subtitle')}</p>
          </div>
        </div>

        <div className="rounded-2xl bg-white p-7 shadow-2xl">
          <div className="mb-5 flex items-center gap-2 text-xs font-medium uppercase tracking-wide text-ink-500">
            <ShieldCheck size={14} className="text-teal-600" />
            {t('login.restricted')}
          </div>

          {alertMessage ? (
            <div className="mb-4 rounded-md border border-invalid-200 bg-invalid-50 px-3 py-2 text-sm text-invalid-700">
              {alertMessage}
            </div>
          ) : null}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="login-email" className="mb-1 block text-sm font-medium text-ink-700">
                {t('login.email')}
              </label>
              <Input
                id="login-email"
                type="email"
                autoComplete="username"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
              />
            </div>

            <div>
              <label htmlFor="login-password" className="mb-1 block text-sm font-medium text-ink-700">
                {t('login.password')}
              </label>
              <Input
                id="login-password"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                required
              />
            </div>

            <Button type="submit" className="w-full !py-2" disabled={submitting}>
              {submitting ? t('login.signingIn') : t('login.signIn')}
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
