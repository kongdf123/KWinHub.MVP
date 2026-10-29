# KWinHub Subscription Center - Implementation Plan

## Task 1: Domain entities for subscriptions, plans, features, quotas, overrides, usage, audit
- **Status**: `pending`
- **Priority**: high
- **Depends On**: None
- **Description**:
  - Add to `KWinHub.Domain`: `Subscriptions/SubscriptionPlan`, `PlanFeature`, `PlanQuota`, `OrganizationSubscription`, `FeatureDefinition`, `OrganizationFeatureOverride`, `OrganizationQuotaOverride`, `UsageLedgerEntry`, `UsageAggregate`, `SubscriptionAuditEvent`.
  - Enums: `SubscriptionStatus`, `PlanStatus` (Draft/Published), `RenewalMode` (SalesManaged), `EntitlementSource`.
  - Immutable published plan versions (no mutation after publish).
  - Follow existing `BaseEntity<Guid>` and private-constructor + factory conventions.
- **Acceptance Criteria Addressed**: AC-3, AC-4, AC-5, AC-8
- **Test Requirements**:
  - `rule` TR-1.1: Published plan rejects further state/feature/quota mutations (throws InvalidOperationException). Evidence: unit test.
  - `rule` TR-1.2: Overlap-conflicting feature overrides are rejected on construction. Evidence: unit test.
- **Notes**: Keep entities persistence-agnostic; EF config in Task 2.

## Task 2: EF Core entity configurations, DbContext DbSets, and migration
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 1
- **Description**:
  - Add `DbSet<>` properties and `OnModelCreating` config for all new entities in `KWinHubDbContext`.
  - Indexes: `(OrganizationId, StartsAt, EndsAt)` on subscriptions; `(OrgId, FeatureKey, StartsAt, EndsAt)` feature overrides; `(OrgId, QuotaKey, StartsAt, EndsAt)` quota overrides; `(OrgId, MetricKey, PeriodStart)` aggregates; unique `(OrgId, MetricKey, BusinessEventId)` ledger; `(OrgId, OccurredAt DESC)` audit.
  - Unique `(Code, Version)` on plans; published immutability enforced in domain.
  - Generate migration `AddSubscriptionsAndEntitlements` via `dotnet ef migrations add`.
  - Seed feature definitions (`members.active`, `devices.registered`, `sessions.monthly`, `storage.bytes`, and feature keys like `testing.sprint`, `analytics.advanced`, `integrations.api`, `reports.export`) in `DatabaseSeeder`.
- **Acceptance Criteria Addressed**: AC-3, AC-8, AC-14
- **Test Requirements**:
  - `rule` TR-2.1: `dotnet ef migrations script` produces valid SQL with all tables/indexes. Evidence: command output.
  - `rule` TR-2.2: Migration applies to a fresh database via `db.Database.MigrateAsync`. Evidence: integration test or manual run.
- **Notes**: No runtime EnsureCreated. Verify snapshot updates.

## Task 3: Application contracts (DTOs) and repository interfaces
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 1
- **Description**:
  - Add Contracts DTOs in `KWinHub.Contracts/Subscriptions` and `KWinHub.Contracts/Platform` matching the spec response shapes (SubscriptionSummary, UsageSummary/UsageItem, EntitlementItem, AccessResponse, plan/subscription/override requests and responses).
  - Add `ISubscriptionRepository`, `IUsageMeter`, `IEntitlementResolver` interfaces in `KWinHub.Application/Subscriptions`.
- **Acceptance Criteria Addressed**: AC-1, AC-3, AC-8
- **Test Requirements**:
  - `rule` TR-3.1: DTO shapes match spec JSON examples (fields, nullability). Evidence: code review.

## Task 4: Infrastructure repositories + EntitlementResolver + UsageMeter
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 2, Task 3
- **Description**:
  - Implement `SubscriptionRepository` (plans, subscriptions, overrides, audit) using `KWinHubDbContext`.
  - Implement `EntitlementResolver`: load active subscription + plan version, evaluate status, apply effective override (deny wins), return enabled/source/reason/effectiveUntil/revision. Use UTC `now` injected for testability.
  - Implement `UsageMeter`: atomic transaction — check ledger for (org, metricKey, businessEventId); if exists no-op; resolve effective limit (plan + override); compare consumed+delta; insert ledger row; upsert aggregate; commit. Retry on DbUpdateConcurrency/unique. Throw `QuotaExceededException` when exhausted.
  - Register in `AddInfrastructure`.
