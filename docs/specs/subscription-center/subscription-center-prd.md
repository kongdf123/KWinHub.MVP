# KWinHub Subscription Center - Product Requirements Document

## Overview
- **Summary**: Implement a V1 sales-managed Subscription Center with plans, immutable plan versions, feature entitlements, quotas, organization overrides, usage metering, audit trail, and customer + platform management UIs.
- **Purpose**: Give KWinHub the authoritative, server-enforced subscription, entitlement and quota layer so that plans, features and limits are provisioned by platform staff and enforced independently of any client-side UI state.
- **Target Users**: Organization Owner/Admin/Coach (customer-facing subscription view), Kunwei Sales/Support (platform provisioning), Kunwei Platform Admin (plans/features/audit).

## Goals
- Add subscription, plan-version, feature-grant, quota, override, usage-ledger and audit persistence via an EF Core migration.
- Expose customer read APIs (subscription summary, usage, entitlements, /me/access) with strict tenant isolation.
- Expose platform mutation APIs (plans, subscriptions, feature overrides, quota overrides, audit) with PlatformAdmin policy, mandatory reason and optimistic concurrency.
- Resolve effective entitlements from immutable plan version + time-bounded overrides; deny wins on conflicting overrides.
- Enforce atomic, idempotent usage metering for `sessions.monthly` and wire it into the real session-creation business operation.
- Add customer-facing Subscription Center page to KWinHub.Web.React settings navigation with real data and robust loading/empty/error states.
- Add plans/subscriptions management to KWinHub.PlatformConsole routing/navigation plus organization entitlement/override UI.
- Add integration tests covering cross-tenant, override expiry, feature deny, concurrent quota and duplicate event idempotency.

## Non-Goals
- No online payment collection (no Stripe, no invoice generation, no auto-renewal).
- No new frontend router or new microservice.
- No automatic deletion of scientific results on subscription expiry.
- No reliance on DEMO_ORG, editable client role state, or request headers for authorization.
- No unbounded (non-expiring) support overrides without explicit reason.

## Background & Context
Inspected current branch state:
- Backend: layered Domain / Application / Infrastructure / Api / Contracts. EF Core + PostgreSQL. `KWinHubDbContext` configures all entities in `OnModelCreating`. Migrations in `KWinHub.Infrastructure/Migrations`. Auto-migrate + dev seed on startup in `Program.cs`.
- Auth: JWT with `org_id`, `org_role`, `platform_admin` claims. `CurrentUserContext` derives `UserId`, `OrganizationId`, `OrganizationRole` from claims. PlatformAdmin policy requires `platform_admin` claim. Single-org-per-token model.
- Existing controllers derive tenant from `_currentUser.OrganizationId` (JWT), not URL. For org-scoped URLs, must compare URL orgId to claim orgId (or allow platform admin).
- Existing customer entities: `Organization`, `OrganizationMembership`, `User`, `Subject`, `MeasurementSession` (has `ExternalId`), `Result`, `Protocol`.
- Session creation (`SessionService.GetOrCreateByExternalIdAsync`) is the natural gated operation for `sessions.monthly` quota; it already uses `ExternalId` for idempotency.
- Frontend `KWinHub.Web.React`: view-switch routing in `App.tsx`, `PRIMARY_NAV` in `lib/navigation.ts`, hardcoded `ROLE_PERMISSIONS` in `lib/app-context.tsx`, `http` client in `lib/api/http.ts` (no abort signal). `ROLE_PERMISSIONS` is UI-only, not production auth.
- Frontend `KWinHub.PlatformConsole`: hash routing in `App.tsx`/`lib/router.tsx`, `NAV_ITEMS` in `components/Layout.tsx`, `api` client with mock-mode toggle.
- Tests: `KWinHub.UnitTests`, `KWinHub.IntegrationTests`, `KWinHub.ContractTests`, `KWinHub.EndToEndTests` projects exist (xunit). Only `SubjectTests.cs` has real tests; others are placeholders.

