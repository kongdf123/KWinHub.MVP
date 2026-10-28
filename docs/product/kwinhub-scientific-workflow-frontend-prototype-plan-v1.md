# KWinHub Scientific Workflow — Frontend Prototype Implementation Plan V1

**Status:** Proposed implementation specification (not approved; no repository changes made)  
**Date:** 2026-10-28  
**Target repository:** `kongdf123/KWinHub.MVP` (`main` inspected 2026-10-28)  
**Suggested destination:** `docs/product/kwinhub-scientific-workflow-frontend-prototype-plan-v1.md`  
**Target applications:** `KWinHub.PlatformConsole` and `KWinHub.Web.React`  
**Scope:** Frontend-only, deterministic stakeholder demo of CMJ + Sprint Start  
**Audience:** Product stakeholders/management; coding agent implementing one backlog item at a time

## 0. AI implementation contract — READ FIRST

1. **Do not implement the scientific backend in this initiative.** Do not change `src/backend`, database schemas, migrations, API endpoints, object storage, worker processes, desktop/iPad acquisition clients, authentication, or production tenant authorization. These remain future targets defined by the architecture documents.
2. **Do not create duplicate PlatformConsole catalog pages.** Existing `AlgorithmVersionsPage.tsx`, `MetricCatalogPage.tsx`, and `ProtocolCatalogPage.tsx` must be reviewed and enhanced, not replaced by unrelated registries.
3. **Preserve existing application separation:** `KWinHub.PlatformConsole` serves platform managers; `KWinHub.Web.React` serves organization managers and individual users. Platform-scientific administration must never appear as unrestricted controls for organization/individual users.
4. **Use deterministic fixtures, not fake scientific computation.** Every numeric example is labeled *demo/illustrative*, and no action implies a working calculation engine, verified device upload, or real release approval. Do not simulate by generating random biomechanical values.
5. **Do not break existing API mode.** Demo scientific behavior must be explicit and isolated; API mode should disable unavailable actions with an informative explanation rather than silently fabricate success.
6. **Complete one backlog item at a time:** inspect relevant existing code; implement its limited scope; run applicable frontend checks; report changed files, tests/results, and known limitations; then proceed to the next item only when requested or explicitly authorized.
7. **Never mark a backlog item complete solely because UI renders.** Verify listed acceptance criteria including navigation, roles, state invariants, and reset behavior.
8. **No hardcoded user secrets or real athlete data** in fixtures; use anonymized synthetic identities.
9. **Prefer small additions/reuse.** Keep existing design tokens, layouts, routing patterns, bilingual strings and chart components. Avoid global restyles and new state-management dependencies unless justified.
10. **Before editing:** re-read repository `main` or current working branch because this plan reflects an inspection snapshot; adapt paths cautiously and report significant divergences.

## 1. Intent, product decisions and boundaries

### 1.1 Objective

Demonstrate a coherent product narrative: data acquired by KWinPerformance (iPad force plates) and KWinSprint (Windows starting blocks) can be associated with one athlete, validated, calculated with governed scientific definitions, published, inspected and compared in KWinHub. All workflows are simulated locally for stakeholder validation.

### 1.2 Explicit decisions

| Decision | Approved working assumption for this prototype |
|---|---|
| Implementation | Frontend-only in existing two React applications |
| Reference datasets | CMJ and Sprint Start, using the same fictional athlete |
| Stakeholder emphasis | Clear workflows, cross-product value, credibility of scientific provenance |
| Formula execution | None; use pre-authored deterministic metric/event fixtures |
| Scientific authority target | Hub authoritative calculations in future backend; NOT implemented now |
| Platform ownership | Central catalog/algorithm governance in PlatformConsole |
| Organization ownership | Use approved protocols; inspect own measurements/results; request permitted recalculation |
| Individual users | Only authorized personal/visible results; no scientific governance operations |
| Release promotion | Simulated platform-level governance only; do not silently activate new result revision |
| Scope exclusions | No real ingestion, cloud job queue, scientific parity assertion, formulas editor, AI interpretation, normative engine, new backend authorization |

### 1.3 Source references — target design, not implemented features

- [`docs/integrations/kwinhub-integration-architecture-roadmap-v2.md`](../integrations/kwinhub-integration-architecture-roadmap-v2.md) — client acquisition vs Hub authority, canonical subject/session/trial mapping, pipeline, version governance, offline states.
- [`docs/architecture/scientific-calculation-engine-v1.md`](../architecture/scientific-calculation-engine-v1.md) — scientific entities, status dimensions, input verification, calculation jobs, immutable result revisions, provenance.
- [`docs/product/kwinhub-mvp-prd-v1.1.md`](kwinhub-mvp-prd-v1.1.md) and existing product/workflow specs — existing UX context, preserve unless explicitly changed by this plan.

