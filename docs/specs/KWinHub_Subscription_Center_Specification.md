# KWinHub Subscription Center — Product & Technical Specification

**Version:** 1.0  
**Date:** 2026-10-28  
**Repository:** https://github.com/kongdf123/KWinHub.MVP  
**Status:** Proposed; not merged or tested in the repository  
**Scope:** V1 sales-managed subscription + effective entitlements + quota management + customer-facing subscription experience

## 1. Executive decision

- **KWinHub.Web.React** owns the customer-facing Subscription Center.
- **KWinHub.PlatformConsole** owns global plans, subscriptions, entitlement provisioning, overrides, and audit.
- **KWinHub.Api** performs authoritative subscription, tenant, permission and quota enforcement.
- Subscription ≠ entitlement ≠ role permission ≠ resource authorization ≠ quota.
- V1 supports internally provisioned contracts and trial periods; it does **not** claim to handle online payments.

## 2. Verified repository integration points

Inspected `main` branch on 2026-10-28:

- `src/frontend/KWinHub.Web.React/src/App.tsx`: renders views using `currentView` switch, no React Router.
- `src/frontend/KWinHub.Web.React/src/lib/navigation.ts`: `PRIMARY_NAV` with `settings` tabs and `WORKSPACE_TABS_BY_VIEW`.
- `src/frontend/KWinHub.Web.React/src/lib/app-context.tsx`: hardcoded `ROLE_PERMISSIONS`, `role` state; suitable for prototype navigation, **not** production authorization.
- `src/frontend/KWinHub.Web.React/src/lib/api/http.ts`: existing `http.get/post/patch/delete`, bearer-token authorization. Its `request` method is internal.
- `src/frontend/KWinHub.Web.React/src/pages/admin/OrganizationPage.tsx`: admin page, currently uses demo organization data.
- `src/frontend/KWinHub.PlatformConsole/src/App.tsx`: separate hash-based routes and `PlatformAdmin` front-end gate.
- `src/frontend/KWinHub.PlatformConsole/src/components/Layout.tsx`: own navigation array.
- `src/backend/KWinHub.Api/Program.cs`: JWT validation, `UseAuthentication`, `UseAuthorization`, `PlatformAdmin` claim policy, EF database migration on startup.

**Implementation instruction:** inspect current branch and resolve drift before applying any patch. Do not overwrite newer changes. Source links are relative to the repository root.

## 3. Product requirements

### 3.1 Personas

| Persona | Scope | Primary jobs |
|---|---|---|
| Organization Owner | Their active organization | View plan, renewal, usage, change requests, invoices when available |
| Organization Admin | Their active organization | View plan, feature availability, limits and organization usage |
| Coach / Analyst | Authorized organization | See feature availability and upgrade explanation, not billing details by default |
| Kunwei Sales/Support | Assigned customers only, per platform staff policy | Provision subscriptions, see contract status, apply approved trial/override |
| Kunwei Platform Admin | Platform scope | Define plans and features; manage and audit any organizational subscription |

### 3.2 Navigation and information architecture

**KWinHub.Web.React / Settings workspace:**

1. Organization
2. Users
3. Roles
4. Subscription Center (`subscription` view)
5. Preferences

**Subscription Center tabs (local sub-tabs, not separate top-level sidebar items):**

- Overview: current plan, status, start/current period/end/renewal fields, cancellation status.
- Usage & Limits: members, registered devices, tests this month, storage; consumed/limit; reset time and units.
- Plan Features: all contracted capabilities, enabled/disabled status, explanations.
- Billing: V1 read-only contract/billing-contact text; hide payment-method/invoice claims unless backed by implemented billing provider.
- History: V1 optional; show audited subscription changes when backend endpoint is ready.

**KWinHub.PlatformConsole routes:**

- `/subscription-plans`: plan list, versions and assignments.
- `/subscriptions`: all organization subscriptions with filters.
- `/organizations/:id`: Subscription and Entitlements section / tab within existing organization detail (preferred first slice).
- `/organizations/:id/entitlements`: optional dedicated screen after complexity warrants it.

