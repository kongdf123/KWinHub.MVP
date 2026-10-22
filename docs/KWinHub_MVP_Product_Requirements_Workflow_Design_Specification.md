# KWinHub MVP Product Requirements & Workflow Design Specification

**Document status:** Draft for product discovery and UI/UX prototyping\
**Version:** 0.1\
**Date:** 22 October 2026\
**Product:** KWinHub\
**Primary audience:** Product, UX/UI design, engineering,
data/integration, and sports-science stakeholders

> **Purpose of this document:** Establish a testable MVP product and
> workflow specification for KWinHub at the prototype/discovery stage.
> It is intended to guide information architecture, screen design,
> workflow prototyping, and requirements validation. It is **not** a
> production implementation specification or a commitment that all
> integrations and analytics will be delivered in the first release.

------------------------------------------------------------------------

## 1. How to read this specification

This document deliberately separates confirmed direction from
recommendations and unanswered product decisions.

  -----------------------------------------------------------------------
  Label                               Meaning
  ----------------------------------- -----------------------------------
  **AGREED**                          Explicitly established in the
                                      project discussion and to be
                                      treated as a current design
                                      constraint.

  **PROPOSED**                        A recommended requirement or design
                                      choice for the MVP prototype. It
                                      requires stakeholder validation
                                      before it becomes a committed
                                      product requirement.

  **ASSUMPTION**                      A working premise used to make the
                                      design concrete where the project
                                      has not yet established a rule.

  **OPEN QUESTION**                   A decision that should be answered
                                      before detailed implementation or
                                      before the affected workflow is
                                      finalized.

  **OUT OF SCOPE**                    Not required to demonstrate the
                                      current prototype/discovery
                                      objective. This does not
                                      necessarily mean it will never be
                                      supported.
  -----------------------------------------------------------------------

If a requirement is marked **PROPOSED**, it should not be interpreted as
an already-approved business rule. Where multiple device vendors or
measurement domains are involved, the prototype should demonstrate the
workflow and data contract without implying that every vendor-specific
integration is already available.

## 2. Executive summary

### 2.1 Product concept

**AGREED ---** KWinHub is being explored as a **cloud-first hub** for
sports organizations, connected to desktop/mobile client applications
and a mixed ecosystem of force-measurement, timing, and video/motion
systems. The primary user for the first design pass is a **sports
organization administrator**. The MVP prototype should demonstrate a
complete end-to-end testing workflow rather than a collection of
disconnected dashboard pages.

**PROPOSED ---** KWinHub should act as the organization's coordination
and data-management layer across testing operations:

1.  Set up the organization, teams, users, and athletes.
2.  Register and monitor devices and connected client applications.
3.  Define or select a testing protocol.
4.  Plan and conduct a test session.
5.  Receive data from the capture environment.
6.  Validate data, resolve exceptions, and preserve traceability.
7.  Review results across measurement domains.
8.  Generate and share a report with clear scope and data-quality
    context.

KWinHub should not assume that the cloud directly controls every sensor.
Device-specific acquisition may remain in a local desktop/mobile
application, while KWinHub provides organization-level configuration,
session context, synchronization status, canonical records, analysis,
and reporting.

### 2.2 MVP prototype outcome

The prototype should allow a reviewer to complete a realistic workflow
from organization setup to a report, including at least one recoverable
data or connection problem. The reviewer should be able to understand:

-   What is configured and ready.
-   Which device or client application is associated with a session.
-   Whether capture, upload, validation, and analysis are each complete.
-   Which results are usable, pending, flagged, or blocked.
-   Where a result came from and how it relates to an athlete, session,
    protocol, and source record.
-   What action is needed when an exception occurs.
-   What a report includes, excludes, or marks as incomplete.

The prototype may use simulated data and mocked integration behavior. It
must clearly distinguish a simulated state from a verified live
integration.

## 3. Goals, non-goals, and success criteria

### 3.1 Goals

**AGREED** - Design for a sports organization administrator as the
primary user. - Prioritize a complete end-to-end testing workflow. -
Cover a mixed ecosystem of force, timing, and video/motion
measurement. - Use a cloud-first Hub connected to client applications
and external APIs. - Remain at the product-requirements and
UI/UX-prototype stage; do not treat this document as authorization to
start production implementation.

**PROPOSED** - Provide a consistent organization-level view of people,
testing operations, devices/data sources, results, and reports. - Make
ingestion and data-quality operations visible rather than hiding them
behind a generic "sync" status. - Preserve source provenance from
acquisition through derived result and report. - Support a useful first
version without requiring every measurement domain to have equal
analytical depth. - Make errors actionable: explain the problem, show
its scope, and offer a safe next step. - Keep the core workflow usable
even when one source is disconnected or data arrives late.

### 3.2 Non-goals for the current prototype

**OUT OF SCOPE for the prototype/discovery phase** - Production-grade
authentication, authorization, and identity-provider integration. -
Production deployment, monitoring, security certification, and
service-level commitments. - Complete implementation of vendor-specific
SDKs or device drivers. - Real-time cloud control of all hardware. - A
complete generalized ingestion pipeline for every device format. -
Exhaustive biomechanics/video processing, pose estimation, or advanced
motion analytics. - Automated AI coaching or prescriptive training
recommendations. - A comprehensive normative database or
cross-organization benchmarking system. - A full billing/subscription
system. - Every possible third-party integration. - A final decision on
raw video storage, retention, or distribution policy.

### 3.3 Proposed prototype success criteria

The prototype is successful if a representative stakeholder can complete
the scenarios below without needing verbal explanation from the
designer:

1.  Configure a fictional organization and add a team and athletes.
2.  Identify the readiness state of at least one force source, one
    timing source, and one video/motion source.
3.  Create a session with a protocol and assigned athletes.
4.  Understand the difference between capture, upload/ingestion,
    validation, analysis readiness, and session completion.
5.  Recover from a simulated disconnection or delayed upload.
6.  Resolve or deliberately defer an athlete-identity or data-validation
    exception.
7.  Open a result and trace it to its session and source.
8.  Compare available results while seeing units, metric definitions,
    and comparability warnings.
9.  Generate a report and see its included data, excluded data, and
    incomplete-data warnings.
10. See an access-denied or restricted-data state when the prototype
    user lacks the required permission.

These are **PROPOSED acceptance criteria**, not evidence that the
current code repository already implements the behaviors.

## 4. Users, roles, and jobs to be done

### 4.1 Primary persona

**AGREED --- Primary user: Sports organization administrator.**

**PROPOSED persona description:** An administrator responsible for
organizing the organization's people, teams, testing resources, and
operational visibility. They may not be the person who operates every
sensor or interprets every sport-science metric, but they need to know
whether testing is ready, whether data has arrived, and whether the
organization can trust the resulting records.

**Primary jobs to be done** - Keep athlete and team records organized. -
Ensure users have appropriate access. - Understand which devices and
client apps are available, disconnected, or unhealthy. - Prepare testing
sessions with appropriate protocols and participants. - Track the
progress of testing and data synchronization. - Resolve operational
exceptions or route them to the responsible person. - Review whether
results are complete and traceable. - Produce a report that can be
shared with the intended audience.

### 4.2 Secondary roles to consider

**PROPOSED ---** Include these role concepts in the information
architecture even if the prototype uses only a few seeded accounts.

  -----------------------------------------------------------------------
  Role concept            Typical                 Key UX need
                          responsibilities        
  ----------------------- ----------------------- -----------------------
  Organization            Organization setup,     Organization-wide
  administrator           user access, teams,     status and
                          devices, integrations,  administrative controls
                          oversight               

  Coach / performance     Plan sessions, review   Fast athlete/session
  practitioner            athlete results,        workflow and meaningful
                          compare performance,    interpretation
                          generate reports        

  Test operator           Run a session, monitor  Clear live-session
                          capture, resolve        status and recovery
                          immediate operational   instructions
                          issues                  

  Analyst / sport         Validate data, inspect  Metric definitions,
  scientist               details, compare        source traceability,
                          metrics, review         quality flags
                          provenance              

  Read-only stakeholder   View authorized         Clear read-only
                          dashboards and reports  experience and no
                                                  misleading action
                                                  affordances
  -----------------------------------------------------------------------

