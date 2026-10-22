# KWinHub V1 — PostgreSQL Domain Model & ERD Specification

**Product:** KWinHub / 坤维智库  
**Version:** V1.0  
**Status:** Engineering Baseline  
**Database:** PostgreSQL  
**Purpose:** Define the transactional domain model and initial relational schema for KWinHub V1.

---

# 1. Database Design Principles

The PostgreSQL database is the **system of record for KWinHub metadata and transactional state**.

It stores:

- organizations;
- users and memberships;
- subjects;
- teams;
- protocols;
- sessions;
- trials;
- datasets;
- data objects;
- data sources;
- devices;
- processing jobs;
- results;
- metrics;
- reports;
- integrations;
- sync jobs;
- audit records.

It does **not** store large binary measurement/video files directly.

Large files are stored in object storage and referenced through `data_objects`.

The most important database rule is:

> **The database models reusable measurement entities, not individual sports applications.**

---

# 2. Logical ERD

The primary relationship graph is:

```text
Organization
│
├── OrganizationMembership ── User
│
├── Team
│     └── TeamSubject ── Subject
│
├── DataSource
│     └── Device
│
├── Protocol
│     └── ProtocolVersion
│
├── MeasurementSession
│     ├── Trial
│     ├── Dataset
│     │     └── DataObject
│     ├── ProcessingJob
│     └── Result
│           └── Metric
│
├── Report
├── Integration
├── SyncJob
└── AuditLog
```

The central measurement relationship is:

```text
Subject
   │
   │ 1:N
   ▼
MeasurementSession
   │
   ├──────────────► ProtocolVersion
   │
   ├──────────────► DataSource
   │
   ├──────────────► Device
   │
   ├── 1:N ───────► Trial
   │
   ├── 1:N ───────► Dataset
   │                    │
   │                    └── 1:N ──► DataObject
   │
   ├── 1:N ───────► ProcessingJob
   │
   └── 1:N ───────► Result
                         │
                         └── 1:N ──► Metric
```

---

# 3. Database Schema Organization

Use PostgreSQL schemas only where they provide a real boundary.

For V1, a single application schema is sufficient:

```text
kwinhub
```

Recommended logical grouping within that schema:

```text
Identity
Organization
Subject
Measurement
Data
Processing
Results
Platform
Audit
```

Do not create separate databases for CMJ, Sprint, Jump, etc.

---

# 4. Common Database Conventions

## 4.1 Primary Keys

Use UUID identifiers.

Example:

```sql
id uuid primary key
```

Recommended application-level ID generation rather than database-dependent sequential IDs.

---

## 4.2 Timestamps

All persisted timestamps are UTC.

Recommended:

```sql
created_at timestamptz not null
updated_at timestamptz not null
```

Do not use `timestamp without time zone` for event timestamps.

---

## 4.3 Soft Delete

Do not add `deleted_at` mechanically to every table.

Use soft deletion only where historical references or audit requirements justify it.

For example:

```text
Subject
Protocol
Device
Integration
```

may need logical deactivation.

Measurement and processing records should generally be immutable rather than deleted.

---

# 5. Organizations

## organizations

```text
organizations
------------------------------
id                  UUID PK
name                VARCHAR(200)
slug                VARCHAR(100)
status              VARCHAR(32)
region              VARCHAR(32)
timezone            VARCHAR(64)
locale              VARCHAR(16)
metadata            JSONB
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Constraints:

```text
UNIQUE(slug)
```

Potential future:

```text
billing_plan
data_region
retention_policy
```

These should not be required for the first database migration unless product requirements demand them.

---

# 6. Users

## users

```text
users
------------------------------
id                  UUID PK
identity_provider_id VARCHAR(200)
email               VARCHAR(320)
display_name        VARCHAR(200)
status              VARCHAR(32)
metadata            JSONB
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

The identity provider remains external.

The database stores the application identity mapping.

---

# 7. Organization Membership

## organization_memberships

```text
organization_memberships
------------------------------
id                  UUID PK
organization_id     UUID FK
user_id             UUID FK
role_id             UUID FK
status              VARCHAR(32)
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Constraints:

```text
UNIQUE(organization_id, user_id)
```

This allows:

```text
User A
 ├── Organization A
 └── Organization B
```

---

# 8. Roles

## roles

```text
roles
------------------------------
id                  UUID PK
organization_id     UUID NULL
code                VARCHAR(64)
name                VARCHAR(100)
description         TEXT
is_system           BOOLEAN
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