### 3.3 Feature requirements and acceptance criteria

| ID | Requirement | Priority | Acceptance |
|---|---|---|---|
| SUB-001 | View current subscription | P0 | Authorized organization user sees server-derived plan/status/period; not demo data |
| SUB-002 | View organization quotas | P0 | Usage and limits show consistent units, timestamps, unlimited handling |
| SUB-003 | View effective features | P0 | Feature list incorporates plan snapshot and time-bounded overrides |
| SUB-004 | Enforce organization boundary | P0 | Caller cannot read another org by modifying URL, ID, or header |
| SUB-005 | Platform provision subscription | P0 | Platform admin can assign approved plan and dates with auditable reason |
| SUB-006 | Platform entitlement overrides | P0 | Explicit grant/deny, reason, expiry, audit; no unbounded support overrides |
| SUB-007 | Platform quota overrides | P0 | Signed integer capacity, unlimited explicit, reason, expiry |
| SUB-008 | Server-authorized navigation | P0 | UI renders based on `/me/access` and page guards, backend independently checks |
| SUB-009 | Subscription expiration policy | P0 | Grace/expired effects match contract; read-only data access policy documented |
| SUB-010 | Retry-safe usage metering | P0 | Duplicate upload/idempotency does not increment quota twice |
| SUB-011 | Upgrade request | P1 | Owner can request upgrade (not directly grant feature) |
| SUB-012 | Stripe or other billing | P1 | Verified webhook + idempotent reconciliation; never trust client callback |
| SUB-013 | Coupons and invoicing | P2 | Deferred |

## 4. Domain semantics

### 4.1 Subscription statuses

`Trialing`, `Active`, `PastDue`, `GracePeriod`, `Suspended`, `Expired`, `Canceled`.

- `Canceled` indicates cancellation completed, not necessarily immediate loss of access; store effective end.
- Plan changes must be effective-dated and versioned; don't silently rewrite past contracts.
- `PastDue` is meaningful only with billing integration or manually managed receivables.
- No invoice/payment fields are invented for sales-managed customers.

### 4.2 Entitlement resolution

For each feature:

1. Load an organization's active/effective subscription and **immutable contracted plan version**.
2. Evaluate whether the subscription's current status allows this capability.
3. Apply any currently effective organization override (explicit grant/deny; deny wins if overlapping conflicts are erroneously present).
4. Return `enabled`, `reason`, `source`, `effectiveUntil` and `revision`.
5. Check user permission and resource scope separately when authorizing action.

Exception: basic account access, account recovery, organization administration, existing-data export and contract/billing view must follow explicit lifecycle policies. Do not inadvertently deny all access because a commercial feature expires.

**Plan entitlements are not menus.** Map UI views to feature codes in a central frontend registry, while the API uses the same canonical feature codes for enforcement.

### 4.3 Effective authorization

`allow = authenticated && activeMembership && tenantScope && featureEntitled && permissionGranted && resourceAllowed && (noApplicableQuota || quotaAdmitted)`.

- `organizationId` from URL/header is a selector, not a proof of membership.
- Role permissions must be checked server-side.
- Never use a hidden sidebar item as an authorization mechanism.
- Platform administrators use separate APIs/policies, with operator impersonation avoided unless explicitly designed, approved and audited.

### 4.4 Quota policy

Codes for MVP: `members.active`, `devices.registered`, `sessions.monthly`, `storage.bytes`.

- `limit=null` = explicitly unlimited; `limit=0` = no capacity.
- Test session usage is counted **once per stable business session ID**, not once per upload attempt/trial.
- Monthly windows use UTC boundaries for consistency (or persist contractual billing timezone, if sold that way). Return `periodStart` and `periodEnd` in UTC.
- For new writes, enforce quota atomically using a transaction/reservation/idempotency ledger; UI counters can lag but authorization may not.
- At-limit behavior is action-specific: reads of existing data do not count against new-write quota.
- Storage counts committed managed objects; staging uploads must have expiration/cleanup and reserved capacity.

## 5. Proposed persistence model (PostgreSQL/EF Core)