These role names and their exact permissions are **not finalized**.
Avoid hard-coding them as a definitive permission model until role
ownership and access boundaries are agreed.

### 4.3 Permission principles

**PROPOSED** - Permission should be checked at the organization and,
where required, team or athlete scope. - The UI should distinguish "not
authorized" from "not found" and from "not configured." - Read-only
users should not see active controls that imply they can edit, delete,
correct, or export data. - Reports should respect the access scope of
both the creator and the recipient. - Any data correction, identity
merge, or destructive action should show the affected records and
require confirmation. - Permission failures should be handled as
explicit UI states, not as empty tables or generic errors.

**OPEN QUESTION:** Are team-scoped permissions required for the first
implementation, or is organization-wide access sufficient for the first
release?

## 5. Product principles

1.  **Workflow before dashboard breadth.** Demonstrate one complete
    testing lifecycle before adding many standalone analytical views.
2.  **Separate operational states.** "Device connected," "capture
    complete," "upload complete," "data valid," and "analysis ready" are
    different conditions.
3.  **Traceability by default.** Every displayed result should have a
    discoverable path back to its athlete, session, protocol, and source
    record.
4.  **Mixed ecosystem, common workflow.** Force, timing, and
    video/motion sources may have different payloads and processing
    stages, but the Hub should present consistent session and
    data-operation concepts.
5.  **Be honest about uncertainty.** Missing, delayed, ambiguous,
    incompatible, or invalid data must be visibly represented.
6.  **Recovery is part of the happy path.** A disconnection, retry,
    duplicate upload, or identity conflict should not strand the user.
7.  **Show the next action.** Every actionable error should say what
    happened, what is affected, and what the user can do next.
8.  **Do not imply unsupported integration.** A prototype status must be
    labeled as simulated when it does not reflect a live device or
    service.
9.  **Metric context matters.** Show units and definitions; do not
    compare values solely because they share a display label.
10. **Preserve history.** Corrections should be auditable and should not
    silently overwrite the original source evidence.

## 6. Information architecture and navigation

### 6.1 Proposed primary navigation

**PROPOSED --- Five top-level sections**

  -----------------------------------------------------------------------
  Section                 Purpose                 Main content
  ----------------------- ----------------------- -----------------------
  Home                    Organization-wide       Readiness, sessions
                          operational overview    requiring attention,
                                                  sync exceptions, recent
                                                  results, key activity

  People                  Manage the human        Teams, athletes, users,
                          entities in the         athlete profiles
                          organization            

  Testing                 Configure and run tests Protocols, session
                                                  planner, live session
                                                  workspace, session
                                                  history

  Performance             Review and communicate  Results, trends,
                          outcomes                comparisons,
                                                  dashboards, reports

  Administration          Manage the operational  Devices, data sources,
                          control plane           integrations, sync
                                                  center, roles,
                                                  settings, audit trail
  -----------------------------------------------------------------------

This is a recommended grouping, not a requirement to reproduce any
existing repository's navigation exactly.

### 6.2 Proposed screen inventory

The following screens are the recommended minimum design inventory. Some
can be tabs or contextual subviews rather than separate routes.

  --------------------------------------------------------------------------------
  ID                Screen            Primary user      Minimum prototype behavior
                                      question          
  ----------------- ----------------- ----------------- --------------------------
  S01               Organization      What needs        Show setup/readiness,
                    Overview          attention across  upcoming and active
                                      my organization?  sessions, source health,
                                                        pending exceptions, recent
                                                        results

  S02               Organization      What is           Show organization details,
                    Settings          configured for    units/locale placeholders,
                                      this              configuration status
                                      organization?     

  S03               Teams and Users   Who belongs to    List, filter, view,
                                      the organization  add/edit mock records,
                                      and what can they show role and status
                                      access?           

  S04               Athlete Directory Which athletes    Search/filter athletes,
                                      are available for show team and identity
                                      testing?          status, open profile

  S05               Athlete Profile   What is this      Identity details, team
                                      athlete's testing affiliation, sessions,
                                      history?          result timeline, flagged
                                                        records

  S06               Devices and Data  Are the sources   Show source type, client
                    Sources           ready and         app, connection/sync
                                      connected?        health, last-seen time,
                                                        assigned capabilities

  S07               Protocol Library  What test         List protocol templates,
                                      definition should domain coverage, version,
                                      be used?          status, and requirements

  S08               Session Planner   How do I          Select protocol,
                                      configure the     participants, sources,
                                      assessment?       schedule/context, save
                                                        draft

  S09               Live Session      What is happening Show participant progress,
                    Workspace         during the        per-source state, capture
                                      session?          events, exceptions, and
                                                        recovery affordances

  S10               Data Operations / Which records are Queue, validation results,
                    Sync Center       waiting or        retry state, identity
                                      failing?          mapping, issue detail

  S11               Result Detail and Can I trust this  Metric, units, flags,
                    Review            result and where  source/session links,
                                      did it come from? correction/review status

  S12               Performance       What changed and  Athlete/group filters,
                    Workspace         how do measures   charts/tables, metric
                                      relate?           definitions, comparison
                                                        constraints

  S13               Report Builder    What can I share, Select scope, preview
                    and Preview       and what is       output, show
                                      missing?          incomplete-data warnings,
                                                        generate/export mock
                                                        result

  S14               Integration and   What is           Integration status,
                    Audit Center      connected, and    credential/configuration
                                      what changed?     placeholder, event
                                                        history, audit trail
  --------------------------------------------------------------------------------

**PROPOSED ---** Add global search or command-level navigation if the
prototype can support it without diluting the main workflow. It is not a
blocker for the first pass.

### 6.3 Navigation and context rules

**PROPOSED** - Preserve the current organization context across
sections. - Keep athlete, session, protocol, and result names visible in
detail-page headers. - Provide contextual links between related records
(e.g., session → athlete → result → source record). - Use consistent
terminology and status labels across Home, Testing, Performance, and
Administration. - Ensure browser back/navigation returns the user to the
prior filtered list when feasible. - Use breadcrumbs for deep details
and provide a clear route back to the parent object. - Keep destructive
or irreversible actions out of primary navigation and behind
confirmation.

## 7. End-to-end workflow

### 7.1 Workflow overview

**PROPOSED end-to-end path**

1.  **Set up organization** --- organization details, users/roles,
    teams.
2.  **Prepare people and sources** --- athletes, identity matching,
    device/client-app readiness.
3.  **Select protocol** --- choose or configure the required testing
    protocol.
4.  **Create session** --- assign participants and intended measurement
    sources.
5.  **Conduct testing** --- operator monitors capture and participant
    progress.
6.  **Ingest data** --- connected client applications send
    session-linked records to KWinHub.
7.  **Validate and resolve** --- check identity, schema, units,
    duplicates, source/session matching, and processing status.
8.  **Review results** --- inspect valid and flagged results, source
    provenance, and comparability.
9.  **Generate report** --- choose scope, preview results and warnings,
    generate/export/share.
10. **Retain operational history** --- preserve relevant synchronization
    events, corrections, and audit events.

### 7.2 Workflow boundaries

The Hub should distinguish at least the following completion milestones.

  -----------------------------------------------------------------------
  Milestone               Meaning                 What it does **not**
                                                  prove
  ----------------------- ----------------------- -----------------------
  Capture complete        The local capture       The data has reached
                          workflow indicates that the cloud or passed
                          the intended            validation
                          measurement was         
                          recorded                

  Ingestion complete      KWinHub has received    The records are valid,
                          the expected source     correctly matched, or
                          payload or record set   processed

  Validation complete     Required schema,        Every record passed;
                          identity, and quality   validation may finish
                          checks have finished    with flagged/failed
                                                  records

  Analysis ready          Required processing is  The result is
                          complete and a result   automatically
                          can be reviewed         trustworthy or
                                                  comparable to every
                                                  other result

  Session complete        The session meets the   All optional sources or
                          product's defined       all expected results
                          closure policy          necessarily exist

  Report ready            A report artifact has   Every possible data
                          been generated from a   source was available or
                          defined data            every metric is
                          snapshot/scope          comparable
  -----------------------------------------------------------------------

**OPEN QUESTION:** The precise rule for marking a session "Completed"
must be agreed. See Section 16.

## 8. Functional requirements

