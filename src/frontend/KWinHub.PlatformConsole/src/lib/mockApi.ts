import { ApiError } from './api';
import type {
  AlgorithmVersionResponse,
  AuditLogResponse,
  AuthProfileResponse,
  DeviceModelResponse,
  FeatureOverrideResponse,
  GlobalMetricResponse,
  GlobalProtocolResponse,
  LoginResponse,
  PlatformDashboardResponse,
  PlatformOrganizationDetailResponse,
  PlatformOrganizationMemberResponse,
  PlatformOrganizationResponse,
  PlatformSubscriptionResponse,
  PlanResponse,
  PlatformUserResponse,
  QuotaOverrideResponse,
  SubscriptionAuditEventResponse,
} from './api';

// In-memory mock of the KWinHub platform API, used for prototype demos while
// the real backend (src/backend/KWinHub.Api) is not running. State lives for
// the duration of the browser session and resets on reload.

const ADMIN_EMAIL = 'admin@kwinhub.com';
const ADMIN_PASSWORD = 'Admin@12345';

const now = Date.now();
const iso = (daysAgo: number, hoursAgo = 0) =>
  new Date(now - daysAgo * 86_400_000 - hoursAgo * 3_600_000).toISOString();

const delay = () => new Promise<void>((resolve) => setTimeout(resolve, 120 + Math.random() * 180));

let idSeq = 100;
function nextId(prefix: string): string {
  idSeq += 1;
  return `${prefix}-${String(idSeq).padStart(4, '0')}`;
}

function byCreatedDesc<T extends { createdAt: string }>(items: T[]): T[] {
  return [...items].sort((a, b) => b.createdAt.localeCompare(a.createdAt));
}

// --- Organizations ---------------------------------------------------------

let organizations: PlatformOrganizationResponse[] = [
  {
    id: 'org-001',
    name: 'Velocity Sports Lab',
    slug: 'velocity-sports-lab',
    type: 'Standard',
    status: 'Active',
    countryCode: 'US',
    createdAt: iso(412),
    memberCount: 24,
    subjectCount: 18,
    sessionCount: 412,
    resultCount: 3870,
  },
  {
    id: 'org-002',
    name: 'Apex Performance Institute',
    slug: 'apex-performance',
    type: 'Standard',
    status: 'Active',
    countryCode: 'GB',
    createdAt: iso(365),
    memberCount: 16,
    subjectCount: 11,
    sessionCount: 208,
    resultCount: 1940,
  },
  {
    id: 'org-003',
    name: 'Nordic Motion Center',
    slug: 'nordic-motion',
    type: 'Standard',
    status: 'Suspended',
    countryCode: 'NO',
    createdAt: iso(320),
    memberCount: 9,
    subjectCount: 6,
    sessionCount: 74,
    resultCount: 610,
  },
  {
    id: 'org-004',
    name: 'PeakForm Analytics',
    slug: 'peakform-analytics',
    type: 'Standard',
    status: 'Active',
    countryCode: 'DE',
    createdAt: iso(268),
    memberCount: 12,
    subjectCount: 9,
    sessionCount: 156,
    resultCount: 1320,
  },
  {
    id: 'org-005',
    name: 'Coach Chen Individual',
    slug: 'coach-chen',
    type: 'Individual',
    status: 'Active',
    countryCode: 'CN',
    createdAt: iso(214),
    memberCount: 1,
    subjectCount: 3,
    sessionCount: 28,
    resultCount: 190,
  },
  {
    id: 'org-006',
    name: 'Summit Athletics',
    slug: 'summit-athletics',
    type: 'Standard',
    status: 'Active',
    countryCode: 'CA',
    createdAt: iso(160),
    memberCount: 20,
    subjectCount: 14,
    sessionCount: 300,
    resultCount: 2450,
  },
  {
    id: 'org-007',
    name: 'Rio Motion Lab',
    slug: 'rio-motion-lab',
    type: 'Standard',
    status: 'Suspended',
    countryCode: 'BR',
    createdAt: iso(96),
    memberCount: 7,
    subjectCount: 4,
    sessionCount: 41,
    resultCount: 360,
  },
  {
    id: 'org-008',
    name: 'Kyoto Sports Science',
    slug: 'kyoto-sports-science',
    type: 'Standard',
    status: 'Active',
    countryCode: 'JP',
    createdAt: iso(42),
    memberCount: 5,
    subjectCount: 2,
    sessionCount: 18,
    resultCount: 120,
  },
];

const organizationMeta: Record<
  string,
  { region: string | null; timezone: string | null; locale: string | null; createdByUserId: string | null }
> = {
  'org-001': { region: 'North America', timezone: 'America/New_York', locale: 'en-US', createdByUserId: 'usr-002' },
  'org-002': { region: 'Europe', timezone: 'Europe/London', locale: 'en-GB', createdByUserId: 'usr-004' },
  'org-003': { region: 'Europe', timezone: 'Europe/Oslo', locale: 'nb-NO', createdByUserId: 'usr-005' },
  'org-004': { region: 'Europe', timezone: 'Europe/Berlin', locale: 'de-DE', createdByUserId: 'usr-006' },
  'org-005': { region: 'Asia', timezone: 'Asia/Shanghai', locale: 'zh-CN', createdByUserId: 'usr-007' },
  'org-006': { region: 'North America', timezone: 'America/Toronto', locale: 'en-CA', createdByUserId: 'usr-008' },
  'org-007': { region: 'South America', timezone: 'America/Sao_Paulo', locale: 'pt-BR', createdByUserId: 'usr-009' },
  'org-008': { region: 'Asia', timezone: 'Asia/Tokyo', locale: 'ja-JP', createdByUserId: 'usr-010' },
};

const organizationMembers: Record<string, PlatformOrganizationMemberResponse[]> = {
  'org-001': [
    { userId: 'usr-002', email: 'sarah.chen@velocitylab.com', displayName: 'Sarah Chen', role: 'Owner', status: 'Active', joinedAt: iso(405) },
    { userId: 'usr-011', email: 'marcus.hale@velocitylab.com', displayName: 'Marcus Hale', role: 'Admin', status: 'Active', joinedAt: iso(330) },
    { userId: 'usr-012', email: 'lena.ortiz@velocitylab.com', displayName: 'Lena Ortiz', role: 'Member', status: 'Active', joinedAt: iso(200) },
  ],
  'org-002': [
    { userId: 'usr-004', email: 'oliver.grant@apexpi.com', displayName: 'Oliver Grant', role: 'Owner', status: 'Active', joinedAt: iso(360) },
    { userId: 'usr-013', email: 'priya.nair@apexpi.com', displayName: 'Priya Nair', role: 'Member', status: 'Active', joinedAt: iso(120) },
  ],
  'org-003': [
    { userId: 'usr-005', email: 'ingrid.dahl@nordicmotion.no', displayName: 'Ingrid Dahl', role: 'Owner', status: 'Active', joinedAt: iso(315) },
  ],
  'org-004': [
    { userId: 'usr-006', email: 'jonas.weber@peakform.de', displayName: 'Jonas Weber', role: 'Owner', status: 'Active', joinedAt: iso(260) },
    { userId: 'usr-014', email: 'anna.becker@peakform.de', displayName: 'Anna Becker', role: 'Member', status: 'PendingEmailVerification', joinedAt: iso(30) },
  ],
  'org-005': [
    { userId: 'usr-007', email: 'chen.wei@coachchen.cn', displayName: 'Chen Wei', role: 'Owner', status: 'Active', joinedAt: iso(210) },
  ],
  'org-006': [
    { userId: 'usr-008', email: 'daniel.roy@summitathletics.ca', displayName: 'Daniel Roy', role: 'Owner', status: 'Active', joinedAt: iso(155) },
    { userId: 'usr-015', email: 'emily.foster@summitathletics.ca', displayName: 'Emily Foster', role: 'Admin', status: 'Active', joinedAt: iso(90) },
  ],
  'org-007': [
    { userId: 'usr-009', email: 'rafael.souza@riomotion.br', displayName: 'Rafael Souza', role: 'Owner', status: 'Suspended', joinedAt: iso(95) },
  ],
  'org-008': [
    { userId: 'usr-010', email: 'kenji.tanaka@kyotosports.jp', displayName: 'Kenji Tanaka', role: 'Owner', status: 'Active', joinedAt: iso(40) },
  ],
};