Canonical table suggestions. Adapt naming and base entity conventions to repository.

| Table | Key fields | Constraints |
|---|---|---|
| SubscriptionPlans | Id, Code, Name, Version, IsPublished, CreatedAt | unique(Code,Version); published versions immutable |
| PlanFeatures | PlanId, FeatureKey, Enabled | PK(PlanId, FeatureKey) |
| PlanQuotas | PlanId, QuotaKey, LimitValue nullable | PK(PlanId, QuotaKey); limit >= 0 or null |
| OrganizationSubscriptions | Id, OrganizationId, PlanId, Status, StartsAt, EndsAt, GraceUntil, CancelAtPeriodEnd, CreatedAt, RowVersion | FK organization/plan, no ambiguous overlapping effective subscriptions |
| FeatureDefinitions | Key, Category, DisplayName, DefaultPolicy | unique Key; created via controlled migration/catalog seeding |
| OrganizationFeatureOverrides | Id, OrganizationId, FeatureKey, Enabled, StartsAt, EndsAt, Reason, CreatedBy | overlapping conflicting overrides prohibited |
| OrganizationQuotaOverrides | Id, OrganizationId, QuotaKey, LimitValue nullable, StartsAt, EndsAt, Reason, CreatedBy | same overlap rule |
| UsageLedger | Id, OrganizationId, MetricKey, BusinessEventId, Delta, OccurredAt, PeriodStart | unique(OrgId,MetricKey,BusinessEventId) |
| UsageAggregates | OrganizationId, MetricKey, PeriodStart, PeriodEnd, Consumed, UpdatedAt | unique(OrgId,MetricKey,PeriodStart) |
| SubscriptionAuditEvents | Id, OrgId, ActorId, Action, BeforeJson, AfterJson, Reason, OccurredAt, RequestId | immutable append-only |

Keep existing user, org, membership and device tables; **do not duplicate** those identities. `OrganizationSubscriptions` may need a partial exclusion constraint, transaction-based overlap validation, or serializable transaction policy to avoid concurrent active assignments. Always migrate via EF migrations, no runtime `EnsureCreated`.

### 5.1 Suggested database indexes

- `(OrganizationId, StartsAt, EndsAt)` on subscriptions.
- `(OrganizationId, FeatureKey, StartsAt, EndsAt)` on feature overrides.
- `(OrganizationId, QuotaKey, StartsAt, EndsAt)` on quota overrides.
- `(OrganizationId, MetricKey, PeriodStart)` on usage aggregates.
- Unique `(OrganizationId, MetricKey, BusinessEventId)` on ledger.
- `(OrganizationId, OccurredAt DESC)` on audit records.

## 6. HTTP API contracts

Version prefix is already supplied by frontend `http` base `/api/v1`. Paths below exclude that prefix inside frontend methods.

### 6.1 Customer endpoints

| Method | Route | Permission |
|---|---|---|
| GET | `/organizations/{organizationId}/subscription` | member + `subscription.view` |
| GET | `/organizations/{organizationId}/usage` | member + `subscription.usage.view` |
| GET | `/organizations/{organizationId}/entitlements` | member, filtered public entitlements |
| GET | `/me/access?organizationId={organizationId}` | member; server derives effective grants |
| POST | `/organizations/{organizationId}/upgrade-requests` | `subscription.manage`; P1 |

Response `GET /organizations/{id}/subscription`:

```json
{
  "organizationId": "11111111-1111-1111-1111-111111111111",
  "subscriptionId": "22222222-2222-2222-2222-222222222222",
  "plan": { "code": "performance-pro", "name": "Performance Pro", "version": 1 },
  "status": "Active",
  "startsAt": "2026-10-01T00:00:00Z",
  "currentPeriodStart": "2026-10-01T00:00:00Z",
  "currentPeriodEnd": "2026-11-01T00:00:00Z",
  "graceUntil": null,
  "cancelAtPeriodEnd": false,
  "renewalMode": "SalesManaged",
  "currency": null,
  "amountMinor": null,
  "revision": 8
}
```

Response `GET /organizations/{id}/usage`:

```json
{
  "organizationId": "11111111-1111-1111-1111-111111111111",
  "asOf": "2026-10-28T09:00:00Z",
  "items": [
    { "key": "members.active", "consumed": 8, "limit": 25, "unit": "members", "periodStart": null, "periodEnd": null },
    { "key": "sessions.monthly", "consumed": 428, "limit": 1000, "unit": "sessions", "periodStart": "2026-10-01T00:00:00Z", "periodEnd": "2026-11-01T00:00:00Z" }
  ]
}
```

Response `GET /me/access`:

```json
{
  "organizationId": "11111111-1111-1111-1111-111111111111",
  "permissions": ["subscription.view", "athletes.read", "sessions.read"],
  "features": { "athletes.management": true, "testing.forceplate": true, "testing.sprint": false },
  "effectiveStatus": "Active",
  "revision": 8
}
```

### 6.2 Platform endpoints

| Method | Route | Use |
|---|---|---|
| GET | `/platform/subscription-plans` | list published/draft plans |
| POST | `/platform/subscription-plans` | create draft plan version |
| POST | `/platform/subscription-plans/{id}/publish` | publish immutable plan version |
| GET | `/platform/subscriptions` | filter/search active and historical records |
| PUT | `/platform/organizations/{id}/subscription` | transactional assignment/change with audit |
| PUT | `/platform/organizations/{id}/entitlements/{featureKey}` | update a single time-bound override |
| PUT | `/platform/organizations/{id}/quotas/{quotaKey}` | update time-bound quota override |
| GET | `/platform/organizations/{id}/subscription-audit` | history |

Sensitive mutations require `PlatformAdmin` or narrower platform staff policy, validation, mandatory reason, idempotency key where relevant and `If-Match`/row-version to avoid lost updates. Return 409 on conflicts. Do not allow customer APIs to mutate entitlements.

### 6.3 Error semantics

- 401 unauthenticated / invalid token.
- 403 authenticated but forbidden: missing membership, feature, permission, or platform policy.
- 404 nonexistent resource; to avoid enumeration, sensitive resource lookups may use 404 for foreign tenants.
- 409 conflict: stale subscription revision, duplicate effective assignment, concurrent override.
- 422 quota exhausted or business constraint invalid; use ProblemDetails and code such as `quota_exceeded`.
- 503 entitlement store temporarily unavailable; fail closed for gated operations, permit explicitly designated basic/offline fallback workflows.

ProblemDetails includes stable machine-readable `code`, `traceId` and optional `retryAfter` or `quota` details. Don't leak another org's information.

## 7. Backend module design (.NET)

Preferred location by existing architectural layers:

```text
src/backend/
  KWinHub.Domain/
    Subscriptions/{SubscriptionPlan,OrganizationSubscription,FeatureOverride,QuotaOverride}.cs
  KWinHub.Application/
    Subscriptions/{DTOs,Interfaces,Handlers,Policies}.cs
    Authorization/{IOrganizationAccessService,AccessDecision}.cs
  KWinHub.Infrastructure/
    Persistence/{EntityConfigurations and new migration}
    Subscriptions/{SubscriptionService,EntitlementResolver,UsageMeter}.cs
  KWinHub.Api/
    Controllers/{OrganizationSubscriptionsController,PlatformSubscriptionsController}.cs
```

Use existing project DI convention (`AddApplication`, `AddInfrastructure`) rather than registering ad hoc services throughout `Program.cs`.

**Reference interface:**

```csharp
public interface IOrganizationAccessService
{
    Task<AccessDecision> AuthorizeAsync(
        Guid organizationId,
        Guid userId,
        string featureKey,
        string permissionKey,
        string? resourceType,
        Guid? resourceId,
        CancellationToken ct);
}

public sealed record AccessDecision(bool Allowed, string? DenialCode);
```

**Policy approach:** authenticate first; validate membership; derive organization ID from authorized request context; resolve plan/override; check actual permission, scope and applicable quota. Put authorization in application services or authorization handlers, not only controller attributes. Enforce object-store file access, export, processing jobs and sync ingestion through the same contract.

