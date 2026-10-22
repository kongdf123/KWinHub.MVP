# KWinHub V1 — REST API & OpenAPI Specification

**Product:** KWinHub / 坤维智库  
**Version:** V1.0  
**Status:** Engineering Baseline  
**Base URL:** `https://api.<kwinhub-domain>/v1`

---

# 1. API Philosophy

KWinHub exposes a stable REST API to:

- Web UI;
- Kunwei desktop applications;
- Kunwei mobile applications;
- partner applications;
- future SDKs;
- integrations.

The API is the boundary between clients and the platform.

Clients must not depend on:

- PostgreSQL;
- EF Core entities;
- internal application services;
- worker implementation;
- KWinScience implementation.

---

# 2. API Resource Model

Primary resources:

```text
organizations
users
subjects
teams
protocols
protocol-versions
sessions
trials
datasets
data-objects
data-sources
devices
processing-jobs
results
reports
integrations
sync-jobs
webhooks
audit-logs
```

---

# 3. API Authentication

Browser/API requests:

```http
Authorization: Bearer <access-token>
```

Machine-to-machine integrations may use:

```text
OAuth 2.0 Client Credentials
```

API keys may be supported for selected partner integrations.

The API must never accept:

```text
organizationId
```

as proof of tenant authorization.

Organization scope comes from the authenticated principal.

---

# 4. Common Headers

Recommended request headers:

```http
Authorization: Bearer <token>
Content-Type: application/json
Accept: application/json
X-Correlation-Id: <uuid>
Idempotency-Key: <unique-key>
```

`Idempotency-Key` is required for operations where duplicate submission would be harmful.

---

# 5. Common Response Headers

```http
X-Correlation-Id: <uuid>
```

The correlation ID should be returned even when the server generates it.

---

# 6. Standard Error Format

All API errors use:

```json
{
  "code": "SESSION_NOT_FOUND",
  "message": "Measurement session was not found.",
  "traceId": "7f91d9...",
  "details": {}
}
```

Clients must branch on:

```text
code
```

rather than parsing `message`.

---

# 7. HTTP Status Conventions

```text
200 OK
201 Created
202 Accepted
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity
429 Too Many Requests
500 Internal Server Error
503 Service Unavailable
```

---

# 8. Pagination

List endpoints use cursor pagination.

Example:

```http
GET /v1/subjects?limit=50&cursor=eyJpZCI6...
```

Response:

```json
{
  "items": [],
  "nextCursor": "eyJpZCI6...",
  "hasMore": true
}
```

Cursor pagination is preferred over large offset pagination for long-running SaaS datasets.

---

# 9. Organizations

## GET /v1/organizations

Return organizations available to the authenticated user.

```json
{
  "items": [
    {
      "id": "org_...",
      "name": "Kunwei Sports Lab",
      "status": "ACTIVE",
      "timezone": "Asia/Shanghai",
      "locale": "zh-CN"
    }
  ]
}
```

---

## GET /v1/organizations/{organizationId}

Return organization metadata.

Authorization must verify membership.

---

# 10. Subjects

## GET /v1/subjects

Filters:

```text
search
subjectType
status
teamId
createdFrom
createdTo
```

Example:

```http
GET /v1/subjects?search=John&subjectType=ATHLETE
```

---

## POST /v1/subjects

```json
{
  "externalId": "ATH-001",
  "subjectType": "ATHLETE",
  "firstName": "John",
  "lastName": "Smith",
  "displayName": "John Smith",
  "dateOfBirth": "2000-05-20",
  "sex": "MALE"
}
```

Response:

```http
201 Created
```

---

## GET /v1/subjects/{subjectId}

Returns:

```text
identity
teams
recent sessions
recent results
key metrics
trends summary
```

The API should not return every historical session by default.

Use dedicated endpoints.

---

## PATCH /v1/subjects/{subjectId}

Supports partial updates.

Example:

```json
{
  "displayName": "John Smith",
  "status": "ACTIVE"
}
```

