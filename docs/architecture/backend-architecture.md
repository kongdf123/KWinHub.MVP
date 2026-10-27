# KWinHub V1 — Backend Architecture & Technical Design Specification

**Product:** KWinHub / 坤维智库  
**Document Type:** Backend Architecture & Technical Design Specification  
**Version:** V1.0  
**Status:** Engineering Baseline  
**Target:** KWinHub V1 Production Development  
**Audience:** Product Managers, Backend Engineers, Frontend Engineers, DevOps/Cloud Engineers, QA Engineers, Data/Science Engineers, Technical Leads

---

# 1. Purpose

This document defines the backend and cloud architecture for KWinHub V1.

The KWinHub Product Requirements & Architecture Document defines what KWinHub should provide as a product.

This document defines how the engineering team should build the platform.

The architecture is designed for:

- SaaS deployment;
- organization-level multi-tenancy;
- international customers;
- Kunwei desktop and mobile applications;
- partner applications and APIs;
- multiple measurement devices;
- large raw-data and video assets;
- asynchronous scientific processing;
- longitudinal performance data;
- protocol-driven extensibility;
- future KWinScience expansion;
- future multi-region deployment.

The architecture must avoid turning KWinHub into a CMJ-specific testing application or a collection of independent sport-specific applications.

---

# 2. Architecture Vision

The KWinHub architecture follows this principle:

> **Every Kunwei measurement becomes reusable data.**

The fundamental data flow is:

```text
Capture anywhere
      ↓
Sync automatically
      ↓
Receive in KWinHub
      ↓
Validate
      ↓
Store
      ↓
Process
      ↓
Analyze
      ↓
Standardize
      ↓
Results / Metrics
      ↓
Longitudinal Performance
      ↓
Dashboard / Report / Research
```

This directly follows the product architecture defined in the PRD.

The client applications remain responsible for live measurement and device control.

KWinHub becomes the centralized data and intelligence platform.

---

# 3. Architectural Principles

## 3.1 Platform, not testing application

KWinHub is not the primary live-testing application.

Client applications are responsible for:

- device connection;
- device control;
- live measurement;
- sensor synchronization;
- real-time feedback;
- temporary/offline storage;
- session submission.

KWinHub is responsible for:

- centralized data;
- organization management;
- subject management;
- protocol management;
- ingestion;
- validation;
- processing;
- results;
- analysis;
- longitudinal performance;
- reports;
- integrations;
- APIs.

---

## 3.2 Protocol-driven architecture

Sports and measurement types must be represented through protocols rather than separate application architectures.

For example:

```text
Protocol
 ├── CMJ
 ├── IMTP
 ├── Sprint
 ├── High Jump
 ├── Swimming
 ├── Balance
 ├── Rehabilitation
 └── Future Protocols
```

Adding a new protocol should not require creating:

```text
CMJDatabase
SprintDatabase
JumpDatabase
```

or:

```text
CMJService
SprintService
JumpService
```

Instead:

```text
New Protocol
     ↓
Existing Session Model
     ↓
Existing Data Model
     ↓
New Processing Definition
     ↓
New Metrics
     ↓
Existing Results / Dashboard / Reports
```

---

# 4. SaaS Business and Tenant Model

## 4.1 Primary customer model

KWinHub V1 is designed as an organization-centric B2B SaaS platform.

Primary customers may include:

- professional sports organizations;
- universities;
- sports science laboratories;
- clinics;
- rehabilitation organizations;
- research organizations;
- training centers;
- national/regional sports organizations;
- Kunwei internal organizations;
- future enterprise customers.

Individual consumer accounts are not the primary V1 product model.

---

# 5. Multi-Tenant Architecture

The logical tenant hierarchy is:

```text
KWinHub
│
├── Organization A
│   ├── Users
│   ├── Teams / Groups
│   ├── Subjects
│   ├── Devices
│   ├── Data Sources
│   ├── Protocols
│   ├── Sessions
│   ├── Results
│   └── Reports
│
├── Organization B
│   └── ...
│
└── Organization C
    └── ...
```

Every tenant-owned resource must be associated with an organization.

Typical tables must contain:

```text
organization_id
```

Examples:

```text
subjects
measurement_sessions
protocols
datasets
data_objects
results
reports
devices
data_sources
integrations
processing_jobs
```

---

# 6. V1 Tenant Isolation Strategy

V1 should use:

> **Shared application + shared PostgreSQL cluster + logical tenant isolation**

Example:

```text
measurement_sessions

id
organization_id
subject_id
protocol_id
status
created_at
...
```

The backend must never trust the client-provided `organization_id`.

The organization context must come from the authenticated identity/application context.

Incorrect:

```http
POST /sessions

{
    "organizationId": "org-123"
}
```

and blindly accepting it.

Correct:

```text
Authenticated Principal
        ↓
Organization Membership
        ↓
Authorization Context
        ↓
Repository / Query
        ↓
organization_id = authorized organization
```

---

# 7. Future Enterprise Isolation

The architecture should allow future enterprise customers to use dedicated infrastructure.

Possible evolution:

```text
Standard Tenant
     ↓
Shared Database

Enterprise Tenant
     ↓
Dedicated Database

Enterprise / Regulatory Tenant
     ↓
Dedicated Region + Database + Storage
```

The domain model should not depend on which isolation strategy is used.

---

# 8. Global / International Architecture

KWinHub should be designed for international use from V1, but operational complexity should remain controlled.

## V1

Deploy initially to one primary cloud region:

```text
Global Users
     ↓
Global DNS / CDN
     ↓
Primary Region
     ↓
KWinHub
```

## Future

Support regional deployment:

```text
                 Global Control Plane
                         │
          ┌──────────────┼──────────────┐
          ↓              ↓              ↓
       APAC            Europe           US
          │              │              │
      KWinHub         KWinHub         KWinHub
      Region          Region          Region
```

The application should not assume:

```text
one country
one timezone
one currency
one language
one storage region
```