## Functional Requirements
- **FR-1**: Subscription plan versions with code/name/version, draft/published states; published versions immutable. Plan has feature grants (enabled/disabled per feature key) and quota rules (limit, nullable = unlimited).
- **FR-2**: Organization subscription linking an org to a published plan version, with status (Trialing/Active/PastDue/GracePeriod/Suspended/Expired/Canceled), start/end dates, grace-until, cancel-at-period-end, renewal mode (SalesManaged), revision/rowversion.
- **FR-3**: Feature definitions catalog (stable keys) created via migration seeding.
- **FR-4**: Organization feature overrides (grant/deny, time-bounded, mandatory reason). Deny wins on overlap.
- **FR-5**: Organization quota overrides (signed/nullable limit, time-bounded, reason).
- **FR-6**: Usage ledger (org, metricKey, businessEventId unique, delta, occurredAt, periodStart) and usage aggregates (org, metricKey, periodStart unique, consumed).
- **FR-7**: Subscription audit events (append-only: actor, action, before/after JSON, reason, occurredAt, requestId).
- **FR-8**: Customer GET endpoints: `/organizations/{id}/subscription`, `/usage`, `/entitlements`, `/me/access?organizationId=`. Tenant-isolated: URL orgId must match caller's JWT org_id (or 404/403).
- **FR-9**: Platform endpoints: `/platform/subscription-plans` (list/create/publish), `/platform/subscriptions` (list), `/platform/organizations/{id}/subscription` (PUT assign/change), `/platform/organizations/{id}/entitlements/{featureKey}` (PUT override), `/platform/organizations/{id}/quotas/{quotaKey}` (PUT override), `/platform/organizations/{id}/subscription-audit` (GET history). Require PlatformAdmin policy, mandatory reason, If-Match/rowversion, 409 on conflict.
- **FR-10**: Entitlement resolution: load active subscription + immutable plan version, check subscription status permits capability, apply effective override (deny wins), return enabled/reason/source/effectiveUntil/revision.
- **FR-11**: Atomic/idempotent usage enforcement for `sessions.monthly`: in a single transaction, check ledger for businessEventId (no-op if exists), resolve effective limit, compare consumed+delta, insert ledger + update aggregate, commit. Retry on serialization/unique races. Return 422 `quota_exceeded` when exhausted.
- **FR-12**: Wire quota enforcement into `SessionService` session creation (the real gated operation).
- **FR-13**: `IOrganizationAccessService.AuthorizeAsync(orgId, userId, featureKey, permissionKey, resourceType, resourceId, ct)` returning `AccessDecision(Allowed, DenialCode)`.
- **FR-14**: KWinHub.Web.React: add `subscription` view to settings nav + `ROLE_PERMISSIONS`, `<SubscriptionPage>` fetching real org ID from authenticated context, Overview/Usage/Features tabs, loading/error/empty states, server-aware feature navigation.
- **FR-15**: KWinHub.PlatformConsole: add `/subscription-plans` and `/subscriptions` routes + nav items, plan list/feature/quota display, org subscription + override editor.

## Non-Functional Requirements
- **NFR-1**: Tenant isolation enforced server-side; URL orgId never trusted as proof of membership.
- **NFR-2**: Published plan versions immutable after publish.
- **NFR-3**: No secrets or customer credentials in code or seeds.
- **NFR-4**: Existing navigation, builds and tests remain green.
- **NFR-5**: EF migration applies cleanly to fresh and existing databases.

## Constraints
- **Technical**: EF Core migrations only (no runtime EnsureCreated). PostgreSQL. Use existing DI (`AddApplication`, `AddInfrastructure`). Feature codes stable in centralized catalog.
- **Business**: V1 sales-managed only. Preserve scientific data on expiry.
- **Dependencies**: Existing JWT auth, `CurrentUserContext`, `KWinHubDbContext`, `SessionService`.

## Assumptions
- JWT `org_id` claim is the authoritative tenant for customer APIs; platform admins may access any org via platform endpoints.
- `members.active`, `devices.registered`, `storage.bytes` quotas are tracked but not enforced on real write paths in V1 (only `sessions.monthly` is wired to a real operation); they are returned in usage summaries.
- Organization role-based permission checks are simplified in V1 (member-of-org is the primary gate for customer read APIs); a fuller permission model is deferred.

## Acceptance Criteria

### AC-1: Customer cannot read another organization's subscription
- **Type**: `rule`
- **Given**: A user authenticated as a member of Org A
- **When**: They call `GET /organizations/{OrgBId}/subscription` (or usage/entitlements)
- **Then**: The API returns 403 (or 404 to avoid enumeration) and does not leak Org B data
- **Pass Condition**: Integration test asserts non-2xx for cross-tenant call
- **Evidence**: Integration test output

### AC-2: Platform-admin-only mutation endpoints reject org members
- **Type**: `rule`
- **Given**: An authenticated org member (non platform-admin)
- **When**: They call any `/platform/...` mutation endpoint
- **Then**: The API returns 403
- **Pass Condition**: Integration test asserts 403
- **Evidence**: Integration test output