Priority labels are proposed for prototype planning: - **P0:** Required
to demonstrate the core end-to-end workflow. - **P1:** Valuable after
the core workflow is validated. - **P2:** Advanced capability or
expansion.

All requirements below are **PROPOSED** unless explicitly marked
**AGREED** in the text.

### 8.1 Organization and access

  -----------------------------------------------------------------------------
  ID                Priority          Requirement             Acceptance notes
  ----------------- ----------------- ----------------------- -----------------
  ORG-01            P0                Show organization name  Context is
                                      and active organization visible across
                                      context.                main screens.

  ORG-02            P0                Show organization       State has a clear
                                      setup/readiness state.  explanation and
                                                              next action.

  ORG-03            P0                Show teams and user     Seeded data is
                                      accounts in             sufficient for
                                      organization scope.     prototype.

  ORG-04            P0                Display each user's     Roles are labeled
                                      role and account        as prototype role
                                      status.                 concepts if not
                                                              finalized.

  ORG-05            P0                Demonstrate a           Restricted state
                                      permission-restricted   is explicit; it
                                      view or action.         is not rendered
                                                              as an unexplained
                                                              empty page.

  ORG-06            P1                Record administrative   Show actor,
                                      changes in an audit     action, target,
                                      view.                   and timestamp
                                                              where available.

  ORG-07            P2                Support detailed        Depends on a
                                      team-scoped permission  decision about
                                      policies.               permission
                                                              boundaries.
  -----------------------------------------------------------------------------

### 8.2 Athletes and identity

  --------------------------------------------------------------------------
  ID                Priority          Requirement          Acceptance notes
  ----------------- ----------------- -------------------- -----------------
  ATH-01            P0                Search and filter    Support at least
                                      the athlete          name, team, and
                                      directory.           status in the
                                                           prototype.

  ATH-02            P0                Show an athlete      Profile links to
                                      profile with team    sessions and
                                      affiliation and      results.
                                      testing history.     

  ATH-03            P0                Represent            Do not assume
                                      external/source      identifiers are
                                      athlete identifiers  globally unique
                                      separately from the  across vendors.
                                      Hub athlete ID.      

  ATH-04            P0                Surface ambiguous or User can inspect
                                      unmatched athlete    the conflict and
                                      identity.            choose a proposed
                                                           resolution path.

  ATH-05            P0                Show the impact of a The affected
                                      mapping/correction   records are
                                      before confirming.   visible;
                                                           destructive merge
                                                           behavior is not
                                                           silently implied.

  ATH-06            P1                Show                 Supports
                                      identity-mapping     traceability and
                                      history and previous future
                                      identifiers.         correction.

  ATH-07            P1                Support controlled   Detailed policy
                                      athlete merge/split  remains open;
                                      workflows.           prototype may
                                                           simulate only.
  --------------------------------------------------------------------------

### 8.3 Devices and data sources

  -------------------------------------------------------------------------------------------
  ID                Priority          Requirement                Acceptance notes
  ----------------- ----------------- -------------------------- ----------------------------
  SRC-01            P0                List force, timing, and    Mixed ecosystem is
                                      video/motion source types. represented, without
                                                                 claiming every integration
                                                                 is live.

  SRC-02            P0                Show source name/type,     Missing timestamps are
                                      linked client app,         explicitly shown as
                                      connection/sync status,    unavailable.
                                      and last-seen or last-sync 
                                      time when applicable.      

  SRC-03            P0                Distinguish device         A connected device may still
                                      connectivity from data     have pending uploads.
                                      synchronization.           

  SRC-04            P0                Show whether a source is   Requirement is
                                      required or optional for a session/protocol-specific.
                                      session.                   

  SRC-05            P0                Show a                     Retry/reconnect behavior may
                                      source-disconnection or    be simulated.
                                      delayed-upload exception   
                                      and a recovery path.       

  SRC-06            P1                Show source                Avoid implying universal
                                      capability/compatibility   protocol compatibility.
                                      metadata.                  

  SRC-07            P1                Show integration health    Details may be mocked in
                                      and recent error history.  prototype.

  SRC-08            P2                Support a full             Not required for first
                                      vendor/device capability   prototype.
                                      registry.                  
  -------------------------------------------------------------------------------------------

### 8.4 Protocols and sessions

  -----------------------------------------------------------------------
  ID                Priority          Requirement       Acceptance notes
  ----------------- ----------------- ----------------- -----------------
  TST-01            P0                List protocol     Example templates
                                      templates with    should include
                                      domain coverage   mixed-domain
                                      and version.      testing.

  TST-02            P0                Create a session  Save as draft
                                      from a protocol   before running.
                                      and assign        
                                      athletes.         

  TST-03            P0                Associate         Make required
                                      expected sources  versus optional
                                      with a session.   sources explicit.

  TST-04            P0                Show session      Do not rely on
                                      state and         one overall
                                      per-participant   status to
                                      progress.         communicate all
                                                        detail.

  TST-05            P0                Show capture      See Section 7.2.
                                      progress          
                                      separately from   
                                      cloud ingestion   
                                      and validation.   

  TST-06            P0                Allow the         The user can
                                      prototype user to navigate to the
                                      review session    relevant
                                      exceptions.       source/data
                                                        record.

  TST-07            P0                Show a session    Include missing,
                                      summary before    failed, and
                                      closure.          pending data.

  TST-08            P1                Support           State transition
                                      rescheduling,     rules need
                                      cancellation, and validation.
                                      session           
                                      duplication.      

  TST-09            P1                Support protocol  Existing sessions
                                      version changes   should retain the
                                      with a visible    protocol version
                                      version           used.
                                      reference.        

  TST-10            P2                Support complex   Requires
                                      multi-station     technical and
                                      orchestration and device-specific
                                      live hardware     discovery.
                                      commands.         
  -----------------------------------------------------------------------

### 8.5 Data ingestion, validation, and operations

  --------------------------------------------------------------------------------------------
  ID                Priority          Requirement                Acceptance notes
  ----------------- ----------------- -------------------------- -----------------------------
  DAT-01            P0                Show received, pending,    State definitions must be
                                      failed, validated, and     consistent across views.
                                      quarantined record states. 

  DAT-02            P0                Show the source and        Provide links to athlete,
                                      session context for each   session, source, and result
                                      data item.                 where known.

  DAT-03            P0                Detect/display simulated   Show the deduplication
                                      duplicate payloads.        outcome; do not silently
                                                                 multiply results.

  DAT-04            P0                Surface missing or         Explain the missing link and
                                      ambiguous                  a proposed next action.
                                      athlete/session/protocol   
                                      mapping.                   

  DAT-05            P0                Surface invalid fields,    Show field-level detail where
                                      incompatible units, or     useful.
                                      missing required metadata. 

  DAT-06            P0                Show retry state and the   A retry control must not
                                      last outcome of an         claim success before the
                                      attempted retry.           outcome is available.

  DAT-07            P0                Distinguish transient      Different failures require
                                      transport failure from     different recovery actions.
                                      validation failure.        

  DAT-08            P0                Preserve a traceable link  A simulated trace is
                                      between raw/source record  acceptable for the prototype.
                                      and processed result.      

  DAT-09            P1                Provide queue filters and  Avoid bulk actions for
                                      bulk triage for repeated   identity merges or
                                      exceptions.                destructive corrections
                                                                 without confirmation.

  DAT-10            P1                Show correction history    May be represented by seeded
                                      and who/what initiated the audit events.
                                      correction.                

  DAT-11            P2                Define and implement       Architecture/implementation
                                      general-purpose            work is out of scope for this
                                      dead-letter/replay         design document.
                                      infrastructure.            
  --------------------------------------------------------------------------------------------