// --- Users -----------------------------------------------------------------

let users: PlatformUserResponse[] = [
  {
    id: 'usr-001',
    email: ADMIN_EMAIL,
    displayName: 'Platform Admin',
    status: 'Active',
    platformRole: 'PlatformAdmin',
    emailVerified: true,
    lastLoginAt: iso(0, 1),
    createdAt: iso(430),
    organizationCount: 0,
  },
  {
    id: 'usr-002',
    email: 'sarah.chen@velocitylab.com',
    displayName: 'Sarah Chen',
    status: 'Active',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(0, 6),
    createdAt: iso(410),
    organizationCount: 2,
  },
  {
    id: 'usr-003',
    email: 'david.okafor@velocitylab.com',
    displayName: 'David Okafor',
    status: 'Suspended',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(38),
    createdAt: iso(390),
    organizationCount: 1,
  },
  {
    id: 'usr-004',
    email: 'oliver.grant@apexpi.com',
    displayName: 'Oliver Grant',
    status: 'Active',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(1, 3),
    createdAt: iso(365),
    organizationCount: 1,
  },
  {
    id: 'usr-005',
    email: 'ingrid.dahl@nordicmotion.no',
    displayName: 'Ingrid Dahl',
    status: 'Active',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(12),
    createdAt: iso(320),
    organizationCount: 1,
  },
  {
    id: 'usr-006',
    email: 'jonas.weber@peakform.de',
    displayName: 'Jonas Weber',
    status: 'Active',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(2, 8),
    createdAt: iso(268),
    organizationCount: 1,
  },
  {
    id: 'usr-007',
    email: 'chen.wei@coachchen.cn',
    displayName: 'Chen Wei',
    status: 'Active',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(4),
    createdAt: iso(214),
    organizationCount: 1,
  },
  {
    id: 'usr-008',
    email: 'daniel.roy@summitathletics.ca',
    displayName: 'Daniel Roy',
    status: 'Active',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(3, 2),
    createdAt: iso(160),
    organizationCount: 1,
  },
  {
    id: 'usr-009',
    email: 'rafael.souza@riomotion.br',
    displayName: 'Rafael Souza',
    status: 'Disabled',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(75),
    createdAt: iso(96),
    organizationCount: 1,
  },
  {
    id: 'usr-010',
    email: 'kenji.tanaka@kyotosports.jp',
    displayName: 'Kenji Tanaka',
    status: 'Active',
    platformRole: null,
    emailVerified: true,
    lastLoginAt: iso(1, 11),
    createdAt: iso(42),
    organizationCount: 1,
  },
  {
    id: 'usr-014',
    email: 'anna.becker@peakform.de',
    displayName: 'Anna Becker',
    status: 'PendingEmailVerification',
    platformRole: null,
    emailVerified: false,
    lastLoginAt: null,
    createdAt: iso(30),
    organizationCount: 1,
  },
  {
    id: 'usr-016',
    email: 'lucas.martin@velocitylab.com',
    displayName: 'Lucas Martin',
    status: 'PendingEmailVerification',
    platformRole: null,
    emailVerified: false,
    lastLoginAt: null,
    createdAt: iso(9),
    organizationCount: 1,
  },
];

// --- Catalogs --------------------------------------------------------------

let protocols: GlobalProtocolResponse[] = [
  {
    id: 'proto-001',
    code: 'GOLF_SWING_V2',
    name: 'Golf Swing Analysis',
    version: '2.1.0',
    category: 'Biomechanics',
    sport: 'Golf',
    description: 'Full-body kinematic capture for golf swing sequencing and power transfer.',
    status: 'Published',
    createdAt: iso(324),
  },
  {
    id: 'proto-002',
    code: 'BASEBALL_PITCH_V1',
    name: 'Baseball Pitching Mechanics',
    version: '1.4.2',
    category: 'Throwing',
    sport: 'Baseball',
    description: 'Pitching delivery analysis including arm path and lower-body drive.',
    status: 'Published',
    createdAt: iso(296),
  },
  {
    id: 'proto-003',
    code: 'TENNIS_SERVE_V3',
    name: 'Tennis Serve Kinematics',
    version: '3.0.0',
    category: 'Biomechanics',
    sport: 'Tennis',
    description: 'Serve toss, racket acceleration and landing mechanics.',
    status: 'Published',
    createdAt: iso(240),
  },
  {
    id: 'proto-004',
    code: 'RUN_GAIT_V1',
    name: 'Running Gait Assessment',
    version: '1.2.0',
    category: 'Gait',
    sport: 'Running',
    description: 'Stride symmetry, cadence and ground contact timing.',
    status: 'Draft',
    createdAt: iso(88),
  },
  {
    id: 'proto-005',
    code: 'WEIGHTLIFT_SQUAT_V2',
    name: 'Squat Movement Screen',
    version: '2.0.1',
    category: 'Strength',
    sport: 'Weightlifting',
    description: 'Depth, knee tracking and trunk stability during loaded squat.',
    status: 'Published',
    createdAt: iso(150),
  },
  {
    id: 'proto-006',
    code: 'LEGACY_JUMP_V1',
    name: 'Vertical Jump (Legacy)',
    version: '1.0.0',
    category: 'Power',
    sport: 'Track & Field',
    description: 'Superseded by JUMP_ANALYSIS_V2; retained for historical sessions.',
    status: 'Archived',
    createdAt: iso(520),
  },
  {
    id: 'proto-007',
    code: 'CMJ_BILATERAL',
    name: 'Countermovement Jump (Bilateral)',
    version: '2.0',
    category: 'Power',
    sport: 'General Athletics',
    description: 'Bilateral countermovement jump on force plate(s); gold-standard power assessment.',
    status: 'Published',
    createdAt: iso(180),
  },
  {
    id: 'proto-008',
    code: 'SPRINT_30M',
    name: '30m Sprint',
    version: '1.0',
    category: 'Sprint',
    sport: 'Track & Field',
    description: '30m sprint with multi-device capture: radar gun + starting-block force plates.',
    status: 'Published',
    createdAt: iso(160),
  },
  {
    id: 'proto-009',
    code: 'IMTP_STANDARD',
    name: 'Isometric Mid-Thigh Pull (Standard)',
    version: '1.1',
    category: 'Strength',
    sport: 'General',
    description: 'Maximal isometric mid-thigh pull for peak force and rate of force development.',
    status: 'Published',
    createdAt: iso(120),
  },
];