---

# 9. Recommended Technology Stack

## 9.1 Backend

**ASP.NET Core / .NET**

Recommended architecture:

```text
.NET
ASP.NET Core
EF Core
PostgreSQL
```

Rationale:

- strong fit with Kunwei's existing engineering ecosystem;
- mature enterprise backend platform;
- strong async processing support;
- good API and OpenAPI tooling;
- good cloud/container support;
- suitable for high-performance data processing;
- clean integration boundary with KWinScience.

---

## 9.2 Database

**PostgreSQL**

Use PostgreSQL for:

- transactional data;
- tenant metadata;
- users and organizations;
- subjects;
- sessions;
- trials;
- protocols;
- results;
- processing state;
- device metadata;
- integration metadata;
- audit records.

Do not use PostgreSQL as the primary storage location for large raw binary data.

---

## 9.3 Object Storage

Use cloud object storage compatible with S3-style semantics.

Examples:

```text
AWS S3
Azure Blob Storage
Google Cloud Storage
```

Object storage is used for:

- raw force data;
- raw sensor data;
- video;
- images;
- imported files;
- processed data;
- generated reports;
- research datasets.

---

## 9.4 Cache

Use:

```text
Redis
```

for:

- short-lived cache;
- distributed locks where necessary;
- rate limiting;
- temporary processing state;
- session-independent performance caching.

Redis must not become the system of record.

---

## 9.5 Messaging

Use a cloud-managed queue/event system.

The exact provider may depend on the selected cloud platform.

The abstraction should support:

```text
Queue
Topic
Event
Retry
Dead Letter Queue
```

---

# 10. High-Level Architecture

```text
                         KWinHub SaaS
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
      Web UI             Client Applications    Partner APIs
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
                         API Gateway
                              │
              ┌───────────────┼────────────────┐
              │               │                │
          Identity        Core Domain      Integrations
              │               │                │
              │      ┌────────┼────────┐       │
              │      │        │        │       │
              │  Subjects Sessions Protocols   │
              │      │        │        │       │
              │      └────────┼────────┘       │
              │               │                │
              └───────────────┼────────────────┘
                              │
                     Application Services
                              │
              ┌───────────────┼────────────────┐
              │               │                │
         PostgreSQL      Object Storage       Redis
              │               │
              │       Raw / Video / Files
              │
              └───────────────┬────────────────┘
                              │
                         Message Queue
                              │
                    Processing Workers
                              │
                              ▼
                         KWinScience
                              │
                              ▼
                      Events / Metrics
                              │
                              ▼
                       Results / Analysis
                              │
              ┌───────────────┼────────────────┐
              ↓               ↓                ↓
         Dashboard         Reports       Longitudinal
                                            Performance
```

---

# 11. Modular Monolith Strategy

KWinHub V1 should **not start as a large microservice system**.

Recommended architecture:

> **Modular Monolith + Asynchronous Workers**

The API and core domain should initially be deployed as one application while maintaining strong internal module boundaries.

Suggested modules:

```text
KWinHub.Api
│
├── Identity
├── Organizations
├── Subjects
├── Teams
├── Protocols
├── MeasurementSessions
├── Datasets
├── DataObjects
├── Results
├── Analysis
├── Reports
├── Devices
├── DataSources
├── Integrations
├── Sync
└── Administration
```

Workers are separate deployable processes.

---

# 12. Backend Repository Architecture

Recommended structure:

```text
KWinHub/
│
├── src/
│   ├── KWinHub.Api/
│   │
│   ├── KWinHub.Application/
│   │   ├── Subjects/
│   │   ├── Sessions/
│   │   ├── Protocols/
│   │   ├── Results/
│   │   ├── Reports/
│   │   └── Integrations/
│   │
│   ├── KWinHub.Domain/
│   │   ├── Organizations/
│   │   ├── Subjects/
│   │   ├── Sessions/
│   │   ├── Protocols/
│   │   ├── Results/
│   │   └── Processing/
│   │
│   ├── KWinHub.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Storage/
│   │   ├── Messaging/
│   │   ├── Identity/
│   │   └── Integrations/
│   │
│   ├── KWinHub.Workers/
│   │
│   └── KWinHub.Contracts/
│
├── tests/
│   ├── Unit/
│   ├── Integration/
│   ├── Contract/
│   └── EndToEnd/
│
├── deploy/
│   ├── terraform/
│   ├── docker/
│   └── kubernetes/
│
└── docs/
```

---

# 13. Domain Model

The canonical domain model is:

```text
Organization
      │
      ├── Users
      ├── Teams
      ├── Subjects
      ├── Devices
      ├── Data Sources
      ├── Protocols
      │
      └── Measurement Sessions
                │
                ├── Trials
                │
                ├── Datasets
                │
                ├── Raw Data Objects
                │
                ├── Processing Jobs
                │
                └── Results
                        │
                        └── Metrics
```

---

# 14. Core Entities

## 14.1 Organization

```text
Organization
----------------
id
name
slug
status
region
timezone
locale
created_at
updated_at
```

---

## 14.2 User

```text
User
----------------
id
identity_provider_id
email
display_name
status
created_at
updated_at
```

User identity should be separated from organization membership.

---

## 14.3 Organization Membership

```text
OrganizationMembership
----------------
id
organization_id
user_id
role_id
status
created_at
```

This supports a user belonging to multiple organizations in the future.

---

# 15. Roles and Authorization

V1 should support RBAC.

Initial conceptual roles:

```text
Organization Owner
Administrator
Manager
Practitioner / Coach
Researcher
Viewer
Application
```

The exact product-facing role names can evolve.

Authorization must be enforced at the API/service layer.

UI visibility is not security.

---

# 16. Subject

The PRD defines Subject as the canonical human entity rather than limiting the system to athletes.

Recommended model:

```text
Subject
----------------
id
organization_id
external_id
subject_type
first_name
last_name
display_name
date_of_birth
sex
status
metadata
created_at
updated_at
```