**Atomic usage pseudocode:**

```csharp
// In a single database transaction:
// 1) Resolve & lock current quota aggregate / reservation row.
// 2) Check if BusinessEventId already exists in UsageLedger; if yes, no-op.
// 3) Resolve effective limit and compare consumed + requestedDelta.
// 4) Insert unique ledger record, update aggregate, persist business write.
// 5) Commit. Retry on serialization/unique-key races.
```

Preserve past scientific test data during subscription plan changes. Never delete test results automatically as a side effect of expiration.

## 8. Frontend integration (KWinHub.Web.React)

### 8.1 Minimum changes

1. Add `'subscription'` view to `ROLE_PERMISSIONS['org-admin']` and, if desired, Owner when the actual identity/role model supports it. Give Coach a feature overview only through separate permission if required.
2. Add `{ view: 'subscription', labelKey: 'nav.subscription' }` under `settings.tabs`, append it to `matchViews`.
3. Add translation key in existing i18n dictionaries.
4. Add `<SubscriptionPage />` in `App.tsx` switch.
5. Implement API client and typed DTOs in `features/subscriptions/`.
6. Implement `SubscriptionPage` fetching current organization's server-verified ID. **Do not use `DEMO_ORG.id` for live subscription calls.**
7. Add link from Organization / Administration overview.
8. Upgrade `canAccess` to incorporate server-provided access grants; separate subscription-view permission from feature gating.
9. Keep loading/error/empty states and avoid claiming payment functionality not yet implemented.

### 8.2 Client-side feature registry example

```typescript
export const VIEW_FEATURES: Partial<Record<string, string>> = {
  'biomechanics': 'analytics.advanced',
  'integrations': 'integrations.api',
  'reports': 'reports.export',
};

export function canOpenView(
  view: string,
  allowedViews: ReadonlySet<string>,
  features: Readonly<Record<string, boolean>>,
): boolean {
  if (!allowedViews.has(view)) return false;
  const feature = VIEW_FEATURES[view];
  return !feature || features[feature] === true;
}
```

**Caution:** The existing role model is demo-oriented. Do not rely on `role` selected in client state for real security. Backend must independently authorize all operations.

### 8.3 TypeScript API client (ready to adapt)

```typescript
import { http } from '@/lib/api/http';

export type SubscriptionStatus =
  | 'Trialing' | 'Active' | 'PastDue' | 'GracePeriod'
  | 'Suspended' | 'Expired' | 'Canceled';

export interface SubscriptionSummary {
  organizationId: string;
  subscriptionId: string;
  plan: { code: string; name: string; version: number };
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

const orgPath = (id: string) => `/organizations/${encodeURIComponent(id)}`;

export const subscriptionApi = {
  getSummary: (orgId: string) =>
    http.get<SubscriptionSummary>(`${orgPath(orgId)}/subscription`),
  getUsage: (orgId: string) =>
    http.get<UsageSummary>(`${orgPath(orgId)}/usage`),
  getEntitlements: (orgId: string) =>
    http.get<{ items: EntitlementItem[] }>(`${orgPath(orgId)}/entitlements`),
};
```

### 8.4 React page baseline