---

## DELETE /v1/subjects/{subjectId}

Prefer logical deactivation where historical data exists.

---

# 11. Teams

## GET /v1/teams

## POST /v1/teams

```json
{
  "name": "First Team",
  "code": "FIRST",
  "description": "Professional first team"
}
```

---

## POST /v1/teams/{teamId}/subjects/{subjectId}

Adds a Subject to a Team.

---

## DELETE /v1/teams/{teamId}/subjects/{subjectId}

Removes membership.

---

# 12. Protocols

## GET /v1/protocols

Filters:

```text
search
category
sport
status
```

---

## POST /v1/protocols

```json
{
  "code": "CMJ",
  "name": "Countermovement Jump",
  "category": "JUMP",
  "sport": "GENERAL",
  "description": "..."
}
```

---

## GET /v1/protocols/{protocolId}

Returns protocol metadata and available versions.

---

## POST /v1/protocols/{protocolId}/versions

```json
{
  "version": "2.0",
  "inputSchema": {},
  "trialConfiguration": {},
  "validationRules": {},
  "processingDefinition": {},
  "metricDefinition": {},
  "visualizationDefinition": {},
  "comparisonDefinition": {},
  "reportDefinition": {}
}
```

A published version becomes immutable.

---

## GET /v1/protocols/{protocolId}/versions/{version}

Returns the exact protocol definition.

---

# 13. Measurement Sessions

Measurement Sessions are the central historical API resource.

## GET /v1/sessions

Filters:

```text
search
subjectId
protocolId
sourceId
deviceId
status
startedFrom
startedTo
```

Example:

```http
GET /v1/sessions?subjectId=...&status=PUBLISHED
```

---

# 14. Create Measurement Session

## POST /v1/sessions

Request:

```json
{
  "subjectId": "sub_123",
  "protocolId": "pro_123",
  "protocolVersionId": "pv_123",
  "sourceId": "src_123",
  "deviceId": "dev_123",
  "externalId": "client-session-001",
  "startedAt": "2026-10-04T08:30:00Z",
  "clientApplication": "KWinPerformance",
  "clientVersion": "4.2.0",
  "metadata": {}
}
```

Response:

```http
201 Created
```

```json
{
  "id": "ses_123",
  "status": "RECEIVED"
}
```

---

# 15. Idempotent Session Creation

Client sends:

```http
Idempotency-Key: client-session-001
```

If the request is repeated, the server must return the existing logical result rather than creating another Session.

Additionally:

```text
organizationId
+
sourceId
+
externalId
```

should be checked where external identity is available.

---

# 16. Session Detail

## GET /v1/sessions/{sessionId}

Returns:

```text
metadata
protocol
source
device
trials
datasets
processing status
data quality
results summary
```

It should not automatically return raw binary data.

---

# 17. Session State

## GET /v1/sessions/{sessionId}/status

Example:

```json
{
  "sessionId": "ses_123",
  "status": "PROCESSING",
  "progress": 65,
  "currentStage": "ANALYZING",
  "processingJobId": "job_123",
  "updatedAt": "2026-10-04T08:35:00Z"
}
```

Progress is optional and should not be treated as exact unless the processing engine provides meaningful progress.

---

# 18. Session Trials

## GET /v1/sessions/{sessionId}/trials

---

## POST /v1/sessions/{sessionId}/trials

```json
{
  "trialIndex": 1,
  "startedAt": "2026-10-04T08:31:00Z",
  "durationMs": 1200,
  "metadata": {}
}
```

---

## GET /v1/sessions/{sessionId}/trials/{trialId}

Returns trial metadata, datasets, quality and result summary.

---

# 19. Data Upload

The API should use direct object-storage uploads.

## POST /v1/sessions/{sessionId}/uploads

Request:

```json
{
  "files": [
    {
      "name": "trial_001_force.bin",
      "contentType": "application/octet-stream",
      "sizeBytes": 10485760,
      "checksum": "..."
    }
  ]
}
```