let metrics: GlobalMetricResponse[] = [
  {
    id: 'metric-001',
    code: 'HIP_ROTATION_VELOCITY',
    name: 'Hip Rotation Velocity',
    unit: 'deg/s',
    category: 'Kinematics',
    description: 'Peak angular velocity of pelvis rotation during the swing.',
    status: 'Published',
    createdAt: iso(310),
  },
  {
    id: 'metric-002',
    code: 'SWING_TEMPO_RATIO',
    name: 'Swing Tempo Ratio',
    unit: 'ratio',
    category: 'Timing',
    description: 'Backswing-to-downswing duration ratio.',
    status: 'Published',
    createdAt: iso(300),
  },
  {
    id: 'metric-003',
    code: 'GROUND_REACTION_FORCE',
    name: 'Ground Reaction Force',
    unit: 'N',
    category: 'Force',
    description: 'Peak vertical ground reaction force at push-off.',
    status: 'Published',
    createdAt: iso(280),
  },
  {
    id: 'metric-004',
    code: 'STRIDE_SYMMETRY_INDEX',
    name: 'Stride Symmetry Index',
    unit: '%',
    category: 'Gait',
    description: 'Left/right stride asymmetry percentage.',
    status: 'Published',
    createdAt: iso(210),
  },
  {
    id: 'metric-005',
    code: 'ARM_SPEED_MAX',
    name: 'Maximum Arm Speed',
    unit: 'deg/s',
    category: 'Kinematics',
    description: 'Peak elbow extension velocity during throwing.',
    status: 'Draft',
    createdAt: iso(64),
  },
  {
    id: 'metric-006',
    code: 'CMJ_JUMP_HEIGHT_FLIGHT_TIME',
    name: 'Jump Height (Flight Time)',
    unit: 'm',
    category: 'Power',
    description: 'Jump height derived from flight time using impulse-momentum relationship.',
    status: 'Published',
    createdAt: iso(178),
  },
  {
    id: 'metric-007',
    code: 'CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM',
    name: 'Jump Height (Impulse-Momentum)',
    unit: 'm',
    category: 'Power',
    description: 'Jump height derived from net vertical impulse during eccentric + concentric phase.',
    status: 'Published',
    createdAt: iso(178),
  },
  {
    id: 'metric-008',
    code: 'CMJ_PEAK_FORCE',
    name: 'Peak Vertical Ground Reaction Force',
    unit: 'N',
    category: 'Force',
    description: 'Maximum vertical force measured during the concentric push-off phase.',
    status: 'Published',
    createdAt: iso(178),
  },
  {
    id: 'metric-009',
    code: 'CMJ_RSI_MOD',
    name: 'RSI (Modified)',
    unit: 'm/s',
    category: 'Rate',
    description: 'Jump height divided by eccentric + concentric phase time.',
    status: 'Published',
    createdAt: iso(178),
  },
  {
    id: 'metric-010',
    code: 'CMJ_ASYMMETRY_INDEX',
    name: 'Asymmetry Index',
    unit: '%',
    category: 'Symmetry',
    description: 'Percentage difference between left and right peak forces.',
    status: 'Published',
    createdAt: iso(178),
  },
  {
    id: 'metric-011',
    code: 'CMJ_TAKEOFF_VELOCITY',
    name: 'Takeoff Velocity',
    unit: 'm/s',
    category: 'Kinematics',
    description: 'Vertical velocity at takeoff computed from net impulse and body mass.',
    status: 'Published',
    createdAt: iso(178),
  },
  {
    id: 'metric-012',
    code: 'SPRINT_REACTION_TIME_BLOCK',
    name: 'Block Reaction Time',
    unit: 'ms',
    category: 'Timing',
    description: 'Time from signal to rear foot force exceeding threshold on starting block.',
    status: 'Published',
    createdAt: iso(155),
  },
  {
    id: 'metric-013',
    code: 'SPRINT_PEAK_VELOCITY',
    name: 'Peak Velocity',
    unit: 'm/s',
    category: 'Kinematics',
    description: 'Maximum horizontal velocity achieved during 30m sprint.',
    status: 'Published',
    createdAt: iso(155),
  },
  {
    id: 'metric-014',
    code: 'SPRINT_GROUND_CONTACT_TIME',
    name: 'Ground Contact Time',
    unit: 'ms',
    category: 'Contact',
    description: 'Average foot-ground contact time across all steps.',
    status: 'Published',
    createdAt: iso(155),
  },
  {
    id: 'metric-015',
    code: 'SPRINT_PEAK_GROUND_REACTION_FORCE',
    name: 'Peak GRF (Single Step)',
    unit: 'N',
    category: 'Force',
    description: 'Maximum force recorded on front block plate during push-off.',
    status: 'Published',
    createdAt: iso(155),
  },
  {
    id: 'metric-016',
    code: 'SPRINT_POWER_TRANSFER',
    name: 'Power Transfer Index',
    unit: 'N/s',
    category: 'Rate',
    description: 'Rate of force development on rear block as proxy for block efficiency.',
    status: 'Published',
    createdAt: iso(155),
  },
];

let devices: DeviceModelResponse[] = [
  {
    id: 'device-001',
    manufacturer: 'TrackMan',
    model: 'TrackMan 4',
    deviceType: 'Radar',
    notes: 'Dual radar; club and ball tracking for golf.',
    status: 'Published',
    createdAt: iso(288),
  },
  {
    id: 'device-002',
    manufacturer: 'K-Motion',
    model: 'K-Force 3D',
    deviceType: 'Wearable',
    notes: 'Six-sensor body suit for kinematic sequencing.',
    status: 'Published',
    createdAt: iso(250),
  },
  {
    id: 'device-003',
    manufacturer: 'Catapult',
    model: 'Vector S7',
    deviceType: 'Wearable',
    notes: 'IMU-based athlete load monitoring.',
    status: 'Published',
    createdAt: iso(190),
  },
  {
    id: 'device-004',
    manufacturer: 'Garmin',
    model: 'Approach S70',
    deviceType: 'Wearable',
    notes: 'Consumer GPS watch; limited metric support.',
    status: 'Draft',
    createdAt: iso(72),
  },
  {
    id: 'device-005',
    manufacturer: 'Hawk-Eye',
    model: 'HE-2000',
    deviceType: 'Camera',
    notes: 'Multi-camera optical tracking rig.',
    status: 'Archived',
    createdAt: iso(480),
  },
];

let algorithms: AlgorithmVersionResponse[] = [
  {
    id: 'algo-001',
    code: null,
    name: 'Swing Kinematics',
    version: '2.1.0',
    supportedMetricCodes: 'HIP_ROTATION_VELOCITY,SWING_TEMPO_RATIO',
    notes: 'Stable release used for all published golf protocols.',
    status: 'Published',
    createdAt: iso(266),
  },
  {
    id: 'algo-002',
    code: null,
    name: 'Gait Analysis Engine',
    version: '1.3.0',
    supportedMetricCodes: 'STRIDE_SYMMETRY_INDEX',
    notes: 'Optimized for treadmill and overground running sessions.',
    status: 'Published',
    createdAt: iso(205),
  },
  {
    id: 'algo-003',
    code: null,
    name: 'Force Plate Fusion',
    version: '0.9.5',
    supportedMetricCodes: 'GROUND_REACTION_FORCE',
    notes: 'Beta release; calibration workflow still under review.',
    status: 'Draft',
    createdAt: iso(58),
  },
  {
    id: 'algo-004',
    code: null,
    name: 'Throw Mechanics',
    version: '1.0.0',
    supportedMetricCodes: 'ARM_SPEED_MAX',
    notes: 'Initial release for baseball pitching assessments.',
    status: 'Archived',
    createdAt: iso(420),
  },
  {
    id: 'algo-005',
    code: 'CMJ_BILATERAL',
    name: 'CMJ Bilateral Analysis',
    version: '2.1',
    supportedMetricCodes: 'CMJ_JUMP_HEIGHT_FLIGHT_TIME,CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM,CMJ_PEAK_FORCE,CMJ_RSI_MOD,CMJ_ASYMMETRY_INDEX,CMJ_TAKEOFF_VELOCITY',
    notes: 'Current production release. Baseline takeoff force threshold 100 N, event detection via force-threshold method.',
    status: 'Published',
    createdAt: iso(90),
  },
  {
    id: 'algo-006',
    code: 'CMJ_BILATERAL',
    name: 'CMJ Bilateral Analysis',
    version: '2.2',
    supportedMetricCodes: 'CMJ_JUMP_HEIGHT_FLIGHT_TIME,CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM,CMJ_PEAK_FORCE,CMJ_RSI_MOD,CMJ_ASYMMETRY_INDEX,CMJ_TAKEOFF_VELOCITY',
    notes: 'Refined takeoff onset detection (force-rate derivative). Marginal improvement on noisy plate data.',
    status: 'Published',
    createdAt: iso(45),
  },
  {
    id: 'algo-007',
    code: 'CMJ_BILATERAL',
    name: 'CMJ Bilateral Analysis',
    version: '3.0',
    supportedMetricCodes: 'CMJ_JUMP_HEIGHT_FLIGHT_TIME,CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM,CMJ_PEAK_FORCE,CMJ_RSI_MOD,CMJ_ASYMMETRY_INDEX,CMJ_TAKEOFF_VELOCITY',
    notes: 'New asymmetric impulse decomposition; bilateral channel required. Breaking change from v2.x input schema.',
    status: 'Draft',
    createdAt: iso(10),
  },
  {
    id: 'algo-008',
    code: 'SPRINT_30M_MULTIDEVICE',
    name: '30m Sprint Multi-Device Engine',
    version: '1.1',
    supportedMetricCodes: 'SPRINT_REACTION_TIME_BLOCK,SPRINT_PEAK_VELOCITY,SPRINT_GROUND_CONTACT_TIME,SPRINT_PEAK_GROUND_REACTION_FORCE,SPRINT_POWER_TRANSFER',
    notes: 'Merges radar velocity trace with front/rear starting-block force plates per trial.',
    status: 'Published',
    createdAt: iso(150),
  },
];