```tsx
import { useEffect, useState } from 'react';
import { subscriptionApi, type SubscriptionSummary, type UsageSummary } from './api';

type Props = { organizationId: string };

export function SubscriptionPage({ organizationId }: Props) {
  const [summary, setSummary] = useState<SubscriptionSummary | null>(null);
  const [usage, setUsage] = useState<UsageSummary | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let live = true;
    setSummary(null); setUsage(null); setError(null); setLoading(true);
    Promise.all([
      subscriptionApi.getSummary(organizationId),
      subscriptionApi.getUsage(organizationId),
    ]).then(([s, u]) => {
      if (!live) return;
      setSummary(s); setUsage(u);
    }).catch((e: unknown) => {
      if (live) setError(e instanceof Error ? e.message : 'Subscription unavailable');
    }).finally(() => { if (live) setLoading(false); });
    return () => { live = false; };
  }, [organizationId]);

  if (loading) return <p role="status">Loading subscription…</p>;
  if (error) return <p role="alert">{error}</p>;
  if (!summary) return <p>No subscription found.</p>;

  return (
    <section className="space-y-6 p-6">
      <h1 className="text-xl font-semibold">Subscription Center</h1>
      <article className="rounded-xl border p-5">
        <h2 className="font-semibold">{summary.plan.name}</h2>
        <p>Status: {summary.status}</p>
        <p>Period end: {new Date(summary.currentPeriodEnd).toLocaleDateString()}</p>
      </article>
      <article className="rounded-xl border p-5">
        <h2 className="font-semibold">Usage</h2>
        {usage?.items.map(item => (
          <div key={item.key} className="py-2">
            <span>{item.key}: {item.consumed} / {item.limit ?? 'Unlimited'}</span>
            {item.limit !== null && item.limit > 0 && (
              <progress className="block w-full" max={item.limit} value={Math.min(item.consumed, item.limit)} />
            )}
          </div>
        ))}
      </article>
    </section>
  );
}
```

The example above is a **baseline**, not the complete screen. When integrating, add existing `TopBar`, workspace tabs, translation keys, fully accessible states, entitlements listing, and selected organization resolution. The current `http.get` does not accept an `AbortSignal`; the sample uses a `live` guard. Extend the shared HTTP utility with abort signals if desired. Do not pass a local demo organization to the live service.

### 8.5 Exact routing changes (adapt in the real branch)

`App.tsx`:

```tsx
import { SubscriptionPage } from '@/pages/admin/SubscriptionPage';
// In renderView switch:
case 'subscription': return <SubscriptionPage />;
```

If using the sample component with mandatory `organizationId`, create a small page wrapper that obtains the verified organization ID from the authenticated context and passes it as a prop. That context must be implemented/verified first; do not copy the sample verbatim and invent a demo ID.

`navigation.ts` under the existing `settings` nav item:

```typescript
{ view: 'subscription', labelKey: 'nav.subscription' },
// and add 'subscription' to its matchViews
```

`app-context.tsx` temporary addition to role view list:

```typescript
'org-admin': [
  // existing views...
  'subscription',
],
```

**Important:** This temporary role-view addition is UI-only. Production should fetch actual server permissions and protect read/write APIs.

## 9. PlatformConsole implementation

Current `App.tsx` uses `resolveRoute` and `useHashRoute`; current `Layout.tsx` has `NAV_ITEMS`. Add `SubscriptionsPage` and `SubscriptionPlansPage` using those conventions, and i18n keys in existing `lib/i18n`.

### 9.1 Plan list

- Draft version, published version, archived version.
- Display feature grants and quota rules.
- Creating a new version must not alter organizations bound to an older version.
- Publishing is a privileged, audited action.

### 9.2 Organization subscription tab

- Current plan, status, dates, renewal mode, effective features and quotas.
- Change plan with confirmation, effective date, mandatory audit reason, optimistic concurrency.
- Create grant/deny override, optional expiry (mandatory expiry for temporary support grants), explanation.
- Changes reflected in customer read API after cache invalidation.

### 9.3 Operational guardrails

- No silent entitlement mutations.
- Explicit distinction between plan default and override.
- Audited `before/after` snapshots.
- Support staff may have read-only access unless granted a narrower change privilege.
- Never expose billing provider secrets to frontend.

## 10. Lifecycle and device/offline behavior

- Expiration prevents new **cloud-only gated operations** after applicable grace period.
- Existing measurements/history should remain readable/exportable for a documented retention window.
- Local device data collection and local basic calculation should not be rendered inoperative just because cloud connectivity fails; respect device sales/license contract.
- Sync client uses last known state for display only. The server decides whether to accept new cloud uploads.
- If offline uploads were captured before expiry, specify separately whether grace permits delayed synchronization (recommended: configurable contractual window).
- Never delete athlete data automatically on a single payment or renewal failure.

## 11. Security and acceptance testing

**API tests (must pass):**