- **Acceptance Criteria Addressed**: AC-3, AC-4, AC-5, AC-6, AC-7, AC-10
- **Test Requirements**:
  - `rule` TR-4.1: Resolver returns enabled=false for plan-disabled feature. Evidence: unit test.
  - `rule` TR-4.2: Resolver returns enabled=true for active grant override on plan-disabled feature. Evidence: unit test.
  - `rule` TR-4.3: Resolver returns enabled=false after grant override expires. Evidence: unit test with injected clock.
  - `rule` TR-4.4: Resolver deny override wins over plan grant. Evidence: unit test.
  - `rule` TR-4.5: UsageMeter no-ops on duplicate businessEventId. Evidence: unit/integration test.
  - `rule` TR-4.6: UsageMeter throws QuotaExceeded when consumed+delta > limit. Evidence: unit test.
- **Notes**: Inject an `IClock` (or `TimeProvider`) for deterministic expiry tests.

## Task 5: IOrganizationAccessService and SubscriptionService application layer
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 4
- **Description**:
  - Implement `IOrganizationAccessService.AuthorizeAsync` in Application: validate membership (orgId vs userId), resolve feature entitlement, return `AccessDecision(Allowed, DenialCode)`. Fail closed on resolver errors for gated writes.
  - Implement `ISubscriptionService` for customer reads (get summary, usage, entitlements, /me/access) deriving org from caller and enforcing URL==claim match.
  - Implement `IPlatformSubscriptionService` for plan CRUD/publish, subscription assign/change, override PUT, audit list. Mandatory reason, rowversion concurrency (409 on mismatch), audit append.
  - Register in `AddApplication`.
- **Acceptance Criteria Addressed**: AC-1, AC-2, AC-8, AC-9, AC-10, AC-11
- **Test Requirements**:
  - `rule` TR-5.1: AuthorizeAsync denies when user is not a member of the org. Evidence: unit test.
  - `rule` TR-5.2: AuthorizeAsync denies when feature not entitled. Evidence: unit test.
  - `rule` TR-5.3: AuthorizeAsync fails closed (denies) when resolver throws. Evidence: unit test.
  - `rule` TR-5.4: Subscription summary enforces URL orgId == caller orgId (else null/denied). Evidence: unit test.

## Task 6: API controllers (customer + platform)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 5
- **Description**:
  - `OrganizationSubscriptionsController`: `[Authorize]`, GET `/organizations/{id}/subscription`, `/usage`, `/entitlements`; GET `/me/access`. Enforce `id == _currentUser.OrganizationId` (or platform admin). Use service layer.
  - `PlatformSubscriptionsController`: `[Authorize(Policy="PlatformAdmin")]`, plan list/create/publish, subscription list, PUT org subscription (If-Match), PUT entitlement override, PUT quota override, GET audit. Return 409 on concurrency conflict, 422 for quota/business.
  - Map exceptions to ProblemDetails with stable `code`.
- **Acceptance Criteria Addressed**: AC-1, AC-2, AC-11
- **Test Requirements**:
  - `rule` TR-6.1: Cross-tenant customer GET returns 403/404. Evidence: integration test.
  - `rule` TR-6.2: Non-platform-admin hitting platform endpoint returns 403. Evidence: integration test.
  - `rule` TR-6.3: Stale If-Match on PUT subscription returns 409. Evidence: integration test.

## Task 7: Wire quota enforcement into SessionService (real gated operation)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 5
- **Description**:
  - In `SessionService.GetOrCreateByExternalIdAsync`, before creating a new session, call `IUsageMeter.TryConsumeAsync(orgId, "sessions.monthly", businessEventId=externalId or sessionId, delta=1)`. On `QuotaExceededException`, surface 422. Existing session (duplicate externalId) returns existing without metering (idempotent).
  - Use the session ExternalId as the business event id when present; otherwise use the new session Id.