Subject types:

```text
ATHLETE
PATIENT
PARTICIPANT
RESEARCH_SUBJECT
OTHER
```

Sensitive personal information should be handled according to applicable privacy requirements.

---

# 17. Measurement Session

Measurement Session is the canonical historical measurement object.

```text
MeasurementSession
----------------
id
organization_id
subject_id
protocol_id
protocol_version
source_id
device_id
status
started_at
completed_at
timezone
client_application
client_version
external_id
created_at
updated_at
```

A session must remain useful even if the original client application is no longer available.

---

# 18. Trial

A session may contain multiple trials.

```text
MeasurementSession
    │
    ├── Trial 1
    ├── Trial 2
    └── Trial 3
```

```text
Trial
----------------
id
session_id
trial_index
status
started_at
duration
quality_status
metadata
```

Trial-level raw and processed data should be independently addressable.

---

# 19. Protocol

```text
Protocol
----------------
id
organization_id
name
code
category
sport
version
status
description
input_schema
trial_configuration
validation_rules
processing_definition
metric_definition
visualization_definition
comparison_definition
report_definition
created_at
updated_at
```

Protocols must be versioned.

A historical session must retain the exact protocol version used during processing.

---

# 20. Dataset

Dataset is a logical collection of data.

```text
Dataset
----------------
id
organization_id
session_id
type
name
version
status
metadata
created_at
```

Possible dataset types:

```text
RAW
PROCESSED
ANALYSIS
RESEARCH
EXPORT
```

---

# 21. Data Object

Large data should be represented as objects rather than embedded in relational rows.

```text
DataObject
----------------
id
organization_id
dataset_id
session_id
trial_id
type
format
storage_provider
storage_key
size_bytes
checksum
sample_rate
duration
metadata
created_at
```

Examples:

```text
FORCE_SIGNAL
VIDEO
IMU_SIGNAL
EMG_SIGNAL
TIMING_SIGNAL
POSE_DATA
IMAGE
CSV
JSON
BINARY
```

---

# 22. Object Storage Layout

Logical storage structure:

```text
/{organizationId}/
    sessions/
        {sessionId}/
            raw/
            processed/
            video/
            reports/
            exports/
```

Example:

```text
org_001/
  sessions/
    sess_123/
      raw/
        trial_001_force.bin
      video/
        camera_01.mp4
      processed/
        events.json
        metrics.json
      reports/
        report_001.pdf
```

Storage keys must never be treated as public URLs.

Use signed URLs for access.

---

# 23. Upload Architecture

Large files should bypass the API server.

Recommended flow:

```text
Client
  │
  │ Create Session
  ▼
KWinHub API
  │
  │ Signed Upload URL
  ▼
Client
  │
  │ Direct Upload
  ▼
Object Storage
  │
  │ Upload Complete Event
  ▼
KWinHub
  │
  ▼
Processing Queue
```

The API should not proxy large video files.

---

# 24. Data Ingestion

Session ingestion should support:

```text
POST /v1/sessions
```

and asynchronous processing.

Expected behavior:

```text
Client
   ↓
POST /sessions
   ↓
202 Accepted
   ↓
Session Created
   ↓
Upload Data
   ↓
Processing
```

The PRD explicitly defines asynchronous ingestion and processing as a core architecture requirement.

---

# 25. Idempotency

All important ingestion operations must support idempotency.

Example:

```http
Idempotency-Key: 7a2c...
```

The server must ensure repeated submission does not create duplicate sessions or duplicate processing jobs.

Recommended idempotency scope:

```text
organization
+
client_application
+
idempotency_key
```

---

# 26. Processing Lifecycle

The canonical lifecycle is:

```text
RECEIVED
    ↓
VALIDATING
    ↓
VALIDATED
    ↓
QUEUED
    ↓
PROCESSING
    ↓
ANALYZING
    ↓
PUBLISHED
```

Failure states:

```text
VALIDATION_FAILED
PROCESSING_FAILED
ANALYSIS_FAILED
```

Retryable jobs should return to:

```text
QUEUED
```

rather than creating an entirely new session.

---

# 27. Processing Job

Processing is a first-class backend concept.

```text
ProcessingJob
----------------
id
organization_id
session_id
job_type
status
priority
attempt
algorithm_version
protocol_version
started_at
completed_at
error_code
error_message
created_at
updated_at
```

Possible job types:

```text
VALIDATION
PROCESSING
ANALYSIS
REPORT_GENERATION
DATA_EXPORT
IMPORT
```

---

# 28. Worker Architecture

Workers should be independently scalable.

```text
Queue
 │
 ├── Validation Worker
 │
 ├── Processing Worker
 │
 ├── Analysis Worker
 │
 ├── Report Worker
 │
 └── Webhook Worker
```

Workers must be stateless wherever possible.

---

# 29. KWinScience Boundary

KWinHub and KWinScience must remain separate architectural responsibilities.

## KWinHub

```text
Organizations
Subjects
Sessions
Protocols
Storage
APIs
Permissions
Processing orchestration
Results
Reports
```

## KWinScience

```text
Signal Processing
Event Detection
Biomechanics
Metric Calculation
Performance Algorithms
Norms
Scientific Analysis
```

This separation is explicitly defined in the PRD's long-term architecture.

---

# 30. KWinScience Interface

KWinHub should communicate with KWinScience through a stable processing contract.

Conceptually:

```text
ProcessingRequest
        ↓
KWinScience
        ↓
ProcessingResult
```

Example:

```json
{
  "protocol": "CMJ",
  "protocolVersion": "2.0",
  "algorithmVersion": "cmj-3.1.0",
  "input": {
    "datasetId": "dataset_123"
  }
}
```

Response:

```json
{
  "events": [],
  "metrics": [],
  "quality": {},
  "metadata": {}
}
```

KWinHub should not implement scientific formulas.

---

# 31. Algorithm Versioning

Historical reproducibility requires explicit algorithm versioning.

Every processed result should be traceable to:

```text
Protocol Version
Algorithm Version
Input Dataset Version
Processing Time
```

Example:

```text
Session
 ├── Protocol: CMJ v2.0
 ├── Algorithm: CMJ-3.1.0
 ├── Dataset: force.v1
 └── Result: generated 2026-10-04
```

Updating the algorithm must not silently modify historical results.

---

# 32. Result Model

```text
Result
----------------
id
organization_id
session_id
trial_id
protocol_id
protocol_version
algorithm_version
status
quality_status
created_at
```

Metrics should be represented generically.

```text
Metric
----------------
id
result_id
metric_code
metric_name
value
unit
side
context
metadata
```

This allows the same infrastructure to represent:

```text
Jump Height
Peak Force
RSI
Sprint Time
Velocity
Impulse
Asymmetry
Range of Motion
```

without hard-coding them into the database architecture.

---

# 33. Analysis Layer

Analysis should consume standardized results.

```text
Results
   ↓
Performance
   ↓
Trends
   ↓
Comparisons
   ↓
Longitudinal Performance
```

Comparison modes may include:

```text
Current vs Previous
Current vs Baseline
Current vs Personal Best
Current vs Team
Current vs Group
```

The backend should expose generic query capabilities rather than protocol-specific APIs.

---

# 34. API Architecture

Public API base:

```text
https://api.kunwei.com/v1
```

The API should be resource-oriented.

Example:

```text
/v1/organizations
/v1/subjects
/v1/teams
/v1/protocols
/v1/sessions
/v1/datasets
/v1/data-objects
/v1/results
/v1/reports
/v1/devices
/v1/data-sources
/v1/integrations
/v1/sync
/v1/webhooks
```

API versioning must be independent from protocol and algorithm versioning.

---

# 35. API Design Rules

All APIs should provide:

- stable resource identifiers;
- consistent error format;
- pagination;
- filtering;
- sorting;
- validation;
- idempotency where applicable;
- authentication;
- authorization;
- correlation IDs;
- rate limiting;
- API versioning.

---

# 36. API Error Model

Use a consistent structure:

```json
{
  "code": "SESSION_NOT_FOUND",
  "message": "Measurement session was not found.",
  "traceId": "7f91...",
  "details": {}
}
```

Clients should rely on `code`, not parse human-readable messages.

---

# 37. Authentication

Authentication should be abstracted behind an identity layer.

Supported/future mechanisms:

```text
Email / Password
OAuth
OpenID Connect
Enterprise SSO
Machine-to-Machine
Application Credentials
API Keys
```

The PRD explicitly requires authentication abstraction and future support for enterprise identity mechanisms.

---

# 38. Client Application Authentication

Kunwei desktop/mobile applications must not embed privileged organization credentials.

Recommended:

```text
Application Registration
       ↓
Application Credential
       ↓
Authentication
       ↓
Organization Context
       ↓
Access Token
       ↓
KWinHub API
```

Application credentials should be revocable and rotatable.

---

# 39. Authorization

Authorization must be enforced at:

```text
API
 ↓
Application Service
 ↓
Repository / Data Access
```

Not merely:

```text
Frontend
```

Every request should resolve:

```text
User / Application
       ↓
Organization
       ↓
Role
       ↓
Permission
       ↓
Resource
```

---

# 40. API Security

Minimum V1 requirements:

- HTTPS;
- bearer authentication;
- organization authorization;
- API key management;
- idempotency;
- request validation;
- rate limiting;
- audit logging;
- webhook signature verification;
- secure file access.

These requirements are consistent with the PRD security baseline.

---

# 41. Data Source

Data Source represents where measurement data comes from.

```text
DataSource
----------------
id
organization_id
name
type
provider
version
status
configuration
last_sync_at
error_count
created_at
updated_at
```

Examples:

```text
KWinPerformance
KWinSprint
Kunwei Force Plate
Partner API
Research Import
CSV Import
```

---

# 42. Device

```text
Device
----------------
id
organization_id
data_source_id
device_type
manufacturer
model
serial_number
firmware_version
status
last_seen_at
metadata
```

Device identity should be independent from individual sessions.

---

# 43. Heartbeat

Heartbeat is a device/application-health mechanism.

It should not be used as the primary measurement-data transport.

Example:

```http
POST /v1/client/heartbeat
```

Possible information:

```text
Application Version
Device Status
Connectivity
Offline Queue Size
Last Synchronization
Client Timestamp
```

Measurement data should use the ingestion/upload API.

---

# 44. Sync Architecture

Sync jobs should be represented independently from measurement sessions.

```text
SyncJob
----------------
id
organization_id
source_id
session_id
status
started_at
completed_at
attempt
error_code
error_message
created_at
```

Statuses:

```text
QUEUED
RECEIVED
PROCESSING
COMPLETED
FAILED
```

This directly supports the PRD Sync Center.

---

# 45. Webhook Architecture

KWinHub should eventually support outbound webhooks.

Example event:

```text
session.received
session.processing
session.completed
session.failed
result.created
report.generated
```

Webhook request:

```http
POST https://client.example.com/webhooks/kwinhub

X-Kunwei-Signature: ...
X-Kunwei-Event: session.completed
```

Payloads must be signed.

---

# 46. Webhook Reliability

Webhook delivery must support:

```text
Retry
Exponential Backoff
Dead Letter
Delivery History
Signature Verification
Idempotent Consumer
```

A webhook failure must not cause the underlying session or result transaction to fail.

---

# 47. Reporting

Report generation should be asynchronous.

```text
POST /reports
      ↓
202 Accepted
      ↓
Report Job
      ↓
Report Worker
      ↓
PDF / Other Output
      ↓
Object Storage
      ↓
Signed Download URL
```

The report builder defined by the PRD should ultimately map to backend resources rather than frontend-only state.

---

# 48. Audit Logging

Audit events should record important actions:

```text
User Login
Organization Changes
User Role Changes
Subject Creation
Subject Update
Data Upload
Data Download
Data Deletion
Protocol Changes
Processing Retry
Integration Changes
API Credential Changes
Report Generation
```

Example:

```text
AuditLog
----------------
id
organization_id
actor_id
action
resource_type
resource_id
timestamp
ip_address
metadata
```

---

# 49. Observability

Every request should have:

```text
traceId
correlationId
```

Important identifiers:

```text
organizationId
sessionId
trialId
processingJobId
datasetId
resultId
```

These identifiers should appear in structured logs where appropriate.

---

# 50. Distributed Trace

The desired debugging chain is:

```text
Client
 ↓
API
 ↓
Session
 ↓
Upload
 ↓
Queue
 ↓
Worker
 ↓
KWinScience
 ↓
Result
 ↓
Webhook
```

An engineer should be able to trace a failed measurement through the entire lifecycle.

---

# 51. Metrics

At minimum monitor:

```text
API request rate
API latency
API error rate
Authentication failures
Upload throughput
Queue depth
Processing latency
Processing failure rate
Worker utilization
Database latency
Object-storage failures
Webhook delivery failures
```

Business metrics:

```text
Sessions received
Sessions processed
Sessions failed
Results generated
Active organizations
Active users
Connected devices
```

---

# 52. Logging

Use structured logging.

Avoid:

```text
"Something went wrong"
```

Prefer:

```json
{
  "event": "processing_failed",
  "sessionId": "sess_123",
  "jobId": "job_456",
  "algorithmVersion": "cmj-3.1.0",
  "errorCode": "INVALID_SIGNAL",
  "traceId": "..."
}
```

Never log:

- passwords;
- access tokens;
- API secrets;
- private credentials;
- unnecessary sensitive subject information.

---

# 53. Database Architecture

PostgreSQL is the system of record.

Logical groups:

```text
Identity
 ├── users
 ├── organizations
 ├── memberships
 └── roles

Core
 ├── subjects
 ├── teams
 ├── devices
 └── data_sources

Measurement
 ├── protocols
 ├── sessions
 ├── trials
 ├── datasets
 └── data_objects

Processing
 ├── processing_jobs
 ├── processing_attempts
 └── processing_errors

Results
 ├── results
 ├── metrics
 └── analysis_records

Platform
 ├── integrations
 ├── sync_jobs
 ├── webhooks
 └── audit_logs
```

---

# 54. Database Rules

Every migration must be:

- version controlled;
- repeatable;
- tested;
- backward-compatible where required;
- deployable independently from application release where practical.

Avoid database changes that require long production downtime.

---

# 55. Search

V1 can initially use PostgreSQL search capabilities for:

- subject search;
- session search;
- protocol search;
- device search.

Do not introduce Elasticsearch/OpenSearch unless actual scale or search requirements justify it.

The architecture should allow introducing a dedicated search engine later.

---

# 56. Caching

Cache:

- protocol metadata;
- organization configuration;
- frequently accessed dashboards;
- non-sensitive reference data.

Do not cache raw measurement data as the authoritative source.

Cache invalidation should be explicit.

---

# 57. Data Retention

Raw data should be retained according to organization policy and product plan.

The system should distinguish:

```text
Operational Data
Raw Data
Processed Data
Reports
Audit Data
Temporary Data
```

Retention policies should be configurable in future SaaS plans.

---

# 58. Backup and Disaster Recovery

Production database:

```text
Automated Backups
Point-in-Time Recovery
Backup Verification
```

Object storage:

```text
Versioning where appropriate
Replication
Lifecycle policies
Recovery procedures
```

Define target:

```text
RPO
RTO
```

before production launch.

Initial V1 target should be explicitly agreed with engineering/business rather than left undefined.

---

# 59. Data Privacy

The architecture must support international privacy requirements.

The system should be capable of:

```text
Data Export
Data Correction
Data Deletion
Access Control
Audit
Retention Policies
Regional Storage
```

The exact legal requirements should be validated with legal/compliance teams.

This document does not itself define legal compliance.

---

# 60. Internationalization

Backend must support:

```text
UTC storage
Organization timezone
Localized display
Locale
Units
Metric systems
Language
```

Recommended rule:

> Store timestamps in UTC; convert to organization/user timezone at presentation boundaries.

Measurements should retain their unit definitions explicitly.

---

# 61. Units

Metrics must not rely on implicit units.

Example:

```text
metric_code: peak_force
value: 1820.4
unit: N
```

Not:

```text
peak_force: 1820.4
```

This is especially important when international customers use different measurement conventions.

---

# 62. Client API Contract

Client applications should interact through stable contracts.

Example:

```text
Client Application
       ↓
KWinHub.Contracts
       ↓
REST API
```

The client must not depend on internal domain models.

---

# 63. SDK Strategy

Do not build many SDKs initially.

First establish:

```text
OpenAPI
REST API
Authentication
Examples
Integration Guide
```

Later provide:

```text
.NET SDK
TypeScript SDK
Python SDK
```

if partner demand justifies them.

---

# 64. Integration Types

V1 should conceptually support:

```text
Kunwei Application
Kunwei Device
Partner API
File Import
Research Import
```

Authentication types:

```text
API Key
OAuth 2.0
Machine-to-Machine
```

These are explicitly identified by the PRD.

---

# 65. API Rate Limiting

Rate limits should exist at several levels:

```text
IP
User
Application
Organization
API Key
```

Different limits can eventually be applied to:

```text
Read APIs
Write APIs
Upload initialization
Report generation
Export
```

---

# 66. API Documentation

OpenAPI should be generated from the backend contract.

Documentation should provide:

```text
Authentication
Organizations
Subjects
Sessions
Protocols
Data Upload
Results
Reports
Webhooks
Errors
Rate Limits
Examples
```

Partner-facing API documentation should be separate from internal engineering documentation.

---

# 67. CI/CD

Recommended flow:

```text
Git Push
   ↓
Build
   ↓
Unit Tests
   ↓
Static Analysis
   ↓
Integration Tests
   ↓
Container Build
   ↓
Security Scan
   ↓
Deploy Staging
   ↓
Smoke Test
   ↓
Production Approval
   ↓
Production Deploy
```

---

# 68. Infrastructure as Code

Cloud infrastructure should be defined through IaC.

Recommended:

```text
Terraform
```

Potential infrastructure:

```text
Networking
Database
Object Storage
Cache
Queue
Container Runtime
Secrets
Monitoring
CDN
DNS
```

Infrastructure changes must be version controlled.

---

# 69. Deployment Model

V1:

```text
Cloud
 │
 ├── Load Balancer
 │
 ├── KWinHub API
 │
 ├── Worker
 │
 ├── PostgreSQL
 │
 ├── Redis
 │
 ├── Queue
 │
 └── Object Storage
```

Containers should be the standard deployment unit.

Kubernetes is an option, but should not be introduced solely for architectural fashion.

A managed container service is acceptable for early production if it reduces operational complexity.

---

# 70. Scaling Strategy

Scale independently:

```text
API
Workers
Processing Workers
Report Workers
Webhook Workers
```

Example:

```text
Normal load
API: 2 instances
Worker: 2 instances

Large ingestion
API: 2 instances
Processing Worker: 10 instances
```

This is one of the major advantages of asynchronous processing.

---

# 71. Processing Priority

The queue should eventually support priority.

Example:

```text
Priority 1
Interactive user result

Priority 2
Normal processing

Priority 3
Batch processing

Priority 4
Research/export jobs
```

Do not over-engineer this in the first implementation, but keep the job model extensible.

---

# 72. Concurrency

A processing job must have clear ownership.

Avoid two workers processing the same session simultaneously.

Use:

```text
Queue visibility timeout
+
Job state transition
+
Idempotent processing
```

and, where necessary:

```text
Distributed lock
```

---

# 73. Failure Handling

Every external or asynchronous operation should define:

```text
Retryable
Non-Retryable
Permanent Failure
```

Examples:

```text
Network timeout
→ Retry

Temporary object-storage error
→ Retry

Invalid raw data
→ Permanent failure

Unsupported protocol version
→ Permanent failure
```

---

# 74. Dead Letter Queue

Failed jobs exceeding retry limits should enter:

```text
Dead Letter Queue
```

Engineering should be able to:

```text
Inspect
Correct
Retry
Discard
```

through an administrative mechanism.

---

# 75. Frontend / Backend Boundary

The production architecture should be:

```text
Web UI
   ↓
KWinHub API
   ↓
Application Services
   ↓
Domain
   ↓
Infrastructure
```

Not:

```text
Web UI
   ↓
PostgreSQL
```

The existing prototype's service abstraction should evolve into this architecture.

---

# 76. Backend API vs KWinScience

A critical rule:

```text
Frontend
   ↓
KWinHub API
   ↓
Processing Service
   ↓
KWinScience
```

Frontend should never directly invoke scientific engines.

---

# 77. Security Boundary

The main security boundary is:

```text
Internet
   ↓
API Gateway
   ↓
Authenticated API
   ↓
Organization Authorization
   ↓
Application Services
   ↓
Data
```

KWinScience should not be publicly exposed.

---

# 78. Internal Event Model

The architecture should support internal domain events.

Examples:

```text
SessionCreated
DataUploaded
SessionValidated
ProcessingQueued
ProcessingCompleted
ResultCreated
ReportGenerated
IntegrationFailed
```

These events can eventually drive:

```text
Notifications
Webhooks
Analytics
Audit
Background Jobs
```

The PRD also identifies event-driven architecture as a future direction.

---

# 79. Outbox Pattern

For important domain events, consider the transactional outbox pattern.

Example:

```text
Database Transaction
 ├── Update Session
 └── Insert Outbox Event

Background Publisher
       ↓
Message Queue
```

This prevents:

```text
Database updated
but
event lost
```

The outbox should become important as integrations and webhooks increase.

---

# 80. Data Consistency

Use strong transactional consistency for:

```text
Organization
User
Subject
Session metadata
Protocol
Result metadata
```

Use eventual consistency for:

```text
Dashboards
Search indexes
Notifications
Webhooks
Aggregated analytics
```

---

# 81. V1 Domain Boundaries

The minimum stable domain should be:

```text
Organization
Subject
Protocol
Measurement Session
Trial
Data Source
Device
Dataset
Data Object
Processing Job
Result
Metric
Report
```

This is the core platform.

---

# 82. V1 Backend Priority

## P0 — Architecture

- tenant model;
- canonical domain model;
- API architecture;
- authentication;
- authorization;
- database;
- object storage;
- processing lifecycle.

## P0 — Data Pipeline

- session submission;
- upload;
- validation;
- idempotency;
- processing queue;
- worker;
- result generation.

## P0 — Core API

- Subjects;
- Protocols;
- Sessions;
- Results;
- Data Sources;
- Sync.

## P1 — Platform

- Reports;
- Integrations;
- Webhooks;
- Imports;
- Audit;
- advanced administration.

## P1 — Performance

- Trends;
- Comparisons;
- longitudinal queries;
- advanced visualization data APIs.

## P2 — Intelligence

- AI;
- advanced biomechanics;
- normative datasets;
- automated interpretation.

The priority is consistent with the PRD's V1 priorities.

---

# 83. V1 Development Sequence

## Phase 1 — Foundation

```text
Repository
CI/CD
Cloud environment
PostgreSQL
Object Storage
Authentication
Secrets
Logging
Observability
```

## Phase 2 — Domain

```text
Organization
User
Role
Subject
Team
Device
Data Source
Protocol
Session
Trial
Dataset
Data Object
Result
```

## Phase 3 — API

```text
Authentication
Subject API
Protocol API
Session API
Result API
Data API
```

## Phase 4 — Ingestion

```text
Session creation
Upload authorization
Object storage
Idempotency
Validation
Processing jobs
```

