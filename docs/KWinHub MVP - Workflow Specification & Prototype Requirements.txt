# KWinHub MVP: Workflow Specification & Prototype Requirements

Product discovery · UI/UX prototype · Version 0.1 · October 2026

## 1. Product definition

KWinHub should be designed as a cloud-first sports performance data and operations hub that connects an organization's athletes, testing devices, client applications, test results, and performance reporting in one coherent workflow.

The primary user for the MVP is the sports organization administrator. However, the administrator needs the platform to support the full testing operation, including sports scientists, coaches, testing operators, and external client applications that collect data.

The first prototype should demonstrate that the organization can move from setup to a completed, trustworthy testing workflow across three data domains:

[Biomechanics laboratory facility at Solent University](https://images.openai.com/static-rsc-4/CzLL9r2QMXPxD-N9yVzDX1IxFBg0y25JfkZSIjIqqGvCvFWf8I57jzce3CWiO3TNGboHNMyv_fBnUiLxisr4gry9kXJJn-a-b-vaqa64SxtjmwYpYcnvc-Jf1Uvcpaq6adZRfrT0f7u-PZN9C0hi0M5rLYAi7rRPejS1dYDQauU?purpose=inline)

### Force

Force plates, strength tests, force-time metrics

[Exercise & Sport Science Courses | Charles Darwin University](https://images.openai.com/static-rsc-4/pFpu9JMa3XDIpLkxfFxKsU_Pw7zE9TkMSPG59N9fbAgk0VPH3z0R-o9HSh93uY7ZJkUAxYhDpFtYZDqUDyk6DnWTjnEpZv6KWsI8Esc2NmoAo5nDzZh9wCYWCJ1m5Dpk8JhAnzKsN81NdDfXZ7bmxLz0KuWXgcKdHdLabaGtX_w?purpose=inline)

### Timing

Sprint timing, split times, speed tests

[Bewegungsanalyse & Biomechanik für Sportmedizin](https://images.openai.com/static-rsc-4/PIyMuMd0RZ5o3duOHYzWHSTQudTeo1lW0AW-e075SzaX-hmB39ic8JRxOCQ765qFFjhFx8KJzB5DamVkossW8vdaXysCMLA9uuZ2sCw4ae7amf6-dbb1fpOlf9ivZJiq9caNmV4NXZOF6MZ91Bf_n7njx9Q2U3LHw04O9iyIOFo?purpose=inline)

### Video & motion

Video, movement analysis, derived kinematic metrics

These domains should share a common athlete identity, testing-session context, and reporting experience, without pretending that their measurements are interchangeable.

### MVP product outcome

An organization administrator can configure the organization and its users, prepare devices and connected applications, oversee testing, resolve data issues, and ensure that practitioners can access valid results and produce reports.

### Product boundaries

| In scope for the prototype                                     | Out of scope for the initial prototype                    |
| -------------------------------------------------------------- | --------------------------------------------------------- |
| Organization, teams, athletes, and role-based views            | Full production identity and authorization infrastructure |
| Device and client-app inventory, connection and sync status    | Actual vendor SDK integrations and live hardware control  |
| Protocol configuration and testing-session workflows           | Full protocol execution engines for every device vendor   |
| Simulated data ingestion, validation, and exception resolution | Production-grade data pipelines and background processing |
| Athlete results, history, cross-domain comparisons             | Advanced AI coaching recommendations                      |
| Dashboard and report workflows                                 | Comprehensive enterprise analytics and billing            |
| Loading, error, empty, permission, and recovery states         | Exhaustive support for every external platform            |

This distinction is important: the prototype should simulate the intended behavior faithfully without implying that the underlying integrations or backend already exist.

## 2. End-to-end workflow architecture

The workflow should have two connected layers: the testing lifecycle and the operational control layer.

- The testing lifecycle moves from athlete preparation to session execution, data validation, analysis, and reporting.
- The operational control layer manages organization configuration, permissions, device connectivity, client-app integration, data quality, and exceptions throughout that lifecycle.

### A. Organization setup

Organization · users · teams · permissions · settings

### B. Prepare testing

Athletes · devices · client apps · protocols · metrics

### C. Create & conduct session

Session plan · participants · test execution · capture status

### D. Ingest & validate data

Receive · match identity · deduplicate · validate · process

### E. Analyze & report

Results · athlete history · cross-domain analysis · reports

Operational control plane

Access control · device health · integration monitoring · audit history · data-quality exceptions

Applies across all stages; it is not a final step.

### 2.1 Workflow states and transitions

Use explicit business states in the prototype. Avoid a UI in which an item simply switches between “done” and “not done.”

| Entity             | Suggested states                                                                    | Important transition                                 |
| ------------------ | ----------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Organization setup | Not started, In progress, Ready                                                     | Required configuration checks pass                   |
| Device/data source | Not configured, Connected, Degraded, Disconnected, Error                            | Connection or heartbeat status changes               |
| Testing session    | Draft, Scheduled, In progress, Awaiting data, Review required, Completed, Cancelled | Session and required result conditions are met       |
| Data record        | Received, Pending match, Validation failed, Validated, Processed, Quarantined       | Identity and quality checks complete                 |
| Result             | Pending, Available, Flagged, Corrected, Superseded                                  | Validation, review, or correction changes its status |
| Report             | Draft, Generated, Failed, Shared or Exported                                        | Report generation and sharing actions complete       |

These are proposed states to validate, not finalized business rules. For example, a session can be marked operationally complete even if optional tests are missing, but it should not silently be labeled as having a complete dataset.

### 2.2 Define completion separately for each stage

This prevents one of the most common workflow problems: treating “data received” as equivalent to “testing completed.”

- Capture complete: the client application indicates that the intended capture is finished.
- Ingestion complete: the Hub has received the expected payloads or files.
- Validation complete: identity and data-quality checks have finished.
- Analysis ready: the required results are available for interpretation.
- Session complete: the operator or authorized user has finalized the session according to the defined rules.
- Report ready: the report was generated successfully from a known dataset.

The UI should display these statuses independently where they can differ.

## 3. Screen inventory and navigation

I recommend a five-section primary navigation, with contextual workspaces within each section.

| Primary section | Key screens                                                 | Main user question                                  |
| --------------- | ----------------------------------------------------------- | --------------------------------------------------- |
| Home            | Organization overview, activity feed, operational alerts    | What needs attention now?                           |
| People          | Athlete directory, athlete profile, teams, users and roles  | Who is involved and what can they access?           |
| Testing         | Protocols, devices and sources, sessions, data operations   | Can we conduct and complete testing?                |
| Performance     | Result details, trends, comparisons, dashboards, reports    | What do the results tell us?                        |
| Administration  | Organization settings, integrations, permissions, audit log | Is the platform configured and operating correctly? |

A device may appear in Administration for setup, Testing for readiness checks, and Home when it requires attention. Those should be contextual views of the same device—not separate, disconnected records.

### Prototype screen specification

The following is the proposed MVP screen set. It is deliberately focused on screens that prove a workflow, rather than creating a separate page for every possible capability.

## A. Organization setup

01\. Organization Overview

Organization profile, setup checklist, teams and athlete counts, active devices, recent sessions, unresolved exceptions, recent activity.

Primary actions: Continue setup, Add team, Invite user, Connect data source.

02\. Organization Settings

Organization identity, time zone, date/time conventions, measurement units, data-retention policy placeholder, report branding, locale.

Primary actions: Save settings, Preview report branding.

03\. Teams and Users

Team membership, invitations, role assignment, active/disabled users, access summaries.

Primary actions: Create team, Invite user, Assign role, Review access.

## B. People and preparation

04\. Athlete Directory

Search, filters, team assignment, external identifiers, duplicate warnings, import preview.

Primary actions: Add athlete, Import athletes, Resolve duplicate.

05\. Athlete Profile

Profile identity, team membership, recent tests, results timeline, test-domain filters, source provenance, reports.

Primary actions: Start session, Open result, Compare history, Generate report.

06\. Devices and Data Sources

Device category, vendor/model, capture app, connection mode, assigned location, last-seen time, integration health.

Primary actions: Register device, Configure source, Test connection, View sync history.

07\. Protocol Library

Protocol definitions, test domain, required device capabilities, metrics, instructions, version, active status.

Primary actions: Create protocol, Duplicate protocol, Preview test requirements.

## C. Testing and data operations

08\. Session Planner

Session name, date/time, location, team or athletes, protocol selection, assigned devices, responsible staff.

Primary actions: Save draft, Schedule, Start session.

09\. Live Session Workspace

Participant list, planned tests, capture status, received-result count, incomplete tests, device warnings.

Primary actions: Open capture app, Mark test status, Retry sync, Finalize session.

10\. Data Operations / Sync Center

Incoming batches, source, timestamps, identity-match status, validation failures, retry history, quarantine, duplicate detection.

Primary actions: Review record, Match athlete, Retry ingestion, Quarantine, Escalate issue.

11\. Result Detail and Review

Metrics, units, validity status, raw-data preview or media link, protocol, session, source device, processing version, correction history.

Primary actions: Flag result, Correct an allowed field, View original, Compare with previous result.

## D. Performance and administration

12\. Performance Workspace

Individual and group filters, metric selection, historical trends, comparisons, test-domain switching.

Primary actions: Compare athletes, Inspect outlier, Save view, Create report.

13\. Report Builder and Preview

Report scope, athletes or teams, time range, included tests and metrics, charts, interpretation notes, provenance, preview.

Primary actions: Generate, Export, Share with authorized users.

14\. Integration and Audit Center

Connected clients, credential/configuration status, sync events, access events, administrative changes, failure details.

Primary actions: Inspect integration, Rotate or update credentials where supported, Review audit event.

The prototype can reuse page templates for directory tables, detail pages, status panels, and review drawers. Reuse should simplify navigation and interaction—not force fundamentally different tasks into an identical layout.

## 4. Detailed workflow requirements

This section defines the trigger, preconditions, main path, alternate paths, and expected outcomes for each workflow. These can become the source of truth for the interactive prototype and eventual PRD.

### WF-01 — Organization onboarding and readiness

### Goal

Foundation

The administrator configures the minimum organization structure needed to run the first test.

Main path

1. Enter organization details and preferred locale, time zone, and units.
2. Create teams and invite users.
3. Assign roles and team access.
4. Register or import athletes.
5. Configure at least one data source and one protocol.
6. Review readiness checklist and open the first session planner.

Alternative paths

- Skip optional settings and return later.
- Invite a user who has not accepted yet.
- Save a partially completed setup and resume it.
- Display a clear blocker when a required configuration is missing.

  Success condition

The organization reaches “Ready for first test” only when the minimum required configuration is satisfied. The checklist explains each incomplete requirement and links to the relevant screen.

### WF-02 — Athlete identity and roster preparation

Goal: Ensure that incoming data can be attributed to the correct athlete.

1. Create an athlete manually or import a roster.
2. Preview imported records, including identifier and team mapping.
3. Detect possible duplicates using configured identity rules.
4. Resolve duplicates or leave them pending review.
5. Assign athletes to one or more teams, if supported by the product policy.
6. Preserve the athlete's stable internal identifier and external-system mappings.

Important design rules

- A name alone must not be treated as a reliable unique identifier.
- An unresolved identity should block automatic publication of affected results, not prevent unrelated athletes' results from progressing.
- Merging profiles must show which profile will remain primary and what linked records will be consolidated.
- Historical results must remain traceable to the identity under which they were originally received, with an explicit mapping after a merge.

Success condition: The prototype can demonstrate that a roster import, identity conflict, and subsequent resolution do not silently create duplicate athlete histories.

### WF-03 — Device and client-app connection

This workflow is particularly important for the cloud-first deployment model.

The Hub should distinguish the physical device, the capture application, and the integration connection. A physical device may communicate through local software rather than directly to the cloud.

### Proposed connection flow

1\. Register source

Select force, timing, or video/motion category

2\. Configure capture pathway

Vendor device, local capture app, or external client integration

3\. Verify connectivity and authorization

Connection check, credentials, permissions, expected capabilities

4\. Verify data receipt

Simulated test payload, source identity, timestamp, data schema

5\. Mark ready

Ready, degraded, or action required—with a reason

Requirements

- Show last successful heartbeat or data receipt separately; they are not necessarily the same thing.
- Display integration status, source version where available, and the last sync outcome.
- Distinguish a source being reachable from its data being successfully ingested.
- Provide clear setup instructions and recovery actions for each error state.
- Never display “Connected” simply because the source was registered.

Success condition: An administrator can configure and verify a simulated force, timing, and video/motion source, understand its readiness, and navigate to the associated data history.

### WF-04 — Configure a testing session

Goal: Define a test event before capture begins.

1. Choose a session name, date/time, location, and responsible operator.
2. Select a team or individual athletes.
3. Add one or more protocols.
4. Check that the assigned data sources support the required capture capabilities.
5. Review planned tests, participants, and device readiness.
6. Save as draft, schedule, or start the session.

The session planner should support mixed-domain sessions—for example, a sprint timing test followed by a force-plate jump test and a movement video assessment.

It should also make dependencies visible. If a selected protocol requires a device that is unavailable, the user should see the issue before starting rather than discovering it after data is missing.

Success condition: A session can be configured with multiple athletes and test domains, with clear readiness checks and the ability to save an incomplete draft.

### WF-05 — Conduct the session and receive data

The live session workspace should provide a unified view of progress, while capture remains the responsibility of the appropriate connected application or device.

The prototype should show:

- Which athletes are expected to participate.
- Which tests are planned, started, completed, or outstanding.
- Which client applications are expected to send data.
- Whether data has been received and whether it has passed validation.
- What the operator should do when capture or synchronization fails.

The workflow must support asynchronous arrival. A capture application might finish a test while the cloud Hub receives its payload later.

Important behavior: Do not mark a test as successfully completed just because the capture application says “finished.” The Hub should distinguish capture completion from data receipt and analysis readiness.

Success condition: A user can follow the progress of a mixed-domain session and identify exactly which stage is incomplete for each athlete and test.

### WF-06 — Ingest, validate, and resolve exceptions

This should be a first-class workflow, not merely a technical log.

### Data processing sequence

1. Receive

   Record the source, receipt time, external event ID, and ingestion outcome.
2. Match

   Resolve the athlete, session, protocol, and source. Flag ambiguous identity or session mappings.
3. Deduplicate

   Detect retries and repeated uploads without creating duplicate results.
4. Validate

   Check required fields, units, timestamps, schema compatibility, and domain-specific quality rules.
5. Process

   Produce derived metrics where supported, retaining the relationship to the original data and processing version.
6. Publish or quarantine

   Make valid results available for analysis, or isolate records that need review.

For the prototype, each failed record should expose:

- The failure category and a human-readable explanation.
- The affected athlete, session, source, and received timestamp, when known.
- Whether the failure is retryable.
- The permitted resolution action.
- The result after the action, including whether downstream analysis needs to be refreshed.

A retry must not create a second result. Correcting an athlete mapping must not silently overwrite the original payload. If a result is corrected, the UI should show the correction and preserve the original value or record according to the proposed audit policy.

### WF-07 — Analyze results across domains

The Performance workspace should make it possible to move from an organization or team overview to an individual result without losing context.

Recommended interaction path

1. Choose the organization, team, or athlete.
2. Select a test domain and metric.
3. Choose a time range or comparison baseline.
4. Inspect a trend, table, or comparison visualization.
5. Open a data point to see its test, session, source, and quality status.
6. Flag an unusual or questionable result, or include valid data in a report.

For the mixed ecosystem, the product must distinguish three levels of analysis:

- Within-domain analysis: Compare the same or explicitly comparable metrics across tests.
- Cross-domain context: Display force, timing, and motion findings together for the same athlete or session.
- Derived composite analysis: Combine measurements only when a documented definition, units, calculation, and compatibility rules exist.

For example, a force metric and a sprint split time can appear together in an athlete review, but the prototype should not imply that their numerical values are directly comparable. Similarly named metrics from different devices should not automatically be treated as equivalent.

Success condition: The user can inspect an athlete's history across all three domains and trace every displayed value back to its source and test context.

### WF-08 — Generate and share a report

Goal: Produce a report with a clearly defined scope and traceable data.

1. Select athletes, teams, or a single session.
2. Select a reporting period and test domains.
3. Choose a template or report layout.
4. Select supported metrics, charts, and optional interpretation notes.
5. Preview the report and inspect missing or flagged data.
6. Generate the report and export or share it with authorized recipients.

The preview should make clear whether results are missing, excluded, flagged, or still processing. A report must not imply that a team was comprehensively assessed when only a subset of the intended tests was available.

Success condition: A user can generate an individual report and a team report from the same underlying dataset, while respecting data access permissions and showing relevant provenance.

## 5. Cross-domain data model and integration requirements

The UI/UX prototype should reflect a consistent conceptual data model even though the actual backend is not being built yet.

### 5.1 Core entities

| Entity                   | Purpose                                   | Key relationships                                                     |
| ------------------------ | ----------------------------------------- | --------------------------------------------------------------------- |
| Organization             | Tenant and administrative boundary        | Owns teams, users, athletes, sources, sessions                        |
| User and Role            | Person and permission set                 | Accesses permitted organization resources                             |
| Team                     | Organizational grouping                   | Contains athletes and may scope access                                |
| Athlete                  | Stable identity for an individual         | Has external identifiers, memberships, sessions, results              |
| Device                   | Physical measurement equipment            | May be connected through a capture application                        |
| Data Source / Client App | Connection and ingestion identity         | Sends payloads, files, or event records to the Hub                    |
| Protocol                 | Test definition and expected measurements | Defines test requirements, supported capabilities, metrics            |
| Session                  | Context for a testing event               | Links participants, protocols, capture sources, results               |
| Test Attempt             | An individual execution of a planned test | Belongs to a session participant and protocol                         |
| Raw Data Record          | Original received data or media reference | Linked to source, ingestion event, and associated attempt where known |
| Result / Metric          | A measured or derived value               | Linked to athlete, attempt, units, processing version, quality status |
| Report                   | A generated presentation of results       | Has scope, template, creator, recipients, and source-data references  |

A critical design distinction is that one session is not one result. A session may contain multiple athletes, protocols, attempts, and result records. The prototype should represent those relationships explicitly.

### 5.2 Cloud-first integration model

### Measurement sources

Force plates · Timing gates · Cameras and motion systems

### Local capture applications / external clients

Device communication · Test execution · Temporary buffering · Upload

### KWinHub integration boundary

Authentication · Source identification · Payload receipt · Retry/idempotency · Schema mapping

### Canonical data and validation

Identity resolution · Units · Timestamps · Provenance · Quality checks

### Hub experiences

Sessions · Athlete profiles · Analytics · Dashboards · Reports

The diagram is a conceptual boundary, not a prescribed technical stack.

### 5.3 Prototype integration behavior

For the prototype, create simulated client applications or source simulators that can produce these events:

- Connection established or lost.
- Heartbeat received.
- Test capture started or completed.
- Payload uploaded.
- Upload retried.
- Payload accepted, rejected, or quarantined.
- Athlete or session mapping resolved.
- Result processed and published.

Use a consistent event structure across source types, while allowing each domain to have its own payload and validation rules.

For video/motion data, the prototype should distinguish the video or media asset from the derived measurements. It should show upload or processing status, media references, and derived metrics where appropriate; it need not implement video playback or motion-analysis algorithms in the first iteration.

### 5.4 Provenance and data trust

Every displayed result should be traceable through a chain such as:

`Athlete → Session → Test Attempt → Source Record → Processed Result`

Where relevant, also retain:

- Original source identifier and receipt time.
- Device and capture-application identity.
- Protocol version and metric definition.
- Units and relevant calibration metadata, if provided.
- Processing version and validation status.
- Correction and review history.

The prototype should make this chain visible in Result Detail. It is a central part of the product's trust model, not merely an implementation detail.

## 6. Edge-case catalogue

These cases should be deliberately designed into the prototype. They are not optional polish: they determine whether the workflow remains understandable when real-world testing is imperfect.

| ID  | Scenario                                           | Expected UI behavior                                                                                                 |
| --- | -------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| E01 | Device disconnects before a test                   | Mark the source unavailable; explain the impact on planned tests and offer retry or rescheduling paths.              |
| E02 | Capture completes but upload is delayed            | Show capture complete and data pending separately; do not invent a result.                                           |
| E03 | Client retries the same upload                     | Show retry or duplicate detection; prevent a duplicate result from being created.                                    |
| E04 | Athlete identity is ambiguous                      | Quarantine or hold the affected record for review; show candidate matches and evidence.                              |
| E05 | Incoming athlete is unknown                        | Offer a permitted athlete-creation or mapping flow without silently creating a duplicate.                            |
| E06 | Session or protocol cannot be matched              | Hold the record with a reason and a resolution path.                                                                 |
| E07 | Payload has missing fields or invalid units        | Show validation errors and affected fields; prevent invalid metrics from appearing as normal results.                |
| E08 | A video asset uploads but processing fails         | Preserve the upload status, report the processing failure separately, and provide a retry or review action.          |
| E09 | Results arrive out of order                        | Associate results using source identifiers and session context, not arrival order alone.                             |
| E10 | User corrects a result or identity mapping         | Show original and corrected state, actor, time, and impact on derived results.                                       |
| E11 | User lacks permission to view an athlete           | Hide unauthorized details or show an access-denied state; never expose data in a summary or report preview.          |
| E12 | A team is renamed or an athlete changes teams      | Preserve historical context and apply the defined rules for historical and current group views.                      |
| E13 | A report contains incomplete data                  | Warn the user and identify missing or excluded records before generation.                                            |
| E14 | An integration credential expires                  | Mark the integration as requiring action, show the last successful receipt, and provide a safe reauthorization path. |
| E15 | A protocol changes after testing                   | Preserve the version used for the existing test; do not silently reinterpret historical results.                     |
| E16 | The same athlete has results from multiple devices | Display source provenance and metric compatibility; compare only where definitions and units are compatible.         |

### Prototype interaction rules

For each edge case, the design should specify four things:

1. Detection: What status, event, or validation rule identifies the problem?
2. Visibility: Who sees the problem, and where?
3. Resolution: What actions are allowed, and what information does the user need?
4. Recovery: What happens to the affected session, data, results, and reports afterward?

For example, if an athlete is matched incorrectly, the UI should not simply change the displayed name. It should make the mapping correction explicit and show which downstream records are affected.

## 7. Prototype acceptance criteria

These criteria determine whether the prototype is sufficiently complete to validate product requirements. They do not certify production functionality or real integrations.

## A. Core workflow validation

Use this checklist during prototype review.

#### 0 / 8

A1

&#x20;— An administrator can progress from a new organization to a configured, ready-to-test state.

A2

&#x20;— A roster can be imported or created, with duplicate and unresolved identity states represented.

A3

&#x20;— A mixed-domain session can include force, timing, and video/motion tests.

A4

&#x20;— The session workspace distinguishes capture, ingestion, validation, and analysis readiness.

A5

&#x20;— The prototype demonstrates normal ingestion and at least one recovery flow for each domain.

A6

&#x20;— A result can be traced to its athlete, session, test attempt, source, and processing state.

A7

&#x20;— An authorized user can move from results to an individual or team report.

A8

&#x20;— A report clearly indicates its scope and any excluded, missing, or flagged data.

### B. UX consistency criteria

The prototype should also pass these cross-screen checks:

- The same athlete, team, session, source, and result use consistent identities and terminology everywhere.
- Every status label has a defined meaning and is used consistently.
- Every prominent alert links to an appropriate detail or resolution screen.
- Every simulated primary action produces a visible state change or a clear outcome.
- Empty, loading, error, success, and permission-denied states are designed for all critical workflows.
- Tables, filters, and detail views preserve useful context when the user navigates back.
- The layout remains usable with realistic volumes of athletes, tests, results, and exceptions.
- Charts show units, time ranges, and metric definitions; incomparable metrics are not presented as directly equivalent.
- Destructive or consequential actions have clear confirmation and recovery behavior.
- Report previews and dashboard summaries respect the same access rules as the underlying records.

### C. Three end-to-end acceptance scenarios

Scenario 1 · Happy path

Run a complete mixed-domain assessment

A configured organization selects athletes, runs force, sprint-timing, and motion tests, receives data, validates results, reviews performance, and generates a report. Every result is traceable to its source.

Scenario 2 · Recoverable failure

Resolve a failed upload and an identity conflict

One source disconnects and later retries an upload; a different incoming record has an ambiguous athlete match. The user can distinguish both issues, resolve them, and verify that no duplicate or incorrectly assigned result appears.

Scenario 3 · Permission boundary

Review and share team performance safely

A coach can view permitted team results and generate an authorized report, but cannot access an unrelated team's restricted athlete records. The interface explains access limitations without leaking restricted information.

The acceptance bar is not that every screen has been drawn. It is that these scenarios can be completed from beginning to end without dead ends, contradictory statuses, unexplained data, or ambiguous recovery behavior.

## 8. Prioritization: what must be designed first?

I recommend using P0, P1, and P2 to prioritize prototype fidelity and workflow validation, not to define a coding schedule.

| Priority       | Design scope                                                                                                                                      | Why                                                                                      |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| P0 — Essential | Organization setup, roles, athletes, device/source configuration, session planning, ingestion and validation, result provenance, basic reports    | Without these, the end-to-end workflow cannot be demonstrated.                           |
| P1 — Important | Historical trends, team comparisons, richer report layouts, advanced filters, operational dashboards, audit history                               | These validate the value of the Hub beyond simple data collection.                       |
| P2 — Later     | Advanced dashboard customization, normative benchmarks, sophisticated motion analytics, automated interpretation, extensive external integrations | These require more domain decisions and are not necessary to prove the initial workflow. |

One qualification: the three data domains must all appear in the P0 prototype, but they do not need equal analytical depth. A sensible first iteration would demonstrate one representative force test, one sprint-timing test, and one video/motion test, with realistic source data and a common session. The goal is to validate the integration and user experience across domains before designing every vendor-specific workflow.

## 9. How to organize the requirements and prototype work

Maintain a requirements traceability matrix connecting business needs to the actual prototype:

| Requirement                   | Workflow | Screens                                             | Edge cases    | Acceptance test |
| ----------------------------- | -------- | --------------------------------------------------- | ------------- | --------------- |
| Manage organization access    | WF-01    | Organization Settings, Teams and Users              | E11           | A1, Scenario 3  |
| Maintain athlete identity     | WF-02    | Athlete Directory, Athlete Profile, Data Operations | E04, E05, E10 | A2, Scenario 2  |
| Connect heterogeneous sources | WF-03    | Devices and Data Sources, Integration Center        | E01, E14      | A3, A5          |
| Run a testing session         | WF-04–05 | Session Planner, Live Session Workspace             | E01–E03, E09  | A3, A4          |
| Trust incoming results        | WF-06    | Data Operations, Result Detail                      | E03–E10, E15  | A5, A6          |
| Interpret performance         | WF-07    | Performance Workspace, Athlete Profile              | E12, E16      | A6              |
| Generate controlled reports   | WF-08    | Report Builder, Preview                             | E11, E13      | A7, A8          |

For each requirement, add a priority, the responsible user role, the relevant business rule, and the prototype's current validation status. This will help prevent gaps between the PRD, wireframes, and interactive implementation.

## 10. Unresolved product decisions to settle during discovery

The assumptions are now clear enough to design the first workflow, but several decisions should remain explicit rather than being silently baked into the prototype.

Session completion policy

Does a session require every planned test to pass validation, or can authorized users finalize it with outstanding items and documented exceptions?

Identity and correction policy

Who can resolve athlete matches, merge profiles, correct results, or approve quarantined data?

Media storage and access

For video/motion, should the Hub store media, store references to external storage, or support both? What retention and access rules apply?

Integration ownership

Which client applications will publish data to the Hub first, and which system is authoritative for athlete identity, test definitions, and result corrections?

Metric comparability

Which metrics are comparable across devices, which require normalization, and which should only be shown together as contextual evidence?

These questions can be explored with realistic examples during prototype review. They do not need to block the initial wireframes, but their unresolved status should be documented.

## 11. Recommended next deliverable

The most valuable next artifact is a KWinHub MVP Product Requirements & Workflow Design Specification that contains:

1. Product goals, personas, and MVP boundaries.
2. Workflow diagrams and state-transition rules.
3. Conceptual domain model and entity relationships.
4. Screen-level requirements and navigation.
5. Integration and data-provenance behavior.
6. Edge-case catalogue and recovery paths.
7. Acceptance criteria and end-to-end test scenarios.
8. Prioritized requirements traceability matrix.
9. An explicit list of open product decisions.

Then use that specification to guide a connected, clickable prototype using realistic sample data.

My strongest recommendation: design KWinHub as an operational system with a coherent data lifecycle, not just a sports analytics dashboard. The differentiator should be the ability to coordinate heterogeneous devices and client applications, maintain trustworthy athlete and result identity, resolve operational exceptions, and turn validated data into useful performance insights—all within one organization-aware experience.