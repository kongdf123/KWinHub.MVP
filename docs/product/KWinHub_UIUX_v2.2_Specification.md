# KWinHub UI/UX v2.2 — Product Design & Frontend Implementation Specification

**Status:** Proposed / ready for engineering planning  
**Date:** 2026-10-28  
**Scope:** `src/frontend/KWinHub.Web.React` in `kongdf123/KWinHub.MVP`  
**Design intent:** Enterprise-grade sports-science SaaS; coherent, calm, professional, trustworthy.  
**Reference approach:** VALD Hub for athlete/team workflow organization; Linear for quiet interface consistency; Stripe-style operational tables. These are inspiration, not instructions to copy assets or proprietary layouts.

## 1. Objectives, non-goals, and principles

### Objectives
1. Make KWinHub recognizable as one unified SaaS product, not unrelated dashboards.
2. Reduce time to locate athlete, test, trend, and report.
3. Present scientific metrics with correct unit, test context, and comparison semantics.
4. Establish reusable UI primitives and tokens for all React pages.
5. Support desktop-first workflows and functional tablet/mobile access.
6. Explicitly separate demo/placeholder data from authenticated production data.

### Non-goals for this UI iteration
- Implement all backend SaaS capabilities or rewrite the science engine.
- Build a full training/rehab management system.
- Replace React/Tailwind merely for aesthetic reasons.
- Claim that mock status or results are live device telemetry.

### Design principles
- **Decision-first:** highlight actions and relevant changes before raw counts.
- **Progressive disclosure:** organize complexity into page sections and contextual tabs.
- **Scientific honesty:** no mixing metrics, units, protocols, or algorithm versions without explicit comparability rules.
- **One visual language:** tokens and shared components, not repeated ad hoc styles.
- **Accessible by default:** keyboard semantics, focus, contrast, motion preferences.
- **Responsive, not compressed:** table adaptations and deliberate content priority.

## 2. Current code baseline and identified gaps

Evidence from the React repository review:
- `src/App.tsx` dispatches pages through a `currentView` switch rather than browser routes.
- `src/components/layout/Sidebar.tsx` supports collapsible width (240px/68px), permissions-aware destinations, and a demo organization label.
- `src/lib/navigation.ts` defines seven primary destinations (Overview, People, Testing, Analysis, Reports, Data, Settings) and contextual tabs.
- `src/index.css` and `tailwind.config.js` define a teal/green semantic palette, but page components still use many literal `bg-white`, `ink-*`, and `teal-*` classes.
- `src/pages/HomePage.tsx` mixes coach-centric performance overview with sync/infrastructure information, and uses fallback metrics in trend construction.
- `src/pages/AthleteProfilePage.tsx` has overview/timeline/trends tabs with meaningful building blocks, but lacks a standardized flagship profile composition.
- `src/pages/DashboardsPage.tsx` has add/remove widgets, but configuration is local component state; its trend selection can fall back to an unrelated metric.

These observations are **code-derived**. Exact rendered defects, contrast failures, and performance benchmarks require a browser-based QA pass.

## 3. Target information architecture

### Primary navigation (sidebar)
1. **Overview** `/overview`
2. **Athletes** `/athletes` (tab/subview: Teams `/teams`)
3. **Testing** `/testing/sessions` (Sessions, Protocols, Devices, Sync Center)
4. **Analytics** `/analytics/results` (Results, Performance, Trends, Comparisons, Biomechanics, Dashboards)
5. **Reports** `/reports`
6. **Data & Integrations** `/data/sources` (Sources, Integrations, Datasets)
7. **Settings** `/settings` (Organization, Members, Roles, Preferences)

**Implementation note:** Retain the underlying destinations already defined by `navigation.ts`; this primarily clarifies naming, hierarchy, and route behavior. Detail routes remain contextual: `/athletes/:id`, `/testing/sessions/:id`, `/analytics/results/:id`.

### Global app shell
- Left navigation: **240px** expanded, **68px** collapsed, persistent per user.
- Header: **64px** high; breadcrumb/location on left; search, notifications, help and account on right.
- Organization selector anchored at top of sidebar; show authorized organizations only.
- Main canvas: `#F5F7F8`, white panels, a maximum comfortable width of **1440px** for overview pages; specialized analysis workspace may use full available width.
- Horizontal padding: 24px desktop, 16px tablet/compact; vertical page rhythm: 24px.
- Page header: title, contextual summary, optional filters, at most one visually dominant primary CTA.
- Detail page breadcrumbs and meaningful back navigation; browser back/forward must work.

