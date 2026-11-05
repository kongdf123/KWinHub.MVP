# 02 — KWinHub Information Architecture

**Status:** Proposed

## 1. Navigation rules

- Keep customer workspace and internal control plane separate.
- Organize Web.React by user task, not source-app name.
- Preserve stable route/view identifiers where feasible.
- Make active location clear on detail pages.
- Support expanded and compact sidebar modes.
- Separate prototype utilities from committed product navigation.
- Navigation filtering is not authorization.

## 2. Web.React — customer workspace

```text
KWinHub Workspace
├── Overview
├── People
│   ├── Athletes
│   └── Teams & Groups
├── Testing
│   ├── Test Sessions
│   ├── Protocols
│   ├── Devices
│   └── Sync Center
├── Analysis & Reporting
│   ├── Results
│   ├── Performance
│   ├── Comparisons
│   ├── Trends / Analytics
│   └── Reports
├── Data & Integrations
│   ├── Data Sources
│   ├── Datasets / Raw Data
│   └── Integrations
└── Administration
    ├── Organization
    ├── Members & Roles
    ├── Subscription (if enabled)
    └── Settings
```

This is a target IA, not a requirement to add every route immediately. Map existing view IDs to these destinations and avoid duplicate pages.

### Overview

Show organization/team context, recent testing, valid attention items, sync exceptions, recent reports and useful quick actions. Do not show operational health claims without real telemetry.

### Athlete profile

Recommended tabs: **Overview** (profile summary and recent activity), **Timeline** (chronological assessments across capabilities), **Assessments**, **Analytics** (supported trends), **Reports**, and **Profile** (organization-scoped details). Not every athlete has data in every capability; empty states must explain what is missing and how to add it.

### Session / assessment detail

Common header: athlete/participant, session name, timestamp, protocol/version, source app/device, status and quality state. Content: summary metrics, specialized analysis, historical comparison, provenance, assets/media and permitted audit history.

### Capability result presentation

| Capability | Result renderer |
|---|---|
| Sprint | Split times, timing events, reaction output, speed curve, gate sequence, video sync if available |
| Force / Performance | Jump metrics, force-time curve, impulse, phases and supported asymmetry |
| Muscle Strength | Bilateral force, limb comparison and protocol context |
| Hamstrings | Peak force/torque, repetition/event analysis and side comparison |
| ROM | Body region, movement/action, position, left/right values and degrees |
| PowerRack | Exercise/action, load/context, repetitions and supported outputs |
| Golf | Golf-specific metrics and protocol context |

Use a shared result shell with specialized renderers. Metric labels, units and interpretation come from approved definitions and source contracts.

## 3. PlatformConsole — internal control plane

```text
Platform Console
├── Overview
├── Organizations / Tenants
│   ├── Organization Directory
│   └── Organization Detail
├── Subscriptions & Entitlements
│   ├── Plans
│   ├── Subscriptions
│   └── Overrides / Limits (if approved)
├── Platform Users
├── Global Catalogs
│   ├── Test Protocols & Versions
│   ├── Metric Catalog
│   ├── Device Models & Assemblies
│   ├── Algorithm / Processing Versions
│   └── Movement Taxonomy (future)
├── Integrations Registry (future)
├── Security & Governance
│   ├── Audit Logs
│   └── Support Access (if supported)
└── Platform Operations (future)
```

Keep prototype tools such as workflow explorers visually separate and disabled in production as required. Tenant detail should show identity/lifecycle, approved entitlements, enabled capabilities, platform contacts, provisioning state and audit history—not become a backdoor for editing customer athlete records.

Global catalog lifecycle should be explicit: Draft → In Review → Published → Deprecated → Retired. Show immutable version IDs, effective dates, compatibility notes and change history. Publishing a new version must not rewrite historical assessment interpretation.

## 4. Personas and journeys

| Persona | Main tasks | App |
|---|---|---|
| Coach | Review athlete status, compare history, share reports | Web.React |
| Sports scientist | Inspect metrics, trends and provenance | Web.React |
| Test operator | Create sessions, select protocols, monitor ingestion | Web.React |
| Organization admin | Manage members, groups, devices and settings | Web.React |
| Platform admin | Provision tenants, manage global access/catalogs | PlatformConsole |
| Catalog manager | Maintain protocol/metric/device definitions and versions | PlatformConsole |
| Support operator | Investigate platform issues with audited access | PlatformConsole |

### Journey A — Review an athlete

1. Open Athletes and search/filter by team.
2. Open athlete profile and review overview/timeline.
3. Open an assessment and inspect capability-specific results/provenance.
4. Compare with a valid prior assessment or export/share a report if authorized.

### Journey B — Capture and review a session

1. Open Test Sessions and create/select a session.
2. Select athlete/group and available protocol.
3. Confirm device/source context.
4. Receive data from an approved client/source integration.
5. Review ingestion/validation status.
6. Open assessment and inspect results.
7. Resolve failed/partial sync through Sync Center.

Never imply upload success before backend confirmation.

### Journey C — Provision an organization

1. Platform operator creates tenant and sets lifecycle state.
2. Assign approved capabilities/entitlements.
3. Associate applicable catalog versions.
4. Assign/invite initial organization administrator.
5. Verify provisioning and audit record.
6. Organization admin completes setup in Web.React.

### Journey D — Publish a protocol revision

1. Catalog manager creates draft and validates fields, metrics, units and compatibility.
2. Review/approve according to governance.
3. Publish immutable version.
4. Hub receives published catalog through controlled Platform→Hub integration.
5. Web.React offers new version for future sessions; historical sessions retain original version.

## 5. Search, filtering and layout

Web.React global search should prioritize athletes and sessions and display entity type/context. Search respects tenant scope and permissions. Show active filter chips and reset action; preserve filters when navigating into detail where feasible. Use breadcrumbs on deep pages. Detail pages highlight their owning section. Test refresh, deep links and browser back/forward.

Initial target is desktop/tablet landscape: expanded sidebar 240–248 px, compact 64–72 px, fluid content, sensible max widths for reading-heavy pages. Wide tables may scroll horizontally; keep identity/status discoverable. Dense scientific charts may use full-width detail regions. Do not treat mobile as a scaled-down desktop if mobile browser support becomes a requirement.