### 8.6 Results and performance analysis

  -------------------------------------------------------------------------
  ID                Priority          Requirement         Acceptance notes
  ----------------- ----------------- ------------------- -----------------
  RES-01            P0                Display a result    Do not show a
                                      with metric name,   unitless number
                                      value, unit,        when the unit is
                                      athlete, session,   known.
                                      and source context. 

  RES-02            P0                Display             Flags must be
                                      quality/review      explained, not
                                      status where        conveyed by color
                                      applicable.         alone.

  RES-03            P0                Link a result to    If information is
                                      its source record   unavailable,
                                      and protocol        label it as
                                      version.            unavailable.

  RES-04            P0                Support a basic     Only compare
                                      comparison view     compatible metric
                                      across athletes or  definitions and
                                      sessions.           units, or show a
                                                          warning/block.

  RES-05            P0                Display missing     Missing must not
                                      data distinctly     be charted as
                                      from a zero value.  zero unless a
                                                          metric-specific
                                                          rule explicitly
                                                          says so.

  RES-06            P1                Show trends over    Avoid implying a
                                      time with           trend when sample
                                      date/session        count is
                                      context.            inadequate.

  RES-07            P1                Support result      Preserve original
                                      correction/review   source evidence
                                      status.             and change
                                                          history.

  RES-08            P2                Provide normative   Requires
                                      benchmarking and    validated data
                                      advanced            definitions and
                                      biomechanics        product
                                      analysis.           decisions.
  -------------------------------------------------------------------------

### 8.7 Reports

  -----------------------------------------------------------------------------
  ID                Priority          Requirement             Acceptance notes
  ----------------- ----------------- ----------------------- -----------------
  RPT-01            P0                Create a report from a  Scope is visible
                                      selected athlete, team, in preview.
                                      or session scope.       

  RPT-02            P0                Preview included        Warnings remain
                                      results and             visible before
                                      missing/flagged data.   generation.

  RPT-03            P0                Show generation state:  A failure state
                                      draft,                  includes a
                                      generating/generated,   recovery path.
                                      or failed.              

  RPT-04            P0                Identify the data       Prevent ambiguity
                                      snapshot or generation  about which
                                      time where feasible.    results were
                                                              included.

  RPT-05            P0                Show incomplete-data    Do not imply the
                                      disclosure in the       report covers
                                      report preview.         sources that
                                                              never delivered
                                                              data.

  RPT-06            P1                Support report          Prototype can use
                                      templates and reusable  one or two fixed
                                      layouts.                templates.

  RPT-07            P1                Record report           Actual external
                                      export/share events in  sharing can
                                      audit history.          remain mocked.

  RPT-08            P2                Support scheduled       Requires security
                                      distribution and        and sharing
                                      audience-specific       decisions.
                                      report policies.        
  -----------------------------------------------------------------------------

### 8.8 Integrations and audit

  ----------------------------------------------------------------------------------
  ID                Priority          Requirement                  Acceptance notes
  ----------------- ----------------- ---------------------------- -----------------
  INT-01            P0                Show the connected           Make the boundary
                                      client-app/source            between local
                                      relationship.                acquisition and
                                                                   cloud Hub
                                                                   visible.

  INT-02            P0                Show simulated               Label simulation
                                      connection/synchronization   where applicable.
                                      state for mixed source       
                                      types.                       

  INT-03            P0                Show recent integration      Include
                                      events and errors.           timestamp,
                                                                   source,
                                                                   event/outcome,
                                                                   and affected
                                                                   record when
                                                                   known.

  INT-04            P1                Show                         Real credential
                                      credential/configuration     handling is out
                                      status without exposing      of scope for
                                      secret values.               prototype.

  INT-05            P1                Show an audit timeline for   Seeded audit data
                                      identity mapping and result  is acceptable.
                                      corrections.                 

  INT-06            P2                Support advanced integration Needs API
                                      configuration and            ownership and
                                      self-service connector       source-of-truth
                                      management.                  decisions.
  ----------------------------------------------------------------------------------

## 9. Detailed workflow specifications

### WF-01 --- Organization onboarding and readiness

**Goal:** Establish the minimum organization context needed to prepare a
testing session.

**Preconditions:** A prototype organization exists, or the user can
enter the onboarding flow.

**Main flow** 1. User opens Organization Overview. 2. The page
summarizes setup areas: organization details, users/roles, teams,
athletes, and data sources. 3. User opens any incomplete area from a
clearly labeled next-action link. 4. User completes or reviews the
required information. 5. The overview updates the relevant readiness
indicator. 6. The page explains any remaining blockers to running a
session.

**Alternative/exception flows** - Some setup areas are incomplete: show
partial readiness and identify blockers. - A user lacks permission: show
an access-denied state with a useful explanation. - A source has been
registered but is disconnected: organization setup may be complete while
source readiness is not. - There are no athletes yet: show an empty
state with a single primary next action.

**Required UI states** - Empty, partially configured, ready, restricted,
and error. - A readiness status must be supported by a list of explicit
conditions, not just an unexplained percentage.

**Acceptance criteria** - User can identify the next setup action in one
glance. - A disconnected source is not mistaken for an unregistered
source. - Organization readiness and source health remain distinct
concepts.

### WF-02 --- Athlete identity and roster preparation

**Goal:** Ensure the participants assigned to a session can be matched
to source records.

**Main flow** 1. User searches the athlete directory. 2. User opens an
athlete profile and verifies identity/team details. 3. User reviews any
known external identifiers or mappings. 4. User assigns the athlete to
the relevant team or session. 5. If source identity is ambiguous, the
system flags the mapping rather than guessing silently.

**Exception flows** - Unknown athlete arrives with a data payload: place
the record in a pending identity queue. - Two athletes appear to match
one source identity: require review. - An external identifier is
missing: show which source supplied the record and what mapping is
unavailable. - A mapping correction affects historical data: preview the
affected records and preserve an audit event.

**Acceptance criteria** - An unknown or ambiguous identity does not
automatically create an unreviewed duplicate athlete. - The user can
inspect the source identifier and affected records before confirming a
correction. - The system distinguishes the canonical Hub athlete ID from
vendor/client identifiers.

### WF-03 --- Device and client-application readiness

**Goal:** Confirm that required measurement sources and their client
applications are available for the planned session.

**Main flow** 1. User opens Devices and Data Sources. 2. User filters by
source type: force, timing, or video/motion. 3. User inspects the source
and its linked client application. 4. User checks connection state,
last-seen/last-sync information, and current exceptions. 5. User returns
to the session planner and associates required sources.

**Exception flows** - Source is disconnected: show last known state and
a reconnect/check-client-app action if applicable. - Device appears
connected but uploads are pending: show connection as healthy while
synchronization is pending. - Client app is not configured for the Hub:
show configuration missing rather than device offline. - A source is
unsupported for the selected protocol: show incompatibility and do not
imply the source can fulfill that requirement.

**Acceptance criteria** - User can distinguish device, local client app,
and Hub data source. - The UI does not equate a network connection with
successful ingestion. - Source-specific capabilities are presented only
when known.

### WF-04 --- Session configuration

**Goal:** Create a session that identifies participants, protocol, and
expected measurement sources.

**Main flow** 1. User selects a protocol template. 2. User reviews
protocol version, domain coverage, and expected outputs. 3. User selects
athletes or a team subset. 4. User associates available sources and
marks required versus optional sources. 5. User reviews the
configuration summary. 6. User saves the session as Draft or moves it to
the next permitted state.

**Exception flows** - No source is available for a required domain:
block readiness or clearly warn, according to the finalized policy. - A
selected source does not support the protocol: show incompatibility. - A
participant has unresolved identity mapping: show the impact before the
session starts. - The protocol version has changed: make the selected
version explicit.

**Acceptance criteria** - The session summary lists participants,
protocol/version, and expected sources. - Required versus optional
source behavior is understandable. - The user can return to edit a draft
without losing configuration.

### WF-05 --- Conduct a session and receive data

**Goal:** Make the operational state of a testing session understandable
to the operator.

**Main flow** 1. Operator opens the Live Session Workspace. 2. The
workspace shows session status, participant progress, and source state.
3. Local client applications perform device-specific capture. 4. Each
source reports capture progress or a capture outcome to the Hub when
available. 5. Captured data is sent to KWinHub by the connected client
app or integration. 6. The Hub shows ingestion progress separately from
capture. 7. When data arrives, the Hub shows validation/processing
status. 8. The operator reviews exceptions and determines whether to
continue, retry, or leave the session awaiting data.

**Exception flows** - A source disconnects mid-session: show which
participants/measurements may be affected. - Capture succeeds but upload
is delayed: show captured/pending-upload rather than failed capture. - A
source upload fails: show retry status and preserve the original
event. - One domain is unavailable while others succeed: show partial
results and the missing domain; do not fabricate a complete record. -
Duplicate payload arrives after retry: show duplicate detection outcome
and avoid duplicate results.