## 4. Wireframe specifications

### 4.1 Overview `/overview`

```text
┌──────────── SIDEBAR ────────────┬────────────────────── APP HEADER ─────────────────────────────┐
│ KWinHub  ▾ Organization        │ Overview                         Search   Alerts   Account │
│ Overview                      ├──────────────────────────────────────────────────────────────┤
│ Athletes                      │ Performance Overview                           [Date ▼]     │
│ Testing                       │ Team or organization summary                     [New test]  │
│ Analytics                     │                                                              │
│ Reports                       │ [Athletes] [Sessions this period] [Awaiting review] [Sync]   │
│ Data & Integrations           │                                                              │
│ Settings                      │ ┌─────────────────────────────────┬─────────────────────────┐ │
│                               │ │ Performance trend               │ Needs attention         │ │
│                               │ │ [Protocol ▼] [Metric ▼]         │ Review queue / alerts   │ │
│                               │ │ line chart + units + legend     │                         │ │
│                               │ └─────────────────────────────────┴─────────────────────────┘ │
│                               │ Recent sessions [Filter ▾]                 [View all]          │
│                               │ Athlete | Protocol | Date | Value/unit | Status                │
└───────────────────────────────┴──────────────────────────────────────────────────────────────┘
```

**Behavior:**
- Default scope is authorized organization, selected date range, and view permissions.
- KPI values display period and source; no ambiguous 'today' if data is delayed.
- Trend chart requires explicit `metricDefinitionId`, compatible protocol, unit, aggregation, and valid values. Missing metric => excluded observation with tooltip explanation, never fallback to a different value.
- Needs Attention is based only on grounded triggers (e.g., explicitly failed processing or a rule-defined alert); don't infer injury/readiness from an unexplained status.
- Secondary ingestion diagnostics link to Sync Center rather than occupying a large portion of the coach overview.
- A zero-data organization shows onboarding suggestions, not fake charts.

### 4.2 Athlete directory `/athletes`

```text
Athletes                                             [Import] [Add athlete]
[Search name/ID] [Team ▼] [Sport ▼] [Status ▼] [More filters]
Avatar | Name | Team | Sport | Last test | Latest protocol | Actions
...
[Rows per page]                                    [Pagination]
```

**Behavior:** preserve filters in URL; clear active filters; sortable columns; keyboard accessible row actions; server-side pagination when backend is connected. Empty, loading, error, no-permission, and no-search-results states are distinct.

### 4.3 Athlete Profile `/athletes/:id`

```text
Athletes / Athlete 001                                               [Export report]
[Avatar] Athlete 001  • Team A • Sprint • Active
          Last tested 24 Oct 2026 | Athlete ID ...

[Overview] [Testing history] [Trends] [Comparisons] [Reports]

[Key metric 1 + unit + delta] [Key metric 2] [Key metric 3] [Tests this period]

┌─────────────────────────────────────┬──────────────────────────────┐
│ Primary metric trend                │ Athlete facts / baselines   │
│ [Metric ▼] [Period ▼]               │ Profile / selected norms    │
│ values; baseline markers; legend    │ Context of comparison       │
└─────────────────────────────────────┴──────────────────────────────┘

Recent tests                                  [View all]
Protocol | Test date | Metric summary with units | Source | Status
```

**Behavior:**
- Identity header remains consistent across tabs, with stable athlete ID, team, and sport.
- Trends are within-athlete chronological series, never mixed across unrelated measurement definitions.
- Metric deltas show comparator: previous comparable test, personal baseline, or reference cohort. Show “Not comparable” where appropriate.
- Reference norms are shown only when a validated dataset, cohort definition, and provenance exist; otherwise hide or label unavailable.
- A session drilldown includes protocol, data source/device, trial, algorithm version, and link to raw data when permission allows.

### 4.4 Session Detail `/testing/sessions/:id`
- Header: athlete, protocol, test date/time + timezone, source, processing status.
- Summary row: 3–5 protocol-specific metrics with values **and units**.
- Content tabs: Summary / Trials / Force-time (if available) / Video (if available) / Data provenance.
- No claim of synchronized force/video unless synchronization metadata exists.
- Failed processing shows cause, retry action only when authorized, and recoverable error guidance.

## 5. Design tokens and visual system