### AC-3: Feature denial enforced server-side regardless of client
- **Type**: `rule`
- **Given**: An org on a plan without `testing.sprint` (or with explicit deny override)
- **When**: A gated operation requiring `testing.sprint` is invoked
- **Then**: The server rejects with 403/422 even if the frontend is altered
- **Pass Condition**: Test invokes entitlement resolver / access service and observes denial
- **Evidence**: Unit/integration test output

### AC-4: Feature override grant active then expired reverts
- **Type**: `rule`
- **Given**: An org on a plan where feature X is disabled, with an active grant override
- **When**: The override's EndsAt passes
- **Then**: The feature is no longer enabled (reverts to plan default)
- **Pass Condition**: Entitlement resolver test with clock past EndsAt returns disabled
- **Evidence**: Unit test output

### AC-5: Explicit deny override overrides plan grant
- **Type**: `rule`
- **Given**: An org on a plan where feature X is enabled, with an active deny override
- **When**: Entitlement for X is resolved
- **Then**: The feature is disabled (deny wins)
- **Pass Condition**: Resolver returns enabled=false with source Override
- **Evidence**: Unit test output

### AC-6: Concurrent monthly session uploads cannot exceed hard quota
- **Type**: `rule`
- **Given**: An org with `sessions.monthly` limit = N
- **When**: N+1 concurrent session-create requests arrive
- **Then**: Exactly N succeed and at least one fails with 422 `quota_exceeded`; aggregate consumed = N
- **Pass Condition**: Concurrent integration test asserts count
- **Evidence**: Integration test output

### AC-7: Duplicate business event id does not double-count usage
- **Type**: `rule`
- **Given**: An org with remaining quota
- **When**: The same `businessEventId` is submitted twice for `sessions.monthly`
- **Then**: Consumed increases by 1 only; ledger has one row
- **Pass Condition**: Test submits twice and asserts consumed delta = 1
- **Evidence**: Integration test output

### AC-8: Plan version change preserves historical assignments and audit
- **Type**: `rule`
- **Given**: An org subscribed to plan v1
- **When**: A new plan version v2 is published and the org is reassigned
- **Then**: The original v1 subscription record and audit history remain queryable
- **Pass Condition**: Audit endpoint returns prior assignment event
- **Evidence**: Integration test output

### AC-9: Tenant-scoped file/resource operations enforce tenant scope
- **Type**: `rule`
- **Given**: A user of Org A
- **When**: They attempt a resource-scoped operation on an Org B resource
- **Then**: `IOrganizationAccessService` returns denied
- **Pass Condition**: Unit test of AuthorizeAsync with mismatched resource org
- **Evidence**: Unit test output

### AC-10: Failed entitlement dependency defaults to deny gated writes
- **Type**: `rule`
- **Given**: Entitlement store unavailable (simulated)
- **When**: A gated write operation is attempted
- **Then**: The operation is denied (fail closed)
- **Pass Condition**: Service returns denial when resolver throws/unavailable
- **Evidence**: Unit test output

### AC-11: Outdated If-Match returns 409 on platform state change
- **Type**: `rule`
- **Given**: A platform subscription with revision R
- **When**: A PUT includes If-Match with a stale revision
- **Then**: The API returns 409
- **Pass Condition**: Integration test asserts 409
- **Evidence**: Integration test output

### AC-12: Customer Subscription Center shows real plan/status/usage with robust states
- **Type**: `rubric`
- **Dimension**: Frontend subscription page completeness and robustness
- **Scale**: 1-5
- **Anchors**: 1 = broken/missing; 3 = renders plan+usage with basic states; 5 = full Overview/Usage/Features tabs, correct loading/error/empty, no demo IDs, no fabricated payment fields
- **Pass Threshold**: >= 4
- **Evidence**: Code review + build

### AC-13: PlatformConsole plans/subscriptions UI is functional and consistent
- **Type**: `rubric`
- **Dimension**: Platform console subscription management completeness
- **Scale**: 1-5
- **Anchors**: 1 = missing; 3 = plan list + org subscription editor stubbed; 5 = plan versions/features/quotas, org assign + override editor, audit view, all wired to real APIs
- **Pass Threshold**: >= 4
- **Evidence**: Code review + build

### AC-14: Builds and tests pass
- **Type**: `rule`
- **Given**: Clean checkout
- **When**: `dotnet build`, `dotnet test`, `npm run build` (both frontends) are run
- **Then**: All succeed with no errors (pre-existing failures noted separately)
- **Pass Condition**: Commands exit 0
- **Evidence**: Build/test command output

## Open Questions
- None blocking; assumptions above govern V1. Fuller RBAC permission keys and storage/device quota enforcement are deferred to later phases.
