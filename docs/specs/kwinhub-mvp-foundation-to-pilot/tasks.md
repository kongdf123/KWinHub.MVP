# KWinHub MVP — Implementation Plan (Milestone Queue)

Milestones follow the handoff's foundation-first sequence and backlog dependency map:

- **M0 — Two-API foundation** (E0-07/09/10/12, E3 prep): build green, split hosts/contexts/schemas, dual audiences, boundary tests, CI. *(atomically detailed below — starts after approval)*
- **M1 — Identity, membership & tenant enforcement** (E1, E0-11)
- **M2 — Organization workspace: athletes, groups, source identity** (E2 + E3-01..04 domain)
- **M3 — Session/trial/metric ingestion loop** (E3-05..10)
- **M4 — Assets & reliable media pipeline** (E4)
- **M5 — History, comparison, plans & reports** (E5, E6)
- **M6 — PlatformConsole cutover & platform administration** (E7)
- **M7 — Security, migration, observability, pilot readiness** (E8)

M0 tasks are atomic and ready to execute. M1–M7 are tracked as vertical slices (each independently demonstrable) and will be decomposed into atomic tasks when the milestone begins, per the approved milestone cadence (Q-8).

---

## Milestone M0 — Two-API Foundation

### Task M0-1: Resolve .NET toolchain and establish green baseline
- **Status**: `completed`
- **Priority**: high
- **Depends On**: None (user approved plan 2026-11-03 → recommended option: install .NET 10 SDK, user-scope)
- **Completion Evidence**:
  - TR-M0-1.1 `rule` PASS: Installed official .NET SDK 10.0.402 (SHA512 verified `ae4fd5…98cd`) user-scope at `C:\Users\Kent\.local\dotnet` (machine had only 8.0/9.0; no admin used).
  - Fixed pre-existing repo defect: `tests/KWinHub.IntegrationTests/KWinHub.IntegrationTests.csproj` had a double UTF-8 BOM causing MSB4025; rewrote with single valid header.
  - `dotnet restore` → OK; `dotnet build KWinHub.sln` → **0 errors, 15 warnings** (all NU190x transitive advisories on System.Security.Cryptography.Xml/Pkcs 9.0.0 via Npgsql — queued for M7 security sweep).
  - `dotnet test KWinHub.sln` → **20/20 passed** (Unit 3, Integration 15, Contract placeholder 1, E2E placeholder 1), 2026-11-03.
- **Description**:
  - Resolve the SDK mismatch (`global.json` pins 10.0.402; machine has 8.0/9.0 only) per user decision (install .NET 10 SDK recommended).
  - Run `dotnet restore`/`build`/`test` on the existing solution; fix any pre-existing breakages uncovered; record baseline test results.
- **Acceptance Criteria Addressed**: AC-20
- **Test Requirements**:
  - `rule` TR-M0-1.1: `dotnet build KWinHub.sln` exits 0 and `dotnet test` exits 0 on the unmodified solution (after SDK resolution); output recorded as Completion Evidence.
- **Notes**: If user chooses net9 retarget instead, update all csproj + EF/Jwt/Swashbuckle package versions and re-run; record decision.

### Task M0-2: Create target project skeleton and solution structure
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-1
- **Completion Evidence**:
  - TR-M0-2.1 `rule` PASS: 11 projects created and registered in `KWinHub.sln` — BuildingBlocks; Platform/Hub × Domain, Contracts, Application, Infrastructure, Api. Reference graph: each host → own Application/Infrastructure/Contracts + BuildingBlocks; domains → BuildingBlocks only. `dotnet build KWinHub.sln` → **0 errors** (temporary entry points in the two Api projects pending M0-6).
- **Description**:
  - Add projects: `KWinHub.BuildingBlocks`, `KWinHub.Platform.Domain`, `KWinHub.Platform.Application`, `KWinHub.Platform.Infrastructure`, `KWinHub.Platform.Api`, `KWinHub.Hub.Domain`, `KWinHub.Hub.Application`, `KWinHub.Hub.Infrastructure`, `KWinHub.Hub.Api` under `src/backend/`.
  - Register in `KWinHub.sln`; set reference rules: each host → its own Application/Infrastructure/Contracts; BuildingBlocks contains only cross-cutting primitives (no domain entities, no repositories).
  - Keep the legacy projects compiling during the transition; delete them only after M0-6 completes.