**Evidence note:** These two architectural documents are proposed reviews; their API routes, new entity names, workers and algorithms are *not* evidence of implemented backend functionality. This plan is a frontend consumer/admin prototype based on their concepts.

## 2. Product-area responsibility and permissions

| Capability | PlatformConsole (platform manager) | Web.React organization manager | Web.React individual |
|---|---|---|---|
| Algorithm catalog, versions, input/output contracts | Manage/display simulated catalog and governance | View approved applied release/method | View result's applied release/method where permitted |
| Algorithm release approval/deprecation | Platform-only simulated action | No | No |
| Metric definitions and protocol templates | Central catalog display/management (existing pages) | Read approved protocol; select permitted preset | Read permitted protocol/test explanation |
| Per-organization protocol parameters | Define allowed ranges/presets conceptually | Choose approved preset if UI allows | Read only |
| Session/trial and athlete data | No default broad athlete browsing (support-only in future) | Own-organization results and history | Own/authorized data only |
| Upload/validation/calculation lifecycle | Aggregate platform demo monitoring (P1) | View own sessions/jobs | View authorized results/status |
| Recalculation | Define allowed releases and policy | Request simulated recalculation if authorized | Read only |
| Result revision promotion | Platform-scientific governance demo only (subject to future policy) | No platform-level approval | No |
| Revision comparison | Limited support/audit demo only | Compare permitted result revisions | Read permitted revisions |

**Security caveat:** Frontend visibility is *not* tenant security. Existing app role gates should remain intact; do not make claims of production authorization or introduce broad platform access to athlete records for the sake of demo convenience.

**UX rule:** expose scientific administration in PlatformConsole; expose scientific *outcomes and explanation* in Web.React. Avoid a new general-purpose "Scientific Engine" administration section in Web.React.

## 3. Current repository baseline / reuse map

Observed on GitHub `main` during review:

### 3.1 PlatformConsole — EXISTING, REUSE

- `src/frontend/KWinHub.PlatformConsole/src/pages/AlgorithmVersionsPage.tsx` — existing algorithm version catalog UI and forms, uses `lib/api.ts`; **extend**, don't duplicate.
- `src/frontend/KWinHub.PlatformConsole/src/pages/MetricCatalogPage.tsx` — existing metric catalog UI; **extend** with scientific meaning/method read-only demo fields.
- `src/frontend/KWinHub.PlatformConsole/src/pages/ProtocolCatalogPage.tsx` — existing global protocol catalog; **extend** mapping/requirements panel.
- `src/frontend/KWinHub.PlatformConsole/src/lib/api.ts` and `src/lib/mockApi.ts` — API abstraction plus in-memory mock behavior; respect current API contracts and avoid mock-only leakage into real mode.
- `src/frontend/KWinHub.PlatformConsole/src/components/Layout.tsx`, `src/App.tsx`, `src/lib/router.tsx` — existing navigation/layout/hash route conventions; preserve.

### 3.2 Web.React — EXISTING, REUSE

- `src/frontend/KWinHub.Web.React/src/pages/SessionsPage.tsx` — search/filter/list and session status; currently uses direct `DEMO_*` fixtures.
- `src/frontend/KWinHub.Web.React/src/pages/SessionDetailPage.tsx` — existing trials, force-time view, lifecycle and nine tabs; recompose, don't rebuild.
- `src/frontend/KWinHub.Web.React/src/pages/ResultDetailPage.tsx` — existing result summary and visualizations; direct `DEMO_*` access.
- `src/frontend/KWinHub.Web.React/src/pages/SyncCenterPage.tsx` — existing sync queue and status, direct fixtures.
- `src/frontend/KWinHub.Web.React/src/pages/AthleteProfilePage.tsx` — existing history/timeline.
- `src/frontend/KWinHub.Web.React/src/pages/ProtocolDetailPage.tsx` — protocol presentation.
- `src/frontend/KWinHub.Web.React/src/lib/types.ts`, `src/lib/services.ts`, `src/lib/demo-data.ts` — current DTOs, dual `VITE_DATA_MODE` demo/API service and fixtures. Avoid breaking compatibility.

**Important gap:** separate `SessionStatus` does not fully capture upload vs scientific validation vs calculation vs publication; introduce a *projection* with separate statuses rather than extending one global enum without limit.

