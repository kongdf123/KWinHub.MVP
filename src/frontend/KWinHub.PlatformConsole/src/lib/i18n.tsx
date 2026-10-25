import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';

export type Locale = 'en' | 'zh-CN';

const en = {
  // App chrome
  'app.name': 'KWinHub Platform Console',
  'app.subtitle': 'Platform administration',
  'layout.brand': 'KWinHub',
  'layout.console': 'Platform Console',
  'layout.badge': 'PLATFORM ADMIN',
  'layout.adminRole': 'Admin',
  'layout.signOut': 'Sign out',

  // Language switcher
  'lang.en': 'EN',
  'lang.zh': '中文',

  // Navigation
  'nav.dashboard': 'Dashboard',
  'nav.organizations': 'Organizations',
  'nav.users': 'Users',
  'nav.protocols': 'Protocols',
  'nav.metrics': 'Metrics',
  'nav.devices': 'Devices',
  'nav.algorithms': 'Algorithms',
  'nav.auditLogs': 'Audit log',
  'nav.group.overview': 'Overview',
  'nav.group.tenants': 'Tenants & Access',
  'nav.group.catalogs': 'Platform Catalogs',
  'nav.group.security': 'Security & Audit',

  // Common
  'common.loading': 'Loading…',
  'common.error': 'Something went wrong. Please try again.',
  'common.retry': 'Retry',
  'common.save': 'Save',
  'common.cancel': 'Cancel',
  'common.search': 'Search',
  'common.actions': 'Actions',
  'common.edit': 'Edit',
  'common.delete': 'Delete',
  'common.close': 'Close',
  'common.created': 'Created',
  'common.all': 'All',
  'common.notFound': 'Page not found',
  'common.noResults': 'No results match your filters',
  'common.confirm': 'Confirm',

  // Login
  'login.productLabel': 'Platform Management',
  'login.title': 'Platform administrator sign in',
  'login.subtitle': 'Sign in to manage the KWinHub SaaS platform and its services.',
  'login.formTitle': 'Administrator access',
  'login.accessDescription':
    'Use your authorized platform administrator account to continue.',
  'login.audienceNote':
    'This console is for KWinHub platform administrators. Tenant members and customer users should use the KWinHub application.',
  'login.brandHeadline': 'One platform. Complete oversight.',
  'login.brandDescription':
    'Manage the organizations, accounts, and shared resources that power the KWinHub ecosystem.',
  'login.feature.organizations': 'Organization and user administration',
  'login.feature.catalogs': 'Global protocols, metrics, and device catalogs',
  'login.feature.audit': 'Platform-wide administrative audit visibility',
  'login.footer': 'Secure access for platform operations',
  'login.email': 'Administrator email',
  'login.emailPlaceholder': 'Enter your administrator email',
  'login.password': 'Password',
  'login.passwordPlaceholder': 'Enter your password',
  'login.signIn': 'Sign in to PlatformConsole',
  'login.signingIn': 'Signing in…',
  'login.notAdmin': 'This account is not authorized to access the platform console.',
  'login.restricted': 'Restricted administrative access',

  // Statuses
  'status.active': 'Active',
  'status.suspended': 'Suspended',
  'status.disabled': 'Disabled',
  'status.draft': 'Draft',
  'status.published': 'Published',
  'status.archived': 'Archived',
  'status.pendingEmailVerification': 'Pending email verification',
  'status.individual': 'Individual',
  'status.standard': 'Standard',

  // Dashboard
  'dashboard.title': 'Dashboard',
  'dashboard.subtitle': 'Platform-wide activity at a glance',
  'dashboard.totalOrganizations': 'Total organizations',
  'dashboard.activeOrganizations': 'Active organizations',
  'dashboard.suspendedOrganizations': 'Suspended organizations',
  'dashboard.individualOrganizations': 'Individual organizations',
  'dashboard.totalUsers': 'Total users',
  'dashboard.pendingVerification': 'Pending verification',
  'dashboard.totalProtocols': 'Protocol definitions',
  'dashboard.totalMetrics': 'Metrics',
  'dashboard.recentOrganizations': 'Recent organizations',
  'dashboard.recentAuditEvents': 'Recent audit events',
  'dashboard.noOrganizations': 'No organizations yet',
  'dashboard.noAuditEvents': 'No audit events yet',

  // Organizations
  'orgs.title': 'Organizations',
  'orgs.subtitle': 'Every organization registered on the platform',
  'orgs.searchPlaceholder': 'Search by name or slug…',
  'orgs.allStatuses': 'All statuses',
  'orgs.allTypes': 'All types',
  'orgs.name': 'Name',
  'orgs.type': 'Type',
  'orgs.status': 'Status',
  'orgs.country': 'Country',
  'orgs.members': 'Members',
  'orgs.subjects': 'Subjects',
  'orgs.sessions': 'Sessions',
  'orgs.created': 'Created',
  'orgs.suspend': 'Suspend',
  'orgs.reactivate': 'Reactivate',
  'orgs.suspendTitle': 'Suspend organization',
  'orgs.suspendBody': 'Suspended organizations and their members lose access until reactivated.',
  'orgs.empty': 'No organizations found',
  'orgs.noResults': 'No organizations match your filters',
  'orgs.actionFailed': 'Could not update the organization status.',

  // Organization detail
  'orgDetail.title': 'Organization',
  'orgDetail.back': 'Back to organizations',
  'orgDetail.infoTitle': 'Details',
  'orgDetail.slug': 'Slug',
  'orgDetail.type': 'Type',
  'orgDetail.status': 'Status',
  'orgDetail.created': 'Created',
  'orgDetail.country': 'Country',
  'orgDetail.region': 'Region',
  'orgDetail.timezone': 'Timezone',
  'orgDetail.locale': 'Locale',
  'orgDetail.createdBy': 'Created by',
  'orgDetail.membersTitle': 'Members',
  'orgDetail.memberEmail': 'Email',
  'orgDetail.memberName': 'Display name',
  'orgDetail.memberRole': 'Role',
  'orgDetail.memberStatus': 'Status',
  'orgDetail.memberJoined': 'Joined',
  'orgDetail.noMembers': 'No members in this organization',
  'orgDetail.notFound': 'Organization not found',
  'orgDetail.tab.overview': 'Overview',
  'orgDetail.tab.members': 'Members',

  // Users
  'users.title': 'Users',
  'users.subtitle': 'All platform user accounts',
  'users.searchPlaceholder': 'Search by email or name…',
  'users.allStatuses': 'All statuses',
  'users.email': 'Email',
  'users.displayName': 'Display name',
  'users.status': 'Status',
  'users.platformRole': 'Platform role',
  'users.emailVerified': 'Email verified',
  'users.organizations': 'Organizations',
  'users.lastLogin': 'Last login',
  'users.created': 'Created',
  'users.suspend': 'Suspend',
  'users.activate': 'Activate',
  'users.grantAdmin': 'Grant platform admin',
  'users.revokeAdmin': 'Revoke platform admin',
  'users.verified': 'Verified',
  'users.unverified': 'Unverified',
  'users.never': 'Never',
  'users.empty': 'No users found',
  'users.noResults': 'No users match your filters',
  'users.confirmTitle': 'Confirm user change',
  'users.confirmSuspend': 'Suspend this user account?',
  'users.confirmActivate': 'Activate this user account?',
  'users.confirmGrant': 'Grant platform administrator access to this user?',
  'users.confirmRevoke': 'Revoke platform administrator access from this user?',
  'users.platformAdmin': 'Platform admin',
  'users.actionFailed': 'Could not update the user.',

  // Catalogs — shared
  'catalog.searchPlaceholder': 'Search by code or name…',
  'catalog.new': 'New',
  'catalog.createTitle': 'Create entry',
  'catalog.editTitle': 'Edit entry',
  'catalog.code': 'Code',
  'catalog.name': 'Name',
  'catalog.version': 'Version',
  'catalog.category': 'Category',
  'catalog.sport': 'Sport',
  'catalog.description': 'Description',
  'catalog.unit': 'Unit',
  'catalog.status': 'Status',
  'catalog.created': 'Created',
  'catalog.actions': 'Actions',
  'catalog.publish': 'Publish',
  'catalog.archive': 'Archive',
  'catalog.deleteTitle': 'Delete entry',
  'catalog.deleteBody': 'This action cannot be undone.',
  'catalog.required': 'Please fill in all required fields.',
  'catalog.empty': 'No entries yet',
  'catalog.noResults': 'No entries match your search',
  'catalog.deviceManufacturer': 'Manufacturer',
  'catalog.deviceModel': 'Model',
  'catalog.deviceType': 'Device type',
  'catalog.notes': 'Notes',
  'catalog.supportedMetrics': 'Supported metric codes',
  'catalog.codePlaceholder': 'e.g. CMJ_BASIC',
  'catalog.namePlaceholder': 'Display name',
  'catalog.versionPlaceholder': 'e.g. 1.0.0',
  'catalog.categoryPlaceholder': 'e.g. Jump testing',
  'catalog.sportPlaceholder': 'e.g. Basketball',
  'catalog.unitPlaceholder': 'e.g. cm',
  'catalog.descriptionPlaceholder': 'Optional description',
  'catalog.manufacturerPlaceholder': 'e.g. Kistler',
  'catalog.modelPlaceholder': 'e.g. Force Plate 9287',
  'catalog.deviceTypePlaceholder': 'e.g. ForcePlate',
  'catalog.notesPlaceholder': 'Optional notes',
  'catalog.supportedMetricsPlaceholder': 'Comma-separated metric codes',
  'catalog.saving': 'Saving…',
  'catalog.actionFailed': 'Could not complete the request.',

  // Protocol catalog
  'protocols.title': 'Protocols',
  'protocols.subtitle': 'Global protocol definitions available to all organizations',

  // Metric catalog
  'metrics.title': 'Metrics',
  'metrics.subtitle': 'Global metric definitions and their units',

  // Device catalog
  'devices.title': 'Devices',
  'devices.subtitle': 'Supported device models',

  // Algorithm catalog
  'algorithms.title': 'Algorithms',
  'algorithms.subtitle': 'Processing algorithm versions',

  // Audit log
  'audit.title': 'Audit log',
  'audit.subtitle': 'Platform-wide administrative activity',
  'audit.time': 'Time',
  'audit.actor': 'Actor',
  'audit.action': 'Action',
  'audit.entityType': 'Entity type',
  'audit.entityId': 'Entity id',
  'audit.summary': 'Summary',
  'audit.allEntityTypes': 'All entity types',
  'audit.empty': 'No audit events recorded',
  'audit.system': 'System',
  'audit.entity.organization': 'Organization',
  'audit.entity.user': 'User',
  'audit.entity.globalProtocol': 'Global protocol',
  'audit.entity.globalMetric': 'Global metric',
  'audit.entity.deviceModel': 'Device model',
  'audit.entity.algorithmVersion': 'Algorithm version',
  'audit.export': 'Export CSV',
  'audit.exportNone': 'No records to export',
  'audit.search': 'Search actor, action, or summary…',

  // Dashboard extras
  'dashboard.needsAttention': 'Needs attention',
  'dashboard.suspendedOrgs': 'suspended organizations',
  'dashboard.pendingUsers': 'users pending verification',
  'dashboard.viewFiltered': 'View filtered list',
  'dashboard.securityEvents': 'Recent security-sensitive actions',
} as const;

