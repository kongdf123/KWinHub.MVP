# KWinHub /analysis/results — Frontend Implementation Plan V1

**Status:** Proposed; pending internal review  
**Date:** 2026-10-28  
**Repository:** `kongdf123/KWinHub.MVP`  
**Target applications:** `KWinHub.PlatformConsole`, `KWinHub.Web.React`  
**Scope:** Frontend-only interactive workflow and user-story explorer  
**Audience:** Product, UX, engineering, QA, sports-science stakeholders, and coding agents

## 1. Purpose

Add a discoverable **Product Workflow & User Story Explorer** to both frontend prototypes so internal reviewers can understand the intended product, follow realistic user journeys, open existing prototype screens, and distinguish demonstrated behavior from future requirements.

This is a **presentation and requirements-discovery feature**, not a production workflow engine. The explorer must not claim operational capabilities that are only mocked or specified.

## 2. Source documents and precedence

Use the existing repository specifications rather than maintaining an independent competing PRD:

1. `docs/product/kwinhub-workflow-spec-v0.2.md` — primary source for business workflows, roles, states, and exception paths.
2. `docs/product/kwinhub-workflow-specification-and-prototype-requirements.md` — additional workflow / screen requirements.
3. `docs/product/kwinhub-scientific-workflow-frontend-prototype-plan-v1.md` — constraints and demo scenarios for CMJ / Sprint Start.
4. `docs/product/kwinhub-mvp-prd-v1.1.md` — MVP scope and feature intent.
5. `docs/product/KWinHub_UIUX_v2.2_Specification.md` — interaction and visual guidance.
6. Relevant `docs/architecture`, `docs/data`, `docs/api`, and `docs/integrations` — contracts and boundaries when explaining the workflow, not authorization to implement those systems.

**Conflict handling:** Do not silently reconcile contradictory requirements. Document the conflict, link the passages, and mark it **Open Question**. Preserve source-document labels: `AGREED`, `PROPOSED`, `ASSUMPTION`, `OPEN QUESTION`, and `OUT OF SCOPE`.

## 3. Non-goals and implementation boundaries

- Do not change backend projects, database schemas, API endpoints, migrations, workers, object storage, authentication, or tenant authorization.
- Do not implement real hardware control, acquisition, upload, scientific processing, or third-party integration as part of the explorer.
- Do not replace existing catalog, athlete, session, synchronization, or reporting pages.
- Do not turn the explorer into a project-management tool or general-purpose drag-and-drop workflow editor.
- Do not show mocked behavior as a working production capability.
- Preserve current navigation, theme, i18n, and app-specific layout conventions.
- Do not imply a feature is approved simply because its diagram has been built.

## 4. Product principles

1. **One shared product narrative, two role-specific perspectives.**
2. **Workflows before screen inventories:** show action, decision, outcome, and recovery, not merely site maps.
3. **Every stage has context:** actor, goal, preconditions, expected result, and relevant requirements.
4. **Traceable to the specifications:** cite document paths and stable headings / IDs; do not invent page-level references.
5. **Navigate to real prototype screens where they exist.**
6. **Make uncertainty explicit:** proposed vs agreed, simulated vs integrated vs verified.
7. **Support guided demonstrations:** one end-to-end happy path and at least one exception/recovery path.
8. **Accessible and maintainable:** keyboard selection, readable fallback list, deterministic data fixtures, and automated validation.

## 5. Scope of workflows

### 5.1 PlatformConsole — platform governance view

Primary actor: platform administrator. These are proposed workflow labels to map against the canonical specifications, not new approved business requirements.

| ID | Workflow | Story / objective | Existing likely target screens |
|---|---|---|---|
| PC-01 | Organization lifecycle | Configure and manage organizations | `OrganizationsPage`, `OrganizationDetailPage` |
| PC-02 | Platform users and access | Manage platform users and access boundaries | `UsersPage` |
| PC-03 | Device catalog | Manage supported device definitions | `DeviceCatalogPage` |
| PC-04 | Protocol catalog | Govern reusable protocol definitions | `ProtocolCatalogPage` |
| PC-05 | Metric catalog | Govern metric definitions, units, metadata | `MetricCatalogPage` |
| PC-06 | Algorithm versions | Govern algorithm versions and their relationships | `AlgorithmVersionsPage` |
| PC-07 | Audit and governance | Review changes and actions | `AuditLogPage` |