- **Acceptance Criteria Addressed**: AC-6, AC-7
- **Test Requirements**:
  - `rule` TR-7.1: Creating a session beyond monthly limit returns 422 and no session is persisted. Evidence: integration test.
  - `rule` TR-7.2: Replaying the same ExternalId does not increment consumed. Evidence: integration test.

## Task 8: KWinHub.Web.React Subscription Center page + navigation
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 6
- **Description**:
  - Add `features/subscriptions/api.ts` (typed client + DTOs) and `features/subscriptions/SubscriptionPage.tsx` (Overview/Usage/Features tabs, loading/error/empty, no demo IDs, no fabricated payment fields).
  - Add `subscription` to settings `PRIMARY_NAV` tabs + `matchViews` in `navigation.ts`.
  - Add `subscription` to `ROLE_PERMISSIONS['org-admin']` (and owner if role model supports) in `app-context.tsx`.
  - Add `case 'subscription'` in `App.tsx` renderView.
  - Add translation keys `nav.subscription` and page strings in `i18n.ts`.
  - Resolve organization ID from authenticated context (not DEMO_ORG). Use existing `http` client.
  - Add `VIEW_FEATURES` registry and `canOpenView` helper; upgrade `canAccess` to consult server features where available.
- **Acceptance Criteria Addressed**: AC-12
- **Test Requirements**:
  - `rubric` TR-8.1: Page completeness; scale 1-5; anchors 1=missing 3=basic 5=full tabs+states; threshold >=4; evidence: build + code review.
  - `rule` TR-8.2: No DEMO_ORG.id used in subscription API calls. Evidence: grep.

## Task 9: KWinHub.PlatformConsole plans + subscriptions UI
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 6
- **Description**:
  - Add `SubscriptionPlansPage` and `SubscriptionsPage`; wire routes `/subscription-plans`, `/subscriptions` in `App.tsx` resolveRoute.
  - Add nav items to `Layout.tsx` `NAV_ITEMS`.
  - Add organization entitlement/override section to `OrganizationDetailPage` (or dedicated component) showing current plan, status, features, quotas, and override editor.
  - Add API client types/methods in `lib/api.ts` for plans, subscriptions, overrides, audit.
  - Add i18n keys.
- **Acceptance Criteria Addressed**: AC-13
- **Test Requirements**:
  - `rubric` TR-9.1: Platform console completeness; scale 1-5; anchors 1=missing 3=stub 5=full wired; threshold >=4; evidence: build + code review.

## Task 10: Integration tests (cross-tenant, overrides, quota concurrency, idempotency, If-Match)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 6, Task 7
- **Description**:
  - Add xunit integration tests in `KWinHub.IntegrationTests` using an in-memory/real DbContext (or Testcontainers if available; else use a shared scoped DbContext with SQLite in-memory or the configured PG). Seed two orgs + users + memberships + plans.
  - Tests: cross-tenant subscription read denied; org member denied platform mutation; feature deny enforced; override grant active+expired; deny overrides plan; concurrent quota (N+1); duplicate event idempotency; plan version change preserves audit; stale If-Match 409.
- **Acceptance Criteria Addressed**: AC-1 through AC-11
- **Test Requirements**:
  - `rule` TR-10.1: All listed integration tests pass. Evidence: `dotnet test` output.

## Task 11: Builds, migrations, and final verification
- **Status**: `pending`
- **Priority**: high
- **Depends On**: Task 8, Task 9, Task 10
- **Description**:
  - Run `dotnet build`, `dotnet test` (solution), `npm run build` in both frontend dirs. Fix any breakage.
  - Run migration against a fresh database and confirm success.
  - Produce final output: source inventory, migration name + rollback notes, test/build results, config notes, known limitations.
- **Acceptance Criteria Addressed**: AC-14
- **Test Requirements**:
  - `rule` TR-11.1: `dotnet build` exits 0. Evidence: command output.
  - `rule` TR-11.2: `dotnet test` exits 0 (excluding pre-existing unrelated failures). Evidence: command output.
  - `rule` TR-11.3: Both frontend `npm run build` exit 0. Evidence: command output.