// --- Subscription plans & subscriptions -----------------------------------

const ALL_FEATURE_KEYS = [
  'testing.sprint',
  'testing.forceplate',
  'analytics.advanced',
  'integrations.api',
  'reports.export',
  'subscription.view',
  'subscription.manage',
];

const ALL_QUOTA_KEYS = [
  'members.active',
  'devices.registered',
  'sessions.monthly',
  'storage.bytes',
];

let plans: PlanResponse[] = [
  {
    id: 'plan-001',
    code: 'STARTER',
    name: 'Starter',
    version: 1,
    status: 'Published',
    description: 'Entry plan for small teams and individual coaches.',
    features: [
      { featureKey: 'testing.sprint', enabled: true },
      { featureKey: 'testing.forceplate', enabled: false },
      { featureKey: 'analytics.advanced', enabled: false },
      { featureKey: 'integrations.api', enabled: false },
      { featureKey: 'reports.export', enabled: true },
      { featureKey: 'subscription.view', enabled: true },
      { featureKey: 'subscription.manage', enabled: false },
    ],
    quotas: [
      { quotaKey: 'members.active', limitValue: 10 },
      { quotaKey: 'devices.registered', limitValue: 5 },
      { quotaKey: 'sessions.monthly', limitValue: 500 },
      { quotaKey: 'storage.bytes', limitValue: 10_737_418_240 },
    ],
    createdAt: iso(400),
  },
  {
    id: 'plan-002',
    code: 'PERFORMANCE_PRO',
    name: 'Performance Pro',
    version: 1,
    status: 'Published',
    description: 'Full-featured plan for performance labs and institutes.',
    features: [
      { featureKey: 'testing.sprint', enabled: true },
      { featureKey: 'testing.forceplate', enabled: true },
      { featureKey: 'analytics.advanced', enabled: true },
      { featureKey: 'integrations.api', enabled: true },
      { featureKey: 'reports.export', enabled: true },
      { featureKey: 'subscription.view', enabled: true },
      { featureKey: 'subscription.manage', enabled: false },
    ],
    quotas: [
      { quotaKey: 'members.active', limitValue: 100 },
      { quotaKey: 'devices.registered', limitValue: 50 },
      { quotaKey: 'sessions.monthly', limitValue: 10000 },
      { quotaKey: 'storage.bytes', limitValue: 536_870_912_000 },
    ],
    createdAt: iso(380),
  },
  {
    id: 'plan-003',
    code: 'ELITE',
    name: 'Elite',
    version: 1,
    status: 'Draft',
    description: 'Unlimited plan for high-volume enterprises (draft).',
    features: ALL_FEATURE_KEYS.map((k) => ({ featureKey: k, enabled: true })),
    quotas: ALL_QUOTA_KEYS.map((k) => ({ quotaKey: k, limitValue: null })),
    createdAt: iso(20),
  },
];

let subscriptions: PlatformSubscriptionResponse[] = [
  {
    id: 'sub-001',
    organizationId: 'org-001',
    planId: 'plan-002',
    planCode: 'PERFORMANCE_PRO',
    planName: 'Performance Pro',
    planVersion: 1,
    status: 'Active',
    startsAt: iso(400),
    endsAt: iso(-365),
    graceUntil: null,
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    revision: 1,
  },
  {
    id: 'sub-002',
    organizationId: 'org-002',
    planId: 'plan-002',
    planCode: 'PERFORMANCE_PRO',
    planName: 'Performance Pro',
    planVersion: 1,
    status: 'Active',
    startsAt: iso(365),
    endsAt: iso(-400),
    graceUntil: null,
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    revision: 1,
  },
  {
    id: 'sub-003',
    organizationId: 'org-003',
    planId: 'plan-001',
    planCode: 'STARTER',
    planName: 'Starter',
    planVersion: 1,
    status: 'Suspended',
    startsAt: iso(320),
    endsAt: iso(-40),
    graceUntil: iso(-20),
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    revision: 2,
  },
  {
    id: 'sub-004',
    organizationId: 'org-004',
    planId: 'plan-002',
    planCode: 'PERFORMANCE_PRO',
    planName: 'Performance Pro',
    planVersion: 1,
    status: 'Active',
    startsAt: iso(268),
    endsAt: iso(-500),
    graceUntil: null,
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    revision: 1,
  },
  {
    id: 'sub-005',
    organizationId: 'org-005',
    planId: 'plan-001',
    planCode: 'STARTER',
    planName: 'Starter',
    planVersion: 1,
    status: 'Trialing',
    startsAt: iso(214),
    endsAt: iso(-180),
    graceUntil: null,
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    revision: 1,
  },
  {
    id: 'sub-006',
    organizationId: 'org-006',
    planId: 'plan-002',
    planCode: 'PERFORMANCE_PRO',
    planName: 'Performance Pro',
    planVersion: 1,
    status: 'Active',
    startsAt: iso(160),
    endsAt: iso(-600),
    graceUntil: null,
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    revision: 1,
  },
  {
    id: 'sub-007',
    organizationId: 'org-007',
    planId: 'plan-001',
    planCode: 'STARTER',
    planName: 'Starter',
    planVersion: 1,
    status: 'Suspended',
    startsAt: iso(96),
    endsAt: iso(-30),
    graceUntil: null,
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    revision: 1,
  },
  {
    id: 'sub-008',
    organizationId: 'org-008',
    planId: 'plan-002',
    planCode: 'PERFORMANCE_PRO',
    planName: 'Performance Pro',
    planVersion: 1,
    status: 'Active',
    startsAt: iso(42),
    endsAt: iso(-700),
    graceUntil: null,
    cancelAtPeriodEnd: false,
    renewalMode: 'SalesManaged',
    revision: 1,
  },
];

let featureOverrides: FeatureOverrideResponse[] = [
  {
    id: 'fo-001',
    organizationId: 'org-003',
    featureKey: 'analytics.advanced',
    enabled: true,
    startsAt: iso(300),
    endsAt: iso(-60),
    reason: 'Trial extension for suspended org',
    createdAt: iso(300),
  },
];

let quotaOverrides: QuotaOverrideResponse[] = [
  {
    id: 'qo-001',
    organizationId: 'org-001',
    quotaKey: 'sessions.monthly',
    limitValue: 20000,
    startsAt: iso(100),
    endsAt: null,
    reason: 'High-volume research agreement',
    createdAt: iso(100),
  },
];

let subscriptionAudit: SubscriptionAuditEventResponse[] = [
  {
    id: 'sa-001',
    organizationId: 'org-001',
    actorId: 'usr-001',
    action: 'Subscription.Assigned',
    beforeJson: null,
    afterJson: JSON.stringify({ planCode: 'PERFORMANCE_PRO', status: 'Active' }),
    reason: 'Initial onboarding',
    occurredAt: iso(400),
    requestId: 'req-init-001',
  },
  {
    id: 'sa-002',
    organizationId: 'org-003',
    actorId: 'usr-001',
    action: 'Subscription.StatusChanged',
    beforeJson: JSON.stringify({ status: 'Active' }),
    afterJson: JSON.stringify({ status: 'Suspended' }),
    reason: 'Payment overdue — suspended',
    occurredAt: iso(40),
    requestId: 'req-susp-003',
  },
];

function orgNameById(id: string): string {
  return organizations.find((o) => o.id === id)?.name ?? id;
}

// --- Scientific algorithm meta (demo-only projection) ----------------------
//
// Demo-only enrichment joined to AlgorithmVersionResponse by `code`. It is NOT
// part of the backend API contract and must never be returned as API fields.
// Pages look up meta by code to render scientific release detail without
// inventing production API support.

export interface AlgorithmChannelRequirement {
  name: string;
  description: string;
  required: boolean;
}

export interface AlgorithmParameter {
  name: string;
  description: string;
  defaultValue: string;
}

export interface AlgorithmScientificMeta {
  domain: 'CMJ' | 'Sprint' | 'Force' | 'Other';
  requiredChannels: AlgorithmChannelRequirement[];
  parameters: AlgorithmParameter[];
  supportedProtocols: string[];
  releaseNotes: string;
  outputMetricCount: number;
}