First guided demo: **organization setup → configure relevant catalogs → make definitions available to authorized organizations → review audit/governance**. Steps related to publishing / availability must be shown as **proposed or simulated** unless implementation is independently verified.

### 5.2 Web.React — organization testing view

Primary actor: sports-organization administrator; secondary actors include coach, operator, and scientist.

| ID | Workflow | Story / objective | Existing likely target screens |
|---|---|---|---|
| WEB-01 | Organization readiness | Set up people, teams, and testing resources | `OrganizationPage`, `TeamsPage`, `UsersPage` |
| WEB-02 | Athlete management | Maintain athlete identity and memberships | `AthletesPage`, `AthleteProfilePage` |
| WEB-03 | Testing preparation | Select protocol, participants, and sources | `ProtocolsPage`, `SessionsPage`, `DataSourcesPage` |
| WEB-04 | Measurement lifecycle | Track capture and session progress | `SessionsPage`, `SessionDetailPage` |
| WEB-05 | Synchronization and recovery | Diagnose ingestion, validation, and retry | `SyncCenterPage` |
| WEB-06 | Scientific review | Examine results, provenance, and metrics | `ResultsPage`, `ResultDetailPage`, `BiomechanicsPage` |
| WEB-07 | Trends and comparisons | Compare history with scientific context | `TrendsPage`, `ComparisonsPage` |
| WEB-08 | Reporting | Generate or preview communication of results | `ReportsPage` |

First guided demo: **athlete → session planning → capture via local client (represented, not controlled by browser) → ingestion/validation → scientific review → report**, with a **simulated failed sync/retry or identity-matching exception**. Preserve distinctions among *capture complete*, *uploaded*, *validated*, *analysis ready*, and *session completed*.

## 6. Page experience / information architecture

Add one discoverable navigation destination per app, tentatively **Workflow Explorer** (`Workflow & User Stories` is an acceptable label).

Provide these views or sections:

1. **Workflow Map** (P0): interactive directed flow diagram; select nodes to inspect step details; highlight the active path and branch; provide zoom/pan and a responsive fallback.
2. **User Stories** (P0): actor, goal, preconditions, main flow, alternate flows, outcome, requirement links, and acceptance criteria. Do not fabricate acceptance criteria; label newly proposed ones.
3. **Prototype Walkthrough** (P0): previous/next, selected actor/scenario, step narration, link to existing screen, and clear simulation notes.
4. **Coverage & Gaps** (P1): compact requirement-by-workflow matrix and status metadata. Do not generate implied verification from presence of a page.

Recommended layout: workflow/scenario picker at top, central interactive map, right or bottom step-details panel, and a prominent **Open prototype screen** link when its route is verified. Include a visible legend for requirement status, delivery status, and simulation indicators.

## 7. Status model: keep two independent axes

**Requirement decision status** (source semantics): `AGREED`, `PROPOSED`, `ASSUMPTION`, `OPEN QUESTION`, `OUT OF SCOPE`.

**Implementation / verification status** (based on evidence):

- `not-started` — no corresponding demonstration known;
- `prototype` — UI or deterministic demo exists, behavior may be mocked;
- `integrated` — connected to a functional system, but not yet verified for release;
- `verified` — supported by documented testing / accepted evidence.

Maintain a separate `simulationMode` or `demoBehavior` indicator (e.g. `mock`, `mixed`, `live`) to avoid treating integration status as identical to simulation status. Use `unknown` rather than guessing where evidence is insufficient.

**Rule:** A workflow map node, a frontend route, or a passing UI unit test does *not* establish a production-ready backend capability.

## 8. Data model suggestion

Static typed TypeScript definitions are sufficient for V1; no workflow backend is necessary.

