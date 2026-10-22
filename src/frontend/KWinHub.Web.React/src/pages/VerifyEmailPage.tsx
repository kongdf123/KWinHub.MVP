import { useCallback, useEffect, useRef, useState } from 'react';
import { Activity, AlertCircle, CheckCircle2, Loader2, Mail } from 'lucide-react';
import { http } from '@/lib/api/http';
import { LanguageSwitcher, useI18n } from '@/lib/i18n-context';

interface VerifyEmailPageProps {
  email: string;
  initialToken: string | null;
  onDone: (verified: boolean) => void;
  onGoToLogin: () => void;
}

interface VerifyEmailResponse {
  verified: boolean;
  email: string;
  status: string;
}

type VerifyState = 'idle' | 'verifying' | 'verified' | 'failed';

function getErrorMessage(err: unknown, fallback: string): string {
  if (typeof err === 'object' && err !== null) {
    const candidate = err as { message?: unknown; detail?: unknown };
    if (typeof candidate.message === 'string' && candidate.message.trim()) return candidate.message;
    if (typeof candidate.detail === 'string' && candidate.detail.trim()) return candidate.detail;
  }
  return fallback;
}

export function VerifyEmailPage({ email, initialToken, onDone, onGoToLogin }: VerifyEmailPageProps) {
  const { t } = useI18n();
  const [token, setToken] = useState(initialToken ?? '');
  const [state, setState] = useState<VerifyState>('idle');
  const [error, setError] = useState<string | null>(null);
  const autoSubmitted = useRef(false);

  const verify = useCallback(
    async (rawToken: string) => {
      const value = rawToken.trim();
      if (!value) {
        setState('failed');
        setError(t('verify.tokenRequired'));
        return;
      }

      setState('verifying');
      setError(null);

      try {
        if (import.meta.env.VITE_DATA_MODE === 'api') {
          await http.post<VerifyEmailResponse>('/auth/verify-email', { token: value });
        } else {
          await new Promise((resolve) => setTimeout(resolve, 500));
        }
        setState('verified');
      } catch (err) {
        setState('failed');
        setError(getErrorMessage(err, t('verify.error')));
      }
    },
    [t],
  );

  useEffect(() => {
    if (!initialToken || autoSubmitted.current) return;
    autoSubmitted.current = true;
    void verify(initialToken);
  }, [initialToken, verify]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    void verify(token);
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-ink-50 px-6 py-10">
      <div className="w-full max-w-md">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2.5">
            <div className="w-10 h-10 rounded-xl bg-teal-500 flex items-center justify-center">
              <Activity className="w-5 h-5 text-white" />
            </div>
            <span className="text-lg font-semibold text-ink-900">KWinHub</span>
          </div>
          <LanguageSwitcher />
        </div>

        <div className="bg-white border border-ink-200 rounded-lg shadow-sm p-8 text-center">
          {state === 'verified' ? (
            <>
              <div className="w-14 h-14 rounded-full bg-success-50 flex items-center justify-center mx-auto mb-5">
                <CheckCircle2 className="w-7 h-7 text-success-600" />
              </div>
              <h1 className="text-xl font-semibold text-ink-900">{t('verify.verified')}</h1>
              <p className="text-sm text-ink-500 mt-2">{t('verify.verifiedHint')}</p>
              <button
                type="button"
                onClick={onGoToLogin}
                className="w-full mt-6 flex items-center justify-center gap-2 py-2.5 bg-teal-600 hover:bg-teal-700 text-white font-medium text-sm rounded-lg transition-colors"
              >
                {t('verify.continue')}
              </button>
            </>
          ) : state === 'verifying' ? (
            <>
              <div className="w-14 h-14 rounded-full bg-teal-50 flex items-center justify-center mx-auto mb-5">
                <Loader2 className="w-7 h-7 text-teal-600 animate-spin" />
              </div>
              <h1 className="text-xl font-semibold text-ink-900">{t('verify.verifying')}</h1>
              <p className="text-sm text-ink-500 mt-2">{t('verify.verifyingHint')}</p>
            </>
          ) : (
            <>
              <div className="w-14 h-14 rounded-full bg-teal-50 flex items-center justify-center mx-auto mb-5">
                <Mail className="w-7 h-7 text-teal-600" />
              </div>
              <h1 className="text-xl font-semibold text-ink-900">
                {state === 'failed' ? t('verify.failed') : t('verify.title')}
              </h1>
              {email && <p className="text-sm text-ink-500 mt-2">{t('verify.subtitle', { email })}</p>}

              {state === 'failed' && (
                <div className="mt-4 flex items-start gap-2 rounded-lg border border-invalid-200 bg-invalid-50 px-3 py-2 text-left text-xs text-invalid-700">
                  <AlertCircle className="w-4 h-4 shrink-0 mt-0.5" />
                  <span>{error || t('verify.error')}</span>
                </div>
              )}

              <form onSubmit={handleSubmit} className="mt-6 text-left space-y-3">
                <div>
                  <label htmlFor="verify-token" className="block text-sm font-medium text-ink-700 mb-1.5">
                    {t('verify.tokenLabel')}
                  </label>
                  <input
                    id="verify-token"
                    type="text"
                    value={token}
                    onChange={(e) => setToken(e.target.value)}
                    placeholder={t('verify.tokenPlaceholder')}
                    className="w-full rounded-lg border border-ink-200 bg-white px-3 py-2.5 text-sm transition-all focus:outline-none focus:ring-2 focus:border-teal-400 focus:ring-teal-400"
                  />
                </div>
                <button
                  type="submit"
                  className="w-full flex items-center justify-center gap-2 py-2.5 bg-teal-600 hover:bg-teal-700 text-white font-medium text-sm rounded-lg transition-colors"
                >
                  {t('verify.submit')}
                </button>
              </form>

              {state === 'failed' && (
                <button
                  type="button"
                  onClick={() => onDone(false)}
                  className="mt-4 text-sm font-medium text-teal-600 hover:text-teal-700"
                >
                  {t('verify.backToSignIn')}
                </button>
              )}
            </>
          )}
        </div>

        <p className="text-xs text-ink-400 text-center mt-4">{t('verify.productionNote')}</p>
      </div>
    </div>
  );
}