**Shared-contract packaging note:** Two separate Vite frontend applications should use **one canonical scientific fixture schema**. Start with a small repository-local shared TypeScript package if their module/build settings allow it without disproportionate configuration, e.g. `src/frontend/shared/scientific-demo/` containing types and static JSON/TS fixtures. Otherwise copy the exported *data projection* through a lightweight generated import at build time or create identical typed fixtures with a consistency test. **Do not force a monorepo workspace migration solely for this demo.** PlatformConsole and Web.React may keep separate UI stores; shared *definitions/IDs/fixture outputs* must remain consistent. Any actions across apps are illustrative and need not propagate live between two open browser windows.

## 4. Stakeholder demo narrative and fixture requirements

### 4.1 Four curated demo stories

| Story | Demonstrates | Required visible result |
|---|---|---|
| A — CMJ normal | KWinPerformance → Hub scientific lifecycle → athlete result | One published CMJ trial, force-time events, metrics with method/units, provenance |
| B — Sprint normal | KWinSprint front/rear source devices → Hub → same athlete | Multi-channel force traces/device role identities, event markers, published sprint metrics |
| C — Failure and recovery | Uploaded does not imply scientifically valid | One upload failure recoverable with retry; one checksum/schema/calibration validation failure requiring correction |
| D — Recalculation | Scientific definitions and immutable revision lineage | Revision 1 unchanged; new Revision 2 candidate; method/version comparison; simulated governance promotion separated |

### 4.2 Synthetic fixture pack

- `athlete-demo-001`: fictional, stable across CMJ and Sprint sessions.
- `session-cmj-001` with `trial-cmj-001`: force-plate source(s), known sample rate, timestamped onset/takeoff/landing examples, metric codes distinct by method.
- `session-sprint-001` with `trial-sprint-001`: starting-block *front/rear* devices and optional camera metadata, synchronized-or-uncertain clock flags, sprint-specific events and metrics.
- `session-cmj-upload-failed`, `session-cmj-input-rejected`: failure scenarios with specific user-actionable demo diagnostics.
- `result-cmj-r1` and `result-cmj-r2`: same trial, differing illustrative approved/candidate algorithm versions and explicit `supersedesId`; r2 initially not active.
- `algorithm-cmj-force-time`, `algorithm-sprint-start`: catalog items with input requirements, output metric codes, version and release state.
- Include fixture provenance (`fixtureId`, `demoOnly: true`, schemaVersion), clear units/methods, and no actual personal data.

All numbers, signal curves, algorithm versions and names in fixture examples are **synthetic placeholders** and must never be presented as independently validated biomechanics or parity against legacy C# calculations. Maintain physically coherent event order/time ranges, but do not infer scientific validity from plotting a plausible curve.

### 4.3 Demo execution rules

- Each demo story has named steps and a `Reset demo` action.
- User-driven `Next step`, `Retry upload`, `Request recalculation` changes local demo state deterministically; no unexpected automatic transitions required.
- Refresh behavior should be deliberate: reset to a documented default fixture or restore from URL scenario query (prefer shareable URL; persistent browser storage optional).
- No real progress percentages or elapsed times unless sourced from scripted fixture steps and labeled simulated.
- Avoid random latency, IDs, metrics and event timing. Stable outputs make stakeholder review and automated tests reproducible.
- Demo mode clearly labeled. API mode does not silently report successful calculation/release operations.

## 5. Frontend contracts (prototype read models, NOT EF entities)

Suggested location: `src/frontend/shared/scientific-demo/types.ts` if shared build integration is straightforward, otherwise `Web.React/src/features/science/contracts/scientific-types.ts` with a parallel PlatformConsole typed projection and consistency test.