Response:

```json
{
  "uploads": [
    {
      "dataObjectId": "obj_123",
      "uploadUrl": "signed-url",
      "expiresAt": "2026-10-04T08:45:00Z"
    }
  ]
}
```

The signed URL is temporary.

---

# 20. Upload Completion

## POST /v1/data-objects/{dataObjectId}/complete

```json
{
  "checksum": "...",
  "sizeBytes": 10485760
}
```

The backend verifies object existence and metadata.

Successful completion may enqueue validation.

---

# 21. Datasets

## GET /v1/sessions/{sessionId}/datasets

## GET /v1/datasets/{datasetId}

The Dataset API returns:

```text
dataset metadata
version
type
status
data objects
```

---

# 22. Data Objects

## GET /v1/data-objects/{dataObjectId}

Returns metadata only.

---

## POST /v1/data-objects/{dataObjectId}/download-url

Returns a temporary signed URL.

Do not expose permanent object-storage URLs.

---

# 23. Processing Jobs

## GET /v1/processing-jobs

Filters:

```text
sessionId
status
jobType
createdFrom
createdTo
```

---

## GET /v1/processing-jobs/{jobId}

Example:

```json
{
  "id": "job_123",
  "sessionId": "ses_123",
  "jobType": "PROCESSING",
  "status": "RUNNING",
  "attempt": 1,
  "algorithmVersion": "cmj-3.1.0",
  "startedAt": "2026-10-04T08:35:00Z"
}
```

---

## POST /v1/processing-jobs/{jobId}/retry

Authorization:

```text
ADMIN
MANAGER
APPLICATION
```

depending on deployment policy.

Only retry jobs that are eligible.

---

# 24. Results

## GET /v1/results

Filters:

```text
subjectId
sessionId
protocolId
metricCode
createdFrom
createdTo
```

---

## GET /v1/results/{resultId}

Example:

```json
{
  "id": "res_123",
  "sessionId": "ses_123",
  "trialId": "trial_001",
  "protocol": {
    "id": "pro_123",
    "version": "2.0"
  },
  "algorithmVersion": "cmj-3.1.0",
  "qualityStatus": "GOOD",
  "metrics": [
    {
      "code": "jump_height",
      "name": "Jump Height",
      "value": 42.5,
      "unit": "cm"
    },
    {
      "code": "peak_force",
      "name": "Peak Force",
      "value": 1820.4,
      "unit": "N"
    }
  ]
}
```

---

# 25. Metric Query

## GET /v1/subjects/{subjectId}/metrics

Parameters:

```text
metricCode
protocolId
from
to
aggregation
```

Example:

```http
GET /v1/subjects/sub_123/metrics?metricCode=jump_height&from=2026-01-01
```

This endpoint is the foundation for:

```text
Performance
Trends
Personal Best
Longitudinal Charts
```

---

# 26. Trends

## GET /v1/subjects/{subjectId}/trends

Example:

```http
GET /v1/subjects/sub_123/trends?metricCode=jump_height&from=2026-01-01&to=2026-10-04
```

Response:

```json
{
  "metric": {
    "code": "jump_height",
    "unit": "cm"
  },
  "points": [
    {
      "date": "2026-08-01",
      "value": 39.2
    },
    {
      "date": "2026-09-01",
      "value": 41.0
    },
    {
      "date": "2026-10-04",
      "value": 42.5
    }
  ]
}
```

---

# 27. Comparisons

## POST /v1/analysis/comparisons

Request:

```json
{
  "subjectId": "sub_123",
  "metricCodes": [
    "jump_height",
    "peak_force"
  ],
  "baseline": {
    "type": "PERSONAL_BEST"
  },
  "currentSessionId": "ses_123"
}
```

Response:

```json
{
  "comparisons": []
}
```

Comparison types:

```text
PREVIOUS
BASELINE
PERSONAL_BEST
TEAM
GROUP
```

---