```ts
type DecisionStatus =
  | 'AGREED' | 'PROPOSED' | 'ASSUMPTION'
  | 'OPEN QUESTION' | 'OUT OF SCOPE';

type ImplementationStatus =
  | 'unknown' | 'not-started' | 'prototype'
  | 'integrated' | 'verified';

type DemoBehavior = 'unknown' | 'mock' | 'mixed' | 'live';

type SpecificationRef = {
  documentPath: string;
  section?: string;
  requirementId?: string;
};

type WorkflowStep = {
  id: string;
  title: string;
  description: string;
  actor: string;
  preconditions?: string[];
  expectedOutcome?: string;
  requirementStatus: DecisionStatus;
  implementationStatus: ImplementationStatus;
  demoBehavior: DemoBehavior;
  specificationRefs: SpecificationRef[];
  demoRoute?: string; // verified route/view identifier for the target app
  demoScenarioId?: string;
};

type WorkflowEdge = {
  id: string;
  from: string;
  to: string;
  label?: string; // e.g. Success, Validation failed, Retry
};

type WorkflowDefinition = {
  id: string;
  name: string;
  persona: string;
  summary: string;
  steps: WorkflowStep[];
  edges: WorkflowEdge[];
};
```

All workflow IDs and step IDs must be unique within the relevant scope. `demoRoute` should be absent or disabled unless a real target view can be resolved. Do not store secrets, athlete data, or sensitive live identifiers in fixtures.

## 9. Frontend integration strategy

### PlatformConsole

Existing navigation uses hash routes (`src/lib/router.tsx`). Add `WorkflowExplorerPage.tsx` to its established route registration and navigation sidebar. Follow the current page, layout, theme, and i18n patterns. Reuse existing catalog/detail pages rather than creating duplicates.

### Web.React

Existing navigation uses application-context `currentView` and `src/lib/navigation.ts`. Register a `workflow-explorer` view, add the corresponding branch in `src/App.tsx`, and expose it in the existing navigation model. Use the app's established navigation actions for deep links; do not assume hash URLs are supported here.

### Component design

Suggested per-application feature structure (extract shared components only if duplication becomes material):

```text
src/pages/WorkflowExplorerPage.tsx
src/features/workflows/
  workflow-definitions.ts
  WorkflowDiagram.tsx
  WorkflowDetails.tsx
  WorkflowWalkthrough.tsx
  workflow-validation.ts
```

Prefer `@xyflow/react` (React Flow) for selectable nodes/edges and pan/zoom if it fits the existing dependency policy. Mermaid may be retained for written documentation, but static diagrams alone cannot provide the required interactive navigation. Avoid complex graph editing, user-defined nodes, or runtime workflow execution.

## 10. Prototype-Only Navigation & Visibility Requirements

### Purpose

The Demo Explorer is an internal prototype demonstration and requirements-validation utility. It is **not a production business module, workflow engine, or permanent customer-facing navigation destination**. The explorer is a tool for explaining the product, not a feature of the product. That distinction must be explicit in the sidebar, page header, implementation specification, and environment configuration.

### Navigation requirements

1. Use the navigation label **"Demo Explorer"**, rather than simply "Workflows".
2. Place it in a separate sidebar section labeled **"Prototype Tools"** or **"Internal Demo"**.
3. Visually separate this section from ordinary product navigation (e.g. a border divider and a muted section label with a hint such as "Internal demo only").
4. Display a persistent **"Prototype Demonstration Only"** explanation on the explorer page, directly below the title.
5. Support English and Chinese explanatory text.
6. Preserve existing primary navigation and business workflows.
7. Allow direct navigation from explorer steps to existing prototype screens, without treating those screens as completed features.

### Environment visibility

- Development/demo environments: show the Demo Explorer.
- Staging environments: enable only when explicitly configured for internal demonstrations.
- Production environments: hide the explorer by default.
- If included in a production build, enforce authorized internal-only access rather than relying solely on hiding the navigation item.

Configuration:

```
VITE_ENABLE_DEMO_EXPLORER=true
```

This frontend flag controls navigation visibility, not security authorization. Security must be enforced by the backend.

### Status and communication

The explorer must differentiate:

- **Requirement status:** Agreed, Proposed, Assumption, Open Question, Out of Scope.
- **Implementation status:** Not Started, Prototype, Integrated, Verified.

No diagram, walkthrough, simulated interaction, or demo screen may imply a feature is commercially available or approved for production.

### Acceptance criteria

- Internal stakeholders can immediately recognize the explorer as a prototype-only utility.
- The sidebar does not position it alongside permanent product modules without visual separation.
- The page explains its purpose and limitations without requiring additional documentation.
- It can be disabled independently in both frontend applications via `VITE_ENABLE_DEMO_EXPLORER=false`.
- Disabling the explorer does not affect normal application navigation or functionality.
- Both applications apply consistent terminology and disclosure rules.