| Token | Proposed value | Usage |
|---|---|---|
| `--surface-app` | `#F5F7F8` | Main background |
| `--surface-panel` | `#FFFFFF` | Content cards and forms |
| `--surface-sidebar` | `#102523` | Primary navigation |
| `--accent-primary` | `#168373` | Primary action, focus, selected state |
| `--accent-hover` | `#0D675C` | Hover/pressed state |
| `--accent-soft` | `#E6F4F1` | Subtle selection backgrounds |
| `--text-primary` | `#172126` | Headings and body |
| `--text-secondary` | `#64717C` | Secondary labels (contrast validation required) |
| `--border-default` | `#E2E8E9` | Panel and control boundaries |
| `--status-warning` | `#B78021` | Warning status (pair with text/icon) |
| `--status-danger` | `#B9564E` | Error state (pair with text/icon) |

- Typography: Inter for Latin; PingFang SC / Microsoft YaHei fallback for Chinese. Body 14px/20–22px; small labels normally >=12px; page heading 24px/32px; section title 16px/24px; metric figures 28–36px with tabular numerals.
- Radius: 8px controls, 12px cards; use subtle borders rather than prominent shadows.
- Spacing scale: 4 / 8 / 12 / 16 / 24 / 32px.
- Interactive targets: aim for 40px or larger where practicable; keyboard focus must be clearly visible.
- Dark mode: treat as a distinct token mapping; do not assume white panels automatically invert.
- Status meaning always conveyed by label and/or icon, not color alone.

## 6. Shared component contracts

| Component | Required behavior/props | Adoption |
|---|---|---|
| `AppShell` | sidebar state, org context, main region, responsive overlay | All protected routes |
| `PageHeader` | title, breadcrumb, description, actions, responsive wrapping | All pages |
| `WorkspaceTabs` | URL-backed active route, permissions filtering, scroll overflow | Multi-view sections |
| `MetricCard` | label, value, unit, source period, comparator, direction, loading/empty | Overview, Athlete, Session |
| `MetricTrendChart` | metric ID, unit, series, comparable grouping, tooltip, missing-data behavior | All analytics |
| `EntityDataTable` | search, filters, sorting, pagination, columns, empty/error/loading | Directory pages |
| `StatusIndicator` | canonical status, human text, accessible label | Sync, results, sessions |
| `FilterBar` | URL state, reset control, applied-filter chips | Tables, analytics |
| `EmptyState` | specific reason, action if valid, no misleading placeholder | All pages |
| `ErrorState` | plain-language error, retry where valid, diagnostic ID | API and processing |
| `ReportPreview` | branding, units, date/time, metric definitions/provenance | Reports |

**UI state matrix for components:** loading, ready, empty, filtered-empty, error, forbidden, offline/stale (where relevant). Establish Storybook or equivalent component examples for each state.

## 7. Critical data presentation rules

1. Never use `session.summary[selectedMetric] || Object.values(session.summary)[0]` for charts. Numeric zero must be treated as a real value.
2. Model data points as `{ timestamp, metricDefinitionId, value, unit, protocolVersion, algorithmVersion, sessionId }` (plus required scientific context) rather than anonymous numbers.
3. Only compare compatible metric definitions, units, protocol versions, conditions, and algorithm versions according to an explicitly documented comparison policy.
4. Distinguish **improvement**, **increase**, and **decrease**. A higher value is not universally better across sports metrics.
5. Clearly distinguish baseline, previous test, personal best, and population norm.
6. Include sample count, date range, and filter scope for aggregated team values.
7. Never label demo data as live, or device connectivity inferred from seeded fixtures as actual online status.

## 8. Responsive and accessibility requirements

**Breakpoints (implementation targets):** desktop >=1200px, compact/tablet 768–1199px, mobile <768px. Validate at 1440, 1280, 1024, 768, 390px widths.

- Desktop: visible sidebar, two-column analytic panels, data tables.
- Tablet: collapsible/sidebar drawer, stacked content as needed, narrower grid; avoid horizontal page scroll.
- Mobile: navigation drawer, priority metrics in 1–2 columns, charts readable, tables use deliberate responsive cards or contained scroll with visible affordance.
- Use semantic buttons/links, visible focus outlines, keyboard tab/arrow behavior where appropriate, heading hierarchy, meaningful chart summaries, labeled inputs, and screen-reader status announcements.
- Validate WCAG 2.2 AA contrast in final implementation, including status text, small secondary text, focus indications and icon-only buttons.
- Honor `prefers-reduced-motion`.
- Ensure English and Chinese strings do not overflow common controls; localize date/time and units explicitly.

## 9. Route migration plan