System roles may include:

```text
OWNER
ADMIN
MANAGER
PRACTITIONER
RESEARCHER
VIEWER
APPLICATION
```

The permission model can be extended later.

---

# 9. Teams / Groups

## teams

```text
teams
------------------------------
id                  UUID PK
organization_id     UUID FK
name                VARCHAR(200)
code                VARCHAR(100)
description         TEXT
status              VARCHAR(32)
metadata            JSONB
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

## team_subjects

```text
team_subjects
------------------------------
team_id             UUID FK
subject_id          UUID FK
created_at          TIMESTAMPTZ
```

Primary key:

```text
(team_id, subject_id)
```

This supports:

```text
Subject
 ├── Team A
 ├── Team B
 └── Rehabilitation Group
```

without duplicating the Subject.

---

# 10. Subjects

## subjects

```text
subjects
------------------------------
id                  UUID PK
organization_id     UUID FK
external_id         VARCHAR(200)
subject_type        VARCHAR(32)
first_name          VARCHAR(100)
last_name           VARCHAR(100)
display_name        VARCHAR(200)
date_of_birth       DATE NULL
sex                 VARCHAR(32) NULL
status              VARCHAR(32)
metadata            JSONB
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Supported V1 subject types:

```text
ATHLETE
PATIENT
PARTICIPANT
RESEARCH_SUBJECT
OTHER
```

Recommended indexes:

```text
(organization_id, display_name)
(organization_id, external_id)
(organization_id, subject_type)
```

---

# 11. Data Sources

## data_sources

```text
data_sources
------------------------------
id                  UUID PK
organization_id     UUID FK
name                VARCHAR(200)
type                VARCHAR(64)
provider            VARCHAR(100)
version             VARCHAR(100)
status              VARCHAR(32)
configuration       JSONB
last_sync_at        TIMESTAMPTZ NULL
error_count         INTEGER
metadata            JSONB
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Examples:

```text
KWinPerformance
KWinSprint
Partner API
CSV Import
Research Import
```

Sensitive configuration must not store plaintext credentials.

---

# 12. Devices

## devices

```text
devices
------------------------------
id                  UUID PK
organization_id     UUID FK
data_source_id      UUID FK NULL
device_type         VARCHAR(64)
manufacturer        VARCHAR(100)
model               VARCHAR(100)
serial_number       VARCHAR(200)
firmware_version    VARCHAR(100)
status              VARCHAR(32)
last_seen_at        TIMESTAMPTZ NULL
metadata            JSONB
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Recommended unique constraint:

```text
UNIQUE(organization_id, serial_number)
```

Device identity is independent from sessions.

---

# 13. Protocols

A protocol is the reusable definition of a measurement workflow.

## protocols

```text
protocols
------------------------------
id                  UUID PK
organization_id     UUID FK
code                VARCHAR(100)
name                VARCHAR(200)
category            VARCHAR(100)
sport               VARCHAR(100) NULL
description         TEXT
status              VARCHAR(32)
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Protocol identity should remain stable across versions.

---

# 14. Protocol Versions

Protocol configuration must be immutable once used by a measurement session.

## protocol_versions

```text
protocol_versions
------------------------------
id                  UUID PK
protocol_id         UUID FK
version             VARCHAR(50)
status              VARCHAR(32)

input_schema        JSONB
trial_configuration JSONB
validation_rules    JSONB
processing_definition JSONB
metric_definition   JSONB
visualization_definition JSONB
comparison_definition JSONB
report_definition   JSONB

created_at          TIMESTAMPTZ
published_at        TIMESTAMPTZ NULL
```

Constraint:

```text
UNIQUE(protocol_id, version)
```

A session references `protocol_version_id`, not merely `protocol_id`.

---

# 15. Measurement Sessions

## measurement_sessions

```text
measurement_sessions
------------------------------
id                  UUID PK
organization_id     UUID FK
subject_id          UUID FK
protocol_id         UUID FK
protocol_version_id UUID FK

source_id           UUID FK NULL
device_id           UUID FK NULL

external_id         VARCHAR(200) NULL
status              VARCHAR(32)

started_at          TIMESTAMPTZ NULL
completed_at        TIMESTAMPTZ NULL

timezone            VARCHAR(64)

client_application  VARCHAR(100) NULL
client_version      VARCHAR(100) NULL

metadata            JSONB

