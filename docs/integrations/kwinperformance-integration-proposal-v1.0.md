I recommend treating this as a Product Integration Proposal, rather than an API specification or a database redesign. The purpose is to define how KWinPerformance and KWinHub should work together from the user's perspective, identify the gaps in the current design, and establish requirements that can later be reused when you assess KWinSprint and the other desktop, iOS, and Android applications.

The key product decision is this:

KWinPerformance should remain the specialized iPad application for conducting sports-performance tests, while KWinHub becomes the shared platform for managing athletes, organizations, and test data across applications.

The integration should not require the iPad application to stop working when the network is unavailable. Nor should it force KWinSprint to adopt the iPad's local SQLite schema as its data contract.

The proposal below builds on your original iPad database document, the existing KWinHub MVP review, and the product direction described in your competitor analysis. It distinguishes existing design decisions from proposed requirements that still need validation.

# KWinPerformance × KWinHub

## Product Integration Proposal — V1.0

**Document type:** Product Management / Integration Requirements\
**Status:** Draft for product and engineering review\
**Scope:** KWinPerformance on iPad ↔ KWinHub\
**Future consideration:** KWinSprint desktop and other Kunwei applications\
**Primary objective:** Establish a reliable, extensible integration between local sports-performance testing and the shared KWinHub platform.

---

## 1. Executive Summary

### 1.1 Background

Kunwei has multiple software products supporting different testing workflows. KWinPerformance is an iPad application designed for sports-performance testing, including force-plate acquisition, movement segmentation, indicator calculation, and related assets. KWinSprint is a desktop application serving sprint testing workflows.

The current KWinPerformance database design uses SQLite for local application data. Its core model includes `athlete`, `records`, `record_fp`, `segments`, `record_indicators`, `assets`, and test-solution tables.

KWinHub is being developed as a shared sports-performance data platform. Its intended capabilities include organization and user management, athlete profiles, test-session ingestion, results, asset references, history, and reporting.

Integrating these products creates an opportunity to establish a unified athlete and test-data experience without merging their specialized acquisition workflows.

### 1.2 Product vision

**One athlete identity, multiple testing applications, one unified performance history.**

A coach should be able to:

1. Manage an athlete in KWinHub.
2. Conduct a test using KWinPerformance or another authorized Kunwei application.
3. Save the test locally, even without an internet connection.
4. Synchronize the completed test to KWinHub.
5. View the test and its results in the athlete's unified history.
6. Later, view comparable results collected by KWinSprint or other connected applications.

### 1.3 Recommended product boundaries

| Product              | Primary responsibility                                                                                                                                                 |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| KWinPerformance      | iPad testing workflow, device interaction, force-plate acquisition, movement segmentation, specialized analysis, local storage                                         |
| KWinSprint           | Desktop sprint-testing workflow, timing and force acquisition, specialized analysis, local storage                                                                     |
| KWinHub              | Shared athlete and organization management, cross-application test history, centralized results and asset metadata, synchronization monitoring, reporting and analysis |
| KWinHub API          | Authentication, authorization, data validation, ingestion, idempotency, synchronization contracts                                                                      |
| Shared data contract | Common identity, test-session, device, result, asset, and provenance definitions                                                                                       |

The boundary between local calculation and cloud calculation remains a product decision. For V1, existing applications should continue to own their established acquisition and calculation workflows unless a specific calculation is intentionally moved to KWinHub.

---

## 2. Objectives and Non-Goals

### 2.1 Objectives

- Establish a reliable data path from KWinPerformance to KWinHub.
- Maintain one authoritative athlete identity within each organization.
- Support offline testing and automatic or user-triggered synchronization.
- Preserve test results, raw-data references, and scientific provenance.
- Allow a test session to associate with multiple devices and assets.
- Make synchronization status and failures understandable to users.
- Define reusable integration requirements for future desktop and mobile clients.

### 2.2 Non-goals for V1

- Replacing KWinPerformance's existing testing UI.
- Rewriting KWinSprint or migrating all applications simultaneously.
- Replacing every local database with a shared cloud database.
- Reimplementing all scientific algorithms in KWinHub.
- Building a complete training-plan, rehabilitation, or AI coaching system.
- Requiring all raw force signals and video files to be stored directly in a relational database.
- Automatically merging potentially duplicate athlete profiles without user confirmation or a defined matching policy.

---

## 3. Current-State Gap Assessment

The following gaps are inferred from the original KWinPerformance database design and the existing KWinHub MVP review. They are requirements to validate, not claims that every capability is entirely absent from the current codebase.

