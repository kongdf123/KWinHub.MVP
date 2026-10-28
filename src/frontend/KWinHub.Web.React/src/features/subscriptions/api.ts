import { http } from '@/lib/api/http';

export type SubscriptionStatus =
  | 'Trialing'
  | 'Active'
  | 'PastDue'
  | 'GracePeriod'
  | 'Suspended'
  | 'Expired'
  | 'Canceled';

export interface PlanSummary {
  code: string;
  name: string;
  version: number;
}

export interface SubscriptionSummary {
  organizationId: string;
  subscriptionId: string;
  plan: PlanSummary;
  status: SubscriptionStatus;
  startsAt: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  graceUntil: string | null;
  cancelAtPeriodEnd: boolean;
  renewalMode: 'SalesManaged' | 'AutoRenew';
  currency: string | null;
  amountMinor: number | null;
  revision: number;
}

export interface UsageItem {
  key: string;
  consumed: number;
  limit: number | null;
  unit: string;
  periodStart: string | null;
  periodEnd: string | null;
}

export interface UsageSummary {
  organizationId: string;
  asOf: string;
  items: UsageItem[];
}

export interface EntitlementItem {
  featureKey: string;
  enabled: boolean;
  source: 'Plan' | 'Override' | 'LifecyclePolicy';
  effectiveUntil: string | null;
  reason?: string;
}

export interface EntitlementsResponse {
  organizationId: string;
  items: EntitlementItem[];
}

export interface AccessResponse {
  organizationId: string;
  permissions: string[];
  features: Record<string, boolean>;
  effectiveStatus: string;
  revision: number;
}

const orgPath = (id: string) => `/organizations/${encodeURIComponent(id)}`;

export const subscriptionApi = {
  getSummary: (orgId: string) =>
    http.get<SubscriptionSummary>(`${orgPath(orgId)}/subscription`),
  getUsage: (orgId: string) =>
    http.get<UsageSummary>(`${orgPath(orgId)}/usage`),
  getEntitlements: (orgId: string) =>
    http.get<EntitlementsResponse>(`${orgPath(orgId)}/entitlements`),
  getAccess: () =>
    http.get<AccessResponse>('/me/access'),
};