created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Recommended indexes:

```text
(organization_id, created_at DESC)
(organization_id, subject_id, started_at DESC)
(organization_id, protocol_id, started_at DESC)
(organization_id, status)
(organization_id, source_id)
```

---

# 16. Session External Identity

For client synchronization:

```text
organization_id
source_id
external_id
```

should support uniqueness.

Recommended:

```text
UNIQUE(
    organization_id,
    source_id,
    external_id
)
```

This is important for synchronization and idempotency.

---

# 17. Trials

## trials

```text
trials
------------------------------
id                  UUID PK
session_id          UUID FK
trial_index         INTEGER
status              VARCHAR(32)
started_at          TIMESTAMPTZ NULL
duration_ms         BIGINT NULL
quality_status      VARCHAR(32)
metadata            JSONB
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Constraint:

```text
UNIQUE(session_id, trial_index)
```

---

# 18. Datasets

## datasets

```text
datasets
------------------------------
id                  UUID PK
organization_id     UUID FK
session_id          UUID FK NULL
trial_id            UUID FK NULL

type                VARCHAR(32)
name                VARCHAR(200)
version             VARCHAR(50)
status              VARCHAR(32)

metadata            JSONB

created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Dataset types:

```text
RAW
PROCESSED
ANALYSIS
RESEARCH
EXPORT
```

---

# 19. Data Objects

## data_objects

```text
data_objects
------------------------------
id                  UUID PK
organization_id     UUID FK
dataset_id          UUID FK
session_id          UUID FK NULL
trial_id            UUID FK NULL

type                VARCHAR(64)
format              VARCHAR(32)

storage_provider    VARCHAR(32)
storage_bucket      VARCHAR(200)
storage_key         TEXT

size_bytes          BIGINT
checksum_algorithm  VARCHAR(32)
checksum            VARCHAR(200)

sample_rate         DECIMAL NULL
duration_ms         BIGINT NULL

metadata            JSONB

created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
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

The database contains the metadata and object reference.

The actual binary content lives in object storage.

---

# 20. Processing Jobs

## processing_jobs

```text
processing_jobs
------------------------------
id                  UUID PK
organization_id     UUID FK
session_id          UUID FK

job_type            VARCHAR(32)
status              VARCHAR(32)
priority            INTEGER

attempt             INTEGER
max_attempts        INTEGER

protocol_version    VARCHAR(50)
algorithm_version   VARCHAR(100)

started_at          TIMESTAMPTZ NULL
completed_at        TIMESTAMPTZ NULL

error_code          VARCHAR(100) NULL
error_message       TEXT NULL

created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Job types:

```text
VALIDATION
PROCESSING
ANALYSIS
REPORT_GENERATION
DATA_EXPORT
IMPORT
```

---

# 21. Processing Attempts

For operational diagnostics:

## processing_attempts

```text
processing_attempts
------------------------------
id                  UUID PK
processing_job_id   UUID FK
attempt_number      INTEGER
worker_id           VARCHAR(200)
status              VARCHAR(32)
started_at          TIMESTAMPTZ
completed_at        TIMESTAMPTZ NULL
error_code          VARCHAR(100) NULL
error_message       TEXT NULL
metadata            JSONB
created_at          TIMESTAMPTZ
```

This allows engineering to distinguish:

```text
Job failed once
```

from:

```text
Job failed after five retries
```

---

# 22. Results

A Result represents a scientifically processed output for a session/trial.

## results

```text
results
------------------------------
id                  UUID PK
organization_id     UUID FK
session_id          UUID FK
trial_id            UUID FK NULL

protocol_id         UUID FK
protocol_version_id UUID FK

algorithm_version   VARCHAR(100)

status              VARCHAR(32)
quality_status      VARCHAR(32)

processing_job_id   UUID FK NULL

created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Results should be append-oriented.

A new algorithm version should create a new result rather than silently overwrite the historical result.

---

# 23. Metrics

## metrics

```text
metrics
------------------------------
id                  UUID PK
result_id           UUID FK

metric_code         VARCHAR(100)
metric_name         VARCHAR(200)

value_numeric       DOUBLE PRECISION NULL
value_text          TEXT NULL

unit                VARCHAR(50)

side                VARCHAR(32) NULL
context             JSONB NULL
metadata            JSONB NULL