| ID     | Gap                                                                                                                        | Product impact                                                                             | Priority |
| ------ | -------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ | -------- |
| GAP-01 | No finalized cross-application athlete identity contract                                                                   | The same athlete may have different IDs in local applications and KWinHub                  | P0       |
| GAP-02 | No finalized session-ingestion contract mapping local `records` to KWinHub sessions                                        | Applications may upload incompatible test representations                                  | P0       |
| GAP-03 | Existing `sync_state` is designed around local entity synchronization, but the end-to-end upload contract is not finalized | Users may not know whether a test was received, rejected, or fully synchronized            | P0       |
| GAP-04 | Local audit/version fields do not by themselves define cross-device conflict resolution                                    | Updates may overwrite newer data or create inconsistencies                                 | P0       |
| GAP-05 | Local `record_indicators` does not fully define reusable, versioned metric semantics                                       | Identically named indicators may not be comparable across applications                     | P0       |
| GAP-06 | `assets` references files, but a complete cloud-upload and access lifecycle is not specified                               | Videos or raw data may be missing while a test appears uploaded                            | P0       |
| GAP-07 | The local test model is primarily force-plate-oriented                                                                     | Sprint and future device integrations may require different trial and device relationships | P1       |
| GAP-08 | Protocol and algorithm provenance are not yet defined as an end-to-end contract                                            | Historical results may be difficult to reproduce or compare                                | P1       |
| GAP-09 | The current local model contains application-specific concepts and preferences                                             | Cloud synchronization could accidentally expose local UI settings as global business data  | P1       |
| GAP-10 | Cross-application discovery, filtering, and error handling are not yet specified                                           | The user experience may differ substantially between clients                               | P1       |

**P0:** Required before a reliable production pilot.\
**P1:** Required to prepare the platform for broader integration; some items can be phased after the first end-to-end workflow.

---

## 4. Target User Experience

### 4.1 Primary user story

As a coach, I want to conduct tests in KWinPerformance and find their results in KWinHub, so that I can manage an athlete's long-term performance history without manually transferring data between applications.

### 4.2 Proposed workflow

1. The coach signs in to KWinPerformance and selects an authorized organization.
2. The application obtains the athlete list and relevant organization data from KWinHub when connectivity is available.
3. The coach selects an athlete and performs the test.
4. KWinPerformance saves the session, measurements, calculated indicators, and local asset references.
5. The application creates a synchronization task for the completed test.
6. KWinHub receives and validates the session metadata and structured results.
7. Required files are uploaded and verified through the agreed asset workflow.
8. KWinHub reports the ingestion and processing status.
9. KWinPerformance updates the local synchronization state.
10. The coach can find the test in KWinHub under the correct athlete and organization.

If connectivity is lost at any point, the local test remains available. Synchronization resumes when connectivity returns.

### 4.3 User-visible synchronization states

| State          | Meaning                                                                     | Recommended user message   |
| -------------- | --------------------------------------------------------------------------- | -------------------------- |
| Local only     | The test exists on this device and has not been submitted                   | Saved on this device       |
| Pending upload | Waiting for synchronization                                                 | Waiting to sync            |
| Uploading      | Data or assets are being transferred                                        | Syncing                    |
| Processing     | KWinHub has received the data but has not completed all required processing | Processing                 |
| Synchronized   | All required V1 data and assets have reached their defined successful state | Synced                     |
| Failed         | A non-recoverable error or retry limit requires attention                   | Sync failed — view details |
| Conflict       | A change requires resolution under the applicable conflict policy           | Review required            |

These states should be distinct from the scientific analysis status. A test can be successfully synchronized even if an optional analysis job fails, provided the product contract explicitly defines that behavior.

---

## 5. Proposed Integration Architecture

### 5.1 Logical architecture

```
                 KWinHub
   ┌──────────────────────────────────┐
   │ Organizations and memberships   │
   │ Athlete and group management    │
   │ Test sessions and results       │
   │ Metric definitions and versions │
   │ Asset metadata and permissions  │
   │ Sync status and audit records   │
   └────────────────┬─────────────────┘
                    │
             Versioned API
                    │
          ┌─────────┴─────────┐
          │                   │
   KWinPerformance       KWinSprint
       iPad                Desktop
          │                   │
   Local SQLite DB      Local storage
          │                   │
   Force-plate tests     Sprint tests
   Segments and metrics  Timing and metrics
   Local assets          Local assets
```