**Acceptance criteria** - The workspace can represent mixed-domain
progress independently. - The operator can tell whether an issue is
capture-side, transport-side, validation-side, or processing-side. - A
session can be left in a review/pending state if the completion policy
allows it.

### WF-06 --- Ingestion, validation, and exception resolution

**Goal:** Turn received source data into trustworthy, traceable records
or make the reason it cannot be used explicit.

**Main flow** 1. User opens Data Operations / Sync Center. 2. User
filters by session, source, state, exception type, or time. 3. User
opens an exception. 4. The detail view shows source payload/record
metadata, matching context, and validation messages. 5. User selects an
allowed action: retry transport, map identity, map session/protocol,
correct a supported metadata field, quarantine, or defer for review. 6.
The system shows the action outcome and any affected records. 7. The
record's state and audit history update accordingly.

**Exception types to represent in the prototype** -
Transport/connectivity failure. - Delayed or pending upload. - Duplicate
source record. - Unknown or ambiguous athlete. - Unmatched session or
protocol. - Invalid required field. - Incompatible or missing unit. -
Processing failure, including a simulated video/motion processing
failure. - Out-of-order or late-arriving data. - Potentially
incompatible metrics across protocol or processing versions.

**Acceptance criteria** - Every exception has a reason, scope/impact,
and next action. - Retry is not offered as the default remedy for a
schema or identity problem when it cannot resolve that problem. -
Correcting a mapping does not erase the source identifier or original
event. - A record can be quarantined or deferred if no safe correction
is available. - The UI shows whether a fix affects one record or
multiple records.

### WF-07 --- Result review and cross-domain analysis

**Goal:** Allow users to inspect results and compare them without hiding
data-quality or comparability limitations.

**Main flow** 1. User opens a session, athlete profile, or Performance
Workspace. 2. User filters to the desired athlete/group, date range,
session, and metric. 3. User opens a result to inspect value, unit,
metric definition, source, protocol version, processing/review status,
and provenance. 4. User compares eligible results. 5. The system
presents values, trends, and flags with clear context. 6. If data is
missing or not comparable, the UI displays that limitation.

**Exception flows** - Unit differs: convert only if a defined conversion
rule exists; otherwise warn/block comparison. - Metric names match but
definitions or protocol versions differ: warn or prevent direct
comparison until equivalence is established. - One source is missing:
show a missing-data state, not zero. - A result is flagged: keep the
flag visible in charts and tables. - A result has been corrected: show
current status and provide access to history where authorized.

**Acceptance criteria** - Metric name, unit, and definition are
discoverable. - Missing, invalid, and zero values are visually
distinct. - Comparison is based on compatible metric semantics, not
label similarity alone. - Video/motion data may have a different
processing state than force or timing data; the UI should not imply
identical processing maturity.

### WF-08 --- Report creation and sharing

**Goal:** Produce a report that accurately represents the selected scope
and the data actually available.

**Main flow** 1. User selects a report scope: session, athlete, or team.
2. User selects a report template or default layout. 3. Report Builder
displays eligible results and data-quality warnings. 4. User reviews
which domains and participants are included or missing. 5. User
generates the report. 6. The UI shows generated or failed state and a
preview. 7. User exports or shares through the prototype's available
mock action. 8. The report event and generation context are represented
in history where appropriate.

**Exception flows** - A source is still pending: warn that the report
may be incomplete or offer a wait/review option. - A result is flagged:
include a flag or disclosure rather than silently omitting it. - The
user lacks permission to export/share: disable or block the action with
an explanation. - Generation fails: retain the selected scope and allow
a retry. - Results change after generation: make the report's generation
time/snapshot visible where feasible.

**Acceptance criteria** - The report preview clearly states the scope. -
Missing and flagged data are disclosed. - A generated report does not
imply that unreceived sources were assessed. - Export/share behavior is
clearly marked as simulated if no real file/share service exists.

## 10. Domain model and relationships

The following is a **PROPOSED conceptual model**, intended to guide the
prototype and API discussions. It is not a finalized database schema.

  -----------------------------------------------------------------------
  Entity                  Purpose                 Key relationships /
                                                  considerations
  ----------------------- ----------------------- -----------------------
  Organization            Tenant or               Owns teams, users,
                          organizational boundary athletes, sources,
                                                  protocols, sessions,
                                                  and reports subject to
                                                  permission rules

  User                    Human account operating Has one or more
                          the Hub                 role/permission
                                                  assignments

  Role / Permission       Describes permitted     Exact roles and scope
  Assignment              actions and scope       rules remain open

  Team                    Groups athletes and     Athlete team membership
                          supports team-level     may change over time;
                          views                   history policy is open

  Athlete                 Canonical person record May have multiple
                          in KWinHub              source/vendor
                                                  identifiers; identity
                                                  resolution must be
                                                  explicit

  Device                  Physical measurement    May be associated with
                          device or sensor        a local client
                                                  application and a
                                                  source capability

  Client Application      Desktop/mobile          May manage
                          application that        device-specific SDKs
                          acquires or forwards    and send data/context
                          data                    to the Hub

  Data Source /           Logical source identity Includes
  Integration             for data received by    vendor/system/source
                          the Hub                 IDs and connection/sync
                                                  status

  Protocol                Defines a testing       Must have version
                          procedure and expected  identity; source
                          measurements            compatibility may vary

  Session                 A planned or executed   Has participants,
                          testing event           protocol version,
                                                  expected sources,
                                                  lifecycle status

  Test Attempt            One participant's       Connects participant,
                          attempt or measurement  protocol step, and
                          unit within a session   source records

  Source Record / Raw     Received evidence from  Preserve source ID,
  Data Record             a client/source         timestamps, payload
                                                  metadata, and ingestion
                                                  history where permitted

  Result / Metric         A processed or          Includes value, unit,
                          interpreted measurement metric
                                                  definition/version,
                                                  quality status, and
                                                  provenance

  Report                  A generated             Has scope, generation
                          presentation of         time/snapshot,
                          selected data           template, and
                                                  export/share state

  Audit Event             Records a meaningful    Includes actor/system,
                          administrative or data  action, target, time,
                          operation               and outcome where
                                                  available
  -----------------------------------------------------------------------

### 10.1 Proposed provenance chain

The user should be able to navigate a result through a chain such as:

**Organization → Athlete → Session → Test Attempt → Source Record →
Processed Result → Report**

This is a navigation/provenance concept; not every record will
necessarily have every link. If a link is unknown, show the missing
relationship and its state rather than inventing one.

### 10.2 Provenance fields to consider

**PROPOSED ---** Capture or display these fields where the source
supplies them and the product's privacy policy permits their use:

-   Canonical KWinHub athlete, session, and attempt identifiers.
-   External athlete/source identifier.
-   Source/device/client-app identity and version.
-   Source record ID and event/capture timestamp.
-   Hub receipt/ingestion timestamp.
-   Protocol ID and version.
-   Metric ID/name, definition/version, and unit.
-   Processing pipeline/version where applicable.
-   Validation/review status and flags.
-   Correction/mapping history and audit references.
-   Calibration or device metadata when supplied and meaningful.
-   Raw media reference or storage metadata only after media-storage
    policy is agreed.

Do not invent provenance fields when the source does not supply them.
Show "not provided" or "not available" where that distinction matters.

## 11. State model and terminology

These are **PROPOSED state vocabularies** for the prototype. Exact
transition rules and backend representations require validation.

### 11.1 Organization readiness

-   Not started
-   In progress
-   Ready
-   Action required

Use a short explanation of the conditions behind each state. Do not
treat a readiness percentage as a substitute for actionable details.

### 11.2 Device/source operational state

-   Not configured
-   Connected
-   Degraded
-   Disconnected
-   Error
-   Unknown

**Important distinction:** Connection state describes reachability or
reported connectivity. It does not by itself describe upload
completeness or data validity.

### 11.3 Session lifecycle

Suggested display states: - Draft - Scheduled - Ready - In progress -
Awaiting data - Review required - Completed - Cancelled