created_at          TIMESTAMPTZ
```

Examples:

```text
peak_force
jump_height
rsi
impulse
sprint_time
velocity
asymmetry
range_of_motion
```

The metric schema intentionally does not contain sport-specific columns.

---

# 24. Analysis Records

V1 can initially derive most analysis from Results + Metrics.

If persisted analysis becomes necessary:

## analysis_records

```text
analysis_records
------------------------------
id                  UUID PK
organization_id     UUID FK
subject_id          UUID FK
analysis_type       VARCHAR(64)
version             VARCHAR(50)
status              VARCHAR(32)
definition          JSONB
result_data         JSONB
created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

This should **not** be introduced until a concrete analysis persistence requirement exists.

---

# 25. Reports

## reports

```text
reports
------------------------------
id                  UUID PK
organization_id     UUID FK

subject_id          UUID FK NULL
session_id          UUID FK NULL

name                VARCHAR(200)
report_type         VARCHAR(64)
status              VARCHAR(32)

definition          JSONB

storage_bucket      VARCHAR(200) NULL
storage_key         TEXT NULL

created_by          UUID FK NULL

created_at          TIMESTAMPTZ
completed_at        TIMESTAMPTZ NULL
```

Report generation should be asynchronous.

---

# 26. Integrations

## integrations

```text
integrations
------------------------------
id                  UUID PK
organization_id     UUID FK

name                VARCHAR(200)
provider            VARCHAR(100)
integration_type    VARCHAR(32)

status              VARCHAR(32)
scopes              JSONB

configuration       JSONB

last_request_at     TIMESTAMPTZ NULL
error_count         INTEGER

created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Credential secrets should be stored in a secret-management system, not directly in PostgreSQL.

---

# 27. Sync Jobs

## sync_jobs

```text
sync_jobs
------------------------------
id                  UUID PK
organization_id     UUID FK
source_id           UUID FK

session_id          UUID FK NULL

status              VARCHAR(32)
attempt             INTEGER

started_at          TIMESTAMPTZ NULL
completed_at        TIMESTAMPTZ NULL

error_code          VARCHAR(100) NULL
error_message       TEXT NULL

metadata            JSONB

created_at          TIMESTAMPTZ
updated_at          TIMESTAMPTZ
```

Statuses:

```text
QUEUED
RECEIVED
PROCESSING
COMPLETED
FAILED
```

---

# 28. Audit Logs

## audit_logs

```text
audit_logs
------------------------------
id                  UUID PK
organization_id     UUID FK NULL
actor_id            UUID FK NULL

action              VARCHAR(100)
resource_type       VARCHAR(100)
resource_id         UUID NULL

timestamp           TIMESTAMPTZ

ip_address          INET NULL
user_agent          TEXT NULL

metadata            JSONB
```

Audit logs should be append-only.

---

# 29. Outbox

The architecture should reserve an outbox table for reliable event publication.

## outbox_messages

```text
outbox_messages
------------------------------
id                  UUID PK
organization_id     UUID NULL

event_type          VARCHAR(200)
aggregate_type      VARCHAR(100)
aggregate_id        UUID

payload             JSONB

status              VARCHAR(32)
attempts            INTEGER

occurred_at         TIMESTAMPTZ
published_at        TIMESTAMPTZ NULL

created_at          TIMESTAMPTZ
```

Example:

```text
SessionCreated
SessionValidated
ProcessingQueued
ProcessingCompleted
ResultPublished
ReportGenerated
```

---

# 30. Tenant Isolation

Every tenant-owned table must contain:

```text
organization_id
```

where appropriate.

Application queries must always apply organization scope.

Example:

```sql
SELECT *
FROM measurement_sessions
WHERE organization_id = @organizationId
  AND id = @sessionId;
```

Never:

```sql
SELECT *
FROM measurement_sessions
WHERE id = @sessionId;
```

unless the authorization layer has already guaranteed tenant ownership through a trusted repository boundary.

---

# 31. Recommended Index Strategy

Initial high-value indexes:

```text
subjects:
  organization_id + display_name
  organization_id + external_id

sessions:
  organization_id + created_at
  organization_id + subject_id + started_at
  organization_id + protocol_id + started_at
  organization_id + status
  organization_id + source_id

trials:
  session_id + trial_index

datasets:
  organization_id + session_id

data_objects:
  organization_id + session_id
  dataset_id

processing_jobs:
  organization_id + status
  session_id
  status + priority

results:
  organization_id + session_id
  organization_id + created_at

metrics:
  result_id
  metric_code