```ts
export type ScientificSyncStatus = 'local' | 'queued' | 'uploading' | 'uploaded' | 'failed';
export type InputValidationStatus = 'awaiting_input' | 'verifying' | 'verified' | 'rejected';
export type CalculationStatus = 'not_started' | 'queued' | 'processing' | 'succeeded' | 'failed';
export type PublicationStatus = 'not_published' | 'published';
export type ReleaseStatus = 'draft' | 'validated' | 'approved' | 'deprecated';
export type ScientificProtocolCode = 'CMJ' | 'SPRINT_START';

export interface ScientificAlgorithm {
  code: string;
  name: string;
  version: string;
  releaseStatus: ReleaseStatus;
  inputSchemaVersion: string;
  supportedProtocols: ScientificProtocolCode[];
  requiredChannels: string[];
  outputMetricCodes: string[];
  parameterDescriptions: Array<{code:string; label:string; unit?:string; defaultDisplayValue?:string}>;
  demoOnly: true;
}
export interface ScientificMetricValue {
  metricCode: string;
  definitionVersion: string;
  label: string;
  method: string;
  value: number | null;
  unit: string;
  side?: 'left'|'right'|'front'|'rear'|'total';
  qualityFlags: string[];
}
export interface ScientificEvent {
  code: string;
  label: string;
  offsetFromTrialStartUs: number;
  detectionMethod?: string;
  confidence?: number;
}
export interface ScientificDeviceRef {
  deviceId: string;
  role: 'force_plate_left'|'force_plate_right'|'starting_block_front'|'starting_block_rear'|'camera'|'other';
  label: string;
  sampleRateHz?: number;
  timeOffsetUs?: number;
  synchronizationQuality?: 'verified'|'estimated'|'unknown';
}
export interface ScientificRawAsset {
  id: string;
  trialId: string;
  kind: 'force_signal'|'timing'|'video';
  deviceId: string;
  schemaVersion: string;
  sampleCount?: number;
  sampleRateHz?: number;
  unit?: string;
  checksumSha256?: string; // omit unless a complete known digest is provided
  validationStatus: InputValidationStatus;
}
export interface ScientificResultRevision {
  id: string;
  trialId: string;
  revision: number;
  status: 'draft'|'validated'|'published';
  isActive: boolean;
  supersedesId?: string;
  algorithmCode: string;
  algorithmVersion: string;
  protocolVersion: string;
  inputFingerprint?: string;
  reason?: string;
  metrics: ScientificMetricValue[];
  events: ScientificEvent[];
  qualityFlags: string[];
  publishedAt?: string;
}
export interface ScientificTrial {
  id: string;
  sessionId: string;
  index: number;
  syncStatus: ScientificSyncStatus;
  inputStatus: InputValidationStatus;
  calculationStatus: CalculationStatus;
  publicationStatus: PublicationStatus;
  devices: ScientificDeviceRef[];
  inputs: ScientificRawAsset[];
  resultRevisions: ScientificResultRevision[];
  activeResultRevisionId?: string;
  failureCode?: string;
  failureMessage?: string;
}
export interface ScientificSession {
  id: string;
  athleteId: string;
  sourceApplication: 'KWinPerformance'|'KWinSprint';
  protocolCode: ScientificProtocolCode;
  protocolVersion: string;
  startedAt: string;
  trials: ScientificTrial[];
}
```

**Modeling guidance:**

- Do not equate `Algorithm` with `Protocol`: a protocol defines a test and required inputs; an algorithm release computes a specific set of metrics/events.
- Do not equate athlete/session/trial: one athlete has many sessions, one session many trials, one trial multiple source devices and revisions.
- Distinguish *metric code + method + definition version*, not just the human display name.
- Conversion for UI display must not mutate canonical metric meaning.
- Events refer to explicit trial-relative time axes; never mix independent device clocks without showing sync confidence.
- Only published, active results should be presented as default authoritative results; candidate revisions clearly labeled.
- Use projections and adapters into existing `TestSession`, `Trial`, `Result`, `SyncJob`; avoid replacing them globally in this initiative.

## 6. Page-level specs

### PC-01 — PlatformConsole: Algorithm Versions (P0)

**Existing page:** `src/frontend/KWinHub.PlatformConsole/src/pages/AlgorithmVersionsPage.tsx`  
**Role:** Platform manager only.  
**Intent:** Explain scientific governance and calculation releases, without pretending a formula editor exists.

**Enhance existing list/detail/modal** with algorithm code, domain (CMJ/Sprint), version, release status, required inputs, output metric count, supported protocol(s), approval/release notes and illustrative effective status. Add detail drawer/section with input channel requirements, parameter descriptions, computed metric codes, link to protocol catalog. Use existing UI component patterns. Existing generic algorithm catalog API may not expose these fields; join demo-only projection by stable code/ID rather than modifying backend contracts.

**Demo interactions:** filter by protocol/status; open detail; simulate `Draft → Validated → Approved` for a *candidate fixture* with an explicit `Demo action` label; no arbitrary formula editing; no destructive actions on real API records. A version's status must not mutate previously published result revision fixtures.

**Acceptance:** CMJ and Sprint versions are discoverable; versions/definitions are distinct; unauthorized organization Web.React does not show these controls; API-mode behavior remains accurate.

### PC-02 — PlatformConsole: Metric Catalog (P0)

**Existing page:** `src/frontend/KWinHub.PlatformConsole/src/pages/MetricCatalogPage.tsx`.  
Extend with method, definition version, canonical unit, side/scope, explanation, used-by algorithm. Demonstrate `CMJ_JUMP_HEIGHT_FLIGHT_TIME` vs `CMJ_JUMP_HEIGHT_IMPULSE_MOMENTUM` as different metric definitions, not one merged display label. Read-only scientific demo metadata is sufficient; do not add live metric-definition migrations.