export type TranslationKey = keyof typeof en;

type Dictionary = Record<TranslationKey, string>;

const zhCN: Dictionary = {
  // App chrome
  'app.name': 'KWinHub 平台控制台',
  'app.subtitle': '平台管理',
  'layout.brand': 'KWinHub',
  'layout.console': '平台控制台',
  'layout.badge': '平台管理员',
  'layout.adminRole': '管理员',
  'layout.signOut': '退出登录',

  // Language switcher
  'lang.en': 'EN',
  'lang.zh': '中文',

  // Navigation
  'nav.dashboard': '仪表盘',
  'nav.organizations': '组织',
  'nav.users': '用户',
  'nav.protocols': '方案',
  'nav.metrics': '指标',
  'nav.devices': '设备',
  'nav.algorithms': '算法',
  'nav.auditLogs': '审计日志',
  'nav.group.overview': '概览',
  'nav.group.tenants': '租户与访问',
  'nav.group.catalogs': '平台目录',
  'nav.group.security': '安全与审计',

  // Common
  'common.loading': '加载中…',
  'common.error': '操作失败，请重试。',
  'common.retry': '重试',
  'common.save': '保存',
  'common.cancel': '取消',
  'common.search': '搜索',
  'common.actions': '操作',
  'common.edit': '编辑',
  'common.delete': '删除',
  'common.close': '关闭',
  'common.created': '创建时间',
  'common.all': '全部',
  'common.notFound': '页面不存在',
  'common.noResults': '没有符合条件的结果',
  'common.confirm': '确认',

  // Login
  'login.productLabel': '平台管理',
  'login.title': '平台管理员登录',
  'login.subtitle': '登录以管理 KWinHub SaaS 平台及其服务。',
  'login.formTitle': '管理员身份验证',
  'login.accessDescription': '请使用已获授权的平台管理员账号继续。',
  'login.audienceNote':
    '本控制台仅供 KWinHub 平台管理员使用。租户成员及客户用户请通过 KWinHub 应用访问。',
  'login.brandHeadline': '统一平台，全局掌控。',
  'login.brandDescription': '集中管理 KWinHub 生态中的组织、账号和共享资源。',
  'login.feature.organizations': '组织与用户管理',
  'login.feature.catalogs': '全局方案、指标及设备目录管理',
  'login.feature.audit': '平台级管理操作审计',
  'login.footer': '平台运营安全访问',
  'login.email': '管理员邮箱',
  'login.emailPlaceholder': '请输入管理员邮箱',
  'login.password': '密码',
  'login.passwordPlaceholder': '请输入密码',
  'login.signIn': '登录平台控制台',
  'login.signingIn': '正在登录…',
  'login.notAdmin': '此账号无权访问平台控制台。',
  'login.restricted': '仅限授权管理员访问',

  // Statuses
  'status.active': '启用',
  'status.suspended': '已暂停',
  'status.disabled': '已禁用',
  'status.draft': '草稿',
  'status.published': '已发布',
  'status.archived': '已归档',
  'status.pendingEmailVerification': '邮箱待验证',
  'status.individual': '个人',
  'status.standard': '标准',

  // Dashboard
  'dashboard.title': '仪表盘',
  'dashboard.subtitle': '平台整体活动概览',
  'dashboard.totalOrganizations': '组织总数',
  'dashboard.activeOrganizations': '活跃组织',
  'dashboard.suspendedOrganizations': '已暂停组织',
  'dashboard.individualOrganizations': '个人组织',
  'dashboard.totalUsers': '用户总数',
  'dashboard.pendingVerification': '待验证用户',
  'dashboard.totalProtocols': '方案定义',
  'dashboard.totalMetrics': '指标',
  'dashboard.recentOrganizations': '最近组织',
  'dashboard.recentAuditEvents': '最近审计事件',
  'dashboard.noOrganizations': '暂无组织',
  'dashboard.noAuditEvents': '暂无审计事件',

  // Organizations
  'orgs.title': '组织',
  'orgs.subtitle': '平台上注册的所有组织',
  'orgs.searchPlaceholder': '按名称或标识搜索…',
  'orgs.allStatuses': '全部状态',
  'orgs.allTypes': '全部类型',
  'orgs.name': '名称',
  'orgs.type': '类型',
  'orgs.status': '状态',
  'orgs.country': '国家/地区',
  'orgs.members': '成员',
  'orgs.subjects': '受试者',
  'orgs.sessions': '测试记录',
  'orgs.created': '创建时间',
  'orgs.suspend': '暂停',
  'orgs.reactivate': '恢复',
  'orgs.suspendTitle': '暂停组织',
  'orgs.suspendBody': '暂停后，该组织及其成员将失去访问权限，直到恢复。',
  'orgs.empty': '未找到组织',
  'orgs.noResults': '没有符合条件的组织',
  'orgs.actionFailed': '无法更新组织状态。',

  // Organization detail
  'orgDetail.title': '组织详情',
  'orgDetail.back': '返回组织列表',
  'orgDetail.infoTitle': '详细信息',
  'orgDetail.slug': '标识',
  'orgDetail.type': '类型',
  'orgDetail.status': '状态',
  'orgDetail.created': '创建时间',
  'orgDetail.country': '国家/地区',
  'orgDetail.region': '区域',
  'orgDetail.timezone': '时区',
  'orgDetail.locale': '语言',
  'orgDetail.createdBy': '创建人',
  'orgDetail.membersTitle': '成员',
  'orgDetail.memberEmail': '邮箱',
  'orgDetail.memberName': '显示名称',
  'orgDetail.memberRole': '角色',
  'orgDetail.memberStatus': '状态',
  'orgDetail.memberJoined': '加入时间',
  'orgDetail.noMembers': '该组织暂无成员',
  'orgDetail.notFound': '未找到组织',
  'orgDetail.tab.overview': '概览',
  'orgDetail.tab.members': '成员',

  // Users
  'users.title': '用户',
  'users.subtitle': '平台所有用户账号',
  'users.searchPlaceholder': '按邮箱或名称搜索…',
  'users.allStatuses': '全部状态',
  'users.email': '邮箱',
  'users.displayName': '显示名称',
  'users.status': '状态',
  'users.platformRole': '平台角色',
  'users.emailVerified': '邮箱验证',
  'users.organizations': '组织数',
  'users.lastLogin': '最近登录',
  'users.created': '创建时间',
  'users.suspend': '暂停',
  'users.activate': '启用',
  'users.grantAdmin': '授予平台管理员',
  'users.revokeAdmin': '撤销平台管理员',
  'users.verified': '已验证',
  'users.unverified': '未验证',
  'users.never': '从未',
  'users.empty': '未找到用户',
  'users.noResults': '没有符合条件的用户',
  'users.confirmTitle': '确认用户变更',
  'users.confirmSuspend': '确定暂停该用户账号吗？',
  'users.confirmActivate': '确定启用该用户账号吗？',
  'users.confirmGrant': '确定授予该用户平台管理员权限吗？',
  'users.confirmRevoke': '确定撤销该用户的平台管理员权限吗？',
  'users.platformAdmin': '平台管理员',
  'users.actionFailed': '无法更新用户。',

  // Catalogs — shared
  'catalog.searchPlaceholder': '按编码或名称搜索…',
  'catalog.new': '新建',
  'catalog.createTitle': '新建条目',
  'catalog.editTitle': '编辑条目',
  'catalog.code': '编码',
  'catalog.name': '名称',
  'catalog.version': '版本',
  'catalog.category': '分类',
  'catalog.sport': '项目',
  'catalog.description': '描述',
  'catalog.unit': '单位',
  'catalog.status': '状态',
  'catalog.created': '创建时间',
  'catalog.actions': '操作',
  'catalog.publish': '发布',
  'catalog.archive': '归档',
  'catalog.deleteTitle': '删除条目',
  'catalog.deleteBody': '此操作无法撤销。',
  'catalog.required': '请填写所有必填字段。',
  'catalog.empty': '暂无条目',
  'catalog.noResults': '没有匹配的条目',
  'catalog.deviceManufacturer': '厂商',
  'catalog.deviceModel': '型号',
  'catalog.deviceType': '设备类型',
  'catalog.notes': '备注',
  'catalog.supportedMetrics': '支持的指标编码',
  'catalog.codePlaceholder': '例如 CMJ_BASIC',
  'catalog.namePlaceholder': '显示名称',
  'catalog.versionPlaceholder': '例如 1.0.0',
  'catalog.categoryPlaceholder': '例如 跳跃测试',
  'catalog.sportPlaceholder': '例如 篮球',
  'catalog.unitPlaceholder': '例如 cm',
  'catalog.descriptionPlaceholder': '可选描述',
  'catalog.manufacturerPlaceholder': '例如 Kistler',
  'catalog.modelPlaceholder': '例如 Force Plate 9287',
  'catalog.deviceTypePlaceholder': '例如 ForcePlate',
  'catalog.notesPlaceholder': '可选备注',
  'catalog.supportedMetricsPlaceholder': '以逗号分隔的指标编码',
  'catalog.saving': '保存中…',
  'catalog.actionFailed': '无法完成请求。',

  // Protocol catalog
  'protocols.title': '方案',
  'protocols.subtitle': '所有组织可用的全局方案定义',

  // Metric catalog
  'metrics.title': '指标',
  'metrics.subtitle': '全局指标定义及其单位',

  // Device catalog
  'devices.title': '设备',
  'devices.subtitle': '支持的设备型号',

  // Algorithm catalog
  'algorithms.title': '算法',
  'algorithms.subtitle': '处理算法版本',

  // Audit log
  'audit.title': '审计日志',
  'audit.subtitle': '平台管理操作记录',
  'audit.time': '时间',
  'audit.actor': '操作人',
  'audit.action': '操作',
  'audit.entityType': '实体类型',
  'audit.entityId': '实体 ID',
  'audit.summary': '摘要',
  'audit.allEntityTypes': '全部实体类型',
  'audit.empty': '暂无审计事件',
  'audit.system': '系统',
  'audit.entity.organization': '组织',
  'audit.entity.user': '用户',
  'audit.entity.globalProtocol': '全局方案',
  'audit.entity.globalMetric': '全局指标',
  'audit.entity.deviceModel': '设备型号',
  'audit.entity.algorithmVersion': '算法版本',
  'audit.export': '导出 CSV',
  'audit.exportNone': '没有可导出的记录',
  'audit.search': '搜索操作人、操作或摘要…',

  // Dashboard extras
  'dashboard.needsAttention': '需要关注',
  'dashboard.suspendedOrgs': '个已暂停组织',
  'dashboard.pendingUsers': '个待验证用户',
  'dashboard.viewFiltered': '查看筛选列表',
  'dashboard.securityEvents': '最近安全相关操作',
};