**OPEN QUESTION:** Whether "Ready" is a persistent session state or a
derived readiness indicator should be decided during domain design.
"Awaiting data" and "Review required" may be better represented as
substatus or flags in some implementations.

### 11.4 Data-record lifecycle

Suggested display states: - Received - Pending match - Validation
failed - Validated - Processing - Processed - Quarantined - Superseded

A record can be received and still be unmatched; a record can pass
schema validation but later fail processing. Avoid compressing all these
states into "Synced."

### 11.5 Result review state

Suggested display states: - Pending - Available - Flagged - Corrected -
Superseded

A result's availability and its review/quality state may be separate
dimensions. If both dimensions are important, avoid a single overloaded
status field.

### 11.6 Report state

Suggested display states: - Draft - Generating - Generated - Failed -
Exported / Shared

"Generated" means the artifact was created; it does not mean every data
source was complete or every result was cleared of flags.

### 11.7 Status design requirements

**PROPOSED** - Pair status labels with icons/text and explanatory copy;
do not rely on color alone. - Use the same wording for the same state
across all screens. - Show a timestamp for "last seen," "last sync,"
"received," or "generated" when the timestamp is available. -
Distinguish a missing timestamp from a recent timestamp. - Avoid
ambiguous labels such as "Done" where a more precise milestone is
available. - If a state is simulated, label it in the prototype or
demonstration data.

## 12. Mixed-domain data and comparability

### 12.1 Common workflow, domain-specific payloads

**AGREED ---** The intended ecosystem includes force, timing, and
video/motion. The prototype should show all three in the workflow.

**PROPOSED ---** Share a common operational envelope across domains: -
Organization context. - Athlete and external identity context. - Session
and protocol context. - Source and client-application identity. -
Capture/receipt timestamps. - Validation and processing state. - Result
provenance and quality flags.

Keep domain-specific measurement payloads and processing requirements
separate where necessary. A force result, a timing result, and a
video-derived measure should not be forced into the same raw-data schema
simply because they appear in one session.

### 12.2 Metric comparability rules

**PROPOSED** - Each metric should have a stable identifier or a
documented mapping, not just a display name. - A metric should have a
defined unit and enough metadata to interpret it. - A comparison should
verify compatible metric semantics, units, and relevant
protocol/processing versions. - If a conversion is applied, the
conversion rule should be explicit and traceable. - If equivalence is
unknown, show "not directly comparable" or prevent comparison rather
than silently combining values. - A missing measurement must not be
represented as numeric zero. - Quality flags should remain visible in
tables, charts, and reports. - A report should disclose domain-level
incompleteness where a source did not deliver expected data.

**OPEN QUESTION:** Which metrics and cross-domain comparisons are
essential to the first prototype? This document proposes that the
prototype demonstrate the comparability guardrail, not that it define a
complete sports-science metric catalog.

## 13. Cloud-first integration concept

### 13.1 Conceptual boundary

**PROPOSED conceptual flow**

1.  **Measurement source:** Force plate, timing system, camera, or
    motion system.
2.  **Local capture/client app:** Uses the appropriate device SDK or
    vendor integration; captures or buffers measurements; associates the
    measurement with the correct session/participant context where
    possible.
3.  **Integration boundary:** Authenticated API or connector submits
    source metadata, records, and processing/status events to KWinHub.
4.  **Hub ingestion and validation:** Receives, identifies, validates,
    deduplicates, maps, and tracks records.
5.  **Canonical records and results:** Stores
    organization/athlete/session relationships, normalized metadata, and
    derived results.
6.  **Hub experiences:** Provides operations, review, analysis, and
    reporting.

This is a logical architecture for product discussion, not a finalized
deployment diagram.

### 13.2 Proposed client-app integration principles

-   The client app should not require a user to manually re-enter raw
    measurement data if a supported integration can submit it.
-   The client app should have a defined way to associate data with a
    KWinHub organization, athlete, session, and protocol/attempt.
-   Where capture happens offline or connectivity is intermittent, the
    client app may need to queue data and retry; the UI should show
    pending upload rather than treating it as lost.
-   Submissions should carry stable source record identifiers so retry
    and duplicate handling are possible.
-   The Hub should acknowledge receipt separately from validating or
    processing the payload.
-   The API contract should define idempotency, error responses,
    versioning, and retry expectations before production implementation.
-   Do not assume that periodic heartbeat/polling is sufficient to
    deliver measurement data; connection health, configuration
    discovery, and data transfer are separate concerns.
-   A client app may report status/heartbeat separately from sending
    measurement records.
-   Secrets or long-lived credentials should not be exposed in
    client-side UI or logs.

The exact API design is not specified here. These are discovery
requirements to validate against the available KWinHub API and client
applications.

### 13.3 Integration questions to resolve

-   Is the client app the source of truth for device capture, while
    KWinHub is the source of truth for organization/session identity and
    consolidated results?
-   Does KWinHub create sessions that clients fetch, or do some client
    apps create local sessions and later synchronize them?
-   What is the identity and authorization model for client apps:
    organization-scoped credentials, per-device credentials,
    user-delegated authorization, or another approach?
-   What event or polling pattern is supported for session/configuration
    updates?
-   What payload/version compatibility policy should clients follow?
-   Which raw data types and media are sent to the cloud versus retained
    locally with a reference?
-   What is the expected behavior for offline capture, retries,
    duplicate submissions, and partial uploads?
-   What is the retention policy for raw measurements and media?
-   Which system owns correction of source data versus correction of
    Hub-level mapping/metadata?

These are unresolved architecture/product decisions, not conclusions.

## 14. Data quality, errors, and recovery UX

### 14.1 Exception design template

Every actionable exception should expose the following:

1.  **What happened?** A plain-language title.
2.  **What is affected?** Session, athlete, source, record count, or
    result.
3.  **What is known?** Last event/time, source, validation message, or
    retry outcome.
4.  **What is the impact?** Blocked, delayed, flagged, incomplete, or
    informational.
5.  **What can the user do?** A safe next action or an explicit reason
    no action is available.
6.  **What happens next?** Expected state after the action, if known.
7.  **What was changed?** An audit entry for a consequential
    correction/action.

### 14.2 Proposed exception catalog

  ------------------------------------------------------------------------------------------------------
  ID                Scenario                    UI behavior                   Candidate recovery
  ----------------- --------------------------- ----------------------------- --------------------------
  E01               Device/client app           Mark affected source and      Reconnect/check local app;
                    disconnects during capture  participants; preserve last   resume only if supported
                                                known event                   

  E02               Capture completed but       Show captured/pending upload, Wait, inspect client
                    upload is delayed           not capture failure           queue, retry when
                                                                              available

  E03               Upload fails transiently    Show transport error and last Retry with visible outcome
                                                attempt                       

  E04               Retry sends a duplicate     Show duplicate detection and  Inspect record; do not
                    payload                     retained canonical record     create duplicate result

  E05               Athlete identity is unknown Place record in pending-match Map to existing athlete or
                                                state                         create a new record under
                                                                              approved policy

  E06               Athlete identity is         Show candidate matches and    Review and confirm mapping
                    ambiguous                   source identifiers            

  E07               Session/protocol cannot be  Show missing association      Select a valid association
                    matched                                                   or defer/quarantine

  E08               Required field is           Show field-level validation   Correct supported metadata
                    invalid/missing             message                       or request resubmission

  E09               Unit is missing or          Block/warn on metric          Map only with an approved
                    incompatible                processing                    conversion/definition

  E10               Video/motion processing     Keep source receipt separate  Retry processing or defer;
                    fails                       from processing state         preserve source reference

  E11               Data arrives out of order   Show event time and receipt   Reconcile ordering under
                                                time separately               defined rules

  E12               Mapping/correction affects  Preview affected records      Confirm with audit trail
                    historical data                                           

  E13               User crosses permission     Explicit restricted state     Request access or switch
                    boundary                                                  to authorized scope

  E14               Athlete changes teams       Preserve current affiliation  Apply agreed
                                                and handle historical context membership-history policy
                                                consistently                  

  E15               Report includes incomplete  Show scope and                Wait for data, generate
                    data                        missing/flagged items         with disclosure, or revise
                                                                              scope

  E16               Metrics from different      Show incompatibility warning  Select comparable records
                    sources/protocol versions   or block comparison           or review mapping policy
                    may not be comparable                                     

  E17               Client                      Show integration              Re-authorize/reconfigure
                    credentials/configuration   authorization/configuration   via approved flow
                    expire                      state without exposing        
                                                secrets                       

  E18               Protocol version changes    Preserve the session's        Review and explicitly
                    after a session is planned  selected version and flag any update if permitted
                                                mismatch                      
  ------------------------------------------------------------------------------------------------------

