## KWinSprint Project Analysis — Workflow & Database Structure

### 1. Database Technology Stack
- **Database Engine**: Firebird (default), with MySQL support via configuration (`KW.Database.DataContext/Firebird/`)
- **ORM**: FreeSql (CodeFirst with `SyncStructure` for schema auto-migration)
- **Migration**: Custom SQL migration scripts in `KW.Database.Migrations`
- **Connection Management**: `FreeSqlFactory` with connection pooling, retry policies, and Firebird private server auto-start

---

### 2. Complete Database Schema (21 Tables + 1 View)

#### **Base Entity** (`BaseEntity.cs`) — Every table inherits this:
| Field | Type | Description |
|-------|------|-------------|
| `create_by` | char(36) GUID | Creator UserId |
| `create_time` | long | Unix timestamp (seconds) |
| `update_by` | char(36) GUID? | Updater UserId |
| `update_time` | long? | Last update Unix timestamp |
| `valid` | bool | Soft-delete / active flag |
| `version` | int | Optimistic concurrency |
| `last_modified` | long? | Sync timestamp |

---

#### **A. Identity & User Management Tables**

##### **`user`** — System Users (database: `user_id` PK)
| Field | Type | Description |
|-------|------|-------------|
| `user_id` | char(36) | **PK** |
| `user_name` | varchar(50) | Login account |
| `password` | varchar(100) | AES encrypted |
| `mail` | varchar(50)? | Email |
| `mobile_number` | varchar(20)? | Phone |
| `name` | varchar(50)? | Display name |
| `role` | int | **Flags enum**: None=0, Guest=1, Athlete=2, Coach=4, Admin=8, SuperAdmin=16 |
| `gender` | int | Male/Female |
| `is_maintained` | bool | Whether user is managed (internal vs external) |
| `user_state` | int | Normal/Disabled |
| `avatar` | varchar(255)? | Avatar path |

##### **`remember_password`** — Save login credentials
| Field | Type | Description |
|-------|------|-------------|
| `remember_id` | char(36) | **PK** |
| `account_id` | char(36) | FK → user.user_id |
| `account_name` | varchar(50) | Cached account name |
| `account_password` | varchar(100) | AES encrypted password |
| `is_remember_password` | bool | Toggle |

##### **`coaches`** — Coach profile (links to user)
| Field | Type | Description |
|-------|------|-------------|
| `coach_id` | char(36) | **PK** |
| `user_id` | char(36) | FK → user.user_id |
| `name` | varchar(50) | Coach name |
| `number` | varchar(50) | Display number |
| `gender` | int | |
| `birthday` | DateTime | |
| `sort_order` | varchar(50) | |

---

#### **B. Athlete/Roster Tables**

##### **`athlete`** — Athlete profiles (key table for KWinHub integration)
| Field | Type | Description |
|-------|------|-------------|
| `athlete_id` | char(36) | **PK** |
| `user_id` | char(36)? | FK → user.user_id (nullable) |
| `name` | varchar(50) | Athlete name |
| `number` | varchar(50) | Jersey/display number |
| `birthday` | DateTime | |
| `gender` | int | |
| `weight` | float | kg |
| `height` | float | cm |
| `specialty_level` | BLOB? | JSON/Text — specialty level details |
| `sport_level` | int? | Sports level enum |
| `sport_info` | BLOB? | JSON/Text — sport information |
| `extension` | BLOB? | JSON/Text — extensible fields |
| `remark` | varchar(200)? | |
| `statue` (typo!) | int | Status |
| `sort_order` | varchar(50) | |

##### **`guest`** — Guest athlete profiles (no user account needed)
Almost identical schema to `athlete` but **no `user_id` foreign key**. Guests can be added to test groups temporarily.

---

#### **C. Project Hierarchy Tables**

##### **`project`** — Organizational hierarchy (tree structure)
| Field | Type | Description |
|-------|------|-------------|
| `project_id` | char(36) | **PK** |
| `name` | varchar(100) | Project name |
| `parent_id` | char(36)? | Self-referencing FK → project.project_id |
| `description` | varchar(255)? | |
| `sort_order` | varchar(50) | |