# 28. Performance Summary

## GET /v1/subjects/{subjectId}/performance

The endpoint should provide aggregated information for the Subject profile:

```text
key metrics
personal bests
recent changes
selected trends
latest sessions
```

It should not contain sport-specific hard-coded fields.

---

# 29. Data Sources

## GET /v1/data-sources

## POST /v1/data-sources

Example:

```json
{
  "name": "KWinPerformance",
  "type": "KUNWEI_APPLICATION",
  "provider": "Kunwei",
  "version": "4.2.0"
}
```

---

# 30. Devices

## GET /v1/devices

Filters:

```text
deviceType
manufacturer
status
dataSourceId
```

---

## GET /v1/devices/{deviceId}

Returns:

```text
identity
firmware
status
lastSeen
data source
recent sessions
```

---

# 31. Heartbeat

## POST /v1/client/heartbeat

Request:

```json
{
  "application": "KWinPerformance",
  "applicationVersion": "4.2.0",
  "deviceIds": [
    "dev_123"
  ],
  "offlineQueueSize": 2,
  "lastSyncAt": "2026-10-04T08:20:00Z",
  "clientTimestamp": "2026-10-04T08:35:00Z"
}
```

Response:

```json
{
  "serverTime": "2026-10-04T08:35:01Z",
  "syncRequired": false
}
```

Heartbeat is for:

```text
health
connectivity
sync state
client status
```

It is not the measurement-data transport.

---

# 32. Sync

## GET /v1/sync/jobs

## GET /v1/sync/jobs/{jobId}

## POST /v1/sync/jobs/{jobId}/retry

---

# 33. Reports

## POST /v1/reports

Request:

```json
{
  "subjectId": "sub_123",
  "sessionIds": [
    "ses_123"
  ],
  "protocolIds": [
    "pro_123"
  ],
  "metricCodes": [
    "jump_height",
    "peak_force"
  ],
  "dateRange": {
    "from": "2026-01-01",
    "to": "2026-10-04"
  },
  "format": "PDF"
}
```

Response:

```http
202 Accepted
```

```json
{
  "reportId": "rep_123",
  "status": "QUEUED"
}
```

---

# 34. Report Status

## GET /v1/reports/{reportId}

Returns:

```json
{
  "id": "rep_123",
  "status": "COMPLETED",
  "downloadAvailable": true
}
```

---

# 35. Report Download

## POST /v1/reports/{reportId}/download-url

Returns a temporary signed URL.

---

# 36. Integrations

## GET /v1/integrations

## POST /v1/integrations

Example:

```json
{
  "name": "Partner System",
  "provider": "PartnerA",
  "integrationType": "MACHINE_TO_MACHINE",
  "scopes": [
    "sessions.read",
    "results.read"
  ]
}
```

---

# 37. Webhooks

## GET /v1/webhooks

## POST /v1/webhooks

```json
{
  "url": "https://partner.example.com/webhooks/kwinhub",
  "events": [
    "session.completed",
    "result.created"
  ]
}
```

---

# 38. Webhook Events

Initial event set:

```text
session.received
session.processing
session.completed
session.failed
result.created
report.generated
sync.completed
sync.failed
```

---

# 39. Webhook Security

Every request should contain:

```http
X-Kunwei-Event
X-Kunwei-Delivery-Id
X-Kunwei-Timestamp
X-Kunwei-Signature
```

Signature should be generated using a shared secret.

Consumers must be able to safely retry based on:

```text
deliveryId
```

---

# 40. Administration

## GET /v1/users

## POST /v1/users/invitations

## GET /v1/organization-memberships

## PATCH /v1/organization-memberships/{id}

## GET /v1/roles

Administration APIs require elevated permissions.

---

# 41. Audit

## GET /v1/audit-logs

Filters:

```text
actorId
action
resourceType
resourceId
from
to
```

Only authorized administrative users can access this endpoint.

---

# 42. OpenAPI Resource Naming

Use plural resource names:

```text
/sessions
/subjects
/protocols
/results
/reports
```

Avoid verbs except where the operation is genuinely an action:

```text
/reports/{id}/download-url
/processing-jobs/{id}/retry
```

---

# 43. API Versioning

Base:

```text
/v1
```

Breaking changes require:

```text
/v2
```

Protocol versions are independent:

```text
API: v1
Protocol: CMJ 2.0
Algorithm: CMJ-3.1.0
```

Do not confuse these three version systems.

---

# 44. OpenAPI Schema Organization

Recommended repository:

```text
docs/
  openapi/
    openapi.yaml
    schemas/
      common.yaml
      organization.yaml
      subject.yaml
      protocol.yaml
      session.yaml
      dataset.yaml
      result.yaml
      report.yaml
      integration.yaml
      sync.yaml
      error.yaml
```

Alternatively, generate OpenAPI from ASP.NET Core contracts while maintaining examples and documentation alongside the code.

---

# 45. DTO Boundary

Never expose EF Core entities directly.

Use:

```text
API DTO
   ↓
Application Command / Query
   ↓
Domain
   ↓
Persistence
```

Example:

```text
CreateMeasurementSessionRequest
CreateMeasurementSessionCommand
MeasurementSession
MeasurementSessionResponse
```

---

# 46. Client Integration Pattern

Kunwei desktop/mobile applications should use:

```text
Authentication
      ↓
Create Session
      ↓
Receive Session ID
      ↓
Request Upload URL
      ↓
Direct Upload
      ↓
Complete Upload
      ↓
Poll / Receive Event
      ↓
Retrieve Result
```

This is preferable to:

```text
Client
 ↓
send huge raw payload
 ↓
API
 ↓
scientific processing
```

---

# 47. Asynchronous Contract

Long-running operations should return:

```http
202 Accepted
```

with a resource identifier.

Examples:

```text
POST /reports
POST /imports
POST /processing
```

Clients can then:

```text
poll
```

or subscribe through:

```text
webhooks
```

---

# 48. API Rate Limits

Initial limits should be configurable rather than hard-coded into the product contract.

Potential dimensions:

```text
organization
application
API key
user
IP
```

Particularly protect:

```text
report generation
export
upload initialization
bulk queries
```

---

# 49. Bulk Operations

Do not introduce generic bulk endpoints everywhere.

Where necessary, provide explicit asynchronous APIs:

```text
POST /v1/imports
POST /v1/exports
```

instead of:

```text
POST /v1/bulk
```

Generic bulk APIs tend to become difficult to secure and maintain.

---

# 50. API Acceptance Scenario

A real Kunwei client must be able to execute:

```text
Authenticate
    ↓
Create Subject
    ↓
Create Session
    ↓
Request Upload URL
    ↓
Upload Raw Data
    ↓
Complete Upload
    ↓
Observe Processing
    ↓
Retrieve Result
    ↓
Retrieve Metrics
    ↓
Retrieve Longitudinal Trend
```

without accessing PostgreSQL or KWinScience directly.

---

# 51. Initial OpenAPI Priority

Implement the following first:

### P0

```text
Authentication
Subjects
Protocols
Sessions
Trials
Uploads
Datasets
Data Objects
Processing Status
Results
Metrics
Data Sources
Devices
Sync
```

### P1

```text
Reports
Integrations
Webhooks
Administration
Audit
Trends
Comparisons
```

### P2

```text
Research APIs
Advanced analytics
AI APIs
Normative data APIs
```

---

# 52. OpenAPI Definition of Done

The V1 API is acceptable when:

- every P0 resource has an OpenAPI definition;
- authentication is documented;
- authorization behavior is documented;
- error codes are stable;
- pagination is standardized;
- idempotency behavior is documented;
- asynchronous operations are documented;
- upload flow is documented;
- webhook signatures are documented;
- examples exist for Kunwei client integration;
- generated OpenAPI can be consumed by SDK tooling.