Future Android and other clients should connect through the same platform contracts rather than directly sharing database tables.

### 5.2 Architectural principles

1. **KWinHub is the shared platform, not a remote copy of the iPad database.**
2. **Each client retains its specialized testing workflow.**
3. **A successful local save must not depend on cloud availability.**
4. **Synchronization is a durable workflow, not just a periodic database upload.**
5. **Every uploaded record must retain its source application, organization, athlete identity, and provenance.**
6. **The API is the integration boundary; clients must not access the server database directly.**

---

## 6. Identity and Organization Management

### 6.1 Athlete identity

The existing local `athlete.athlete_id` is a UUID. It can serve as a stable client-generated identifier for a locally created athlete, but it should not automatically be treated as the universal athlete identity across every existing application.

The recommended model distinguishes:

- **KWinHub athlete ID:** the canonical platform identity.
- **Local athlete ID:** the ID used by an individual application.
- **External athlete reference:** an optional identifier from another system or customer.

Proposed mapping:

| KWinPerformance      | KWinHub                                                        |
| -------------------- | -------------------------------------------------------------- |
| `athlete.athlete_id` | `athlete_id` or a registered client-identity mapping           |
| `athlete.project_id` | Authorized organization ID                                     |
| `athlete.name`       | Athlete display name                                           |
| `athlete.birth_date` | Date of birth, subject to data policy                          |
| `athlete.gender`     | Gender field, subject to the agreed definition and optionality |
| `athlete.weight`     | Profile weight, with explicit units and update semantics       |
| `athlete.height`     | Profile height, with explicit units                            |

The mapping is conceptual. Exact API property names and database columns must be finalized in the technical specification.

### 6.2 Required identity behavior

- A client must be able to distinguish its local athlete ID from the canonical KWinHub ID.
- If an athlete is downloaded from KWinHub, the client should retain the canonical ID.
- If an athlete is created offline, the client should generate a stable ID and register the mapping when synchronization succeeds.
- Matching by name alone must not silently merge athlete profiles.
- An athlete's profile updates must not rewrite historical test values.
- Profile weight must be distinguished from the body weight captured for an individual test.

### 6.3 Organization and access control

The local `project` concept can map to a KWinHub organization, but the exact mapping must be confirmed against the platform's current domain model.

The server must derive permissions from authenticated identity and authorized organization membership. A client-supplied organization ID is not proof of access.

Organization isolation must apply to athlete records, sessions, results, assets, exports, and background jobs.

---

## 7. Test Session and Measurement Data Mapping

### 7.1 Core mapping

| KWinPerformance entity | Proposed KWinHub concept                   | Integration requirement                                                              |
| ---------------------- | ------------------------------------------ | ------------------------------------------------------------------------------------ |
| `records`              | Measurement session                        | Map the stable test ID, athlete, organization, responsible coach, and test metadata  |
| `record_fp`            | Session-specific metadata / device context | Preserve force-plate acquisition settings and test environment                       |
| `segments`             | Trial or movement segment                  | Preserve segment identity, timing, movement class, laterality, and status            |
| `signs`                | Event markers                              | Preserve timestamps and marker semantics                                             |
| `segment_settings`     | Trial or segment configuration             | Preserve typed settings and units where applicable                                   |
| `record_indicators`    | Result metric values                       | Map stable metric definitions, values, units, and scope                              |
| `assets`               | Data objects / asset metadata              | Preserve file identity, ownership, storage reference, checksum, and upload status    |
| `solutions`            | Protocol definition                        | Map to a versioned protocol where appropriate                                        |
| `solution_steps`       | Protocol steps                             | Preserve ordering, movement type, and step configuration                             |
| `solution_run_steps`   | Runtime execution steps                    | Include only if required to reproduce the test workflow                              |
| `module_types`         | Test type / module dictionary              | Map to a shared identifier or a documented application-specific extension            |
| `sync_state`           | Local synchronization queue and status     | Use for local orchestration; it is not a substitute for the server's ingestion state |

### 7.2 Session versus trial

KWinPerformance currently defines one `records` row as one athlete's Start-to-Stop test. This is a reasonable local concept, but the platform must explicitly distinguish:

- **Measurement session:** a logical testing event.
- **Trial:** an individual attempt or measurement unit.
- **Segment:** a movement or time interval within a trial.
- **Result:** a calculated or observed value.
- **Data object:** a raw file, video, processed artifact, or report.

For KWinPerformance, one local record may map to one session containing multiple segments and results.