##### **`project_member`** — Links projects to athletes/coaches
| Field | Type | Description |
|-------|------|-------------|
| `project_member_id` | char(36) | **PK** |
| `project_id` | char(36) | FK → project.project_id |
| `member_id` | char(36) | FK → athlete.athlete_id OR coaches.coach_id |
| `member_type` | int | **1=Coach, 2=Athlete** |

**Unique Index**: `(project_id, member_id)` — same member can't be added twice to same project

---

#### **D. Test Group Tables (核心业务层)**

##### **`group`** — Test groups belong to a project
| Field | Type | Description |
|-------|------|-------------|
| `group_id` | char(36) | **PK** |
| `project_id` | char(36) | FK → project.project_id |
| `name` | varchar(100) | |
| `remark` | varchar(255)? | |
| `sort_order` | varchar(50) | |
| `is_public` | bool | Public/private group flag |

##### **`group_member`** — Links athletes/coaches/guests to test groups
| Field | Type | Description |
|-------|------|-------------|
| `group_member_id` | char(36) | **PK** |
| `project_id` | char(36) | FK → project.project_id (冗余) |
| `group_id` | char(36) | FK → group.group_id |
| `member_id` | char(36) | FK → athlete/coaches/guest |
| `member_type` | int | **1=Coach, 2=Athlete, 3=Guest** |
| `permission` | int? | Permissions in group |
| `sort_index` | int? | Ordering |

**Unique Index**: `(group_id, member_id)` — dedup in same group

##### **`group_join_request`** — Pending join requests (simple workflow)
| Field | Type | Description |
|-------|------|-------------|
| `id` | int | **Identity PK** |
| `group_id` | varchar(36) | Group being joined |
| `requester` | varchar(50) | Who requested |
| `request_time` | long | Unix timestamp |
| `process_result` | int | Result enum |
| `processor` | varchar(50)? | Who approved/rejected |
| `process_time` | long? | When processed |

---

#### **E. Test Record Tables (核心采集数据层)**

##### **`records`** — Core test session records
| Field | Type | Description |
|-------|------|-------------|
| `recode_id` | char(36) | **PK** (note: typo in column name) |
| `system_id` | char(36) | System GUID |
| `project_id` | char(36) | FK → project.project_id |
| `group_id` | char(36) | FK → group.group_id |
| `coach_id` | char(36) | FK → coaches.coach_id |
| `athlete_id` | char(36) | FK → athlete.athlete_id |
| `remark` | varchar(200)? | |
| `raw_data_path` | varchar(200) | Path to CSV raw data |
| `devices` | JSON/BLOB | Serialized device snapshot |
| `cameras` | JSON/BLOB | Camera snapshot |
| `crop_start_ms` | double? | Chart clip start (ms) |
| `crop_end_ms` | double? | Chart clip end (ms) |

##### **`record_indicators`** — Calculated metrics for each record (key-value metric store)
| Field | Type | Description |
|-------|------|-------------|
| `record_indicator_id` | char(36) | **PK** |
| `record_id` | char(36) | FK → records.recode_id |
| `system_id` | char(36) | |
| `project_id` | char(36) | |
| `group_id` | char(36) | |
| `athlete_id` | char(36) | |
| `indicator_key` | varchar(100) | Metric key (e.g., "block3d.front.peakForce") |
| `indicator_value` | BLOB | Metric value (string-encoded number/text) |

##### **`indicator_definitions`** — Metric schema/registry
| Field | Type | Description |
|-------|------|-------------|
| `key` | varchar(50) | **PK** — unique metric identifier |
| `lan_key` | varchar(50) | Multi-language resource key |
| `name` | varchar(50) | Chinese display name |
| `system_id` | char(36) | Which system defines this |
| `unit` | varchar(10)? | Unit |
| `decimals` | int | Display precision (default 2) |
| `hierarchy` | Hierarchy? | SingleSegmentTotal/SingleSegment |
| `classification` | Classification | Time/Speed/Distance/SpaceParameter/TimeParameter/MechanicsParameter/ImpulseParameter/ResultParameter |
| `indicator_type` | MetricFormat? | Original vs Standard value |
| `value_type` | MetricValueType | Number/Text/Boolean/Enum |
| `pedal_position` | PedalPosition? | Forward/Rear/Both (3D starting block specific) |