**Acceptance:** two same-label concepts with different methods remain distinguishable; definitions map to Result Detail displays.

### PC-03 — PlatformConsole: Protocol Catalog (P1)

**Existing page:** `src/frontend/KWinHub.PlatformConsole/src/pages/ProtocolCatalogPage.tsx`.  
Add scientific readiness: required input channels, recommended/approved algorithm release, supported devices and output metric set. Link to algorithm detail; existing protocol creation/edit forms remain stable. Show exactly which content is catalog demo enrichment.

**Acceptance:** CMJ and Sprint Start show distinct input requirements; a protocol refers to an algorithm release, not its formula implementation.

### PC-04 — PlatformConsole: Calculation Monitoring (P1)

**New page only if existing platform dashboard cannot accommodate it without confusion.** First assess `DashboardPage.tsx`/existing platform routes. Read-only scripted aggregate of fake calculation jobs: queued, processing, published, validation failure and failure category. No unrestricted personal athlete drill-down; use anonymized identifiers. Platform oversight does not imply broad subject access.

**Acceptance:** statuses match shared fixtures; no claim of live backend queue monitoring; no production endpoints introduced.

### WEB-01 — Sessions List (P0)

**Existing:** `Web.React/src/pages/SessionsPage.tsx`.  
Preserve search, protocol/source/status/date filters. Add product/source application, a concise derived scientific lifecycle summary, a `Featured scenarios` filter or demo launcher, and links to same-athlete CMJ and Sprint records. Avoid expanding tables beyond readable widths; source/quality tooltip or details drawer as needed.

**Acceptance:** featured records are discoverable, navigate to correct session, and show consistent state with Sync Center.

### WEB-02 — Session Detail (P0)

**Existing:** `Web.React/src/pages/SessionDetailPage.tsx`; currently has nine tabs and `DEMO_SESSIONS` direct access.  
Refactor data source into shared scientific selectors. Group content under four main tabs: `Overview`, `Trials & Signals`, `Scientific Results`, `Processing & Provenance`. Retain access to raw data, quality and reports as nested/advanced subpanels; preserve role gates (particularly raw/diagnostic access). Do not simply delete existing functionality.

**CMJ:** force-time visualization, athlete mass metadata as *source label* if shown, onset/takeoff/landing markers, metric method/unit/quality.  
**Sprint:** front/rear block signals with device-role labels, synchronized/uncertain timing, start-related events; optional camera metadata only (no genuine synchronized playback required).  
**Lifecycle:** separate upload, validation, job and publication stages; failure screen indicates next step.

**Acceptance:** both protocols render correctly; event markers correspond to fixture time axes; no assumption that `DeviceId` single field represents multi-device session; permission-sensitive panels still gated.

### WEB-03 — Result Detail and Revisions (P0)

**Existing:** `Web.React/src/pages/ResultDetailPage.tsx`.  
Show metric cards and exact codes/method/unit on expansion, event list linked to timeline, algorithm/protocol version, quality flags, input provenance display, revision selector and diff. Distinguish active Revision 1 from candidate Revision 2. Compare only same `metricCode + definitionVersion + method`, converting compatible units for display if an existing safe formatter exists; otherwise label incomparable and suppress delta. Do not compute biomechanics from raw traces.

**Demo interaction:** org manager can request simulated recalculation under explicit demo controls; individual read only. Recalculation must add a separate candidate revision; platform governance approval/promotion represented separately and not silently applied.

**Acceptance:** old result immutable, no automatic active revision switch, method-mismatched metrics not compared numerically, API mode does not fake a calculation.

### WEB-04 — Sync Center (P0)

**Existing:** `Web.React/src/pages/SyncCenterPage.tsx`.  
Split transfer status, scientific input verification, job status and publication. Provide scripted detail/error modal. Separate `Retry Upload` (recoverable transfer error) from `Correct input` (validation error; no blind retry) and `Request recalculation` (new revision). Keep existing synchronization overview where compatible.

**Acceptance:** uploaded + rejected scientifically is representable; a failed optional video asset does not block required force calculation in the fixture unless marked required; retry behaves deterministically.

### WEB-05 — Athlete Profile (P0)

**Existing:** `Web.React/src/pages/AthleteProfilePage.tsx`.  
Show same synthetic athlete linked to CMJ and Sprint test sessions, source application and published status. Deep-link to correct session/result. Keep cross-protocol metrics distinct: no invented universal performance score.

**Acceptance:** history is shared by athlete ID, no duplicate athlete per client, navigation round trips work.

### WEB-06 — Protocol Detail (P1)