const DICTIONARIES: Record<Locale, Dictionary> = {
  en,
  'zh-CN': zhCN,
};

const STORAGE_KEY = 'kwinhub.console.locale';

interface I18nContextValue {
  locale: Locale;
  setLocale: (locale: Locale) => void;
  t: (key: TranslationKey) => string;
}

const I18nContext = createContext<I18nContextValue | null>(null);

function readStoredLocale(): Locale {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'en' || stored === 'zh-CN') return stored;
  } catch {
    // localStorage may be unavailable — fall back to English
  }
  return 'en';
}

export function LocaleProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(readStoredLocale);

  const setLocale = useCallback((next: Locale) => {
    setLocaleState(next);
    try {
      localStorage.setItem(STORAGE_KEY, next);
    } catch {
      // ignore storage failures
    }
  }, []);

  const t = useCallback((key: TranslationKey) => DICTIONARIES[locale][key] ?? key, [locale]);

  useEffect(() => {
    document.documentElement.lang = locale;
  }, [locale]);

  const value = useMemo<I18nContextValue>(
    () => ({ locale, setLocale, t }),
    [locale, setLocale, t]
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n(): I18nContextValue {
  const context = useContext(I18nContext);
  if (!context) throw new Error('useI18n must be used inside a LocaleProvider');
  return context;
}

export function LanguageSwitcher({ dark = false }: { dark?: boolean }) {
  const { locale, setLocale, t } = useI18n();

  const baseClass = dark
    ? 'border-white/10 text-ink-300'
    : 'border-default text-content-secondary';
  const activeClass = dark
    ? 'bg-teal-500/20 text-teal-300'
    : 'bg-accent-soft text-accent';
  const idleClass = dark ? 'hover:text-white' : 'hover:text-content-primary';

  return (
    <div className={`inline-flex overflow-hidden rounded-md border text-xs font-medium ${baseClass}`}>
      <button
        type="button"
        onClick={() => setLocale('en')}
        className={`px-2.5 py-1 transition-colors ${locale === 'en' ? activeClass : idleClass}`}
      >
        {t('lang.en')}
      </button>
      <button
        type="button"
        onClick={() => setLocale('zh-CN')}
        className={`border-l px-2.5 py-1 transition-colors ${
          dark ? 'border-white/10' : 'border-default'
        } ${locale === 'zh-CN' ? activeClass : idleClass}`}
      >
        {t('lang.zh')}
      </button>
    </div>
  );
}