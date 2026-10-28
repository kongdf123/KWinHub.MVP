import { ApiError } from './api';
import type {
  AlgorithmVersionResponse,
  AuditLogResponse,
  AuthProfileResponse,
  DeviceModelResponse,
  GlobalMetricResponse,
  GlobalProtocolResponse,
  LoginResponse,
  PlatformDashboardResponse,
  PlatformOrganizationDetailResponse,
  PlatformOrganizationMemberResponse,
  PlatformOrganizationResponse,
  PlatformUserResponse,
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

  throw new ApiError(404, `No mock handler for ${method} ${pathname}`);
}