**Existing:** `Web.React/src/pages/ProtocolDetailPage.tsx`.  
Read-only section describing approved scientific inputs, algorithm/version, outputs, required channels, validations and applicable preset. No arbitrary algorithm edit/release approval. Existing page is rich; append focused scientific information rather than rebuilding it.

**Acceptance:** users can understand protocol vs algorithm vs metric distinctions without admin controls.

## 7. Proposed implementation files

**Inspect exact paths/exports before editing. Proposed new paths may be adjusted to existing conventions.**

### 7.1 Shared read models and fixtures

```text
src/frontend/shared/scientific-demo/              # conditional: only if lightweight sharing compiles
  types.ts
  protocols.ts
  fixtures.ts
  scenarios.ts
```

Alternative: put same contracts under `KWinHub.Web.React/src/features/science/contracts/`, and use a small PlatformConsole adapter for a static exported catalog fixture; add a consistency test for stable codes/versions/IDs. **Choose one approach, document rationale, avoid a full workspace reorganization.**

### 7.2 PlatformConsole changes

```text
src/frontend/KWinHub.PlatformConsole/src/
  pages/AlgorithmVersionsPage.tsx     # MODIFY
  pages/MetricCatalogPage.tsx         # MODIFY
  pages/ProtocolCatalogPage.tsx       # MODIFY (P1)
  pages/DashboardPage.tsx             # MODIFY (optional P1 monitor)
  lib/mockApi.ts                      # EXTEND ONLY IF necessary; keep demo scoped
  lib/api.ts                          # DON'T change live DTO contract without backend support
  components/ScientificReleaseDetail.tsx   # NEW optional
  components/ScientificMetricDetail.tsx    # NEW optional
```

### 7.3 Web.React changes

```text
src/frontend/KWinHub.Web.React/src/
  pages/SessionsPage.tsx              # MODIFY
  pages/SessionDetailPage.tsx         # MODIFY
  pages/ResultDetailPage.tsx          # MODIFY
  pages/SyncCenterPage.tsx            # MODIFY
  pages/AthleteProfilePage.tsx        # MODIFY
  pages/ProtocolDetailPage.tsx        # MODIFY (P1)
  lib/services.ts                     # ADD a demo-science adapter, keep VITE_DATA_MODE semantics
  lib/types.ts                        # MINIMAL additive changes only
  features/science/
    services/scientific-demo-store.ts # NEW deterministic central state
    services/scientific-selectors.ts  # NEW read projections
    hooks/useScientificDemo.ts        # NEW shared subscription hook
    components/ProcessingTimeline.tsx
    components/MultiDeviceSignalChart.tsx
    components/ScientificMetricCard.tsx
    components/ScientificEventTimeline.tsx
    components/ScientificProvenance.tsx
    components/ResultRevisionComparison.tsx
    components/DemoScenarioControls.tsx
    tests/                             # NEW tests using project's existing framework
```

## 8. Prioritized backlog — implement one item per agent turn/PR

Each item has dependencies, scope, evidence and a pass condition. **P0 is prototype-essential. P1 is optional for a first demo.** Effort is relative sizing only, not a schedule or delivery commitment.

| ID | Priority | App | Task | Depends on | Completion evidence |
|---|---|---|---|---|---|
| SCI-FE-001 | P0 | Shared | Audit current frontend build modes, navigation, existing catalog schemas; select fixture-sharing approach and document it | None | Short repo-aligned decision note; both apps still build |
| SCI-FE-002 | P0 | Shared | Add scientific DTOs, deterministic fixture pack (CMJ, Sprint, failure, revisions), status derivation | 001 | Type-check and invariant tests; stable IDs, methods, time axes |
| SCI-FE-003 | P0 | Web.React | Add shared demo scientific store/selectors and user-driven scenario controls, explicit demo/API isolation | 002 | Reset/advance/retry tests; no fabricated API-mode success |
| SCI-FE-004 | P0 | PlatformConsole | Enhance existing AlgorithmVersionsPage with scientific release detail and simulated approval lifecycle | 002 | Both algorithm families shown; existing page/API behavior preserved |
| SCI-FE-005 | P0 | PlatformConsole | Enhance existing MetricCatalogPage with method-specific versioned scientific definitions | 002 | Flight-time and impulse–momentum not conflated; links/codes consistent |
| SCI-FE-006 | P0 | Web.React | Enhance Sessions list with source app, derived scientific status, featured demo sessions | 003 | Filter/navigation and status consistency tests |
| SCI-FE-007 | P0 | Web.React | Consolidate Session Detail tabs; implement CMJ and front/rear Sprint signals/events and processing timeline | 003,006 | Both protocols readable; original advanced role gates preserved |
| SCI-FE-008 | P0 | Web.React | Result Detail: method-aware metric cards, provenance, revision selector and comparison | 003,007 | Revisions immutable; incompatible methods suppress comparison |
| SCI-FE-009 | P0 | Web.React | Sync Center: independent transfer/verification/calculation/publication states, failure and retry | 003,006 | Uploaded-but-invalid state and retry cases pass |
| SCI-FE-010 | P0 | Web.React | Athlete Profile: unified same-athlete CMJ and Sprint timeline | 003,006 | Cross-product deep links correct |
| SCI-FE-011 | P0 | Both | Demo hardening: bilingual copy, scenario walkthrough, regression tests, build/type-check, responsive/accessibility review | 004–010 | Full acceptance matrix in §9 passed |
| SCI-FE-012 | P1 | PlatformConsole | Enhance existing ProtocolCatalogPage with input requirements and pinned approved algorithm | 004,005 | Correct CMJ/Sprint mapping; no new backend API |
| SCI-FE-013 | P1 | Web.React | Enhance Protocol Detail with approved scientific definitions/preset read model | 008,012 | No org-side algorithm approvals |
| SCI-FE-014 | P1 | PlatformConsole | Add read-only simulated calculation monitoring in existing dashboard or minimal new route | 002,004 | Aggregate fixture consistency; no personal athlete disclosure |

