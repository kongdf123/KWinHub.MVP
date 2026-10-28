const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';

export class ApiError extends Error {
  readonly status: number;
  readonly detail?: unknown;

  constructor(status: number, message: string, detail?: unknown) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.detail = detail;
  }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();
  const headers = new Headers(options.headers);
  if (token) headers.set('Authorization', `Bearer ${token}`);
  if (!headers.has('Content-Type') && options.body) headers.set('Content-Type', 'application/json');

  const url = path.startsWith('http') ? path : `${BASE_URL}${path}`;
  const res = await fetch(url, { ...options, headers });

  if (!res.ok) {
    let msg = res.statusText;
    try { const body = await res.json(); if (body?.detail || body?.message) msg = body.detail || body.message; } catch {}
    if (res.status === 401) { clearToken(); throw new ApiError(401, 'Unauthorized — session expired'); }
    if (res.status === 403) throw new ApiError(403, 'Forbidden — insufficient permissions');
    throw new ApiError(res.status, msg);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

export function getToken(): string | null {
  return localStorage.getItem('kwinhub_token');
}

export function setToken(token: string): void {
  localStorage.setItem('kwinhub_token', token);
}

export function clearToken(): void {
  localStorage.removeItem('kwinhub_token');
}

export function isLoggedIn(): boolean {
  return !!getToken();
}

/**
 * Decodes the payload of a JWT stored in localStorage and returns it as a
 * plain object. Returns null when no token is present or decoding fails.
 * This is purely client-side; signature verification happens server-side.
 */
export function decodeJwt<T = Record<string, unknown>>(): T | null {
  const token = getToken();
  if (!token) return null;
  try {
    const payload = token.split('.')[1];
    if (!payload) return null;
    // JWT uses base64url, not standard base64.
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(normalized)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    );
    return JSON.parse(json) as T;
  } catch {
    return null;
  }
}

/**
 * Returns the organization id for the current user from the JWT `org_id`
 * claim. Falls back to the `VITE_DEV_ORG_ID` env var when the claim is absent
 * (e.g. local dev tokens) and to the demo org id as a last resort.
 */
export function getCurrentOrgId(): string | null {
  const claims = decodeJwt<{ org_id?: string }>();
  if (claims?.org_id) return claims.org_id;
  const dev = import.meta.env.VITE_DEV_ORG_ID;
  return dev || null;
}

export const http = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) => request<T>(path, { method: 'POST', body: body ? JSON.stringify(body) : undefined }),
  patch: <T>(path: string, body?: unknown) => request<T>(path, { method: 'PATCH', body: body ? JSON.stringify(body) : undefined }),
  delete: (path: string) => request(path, { method: 'DELETE' }),
};

export async function issueDevToken(userId: string, organizationId: string, role = 'Owner'): Promise<string> {
  const res = await fetch(`${BASE_URL.replace(/\/api\/v1$/, '')}/DevAuth/token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userId, organizationId, role }),
  });
  if (!res.ok) throw new ApiError(res.status, `DevAuth failed: ${res.statusText}`);
  const data = await res.json();
  return data.token as string;
}