For KWinSprint, a session may include multiple timing attempts, split times, force signals, and video assets. Its exact mapping must be validated during the desktop application research.

The platform should not assume that every application uses identical session and trial boundaries.

### 7.3 Multi-device support

The integration contract should allow one session to reference multiple devices and data sources. A force plate, sprint timing system, trigger device, and camera may contribute data to the same testing event.

The KWinHub MVP review already identifies the limitation of a session model with only one optional device reference. A session-to-device relationship is therefore a key platform requirement.

### 7.4 Metric definitions and values

The current local `record_indicators` design includes `name`, `value`, `unit`, `category`, `side`, `model_class`, and `is_summary`.

That is sufficient to organize many local result views, but a cross-application platform requires additional semantic consistency.

Proposed requirements:

- Assign each reusable metric a stable `metric_code`.
- Store the display name separately from the stable identifier.
- Define units and conversion rules.
- Define whether a result applies to a session, trial, segment, side, or summary.
- Distinguish numeric, categorical, bilateral, structured, and time-series-reference values where required.
- Preserve the algorithm version and calculation context.
- Avoid comparing results whose definitions are incompatible.

For example, two values labelled "Jump Height" should not automatically be treated as equivalent if they were calculated using different methods or definitions.

---

## 8. Raw Data, Video, and Asset Management

The current KWinPerformance design keeps large files outside SQLite and uses `assets` to index them. This should remain the basic approach.

### 8.1 Proposed asset workflow

1. KWinPerformance saves the local asset and its metadata.
2. The client creates a stable asset ID and calculates a checksum.
3. The client requests an authorized upload operation from KWinHub.
4. The file is uploaded to the agreed storage service.
5. The service verifies the upload and returns a confirmed state.
6. KWinHub associates the asset with the correct session, trial, or segment.
7. The client records the final status locally.

### 8.2 Asset metadata requirements

At minimum, the shared contract should support:

- Stable asset ID.
- Owning organization and session.
- Optional trial or segment association.
- Asset type and media type.
- File size and checksum.
- Original filename where useful.
- Logical storage reference.
- Upload and verification state.
- Creation time and source application.
- Optional duration, sampling information, or synchronization metadata.

Absolute local filesystem paths must not be treated as portable cloud identifiers.

### 8.3 Partial-upload behavior

A session must not be marked fully synchronized merely because its metadata has reached KWinHub while a required raw-data file is missing.

The product should distinguish between:

- Session metadata accepted.
- Structured results accepted.
- Required assets uploaded and verified.
- Overall synchronization complete.

The V1 contract must specify which asset types are mandatory and which are optional.

---

## 9. Synchronization and Conflict Handling

### 9.1 Recommended strategy

Use a **local-first, queued, retryable synchronization model**.

KWinPerformance should persist the test and enqueue synchronization work locally. The queue must survive application restarts and temporary network failures.

The client should use stable IDs and an idempotency mechanism so that retrying a request does not create duplicate sessions, segments, or assets.

### 9.2 Proposed upload sequence

1. Confirm the local test is complete enough for upload.
2. Validate local references and required metadata.
3. Submit or upsert the session using its stable client identity.
4. Submit related trial/segment records and structured results.
5. Upload required assets.
6. Query or receive the server's final processing state.
7. Mark the local sync task successful only when the defined completion conditions are met.

The exact API calls and ordering may be optimized by engineering, but the user-visible guarantees should remain the same.

### 9.3 Conflict policy

The existing local schema includes `version` and `last_modified`, with a preliminary last-write-wins approach. That is not sufficient as a universal cross-device policy.

Recommended rules:

- Completed test results should be immutable by default.
- Corrections should create an auditable revision or explicit replacement operation.
- Athlete profile fields can use a separate conflict policy.
- Protocol definitions should be versioned.
- Algorithm recalculation should preserve previous results and provenance.
- Deletion should be represented explicitly and propagated according to a defined retention policy.
- Server acceptance and rejection decisions must be traceable.

### 9.4 Recovery and error handling

The client must handle:

- Network loss during metadata submission.
- Network loss during file upload.
- Duplicate requests.
- Expired authentication.
- Invalid or deleted athlete references.
- Insufficient organization permissions.
- Unsupported schema versions.
- Server-side validation failures.
- Processing failures after successful ingestion.

Recoverable failures should be retried automatically with bounded backoff. Permanent failures should provide actionable explanations rather than silently retrying forever.

---

## 10. Authentication, Permissions, and Data Ownership

### 10.1 User experience

The user should authenticate through the supported KWinHub identity flow. Once authenticated, the client should load the organizations and permissions available to that user.