**Stop/go gate:** Complete P0 items and run a stakeholder walkthrough before committing to P1. If feedback indicates protocol governance is critical, reprioritize PC-03 ahead of visual polish. Do not begin scientific backend work under this plan.

### 8.1 Coding-agent invocation template

Use this when assigning each item:

```text
Implement ONLY backlog item SCI-FE-00X in
`docs/product/kwinhub-scientific-workflow-frontend-prototype-plan-v1.md`.

First inspect current repository source and its dependencies.
Do not modify backend or invent production API support.
Reuse existing page/components and preserve both apps' roles.
Add/update tests for the item's acceptance criteria.
Run the relevant frontend build/type-check/tests (or state why unavailable).
Report:
1) changed files and rationale,
2) behavior demonstrated,
3) commands executed and results,
4) deviations/limitations,
5) next dependent item.
Do not implement other backlog items in this turn.
```

### 8.2 Minimal per-item acceptance test examples

- 002: every event time is nonnegative, ordered as specified per protocol and within fixture curve duration; published revision has metrics and an algorithm version; invalid input has no published authoritative result.
- 003: independent page subscribers see the same updated scenario state; reset reproduces identical output; selecting scenario is deterministic.
- 004: platform simulated release approval cannot modify published result revision 1.
- 005: metrics with equal label but differing scientific method have distinct stable codes.
- 007: force-plate channels and sprint front/rear roles are not silently collapsed into one `DeviceId`.
- 008: new revision has unique ID/revision; `supersedesId` points to predecessor; original remains; active pointer unchanged pending approval.
- 009: upload success + input validation failure is a valid UI combination; retry transfer does not falsely imply scientific validation success.
- 010: same athlete ID appears in both CMJ and Sprint session details and profile timeline.

## 9. Complete demo acceptance matrix

| Scenario | Steps | Expected evidence |
|---|---|---|
| A. CMJ success | Open featured CMJ → session → trial curve → result detail | Device source, ordered events, method-labeled metrics, algorithm version, published revision |
| B. Sprint success | Open featured Sprint → inspect front/rear signals → result → athlete | Device roles, sync confidence, distinct sprint metrics, same athlete identity |
| C1. Upload failure | Select failure → inspect Sync Center → retry | Clear recoverable failure, simulated retry success, no duplicates |
| C2. Input rejection | Select invalid input → inspect Sync Center/session | Uploaded transfer, validation rejected, calculation not published, corrective explanation |
| D. Recalculate | Open CMJ result → request version change → compare revisions | New candidate, original unchanged, not auto-promoted, method compatibility enforced |
| E. Platform governance | Open PlatformConsole algorithm version → inspect status → simulated approve | Platform-only action, applied version/model explained; no claim of real backend release |
| F. API mode regression | Run both apps in supported API mode | No scientifically fabricated successful write; legacy navigation/functions unaffected |
| G. Scope/roles | Inspect org manager/individual vs platform menu | No platform-scientific admin in organization UI; diagnostics respect existing visibility gates |
| H. Accessibility/localization | Toggle available languages, keyboard navigate tabs/modal | Labels, units, focus state and error text legible; no critical content only conveyed by color |

## 10. Known limitations and future backend handoff

The demo **does not prove**: scientific numerical parity, real force-plate ingestion, timestamp synchronization accuracy, object integrity, calculation determinism in .NET, tenant isolation, job durability, server-side release approval, or persisted revision audit trails. Stakeholder slides and UI labels must make this boundary explicit.