## Phase 5 — Processing

```text
Queue
Worker
KWinScience Contract
Algorithm Version
Result generation
Data quality
Failure handling
```

## Phase 6 — Frontend Integration

```text
Web UI
    ↓
Real API
    ↓
Real Database
```

Replace prototype/demo data with real services.

## Phase 7 — Client Integration

```text
KWinPerformance
      ↓
KWinHub
```

Then extend to:

```text
KWinSprint
Future Applications
Partner Systems
```

---

# 84. Testing Strategy

Backend testing should include:

## Unit Tests

- domain rules;
- authorization rules;
- protocol validation;
- state transitions;
- metric/result mapping.

## Integration Tests

- PostgreSQL;
- object storage;
- queue;
- authentication;
- API.

## Contract Tests

Ensure:

```text
Client ↔ KWinHub
KWinHub ↔ KWinScience
KWinHub ↔ Partner
```

remain compatible.

## End-to-End Tests

Example:

```text
Create Subject
      ↓
Create Session
      ↓
Upload Data
      ↓
Process
      ↓
Generate Result
      ↓
Retrieve Result
      ↓
Generate Report
```

---

# 85. Development Environment

Every developer should be able to start a local KWinHub environment with:

```text
API
PostgreSQL
Redis
Message Queue
Object Storage
Worker
Mock KWinScience
```

Docker Compose is appropriate for local development.

Example:

```text
docker compose up
```

should provide the basic development environment.

---

# 86. Mock KWinScience

Before the real scientific engines are available, engineering should have a mock processing implementation.

Example:

```text
Raw Dataset
    ↓
Mock KWinScience
    ↓
Synthetic Events
    ↓
Synthetic Metrics
    ↓
Result
```

This allows backend/frontend development to proceed independently.

---

# 87. Definition of Done — Backend V1

KWinHub V1 backend is architecturally successful when:

## Tenant

- organizations exist;
- organization isolation is enforced;
- users and roles exist.

## Data

- canonical Subject exists;
- Measurement Session exists;
- Protocol exists;
- Trial exists;
- Dataset exists;
- raw data can be preserved;
- Data Source exists;
- Device exists.

## Processing

- sessions can be received;
- files can be uploaded;
- validation occurs;
- processing is asynchronous;
- processing state is visible;
- failures are recoverable;
- results are generated.

## API

- authenticated API works;
- organization authorization works;
- idempotency works;
- asynchronous status works;
- results can be retrieved;
- OpenAPI documentation exists.

## Science

- KWinScience has a defined processing contract;
- protocol version is recorded;
- algorithm version is recorded;
- historical results are reproducible.

## Platform

- object storage works;
- queue works;
- workers work;
- monitoring exists;
- logs are traceable;
- backups exist;
- CI/CD exists.

---

# 88. Explicit Non-Goals for V1

The backend team should not attempt to build all of the following in the first release:

```text
Full microservice architecture
Advanced AI
Complete normative database
Global multi-region active-active deployment
Complex billing system
Full enterprise IAM ecosystem
Real-time global analytics platform
Every possible sports protocol
Every possible device integration
```

These are future capabilities.

V1 should establish a strong platform foundation.

---

# 89. Architectural Anti-Patterns

The following are explicitly prohibited as the long-term architecture.

## Anti-pattern 1

```text
CMJ database
Sprint database
Jump database
```

Use a shared domain model.

## Anti-pattern 2

```text
CMJService
SprintService
HighJumpService
```

Use protocols and processing modules.

## Anti-pattern 3

```text
Frontend → Database
```

Use the KWinHub API.

## Anti-pattern 4

```text
API → synchronous scientific processing
```

Use asynchronous workers.

## Anti-pattern 5

```text
Upload large video through API server
```

Use signed object-storage uploads.

## Anti-pattern 6

```text
Raw data → calculate → delete
```

Preserve raw data.

## Anti-pattern 7

```text
Algorithm update
    ↓
rewrite historical results
```

Preserve algorithm version and historical result provenance.

## Anti-pattern 8

```text
New sport
    ↓
new application + new database
```

Use:

```text
New Protocol
+
New Processing Module
+
Existing Platform
```

---

# 90. Long-Term Architecture

The intended long-term platform is:

```text
                         KWinHub SaaS
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
    Protocols            Data Platform          Analysis
        │                     │                     │
        │          ┌──────────┼──────────┐          │
        │          │          │          │          │
        │       Subjects   Sessions   Datasets      │
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
                         KWinHub API
                              │
          ┌───────────────────┼────────────────────┐
          │                   │                    │
  Kunwei Applications    Future Applications   Partner Systems
          │                   │                    │
          └───────────────────┼────────────────────┘
                              │
                         Data Ingestion
                              │
                          Validation
                              │
                          Processing
                              │
                         KWinScience
                              │
                       Metrics / Events
                              │
                     Results / Analysis
                              │
                ┌─────────────┼─────────────┐
                ↓             ↓             ↓
            Dashboard      Reports       Research
```

The PRD's corresponding conceptual split is:

```text
KWinCore
────────
Subjects
Organizations
Sessions
Datasets
Devices
Data Sources
Storage
APIs
Permissions

KWinScience
────────────
Signal Processing
Event Detection
Biomechanics
Metric Calculation
Performance Algorithms
Norms
Analysis
```



---

# 91. Future Product Expansion

The desired expansion mechanism is:

```text
New Hardware
      ↓
New Client Application
      ↓
Existing KWinHub API
      ↓
Existing Data Model
      ↓
New Protocol
      ↓
New Processing Module
      ↓
New Metrics
      ↓
Existing Dashboard / Reports
```

This allows future support for:

```text
Track & Field
Swimming
Football
Basketball
Volleyball
Rehabilitation
Strength & Conditioning
Research
```

without creating separate database architectures.

---

# 92. Architecture Decision Summary

The following decisions are the baseline for KWinHub V1:

| Decision | V1 Choice |
|---|---|
| Product model | B2B organization-centric SaaS |
| Deployment | Cloud |
| International | Designed for international use |
| Initial region | Single region |
| Future regions | Multi-region ready |
| Backend | .NET / ASP.NET Core |
| Database | PostgreSQL |
| Large data | Object Storage |
| Cache | Redis |
| Messaging | Managed Queue/Event system |
| API | REST + OpenAPI |
| Architecture | Modular Monolith |
| Async processing | Dedicated Workers |
| Scientific engine | KWinScience boundary |
| Tenancy | Shared infrastructure + organization isolation |
| Auth | OAuth/OIDC-compatible abstraction |
| Authorization | RBAC + organization scope |
| Raw data | Preserved |
| Upload | Signed object-storage upload |
| Processing | Asynchronous |
| Idempotency | Required |
| Webhooks | Signed + retryable |
| Observability | OpenTelemetry-oriented |
| IaC | Terraform |
| Containers | Docker |
| CI/CD | GitHub Actions or equivalent |
| Microservices | Future, not V1 default |

---

# 93. Final Engineering Principle

The most important architectural rule is:

```text
                  CLIENT APPLICATIONS
                  ───────────────────
                  Capture
                  Control
                  Synchronize
                  Offline
                         │
                         ▼
                      KWinHub
                  ───────────────────
                  Receive
                  Validate
                  Store
                  Process
                  Analyze
                  Report
                  Integrate
                         │
                         ▼
                    KWinScience
                  ───────────────────
                  Scientific computation
```

The system should therefore be designed so that:

> **A new measurement device does not require a new platform.**

> **A new sport does not require a new database.**

> **A new algorithm does not require a new application.**

> **A new client application does not require a new backend.**

> **A new customer does not require a new deployment architecture.**

That is the architectural definition of KWinHub as a SaaS platform.

---

# 94. Immediate Engineering Deliverables

Before substantial backend coding begins, the team should produce these artifacts:

### D1 — Architecture Decision Record

Document:

- cloud provider;
- region;
- database;
- object storage;
- queue;
- authentication provider;
- deployment platform.

### D2 — Domain Model / ERD

Define:

```text
Organization
User
Membership
Subject
Team
Protocol
Session
Trial
Dataset
DataObject
DataSource
Device
ProcessingJob
Result
Metric
Report
Integration
SyncJob
AuditLog
```

### D3 — OpenAPI V1

Define the initial public API.

### D4 — Database Migration V1

Create the initial PostgreSQL schema.

### D5 — Object Storage Specification

Define:

- bucket strategy;
- object naming;
- metadata;
- checksum;
- signed URLs;
- lifecycle.

### D6 — Processing Contract

Define:

```text
KWinHub → KWinScience
KWinScience → KWinHub
```

including:

- input;
- output;
- errors;
- algorithm version;
- protocol version.

### D7 — Processing State Machine

Implement and document:

```text
RECEIVED
→ VALIDATING
→ VALIDATED
→ QUEUED
→ PROCESSING
→ ANALYZING
→ PUBLISHED
```

plus failure and retry paths.

### D8 — Authentication / Authorization Design

Define:

- users;
- applications;
- roles;
- organization membership;
- tokens;
- API keys;
- permissions.

### D9 — Deployment Architecture

Define:

```text
Cloud
Network
API
Workers
Database
Storage
Queue
Redis
Monitoring
Secrets
```

### D10 — Local Development Environment

One command should start:

```text
KWinHub API
PostgreSQL
Redis
Queue
Object Storage
Worker
Mock KWinScience
```

---

# 95. Relationship to the Existing KWinHub V1 Prototype

The current React prototype should now be treated as:

> **UI / UX prototype and frontend reference implementation**

rather than the production architecture.

The next development transition should be:

```text
Current

React
  ↓
Demo Data
  ↓
Service Abstraction


Target

React
  ↓
KWinHub API
  ↓
Application Services
  ↓
PostgreSQL / Object Storage
  ↓
Processing Workers
  ↓
KWinScience
```

The existing frontend service abstractions are therefore useful and should be retained conceptually, but their implementations should eventually call the real backend API.

---

# 96. Architecture Success Criteria

The architecture should be considered successful if the following scenario works without introducing a sport-specific backend:

```text
A new Kunwei device is released
        ↓
Client application receives data
        ↓
Client submits Measurement Session
        ↓
KWinHub identifies Protocol
        ↓
Raw data is stored
        ↓
Processing Job is created
        ↓
KWinScience processes data
        ↓
Metrics are generated
        ↓
Result is stored
        ↓
Subject Performance is updated
        ↓
Dashboard displays result
        ↓
Trend includes result
        ↓
Report includes result
        ↓
Partner API can retrieve result
```

Then repeat the same flow for another sport.

No new database architecture should be required.

No new Hub application should be required.

Only the appropriate:

```text
Protocol
Processing Module
Metrics
Visualization
```

should need to be introduced.

---

# 97. Final Architecture Statement

KWinHub V1 is not simply a backend for the current KWinHub web application.

It is the first production implementation of Kunwei's future **centralized sports-performance data platform**.

The long-term architectural objective is:

```text
                 MANY CLIENTS
                     │
         ┌───────────┼───────────┐
         │           │           │
       Kunwei      Kunwei      Partner
       Desktop      Mobile      Systems
         │           │           │
         └───────────┼───────────┘
                     │
                     ▼
                KWinHub API
                     │
       ┌─────────────┼─────────────┐
       │             │             │
      Data        Processing     Platform
       │             │             │
       └─────────────┼─────────────┘
                     │
                KWinScience
                     │
                     ▼
              Standardized Data
                     │
       ┌─────────────┼─────────────┐
       ↓             ↓             ↓
   Performance    Analysis      Research
       │             │             │
       └─────────────┼─────────────┘
                     ↓
             Reports / Decisions
```

**The backend team's job in V1 is therefore to build the reusable platform foundation, not to reproduce the logic of today's individual testing applications.**