An offline session may continue under a previously authorized local context, subject to the application's security policy. Offline access must not grant additional permissions, and queued uploads must be reauthorized by the server.

### 10.2 Product requirements

- Use a supported token-based authentication mechanism.
- Do not embed permanent confidential client secrets in distributed applications.
- Validate organization membership for every server-side operation.
- Apply authorization to file uploads, downloads, reports, and exports.
- Define behavior for revoked membership and expired sessions.
- Protect athlete information and restrict access according to customer permissions.
- Maintain an audit trail for important administrative and data operations.

The existing KWinHub code review identified authentication and tenant isolation as release-blocking areas. Integration development should depend on a verified authorization boundary rather than assuming that the current prototype already provides one.

---

## 11. Functional Requirements and Acceptance Criteria

| ID      | Requirement                | Priority | Acceptance criterion                                                                                  |
| ------- | -------------------------- | -------- | ----------------------------------------------------------------------------------------------------- |
| INT-001 | Authenticate with KWinHub  | P0       | An authorized user can connect; an unauthorized user cannot access protected resources                |
| INT-002 | Select organization        | P0       | The user can select an authorized organization and cannot upload into another organization            |
| INT-003 | Map athlete identity       | P0       | A local athlete maps to one canonical platform identity without silent duplicate creation             |
| INT-004 | Upload completed session   | P0       | A real KWinPerformance test appears under the correct athlete in KWinHub                              |
| INT-005 | Idempotent ingestion       | P0       | Repeated submission of the same test does not create duplicate sessions or results                    |
| INT-006 | Offline queue              | P0       | A test saved offline is uploaded after connectivity returns without manual data recreation            |
| INT-007 | Sync status                | P0       | The user can distinguish pending, uploading, processing, success, and failure                         |
| INT-008 | Result integrity           | P0       | Uploaded values preserve the defined metric, unit, scope, and source information                      |
| INT-009 | Asset upload               | P0       | Required files are verified before the session is marked fully synchronized                           |
| INT-010 | Failure recovery           | P0       | Recoverable errors can be retried without duplicate data                                              |
| INT-011 | Tenant isolation           | P0       | Cross-organization reads and writes are rejected and covered by automated tests                       |
| INT-012 | Session-device association | P1       | A session can reference multiple devices and associated data objects                                  |
| INT-013 | Protocol provenance        | P1       | The session retains the protocol version or immutable configuration required for interpretation       |
| INT-014 | Algorithm provenance       | P1       | Historical results can be traced to the calculation version and source data                           |
| INT-015 | Cross-application history  | P1       | Results from supported clients can be discovered under the same athlete                               |
| INT-016 | Sync diagnostics           | P1       | Support personnel can identify the failing stage and relevant error without accessing raw credentials |

---

## 12. Recommended Delivery Roadmap

### Phase 0 — Product and contract alignment

**Goal:** Agree on the shared language before implementation.

Deliverables:

- Confirm the canonical athlete and organization identity model.
- Define the meaning of session, trial, segment, result, and data object.
- Agree on metric identifiers and units.
- Define synchronization states and completion criteria.
- Inventory the KWinHub entities and APIs that already exist.
- Identify which decisions are specific to KWinPerformance and which must be shared with KWinSprint.

**Exit criterion:** Product and engineering agree on a versioned integration contract for one complete test workflow.

### Phase 1 — Minimum end-to-end integration

**Goal:** Synchronize a real force-plate test from iPad to KWinHub.

Deliverables:

- Authentication and organization selection.
- Athlete identity mapping.
- Session and result ingestion.
- Idempotency and local retry queue.
- Required asset upload.
- User-visible synchronization status.
- Automated integration tests.

**Exit criterion:** A coach can complete a test offline, reconnect, synchronize it, and find the correct result in KWinHub without duplicates.

### Phase 2 — Reliability and traceability

**Goal:** Make the integration suitable for a controlled pilot.

Deliverables:

- Failure diagnostics and recovery.
- Protocol and algorithm provenance.
- Defined correction and deletion policies.
- Asset integrity verification.
- Security and tenant-isolation tests.
- Operational monitoring.

**Exit criterion:** The system can recover from realistic network and service failures while preserving data integrity and auditability.

### Phase 3 — Prepare for KWinSprint and other applications

**Goal:** Reuse the same platform contract across clients.

Deliverables:

- Compare KWinSprint's existing athlete, test, attempt, metric, and file models.
- Identify concepts that map directly to the shared contract.
- Define application-specific extensions for sprint data.
- Validate multi-device sessions and synchronized timing requirements.
- Test cross-application athlete history and metric comparability.

**Exit criterion:** KWinSprint can integrate without requiring a separate KWinHub identity or session model for every new test type.

---

## 13. Dependencies and Open Product Decisions

These decisions should be resolved before freezing the integration PRD.

| Question                                                      | Why it matters                                                               | Recommended starting point                                                                    |
| ------------------------------------------------------------- | ---------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| Is KWinHub the authoritative source for athlete identity?     | Determines local ID mapping and duplicate handling                           | Yes, for connected organization-managed identities; preserve stable offline IDs               |
| Can a client create athletes offline?                         | Determines offline workflow and reconciliation                               | Allow it, with a stable local ID and explicit mapping process                                 |
| What does one session represent across applications?          | Determines how KWinPerformance records and KWinSprint attempts are organized | Separate session from trial and segment                                                       |
| Which results must be uploaded?                               | Affects payload size, cost, and scientific traceability                      | Upload structured results and required source references; define raw-file policy by test type |
| Are raw force data and video required for a successful sync?  | Determines the final success condition                                       | Define mandatory and optional asset classes explicitly                                        |
| Which application calculates each indicator?                  | Affects consistency and historical reproducibility                           | Keep current client calculation behavior initially; version algorithms and definitions        |
| Can synchronized test results be edited or deleted?           | Affects audit history and conflict handling                                  | Treat completed results as immutable by default; use explicit correction operations           |
| How should metric definitions be shared?                      | Affects cross-client comparison                                              | Use stable codes, units, scope, and versioned definitions                                     |
| How are organizations represented?                            | Affects authorization and data mapping                                       | Confirm the existing `project` to KWinHub organization mapping                                |
| What happens when organization membership is revoked offline? | Affects privacy and access control                                           | Apply server authorization on every upload; define local cache expiry and access restrictions |
| How should timestamps be represented?                         | Affects cross-device event and video synchronization                         | Store UTC timestamps plus explicit relative-time and synchronization metadata                 |
| How will clients be upgraded?                                 | Affects backward compatibility                                               | Version API contracts and support a defined compatibility window                              |

---

## 14. Success Metrics

The integration should be measured by operational outcomes, not simply the number of endpoints implemented.

Suggested pilot metrics:

- **Upload completion rate:** percentage of eligible completed sessions successfully synchronized.
- **Duplicate rate:** percentage of sessions duplicated due to retries or reconciliation.
- **Synchronization latency:** time from local completion to cloud availability, segmented by connectivity conditions.
- **Asset integrity rate:** percentage of required assets successfully verified.
- **Recovery rate:** percentage of recoverable failures resolved without manual data recreation.
- **Identity mismatch rate:** percentage of uploads associated with the wrong or ambiguous athlete.
- **Supportability:** percentage of failed sync tasks with sufficient diagnostics to identify the cause.
- **Cross-client discoverability:** percentage of successfully synchronized tests visible in the correct KWinHub athlete history.

Targets should be established after the team has measured a baseline in representative field conditions.

---

## 15. Final Recommendation

The immediate priority is not to make KWinPerformance's SQLite database identical to KWinHub's PostgreSQL database. It is to establish a stable product contract that connects the two reliably.

I recommend three concrete next steps:

1. **Inventory the existing KWinHub MVP** to distinguish implemented APIs and domain entities from planned architecture.
2. **Define the shared identity and measurement contract** using KWinPerformance as the first real integration case.
3. **Deliver one end-to-end workflow** covering athlete mapping, local test persistence, result ingestion, required asset upload, retry behavior, and visibility in KWinHub.

KWinSprint should be included in the platform-level design from the beginning, but its detailed mapping should follow a structured review of the existing desktop application rather than assumptions based on the iPad schema.

The long-term objective is a consistent Kunwei software ecosystem: specialized applications perform testing, while KWinHub provides the shared identity, data integration, and athlete-history layer across products.

## My recommendation for your next step

Before asking the development team to implement this proposal, I would validate two artifacts side by side:

- The original KWinPerformance database specification, including the exact field definitions.
- The current KWinHub MVP domain model and API implementation.

That comparison should produce a field-level integration mapping and gap matrix with columns for local entity, KWinHub entity, transformation rules, ownership, sync direction, conflict policy, and implementation status. This will turn the product proposal into a concrete implementation plan and give you a repeatable framework for the later KWinSprint, Android, and other application assessments.