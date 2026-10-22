import { useState } from 'react';
import { Activity, ArrowRight, Building2, Loader2, Lock, Mail, User } from 'lucide-react';
import { http } from '@/lib/api/http';
import { LanguageSwitcher, useI18n } from '@/lib/i18n-context';

type AccountKind = 'organization' | 'individual';

interface RegisterPageProps {
  onRegistered: (email: string, verificationToken: string | null) => void;
  onGoToLogin: () => void;
}

interface RegistrationResponse {
  userId: string;
  email: string;
  organizationId: string | null;
  organizationName: string | null;
  organizationType: string | null;
  userStatus: string;
  verificationToken: string | null;
  verificationExpiresAt: string | null;
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const WEBSITE_PATTERN = /^https?:\/\/[^\s]+$/i;

const ORG_TYPES = [
  { value: 'sports_performance_center', labelKey: 'register.orgType.sportsPerformance' },
  { value: 'sports_club_team', labelKey: 'register.orgType.club' },
  { value: 'school_university', labelKey: 'register.orgType.school' },
  { value: 'research_institution', labelKey: 'register.orgType.research' },
  { value: 'other', labelKey: 'register.orgType.other' },
];

const COUNTRIES = [
  { code: 'CN', labelKey: 'register.country.cn' },
  { code: 'US', labelKey: 'register.country.us' },
  { code: 'JP', labelKey: 'register.country.jp' },
  { code: 'KR', labelKey: 'register.country.kr' },
  { code: 'DE', labelKey: 'register.country.de' },
  { code: 'GB', labelKey: 'register.country.gb' },
  { code: 'AU', labelKey: 'register.country.au' },
  { code: 'CA', labelKey: 'register.country.ca' },
  { code: 'SG', labelKey: 'register.country.sg' },
  { code: 'OTHER', labelKey: 'register.country.other' },
];

const INTENDED_USES = [
  { value: 'team_and_athlete_testing', labelKey: 'register.use.teamTesting' },
  { value: 'research_and_lab_testing', labelKey: 'register.use.research' },
  { value: 'commercial_performance_services', labelKey: 'register.use.commercial' },
  { value: 'other', labelKey: 'register.use.other' },
];

const PRIMARY_ROLES = [
  { value: 'coach_sports_scientist', labelKey: 'register.role.coach' },
  { value: 'athlete', labelKey: 'register.role.athlete' },
  { value: 'researcher_student', labelKey: 'register.role.researcher' },
  { value: 'other', labelKey: 'register.role.other' },
];

const labelClass = 'block text-sm font-medium text-ink-700 mb-1.5';
const helperClass = 'mt-1 text-xs text-ink-400';

function inputClass(hasError: boolean): string {
  const base = 'w-full rounded-lg border bg-white px-3 py-2.5 text-sm transition-all focus:outline-none focus:ring-2';
  return hasError
    ? `${base} border-invalid-300 focus:border-invalid-500 focus:ring-invalid-200`
    : `${base} border-ink-200 focus:border-teal-400 focus:ring-teal-400`;
}

function FieldError({ message }: { message?: string }) {
  if (!message) return null;
  return <p className="mt-1 text-xs text-invalid-600">{message}</p>;
}

function getErrorMessage(err: unknown, fallback: string): string {
  if (typeof err === 'object' && err !== null) {
    const candidate = err as { message?: unknown; detail?: unknown };
    if (typeof candidate.message === 'string' && candidate.message.trim()) return candidate.message;
    if (typeof candidate.detail === 'string' && candidate.detail.trim()) return candidate.detail;
  }
  return fallback;
}

export function RegisterPage({ onRegistered, onGoToLogin }: RegisterPageProps) {
  const { t, locale } = useI18n();

  const [kind, setKind] = useState<AccountKind>('organization');

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [acceptTerms, setAcceptTerms] = useState(false);
  const [marketingConsent, setMarketingConsent] = useState(false);

  const [orgName, setOrgName] = useState('');
  const [orgType, setOrgType] = useState('');
  const [orgCountry, setOrgCountry] = useState('');
  const [website, setWebsite] = useState('');
  const [intendedUse, setIntendedUse] = useState('');

  const [fullName, setFullName] = useState('');
  const [workspaceName, setWorkspaceName] = useState('');
  const [primaryRole, setPrimaryRole] = useState('');
  const [personalCountry, setPersonalCountry] = useState('');

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const validate = (): Record<string, string> => {
    const next: Record<string, string> = {};

    if (kind === 'organization') {
      if (!orgName.trim()) next.orgName = t('register.validation.required');
      if (!orgType) next.orgType = t('register.validation.required');
      if (!orgCountry) next.orgCountry = t('register.validation.required');
      if (!intendedUse) next.intendedUse = t('register.validation.required');
      if (website.trim() && !WEBSITE_PATTERN.test(website.trim())) {
        next.website = t('register.validation.website');
      }
    } else {
      if (!fullName.trim()) next.fullName = t('register.validation.required');
      if (!workspaceName.trim()) next.workspaceName = t('register.validation.required');
      if (!personalCountry) next.countryCode = t('register.validation.required');
    }

    if (!email.trim()) {
      next.email = t('register.validation.required');
    } else if (!EMAIL_PATTERN.test(email.trim())) {
      next.email = t('register.validation.email');
    }

    if (!password) {
      next.password = t('register.validation.required');
    } else if (password.length < 8) {
      next.password = t('register.validation.password');
    }

    if (!confirmPassword) {
      next.confirmPassword = t('register.validation.required');
    } else if (confirmPassword !== password) {
      next.confirmPassword = t('register.validation.passwordMismatch');
    }

    if (!acceptTerms) next.terms = t('register.validation.terms');

    return next;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    const nextErrors = validate();
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0) return;

    const normalizedEmail = email.trim();
    const payload: Record<string, unknown> = {
      accountType: kind === 'organization' ? 'Standard' : 'Individual',
      email: normalizedEmail,
      password,
      displayName: kind === 'organization' ? orgName.trim() : fullName.trim(),
      acceptTerms,
      termsVersion: '2026-10',
      preferredLanguage: locale,
      marketingConsent,
    };

    if (kind === 'organization') {
      payload.organization = {
        name: orgName.trim(),
        // The backend accepts only "Standard" or "Individual" for organization.type.
        type: 'Standard',
        countryCode: orgCountry,
        website: website.trim() || null,
        intendedUse,
      };
    } else {
      payload.personalWorkspaceName = workspaceName.trim();
      payload.fullName = fullName.trim();
      payload.primaryRole = primaryRole || null;
      payload.countryCode = personalCountry;
    }

    setSubmitting(true);
    try {
      if (import.meta.env.VITE_DATA_MODE === 'api') {
        const response = await http.post<RegistrationResponse>('/registrations', payload);
        onRegistered(response.email || normalizedEmail, response.verificationToken ?? null);
      } else {
        await new Promise((resolve) => setTimeout(resolve, 500));
        onRegistered(normalizedEmail, 'demo-verification-token');
      }
    } catch (err) {
      setSubmitError(getErrorMessage(err, t('register.error')));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="h-screen flex overflow-hidden">
      <div className="hidden lg:flex lg:w-1/2 bg-ink-950 relative overflow-hidden shrink-0">
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

      <div className="flex-1 min-w-0 overflow-y-auto bg-ink-50">
        <div className="flex min-h-full justify-center px-6 py-10">
          <div className="w-full max-w-xl">
            <div className="flex items-center justify-between mb-8">
              <div className="flex items-center gap-2.5 lg:hidden">
                <div className="w-10 h-10 rounded-xl bg-teal-500 flex items-center justify-center">
                  <Activity className="w-5 h-5 text-white" />
                </div>
                <span className="text-lg font-semibold text-ink-900">KWinHub</span>
              </div>
              <LanguageSwitcher className="ml-auto" />
            </div>

            <h2 className="text-2xl font-semibold text-ink-900">{t('register.title')}</h2>
            <p className="text-sm text-ink-500 mt-1 mb-6">{t('register.subtitle')}</p>

            <form onSubmit={handleSubmit} noValidate className="space-y-4">
              <div>
                <span className={labelClass}>{t('register.accountType')}</span>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <button
                    type="button"
                    onClick={() => setKind('organization')}
                    aria-pressed={kind === 'organization'}
                    className={`flex items-start gap-3 rounded-lg border p-4 text-left transition-all ${
                      kind === 'organization'
                        ? 'border-teal-500 ring-2 ring-teal-400/30 bg-teal-50/40'
                        : 'border-ink-200 bg-white hover:border-ink-300'
                    }`}
                  >
                    <div
                      className={`w-9 h-9 rounded-lg flex items-center justify-center shrink-0 ${
                        kind === 'organization' ? 'bg-teal-100' : 'bg-ink-100'
                      }`}
                    >
                      <Building2 className={`w-5 h-5 ${kind === 'organization' ? 'text-teal-700' : 'text-ink-500'}`} />
                    </div>
                    <div>
                      <div className="text-sm font-semibold text-ink-900">{t('register.accountType.organization')}</div>
                      <div className="text-xs text-ink-500 mt-0.5">{t('register.accountType.organizationHint')}</div>
                    </div>
                  </button>

                  <button
                    type="button"
                    onClick={() => setKind('individual')}
                    aria-pressed={kind === 'individual'}
                    className={`flex items-start gap-3 rounded-lg border p-4 text-left transition-all ${
                      kind === 'individual'
                        ? 'border-teal-500 ring-2 ring-teal-400/30 bg-teal-50/40'
                        : 'border-ink-200 bg-white hover:border-ink-300'
                    }`}
                  >
                    <div
                      className={`w-9 h-9 rounded-lg flex items-center justify-center shrink-0 ${
                        kind === 'individual' ? 'bg-teal-100' : 'bg-ink-100'
                      }`}
                    >
                      <User className={`w-5 h-5 ${kind === 'individual' ? 'text-teal-700' : 'text-ink-500'}`} />
                    </div>
                    <div>
                      <div className="text-sm font-semibold text-ink-900">{t('register.accountType.individual')}</div>
                      <div className="text-xs text-ink-500 mt-0.5">{t('register.accountType.individualHint')}</div>
                    </div>
                  </button>
                </div>
              </div>

              {kind === 'organization' ? (
                <section className="rounded-lg border border-ink-200 bg-white p-5 space-y-4">
                  <h3 className="text-sm font-semibold text-ink-900">{t('register.section.organization')}</h3>

                  <div>
                    <label htmlFor="org-name" className={labelClass}>
                      {t('register.organizationName')} *
                    </label>
                    <input
                      id="org-name"
                      type="text"
                      autoComplete="organization"
                      value={orgName}
                      onChange={(e) => setOrgName(e.target.value)}
                      className={inputClass(!!errors.orgName)}
                    />
                    <FieldError message={errors.orgName} />
                  </div>

                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div>
                      <label htmlFor="org-type" className={labelClass}>
                        {t('register.organizationType')} *
                      </label>
                      <select
                        id="org-type"
                        value={orgType}
                        onChange={(e) => setOrgType(e.target.value)}
                        className={inputClass(!!errors.orgType)}
                      >
                        <option value="">{t('common.selectPlaceholder')}</option>
                        {ORG_TYPES.map((option) => (
                          <option key={option.value} value={option.value}>
                            {t(option.labelKey)}
                          </option>
                        ))}
                      </select>
                      <FieldError message={errors.orgType} />
                    </div>

                    <div>
                      <label htmlFor="org-country" className={labelClass}>
                        {t('register.country')} *
                      </label>
                      <select
                        id="org-country"
                        value={orgCountry}
                        onChange={(e) => setOrgCountry(e.target.value)}
                        className={inputClass(!!errors.orgCountry)}
                      >
                        <option value="">{t('common.selectPlaceholder')}</option>
                        {COUNTRIES.map((option) => (
                          <option key={option.code} value={option.code}>
                            {t(option.labelKey)}
                          </option>
                        ))}
                      </select>
                      <FieldError message={errors.orgCountry} />
                    </div>
                  </div>

                  <div>
                    <label htmlFor="org-website" className={labelClass}>
                      {t('register.website')}{' '}
                      <span className="font-normal text-ink-400">{t('common.optional')}</span>
                    </label>
                    <input
                      id="org-website"
                      type="url"
                      value={website}
                      onChange={(e) => setWebsite(e.target.value)}
                      placeholder={t('register.websitePlaceholder')}
                      className={inputClass(!!errors.website)}
                    />
                    <FieldError message={errors.website} />
                  </div>

                  <div>
                    <label htmlFor="org-intended-use" className={labelClass}>
                      {t('register.intendedUse')} *
                    </label>
                    <select
                      id="org-intended-use"
                      value={intendedUse}
                      onChange={(e) => setIntendedUse(e.target.value)}
                      className={inputClass(!!errors.intendedUse)}
                    >
                      <option value="">{t('common.selectPlaceholder')}</option>
                      {INTENDED_USES.map((option) => (
                        <option key={option.value} value={option.value}>
                          {t(option.labelKey)}
                        </option>
                      ))}
                    </select>
                    <FieldError message={errors.intendedUse} />
                  </div>
                </section>
              ) : (
                <section className="rounded-lg border border-ink-200 bg-white p-5 space-y-4">
                  <h3 className="text-sm font-semibold text-ink-900">{t('register.section.personal')}</h3>

                  <div>
                    <label htmlFor="personal-name" className={labelClass}>
                      {t('register.fullName')} *
                    </label>
                    <input
                      id="personal-name"
                      type="text"
                      autoComplete="name"
                      value={fullName}
                      onChange={(e) => setFullName(e.target.value)}
                      className={inputClass(!!errors.fullName)}
                    />
                    <FieldError message={errors.fullName} />
                  </div>

                  <div>
                    <label htmlFor="personal-workspace" className={labelClass}>
                      {t('register.workspaceName')} *
                    </label>
                    <input
                      id="personal-workspace"
                      type="text"
                      value={workspaceName}
                      onChange={(e) => setWorkspaceName(e.target.value)}
                      className={inputClass(!!errors.workspaceName)}
                    />
                    <p className={helperClass}>{t('register.workspaceNameHelper')}</p>
                    <FieldError message={errors.workspaceName} />
                  </div>

                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div>
                      <label htmlFor="personal-role" className={labelClass}>
                        {t('register.primaryRole')}
                      </label>
                      <select
                        id="personal-role"
                        value={primaryRole}
                        onChange={(e) => setPrimaryRole(e.target.value)}
                        className={inputClass(false)}
                      >
                        <option value="">{t('common.selectPlaceholder')}</option>
                        {PRIMARY_ROLES.map((option) => (
                          <option key={option.value} value={option.value}>
                            {t(option.labelKey)}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div>
                      <label htmlFor="personal-country" className={labelClass}>
                        {t('register.country')} *
                      </label>
                      <select
                        id="personal-country"
                        value={personalCountry}
                        onChange={(e) => setPersonalCountry(e.target.value)}
                        className={inputClass(!!errors.countryCode)}
                      >
                        <option value="">{t('common.selectPlaceholder')}</option>
                        {COUNTRIES.map((option) => (
                          <option key={option.code} value={option.code}>
                            {t(option.labelKey)}
                          </option>
                        ))}
                      </select>
                      <FieldError message={errors.countryCode} />
                    </div>
                  </div>
                </section>
              )}

              <section className="rounded-lg border border-ink-200 bg-white p-5 space-y-4">
                <h3 className="text-sm font-semibold text-ink-900">{t('register.section.credentials')}</h3>

                <div>
                  <label htmlFor="register-email" className={labelClass}>
                    {t('register.email')} *
                  </label>
                  <div className="relative">
                    <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
                    <input
                      id="register-email"
                      type="email"
                      autoComplete="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      className={`${inputClass(!!errors.email)} pl-10`}
                    />
                  </div>
                  <FieldError message={errors.email} />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label htmlFor="register-password" className={labelClass}>
                      {t('register.password')} *
                    </label>
                    <div className="relative">
                      <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
                      <input
                        id="register-password"
                        type="password"
                        autoComplete="new-password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className={`${inputClass(!!errors.password)} pl-10`}
                      />
                    </div>
                    <p className={helperClass}>{t('register.passwordHelper')}</p>
                    <FieldError message={errors.password} />
                  </div>

                  <div>
                    <label htmlFor="register-confirm-password" className={labelClass}>
                      {t('register.confirmPassword')} *
                    </label>
                    <div className="relative">
                      <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-ink-400" />
                      <input
                        id="register-confirm-password"
                        type="password"
                        autoComplete="new-password"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        className={`${inputClass(!!errors.confirmPassword)} pl-10`}
                      />
                    </div>
                    <FieldError message={errors.confirmPassword} />
                  </div>
                </div>

                <div className="space-y-2 pt-1">
                  <label className="flex items-start gap-2 text-sm text-ink-600">
                    <input
                      type="checkbox"
                      checked={acceptTerms}
                      onChange={(e) => setAcceptTerms(e.target.checked)}
                      className="mt-0.5 rounded border-ink-300 text-teal-600 focus:ring-teal-400"
                    />
                    <span>
                      {t('register.terms')} *
                    </span>
                  </label>
                  <FieldError message={errors.terms} />

                  <label className="flex items-start gap-2 text-sm text-ink-600">
                    <input
                      type="checkbox"
                      checked={marketingConsent}
                      onChange={(e) => setMarketingConsent(e.target.checked)}
                      className="mt-0.5 rounded border-ink-300 text-teal-600 focus:ring-teal-400"
                    />
                    <span>{t('register.marketing')}</span>
                  </label>
                </div>
              </section>

              {submitError && (
                <div className="rounded-lg border border-invalid-200 bg-invalid-50 px-3 py-2 text-xs text-invalid-700">
                  {submitError}
                </div>
              )}

              <button
                type="submit"
                disabled={submitting}
                className="w-full flex items-center justify-center gap-2 py-2.5 bg-teal-600 hover:bg-teal-700 text-white font-medium text-sm rounded-lg transition-colors disabled:opacity-60"
              >
                {submitting ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" />
                    {t('register.submitting')}
                  </>
                ) : (
                  <>
                    {t('register.submit')}
                    <ArrowRight className="w-4 h-4" />
                  </>
                )}
              </button>

              <p className="text-xs text-ink-500">{t('register.footerNote')}</p>
            </form>

            <p className="text-sm text-ink-600 mt-6">
              {t('register.haveAccount')}{' '}
              <button
                type="button"
                onClick={onGoToLogin}
                className="font-medium text-teal-600 hover:text-teal-700"
              >
                {t('register.signIn')}
              </button>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