##### **`record_videos`** — Video files per camera per record
| Field | Type | Description |
|-------|------|-------------|
| `record_video_id` | char(36) | **PK** |
| `record_id` | char(36) | FK → records.recode_id |
| `camera_index` | int | Camera number |
| `camera_serial` | varchar(64)? | Device serial |
| `camera_device_name` | varchar(128)? | Basler/MV camera display name |
| `raw_video_path` | varchar(200) | Original video path |
| `trimmed_video_path` | varchar(200)? | Clipped video path |
| `offset_ms` | double? | Sync compensation (negative = started later) |

##### **`event_markers`** — Key timestamps during a test session
| Field | Type | Description |
|-------|------|-------------|
| `marker_id` | int | **Identity PK** |
| `record_id` | char(36) | FK → records.recode_id |
| `marker_key` | varchar(50) | Unique per record (e.g., "StartGun", "FrontPushOffStart") |
| `label` | varchar(100) | Display label (e.g., "发令 t0") |
| `time` | double | Time in seconds |

**Unique Index**: `(record_id, marker_key)`

---

#### **F. System Tables (Local-only, NOT for KWinHub)**

| Table | Purpose | Should be KWinHub? |
|-------|---------|-------------------|
| `system_parameters` | Config key-value store (device groups, video settings, etc.) | **NO** — local config |
| `system_log` | Operation log (user actions, timestamps, log levels) | **NO** — local audit |
| `resource_keys` + `resource_values` | i18n resource strings (key → culture → value) | **NO** — local translations |
| `recycle_bin` | Soft-delete tracking (source_type: Group=0, Record=1, Project=2) | **Partial** — metadata only, data is local |
| `route` | Reserved routes/paths | **NO** — local |

---

### 3. Application Workflow

```
┌─────────────────────────────────────────────────────────────────────┐
│                         App Startup                                │
│  1. Initialize registry config (DB paths, backup, charset...)     │
│  2. Register DI (FreeSql, services, ViewModels...)                │
│  3. Build IFreeSql with Firebird connection                        │
│  4. Auto-create Firebird DB file if needed                         │
│  5. CodeFirst SyncStructure → creates all 21 tables                │
│  6. Run SQL Pre-migrations (schema fixes)                          │
│  7. Run SQL Post-migrations (views, triggers, functions)           │
│  8. Seed default data:                                             │
│     - SuperAdmin user + coach profile                              │
│     - Root + Sub project hierarchy                                 │
│     - Block3D indicator definitions (~hundreds of metrics)         │
└─────────────────────┬───────────────────────────────────────────────┘
                      ▼
┌──────────────────────────────────────────────────────────────────────┐
│                      Login Session Loop                             │
│                                                                      │
│  LoginWindow (ShowDialog)                                            │
│    ├─ Fetch users from local DB (IUserService)                      │
│    ├─ Password = AES(encrypted) → compare with DB                   │
│    └─ On success → open MainWindow                                  │
│                                                                      │
│  MainWindow                                                          │
│    ├─ Sidebar Navigation                                             │
│    │   ├─ Project Workspace  (Projects → Groups → Athlete Roster)   │
│    │   ├─ Testing Workspace  (Live test collection + video)         │
│    │   ├─ Data Records      (Historical records + reports)          │
│    │   └─ Settings          (Users, Devices, Video, RecycleBin)     │
│    └─ On logout → return to LoginWindow                              │
└──────────────────────────────────────────────────────────────────────┘
                      ▼
┌──────────────────────────────────────────────────────────────────────┐
│                    Test Collection Flow                              │
│                                                                      │
│  1. Select Project → Test Group → Athlete + Coach                   │
│  2. Connect devices (ForceStartingBlock 3D, cameras...)             │
│  3. Start collection → raw data streamed to CSV                     │
│  4. Stop collection →                                                │
│     CompleteCollectionAsync()  → Transaction:                       │
│       ├─ INSERT records row                                         │
│       ├─ INSERT record_indicators (session.* metrics)                │
│       ├─ INSERT record_videos (if cameras used)                     │
│       └─ INSERT event_markers (key timestamps)                      │
│  5. Metrics calculation → SaveCalculatedMetricsAsync()               │
│     Upserts record_indicators (block3d.* metrics)                   │
│  6. Generate report (PDF via QuestPDF)                              │
└──────────────────────────────────────────────────────────────────────┘
```