This catalog is a proposed prototype test set. It does not imply that
all recovery operations are already supported by a live backend.

### 14.3 Destructive and consequential actions

**PROPOSED** - Require confirmation for identity merges, bulk mapping
changes, quarantine release, record supersession, and destructive
deletion. - Show a preview of affected records before confirming. -
Preserve the original source reference and an audit event for
corrections. - Prefer reversible states (e.g., quarantine or superseded)
over irreversible deletion when appropriate. - Explain when an action
cannot be undone or when it may affect historical reports.

**OPEN QUESTION:** Which user roles may correct metadata, resolve
identity, retry ingestion, or release quarantined records?

## 15. Reporting and analytics UX requirements

### 15.1 Report scope and disclosure

**PROPOSED** - The report builder should display the selected
organization/team/athlete/session scope before generation. - The preview
should list the measurement domains and participants represented. -
Missing expected sources and flagged results should be visible. - The
report should include metric names, units, and sufficient definitions
for interpretation. - The generation time and data snapshot/version
should be displayed where technically feasible. - If report output is
simulated, label the action/output as a prototype demonstration. -
Sharing/export should not bypass access control.

### 15.2 Performance views

For the prototype, prioritize clarity and data integrity over chart
quantity.

Minimum performance view: - Filter by athlete or team. - Filter by
session/date range. - Select a metric. - Show a table and/or simple
trend visualization. - Show units and metric definition. - Distinguish
valid, flagged, pending, and missing data. - Link each displayed value
to its result detail. - Show an explanation when values cannot be
compared.

**PROPOSED P1 additions:** richer dashboards, reusable comparisons,
configurable widgets, normative benchmarks, and advanced video/motion
visualizations.

## 16. Open product decisions

The following questions should be tracked explicitly. Until resolved,
prototypes should label their behavior as proposed or use a clearly
documented assumption.

  --------------------------------------------------------------------------------------------
  ID                Decision                Why it matters          Recommended next step
  ----------------- ----------------------- ----------------------- --------------------------
  Q01               What exact conditions   Determines operational  Define required/optional
                    make a session          truth and reporting     sources, allowed late
                    "Completed"?            completeness            data, and whether review
                                                                    is required

  Q02               How should unknown or   Affects data            Define match confidence,
                    duplicate athlete       correctness and         manual review,
                    identities be handled?  historical records      merge/split, and rollback
                                                                    policy

  Q03               What is the source of   Prevents conflicting    Define ownership per
                    truth for organization, records between Hub and entity and allowed
                    athlete, session, and   client apps             synchronization direction
                    protocol data?                                  

  Q04               How do client apps      Security and usability  Review current API
                    authenticate and obtain dependency              capabilities and choose a
                    organization/session                            credential/authorization
                    context?                                        model

  Q05               Should clients pull     Affects latency,        Map supported API
                    session/configuration   offline support, and    patterns; keep heartbeat
                    updates, receive        API load                separate from data
                    events, or use a                                submission
                    hybrid?                                         

  Q06               Which raw data and      Storage cost, privacy,  Decide for force/timing
                    media are stored in the access, and retention   payloads and video/motion
                    cloud?                  implications            media separately

  Q07               What is the             Governance and audit    Define lifecycle and
                    retention/deletion      implications            deletion obligations
                    policy for raw data,                            
                    derived results, and                            
                    reports?                                        

  Q08               Which metrics are       Required for valid      Build a prioritized metric
                    canonical and which     cross-device            dictionary for the first
                    mappings are approved?  comparisons             workflows

  Q09               Which actions are       Impacts every           Define minimum
                    allowed for each role?  list/detail/exception   role-permission matrix
                                            screen                  

  Q10               Are team-scoped         Affects data filtering  Validate with target
                    permissions required in and report sharing      organizations
                    MVP?                                            

  Q11               Can a session be        Drives session and      Define per-protocol
                    completed with one      report states           completeness rules
                    required source                                 
                    missing?                                        

  Q12               How are late data and   Affects historical      Define whether completed
                    post-session            reports and user trust  sessions reopen or show a
                    corrections handled?                            post-completion update

  Q13               What is the policy for  Prevents mixing         Freeze version on session
                    protocol version        different procedures    creation or define an
                    changes?                under one label         explicit update flow

  Q14               What audit history is   Governance and support  Define event categories,
                    required and who can    needs                   visibility, and retention
                    view it?                                        

  Q15               Which export/share      Affects report scope    Select the minimum format
                    formats and             and integration         and recipient workflow for
                    destinations are        planning                the first release
                    required first?                                 

  Q16               What is the minimum     Media                   Decide whether prototype
                    useful video/motion     storage/processing may  only shows
                    workflow?               be materially different metadata/results or also
                                            from numeric sensors    media preview/reference

  Q17               What are the            Prevents duplicate data Define source record IDs,
                    retry/idempotency       and inconsistent        idempotency key, and retry
                    guarantees for client   results                 response contract
                    submissions?                                    

  Q18               Which API capabilities  Prevents requirements   Inventory existing API
                    already exist in        from assuming           contracts and mark
                    KWinHub?                nonexistent endpoints   supported, partial, or
                                                                    missing
  --------------------------------------------------------------------------------------------

### 16.1 Recommended decision order

1.  **Entity ownership and identity:** Q03, Q02.
2.  **Session completion and late data:** Q01, Q11, Q12.
3.  **Client-app API and authorization:** Q04, Q05, Q17, Q18.
4.  **Metric semantics and comparison:** Q08, Q13.
5.  **Permissions, media, retention, and sharing:** Q06, Q07, Q09, Q10,
    Q14, Q15, Q16.

This order is proposed because early choices influence data integrity
and workflow states used by many screens.

## 17. Prototype data and demonstration scenarios

### 17.1 Seeded data recommendation

**PROPOSED** --- Use a small but realistic fictional organization
with: - 2--4 teams. - 12--20 athletes. - Several user roles, including
one restricted/read-only user. - At least one force source, one timing
source, and one video/motion source. - At least one local client
application per source family where appropriate. - 3--5 protocol
templates, including a mixed-domain assessment. - Several sessions
across Draft, In progress, Awaiting data, Review required, and
Completed. - Results spanning available, flagged, missing, and pending
states. - Integration events and audit events. - One duplicate upload,
one identity conflict, one disconnection, one invalid unit/field, and
one processing failure.

These quantities are prototype suggestions, not business requirements.

### 17.2 Scenario A --- Happy-path mixed-domain assessment

**Goal:** Demonstrate the full end-to-end flow.

1.  Administrator opens Organization Overview and confirms the
    organization, team, roster, and sources are ready.
2.  Administrator opens Protocol Library and selects a mixed-domain
    protocol.
3.  Administrator creates a session and assigns athletes.
4.  The Live Session Workspace shows force, timing, and video/motion
    capture outcomes.
5.  Each source progresses independently through upload/ingestion,
    validation, and processing.
6.  The administrator reviews results and follows a result to its source
    record.
7.  The administrator opens Performance Workspace and compares eligible
    values.
8.  The administrator builds a report, sees scope and metric units, and
    generates the prototype report.

**Pass criteria** - No unsupported "all complete" status hides a missing
milestone. - All displayed results are linked to athlete/session/source
context. - Report scope and data coverage are explicit.

### 17.3 Scenario B --- Recoverable operational failure

**Goal:** Demonstrate that a realistic issue can be understood and
recovered from.

1.  During a session, the timing source disconnects or a client app
    stops uploading.
2.  The session workspace shows the source problem and its impact.
3.  Force data may still arrive; video/motion may be pending processing.
4.  The administrator opens Data Operations / Sync Center.
5.  The administrator inspects the error, retries or waits for the
    relevant transport action, and sees the outcome.
6.  A duplicate payload or identity conflict is then shown and resolved
    through a review flow.
