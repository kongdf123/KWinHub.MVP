import { mockRequest } from './mockApi';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';
const TOKEN_KEY = 'kwinhub_console_token';

// Prototype mode: serve all requests from the in-memory mock layer until the
// platform backend is available. Set VITE_USE_MOCK_API=false to call the API.
const USE_MOCK_API = (import.meta.env.VITE_USE_MOCK_API ?? 'true') !== 'false';

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
    this.name = 'ApiError';
  }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  if (USE_MOCK_API) return mockRequest<T>(path, options);

  const token = getToken();
  const headers = new Headers(options.headers);
  if (token) headers.set('Authorization', `Bearer ${token}`);
  if (!headers.has('Content-Type') && options.body) headers.set('Content-Type', 'application/json');

  const url = path.startsWith('http') ? path : `${BASE_URL}${path}`;
  const res = await fetch(url, { ...options, headers });

  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = await res.json();
      if (body?.detail || body?.message || body?.title) {
        message = body.detail || body.message || body.title;
      }
    } catch {
      // response had no JSON body — keep the status text
    }

    if (res.status === 401) {
      clearToken();
      throw new ApiError(401, message || 'Unauthorized — session expired');
    }
    if (res.status === 403) {
      throw new ApiError(403, message || 'Forbidden — insufficient permissions');
    }

    throw new ApiError(res.status, message || `Request failed (${res.status})`);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function isLoggedIn(): boolean {
  return !!getToken();
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'POST', body: body === undefined ? undefined : JSON.stringify(body) }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'PUT', body: body === undefined ? undefined : JSON.stringify(body) }),
  del: (path: string) => request<void>(path, { method: 'DELETE' }),
  changeStatus: <T>(path: string, action: string) =>
    request<T>(path, { method: 'POST', body: JSON.stringify({ action }) }),
};

export async function login(email: string, password: string): Promise<LoginResponse> {
  return request<LoginResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  });
}

// --- Backend contracts -----------------------------------------------------

export interface LoginResponse {
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

export interface AuthProfileResponse {
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

export interface PlatformOrganizationResponse {
  id: string;
  name: string;
  slug: string;
  type: string;
  status: string;
  countryCode: string | null;
  createdAt: string;
  memberCount: number;
  subjectCount: number;
  sessionCount: number;
  resultCount: number;
}

export interface PlatformOrganizationMemberResponse {
  userId: string;
  email: string;
  displayName: string;
  role: string;
  status: string;
  joinedAt: string;
}

export interface PlatformOrganizationDetailResponse {
  id: string;
  name: string;
  slug: string;
  type: string;
  status: string;
  countryCode: string | null;
  region: string | null;
  timezone: string | null;
  locale: string | null;
  createdByUserId: string | null;
  createdAt: string;
  members: PlatformOrganizationMemberResponse[];
}

export interface PlatformUserResponse {
  id: string;
  email: string;
  displayName: string;
  status: string;
  platformRole: string | null;
  emailVerified: boolean;
  lastLoginAt: string | null;
  createdAt: string;
  organizationCount: number;
}

export interface AuditLogResponse {
  id: string;
  actorUserId: string | null;
  actorEmail: string | null;
  action: string;
  entityType: string;
  entityId: string | null;
  summary: string;
  metadataJson: string | null;
  createdAt: string;
}

export interface PlatformDashboardResponse {
  totalOrganizations: number;
  activeOrganizations: number;
  suspendedOrganizations: number;
  individualOrganizations: number;
  totalUsers: number;
  activeUsers: number;
  pendingVerificationUsers: number;
  totalProtocolDefinitions: number;
  totalMetrics: number;
  recentOrganizations: PlatformOrganizationResponse[];
  recentAuditLogs: AuditLogResponse[];
}

export interface GlobalProtocolResponse {
  id: string;
  code: string;
  name: string;
  version: string;
  category: string | null;
  sport: string | null;
  description: string | null;
  status: string;
  createdAt: string;
}

export interface GlobalMetricResponse {
  id: string;
  code: string;
  name: string;
  unit: string;
  category: string | null;
  description: string | null;
  status: string;
  createdAt: string;
}

export interface DeviceModelResponse {
  id: string;
  manufacturer: string;
  model: string;
  deviceType: string;
  notes: string | null;
  status: string;
  createdAt: string;
}

export interface AlgorithmVersionResponse {
  id: string;
  name: string;
  version: string;
  supportedMetricCodes: string | null;
  notes: string | null;
  status: string;
  createdAt: string;
}