export const ALGORITHM_SCIENTIFIC_META: Record<string, AlgorithmScientificMeta> = {
  CMJ_BILATERAL: {
    domain: 'CMJ',
    requiredChannels: [
      { name: 'force.vertical.left', description: 'Left force plate vertical ground reaction force (N).', required: true },
      { name: 'force.vertical.right', description: 'Right force plate vertical ground reaction force (N).', required: true },
      { name: 'time', description: 'Shared monotonic time axis (ms), 1000 Hz recommended.', required: true },
    ],
    parameters: [
      { name: 'takeoffForceThreshold', description: 'Vertical force below body weight that marks takeoff onset.', defaultValue: '100 N' },
      { name: 'landingForceThreshold', description: 'Vertical force above body weight that marks landing.', defaultValue: '100 N' },
      { name: 'bodyMassSource', description: 'How body mass is obtained for impulse–momentum calculations.', defaultValue: 'auto (weighing phase)' },
    ],
    supportedProtocols: ['CMJ_BILATERAL'],
    releaseNotes:
      'Event detection via force-threshold method (v2.1); v2.2 adds force-rate derivative for noisy plates; v3.0 introduces asymmetric impulse decomposition and requires bilateral channels.',
    outputMetricCount: 6,
  },
  SPRINT_30M_MULTIDEVICE: {
    domain: 'Sprint',
    requiredChannels: [
      { name: 'radar.velocity', description: 'Horizontal velocity trace from radar gun (m/s), 100 Hz.', required: true },
      { name: 'block.force.front', description: 'Front starting-block vertical force (N).', required: true },
      { name: 'block.force.rear', description: 'Rear starting-block vertical force (N).', required: true },
      { name: 'time', description: 'Shared monotonic time axis (ms).', required: true },
    ],
    parameters: [
      { name: 'blockThreshold', description: 'Rear-block force threshold for reaction-time onset.', defaultValue: '20 N' },
      { name: 'velocitySmoothing', description: 'Savitzky–Golay window for radar velocity trace.', defaultValue: '9 samples' },
      { name: 'distanceCutoff', description: 'Sprint distance included in analysis.', defaultValue: '30 m' },
    ],
    supportedProtocols: ['SPRINT_30M'],
    releaseNotes:
      'Merges radar velocity with front/rear starting-block force plates per trial. Reaction time derived from rear-block force onset; peak velocity from smoothed radar trace.',
    outputMetricCount: 5,
  },
};

export function getAlgorithms(): AlgorithmVersionResponse[] {
  return [...algorithms];
}

export function getProtocols(): GlobalProtocolResponse[] {
  return [...protocols];
}

export function getMetrics(): GlobalMetricResponse[] {
  return [...metrics];
}

// --- Scientific metric meta (demo-only projection) ------------------------
//
// Demo-only enrichment joined to GlobalMetricResponse by `code`. It is NOT
// part of the backend API contract. Pages look up meta by code to render
// method-specific, versioned scientific definitions without inventing
// production API support.

export interface MetricDefinitionVersion {
  version: string;
  effectiveFrom: string;
  changeSummary: string;
}

export interface MetricScientificMeta {
  domain: 'CMJ' | 'Sprint' | 'Other';
  physicalQuantity: string;
  method: string;
  methodLabel: string;
  definitionVersion: string;
  formula: string;
  dependentMetrics: string[];
  versionHistory: MetricDefinitionVersion[];
  notes: string;
}

export const METRIC_SCIENTIFIC_META: Record<string, MetricScientificMeta> = {
  CMJ_JUMP_HEIGHT_FLIGHT_TIME: {
    domain: 'CMJ',
    physicalQuantity: 'Jump Height',
    method: 'flight_time',
    methodLabel: 'Flight Time',
    definitionVersion: '1.0',
    formula: 'h = g · t_flight² / 8',
    dependentMetrics: [],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition using flight-time method.' },
    ],
    notes: 'Jump height derived from flight time using the impulse-momentum relationship. Assumes takeoff and landing heights are equal. This is a DISTINCT method from impulse-momentum and should not be conflated with it.',
  },
  CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM: {
    domain: 'CMJ',
    physicalQuantity: 'Jump Height',
    method: 'impulse_momentum',
    methodLabel: 'Impulse-Momentum',
    definitionVersion: '1.0',
    formula: 'h = (∫F_net·dt / m_body)² / (2g)',
    dependentMetrics: ['CMJ_PEAK_FORCE'],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition using net vertical impulse method.' },
    ],
    notes: 'Jump height derived from net vertical impulse during the eccentric + concentric phase. Requires body mass and force-time integration. This is a DISTINCT method from flight-time and produces systematically different values.',
  },
  CMJ_PEAK_FORCE: {
    domain: 'CMJ',
    physicalQuantity: 'Peak Vertical Force',
    method: 'peak_detection',
    methodLabel: 'Peak Detection',
    definitionVersion: '1.0',
    formula: 'F_peak = max(F_vertical(t)) over concentric phase',
    dependentMetrics: [],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Maximum vertical ground reaction force measured during the concentric push-off phase.',
  },
  CMJ_RSI_MOD: {
    domain: 'CMJ',
    physicalQuantity: 'Reactive Strength Index (Modified)',
    method: 'modified',
    methodLabel: 'Modified RSI',
    definitionVersion: '1.0',
    formula: 'RSI_mod = h_jump / (t_eccentric + t_concentric)',
    dependentMetrics: ['CMJ_JUMP_HEIGHT_FLIGHT_TIME'],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Jump height divided by eccentric + concentric phase time. Uses flight-time jump height as the numerator.',
  },
  CMJ_ASYMMETRY_INDEX: {
    domain: 'CMJ',
    physicalQuantity: 'Asymmetry Index',
    method: 'bilateral_comparison',
    methodLabel: 'Bilateral Comparison',
    definitionVersion: '1.0',
    formula: 'AI = |F_left - F_right| / ((F_left + F_right) / 2) × 100',
    dependentMetrics: [],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Percentage difference between left and right peak forces. Requires bilateral force plate channels.',
  },
  CMJ_TAKEOFF_VELOCITY: {
    domain: 'CMJ',
    physicalQuantity: 'Takeoff Velocity',
    method: 'impulse_integration',
    methodLabel: 'Impulse Integration',
    definitionVersion: '1.0',
    formula: 'v_takeoff = ∫F_net·dt / m_body',
    dependentMetrics: ['CMJ_PEAK_FORCE'],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Vertical velocity at takeoff computed from net impulse and body mass.',
  },
  SPRINT_REACTION_TIME_BLOCK: {
    domain: 'Sprint',
    physicalQuantity: 'Block Reaction Time',
    method: 'force_threshold',
    methodLabel: 'Force Threshold',
    definitionVersion: '1.0',
    formula: 't_reaction = t(rear_block_force > threshold) - t_signal',
    dependentMetrics: [],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Time from the start signal to the rear foot force exceeding the threshold on the starting block.',
  },
  SPRINT_PEAK_VELOCITY: {
    domain: 'Sprint',
    physicalQuantity: 'Peak Velocity',
    method: 'radar_direct',
    methodLabel: 'Radar Direct',
    definitionVersion: '1.0',
    formula: 'v_peak = max(v_horizontal(t)) over 30m',
    dependentMetrics: [],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Maximum horizontal velocity achieved during the 30m sprint, measured directly by radar gun.',
  },
  SPRINT_GROUND_CONTACT_TIME: {
    domain: 'Sprint',
    physicalQuantity: 'Ground Contact Time',
    method: 'force_bilateral',
    methodLabel: 'Bilateral Force',
    definitionVersion: '1.0',
    formula: 'GCT = mean(t_contact_i) over all steps',
    dependentMetrics: [],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Average foot-ground contact time across all steps, detected via bilateral force plates.',
  },
  SPRINT_PEAK_GROUND_REACTION_FORCE: {
    domain: 'Sprint',
    physicalQuantity: 'Peak GRF (Single Step)',
    method: 'force_plate',
    methodLabel: 'Force Plate',
    definitionVersion: '1.0',
    formula: 'F_peak_step = max(F_front_block(t)) during push-off',
    dependentMetrics: [],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Maximum force recorded on the front block plate during push-off.',
  },
  SPRINT_POWER_TRANSFER: {
    domain: 'Sprint',
    physicalQuantity: 'Power Transfer Index',
    method: 'impulse_rate',
    methodLabel: 'Impulse Rate',
    definitionVersion: '1.0',
    formula: 'PTI = dF_rear/dt at block exit',
    dependentMetrics: [],
    versionHistory: [
      { version: '1.0', effectiveFrom: '2025-01-01', changeSummary: 'Initial definition.' },
    ],
    notes: 'Rate of force development on the rear block as a proxy for block efficiency.',
  },
};