1. Add React Router and define `AppShell` as the protected parent route.
2. Map all existing view keys to concrete paths, preserving current access checks.
3. Convert sidebar buttons into links (`NavLink`) and context tabs into nested/related routes.
4. Preserve existing detail-page IDs in path segments; remove hard-coded fallback to first athlete when an ID is missing/invalid (show not-found).
5. Ensure browser back, forward, refresh, deep linking, and filter query strings work.
6. Keep a temporary compatibility adapter for old `navigate(view, params)` calls; remove it page-by-page.
7. Confirm route visibility is **not** authorization: backend must enforce tenant and resource access.

## 10. Engineering work packages

### P0-A — Design foundation
- Extract tokens and component primitives, remove divergent literal colors in first-wave pages.
- Build unified `AppShell`, `PageHeader`, `WorkspaceTabs`, `PanelCard`, `FilterBar`.
- Deliver documented examples and responsive layouts.

### P0-B — Overview redesign
- Implement role/use-case-oriented dashboard hierarchy.
- Separate operational sync health from athlete performance by default.
- Enforce metric-valid trend dataset; add empty and error states.

### P0-C — Athlete Profile redesign
- Stable athlete identity header and tab structure.
- Metric card comparator labels and genuine units.
- Recent tests section with drilldowns; scientific provenance exposure.

### P0-D — Routing and navigation
- React Router route tree, deep links, URL-backed tabs and filters.
- Add 404, forbidden, and access-aware navigation.

### P1-A — Enterprise data views
- Common table component, server-ready pagination, search, filter chips, sort and bulk actions as permitted.

### P1-B — Charts and session detail
- Common trend chart, reference bands if validated, interactive trial selection, protocol-aware metric presentation.

### P1-C — Reporting and finish
- Report preview, consistent PDF/CSV affordance, skeleton/loading/error states, keyboard QA, screenshot regression.

## 11. Acceptance criteria and test scenarios

### Product acceptance
- [ ] Organization and active route are obvious on every protected page.
- [ ] One prominent primary action per page; secondary actions are grouped.
- [ ] Athlete is searchable and opens at a stable, refreshable URL.
- [ ] Athlete's current results and historical trends are discoverable without ambiguous navigation.
- [ ] Comparison labels and unit provenance are explicit; charts never substitute unrelated metrics.
- [ ] Demo state is visibly labeled; initial production experience does not manufacture populated metrics.

### Functional/browser acceptance
- [ ] Navigate via sidebar, breadcrumbs, tabs, browser back/forward and direct URL.
- [ ] View pages at 1440/1024/768/390 without unintended horizontal page overflow.
- [ ] Keyboard-access all major flows and observe focus indicator.
- [ ] Loading, empty, error and forbidden states pass visual review.
- [ ] English and Chinese labels render without clipped controls.
- [ ] Contrast checks meet WCAG 2.2 AA for applicable UI text and interactive controls.
- [ ] No console errors or React key warnings in core workflows.

### Chart integrity acceptance
- [ ] Missing metric gives absent point/empty message, not a different metric.
- [ ] Actual value `0` is shown, not treated as missing.
- [ ] Incompatible units/protocols are not directly connected as a single continuous series.
- [ ] Changes indicate direction/meaning and an explicit comparator.
- [ ] Tooltip shows value, unit, date, and session/context information.

## 12. Suggested implementation order and pull requests

1. **PR #1: Shared tokens and shell** — AppShell, PageHeader, sidebar organization picker presentation, consistent panel patterns.
2. **PR #2: Routing and contextual navigation** — paths, deep links, back/forward, 404.
3. **PR #3: Overview v2.2** — high-signal dashboard composition, metric correctness and empty states.
4. **PR #4: Athlete Profile v2.2** — identity, comparison semantics, timelines and recent sessions.
5. **PR #5: Data table/chart foundations** — reusable enterprise data surfaces, chart interaction and test coverage.
6. **PR #6: Polish and accessibility** — i18n, responsive, Playwright screenshot baselines, docs.

Each PR should include before/after captures at desktop and mobile widths, component-state test cases, and a note describing changes to data assumptions.

## 13. Success measures for pilot validation

Collect baseline before redesign, then re-test with representative coach, scientist, and admin users:
- Time to find a named athlete and their latest test.
- Time to compare two valid tests for the same metric.
- Successful completion rate for locating a failed sync and its reason.
- Number of navigation misclicks / backtracks on core tasks.
- Number of data misinterpretations due to unit, metric, or comparator ambiguity.
- Accessibility and responsive regression count.

**Recommendation:** Begin with the application shell, Overview, and Athlete Profile as one design reference slice. Avoid decorating all 20+ screens before the design system is proven by actual user workflows.