## 11. Minimal interaction and accessibility requirements

- Switch between available workflows and personas.
- Select a diagram node to update the detail panel.
- Navigate previous/next through a predefined demo path.
- Follow clearly labeled alternate/error paths.
- Open the relevant existing prototype page if available; otherwise explain that the screen is not implemented.
- Display requirement status separately from implementation/simulation status.
- Support keyboard navigation and visible focus indication; provide a readable ordered-list alternative on narrow screens or when graph interaction is difficult.
- Match each application's current i18n and theming conventions; do not introduce hard-coded English-only UI where locale is already supported.

## 12. Incremental implementation backlog

| Priority | Work item | Completion evidence |
|---|---|---|
| P0 | Extract workflow inventory from source specs | Traceability mapping with unresolved conflicts marked |
| P0 | Implement typed definitions and validation | Unique IDs, valid edges, valid specification references |
| P0 | Implement PlatformConsole explorer | Route renders, scenario selectable, step details visible |
| P0 | Implement Web.React explorer | Navigation/view renders, end-to-end scenario selectable |
| P0 | Add verified links to existing screens | All enabled links resolve in the target app |
| P0 | Add status/uncertainty indicators | No mock behavior labeled integrated or verified without evidence |
| P1 | Guided scenario player and exception branches | Happy path and recovery branch demonstrable deterministically |
| P1 | Coverage and gaps matrix | Reviewers can see missing capabilities and source refs |
| P1 | Visual, responsive, a11y QA | Keyboard and narrow viewport checks documented |
| P2 | Optional sync with tests/CI evidence | Evidence-backed updates only, no inferred completeness |

## 13. Acceptance criteria

1. Each frontend has one clearly discoverable **Demo Explorer** entry, visually separated from committed product navigation under a "Prototype Tools" section.
2. The PlatformConsole explorer demonstrates at least one complete platform-governance narrative.
3. The Web.React explorer demonstrates one complete athlete-to-report narrative and at least one recovery branch.
4. Every displayed step has an actor, description, valid specification reference, requirement status, and implementation status.
5. Nodes are selectable; details update accordingly; guided navigation works without backend calls.
6. Enabled links point to verified existing screens; unavailable screens cannot be mistaken for live features.
7. The UI visibly distinguishes real integration from seeded/mocked behavior.
8. Existing catalog, session, athlete, results, and report screens still render as before.
9. Tests validate identifiers, graph connectivity/edges, and app-specific navigation mappings.
10. A stakeholder can follow each initial demo without a verbal introduction from an engineer.
11. The explorer page displays a persistent, bilingual "Prototype Demonstration Only" explanation and can be hidden independently via `VITE_ENABLE_DEMO_EXPLORER=false` in each application.

## 14. Review and validation checklist

Before implementation, confirm with product/design owners:

- Exact mapping of PC/WEB IDs to canonical source sections (IDs above are implementation identifiers, not necessarily existing PRD IDs).
- Which workflows are `AGREED` versus `PROPOSED`, especially cross-app publication semantics.
- Which route targets are valid in the current branch.
- What evidence permits `integrated` or `verified` labels.
- Whether the initial demo uses English, Chinese, or both supported locales.

Do not block construction of a basic, clearly labeled prototype while non-critical questions are unresolved; expose them as open questions rather than silently deciding them.

## 15. Suggested coding-agent instructions

> Implement this plan in small frontend-only changes. Read the five source documents named in Section 2. First produce an accurate source-to-workflow mapping and inventory of existing routes/views. Then create typed, validated workflow definitions and a minimal interactive explorer for each application. Keep the established navigation patterns, preserve current screens, and use seeded scenarios. Never present a mock feature as verified. Add tests and report any unimplemented or unverified route, mismatch, or specification conflict. Do not alter backend, database, API, or device integration code as part of this task.

## 16. Suggested PR sequence

- **PR 1:** Workflow-source mapping, data types, fixtures, validation tests.
- **PR 2:** PlatformConsole explorer and navigation.
- **PR 3:** Web.React explorer and navigation.
- **PR 4:** Guided walkthroughs, sync-error branch, accessibility and coverage improvements.

**Definition of done:** Both explorers offer a convincing, navigable description of the intended KWinHub workflows with traceability and accurate prototype-status disclosure, while retaining all existing application behavior.