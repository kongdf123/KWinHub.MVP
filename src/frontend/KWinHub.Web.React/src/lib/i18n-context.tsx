import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';

export type Locale = 'en' | 'zh-CN';

type LocalizedString = { en: string; 'zh-CN': string };

const LOCALE_STORAGE_KEY = 'kwinhub.locale';

const STRINGS: Record<string, LocalizedString> = {
  // Shared brand panel
  'brand.tagline': { en: 'Force testing, elevated to a system.', 'zh-CN': '让力量测试成为一套系统。' },
  'brand.description': {
    en: 'The operating system for sports-performance testing. Capture, analyze, and act on neuromuscular data — all in one place.',
    'zh-CN': '运动表现测试的操作系统。采集、分析并应用神经肌肉数据——全部集中在一处。',
  },
  'brand.feature1': { en: 'Multi-athlete testing workflows', 'zh-CN': '多运动员测试流程' },
  'brand.feature2': { en: 'Real-time validity checking', 'zh-CN': '实时有效性检查' },
  'brand.feature3': { en: 'Automated trend analysis & alerts', 'zh-CN': '自动趋势分析与提醒' },
  'brand.feature4': { en: 'Configurable reports & exports', 'zh-CN': '可配置的报告与导出' },
  'brand.copyright': { en: '© 2026 Kunwei Performance Technologies', 'zh-CN': '© 2026 昆威运动表现科技' },

  // Shared
  'common.optional': { en: '(optional)', 'zh-CN': '（选填）' },
  'common.selectPlaceholder': { en: 'Select...', 'zh-CN': '请选择...' },

  // Login
  'login.title': { en: 'Welcome back', 'zh-CN': '欢迎回来' },
  'login.subtitle': { en: "Sign in to your organization's testing hub", 'zh-CN': '登录你的组织测试中心' },
  'login.email': { en: 'Email', 'zh-CN': '邮箱' },
  'login.password': { en: 'Password', 'zh-CN': '密码' },
  'login.remember': { en: 'Remember me', 'zh-CN': '记住我' },
  'login.forgot': { en: 'Forgot password?', 'zh-CN': '忘记密码？' },
  'login.submit': { en: 'Sign In', 'zh-CN': '登录' },
  'login.submitting': { en: 'Signing in...', 'zh-CN': '正在登录...' },
  'login.demoHint': {
    en: 'Demo account: demo@kwinhub.com / Demo@12345',
    'zh-CN': '演示账号：demo@kwinhub.com / Demo@12345',
  },
  'login.noAccount': { en: "Don't have an account?", 'zh-CN': '还没有账号？' },
  'login.createAccount': { en: 'Create one', 'zh-CN': '创建一个' },
  'login.error': {
    en: 'Login failed — check your credentials or make sure the backend is running.',
    'zh-CN': '登录失败——请检查账号密码，或确认后端服务已启动。',
  },
  'login.validation.required': { en: 'Enter your email and password.', 'zh-CN': '请输入邮箱和密码。' },
  'login.validation.email': { en: 'Enter a valid email address.', 'zh-CN': '请输入有效的邮箱地址。' },
  'login.platformAdmin': {
    en: 'Platform administrator — open the Platform Console',
    'zh-CN': '平台管理员——打开平台控制台',
  },

  // Registration
  'register.title': { en: 'Create your KWinHub account', 'zh-CN': '创建你的 KWinHub 账号' },
  'register.subtitle': {
    en: 'Create a workspace for your organization or yourself.',
    'zh-CN': '为你所在的组织或你自己创建工作空间。',
  },
  'register.accountType': { en: 'Account type', 'zh-CN': '账号类型' },
  'register.accountType.organization': { en: 'Organization', 'zh-CN': '组织' },
  'register.accountType.organizationHint': {
    en: 'Club, school, lab or sports center',
    'zh-CN': '俱乐部、学校、实验室或运动中心',
  },
  'register.accountType.individual': { en: 'Individual', 'zh-CN': '个人' },
  'register.accountType.individualHint': { en: 'Personal workspace', 'zh-CN': '个人工作空间' },
  'register.section.organization': { en: 'Organization details', 'zh-CN': '组织信息' },
  'register.section.personal': { en: 'Personal workspace', 'zh-CN': '个人工作空间' },
  'register.section.credentials': { en: 'Login credentials', 'zh-CN': '登录凭据' },
  'register.organizationName': { en: 'Organization name', 'zh-CN': '组织名称' },
  'register.organizationType': { en: 'Organization type', 'zh-CN': '组织类型' },
  'register.orgType.sportsPerformance': { en: 'Sports performance center', 'zh-CN': '运动表现中心' },
  'register.orgType.club': { en: 'Sports club or team', 'zh-CN': '体育俱乐部或队伍' },
  'register.orgType.school': { en: 'School or university', 'zh-CN': '学校或大学' },
  'register.orgType.research': { en: 'Research institution', 'zh-CN': '研究机构' },
  'register.orgType.other': { en: 'Other', 'zh-CN': '其他' },
  'register.country': { en: 'Country / region', 'zh-CN': '国家 / 地区' },
  'register.country.cn': { en: 'China', 'zh-CN': '中国' },
  'register.country.us': { en: 'United States', 'zh-CN': '美国' },
  'register.country.jp': { en: 'Japan', 'zh-CN': '日本' },
  'register.country.kr': { en: 'South Korea', 'zh-CN': '韩国' },
  'register.country.de': { en: 'Germany', 'zh-CN': '德国' },
  'register.country.gb': { en: 'United Kingdom', 'zh-CN': '英国' },
  'register.country.au': { en: 'Australia', 'zh-CN': '澳大利亚' },
  'register.country.ca': { en: 'Canada', 'zh-CN': '加拿大' },
  'register.country.sg': { en: 'Singapore', 'zh-CN': '新加坡' },
  'register.country.other': { en: 'Other', 'zh-CN': '其他' },
  'register.website': { en: 'Website', 'zh-CN': '网站' },
  'register.websitePlaceholder': { en: 'https://example.com', 'zh-CN': 'https://example.com' },
  'register.intendedUse': { en: 'Intended use', 'zh-CN': '使用目的' },
  'register.use.teamTesting': { en: 'Team and athlete testing', 'zh-CN': '团队与运动员测试' },
  'register.use.research': { en: 'Research and lab testing', 'zh-CN': '研究与实验室测试' },
  'register.use.commercial': { en: 'Commercial performance services', 'zh-CN': '商业运动表现服务' },
  'register.use.other': { en: 'Other', 'zh-CN': '其他' },
  'register.fullName': { en: 'Full name', 'zh-CN': '姓名' },
  'register.workspaceName': { en: 'Personal workspace name', 'zh-CN': '个人工作空间名称' },
  'register.workspaceNameHelper': { en: 'You can change this later.', 'zh-CN': '之后可以修改。' },
  'register.primaryRole': { en: 'Primary role', 'zh-CN': '主要角色' },
  'register.role.coach': { en: 'Coach / sports scientist', 'zh-CN': '教练 / 运动科学家' },
  'register.role.athlete': { en: 'Athlete', 'zh-CN': '运动员' },
  'register.role.researcher': { en: 'Researcher / student', 'zh-CN': '研究人员 / 学生' },
  'register.role.other': { en: 'Other', 'zh-CN': '其他' },
  'register.email': { en: 'Email address', 'zh-CN': '邮箱地址' },
  'register.password': { en: 'Password', 'zh-CN': '密码' },
  'register.passwordHelper': { en: 'At least 8 characters.', 'zh-CN': '至少 8 个字符。' },
  'register.confirmPassword': { en: 'Confirm password', 'zh-CN': '确认密码' },
  'register.terms': {
    en: 'I agree to the Terms of Service and Privacy Policy.',
    'zh-CN': '我同意《服务条款》和《隐私政策》。',
  },
  'register.marketing': {
    en: 'Send me product updates and KWinHub news.',
    'zh-CN': '向我发送产品更新和 KWinHub 新闻。',
  },
  'register.submit': { en: 'Create account', 'zh-CN': '创建账号' },
  'register.submitting': { en: 'Creating account...', 'zh-CN': '正在创建账号...' },
  'register.footerNote': {
    en: 'You will verify your email before accessing your workspace.',
    'zh-CN': '在进入工作空间前，你需要验证邮箱。',
  },
  'register.haveAccount': { en: 'Already have an account?', 'zh-CN': '已有账号？' },
  'register.signIn': { en: 'Sign in', 'zh-CN': '登录' },
  'register.validation.required': { en: 'This field is required.', 'zh-CN': '此字段为必填项。' },
  'register.validation.email': { en: 'Enter a valid email address.', 'zh-CN': '请输入有效的邮箱地址。' },
  'register.validation.password': { en: 'Password must be at least 8 characters.', 'zh-CN': '密码至少需要 8 个字符。' },
  'register.validation.passwordMismatch': { en: 'Passwords do not match.', 'zh-CN': '两次输入的密码不一致。' },
  'register.validation.terms': {
    en: 'You must accept the Terms of Service and Privacy Policy.',
    'zh-CN': '必须同意《服务条款》和《隐私政策》。',
  },
  'register.validation.website': {
    en: 'Enter a valid URL starting with http:// or https://.',
    'zh-CN': '请输入以 http:// 或 https:// 开头的有效链接。',
  },
  'register.error': { en: 'Registration failed. Please try again.', 'zh-CN': '注册失败，请重试。' },

  // Email verification
  'verify.title': { en: 'Verify your email', 'zh-CN': '验证你的邮箱' },
  'verify.subtitle': { en: 'We sent a verification link to {email}.', 'zh-CN': '我们已向 {email} 发送了验证链接。' },
  'verify.verifying': { en: 'Verifying your email...', 'zh-CN': '正在验证邮箱...' },
  'verify.verifyingHint': { en: 'This will only take a moment.', 'zh-CN': '只需片刻。' },
  'verify.verified': { en: 'Your email is verified.', 'zh-CN': '邮箱验证成功。' },
  'verify.verifiedHint': {
    en: 'You can now sign in and start using your workspace.',
    'zh-CN': '你现在可以登录并开始使用工作空间。',
  },
  'verify.failed': { en: "We couldn't verify this email.", 'zh-CN': '无法验证此邮箱。' },
  'verify.tokenLabel': { en: 'Verification token', 'zh-CN': '验证令牌' },
  'verify.tokenPlaceholder': { en: 'Paste the verification token', 'zh-CN': '粘贴验证令牌' },
  'verify.submit': { en: 'Verify email', 'zh-CN': '验证邮箱' },
  'verify.continue': { en: 'Continue to sign in', 'zh-CN': '继续登录' },
  'verify.productionNote': {
    en: 'In production this link arrives by email.',
    'zh-CN': '在生产环境中，此链接会通过邮件发送。',
  },
  'verify.tokenRequired': { en: 'Enter the verification token.', 'zh-CN': '请输入验证令牌。' },
  'verify.error': {
    en: 'The verification token is invalid or has expired.',
    'zh-CN': '验证令牌无效或已过期。',
  },
  'verify.backToSignIn': { en: 'Back to sign in', 'zh-CN': '返回登录' },
};