audit_logs:
  organization_id + timestamp
  resource_type + resource_id
```

Indexes should be validated using real query plans after V1 traffic exists.

---

# 32. Foreign Key Strategy

Important foreign keys should be enforced at database level.

Examples:

```text
session.subject_id
→ subjects.id

session.protocol_version_id
→ protocol_versions.id

trial.session_id
→ measurement_sessions.id

dataset.session_id
→ measurement_sessions.id

data_object.dataset_id
→ datasets.id

processing_job.session_id
→ measurement_sessions.id

result.session_id
→ measurement_sessions.id

metric.result_id
→ results.id
```

Avoid `ON DELETE CASCADE` on historical measurement data unless explicitly reviewed.

For scientific data, accidental cascading deletion is dangerous.

---

# 33. Recommended State Enumerations

Use application/domain enums mapped to PostgreSQL strings rather than database enums initially.

This keeps deployment and backward compatibility easier.

Example:

```text
SessionStatus:
RECEIVED
VALIDATING
VALIDATED
QUEUED
PROCESSING
ANALYZING
PUBLISHED
VALIDATION_FAILED
PROCESSING_FAILED
ANALYSIS_FAILED
```

Similarly:

```text
ProcessingJobStatus:
QUEUED
RUNNING
SUCCEEDED
FAILED
CANCELLED
```

---

# 34. Transaction Boundaries

Create Session transaction:

```text
MeasurementSession
+
initial Trial records
+
optional SyncJob
+
Outbox event
```

Processing completion transaction:

```text
ProcessingJob update
+
Result
+
Metric records
+
Session state update
+
Outbox event
```

The actual scientific computation occurs outside the database transaction.

---

# 35. Reproducibility Model

Every published Result must identify:

```text
session
protocol_version
algorithm_version
input_dataset
processing_job
```

Conceptually:

```text
Result
 │
 ├── Session
 ├── Protocol Version
 ├── Algorithm Version
 ├── Input Dataset
 └── Processing Job
```

This is mandatory for scientific reproducibility.

---

# 36. ERD Summary

The minimum V1 production ERD is:

```text
organizations
 ├── organization_memberships ── users
 ├── roles
 ├── teams ── team_subjects ── subjects
 ├── data_sources ── devices
 ├── protocols ── protocol_versions
 │
 ├── measurement_sessions
 │      ├── trials
 │      ├── datasets
 │      │      └── data_objects
 │      ├── processing_jobs
 │      │      └── processing_attempts
 │      └── results
 │             └── metrics
 │
 ├── reports
 ├── integrations
 ├── sync_jobs
 ├── audit_logs
 └── outbox_messages
```

This should be the starting point for the first EF Core migration.

---

# 37. V1 Schema Implementation Order

Implement migrations in this order:

```text
001 Identity / Organizations
002 Roles / Memberships
003 Subjects / Teams
004 Data Sources / Devices
005 Protocols / Protocol Versions
006 Measurement Sessions / Trials
007 Datasets / Data Objects
008 Processing Jobs
009 Results / Metrics
010 Reports
011 Integrations / Sync
012 Audit
013 Outbox
```

The migration history must be committed to source control.

---

# 38. ERD Acceptance Criteria

The ERD is acceptable when:

1. A user can belong to multiple organizations.
2. Every organization-owned resource can be tenant-scoped.
3. A Subject can have many Measurement Sessions.
4. A Session can have multiple Trials.
5. A Session references an immutable Protocol Version.
6. A Session can contain multiple Datasets.
7. Large data is represented by Data Objects.
8. Processing is represented independently from Sessions.
9. Results identify Protocol and Algorithm versions.
10. Metrics are generic rather than sport-specific.
11. Reports can reference Sessions and Subjects.
12. Integrations and Sync Jobs are independent platform objects.
13. Historical measurement data cannot be accidentally cascaded away.

---

# 39. Architectural Test

The schema must support this without adding new tables:

```text
New Sport
    ↓
New Protocol
    ↓
New Protocol Version
    ↓
New Processing Definition
    ↓
New Metrics
    ↓
Measurement Session
    ↓
Result
```

If a new sport requires:

```text
New SportSession table
New SportResult table
New SportMetric table
```

the domain model has failed its primary architectural objective.

---

# 40. Next Implementation Artifact

The next artifact to implement against this model is:

```text
KWinHub V1 OpenAPI
```

The API must expose this model without leaking EF Core/domain persistence objects directly to clients.