7.  The session summary reflects the remaining missing/flagged data.
8.  The report preview discloses incomplete data if the source has not
    recovered.

**Pass criteria** - The UI distinguishes transport failure from
validation/processing failure. - The user sees a clear next action and
an explicit outcome. - The prototype does not silently duplicate a
result or silently guess an athlete identity.

### 17.4 Scenario C --- Permission boundary

**Goal:** Validate that the interface represents authorization
constraints clearly.

1.  A read-only or team-limited user opens a report or athlete profile
    outside their permitted scope.
2.  The UI shows a clear restricted state or omits the data according to
    the eventual security policy.
3.  Edit, correction, and export controls are not presented as available
    actions.
4.  The user can return to authorized content or see how to request
    access.

**Pass criteria** - Restricted data is not exposed in the UI. - The
state is not confused with a missing record or system error. - The
design does not imply that hiding a button alone is sufficient backend
authorization.

## 18. UX and content standards

### 18.1 Consistency

**PROPOSED** - Use a single vocabulary for organization, athlete, team,
protocol, session, source, source record, result, and report. - Use the
same status wording and definitions across all screens. - Include units
in result tables and charts. - Show date/time context and time-zone
handling where relevant. - Preserve filters and context when navigating
to a detail view and returning. - Use realistic names and varied record
states in prototype data. - Avoid relying on color alone to communicate
success, warning, or error.

### 18.2 Empty, loading, error, and restricted states

Every major screen should define: - **Loading:** what is being loaded
and whether the user can continue. - **Empty:** why there is no content
and the next action. - **Error:** what failed and whether retry is
safe. - **Restricted:** what is unavailable because of access scope. -
**Partial:** which data is present and which is pending/missing. -
**Success:** what action completed and where the user can see the
outcome.

### 18.3 Tables and filters

**PROPOSED** - Provide search/filter/sort where a list may become
long. - Keep state and next action visible without forcing the user to
open every row. - Distinguish display name from stable ID when resolving
identity or source issues. - Avoid presenting raw vendor identifiers as
the primary label unless the user is in a technical detail view. - Use
pagination or another scalable pattern if the prototype includes large
record counts; exact implementation is not required at this stage.

### 18.4 Charts and comparisons

**PROPOSED** - Label axes and units. - Distinguish missing points from
zero. - Provide date/session context for each value. - Show flags and
comparability warnings in chart legends, tooltips, or adjacent
summaries. - Avoid visual comparisons across incompatible units or
definitions. - Provide a table alternative for key values so users are
not dependent on chart interpretation.

## 19. Traceability matrix

This matrix links user goals to prototype screens and acceptance
scenarios.

  ------------------------------------------------------------------------------
  User goal                Main screens      Core requirements Scenario
  ------------------------ ----------------- ----------------- -----------------
  Know organization        S01, S02, S03,    ORG-01--05,       A
  readiness                S06               SRC-01--05        

  Prepare athletes         S04, S05          ATH-01--05        A, B

  Confirm source readiness S06, S14          SRC-01--05,       A, B
                                             INT-01--03        

  Configure a session      S07, S08          TST-01--03        A

  Monitor live testing     S09               TST-04--07        A, B

  Resolve                  S10, S14          DAT-01--08,       B
  ingestion/data-quality                     INT-03            
  issues                                                       

  Trust and trace a result S05, S11, S12     RES-01--05        A, B

  Compare measurements     S12               RES-04--06        A

  Generate a report        S13               RPT-01--05        A, B

  Respect access           S03, S05, S13,    ORG-04--05,       C
  boundaries               S14               RPT-08            

  Understand changes and   S10, S11, S14     DAT-10, INT-05    B
  corrections                                                  
  ------------------------------------------------------------------------------

## 20. Prototype review checklist

Use this checklist during design reviews. It is a **PROPOSED**
validation tool.

### Organization and people

-   [ ] Organization context is visible and stable.
-   [ ] Readiness explains what is complete and what remains.
-   [ ] Athlete identity and source identifiers are distinguishable.
-   [ ] Restricted access has an explicit state.

### Sources and sessions

-   [ ] Force, timing, and video/motion are represented.
-   [ ] Device connection and Hub synchronization are distinct.
-   [ ] Protocol and session show version/context.
-   [ ] Required versus optional sources are explicit.
-   [ ] Capture, ingestion, validation, and analysis are distinct
    milestones.

### Data operations

-   [ ] Pending, failed, validated, processed, and quarantined states
    are understandable.
-   [ ] Retry has a visible outcome.
-   [ ] Duplicate and identity conflict scenarios are represented.
-   [ ] Errors show impact and next action.
-   [ ] Source provenance is available from a result.

### Analysis and reports

-   [ ] Values include units and metric context.
-   [ ] Missing values are not treated as zero.
-   [ ] Incompatible metrics are flagged or blocked from comparison.
-   [ ] Reports disclose incomplete or flagged data.
-   [ ] Report scope and generation state are visible.

### Overall UX

-   [ ] Empty/loading/error/partial/restricted/success states exist for
    major screens.
-   [ ] Status vocabulary is consistent.
-   [ ] Simulated integration behavior is labeled honestly.
-   [ ] The happy path and at least one recovery path can be completed
    without verbal coaching.

## 21. Recommended delivery sequence for the prototype

This is a proposed sequencing plan for design/discovery, not a delivery
commitment.

### Phase 1 --- Validate the core workflow

-   Agree the terminology and state model.
-   Design S01, S04--S10, S11, and S13 at enough fidelity to run the
    three scenarios.
-   Seed realistic mixed-domain data.
-   Prototype happy path, delayed/disconnected source, identity
    conflict, and incomplete report.

### Phase 2 --- Validate trust and administration

-   Add organization/users/roles views.
-   Add permission-restricted scenarios.
-   Add source/integration health and audit views.
-   Test result provenance and correction-history presentation.

### Phase 3 --- Expand performance depth

-   Add trend and comparison refinements.
-   Validate metric definitions and comparability rules with domain
    experts.
-   Decide whether richer dashboards, normative benchmarks, and advanced
    video/motion presentation belong in the next release.

### Phase 4 --- Prepare implementation discovery

-   Inventory existing KWinHub APIs and client-app integration
    capabilities.
-   Mark each required behavior as supported, partially supported, or
    missing.
-   Define canonical entity ownership and identifiers.
-   Specify API contracts, idempotency, retry, versioning, access
    control, and data-retention behavior.
-   Convert approved requirements into engineering stories only after
    the unresolved decisions that affect them have owners and outcomes.

## 22. Decisions and next actions

### 22.1 Current agreed direction

-   **AGREED:** Primary user is the sports organization administrator.
-   **AGREED:** The core priority is a complete end-to-end testing
    workflow.
-   **AGREED:** The ecosystem is mixed: force, timing, and video/motion.
-   **AGREED:** KWinHub is cloud-first and connects with client
    applications.
-   **AGREED:** The project is at UI/UX prototype and
    product-requirements discovery stage, not production implementation.

### 22.2 Recommended next actions

1.  **Validate the session completion policy.** Decide what makes a
    session complete when data is late or one domain is missing.
2.  **Validate entity ownership and identity mapping.** Define the
    source of truth for athletes, sessions, and protocols.
3.  **Inventory current APIs.** Compare each proposed workflow with
    actual API capabilities; do not infer API support from screen names.
4.  **Prioritize the metric catalog.** Select a small set of force,
    timing, and video/motion outputs for the first end-to-end prototype.
5.  **Approve the state vocabulary.** Confirm which states are lifecycle
    states versus independent health/quality flags.
6.  **Define minimum roles and permissions.** At least one administrator
    and one restricted user should be represented in the prototype.
7.  **Confirm raw-data/media policy.** Decide what video/motion content
    is stored in the Hub versus retained by client apps.
8.  **Run scenario-based design reviews.** Test the three scenarios in
    Section 17 and record where users hesitate or misinterpret status.

### 22.3 Final note

This document is a detailed **MVP discovery and prototype
specification**. It defines a coherent proposed product workflow and the
questions that must be resolved to turn it into an implementation-ready
PRD. It does not claim that the existing KWinHub repository, APIs,
device integrations, or backend currently satisfy these requirements.
Each proposed requirement should be validated against stakeholder needs
and actual technical capabilities before implementation planning.