---

### 4. Existing Cloud Integration (What's Already There)

There is a **`CloudDataService`** project with partial cloud integration:

**Cloud API Endpoints defined** (`ApiRoute.cs`):
| Route | Purpose |
|-------|---------|
| `login` + RSA key exchange | Cloud login |
| `token/refresh` | JWT refresh |
| `users` | CRUD users |
| `user/current` | Get/update current user |
| `user/role` | User roles |
| `user/avatar` | Upload avatar |
| `user/modify/password` | Password change |
| `File/{type}` | File upload/download |

**Cloud DTOs already defined** (in `Model/Cloud/`):
| Cloud Model | Status |
|------------|--------|
| `SessionModel` (JwtToken, RefreshToken, SessionId) | ✅ Complete |
| `UserModel` | ✅ Complete |
| `UserInfo` / `UserResult` | ✅ Complete |
| `AthleteResult` (paginated list + LastModified + HasMore) | ⚠️ Skeleton only |
| `CoachResult` | ⚠️ Skeleton only |
| `ProjectDTO` + `ProjectMember` | ✅ Partial (reuses entity types) |
| `GroupDTO` + `GroupMember` | ✅ Partial (reuses entity types) |
| `GroupResult` | ⚠️ Skeleton only |
| `RecordDTO` + `RecordIndicatorss` + `RecordVideos` | ✅ Partial |
| `RecordResult` | ⚠️ Skeleton only |
| `RecycleBin` | ❌ Empty class |

**Cloud Request Services**:
| Service | Methods | Status |
|---------|---------|--------|
| `ILoginRequestService` | `LoginAsync()` with RSA password encryption | ✅ Implemented |
| `IUserRequestService` | `GetUserInfoAsync()`, `UpdateUserInfoAsync()`, `UpdateUserAvatarAsync()`, `UpdateUserPasswordAsync()` | ✅ Implemented |
| `ITokenRequestService` | Token operations | Partial |
| `IRoleRequestService` | Role retrieval | Partial |
| `IRememberPasswordRequestService` | Remember password sync | Partial |
| `IFileRequestService` | File upload/download | Partial |
| `IUploadRequestService` | Upload operations | Partial |

**Key observation**: The Cloud DTOs are designed for **incremental sync** (`LastModified` timestamp, `HasMore` pagination). This is the right pattern for syncing with KWinHub.

---

### 5. Gap Analysis for KWinHub Integration

#### **Tables to MOVE to KWinHub (no longer local)**

| Table | Why KWinHub? | Cloud Sync Complexity |
|-------|-------------|----------------------|
| `user` | Central auth, user management across all clients | ⭐⭐⭐ (auth flow critical) |
| `athlete` | Athlete profiles are global, shared across projects/clients | ⭐⭐⭐ (many fields, links to user) |
| `coaches` | Coach profiles | ⭐⭐ (simple, links to user) |
| `guest` | Guest profiles | ⭐⭐ (similar to athlete) |
| `project` | Organizational hierarchy | ⭐⭐ (self-referencing tree) |
| `project_member` | Project ↔ athlete/coach links | ⭐ (simple join table) |
| `group` | Test groups live within projects | ⭐⭐ (belongs to project) |
| `group_member` | Group ↔ athlete/coach/guest links + permissions | ⭐⭐ (member_type enum, permissions) |
| `group_join_request` | Pending group membership requests | ⭐ (simple workflow) |

#### **Tables that STAY LOCAL (NOT KWinHub)**