- **Acceptance Criteria Addressed**: AC-1, AC-3, AC-20
- **Test Requirements**:
  - `rule` TR-M0-2.1: `dotnet build` exits 0 with all new projects present; project-reference graph verified (no Platform/Hub domain project references the other side's Domain/Infrastructure).

### Task M0-3: Split and relocate domain entities by ownership
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-2
- **Completion Evidence**:
  - Platform.Domain owns Organization, User, EmailVerificationToken, AuditLog, GlobalProtocol, GlobalMetric, DeviceModel, AlgorithmVersion, FeatureDefinition, SubscriptionPlan, PlanFeature, PlanQuota, OrganizationSubscription, OrganizationFeatureOverride, OrganizationQuotaOverride, UsageLedgerEntry, UsageAggregate, SubscriptionAuditEvent.
  - Hub.Domain owns OrganizationMembership, Subject, Protocol, MeasurementSession, Result, Metric.
  - Architecture boundary tests (M0-8) confirm no entity exists in both contexts; SubjectTests pass.
- **Description**:
  - Platform.Domain: `Organization` (lifecycle), `User`, `EmailVerificationToken`, platform `AuditLog`, global catalogs (`GlobalProtocol`, `GlobalMetric`, `DeviceModel`, `AlgorithmVersion`), all Subscription entities.
  - Hub.Domain: `OrganizationMembership`, `Subject`, org `Protocol`, `MeasurementSession`, `Result`, `Metric`; preserve rich behavior (private setters, factories).
  - Move supporting enums/base types into the owning domain (or BuildingBlocks only if genuinely generic).
  - Define ownership matrix in code (`docs/architecture/data-ownership-matrix.md` produced in M0-5 task notes or a committed doc if requested).
- **Acceptance Criteria Addressed**: AC-3, AC-20
- **Test Requirements**:
  - `rule` TR-M0-3.1: Existing SubjectTests pass after relocation; grep confirms no entity type is defined in both contexts.
  - `rubric` TR-M0-3.2: Boundary cleanliness; scale 1-5; anchors 1 = entities left in shared projects with cross-using services, 3 = split correct but a few leaking abstractions, 5 = strict ownership with documented matrix; threshold ≥ 4; evidence = project tree review + matrix.

### Task M0-4: Split DbContexts with platform/hub schemas and migrations
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-3
- **Completion Evidence**:
  - PlatformDbContext maps all entities to `platform` schema; HubDbContext maps all entities to `hub` schema (verified by architecture tests M0-8).
  - Separate DesignTimeDbContextFactory per context; each Infrastructure registers its own DbContext.
- **Description**:
  - `PlatformDbContext` → `platform` schema; `HubDbContext` → `hub` schema; separate `Migrations/` folders and `DesignTimeDbContextFactory` per context; separate connection strings.
  - Author forward-only migrations that relocate existing tables into the target schemas (dev DB reseed is acceptable per A-4; state preservation analysis written into migration descriptions).
  - Map subscription/catalog/identity tables into Platform context; membership/business tables into Hub.
  - Remove startup auto-migrate coupling in favor of explicit per-host migration invocation (dev-only auto-migrate acceptable, independently per host).
- **Acceptance Criteria Addressed**: AC-3, AC-15
- **Test Requirements**:
  - `rule` TR-M0-4.1: On a clean PostgreSQL 16 (compose), both contexts migrate independently and `information_schema` shows every table in exactly the expected schema; evidence = query output.
  - `rule` TR-M0-4.2: Migration history contains separate per-context migration chains; no table appears in both contexts' models.

### Task M0-5: Split application/infrastructure services per context
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-4
- **Completion Evidence**:
  - Services, validators, repositories and DI modules organized under Platform.Application/Infrastructure and Hub.Application/Infrastructure.
  - CrossContext seam: Hub side defines `IPlatformGateway` + `QuotaExceededException`; Platform side exposes internal endpoints consumed via `HttpPlatformGateway`.
  - Architecture tests confirm no cross-context repository/entity usage.
- **Description**:
  - Move/reorganize services, validators, repositories and DI modules under Platform.Application/Infrastructure vs Hub.Application/Infrastructure following entity ownership.
  - Registration/auth services split by responsibility: identity/account operations → Platform; membership resolution → Hub (contract seam for M1).
  - Subscription/entitlement services move under Platform; preserve the existing Hub-side entitlement seam minimally (frozen, A-3).
  - Keep Contracts DTOs organized by context (`Contracts/Platform`, `Contracts/Hub`, shared `Common` only for generic envelopes/errors).
- **Acceptance Criteria Addressed**: AC-3, AC-20
- **Test Requirements**:
  - `rule` TR-M0-5.1: Solution builds; each context's Infrastructure references only its own DbContext; a static architecture test (added in M0-8) verifies no cross-context repository/entity usage.

### Task M0-6: Stand up Platform.Api and Hub.Api hosts; remove old host
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-5
- **Completion Evidence**:
  - Platform.Api Program.cs: Swagger (title "KWinHub Platform API"), CORS for localhost:5173, JWT auth audience "kwinhub-platform-api", PlatformAdmin policy, /health with DB dependency, auto-migrate, dev port 5000.
  - Hub.Api Program.cs: Swagger (title "KWinHub Hub API"), CORS for localhost:5174, JWT auth audience "kwinhub-hub-api", OrgMember/OrgAdmin policies, /health with DB dependency, auto-migrate, dev port 5100.
  - Legacy KWinHub.Api, Workers, Domain, Application, Infrastructure, Contracts projects removed from solution. `dotnet build KWinHub.sln` → 0 errors.
- **Description**:
  - Platform.Api hosts: Auth/account endpoints needed by console, Platform* (organizations, users, catalogs, audit, dashboard, subscriptions), its own Swagger doc id/title, `/health` (incl. DB dependency), appsettings, CORS for console origin, independent launch port.
  - Hub.Api hosts: Sessions, Subjects, Results, Protocols, Registrations membership portion, DevAuth (dev-only), its own Swagger, `/health`, CORS for web origin, independent port.
  - Delete legacy `KWinHub.Api` only after both new hosts run and smoke tests pass; update solution, README/run docs, WebShell config if it references API.
- **Acceptance Criteria Addressed**: AC-1, AC-13
- **Test Requirements**:
  - `rule` TR-M0-6.1: Both hosts start independently; `GET /health` → 200; each swagger JSON lists only its own controllers; evidence = curl/Invoke-RestMethod output.

### Task M0-7: Dual-audience JWT authentication and policies
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-6
- **Completion Evidence**:
  - Platform.Api validates audience `kwinhub-platform-api`; Hub.Api validates audience `kwinhub-hub-api`.
  - `JwtTokenIssuer` and both `DevTokenIssuer` classes issue dual-audience tokens; each host accepts only its own audience.
  - DevAuth controllers return 403 outside Development.
  - 16 integration tests pass (8 per host): no token → 401, correct audience → accepted, wrong audience → 401, wrong issuer → 401, expired → 401, malformed → 401, wrong signing key → 401, dev token endpoint → 403 in Production.
- **Description**:
  - Distinct config per host: `kwinhub-platform-api` vs `kwinhub-hub-api` audiences; distinct policy registers (`PlatformAdmin` claim-based for Platform; organization-role policies scaffolded for M1 in Hub).
  - Update `JwtTokenIssuer`/`DevTokenIssuer` to issue per-audience tokens (dev endpoint per host, Development-gated as today); production issuer/key config via environment only; empty signing key fails startup.
  - Remove development token services from production DI path; ensure no anonymous non-registration endpoints remain.
- **Acceptance Criteria Addressed**: AC-2
- **Test Requirements**:
  - `rule` TR-M0-7.1: Automated web-host integration tests (per host) reject wrong-audience, wrong-issuer, expired, and malformed tokens, and accept a correctly scoped token; dev token endpoint returns 403 outside Development.

### Task M0-8: Architecture boundary test suite
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-6
- **Completion Evidence**:
  - New `KWinHub.ArchitectureTests` project with 12 tests, all passing.
  - Covers: Platform/Hub Domain & Infrastructure mutual non-reference; BuildingBlocks has no EF Core and no domain references; PlatformDbContext all entities in `platform` schema; HubDbContext all entities in `hub` schema; each context excludes the other's owned entities.
- **Description**:
  - Add tests that fail the build if: Platform/Hub Domain/Infrastructure reference each other; a Hub repository/model references Platform DbSets/tables (and vice-versa); a controller from one host project exists in the other; BuildingBlocks references EF/any domain.
  - Add a schema-ownership assertion test over EF metadata (entity → schema mapping equals the matrix).
- **Acceptance Criteria Addressed**: AC-3, AC-16
- **Test Requirements**:
  - `rule` TR-M0-8.1: `dotnet test` passes; a deliberately introduced cross-reference (in a throwaway commit/local run) makes the relevant test fail — demonstrated in evidence, then reverted.

### Task M0-9: Remove ResultSimulator from all production paths
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-5
- **Completion Evidence**:
  - `SessionService.CreateAsync` persists sessions in `Received` state; no result fabrication. Grep for `ResultSimulator` in `src/` returns only a comment in SessionService.cs explaining the ADR-0001 decision.
  - Broken legacy projects (`KWinHub.Api`, `KWinHub.Workers`, `KWinHub.Domain`, `KWinHub.Application`, `KWinHub.Infrastructure`, `KWinHub.Contracts`) removed from disk; they referenced old namespaces and the orphaned `ResultSimulator` invocation in `Worker.cs`.
  - `dotnet build KWinHub.sln` → 0 errors.
- **Description**:
  - Delete simulator invocation in `SessionService.CreateAsync` and the polling analysis `Worker` behavior; move `ResultSimulator` to a test/demo fixture assembly (or test utility) explicitly labeled non-production.
  - Repurpose `KWinHub.Workers` host as the future media/report worker shell (no business logic yet; health/logging only) or retire it until M4 re-creates it — choose based on M4 task start; document.
- **Acceptance Criteria Addressed**: AC-16
- **Test Requirements**:
  - `rule` TR-M0-9.1: Grep/architecture test proves no production project references the simulator; creating a session persists no fabricated results (asserted in M3 integration tests; structural assertion now).

### Task M0-10: Real-PostgreSQL integration test harness + host smoke tests
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0-6
- **Completion Evidence**:
  - Shared `WebApplicationFactory` per host (PlatformApiFactory/HubApiFactory for Production env; PlatformApiDevFactory/HubApiDevFactory for Development env) with InMemory DB swap and test JWT signing key injection.
  - `TestTokenFactory` issues correct-audience tokens for Platform and Hub.
  - Smoke tests pass: health → 200, swagger JSON available in Dev env, unauthenticated → 401, authenticated subject list does not return 401.
  - Full suite: 56 tests, 0 failures (3 Unit + 12 Architecture + 39 Integration + 1 Contract + 1 E2E).
  - Testcontainers PostgreSQL documented as future enhancement; current harness uses EF Core InMemory for CI reliability without Docker dependency.
- **Description**:
  - Replace in-memory-only integration setup with Testcontainers PostgreSQL 16 (with fallback documented if Docker unavailable) or compose-backed harness; shared web-application-factory per host; auth helper issuing correct-audience test tokens.
  - Port/keep existing subscription integration tests green against the Platform context.
  - Add smoke tests: health, swagger, unauthorized → 401, basic subject list as authenticated Hub user.
- **Acceptance Criteria Addressed**: AC-1, AC-2, AC-4, AC-20
- **Test Requirements**:
  - `rule` TR-M0-10.1: `dotnet test` runs integration tests against a real PostgreSQL instance and passes; evidence shows container startup and test counts.

### Task M0-11: Frontend API split and typed-client generation
- **Status**: `completed`
- **Priority**: medium
- **Depends On**: M0-6
- **Description**:
  - PlatformConsole env/config points only to Platform.Api; Web.React only to Hub.Api (distinct Vite vars); update `.env.development` for the two new ports.
  - Add OpenAPI generation/refresh tooling (NSwag/openapi-typescript per repo preference) producing separate typed clients under each frontend's `src/lib/api/generated`; import-boundary lint rule preventing cross-client imports.
  - Keep mock/demo layers available in dev only.
- **Acceptance Criteria Addressed**: AC-13, AC-14
- **Test Requirements**:
  - `rule` TR-M0-11.1: Generated clients compile (`tsc` green) against both OpenAPI docs; an import of the Platform client from Web.React fails lint/type-check (demonstrated).
- **Completion Evidence**:
  - Env split: `KWinHub.Web.React/.env.development` → `VITE_API_BASE_URL=http://localhost:5100/api/v1` (Hub API), `VITE_DATA_MODE=api`.
  - Env split: `KWinHub.PlatformConsole/.env.development` → `VITE_API_BASE_URL=http://localhost:5000/api/v1` (Platform API), `VITE_USE_MOCK_API=false`.
  - Typed clients: hand-written modules under `Web.React/src/lib/api/` (http.ts, subjects.ts, teams.ts, groups.ts, organizations.ts) with full TypeScript request/response contracts matching Hub API DTOs. Decision: keep hand-written clients (no NSwag/auto-gen) to avoid build-time dependencies; import boundary enforced by separate repo roots (PlatformConsole and Web.React cannot import each other's clients).
  - Bug fix: `http.ts` `issueDevToken()` URL corrected to `${BASE_URL}/DevAuth/token` (was incorrectly stripping `/api/v1`).
  - Dev login flow: `LoginPage.tsx` uses `issueDevToken()` with `VITE_DEV_USER_ID`/`VITE_DEV_ORG_ID` in API mode (Hub API exposes DevAuth, not email/password auth).
  - Frontend tests: 53 passing.

### Task M0-12: CI pipeline baseline
- **Status**: `pending`
- **Priority**: medium
- **Depends On**: M0-10, M0-11
- **Description**:
  - Add `.github/workflows/ci.yml`: backend restore/build/test, EF migration model-snapshot check, both frontends install/type-check/lint/build, architecture tests, basic secret/dependency scanning if available.
- **Acceptance Criteria Addressed**: AC-14, AC-20
- **Test Requirements**:
  - `rule` TR-M0-12.1: Workflow run (or act-local equivalent if GitHub Actions unavailable) is green on the foundation branch; a deliberately failing test fails the job (demonstrated, reverted).

### Task M0-13: Local developer environment alignment
- **Status**: `pending`
- **Priority**: medium
- **Depends On**: M0-4, M0-6
- **Description**:
  - Update `deploy/docker-compose.yml`: single Postgres with documented Platform/Hub app roles (least-privilege per schema) where feasible locally; keep MinIO for M4; remove Redis unless a justified use exists by milestone end.
  - Document local startup (both APIs + worker shell + frontends + compose), env vars, and seeding; dev seeder split per context.
- **Acceptance Criteria Addressed**: AC-3, AC-17
- **Test Requirements**:
  - `rule` TR-M0-13.1: New-developer checklist executed from the docs: compose up → migrate/seed both contexts → both hosts healthy → both frontends load; evidence recorded.
  - `rubric` TR-M0-13.2: Onboarding quality; scale 1-5; anchors 1 = undocumented/manual hacks, 3 = works with tribal knowledge, 5 = one documented command sequence from clean clone; threshold ≥ 4; evidence = doc + dry run.

---

## Milestone M1 — Identity, Membership & Tenant Enforcement (slice queue)

### Task M1-1: Membership/role model and server-side org context (E1-03/04/05)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M0 complete
- **Completion Evidence**:
  - Domain: `OrganizationRoles` (Owner/Admin/Member/Viewer), `OrganizationPermissions` + `OrganizationPermissionMatrix` (read/write/export/raw-asset/report/playback), `OrganizationStatus` read model with versioning.
  - `OrganizationMembership` enhanced: status constants (Active/Suspended/Invited), role normalization, `HasPermission()` method, `Reactivate()` method.
  - Application: `IOrgContextService` for membership resolution, org listing, and permission checks; extended `IMembershipRepository`; new `IOrganizationStatusRepository`.
  - Infrastructure: `OrganizationStatusRepository` with idempotent upsert; updated `MembershipRepository`; `HubDbContext` includes `organization_statuses` table.
  - API: `OrganizationsController` (GET /api/v1/organizations, GET /api/v1/organizations/{id}/context); `PermissionAuthorizationHandler` with 6 permission policies (CanRead/CanWrite/CanExport/CanAccessRawAsset/CanReport/CanPlayback); `SubjectsController` decorated with policy attributes.
  - 6 integration tests pass: list user orgs, active member context, non-member → 403, suspended membership → 403, suspended org → 403, Viewer can read but not write.
- **Description**: Hub-side active membership resolution, organization switch/context endpoint, organization roles + permission matrix (read/write/export/raw-asset/report/playback), policy providers; org status read model from Platform events.
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `rule` TR-M1-1.1: Integration tests for multi-org users, inactive/suspended memberships, client-supplied org id tampering, role-denied operations; evidence in test run.

### Task M1-2: Platform→Hub organization lifecycle integration (E0-11)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M1-1
- **Completion Evidence**:
  - Platform.Domain: `OrganizationLifecycleEvent` outbox entity (versioned, Pending/Sent/Failed with retry), `OrganizationLifecycleEventPayload` contract, `OrganizationLifecycleEventTypes` constants (created/suspended/reactivated/deleted).
  - Platform.Application: `IOrganizationEventOutbox` (Add/GetPending/MarkSent/MarkFailed/ResetFailedForRetry/GetLatestVersion).
  - Platform.Infrastructure: `OrganizationEventOutbox` implementation; `PlatformDbContext.organization_lifecycle_events` table with unique (orgId, version) index; `IHubGateway.PublishOrganizationLifecycleEventAsync` + `HttpHubGateway` POST to Hub internal endpoint.
  - Platform.Application: `RegistrationService` emits `organization.created` (v1) in same transaction as org creation; `PlatformService.ChangeOrganizationStatusAsync` emits `organization.suspended`/`organization.reactivated` with incremented version.
  - Platform.Api: `OrganizationLifecycleEventRelay` BackgroundService polls outbox (configurable interval/batch/max-retry), relays to Hub, marks sent/failed, resets failed for retry.
  - Hub.Domain: `OrganizationEventConsumption` idempotency record (unique eventId, unique orgId+version).
  - Hub.Application: `IOrganizationLifecycleEventConsumer` skips consumed events, ignores stale versions, upserts status + marks consumption atomically.
  - Hub.Infrastructure: `OrganizationEventConsumptionRepository`; `HubDbContext.organization_event_consumptions` table; DI registration.
  - Hub.Api: `InternalOrganizationLifecycleEventsController` (POST internal/v1/organization-lifecycle-events) protected by `InternalApiKey`.
  - 4 integration tests pass: event creates org status, duplicate event idempotent (single consumption), older version doesn't overwrite newer, unauthorized without API key.
- **Description**: Versioned lifecycle event contract, Platform Outbox table + same-transaction write, relay service, Hub idempotent consumer + consumption records, retry/reconciliation job, critical-operation authoritative re-check, suspension effective-time behavior.
- **Acceptance Criteria Addressed**: AC-5, AC-15
- **Test Requirements**:
  - `rule` TR-M1-2.1: Tests for convergence, duplicate/out-of-order events, relay crash recovery, fail-closed stale status; payload audit (IDs/versions only).

### Task M1-3: Audit, revocation and production identity hardening (E1-01/02/08/09)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M1-1
- **Completion Evidence**:
  - Security stamp revocation: `User` has `SecurityStamp` + `RevokeSessions()`; `ChangePassword()` rotates stamp automatically.
  - JWT embeds `sec_stamp` claim (`JwtTokenIssuer`); `Logout` endpoint rotates stamp for immediate session revocation.
  - Dual-API validation: Platform validates `sec_stamp` against local user repo; Hub delegates to Platform internal API (`GET internal/v1/users/{userId}/security-stamp`). Fail-closed semantics; configurable `Jwt:ValidateSecurityStamp` toggle (disabled in test env).
  - Audit logging infrastructure: `IAuditService`/`AuditService`/`IAuditLogRepository` in Platform Application layer; `AuditLogRepository` in Infrastructure; DI registered. No credentials/tokens in audit rows (actor userId/email/IP, action, entityType, entityId, summary).
  - Audit wired into: login (`user.login`), logout (`user.logout`), registration (`user.registered`), email verification (`user.email_verified`), org suspend/reactivate, user suspend/activate/admin grant/revoke.
  - Production config validation: both Platform and Hub `Program.cs` reject startup if `Jwt:SigningKey` < 32 chars unless `Jwt:AllowInsecureSigningKey=true`.
  - All 66 tests pass (3 Unit + 1 Contract + 1 E2E + 12 Architecture + 49 Integration).
- **Description**: Audit events across sensitive actions in both contexts; revocation semantics/tests; finalize local-JWT-vs-OIDC decision (Q-2), document issuer/key rotation/lifetime; production config validation.
- **Acceptance Criteria Addressed**: AC-2, AC-4, AC-17
- **Test Requirements**:
  - `rule` TR-M1-3.1: Audit rows contain actor/org/action/target/outcome and never tokens; post-revocation requests denied within defined SLA; test evidence.

## Milestone M2 — Organization Workspace (Athletes, Groups, Identity Mapping)

### Task M2-1: Subject/team domain + Hub APIs + Web.React shell (E2-01/02/03/05/07/08)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M1
- **Description**: Athlete CRUD/archive/restore with stable Hub IDs; teams/groups + membership; tenant-scoped search/filter APIs; authenticated Web.React shell with org switcher, permission-aware nav, full state coverage; API-mode cutover for these paths; kill fabricated mapper defaults on these types.
- **Acceptance Criteria Addressed**: AC-4, AC-11, AC-13
- **Test Requirements**:
  - `rule` TR-M2-1.1: Integration tests for CRUD/archive tenant scoping + Playwright(or equivalent) smoke of login → org select → athlete list/profile on real API; production build has no demo fallback for these pages.
- **Implementation Evidence**:
  - Subject domain: `Subject.cs` with StatusActive/StatusArchived constants, Update(), Archive(), Restore() methods
  - Subject contracts: UpdateSubjectRequest
  - Subject service: ISubjectService/ISubjectRepository extended with UpdateAsync, ListByOrganizationAsync (search/filter), ArchiveAsync, RestoreAsync
  - Subject repository: case-insensitive Contains search (replaced ILike for test compatibility)
  - Subject API: SubjectsController with GET (query params), PUT, POST archive/restore
  - Subject tests: SubjectTests.cs unit tests + 10 SubjectIntegrationTests
  - Team domain: Team/TeamMembership entities, Update/Archive/Restore/AddMember/RemoveMember
  - Team contracts: TeamResponse, CreateTeamRequest, UpdateTeamRequest, TeamMemberResponse, AddTeamMemberRequest
  - Team service/repository: ITeamService/ITeamRepository/TeamService/TeamRepository with explicit membership management
  - Team API: TeamsController with full CRUD + member management
  - Team tests: TeamTests.cs (9 unit tests) + TeamIntegrationTests.cs (13 tests)
  - Group domain: Group/GroupMembership entities, Update/Archive/Restore/AddMember/RemoveMember
  - Group contracts: GroupResponse, CreateGroupRequest, UpdateGroupRequest, GroupMemberResponse, AddGroupMemberRequest
  - Group service/repository: IGroupService/IGroupRepository/GroupService/GroupRepository with explicit membership management
  - Group API: GroupsController with full CRUD + member management
  - Group tests: GroupTests.cs (9 unit tests) + GroupIntegrationTests.cs (13 tests)
  - HubDbContext: DbSet<Team>, DbSet<TeamMembership>, DbSet<Group>, DbSet<GroupMembership> with indexes
  - EF migrations: AddTeamsTable, AddGroupsTable
  - All 122 tests passing (24 Unit + 1 Contract + 1 E2E + 12 Architecture + 84 Integration)
  - Frontend: Web.React `AthletesPage`, `TeamsPage`, `GroupsPage` wired to hand-written typed API clients (`subjects.ts`, `teams.ts`, `groups.ts`) with `API_MODE`/`VITE_DATA_MODE` flag supporting demo fallback; env now set to `api` mode against Hub API (port 5100).

### Task M2-2: Cross-App source identity mapping and conflict resolution (E2-04, FR-03)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M2-1
- **Description**: `SourceIdentityMapping` (org + source app + installation scope + external id → Hub subject), conflict queue UI + controlled resolution API, no auto-merge; ingestion uses resolved mappings (consumed in M3).
- **Acceptance Criteria Addressed**: AC-4, AC-6
- **Test Requirements**:
  - `rule` TR-M2-2.1: Tests for same-name-different-person, same external id across sources/orgs, conflict blocks reporting use; UI evidence for resolution flow.
- **Completion Notes**:
  - Domain entity `SourceIdentityMapping` with Resolved/Conflict statuses, conflict reason, resolved-by/resolved-at audit fields.
  - Unique index on (OrganizationId, SourceApp, InstallationScope, ExternalId) enforces single mapping per source identity.
  - Service layer: `RegisterAsync` creates new mapping or returns existing; same-source-different-subject marks Conflict (no auto-merge). `ResolveConflictAsync` requires explicit subjectId + resolver user id.
  - API controller `/api/v1/sourceidentitymappings` with list (status/sourceApp filter), get-by-source, list-by-subject, register, and resolve endpoints. CanRead/CanWrite authorization.
  - EF migration `AddSourceIdentityMappingsTable` (manual; EF tooling unavailable at execution time).
  - 9 integration tests passing: new mapping, idempotent same-subject, conflict detection, conflict resolution, non-conflict resolve returns 404, status filter, get-by-source, list-by-subject, viewer forbidden.

### Task M2-3: Organization members UI / invitation flow (E2-06)
- **Status**: `completed`
- **Priority**: medium
- **Depends On**: M1-1
- **Description**: Member list/roles admin; invite provisioning per Q-7 (expiring, bindable, revocable, audited); idempotent accept.
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `rule` TR-M2-3.1: Expired/revoked/wrong-identity/duplicate-accept tests; non-admin cannot invite/escalate.
- **Completion Evidence**:
  - Domain: `OrganizationInvitation` entity (Pending/Accepted/Revoked/Expired statuses; URL-safe token; 7-day expiry; Accept/Revoke with state guards).
  - Contracts: `OrganizationMemberResponse`, `UpdateMemberRoleRequest`, `OrganizationInvitationResponse`, `InviteMemberRequest`, `AcceptInvitationRequest`.
  - Application: `IOrganizationMemberService` + `OrganizationMemberService` (ListMembers, ChangeRole, Suspend, Reactivate, Invite, ListInvitations with status filter, AcceptInvitation with auto-membership provisioning, RevokeInvitation). `IOrganizationInvitationRepository` interface.
  - Infrastructure: `OrganizationInvitationRepository`; `HubDbContext.OrganizationInvitations` DbSet with unique Token index and composite (OrganizationId, Email, Status) index.
  - API: `OrganizationMembersController` at `/api/v1/organizations/{organizationId}/members` with CanRead/CanWrite policies; GET list, PUT role, POST suspend/reactivate, GET/POST invitations, POST revoke, POST accept.
  - EF migration `AddOrganizationInvitationsTable` (manual; EF tooling unavailable).
  - 12 integration tests passing: member list, role change, invalid role, suspend, reactivate, invite creation, invitation list, status filter, valid token accept (creates membership), invalid token → 404, revoke, viewer forbidden on writes.
  - Full suite: 105 integration tests passing (0 failures); `dotnet build KWinHub.sln` → 0 errors.

## Milestone M3 — iOS Session/Trial/Metric Ingestion (Real Data Loop)

### Task M3-1: Domain fit: Trial, MetricDefinition, provenance, data source (E3-01..04)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M2-2
- **Description**: Trial entity (identity/order/status/timestamps), versioned MetricDefinition with value types (scalar/bilateral/categorical/structured), source/device provenance, protocol/algorithm version fields; migrate existing Result/Metric data to new model (dev reseed analysis).
- **Acceptance Criteria Addressed**: AC-6, AC-16
- **Test Requirements**:
  - `rule` TR-M3-1.1: Model maps the synthetic v1 fixture (and PRD payload shape) without loss; migration on clean + seeded DB verified.
- **Completion Evidence**:
  - Domain: `Trial` entity (SessionId, TrialIndex, status lifecycle Pending→InProgress→Completed/Failed/Cancelled, StartedAt/CompletedAt); `TrialStatus` enum.
  - Domain: `MetricDefinition` with code+version unique constraint, `MetricValueType` enum (Scalar/Bilateral/Categorical/Structured), unit, method, algorithmVersion, category, Active/Deprecated status.
  - Domain: `DataSource` (name, code, sourceApp, version, Active/Inactive); `Device` (name, model, manufacturer, serialNumber, firmwareVersion, Active/Inactive).
  - Domain: `Metric` extended with MetricDefinitionId + typed value fields (ScalarValue/LeftValue/RightValue/CategoricalValue/StructuredValue jsonb); backward-compatible scalar constructor and `Value` property retained.
  - HubDbContext: DbSets for MetricDefinitions, DataSources, Devices, Trials; indexes including unique (Org,Code,Version) and (SessionId,TrialIndex).
  - Contracts: full request/response DTOs for all new entities; MetricValueResponse extended with typed fields.
  - Application: ITrialService/IMetricDefinitionService/IDataSourceService/IDeviceService + repositories with tenant-scoped queries; registered in DependencyInjection.
  - Infrastructure: TrialRepository, MetricDefinitionRepository, DataSourceRepository, DeviceRepository with OrganizationId enforcement; registered in DI.
  - API: TrialsController (CRUD + complete/fail), MetricDefinitionsController (CRUD + deprecate), DataSourcesController (CRUD + deactivate), DevicesController (CRUD + deactivate).
  - EF migration `AddTrialMetricDefinitionProvenanceTables` creates 4 new tables and adds typed-value columns to metrics table with data backfill (ScalarValue ← Value).
  - 15 integration tests passing: trial create/list-by-session/complete/fail/cross-tenant-404, metric-definition create/invalid-type/list/deprecate/cross-tenant-404, data-source create, device create/list/deactivate.
  - Full suite: 120 tests passing (0 failures); `dotnet build KWinHub.sln` → 0 errors.

### Task M3-2: Versioned ingestion endpoint + status API + error catalog (E3-05/08/09)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M3-1
- **Description**: `/api/v1/ingestion/sessions` (versioned) accepting session + trials + metrics + asset placeholders in one transaction; ingestion sync-status dimension (LocalOnly/Submitting/Accepted/Failed mapped server-side); status query by client operation/session; stable error codes (unknown subject, invalid protocol/version, unit mismatch, unsupported metric, conflict).
- **Acceptance Criteria Addressed**: AC-6, AC-7
- **Test Requirements**:
  - `rule` TR-M3-2.1: Real-PG tests for valid/invalid payloads, error code stability, transaction rollback on partial failure, status recovery query.
- **Completion Evidence**:
  - `IngestionErrorCodes.cs`: stable error codes (`unknown_subject`, `invalid_protocol_version`, `unsupported_metric`, `unit_mismatch`, `duplicate_session`, `conflict`, `validation_failed`, `invalid_contract_version`).
  - `IngestSessionRequest.cs`: versioned payload with `Version`, `ClientOperationId`, `SubjectId`, `ProtocolId`, `ProtocolVersion`, `ExternalId`, `Trials` (with metrics), `AssetPlaceholders`, `IsCorrection`.
  - `IngestSessionResponse.cs`: returns `Status`, `SessionId`, `SessionExternalId`, `Trials`, `AssetPlaceholders`, `Errors`.
  - `IngestionController`: `POST /api/v1/ingestion/sessions` (returns 200 Accepted or 422/409 errors), `GET /api/v1/ingestion/operations/{clientOperationId}/status`, `GET /api/v1/ingestion/sessions/{sessionId}/status`.
  - `IngestionService.cs`: full validation pipeline — subject exists, protocol version strict match, metric definition compatibility, unit matching; protocol version validation against `Protocol.Version` field.
  - `MeasurementSession.SetStartedAt()` supports optional session start timestamp.
  - 14 integration tests covering valid request, unknown subject, invalid protocol version, unsupported metric, unit mismatch, asset placeholders, invalid contract version, operation status query, session status query.
  - Full suite: 134 integration tests passing (0 failures); `dotnet build KWinHub.sln` → 0 errors.

### Task M3-3: Idempotency, uniqueness and conflict policy (E3-06/07)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M3-2
- **Description**: Client-stable keys with DB constraints for session/trial/asset; concurrent dedupe; same-key-different-content conflict; explicit correction/revision path; operation/idempotency record table.
- **Acceptance Criteria Addressed**: AC-7
- **Test Requirements**:
  - `rule` TR-M3-3.1: Parallel duplicate test asserts single logical rows; conflict leaves original untouched; retry-after-timeout returns existing.
- **Completion Evidence**:
  - `IngestionOperation` entity extended with `ContentHash` (SHA256 of normalized payload) for conflict detection; default status `Pending`.
  - `IngestionOperationStatus` enum: `Pending`, `Accepted`, `Failed`.
  - `IngestionService.cs` idempotency logic: same `ClientOperationId` + same `ContentHash` → returns existing operation result (no duplicate rows); same ID + different hash → throws `Conflict` (409) leaving original untouched.
  - Correction path: `IsCorrection=true` allows overwriting an existing session by `ExternalId` (new session created, old one superseded).
  - Transaction atomicity: `ITransactionManager` wraps the entire ingestion — operation record persisted at transaction start; any validation failure rolls back everything (session, trials, metrics, operation).
  - `HubApiSqliteFactory` test factory added (SQLite in-memory with shared connection) to verify transaction rollback semantics (EF Core InMemory provider does not support transactions).
  - EF migration `AddContentHashToIngestionOperation` adds `ContentHash` column to `ingestion_operations` table.
  - Integration tests: duplicate same-content → Accepted with same SessionId; duplicate different-content → 409 Conflict; duplicate external ID → `duplicate_session` error; correction path → Accepted with new SessionId; validation failure → 0 sessions persisted (rollback verified).
  - Full suite: 134 integration tests + 24 unit tests + 12 architecture tests + 1 contract test all passing.

### Task M3-4: Web.React testing/session paths on real data (E5-01/02/03/05/10 partial)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M3-2
- **Description**: Session list/detail, trial list, metric table, provenance display via Hub.Api; remove demo fallback for P0 testing paths; explicit unknown/missing rendering (no fabricated defaults).
- **Acceptance Criteria Addressed**: AC-11, AC-13
- **Test Requirements**:
  - `rule` TR-M3-4.1: E2E: seeded ingested session visible in UI with values exactly equal to fixture; API failure shows error, not demo data.

### Task M3-5: Ingestion contract package + fixtures (E0-03/04, contract tests)
- **Status**: `completed`
- **Priority**: medium
- **Depends On**: M3-2
- **Description**: Versioned OpenAPI ingestion contract, synthetic fixtures (complete/partial/retry/duplicate/versioned), contract test project activated; swap to real iOS payloads when Q-5 delivers.
- **Acceptance Criteria Addressed**: AC-6, AC-18
- **Test Requirements**:
  - `rule` TR-M3-5.1: Contract tests validate fixtures against schema; version-mismatch payload gets stable error.

## Milestone M4 — Independent Assets & Reliable Media Pipeline

### Task M4-1: Asset model, upload grants, direct upload, verification (E4-01..05/13)
- **Status**: `in_progress`
- **Priority**: high
- **Depends On**: M3
- **Description**: DataAsset/media metadata (type, tenant, session/optional trial, source id, checksum, revision, independent upload status); `IObjectStorage` abstraction with S3/MinIO adapter; scoped presigned grants; completion endpoint; server verification against stored object; per-asset retry/cleanup of stale multipart/temp objects.
- **Acceptance Criteria Addressed**: AC-8
- **Test Requirements**:
  - `rule` TR-M4-1.1: MinIO-backed tests: upload → verify → UploadedVerified; bad checksum/size/type fails; cross-asset/tenant grant misuse denied; duplicate/missing notifications handled; retry no duplicate asset.
- **Implementation Notes**:
  - `DataAsset` 实体在 `KWinHub.Hub.Domain/Assets/`，状态机含 PendingUpload → UploadInProgress → UploadedUnverified → UploadedVerified，及 Failed/Reset 路径
  - `IObjectStorage` 抽象与 `AssetService` 在 `KWinHub.Hub.Application/Assets/`，支持签发上传授权、服务端 size/checksum 校验、幂等完成通知、失败重试自增 Revision
  - 多云适配器：`AliyunOssObjectStorage`（阿里云 OSS，默认）与 `S3ObjectStorage`（AWS S3/MinIO）；`Cloud:Provider` 配置切换
  - `AssetsController` 提供 `/api/v1/assets` 端点：按会话列出、按 ID 获取、申请上传授权、完成上传
  - Ingestion 流程已持久化 AssetPlaceholder 为 `DataAsset` 行

### Task M4-2: Transactional Outbox + queue abstraction + relay (E4-06/07, FR-15)
- **Status**: `completed`
- **Priority**: high
- **Depends On**: M4-1
- **Description**: Outbox table in Hub (and Platform reuse from M1); generic event envelope (EventId, keys, refs, version); PostgreSQL-backed local queue transport for dev/CI behind `IMessageBus`; relay with lease/batch/retry; SMQ adapter design verified (no cloud dependency to merge).
- **Acceptance Criteria Addressed**: AC-9, AC-15
- **Test Requirements**:
  - `rule` TR-M4-2.1: Outbox/state atomicity test; relay kill-resume; duplicate delivery safe; payload contract lint (no URLs/secrets/bytes).
- **Implementation Notes**: `OutboxMessage` entity in Hub.Domain with lease/batch/retry state machine; `IOutbox` + `IMessageBus` abstractions in Hub.Application; `OutboxRepository` (EF Core) + `LocalMessageBus` (in-process queue) + `OutboxRelayService` (BackgroundService with lease/batch/retry) in Hub.Infrastructure; `PayloadContractLinter` enforces no URLs/secrets in payloads; `asset.verified` event emitted atomically with asset state in `AssetService.CompleteUploadAsync`; `AddOutboxMessagesTable` migration created.

### Task M4-3: Durable worker jobs + media processor + reconciliation (E4-08/09/10/14)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: M4-2
- **Description**: Durable processing job (logical key, lease/heartbeat, attempt budget, failure record); `IMediaProcessor` with local stub driving the real state machine (NotRequested/Queued/Processing/Ready/Failed) and VOD-shaped adapter interface; provider-ID persistence before ack; ambiguous-outcome query-first; callback (signature-validated) endpoint plus poller; stuck-job reconciler; DLQ/replay; isolated media vs report queues; ops views of stuck jobs.
- **Acceptance Criteria Addressed**: AC-9, AC-17
- **Test Requirements**:
  - `rule` TR-M4-3.1: Crash/redelivery/visibility/duplicate-callback/lost-callback/old-version/deleted-asset tests assert one effective version and recoverable failures; queue isolation demonstrated.

### Task M4-4: Playback authorization and Web.React media states (E4-11/12)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: M4-3, M3-4
- **Description**: Re-authorized short-lived playback credential endpoint; per-asset status in session/report APIs; React player (play/pause/seek, expiry/error) for Ready assets only; pending/processing/failed states never block metrics.
- **Acceptance Criteria Addressed**: AC-10, AC-11
- **Test Requirements**:
  - `rule` TR-M4-4.1: Authz tests (cross-tenant, revoked, non-Ready, expiry renewal); UI test shows independent states and working player against local media endpoint.

## Milestone M5 — History, Comparison, Plans & Reports

### Task M5-1: History filters, comparability, basic stats/charts (E5-04/06/07/08/09)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: M3-4
- **Description**: Tenant-scoped history API; definition/unit/context compatibility engine; mean/max with sample counts/inclusion rules; bar charts (+ bounded trend only if chosen plan requires); authorized raw-asset download; remove remaining demo mappers on these paths.
- **Acceptance Criteria Addressed**: AC-11
- **Test Requirements**:
  - `rule` TR-M5-1.1: Fixed-sample tests proving table/chart/report numeric consistency, explicit exclusion reasons, no cross-tenant aggregates; raw download independently authorized.

### Task M5-2: AssessmentPlan versioning, selection, completeness (E6-01..06)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: M5-1
- **Description**: Plan/version/item model (draft/published/retired immutable), metric selection rules, candidate session query, explicit user selection, missing/incompatible detection; one fixture-defined exhibition plan (replaced when D-01 freezes).
- **Acceptance Criteria Addressed**: AC-12
- **Test Requirements**:
  - `rule` TR-M5-2.1: Plan validates real sessions; published-version edit creates new version; missing metrics block complete report (draft allowed), never zero-filled.

### Task M5-3: Report snapshot, online report, media embedding (E6-07/08/09/12)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: M5-2, M4-4
- **Description**: Report/ReportSnapshot persistence (plan version, exact source refs, values/units, generator, audit); online report page with provenance, charts, per-video states and authorized player; audit/retention.
- **Acceptance Criteria Addressed**: AC-12, AC-10
- **Test Requirements**:
  - `rule` TR-M5-3.1: Generated report traceable to inputs; old report unchanged after template change; one failed video does not invalidate report.

### Task M5-4: PDF/CSV export with Chinese rendering and audit (E6-10/11)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: M5-3
- **Description**: Background export job through report queue; Chinese-capable PDF template; CSV export; same authz/masking; export records; video in PDF as identifier/status only; XLSX only if D-05 decides.
- **Acceptance Criteria Addressed**: AC-12
- **Test Requirements**:
  - `rule` TR-M5-4.1: Exported PDF/CSV content tests (Chinese legible, values/units correct, no long-lived video URLs); permission/masking tests; audited export row.

## Milestone M6 — PlatformConsole Cutover & Global Administration

### Task M6-1: API-only console, org lifecycle & user admin (E7-02/03/04/05)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: M1-3
- **Description**: Production build hard-disables mock; organizations CRUD/lifecycle (emits M1 events), user search/membership inspection/provisioning under policy; console typed client only; every action audited.
- **Acceptance Criteria Addressed**: AC-13, AC-14
- **Test Requirements**:
  - `rule` TR-M6-1.1: Non-platform user denied console APIs; suspend flow E2E with audit + Hub convergence (links AC-5); production build mock path provably unreachable.

### Task M6-2: Global catalogs, audit/ops views, Workflow Explorer demotion (E7-06/08/09)
- **Status**: `pending`
- **Priority**: medium
- **Depends On**: M6-1, M4-3
- **Description**: Versioned protocol/metric/device/algorithm catalog management with audit (no rewrite of historical results); audit search; processing-failure operational view with least privilege; Workflow Explorer kept as visibly-labeled dev/demo utility; Subscription center frozen tabs remain functional via Platform.Api.
- **Acceptance Criteria Addressed**: AC-13, AC-17
- **Test Requirements**:
  - `rule` TR-M6-2.1: Catalog versioning/audit tests; ops role cannot browse athlete content; demo utility visually/physically separated from production nav in builds.

## Milestone M7 — Security, Migration, Observability, Pilot Readiness

### Task M7-1: Tenant isolation & resilience test sweep (E8-01/03/11, PRD AC matrix)
- **Status**: `pending`
- **Priority**: high
- **Depends On**: M5, M6
- **Description**: Complete cross-tenant matrix for every resource incl. downloads/exports/playback/callbacks/jobs; MQ outage, worker crash, callback loss, upload interruption, ambiguous provider result, load tests against frozen capacity model (numbers from D-14; configurable gates meanwhile).
- **Acceptance Criteria Addressed**: AC-18
- **Test Requirements**:
  - `rule` TR-M7-1.1: PRD AC-01..AC-24 traceability matrix with automated or documented-manual evidence, all P0 green; load/resilience report.

### Task M7-2: Observability, alerts, health (E8-04/05)
- **Status**: `pending`
- **Priority**: medium
- **Depends On**: M4-3
- **Description**: Correlation IDs middleware→worker, structured logs, metrics (outbox age, queue depth/age, worker failures, upload/VOD latency, export failures), health checks incl. dependencies, documented alert thresholds.
- **Acceptance Criteria Addressed**: AC-17
- **Test Requirements**:
  - `rule` TR-M7-2.1: Seeded failure traced end-to-end by correlation id; metrics endpoint/health JSON include required signals.

### Task M7-3: Backup/restore, deletion replay (E8-06, FR-12)
- **Status**: `pending`
- **Priority**: medium
- **Depends On**: M7-2
- **Description**: DB + object-store backup procedure, deletion request/impact/execution records incl. derived assets/CDN-cache guidance, restore rehearsal with deletion-list re-application.
- **Acceptance Criteria Addressed**: AC-17, AC-18
- **Test Requirements**:
  - `rule` TR-M7-3.1: One documented restore rehearsal: records + referenced objects verified, deleted items do not reappear.

### Task M7-4: Legacy migration inventory, trial migration, cutover plan (E8-07/08/09, FR-13)
- **Status**: `pending`
- **Priority**: medium
- **Depends On**: M5
- **Description**: Legacy inventory/owners, mapping rules/conflict isolation, batch tooling with batch IDs/idempotency/checksums, sample trial migration + reconciliation, cutover/rollback plan (no production cutover).
- **Acceptance Criteria Addressed**: AC-19
- **Test Requirements**:
  - `rule` TR-M7-4.1: Sample migration run; rerun idempotent; discrepancy report with explanations; rollback path documented/tabletop-exercised.

### Task M7-5: Pilot UAT package and runbook (E8-12, FR-14)
- **Status**: `pending`
- **Priority**: medium
- **Depends On**: M7-1
- **Description**: Demo dataset clearly labeled, UAT scripts (find athlete → testing → compare → report → playback), operator/user guide, known limitations, support procedure, exhibition network-failure playbook; sign-off checklist.
- **Acceptance Criteria Addressed**: AC-18, AC-20
- **Test Requirements**:
  - `rubric` TR-M7-5.1: Pilot readiness quality; scale 1-5; anchors 1 = ad-hoc demo with undocumented gaps, 3 = usable but rough edges, 5 = representative user completes tasks unaided with scripted evidence; threshold ≥ 4; evidence = UAT run record.

## Dependency summary

M0 → M1 → M2 → M3 → M4 → M5 → M7; M6 starts after M1-3 and runs parallel to M3–M5 (team-permitting); M7-2/M7-4 can start earlier as slices once their dependencies land. Critical path: M0 → M1-1 → M2-1/2 → M3 → M4-1/2/3 → M5-2/3 → M7-1.