After stakeholder validation, convert approved frontend projections into backend contracts in dependency order:

1. Canonical identity/session/trial/device mapping and per-tenant authorization.
2. Raw scientific manifest, asset verification and typed input error codes.
3. Protocol/metric registry and pinned algorithm releases.
4. CalculationJob/Run execution and immutable ScientificResultSet publication.
5. Recalculation permissions, promotion policy and audit.
6. Real CMJ parity fixtures; then KWinSprint multi-device adapters/fixtures.

These are future initiatives governed by the two architectural design documents, **not backlog items in this frontend-only plan**.

## 11. Unresolved product decisions (do not block P0 fixtures)

- Who is the scientific approver of algorithm releases in production, distinct from generic platform admins?
- Can organization managers *request* historical recalculation, and must a platform scientific administrator approve *each* publication or just the algorithm release?
- Which specific CMJ metric method and Sprint metric group will become the first validated reference algorithms?
- Is immediate offline numeric feedback required for real device clients, or is pending cloud calculation acceptable? Prototype depicts pending state; it does not resolve field UX.
- Which organization-specific protocol parameters may be configured without compromising scientific comparability?

**Prototype defaults:** platform-only release governance; org manager may request simulated recalculation; individual read-only; Revision 1 stays active until explicit governed promotion; CMJ + Sprint show clearly labeled synthetic data.

## 12. Final engineering recommendation

Treat the prototype as a **two-application scientific platform UX validation**, not a backend-science milestone. Reuse PlatformConsole's existing catalog pages rather than adding competing admin screens; use shared definitions and deterministic demos so the organization-facing Web.React scientifically agrees with platform governance fixtures. Demonstrate the cross-product athlete lifecycle, status separation, method-specific metrics and immutable revisions first. Only after management and scientific stakeholders validate those interactions should engineering commit to the full calculation engine and persisted domain model.


A single, versioned implementation document is the best way to coordinate your coding AI agent, especially since the work spans two frontend applications with different responsibilities.

I've prepared a new 37 KB, 478-line Markdown implementation specification based on our latest discussion and the current GitHub repository structure.
KWinHub Scientific Workflow — Frontend Prototype Implementation Plan V1

Markdown · Ready for GitHub docs/


Download implementation plan

What the document covers
- Application responsibilities: PlatformConsole owns scientific algorithm governance; Web.React handles organization and individual scientific workflows.
- Existing repository reuse: Extends PlatformConsole's existing Algorithm Versions, Metric Catalog and Protocol Catalog pages instead of building duplicate administration features.
- UI/UX specifications: Page-level requirements for both applications, covering CMJ, Sprint Start, scientific processing, provenance and result revisions.
- Frontend DTO contracts: Typed models for algorithms, scientific metrics, trials, devices, processing states and immutable result revisions.
- Repository file map: Existing files to modify, proposed files to create and boundaries that should not change.
- Implementation backlog: 14 individually executable tasks, each with dependencies and acceptance criteria.
- AI agent instructions: A reusable task prompt, validation requirements and explicit restrictions against premature backend work.
Where to put it
I recommend:

docs/product/kwinhub-scientific-workflow-frontend-prototype-plan-v1.md

This keeps it alongside your existing product and workflow specifications, while leaving the two scientific architecture documents in their current directories.
Suggested implementation sequence
1

Contracts, fixtures and shared demo state

SCI-FE-001–003


2

PlatformConsole scientific catalog enhancements

SCI-FE-004–005


3

Web.React scientific workflow experience

SCI-FE-006–010


4

Testing and stakeholder-demo hardening

SCI-FE-011


5

Optional second-phase enhancements

SCI-FE-012–014



How I recommend using it with your coding agent
After adding the document to GitHub, give the agent this initial instruction:
Read docs/product/kwinhub-scientific-workflow-frontend-prototype-plan-v1.md carefully.

Start with SCI-FE-001 only.

Before implementing:
1. Inspect the existing relevant code and dependencies.
2. Identify any conflicts between the plan and the current implementation.
3. Follow the document's application boundaries and frontend-only scope.
After implementing:
1. Summarize changed files and decisions.
2. Run relevant build, type-check and tests.
3. Verify the task's acceptance criteria.
4. Report remaining issues and the next recommended backlog item.
Do not implement subsequent backlog items until I explicitly approve.




One additional recommendation: Keep each backlog item as a separate, reviewable commit or pull request. This will make it much easier to inspect AI-generated changes, detect unintended architectural changes and roll back a problematic implementation.

The document is ready to use. I have not modified your GitHub repository.