| Table | Reason |
|-------|--------|
| `records` | **Core test data** — generated locally from device hardware; may optionally be uploaded to KWinHub later but primary write is local |
| `record_indicators` | Computed metrics — depends on local records |
| `record_videos` | Video file metadata — physical files are local |
| `event_markers` | Key timestamps — local test artifacts |
| `indicator_definitions` | Metric schema — **could move to KWinHub** for centralized metric registry |
| `system_parameters` | Device config, video settings — purely local |
| `system_log` | Local operation audit |
| `resource_keys/values` | i18n — could move but not priority |
| `remember_password` | Local UX preference |
| `recycle_bin` | Deletion tracking — local, but source IDs reference cloud entities |
| `route` | Reserved paths — local only |

#### **Critical Gap: Missing Cloud API Endpoints**

The current `ApiRoute.cs` only defines endpoints for **User auth**. For KWinHub integration, we need:

| Endpoint Category | What's Needed |
|-------------------|---------------|
| **Athletes** | CRUD + List (paginated, incremental via `last_modified`) |
| **Coaches** | CRUD + List |
| **Guests** | CRUD + List |
| **Projects** | CRUD + Tree query + List |
| **ProjectMembers** | Add/Remove + List |
| **Groups** | CRUD per project + List |
| **GroupMembers** | Add/Remove + List + Permissions |
| **GroupJoinRequests** | Submit/Approve/Reject |
| **Records** | Upload results (read-only from KWinHub perspective) |
| **IndicatorDefinitions** | Sync metric schema (could be read-only from KWinHub) |

#### **Architecture Pattern Already in Place**

The good news is KWinSprint already has a **hybrid sync-ready architecture**:

1. **`LastModified` field** on every entity → supports incremental sync
2. **`CloudDataRequestService`** project → REST client with RestSharp, DI registration
3. **Cloud DTOs** with `LastModified` + `HasMore` → sync-friendly pagination
4. **`LoginRequestService`** → RSA encryption, JWT flow (same as KWinHub would use)
5. **`DependencyInjectionExtensions.RegisterHttpRequestService()`** → ready to plug in more API services
6. **`UserAppService.CreateFromCloudUserAsync()`** → already handles "cloud user → local DB" write-through pattern
7. **`INetworkStatusService`** in `LoginAuthService` → already distinguishes online vs offline mode

#### **Recommended Integration Strategy**

```
Phase 1 — Auth + Users
  ├─ Replace local DB login with KWinHub login
  ├─ Cache user in local DB (offline fallback)
  └─ Sync roles, avatar, basic user info

Phase 2 — Organizational Hierarchy
  ├─ Projects tree sync (pull from KWinHub on login)
  ├─ Groups per project sync
  ├─ ProjectMember + GroupMember sync
  └─ Local DB becomes read-through cache

Phase 3 — Roster
  ├─ Athletes CRUD → KWinHub API
  ├─ Coaches CRUD → KWinHub API  
  ├─ Guests CRUD → KWinHub API
  └─ LastModified incremental sync

Phase 4 — Test Data Upload
  ├─ After collection completes → push Records + Indicators + Markers to KWinHub
  ├─ Videos stay local (large binary)
  └─ KWinHub becomes authoritative record archive

Phase 5 — Settings + Cleanup
  ├─ Remove or deprecate local tables (user, athlete, coaches, group, etc.)
  ├─ Keep local only: records, record_*, system_*, resources
  └─ Full offline-first with background sync
```

---

### Questions for You

1. **Will KWinHub also host `records`, `record_indicators`, `event_markers`**, or will those stay local? This is the biggest architectural decision.
2. **How should offline mode work**? Currently all data is local — should we maintain a local cache that syncs when back online?
3. **Will KWinHub have its own metric definitions (`indicator_definitions`)**, or should KWinSprint keep those locally?
4. **What's the authentication model** — JWT with refresh tokens (already implemented pattern), or something else?
5. **Should the cloud sync be push-based (KWinHub sends updates) or pull-based (KWinSprint polls `last_modified` timestamps)?** The current Cloud DTOs support pull-based.

This analysis should give you a solid foundation to compare with KWinHub's data model and plan the migration. Let me know if you want me to dive deeper into any specific area!