// --- Audit log -------------------------------------------------------------

let auditLogs: AuditLogResponse[] = [
  {
    id: 'audit-001',
    actorUserId: 'usr-001',
    actorEmail: ADMIN_EMAIL,
    action: 'Organization.Suspended',
    entityType: 'Organization',
    entityId: 'org-007',
    summary: 'Suspended organization Rio Motion Lab',
    metadataJson: null,
    createdAt: iso(0, 2),
  },
  {
    id: 'audit-002',
    actorUserId: 'usr-001',
    actorEmail: ADMIN_EMAIL,
    action: 'GlobalProtocol.Published',
    entityType: 'GlobalProtocol',
    entityId: 'proto-005',
    summary: 'Published protocol WEIGHTLIFT_SQUAT_V2',
    metadataJson: null,
    createdAt: iso(0, 5),
  },
  {
    id: 'audit-003',
    actorUserId: 'usr-002',
    actorEmail: 'sarah.chen@velocitylab.com',
    action: 'Session.Completed',
    entityType: 'User',
    entityId: 'usr-011',
    summary: 'Member account updated by organization owner',
    metadataJson: null,
    createdAt: iso(1, 1),
  },
  {
    id: 'audit-004',
    actorUserId: 'usr-001',
    actorEmail: ADMIN_EMAIL,
    action: 'GlobalMetric.Created',
    entityType: 'GlobalMetric',
    entityId: 'metric-005',
    summary: 'Created metric ARM_SPEED_MAX',
    metadataJson: null,
    createdAt: iso(1, 6),
  },
  {
    id: 'audit-005',
    actorUserId: 'usr-006',
    actorEmail: 'jonas.weber@peakform.de',
    action: 'Organization.Updated',
    entityType: 'Organization',
    entityId: 'org-004',
    summary: 'Updated organization profile for PeakForm Analytics',
    metadataJson: null,
    createdAt: iso(2, 3),
  },
  {
    id: 'audit-006',
    actorUserId: 'usr-001',
    actorEmail: ADMIN_EMAIL,
    action: 'User.AdminGranted',
    entityType: 'User',
    entityId: 'usr-002',
    summary: 'Granted platform administrator role to sarah.chen@velocitylab.com',
    metadataJson: null,
    createdAt: iso(3, 2),
  },
  {
    id: 'audit-007',
    actorUserId: 'usr-008',
    actorEmail: 'daniel.roy@summitathletics.ca',
    action: 'DeviceModel.Created',
    entityType: 'DeviceModel',
    entityId: 'device-004',
    summary: 'Created device model Garmin Approach S70',
    metadataJson: null,
    createdAt: iso(4, 7),
  },
  {
    id: 'audit-008',
    actorUserId: 'usr-001',
    actorEmail: ADMIN_EMAIL,
    action: 'AlgorithmVersion.Published',
    entityType: 'AlgorithmVersion',
    entityId: 'algo-002',
    summary: 'Published algorithm Gait Analysis Engine 1.3.0',
    metadataJson: null,
    createdAt: iso(6, 4),
  },
  {
    id: 'audit-009',
    actorUserId: 'usr-001',
    actorEmail: ADMIN_EMAIL,
    action: 'User.Suspended',
    entityType: 'User',
    entityId: 'usr-003',
    summary: 'Suspended user david.okafor@velocitylab.com',
    metadataJson: null,
    createdAt: iso(8, 1),
  },
  {
    id: 'audit-010',
    actorUserId: 'usr-009',
    actorEmail: 'rafael.souza@riomotion.br',
    action: 'Organization.Updated',
    entityType: 'Organization',
    entityId: 'org-007',
    summary: 'Updated organization profile for Rio Motion Lab',
    metadataJson: null,
    createdAt: iso(12, 5),
  },
  {
    id: 'audit-011',
    actorUserId: 'usr-001',
    actorEmail: ADMIN_EMAIL,
    action: 'GlobalProtocol.Archived',
    entityType: 'GlobalProtocol',
    entityId: 'proto-006',
    summary: 'Archived protocol LEGACY_JUMP_V1',
    metadataJson: null,
    createdAt: iso(15, 2),
  },
  {
    id: 'audit-012',
    actorUserId: 'usr-004',
    actorEmail: 'oliver.grant@apexpi.com',
    action: 'Organization.Created',
    entityType: 'Organization',
    entityId: 'org-002',
    summary: 'Created organization Apex Performance Institute',
    metadataJson: null,
    createdAt: iso(365),
  },
];

function addAudit(
  entityType: string,
  entityId: string,
  action: string,
  summary: string
): void {
  auditLogs = [
    {
      id: nextId('audit'),
      actorUserId: 'usr-001',
      actorEmail: ADMIN_EMAIL,
      action,
      entityType,
      entityId,
      summary,
      metadataJson: null,
      createdAt: new Date().toISOString(),
    },
    ...auditLogs,
  ];
}

const auditLogsByCreatedDesc = () => auditLogs;

// --- Dashboard -------------------------------------------------------------

function buildDashboard(): PlatformDashboardResponse {
  return {
    totalOrganizations: organizations.length,
    activeOrganizations: organizations.filter((org) => org.status === 'Active').length,
    suspendedOrganizations: organizations.filter((org) => org.status === 'Suspended').length,
    individualOrganizations: organizations.filter((org) => org.type === 'Individual').length,
    totalUsers: users.length,
    activeUsers: users.filter((user) => user.status === 'Active').length,
    pendingVerificationUsers: users.filter(
      (user) => user.status === 'PendingEmailVerification' || !user.emailVerified
    ).length,
    totalProtocolDefinitions: protocols.length,
    totalMetrics: metrics.length,
    recentOrganizations: byCreatedDesc(organizations).slice(0, 3),
    recentAuditLogs: auditLogsByCreatedDesc().slice(0, 5),
  };
}

// --- Catalog helpers -------------------------------------------------------

type CatalogKind = 'protocols' | 'metrics' | 'devices' | 'algorithms';

type CatalogItem = (
  | GlobalProtocolResponse
  | GlobalMetricResponse
  | DeviceModelResponse
  | AlgorithmVersionResponse
) & { id: string; status: string; createdAt: string };

const CATALOGS: Record<CatalogKind, CatalogItem[]> = {
  protocols,
  metrics,
  devices,
  algorithms,
};

const CATALOG_META: Record<
  CatalogKind,
  { prefix: string; entityType: string; label: (item: CatalogItem) => string }
> = {
  protocols: {
    prefix: 'proto',
    entityType: 'GlobalProtocol',
    label: (item) => (item as GlobalProtocolResponse).code,
  },
  metrics: {
    prefix: 'metric',
    entityType: 'GlobalMetric',
    label: (item) => (item as GlobalMetricResponse).code,
  },
  devices: {
    prefix: 'device',
    entityType: 'DeviceModel',
    label: (item) => {
      const device = item as DeviceModelResponse;
      return `${device.manufacturer} ${device.model}`;
    },
  },
  algorithms: {
    prefix: 'algo',
    entityType: 'AlgorithmVersion',
    label: (item) => {
      const algorithm = item as AlgorithmVersionResponse;
      return `${algorithm.name} ${algorithm.version}`;
    },
  },
};