1. Org A user requests Org B subscription → denied; no cross-tenant leakage.
2. Org Admin accesses platform plan mutation route → denied.
3. Valid Plan A without `testing.sprint` → Sprint service rejects gated operation even if frontend is manually altered.
4. Feature override grant active → enabled; after expiry → reverted.
5. Explicit deny override overrides plan grant.
6. Expired/canceled/grace statuses enforce documented action-specific policy.
7. Concurrent monthly session uploads cannot exceed hard quota.
8. Same client event ID retried → no duplicate measurement or usage charge.
9. Plan version change preserves historical assignments and audit trace.
10. Tenant-specific file download and background processing both enforce tenant scope.
11. Failed entitlement dependency defaults to deny gated writes (not silently grant).
12. Platform state change with outdated `If-Match` returns 409.

**Frontend tests:**

- Settings workspace contains Subscription Center for authorized users.
- Correct loading/error/empty states.
- Current plan and quotas match mocked API contracts.
- `limit: null` renders unlimited, `limit: 0` renders no capacity.
- Never display fabricated invoice/payment fields for sales-managed subscription.
- After changing organization, previous organization's subscription is not displayed while loading.
- Unauthorized feature pages cannot be opened via direct view navigation; API still checks independently.

## 12. Delivery sequence

### Phase 0: Architectural guardrails

- Confirm org membership and selected-org context in authenticated backend.
- Define feature/permission/quota keys and policies.
- Fix any tenant-scoping gaps, document expiration behavior.

### Phase 1: Backend contracts and data

- EF entities/migrations/seeds, plan versions, subscriptions, overrides, resolver.
- Customer read APIs and integration tests.
- Platform mutation APIs with audit and concurrency.

### Phase 2: Customer-facing React

- Subscription page and admin navigation, current org from authenticated context.
- Overview / Usage / Features with real APIs.
- Server-driven access integration and tests.

### Phase 3: PlatformConsole

- Plan list, organization subscription editor, override editor, audit view.
- Confirm cross-frontend consistency.

### Phase 4: Production hardening

- Atomic quota usage for actual ingestion workflows.
- Caching and invalidation, monitoring, backup, billing roadmap.
- End-to-end demo of organization provisioning → entitlements → user view → API rejection/approval.

## 13. Explicit non-goals for V1

- No customer payment collection.
- No claims of automatic renewal by Stripe.
- No invoice PDF generation.
- No end-user arbitrary RBAC policy editor unless it exists separately and is validated.
- No new microservices, no Kubernetes required.
- No automatic deletion of data on expiration.

## 14. Definition of Done

- All new functionality has real API-backed behavior, except explicitly marked storybook/mock demos.
- Both frontends show consistent effective plan and entitlement data.
- Server denies unauthorized feature and resource usage regardless of client.
- Multi-tenant and quota concurrency tests are green.
- EF migration applies cleanly to fresh and existing databases.
- Existing app navigation and tests remain green.
- No secrets, billing credentials, or customer-specific demo fixtures introduced into production.
- `npm run build`, TypeScript checks, .NET build and relevant tests pass in CI.

## 15. Instruction block to give to coding AI

> Implement the KWinHub Subscription Center feature according to this document against the current repository branch. Use the existing React view-switch routing in `KWinHub.Web.React` and hash routing in `KWinHub.PlatformConsole`; do not introduce a third frontend or router without a demonstrated need. First inspect organization identity/membership, EF entities, DI and existing API patterns. Implement database migration, customer read APIs, platform mutation APIs, server entitlements and quota enforcement, then React pages. Respect current repository conventions, migrations, translations and tests. Don't use demo organization IDs for live APIs. Use sales-managed subscription for V1; do not invent invoices or integrate a payment provider. Keep tenant isolation and API enforcement authoritative. Add tests for cross-tenant access, feature denial, overrides, concurrent quota use and idempotent retries. Run the repository's build and test commands and report exactly which passed and failed. Deliver changes as reviewable commits/PRs with no secrets.

---

### Implementation notes

This document is an implementation blueprint grounded in the inspected source files; proposed entities, screens and APIs do **not** yet exist merely because they appear here. Sample code requires fitting to actual EF, authentication and organization-context primitives on the current branch.
