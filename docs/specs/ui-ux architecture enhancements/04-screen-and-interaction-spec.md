# 04 — KWinHub Screen Inventory & Interaction Specification

**Status:** Proposed

## 1. Screen inventory

### Web.React

| Screen | User goal / core content | Priority |
|---|---|---|
| Overview | Organization/team context, recent tests, sync exceptions, quick actions | P1 |
| Athlete list | Search, filters, team/group, status, permission-aware create/edit | P0 |
| Athlete profile | Overview, timeline, assessments, analytics, reports, profile | P0 |
| Teams & Groups | Create/edit groups, membership, detail | P0 |
| Test sessions | Session list, filters, create/select, status | P0 |
| Session detail | Participants, protocol/version, trials, data state, results | P0 |
| Result detail | Shared result shell, specialized renderer, provenance | P0 |
| Comparisons | Valid athlete/side/time comparisons and metric compatibility | P1 |
| Analytics | Supported aggregates and filters; no unsupported scientific claims | P1 |
| Reports | Templates, preview, export/share permissions | P1 |
| Devices | Organization device identity, model, assignment; connectivity only if live | P1 |
| Sync Center | Ingestion state, failure reason, retry where supported | P0 |
| Data Sources / Integrations | Connection state, credential policy, mapping and last sync | P1 |
| Organization / Members / Roles | Members, roles, organization profile/settings | P1 |
| Subscription | Entitlement summary; write actions only if approved | P2 |

### PlatformConsole

| Screen | User goal / core content | Priority |
|---|---|---|
| Platform overview | Tenant/catalog/operation summary backed by real data | P1 |
| Organizations | Search, filters, lifecycle state, create tenant | P0 |
| Organization detail | Identity, status, entitlements, admins, audit/provisioning | P0 |
| Platform users | Internal role assignment, status and access review | P0 |
| Protocol catalog | Draft/version/publish/deprecate | P0 |
| Metric catalog | Name, unit, definition, type, capability, version | P0 |
| Device catalog | Manufacturer/model, capabilities, compatibility | P1 |
| Algorithm versions | Immutable version, provenance, status | P1 |
| Subscriptions/plans | Plans, assignments, limits, overrides and audit if approved | P1 |
| Audit logs | Actor, action, target, time, result and filters | P0 |
| Integration registry | Provider metadata, scopes and credential requirements | P2 |
| Workflow explorer | Prototype/demo utility; hide in production | P2 |

## 2. Common page shell

1. Application shell (sidebar, header, account context).
2. Breadcrumb/section context on deep pages.
3. Page title and concise description.
4. Primary action area.
5. Main content.
6. Secondary contextual panel/drawer only where useful.

Avoid duplicating the page title in both global header and content body.

## 3. Common list and detail patterns

Lists require accessible search, relevant filters, sorting where useful, pagination or defined incremental loading, permission-aware actions, active-filter summary/reset, loading skeleton, empty/no-results/error states and stable row identity. Do not make rows clickable where multiple actions compete without a clear model.

Details show entity identity/status, permission-gated primary actions, grouped content/tabs, activity/audit where appropriate, clear back/breadcrumb navigation and explicit missing/partial/stale-data handling. Athlete pages preserve athlete context when opening an assessment and provide a clear route back to the timeline.

## 4. Data and synchronization states

Adapt labels to the actual backend contract:

| State | Meaning | UI behavior |
|---|---|---|
| Draft | Created, not finalized | Show editable state and next action |
| Queued | Accepted for processing/upload | Show timestamp/queue position only if available |
| Uploading | Transfer underway | Show progress only when supplied |
| Processing | Backend/source processing | Indeterminate progress if no percentage |
| Available | Ready for review | Enable result navigation |
| Partial | Some expected data missing | Explain what is available/unavailable |
| Failed | Operation failed | Show reason; retry only if supported |
| Archived | Retained, not active | Read-only/restricted actions |

Never invent percentages, connectivity, completion, retry success or live system health.

## 5. Forms and destructive actions

Use field-level validation and persistent labels. Preserve user input after recoverable server/network errors. Confirm irreversible actions such as tenant suspension or catalog retirement and identify target/consequence. Show success only after server confirmation. Use pending states and prevent duplicate submission for long-running actions.

## 6. Scientific result and provenance

Show where applicable: athlete, session/assessment ID, capture timestamp/timezone, source application, device, protocol/version, metric/value/unit, trial identity, quality/completeness state and calculation/source provenance. Do not compare incompatible metric definitions or units. If protocol versions differ, show that difference and apply explicit compatibility rules. Under the current MVP boundary, Hub presents source-calculated outputs; cloud-side scientific calculation requires a separate decision.

## 7. Theme QA matrix

Test under `classic`, `light` and `hub-light`:
- Web.React: athlete list, athlete profile, session detail, sprint result, force/performance result, Sync Center, report preview.
- PlatformConsole: organizations, organization detail, protocol catalog, metric catalog, audit logs, subscription detail.

Verify contrast, hover/focus/disabled/selected states, overlays, table density, chart legibility, Chinese/English wrapping, no hard-coded color leakage, and no workflow-state reset when switching themes.

## 8. Accessibility and release acceptance

- All interactive controls are keyboard reachable and have visible focus.
- Icon-only controls have accessible names; menus expose expanded/selected state.
- Status is conveyed by text/icon as well as color.
- Forms have persistent labels and associated errors.
- Reduced motion is respected; charts provide textual summaries or accessible data alternatives.
- Empty/error states explain the next useful action.
- No success message appears before server confirmation.

### Product/navigation
- [ ] Web.React and PlatformConsole remain separate products with separate responsibilities.
- [ ] Committed destinations map to real screens or are explicitly marked planned.
- [ ] Prototype utilities are separated and disabled in production as required.
- [ ] Detail pages highlight the correct parent navigation.
- [ ] Refresh, deep links and back/forward behavior are tested.

### Theme/visual system
- [ ] Existing `classic` and `light` behavior is preserved.
- [ ] `hub-light` is available in both apps with matching tokens.
- [ ] Preferences persist independently per app.
- [ ] Theme changes do not reset active workflow state.
- [ ] Charts use theme-aware colors; contrast and keyboard focus are validated.

### Data/permissions
- [ ] Backend authorization remains authoritative and search respects tenant scope.
- [ ] UI does not claim unverified upload/processing/health status.
- [ ] Result provenance and protocol version are visible.
- [ ] Missing/partial data is represented honestly; scientific calculation ownership remains explicit.

### Engineering
- [ ] Both frontend builds, type checks and lint pass.
- [ ] Tests cover theme switching and common states.
- [ ] End-to-end tests cover a cross-capability athlete journey.
- [ ] Visual regression screenshots cover all three themes.