function handleCatalog(
  method: string,
  kind: CatalogKind,
  rest: string[],
  body: Record<string, unknown>
): unknown {
  const collection = CATALOGS[kind];
  const meta = CATALOG_META[kind];

  if (rest.length === 0) {
    if (method === 'GET') return byCreatedDesc(collection);
    if (method === 'POST') {
      const created = {
        id: nextId(meta.prefix),
        status: 'Draft',
        createdAt: new Date().toISOString(),
        ...body,
      } as CatalogItem;
      collection.push(created);
      addAudit(meta.entityType, created.id, `${meta.entityType}.Created`, `Created ${meta.label(created)}`);
      return created;
    }
    throw new ApiError(405, 'Method not allowed');
  }

  const id = rest[0];
  const item = collection.find((entry) => entry.id === id);
  if (!item) throw new ApiError(404, 'Resource not found');

  if (rest.length === 1) {
    if (method === 'GET') return item;
    if (method === 'PUT') {
      Object.assign(item, body);
      addAudit(meta.entityType, item.id, `${meta.entityType}.Updated`, `Updated ${meta.label(item)}`);
      return item;
    }
    if (method === 'DELETE') {
      const index = collection.indexOf(item);
      collection.splice(index, 1);
      addAudit(meta.entityType, item.id, `${meta.entityType}.Deleted`, `Deleted ${meta.label(item)}`);
      return undefined;
    }
    throw new ApiError(405, 'Method not allowed');
  }

  if (rest.length === 2 && rest[1] === 'status' && method === 'POST') {
    const action = String(body.action ?? '');
    if (action === 'publish') item.status = 'Published';
    else if (action === 'archive') item.status = 'Archived';
    else throw new ApiError(400, `Unsupported action '${action}'`);

    addAudit(
      meta.entityType,
      item.id,
      `${meta.entityType}.${item.status}`,
      `${item.status === 'Published' ? 'Published' : 'Archived'} ${meta.label(item)}`
    );
    return undefined;
  }

  throw new ApiError(404, 'Resource not found');
}

// --- Router ----------------------------------------------------------------

