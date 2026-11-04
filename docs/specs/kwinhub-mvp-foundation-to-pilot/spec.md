# KWinHub MVP — Two-API Foundation to Pilot-Ready Business Loop

## Overview

- **Summary**: Evolve the existing KWinHub MVP codebase into the PRD V1.2 target: two independently deployable ASP.NET Core APIs (`KWinHub.Platform.Api` for PlatformConsole, `KWinHub.Hub.Api` for Web.React and approved client apps) with strict data ownership, server-enforced tenant authorization, reliable iOS session/trial/metric ingestion, independent asset upload and a recoverable media pipeline, real-data athlete history/review, versioned assessment plans with traceable reports and authorized video playback, plus pilot hardening.
- **Purpose**: Deliver a demonstrable, pilot-capable vertical workflow — *identity → athlete → App-calculated test ingestion → independent file/video upload → history & comparison → plan-based report → authorized playback* — without a full rewrite and without expanding into full microservices.
- **Target Users**: Organization admins, coaches/scientists, test operators, platform operations staff (per PRD §3.1).

## Baseline Documents (authoritative)

1. [PRD V1.2 Rev 2](file:///d:/Users/Kent/WorkSpace/Kunwei/Resources/KWinHub.MVP/docs/product/kwinhub-mvp-prd-v1.2-zh-CN.md) — FR-01..FR-15, AC-01..AC-24, decisions D-01..D-16
2. [Blueprint V0.3](file:///d:/Users/Kent/WorkSpace/Kunwei/Resources/KWinHub.MVP/docs/architecture/kwinhub-project-blueprint-v0.3-zh-CN.md)
3. [ADR-0001](file:///d:/Users/Kent/WorkSpace/Kunwei/Resources/KWinHub.MVP/docs/architecture/ADR-0001-platform-and-hub-api-separation-zh-CN.md)
4. [Alignment & Revision Plan V1.1](file:///d:/Users/Kent/WorkSpace/Kunwei/Resources/KWinHub.MVP/docs/planning/kwinhub-mvp-v1.0-product-alignment-and-project-revision-plan-zh-CN.md)
5. [Detailed Implementation Backlog V1.1](file:///d:/Users/Kent/WorkSpace/Kunwei/Resources/KWinHub.MVP/docs/planning/kwinhub-mvp-v1.0-detailed-implementation-backlog.md) (E0–E8)

## Goals

- G0: Two-API foundation with separate hosts, audiences, schemas/DbContexts/migrations, build/CI green, contract ownership frozen.
- G1: Real, idempotent iOS structured-data loop on real PostgreSQL with server-side tenant enforcement; Web.React reads real data.
- G1–G2: Independent asset upload, server verification, Outbox + queue + idempotent worker media processing, playback authorization (local-dev substitutes behind cloud-ready abstractions).
- G2: Athlete history, compatible-metric comparison, one versioned exhibition assessment plan, online report with media states, PDF/CSV export.
- G2: PlatformConsole performs real platform administration via Platform.Api only (API-only production).
- G3: Tenant-isolation suite, failure/recovery tests, observability, migration rehearsal, pilot runbook.

## Non-Goals (V1)

- Cloud-side scientific recalculation from raw force signals, event detection, AI advice, algorithm migration (App stays authoritative; `ResultSimulator` removed from all production paths).
- Full microservice decomposition, Kafka, Kubernetes, event sourcing, full CQRS; Redis without measured justification.
- Distributed transactions across contexts; cross-context direct table writes.
- Generic report designer/formula engine/BI dashboards, full CRM, complex team hierarchies, consumer portal.
- Actual production cloud purchase/deployment (Aliyun OSS/VOD/SMQ), real customer data cutover, overseas deployment, third-party developer portal.
- New iOS/Android client; the existing ForcePlateDevTool desktop client is out of scope.
- Expanding the Subscription Center beyond what already exists (PRD D-08: deferred unless commercial pilot requires it).

## Background & Context — Repository Assessment (2026-11-03, read-only)

### Existing solution structure

- **Backend**: single layered solution under `src/backend` — `KWinHub.Api` (one host), `.Domain`, `.Application`, `.Infrastructure`, `.Contracts`, `.Workers`; .NET `net10.0`, EF Core 10 / Npgsql, JWT Bearer, FluentValidation, Swashbuckle.
- **Frontends**: `src/frontend/KWinHub.PlatformConsole` and `KWinHub.Web.React` — React 18.3 / Vite 5.4 / TS 5.5 / Tailwind / Vitest. A separate Tauri `WebShell` and a WPF `ForcePlateDevTool` also exist (out of scope).
- **Tests**: `KWinHub.UnitTests` (1 real file: SubjectTests), `KWinHub.IntegrationTests` (subscription tests using **in-memory** DbContext, not real PostgreSQL), `KWinHub.ContractTests`/`KWinHub.EndToEndTests` (placeholder only).
- **Deploy**: `deploy/docker-compose.yml` with postgres:16, redis:7, minio (no app/worker containers; Redis/MinIO unused by code today). No CI pipeline at repository root.

### What is reusable

- Clean layer/project separation, repository pattern, FluentValidation, problem details, CORS, Swagger — all carry into the split.
- Entities: `Organization`, `User`, `EmailVerificationToken`, `OrganizationMembership`, `Subject`, org `Protocol`, `MeasurementSession` (already has `ExternalId` + unique index per org), `Result`/`Metric` basics, global catalogs, `AuditLog`.
- Registration + email verification flow, local JWT issuance (`JwtTokenIssuer`), `DevAuthController` correctly gated to Development, password hashing abstraction.
- A substantially built Subscription Center (plans/features/quotas/overrides/usage ledger/entitlements) — relocates to the Platform ownership boundary, then frozen.
- 4 existing EF migrations with named, forward-only history; dev seeder.
- PlatformConsole and Web.React already have the page/navigation skeletons matching the target IA.

### Critical gaps vs the target architecture

1. **One API host serves both frontends** with a single audience `kwinhub-api`; Platform* controllers and business controllers coexist. Violates ADR-0001.
2. **One giant DbContext, one `public` schema** mixing Platform-owned and Hub-owned tables; no ownership boundary, no per-context migrations.
3. **Tenant from JWT claim**: controllers pass the `org_id` claim directly into services; active membership / organization status is not re-validated per request (the membership repo is only consulted for subscription-gated features). Role/resource permission checks are explicit TODOs in `OrganizationAccessService`.
4. **Hub fabricates scientific results**: `ResultSimulator` runs both inline in `SessionService.CreateAsync` and in the polling `Worker` — directly conflicts with "App is authoritative; Hub stores/validates/presents".
5. **Ingestion cannot accept real results**: no Trial entity; metrics are embedded strings + one `double Value` (no MetricDefinition/version, scalar/bilateral/categorical support); no endpoint accepts trials/metrics; no client source/device/algorithm provenance contract; idempotency is session-only with no same-key-different-content conflict detection.
6. **No assets/media at all**: no DataAsset, upload authorization, object storage integration, Outbox, MQ, processing jobs, callbacks/reconciliation, playback credentials. MinIO is available in compose but unused.
7. **Single session status dimension** (worker-pipeline enum) instead of the required four: ingestion / upload / media-processing / report.
8. **Missing domain**: Team/TeamMembership, SourceIdentityMapping, AssessmentPlan/version/items, Report/ReportSnapshot, OutboxEvent/ConsumptionRecord, SyncOperation, Consent/Authorization, DataSource, MigrationBatch.
9. **Frontends default to fake data**: PlatformConsole `VITE_USE_MOCK_API` defaults `true`; Web.React `VITE_DATA_MODE=demo`; mappers fabricate values (e.g. `sex: 'M'`, zeroed body metrics, invented processing config). Both point at the same backend URL; no OpenAPI-generated typed clients.
10. **Verification gap**: no real-PostgreSQL integration tests, no tenant isolation suite, no contract/E2E tests, no CI.
11. **Environment blocker**: `global.json` pins SDK `10.0.402`; this machine has only 8.0.422 / 9.0.103 / 9.0.200. The backend cannot build until the SDK is installed or the target is adjusted.

## Functional Requirements

- **FR-1 (Two-API foundation, ADR-0001)**: Two independently buildable/runnable API hosts with distinct OpenAPI, health checks, appsettings, audiences/scopes/policies; PlatformConsole configured only for Platform.Api; Web.React/clients only for Hub.Api.
- **FR-2 (Data ownership)**: Separate `platform` and `hub` PostgreSQL schemas, DbContexts, migrations, and DB principals; each context writes only its own tables; cross-context access is event/contract-based only; architecture tests enforce the boundary.
- **FR-3 (Cross-context integration)**: Platform→Hub organization lifecycle events via transactional Outbox, relay, idempotent consumer with retry/reconciliation; suspension convergence with authoritative re-check for critical operations; no distributed transactions.
- **FR-4 (Identity & tenant enforcement)**: Production token validation in both APIs with distinct audiences; server resolves current organization from active membership (never trusts a client-supplied org id as authorization); organization switching; platform roles vs organization roles; revocation/suspension enforcement; audit events for sensitive actions; no dev token route in production.
- **FR-5 (Organization workspace)**: Subject (athlete) CRUD/archive with stable identity; cross-App source identity mapping with conflict queue (no auto-merge by name/phone); teams/groups with membership; organization member management.
- **FR-6 (Ingestion contract)**: Versioned, authenticated, tenant-scoped session/trial/metric ingestion accepting client-stable IDs, structured metric values with definition/unit/version/provenance; actionable validation error catalog; status query endpoint.
- **FR-7 (Idempotency & transactions)**: Repeated/concurrent submissions yield one logical record; same-key-different-content returns conflict (no silent overwrite); session+trial+metric+asset-placeholder persist atomically; retries distinguishable from corrections.
- **FR-8 (Independent asset upload)**: Per-asset short-lived upload authorization; client direct-to-object-storage upload (S3-compatible locally via MinIO; Aliyun OSS later behind an interface); server-side existence/size/type/checksum verification; independent upload status dimension; resumable/retryable without duplicating assets or rebuilding sessions.
- **FR-9 (Reliable media processing)**: Transactional Outbox + queue abstraction (local transport in dev; SMQ-ready) + bounded, idempotent worker with durable jobs/leases/retries; media-processor abstraction (local stub in dev; VOD-ready) with provider ID persistence, ambiguous-outcome reconciliation, callback/authenticity handling (or poller in dev), and stuck-job reconciliation.
- **FR-10 (Playback authorization)**: Organization/report/subject/asset permission re-check before issuing short-lived playback credentials; per-asset Ready gating; revocation stops new credentials; media never served as permanent public URLs through the API host.
- **FR-11 (History & review)**: Tenant-scoped athlete history filters, session detail with trials/metrics/provenance/asset states; compatible-metric comparison with explicit incompatibility reasons; approved basic statistics (mean/max) with sample counts and inclusion rules; required bar charts; explicit loading/empty/partial/failed/unauthorized states.
- **FR-12 (Plans & reports)**: Distinguish TestProtocol vs versioned AssessmentPlan (draft/published/retired, immutable published versions); candidate session query; explicit selection rules; completeness validation (missing ≠ zero); report snapshot generation; online report with per-video states; PDF (Chinese-capable) + CSV export with authorization/audit; XLSX stays a decision.
- **FR-13 (Frontend cutover)**: Web.React P0 paths use Hub.Api only with no demo fallback in production builds and no fabricated mapper defaults; authenticated shell, org switcher, permission-aware navigation. PlatformConsole API-only in production via Platform.Api only; mocks remain dev/test-only.
- **FR-14 (Platform administration)**: Organization lifecycle create/inspect/activate/suspend, platform user administration, global catalogs with version/audit, audit/operational views; every action authorized and audited.
- **FR-15 (Hardening & pilot readiness)**: Tenant isolation suite across all resource classes; real-PostgreSQL integration tests; CI; observability (correlation IDs, structured logs, health, outbox/queue/worker metrics); backup/restore and deletion replay procedure; migration inventory + idempotent trial migration; capacity model + load/resilience tests; pilot UAT/runbook.

## Non-Functional Requirements

- **NFR-1 (Security)**: Private objects, short-lived credentials, fail-closed authorization, no secrets/tokens in logs or repos; cross-tenant test cases all pass.
- **NFR-2 (Reliability)**: MQ outage / worker crash / duplicate delivery / lost callback / timeout retries never create duplicate effective business versions; Outbox survives relay failure.
- **NFR-3 (Scientific integrity)**: Hub never recalculates authoritative metrics; stored values display identically (precision aside) to submitted values; definition/unit/algorithm provenance retained.
- **NFR-4 (Data safety)**: All schema changes are named forward-only EF migrations preserving existing dev data where feasible; no destructive change without analysis documented in the migration/PR.
- **NFR-5 (Independence)**: Each API builds, tests, publishes, and rolls back independently; two frontends have separate typed API clients with drift detection in CI.
- **NFR-6 (Observability)**: Correlation ID spans client operation → API → DB → outbox/queue → worker; health checks report DB/queue/object-store dependencies.
- **NFR-7 (Time/i18n)**: UTC storage with source offset; Chinese PDF rendering correct; UI language scope per PRD D-08 (no full i18n platform build).
- **NFR-8 (Build quality)**: `dotnet build`/`dotnet test` green; frontend type-check/lint/build green; warnings not treated as errors today but new code must not add warnings.

## Constraints

- **Technical**: .NET ecosystem as-is (SDK pin unresolved — see Open Questions); PostgreSQL 16; S3-compatible object storage (MinIO in dev); modular monolith inside each API; local transactions only; outbox+queue for cross-context/media; provider interfaces for OSS/VOD/SMQ so Aliyun products integrate without domain changes.
- **Architectural (binding, from handoff rules)**: do not merge the two APIs; no cross-context table writes; no full microservices; no Redis without justified requirement; no distributed transactions; no Hub-side scientific recalculation; preserve and reuse working code; no destructive schema changes without migration analysis; no feature reported complete without verification evidence; keep backlog/docs updated.
- **Business**: PRD remains "review draft, not frozen"; decisions D-01..D-16 (exhibition workflow, real iOS payloads, video spec, export formats, capacity numbers, cloud products) are external inputs that must not be invented. Engineering proceeds on contract placeholders/synthetic fixtures and adapts when inputs arrive.
- **Dependencies**: real anonymized iOS payloads (E0-03), identity-provider decision (E1-01), Aliyun PoC (E4-07/D-15). Work not blocked on these proceeds against documented assumptions.

## Assumptions

- A-1: Development/local environment substitutes: MinIO (S3 API) for OSS; an in-process/PostgreSQL-backed queue transport behind an `IMessageBus`/`IObjectStorage`/`IMediaProcessor` interface; a local media-processing stub (no real transcoding) that drives the same state machine. Cloud adapters are later, config-driven.
- A-2: Local JWT issuer remains for now (existing implementation) but is reconfigured to issue/validate two distinct audiences; managed OIDC integration stays an explicit, reversible decision.
- A-3: Existing Subscription Center code moves wholesale into the Platform context and is frozen (no new feature work) per D-08; Hub keeps a thin entitlement seam only where already wired.
- A-4: The existing dev database contains only seed/demo data; schema relocation migrations may reset dev data via the seeder, but must be forward-only and named.
- A-5: Synthetic ingestion fixtures modeled on the PRD/contract stand in for real iOS payloads until E0-03 delivers them; contract is versioned from day one (`v1`).
- A-6: Invite/controlled provisioning is the V1 account path (Blueprint §7); existing self-registration is retained but constrained/clearly marked pending the D-09 identity decision.

## Acceptance Criteria

### AC-1: Independent API hosts and call-path separation — `rule`
- **Given** the solution checked out and dependencies restored, **when** each host project is built/run independently, **then** `KWinHub.Platform.Api` and `KWinHub.Hub.Api` each build, start, expose their own `/health`, OpenAPI JSON and Swagger UI (dev), with separate appsettings and ports, without requiring the other to run.
- **Pass Condition**: `dotnet build` for both host projects succeeds; each health endpoint returns 200 independently; each OpenAPI document contains only that API's endpoints.
- **Evidence**: build output, running-process health checks, swagger JSON path listings in task evidence.

### AC-2: Distinct audiences and authorization policies — `rule`
- **Given** tokens minted for each audience, **when** a Platform token calls a Hub endpoint (and vice-versa), or a token with wrong/expired issuer/lifetime is presented, **then** the call is rejected (401/403); platform-only tokens without Hub membership cannot reach Hub business endpoints.
- **Pass Condition**: automated integration tests prove wrong-audience/wrong-scope/expired rejection on both hosts and `[Authorize(Policy=...)]` coverage on every non-anonymous controller action.
- **Evidence**: integration test results; authorization test inventory.

### AC-3: Separate schemas, DbContexts, migrations and write ownership — `rule`
- **Given** a clean PostgreSQL 16 database, **when** both contexts migrate independently, **then** Platform tables live in `platform` schema and Hub tables in `hub` schema; migrations are separate per context; an automated architecture test fails if a context's model/repositories reference the other context's tables or entities.
- **Pass Condition**: clean DB migrates via both contexts; schema/table ownership matrix documented in code; architecture test passes and deliberately-crossing code fails it (demonstrated).
- **Evidence**: migration list, `psql`/EF schema inspection, architecture test code + run.

### AC-4: Server-side tenant and membership enforcement — `rule`
- **Given** users with memberships in orgs A and/or B in various statuses, **when** they list/get/create/update/archive business resources with IDs belonging to other orgs, or after suspension/demotion, **then** every request is evaluated against current active membership + org status + resource ownership and is denied consistently without leaking existence or content.
- **Pass Condition**: cross-tenant test cases for list/detail/write/related-lookup all return 403/404 as designed; changed membership takes effect on the next request; no controller action accepts an org id from the client without membership validation.
- **Evidence**: tenant isolation integration suite (builds toward AC-22).

### AC-5: Platform→Hub lifecycle convergence — `rule`
- **Given** an organization suspended/activated in Platform, **when** the event flows through Outbox → transport → Hub consumer (including relay downtime, duplicate and out-of-order delivery), **then** Hub's read model converges idempotently; critical Hub operations re-verify authoritative status and no distributed transaction exists in code.
- **Pass Condition**: integration tests prove convergence, duplicate-event idempotency, relay-crash recovery, and fail-closed behavior while status is unknown/stale.
- **Evidence**: outbox/consumer tests, code search showing no cross-DB transaction APIs.

### AC-6: Real structured ingestion with provenance and atomicity — `rule`
- **Given** a valid versioned v1 session/trial/metric payload (synthetic fixture until real payloads land) from an authenticated member, **when** posted to Hub ingestion, **then** session, ordered trials, typed metric values with definition/unit/source/device/protocol/algorithm provenance and asset placeholders persist in one transaction and return stable Hub IDs; invalid payloads return catalogued, actionable errors.
- **Pass Condition**: real-PostgreSQL integration test persists and re-reads a complete graph; failure injection leaves no partial graph; no simulated metrics are generated anywhere in the production path (`ResultSimulator` isolated to test/demo fixtures).
- **Evidence**: integration tests, fixture payload, grep/architecture check that production projects contain no simulator invocation.

### AC-7: Idempotency and conflict semantics — `rule`
- **Given** stable client keys, **when** the same session/trial/asset submission is retried (including concurrent and timeout-after-success cases), **then** exactly one logical record exists and the prior result is returned; when the same key carries different content, **then** a conflict error is returned with no overwrite; correction/revision follows a separate, explicit path.
- **Pass Condition**: concurrency test (parallel duplicate submissions) asserts single row; conflict test asserts error and unchanged data; ingestion status API lets a restarted client recover state unambiguously.
- **Evidence**: integration test run with row counts and status API responses.

### AC-8: Independent verified asset upload — `rule`
- **Given** an accepted session with asset placeholders, **when** the client obtains a scoped short-lived upload grant, uploads directly to object storage (MinIO/S3 in dev), and completion is notified (once, repeatedly, or not at all), **then** the asset only reaches UploadedVerified after server-side object/size/type/checksum verification; failures stay per-asset and never invalidate accepted metrics; reconciliation closes a missing notification; grants cannot touch other objects.
- **Pass Condition**: integration/verification tests cover fail-before-verify, duplicate notification, missing notification + reconciliation, cross-asset grant misuse denial, and retry without duplicate logical assets.
- **Evidence**: object-store-backed test run (Testcontainers or compose MinIO), asset state history rows.

### AC-9: Reliable media pipeline — `rule`
- **Given** a verified video asset, **when** processing is requested through Outbox → transport → worker with a media-provider abstraction, **then** provider/process IDs persist before message ack; crashes, duplicates, visibility expiry, ambiguous external success, and lost/out-of-order callbacks are reconciled to exactly one effective processing version; queues for media vs report work are isolated; terminal failures persist with replay support.
- **Pass Condition**: resilience tests for worker crash mid-processing, duplicate delivery, unknown provider state → query-first, stale Queued/Processing reconciliation, and dead-letter/replay all pass against the local transport + local processor stub.
- **Evidence**: worker integration test run with asserted job/version counts and state transitions.

### AC-10: Authorized playback — `rule`
- **Given** Ready and non-Ready assets and users with varying membership/revocation states, **when** playback is requested, **then** credentials are issued only after re-checking org/subject/report/asset permission and media revision validity; non-Ready assets return state, never credentials; revoked users get no new credentials; URLs are short-lived and scoped.
- **Pass Condition**: playback authorization tests cover cross-tenant asset id, revoked membership, non-Ready asset, and credential expiry/renewal.
- **Evidence**: authorization integration tests.

### AC-11: Real-data history, session review and comparison — `rule`
- **Given** persisted real ingested data, **when** an authorized user filters athlete history and opens a session, **then** trials, metric values, units, provenance and per-asset states are returned from Hub data; comparison includes only definition/unit/context-compatible metrics with reasons for exclusion; basic statistics carry sample count and inclusion rules; production Web.React never falls back to demo data and shows explicit unknown/missing values.
- **Pass Condition**: UI driven by Hub.Api in API mode shows ingested records identically; tests prove incompatibility exclusion, missing ≠ zero, and statistics traceability; production build fails or hard-errors rather than silently demoing.
- **Evidence**: E2E/integration tests, production-mode env/build verification, mapper audit.

### AC-12: Versioned plan, complete report and exports — `rule`
- **Given** one fixed exhibition assessment plan (fixture-defined until D-01/D-02 freeze), **when** a user selects eligible sessions/trials for an authorized athlete and generates a report, **then** the system validates required metrics/versions/units, blocks a "complete" report with missing requirements (allows marked draft), snapshots plan version + exact source IDs + values/units, renders an online report with per-video states, and exports Chinese-capable PDF and CSV under the same authorization with audit; plan edits never mutate historical reports.
- **Pass Condition**: report generation/immutability/export tests pass; PDF renders Chinese text without garbling; export respects masking and is audited; XLSX is not built unless decided.
- **Evidence**: generated artifacts, export/audit tests.

### AC-13: PlatformConsole administration cutover — `rule`
- **Given** the production Console build, **when** any administration page loads, **then** it calls Platform.Api with platform-admin policy only; mock mode is impossible in production configuration; organization lifecycle, user administration and global catalog mutations are authorized and audited; no customer athlete/session/report workflow exists in Console.
- **Pass Condition**: build with production env contains no mock path (flag hard-off/guarded), E2E smoke of org suspend → audit row, typed client imports Platform contracts only.
- **Evidence**: production build inspection, E2E/integration evidence.

### AC-14: Frontend typed clients and CI drift detection — `rule`
- **Given** both OpenAPI documents, **when** frontends generate/refresh their typed clients and CI runs, **then** PlatformConsole imports only Platform types and Web.React only Hub types; type-check/lint/build and backend build/test/migration checks run on PRs and fail the branch on breakage.
- **Pass Condition**: CI workflow exists and passes on a sample PR; cross-import attempt fails type-check.
- **Evidence**: CI config + run logs.

### AC-15: Outbox/business-state transactional consistency — `rule`
- **Given** any state change that emits an event (asset verified, org lifecycle, report job), **when** the transaction commits, **then** the outbox row commits in the same local transaction; the relay publishes only committed rows and is resumable after crash; message payloads carry only IDs/references/versions (never bytes/secrets/long-lived URLs).
- **Pass Condition**: test asserting outbox row visible only with business state; relay crash/restart test; payload contract review.
- **Evidence**: integration tests, envelope/contract code.

### AC-16: No cloud-side scientific computation — `rule`
- **Given** the deployed V1 codebase, **when** sessions are ingested and processed, **then** no production component computes scientific metrics from raw signals; `ResultSimulator` exists only in test/demo fixtures; the worker performs media/report coordination only.
- **Pass Condition**: code/architecture test plus code review evidence; worker job types enumerate media/report only.
- **Evidence**: architecture test, job registry.

### AC-17: Observability and recoverability — `rule`
- **Given** a running local stack, **when** a request fails or a job stalls, **then** correlation IDs connect API logs, DB records, outbox events and worker logs; health checks report dependencies; dashboards/metrics exist for outbox age, queue depth/age, worker failures, upload/processing failures; backup/restore plus deletion-replay procedure is documented and rehearsed at least once.
- **Pass Condition**: trace a seeded failure end-to-end by correlation ID; health JSON includes DB/storage/queue; restore rehearsal record produced in G3.
- **Evidence**: logs, health output, runbook section + rehearsal note.

### AC-18: Pilot acceptance sweep (PRD AC matrix) — `rule`
- **Given** the frozen P0 acceptance set, **when** the full suite runs, **then** scenarios equivalent to PRD AC-01..AC-24 (ingestion, duplicates, recovery, tenant/role attacks, incompatible metrics, plan selection, report immutability, exports, media/message failures, playback/expiry, load gates for frozen numbers) pass on real infrastructure substitutes, with no open P0 defect.
- **Pass Condition**: test matrix maps each PRD AC to an automated test or documented manual pilot case with evidence; all P0 rows green.
- **Evidence**: traceability matrix + results in review.md.

### AC-19: Migration rehearsal readiness — `rule`
- **Given** documented legacy inventory and mapping rules, **when** representative sample data is migrated in batches with the tooling, **then** reruns are idempotent, counts/checksums reconcile, anomalies are isolated (never fabricated), and a rollback path is documented; no production cutover is performed.
- **Pass Condition**: trial migration on a sample dataset with reconciliation report; rerun produces no duplicates.
- **Evidence**: migration run logs + reconciliation report (G3).

### AC-20: Engineering quality and repository hygiene — `rubric`
- **Dimension**: Codebase maintainability, architectural consistency, and evidence discipline across the change.
- **Scale**: 1–5.
- **Anchors**: 1 = hosts remain entangled, dead demo/simulator paths in production, features without tests; 3 = boundaries correct with some leaked abstractions or thin test evidence; 5 = strict modular boundaries, reused existing code, every slice has real automated verification and updated docs/backlog.
- **Pass Threshold**: ≥ 4.
- **Evidence**: code review, architecture tests, task Completion Evidence records, updated backlog status.

## Open Questions

- [ ] Q-1 **Build toolchain (blocker)**: Install .NET SDK 10.0.x to match `global.json`/`net10.0`, or retarget the solution to net9.0 (EF/Jwt packages to 9.x)? Recommendation: install .NET 10 SDK — no production data, lowest churn.
- [ ] Q-2 **Identity provider (D-01/E1-01)**: Proceed on the existing local JWT issuer with two audiences now and defer managed OIDC, or is an OIDC provider available now?
- [ ] Q-3 **Local cloud substitutes (D-15)**: Accept MinIO + in-process/PG queue + local media stub behind provider interfaces as the dev/CI target?
- [ ] Q-4 **Subscription Center (D-08)**: Confirm relocate-into-Platform-and-freeze approach (no deletion, no new features).
- [ ] Q-5 **Exhibition workflow + real payloads (D-01/D-03)**: Who delivers anonymized real iOS payloads and by when? Until then synthetic v1 fixtures define the contract and are replaced when samples arrive.
- [ ] Q-6 **Video spec / export XLSX / capacity numbers (D-02/D-05/D-14)**: External freezes; implementation uses configurable limits and PDF+CSV defaults.
- [ ] Q-7 **Registration policy (D-09/D-10)**: Keep existing self-registration for dev, implement invite-only provisioning for V1 per Blueprint §7 — confirm.
- [ ] Q-8 **Execution cadence**: Proceed milestone-by-milestone (M0 foundation → M1 identity → M2 workspace → M3 ingestion → M4 media → M5 review/reports → M6 console → M7 hardening) with review/approval after each milestone, as the handoff mandates?