interface LocaleContextValue {
  locale: Locale;
  setLocale: (locale: Locale) => void;
  t: (key: string, vars?: Record<string, string | number>) => string;
}

const LocaleContext = createContext<LocaleContextValue | null>(null);

function readInitialLocale(): Locale {
  try {
    const stored = localStorage.getItem(LOCALE_STORAGE_KEY);
    if (stored === 'en' || stored === 'zh-CN') return stored;
  } catch {
    // localStorage can be unavailable (private mode) — fall through to the browser language.
  }
  if (typeof navigator !== 'undefined' && navigator.language?.toLowerCase().startsWith('zh')) return 'zh-CN';
  return 'en';
}

function translate(locale: Locale, key: string, vars?: Record<string, string | number>): string {
  const entry = STRINGS[key];
  if (!entry) return key;
  let value = entry[locale];
  if (vars) {
    for (const [name, replacement] of Object.entries(vars)) {
      value = value.split(`{${name}}`).join(String(replacement));
    }
  }
  return value;
}

export function LocaleProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(readInitialLocale);

  const setLocale = useCallback((next: Locale) => {
    setLocaleState(next);
    try {
      localStorage.setItem(LOCALE_STORAGE_KEY, next);
    } catch {
      // Ignore storage failures — the in-memory locale still updates.
    }
  }, []);

  const t = useCallback(
    (key: string, vars?: Record<string, string | number>) => translate(locale, key, vars),
    [locale],
  );

  const value = useMemo(() => ({ locale, setLocale, t }), [locale, setLocale, t]);

  return <LocaleContext.Provider value={value}>{children}</LocaleContext.Provider>;
}

export function useI18n(): LocaleContextValue {
  const ctx = useContext(LocaleContext);
  if (!ctx) throw new Error('useI18n must be used within a LocaleProvider');
  return ctx;
}

export function LanguageSwitcher({ className = '' }: { className?: string }) {
  const { locale, setLocale } = useI18n();
  const base = 'px-2.5 py-1 text-xs font-medium rounded-full transition-colors';

  return (
    <div className={`inline-flex items-center gap-0.5 rounded-full border border-default bg-surface-panel p-0.5 ${className}`}>
      <button
        type="button"
        onClick={() => setLocale('en')}
        aria-pressed={locale === 'en'}
        className={`${base} ${locale === 'en' ? 'bg-accent text-surface-sidebar' : 'text-content-secondary hover:bg-surface-hover'}`}
      >
        English
      </button>
      <button
        type="button"
        onClick={() => setLocale('zh-CN')}
        aria-pressed={locale === 'zh-CN'}
        className={`${base} ${locale === 'zh-CN' ? 'bg-accent text-surface-sidebar' : 'text-content-secondary hover:bg-surface-hover'}`}
      >
        简体中文
      </button>
    </div>
  );
}