export async function mockRequest<T>(path: string, options: RequestInit = {}): Promise<T> {
  await delay();

  const [pathname, search] = path.split('?');
  const params = new URLSearchParams(search ?? '');
  const method = (options.method ?? 'GET').toUpperCase();
  const segments = pathname.split('/').filter(Boolean);

  let body: Record<string, unknown> = {};
  if (typeof options.body === 'string' && options.body.trim() !== '') {
    try {
      body = JSON.parse(options.body) as Record<string, unknown>;
    } catch {
      throw new ApiError(400, 'Invalid JSON body');
    }
  }

  // Auth
  if (method === 'POST' && pathname === '/auth/login') {
    const email = String(body.email ?? '').trim().toLowerCase();
    const password = String(body.password ?? '');

    if (email !== ADMIN_EMAIL || password !== ADMIN_PASSWORD) {
      throw new ApiError(401, 'Invalid email or password.');
    }

    const profile: AuthProfileResponse = {
      userId: 'usr-001',
      email: ADMIN_EMAIL,
      displayName: 'Platform Admin',
      organizationId: 'org-platform',
      organizationName: 'KWinHub Platform',
      organizationRole: 'PlatformAdmin',
      isPlatformAdmin: true,
      emailVerified: true,
      status: 'Active',
    };

    return {
      ...profile,
      token: `mock-token-${Date.now()}`,
      type: 'Bearer',
    } as LoginResponse as unknown as T;
  }

  if (method === 'GET' && pathname === '/auth/me') {
    return {
      userId: 'usr-001',
      email: ADMIN_EMAIL,
      displayName: 'Platform Admin',
      organizationId: 'org-platform',
      organizationName: 'KWinHub Platform',
      organizationRole: 'PlatformAdmin',
      isPlatformAdmin: true,
      emailVerified: true,
      status: 'Active',
    } as AuthProfileResponse as unknown as T;
  }

  // Dashboard
  if (method === 'GET' && pathname === '/platform/dashboard') {
    return buildDashboard() as unknown as T;
  }

  // Organizations
  if (segments[0] === 'platform' && segments[1] === 'organizations') {
    if (segments.length === 2 && method === 'GET') {
      const searchTerm = (params.get('search') ?? '').trim().toLowerCase();
      const status = params.get('status') ?? '';
      const type = params.get('type') ?? '';

      const filtered = byCreatedDesc(organizations).filter((org) => {
        if (status && org.status !== status) return false;
        if (type && org.type !== type) return false;
        if (searchTerm) {
          const haystack = `${org.name} ${org.slug}`.toLowerCase();
          if (!haystack.includes(searchTerm)) return false;
        }
        return true;
      });

      return filtered as unknown as T;
    }

    const id = segments[2];

    if (segments.length === 3 && method === 'GET') {
      const organization = organizations.find((org) => org.id === id);
      if (!organization) throw new ApiError(404, 'Organization not found');

      const meta = organizationMeta[id] ?? {
        region: null,
        timezone: null,
        locale: null,
        createdByUserId: null,
      };

      return {
        id: organization.id,
        name: organization.name,
        slug: organization.slug,
        type: organization.type,
        status: organization.status,
        countryCode: organization.countryCode,
        region: meta.region,
        timezone: meta.timezone,
        locale: meta.locale,
        createdByUserId: meta.createdByUserId,
        createdAt: organization.createdAt,
        members: organizationMembers[id] ?? [],
      } as PlatformOrganizationDetailResponse as unknown as T;
    }

    if (segments.length === 4 && segments[3] === 'status' && method === 'POST') {
      const organization = organizations.find((org) => org.id === id);
      if (!organization) throw new ApiError(404, 'Organization not found');

      const action = String(body.action ?? '');
      if (action === 'suspend') organization.status = 'Suspended';
      else if (action === 'reactivate') organization.status = 'Active';
      else throw new ApiError(400, `Unsupported action '${action}'`);

      addAudit(
        'Organization',
        organization.id,
        `Organization.${organization.status}`,
        `${organization.status === 'Suspended' ? 'Suspended' : 'Reactivated'} organization ${organization.name}`
      );
      return undefined as unknown as T;
    }
  }

  // Users
  if (segments[0] === 'platform' && segments[1] === 'users') {
    if (segments.length === 2 && method === 'GET') {
      const searchTerm = (params.get('search') ?? '').trim().toLowerCase();
      const status = params.get('status') ?? '';

      const filtered = byCreatedDesc(users).filter((user) => {
        if (status && user.status !== status) return false;
        if (searchTerm) {
          const haystack = `${user.email} ${user.displayName}`.toLowerCase();
          if (!haystack.includes(searchTerm)) return false;
        }
        return true;
      });

      return filtered as unknown as T;
    }

    const id = segments[2];

    if (segments.length === 4 && segments[3] === 'status' && method === 'POST') {
      const user = users.find((entry) => entry.id === id);
      if (!user) throw new ApiError(404, 'User not found');

      const action = String(body.action ?? '');
      if (action === 'suspend') user.status = 'Suspended';
      else if (action === 'activate') user.status = 'Active';
      else if (action === 'grant-admin') user.platformRole = 'PlatformAdmin';
      else if (action === 'revoke-admin') user.platformRole = null;
      else throw new ApiError(400, `Unsupported action '${action}'`);

      const actionLabel: Record<string, string> = {
        suspend: 'User.Suspended',
        activate: 'User.Activated',
        'grant-admin': 'User.AdminGranted',
        'revoke-admin': 'User.AdminRevoked',
      };
      addAudit(
        'User',
        user.id,
        actionLabel[action],
        `${action === 'suspend' ? 'Suspended' : action === 'activate' ? 'Activated' : action === 'grant-admin' ? 'Granted admin to' : 'Revoked admin from'} ${user.email}`
      );
      return undefined as unknown as T;
    }
  }

  // Catalogs
  if (segments[0] === 'platform' && segments[1] === 'catalogs') {
    const kind = segments[2] as CatalogKind;
    if (kind in CATALOGS) {
      return handleCatalog(method, kind, segments.slice(3), body) as unknown as T;
    }
  }

  // Audit logs
  if (method === 'GET' && pathname === '/platform/audit-logs') {
    const entityType = params.get('entityType') ?? '';
    const take = Number(params.get('take') ?? '200');

    const filtered = auditLogsByCreatedDesc()
      .filter((log) => (entityType ? log.entityType === entityType : true))
      .slice(0, Number.isFinite(take) ? take : 200);

    return filtered as unknown as T;
  }

  // Subscription plans
  if (segments[0] === 'platform' && segments[1] === 'subscription-plans') {
    if (segments.length === 2 && method === 'GET') {
      return byCreatedDesc(plans) as unknown as T;
    }
    if (segments.length === 2 && method === 'POST') {
      const code = String(body.code ?? '').trim();
      if (!code) throw new ApiError(400, 'Plan code is required');
      if (plans.some((p) => p.code === code)) throw new ApiError(400, `Plan code '${code}' already exists`);
      const created: PlanResponse = {
        id: nextId('plan'),
        code,
        name: String(body.name ?? code),
        version: Number(body.version ?? 1),
        status: 'Draft',
        description: (body.description as string | null) ?? null,
        features: (body.features as PlanResponse['features']) ?? [],
        quotas: (body.quotas as PlanResponse['quotas']) ?? [],
        createdAt: new Date().toISOString(),
      };
      plans.push(created);
      addAudit('SubscriptionPlan', created.id, 'SubscriptionPlan.Created', `Created plan ${created.code}`);
      return created as unknown as T;
    }
    if (segments.length === 3 && segments[2] === 'publish' && method === 'POST') {
      // handled below as /subscription-plans/{id}/publish
    }
  }
  // POST /platform/subscription-plans/{id}/publish
  if (
    segments[0] === 'platform' &&
    segments[1] === 'subscription-plans' &&
    segments.length === 3 &&
    segments[2] !== 'publish'
  ) {
    // GET single plan not strictly required; fall through
  }
  if (
    segments[0] === 'platform' &&
    segments[1] === 'subscription-plans' &&
    segments[3] === 'publish' &&
    method === 'POST'
  ) {
    const plan = plans.find((p) => p.id === segments[2]);
    if (!plan) throw new ApiError(404, 'Plan not found');
    plan.status = 'Published';
    addAudit('SubscriptionPlan', plan.id, 'SubscriptionPlan.Published', `Published plan ${plan.code}`);
    return plan as unknown as T;
  }

  // Subscriptions
  if (segments[0] === 'platform' && segments[1] === 'subscriptions' && segments.length === 2) {
    if (method === 'GET') {
      const orgId = params.get('organizationId') ?? '';
      const filtered = orgId
        ? subscriptions.filter((s) => s.organizationId === orgId)
        : subscriptions;
      return filtered as unknown as T;
    }
  }

  // PUT /platform/organizations/{id}/subscription (assign/replace)
  if (
    segments[0] === 'platform' &&
    segments[1] === 'organizations' &&
    segments[3] === 'subscription' &&
    method === 'PUT'
  ) {
    const orgId = segments[2];
    const planId = String(body.planId ?? '');
    const plan = plans.find((p) => p.id === planId);
    if (!plan) throw new ApiError(400, 'Plan not found');
    const reason = String(body.reason ?? '').trim();
    if (!reason) throw new ApiError(400, 'Reason is required');

    const existing = subscriptions.find((s) => s.organizationId === orgId);
    const beforeJson = existing ? JSON.stringify(existing) : null;

    const nextRevision = existing ? existing.revision + 1 : 1;
    const sub: PlatformSubscriptionResponse = {
      id: existing?.id ?? nextId('sub'),
      organizationId: orgId,
      planId: plan.id,
      planCode: plan.code,
      planName: plan.name,
      planVersion: plan.version,
      status: String(body.status ?? 'Active'),
      startsAt: String(body.startsAt ?? new Date().toISOString()),
      endsAt: String(body.endsAt ?? new Date(Date.now() + 365 * 86_400_000).toISOString()),
      graceUntil: (body.graceUntil as string | null) ?? null,
      cancelAtPeriodEnd: false,
      renewalMode: 'SalesManaged',
      revision: nextRevision,
    };

    if (existing) {
      Object.assign(existing, sub);
    } else {
      subscriptions.push(sub);
    }

    subscriptionAudit.unshift({
      id: nextId('sa'),
      organizationId: orgId,
      actorId: 'usr-001',
      action: existing ? 'Subscription.Updated' : 'Subscription.Assigned',
      beforeJson,
      afterJson: JSON.stringify(sub),
      reason,
      occurredAt: new Date().toISOString(),
      requestId: `req-${nextId('req')}`,
    });

    return sub as unknown as T;
  }

  // PUT /platform/organizations/{id}/entitlements/{featureKey}
  if (
    segments[0] === 'platform' &&
    segments[1] === 'organizations' &&
    segments[3] === 'entitlements' &&
    method === 'PUT'
  ) {
    const orgId = segments[2];
    const featureKey = segments[4];
    const reason = String(body.reason ?? '').trim();
    if (!reason) throw new ApiError(400, 'Reason is required');

    const override: FeatureOverrideResponse = {
      id: nextId('fo'),
      organizationId: orgId,
      featureKey,
      enabled: Boolean(body.enabled),
      startsAt: String(body.startsAt ?? new Date().toISOString()),
      endsAt: (body.endsAt as string | null) ?? null,
      reason,
      createdAt: new Date().toISOString(),
    };
    const idx = featureOverrides.findIndex((o) => o.organizationId === orgId && o.featureKey === featureKey);
    if (idx >= 0) featureOverrides[idx] = override;
    else featureOverrides.push(override);

    subscriptionAudit.unshift({
      id: nextId('sa'),
      organizationId: orgId,
      actorId: 'usr-001',
      action: 'FeatureOverride.Set',
      beforeJson: null,
      afterJson: JSON.stringify(override),
      reason,
      occurredAt: new Date().toISOString(),
      requestId: `req-${nextId('req')}`,
    });

    return override as unknown as T;
  }

  // PUT /platform/organizations/{id}/quotas/{quotaKey}
  if (
    segments[0] === 'platform' &&
    segments[1] === 'organizations' &&
    segments[3] === 'quotas' &&
    method === 'PUT'
  ) {
    const orgId = segments[2];
    const quotaKey = segments[4];
    const reason = String(body.reason ?? '').trim();
    if (!reason) throw new ApiError(400, 'Reason is required');

    const limitRaw = body.limitValue;
    const limitValue = limitRaw === null || limitRaw === undefined || limitRaw === ''
      ? null
      : Number(limitRaw);

    const override: QuotaOverrideResponse = {
      id: nextId('qo'),
      organizationId: orgId,
      quotaKey,
      limitValue,
      startsAt: String(body.startsAt ?? new Date().toISOString()),
      endsAt: (body.endsAt as string | null) ?? null,
      reason,
      createdAt: new Date().toISOString(),
    };
    const idx = quotaOverrides.findIndex((o) => o.organizationId === orgId && o.quotaKey === quotaKey);
    if (idx >= 0) quotaOverrides[idx] = override;
    else quotaOverrides.push(override);

    subscriptionAudit.unshift({
      id: nextId('sa'),
      organizationId: orgId,
      actorId: 'usr-001',
      action: 'QuotaOverride.Set',
      beforeJson: null,
      afterJson: JSON.stringify(override),
      reason,
      occurredAt: new Date().toISOString(),
      requestId: `req-${nextId('req')}`,
    });

    return override as unknown as T;
  }

  // GET /platform/organizations/{id}/subscription-audit
  if (
    segments[0] === 'platform' &&
    segments[1] === 'organizations' &&
    segments[3] === 'subscription-audit' &&
    method === 'GET'
  ) {
    const orgId = segments[2];
    const filtered = subscriptionAudit.filter((e) => e.organizationId === orgId);
    return filtered as unknown as T;
  }

  // GET /platform/organizations/{id}/entitlements (override list for org detail)
  if (
    segments[0] === 'platform' &&
    segments[1] === 'organizations' &&
    segments[3] === 'entitlements' &&
    method === 'GET'
  ) {
    const orgId = segments[2];
    return featureOverrides.filter((o) => o.organizationId === orgId) as unknown as T;
  }

  // GET /platform/organizations/{id}/quotas (override list for org detail)
  if (
    segments[0] === 'platform' &&
    segments[1] === 'organizations' &&
    segments[3] === 'quotas' &&
    method === 'GET'
  ) {
    const orgId = segments[2];
    return quotaOverrides.filter((o) => o.organizationId === orgId) as unknown as T;
  }

  throw new ApiError(404, `No mock handler for ${method} ${pathname}`);
}