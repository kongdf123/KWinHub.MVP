# KWinHub — Six Legacy Database Gap Analysis, Target Data Model & Delivery Backlog

Full document content

Version 1.0

2026-11-06

Status: Architecture proposal for review\
Scope: Analysis of six legacy MySQL database schemas and recommendations for the current KWinHub project.

## 1. Executive summary

The six legacy databases represent six measurement products, but they are not six unrelated business domains. They contain a substantial shared core:

- Organization/company ownership.
- Athlete (`Subject`) identity and coach relationships.
- Projects and project membership.
- Measurement records.
- Raw measurement payloads or references to payload files.
- Calculated indicators and event/peak details.
- Reference or standard values.
- In several products, training plans and training-record mappings.
- Video, snapshots, device information, and test configuration in selected products.

The existing schemas evolved independently. The same concepts are named and structured differently, and some products place calculated values directly on a record while others use child tables. Some product schemas retain raw payloads in text columns; others store file-like identifiers.

All six dumps also show a repeated physical table-sharding pattern (`_00` through `_09`) for one or more high-volume tables, with `UNION ALL` views in several databases.

Recommended direction: KWinHub should become the shared athlete, organization, testing, session, asset, result and reporting platform. It should not become a lowest-common-denominator replacement for the scientific models inside each product.

The target model should have:

1. A normalized shared platform core.
2. A common measurement lifecycle (`MeasurementSession → Trial → DataObject/Asset → ProcessingRun → Result → ResultMetricValue`).
3. Versioned protocol, metric and device catalogs.
4. Product-specific measurement context and specialized result details.
5. Immutable processing provenance.
6. A controlled legacy-import boundary.

The recommended implementation is a modular monolith, with separate Platform and Organization/Hub API boundaries as already discussed. Do not introduce microservices or a universal signal-processing engine as part of this schema-consolidation effort.

### 1.1 Main conclusions

| Area                | What the six schemas show                                                                                                        | KWinHub decision                                                                                                           |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| Athlete identity    | `Subject` data is duplicated or referenced through product-local tables; coach relationships are often stored as account strings | One organization-scoped Athlete profile and explicit memberships/relationships                                             |
| Projects            | Similar `project` / `projectmember` tables, but different attributes and semantics                                               | Preserve a shared organizational grouping concept only after confirming legacy meaning; do not equate Project with Session |
| Measurement records | Product-specific record tables carry different test context and result payloads                                                  | Shared Session and Trial lifecycle plus typed product-specific context                                                     |
| Metrics             | Inline columns, key/value tables, indicator tables, peaks and event tables coexist                                               | Versioned MetricDefinition plus typed ResultMetricValue and optional specialized detail/event tables                       |
| Raw data            | `RawData`, `FileCode`, `FilePath`, `VideoPath`, `CameraInfo`, `KeyPointInfo` and related fields vary widely                      | DataObject/Asset metadata in PostgreSQL; large raw files and video in object storage                                       |
| References/norms    | Per-athlete saved references and population standard-data tables are mixed together                                              | Separate PersonalBaseline from NormativeDataset                                                                            |
| Training plans      | Similar plan/detail/map tables appear in several products                                                                        | Model as a separate TrainingPlan bounded context; do not force into the first measurement MVP                              |
| Data integrity      | No declared foreign keys in the six dumps; uneven indexes and tenant columns                                                     | Explicit FK, tenant scope, unique constraints, delete policy and indexes                                                   |
| Physical sharding   | `_00`–`_09` tables and `UNION ALL` views are present                                                                             | Import into logical tables; partition only when measured PostgreSQL workload justifies it                                  |

## 2. Source material and analysis boundaries

### 2.1 Six database cases reviewed

| Legacy database      | Primary product/domain indicated by schema | Main schema characteristics                                                                                            |
| -------------------- | ------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------- |
| `sportsperformance`  | Force-plate / sports-performance testing   | Device and camera metadata, model settings, indicator values, signs/events, standard-data tables                       |
| `powerrack`          | PowerRack testing                          | Limb/action/pose context, force ratios, record summaries, peak/impulse rows, saved references and training plans       |
| `musclestrength`     | Muscle strength testing                    | Assembly/BOM configuration, limb/action/pose context, indicator rows, standard data, parameters and training plans     |
| `hamstrings`         | Hamstring strength testing                 | Action/knee position, raw data, torque and bilateral force summaries, peak/impulse rows, references and training plans |
| `jointrangeofmotion` | Joint range-of-motion testing              | Body-part/action/position catalog, record header and ordered bilateral degree/time items                               |
| `golf`               | Golf swing / golf measurement              | Athlete golf attributes, club/setup values, raw record file, video/snapshot, offset and time-position tags             |

The names above are taken from the database names and table/column names. They are not a claim that every possible product workflow is represented by the dump.

### 2.2 What can and cannot be concluded

Supported directly by the SQL dumps:

- Table names, column names, declared data types, primary keys, indexes and view definitions.
- Repeated product-local structures.
- Physical table suffixes and `UNION ALL` view patterns.
- Which tables carry `CompanyId` and which do not.
- Where raw data, media paths, metrics, events, references and catalog-like values are represented.

Not established by schema-only evidence:

- Exact meaning of enum/tinyint/int values such as `TestType`, `Action`, `Side`, `SignType`, `RecordPeakType`, `Tag` and `Limb`.
- Whether a `RawData`, `FileCode` or `FilePath` value is a path, serialized JSON, compressed data, object key or another encoding.
- Actual cardinality, data volume, retention policy or query workload.
- Whether `CompanyId` is a tenant, legal entity, customer account or legacy partition key in every application.
- Whether `Creator`, `Coach` and `Account` strings refer to the same identity system.
- Exact semantics of `Project`, `TraceId`, `BSA`, `BSARelevantId`, `RecordScol`, `Offset` and other domain-specific fields.

These unknowns must be resolved from application code, representative data samples and product-owner confirmation before finalizing migration mappings.

## 3. Six-database schema analysis

### 3.1 SportsPerformance

Core tables

- `project`
- `projectmember`
- `subjectrelation`
- `record`
- `recordindicator`
- `recordmodelsetting`
- `sign`
- `forceplatesinfo`
- `indicators`
- `indicatorsstate`
- `standarddata_header`
- `standarddata_detail`
- `carousel`

What the schema tells us

- A measurement record carries `CompanyId`, `ProjectId`, `SubjectId`, `Weight`, `ModelKey`, `MinimumForceThreshold`, `KeyPointInfo`, `DeviceInfo`, `CameraInfo`, `Duration`, `FilePath`, creator and timestamps.
- Calculated indicator rows are stored separately in `recordindicator`, with `IndicatorKey`, string `Value`, optional `RatioValue` and `Times`.
- `recordmodelsetting` stores per-record model parameters/settings.
- `sign` stores time-positioned values with `Time`, `Name`, `Value` and `SignType`. This appears to be an event/marker or signal annotation structure, but its exact semantics need source-code confirmation.
- `forceplatesinfo` is a device-model/configuration catalog.
- `standarddata_header` and `standarddata_detail` represent a model/gender/age-banded standard dataset.
- `indicatorsstate` appears to persist user-specific indicator display state, not scientific result data.

Modeling implications

1. Preserve the distinction between measurement configuration, acquired data, derived metrics, event markers and UI preferences.
2. Treat device and camera metadata as structured provenance, not as opaque text forever.
3. Separate population norms from individual athlete baselines.
4. Do not treat `indicatorsstate` as a metric catalog or result table.

Migration questions

- Is `KeyPointInfo` a detected event sequence, a configuration, or both?
- What is the serialization contract for `DeviceInfo` and `CameraInfo`?
- Are `sign` rows scientifically meaningful events, user annotations, or chart markers?
- Does `recordindicator.Value` contain numeric values only, or categorical/formatted values too?

### 3.2 PowerRack

Core tables

- `project`
- `projectmember`
- `subjectrelation`
- `record`
- `recordinfos`
- `recordpeak`
- `references`
- `action_data`
- `trainingplan`
- `trainingplandetail`
- `trainingplanactionrepeattimes`
- `trainingrecordmap`

What the schema tells us

- `record` stores `CompanyId`, project and subject identifiers, `Limb`, `Action`, `Pose`, optional `Side`, scale values, `Threshold`, `RangeThreshold`, `RawData`, bilateral force ratios, creator and timestamps.
- The action context is stored as strings (`Limb`, `Action`, `Pose`, `Side`) rather than stable catalog identifiers.
- `recordinfos` stores per-record summary values such as left/right maximum and average force, counts and asymmetry indices.
- `recordpeak` stores repeated peak rows with `Times`, `LoR`, `Value`, `Checked` and `Impulse`.
- `references` stores a saved reference associated with an account, subject, test context and a source `RecordId`, including `AvgCount`.
- Training-plan tables describe ordered actions and repetition counts.

Modeling implications

1. Model test context using versioned catalog identifiers, not free-form labels as the long-term canonical representation.
2. Keep summary metrics and peak/event details distinguishable.
3. A saved reference is not the same thing as a population norm.
4. A peak is a repeated, typed observation/event, not simply another generic scalar metric.

Migration questions

- Confirm the allowed values and historical spelling/casing of `Limb`, `Action`, `Pose` and `Side`.
- Confirm whether `recordpeak` rows are algorithm-detected peaks, manually checked peaks, or both.
- Clarify whether `references` is a user-selected baseline, a rolling average or another comparison mechanism.

### 3.3 MuscleStrength

Core tables

- `project`
- `projectmember`
- `projectmodule`
- `records`
- `recordindicator`
- `bomtypes`
- `modulebom`
- `moduleassembly`
- `parameters`
- `standarddata`
- `systemlog`
- `trainingplan`
- `trainingplandetail`
- `trainingrecordmap`

What the schema tells us

- A record is associated with an `AssemblyKey`, `Limb`, `Action`, `Pose`, optional `Side`, duration and `FileCode`.
- `projectmodule` connects a project with one or more assemblies.
- The BOM tables describe hierarchical module configuration using `ParentKey`, `BOMType`, `Value`, `Seq` and optional English label.
- `recordindicator` stores `IndicatorKey`, numeric `IndicatorValue`, `Unit`, optional `Times` and `Index`.
- `standarddata` stores limb/action/pose/gender/age-banded values.
- `parameters` stores company-scoped typed values as `longtext`.
- Training-plan structures are present.

Modeling implications

1. The Assembly/BOM concept is a product configuration model. It should not be confused with a test protocol or a device model.
2. Preserve assembly configuration/version at the time of measurement.
3. Metric values need units and optional repetition/index dimensions.
4. Company-specific parameters should become typed, validated configuration rather than arbitrary unversioned key/value text.
5. Keep operational logs separate from scientific observations.

Migration questions

- Is `AssemblyKey` a physical device assembly, a test setup, a configurable test module or a combination?
- What does `BSA` mean in the application context, and is it a measured input, normalized value or derived output?
- Does `FileCode` identify an external file, serialized raw payload or a record package?
- Are `standarddata` values population norms, reference targets or protocol-specific expected values?

### 3.4 Hamstrings

Core tables

- `project`
- `projectmember`
- `subjectrelation`
- `records`
- `recordinfos`
- `recordpeak`
- `references`
- `trainingplan`
- `trainingplandetail`
- `trainingplanactionrepeattimes`
- `trainingrecordmap`

What the schema tells us

- The main record contains `Action`, optional `KneePosition`, `Threshold`, `RangeThreshold`, `RawData`, left/right torque, left/right repetition counts, bilateral maximum/average force and asymmetry values.
- `recordinfos` stores extensible `Key`/`Value` summaries.
- `recordpeak` stores repeated peaks with side (`LoR`), value, checked state and impulse.
- `references` is linked to company, account, subject, action and source record.
- Training plan structures are similar to PowerRack, but not identical.

Modeling implications

1. The product has both fixed, queryable bilateral summary fields and extensible key/value details.
2. The canonical model should support left/right values without forcing every measurement into a single scalar.
3. Keep peak/event rows separately queryable.
4. Product-specific action and knee-position semantics should be represented through a versioned test-context/catalog model.

Migration questions

- Clarify the semantic distinction between `Torque_L/R` and `MaxForce_L/R`.
- Confirm how `Times_L/R` relate to `recordpeak.Times`.
- Determine whether `recordinfos.Key` duplicates fields on `records` or stores additional outputs.
- Confirm `LoR` encoding and whether unknown/combined sides exist.

### 3.5 JointRangeOfMotion

Core tables

- `body_part`
- `body_action`
- `body_position`
- `project`
- `projectmember`
- `subject_relation`
- `record`
- `record_item`

What the schema tells us

- The movement catalog is hierarchical: body part → body action → body position.
- Catalog entries have Chinese and optional English labels, validity, timestamps and sort order.
- A record captures organization, project, subject, relation, creator and selected body-part/action/position identifiers.
- `record_item` stores ordered measurements with left/right degrees and left/right time values.
- `raw_data` is a short string field; its payload semantics are not visible from the schema.

Modeling implications

1. Movement taxonomy should be modeled as a reusable, versioned catalog, not copied into every record.
2. Bilateral angle and time outputs should map to typed result values or a specialized ROM detail structure.
3. Ordered repeated measurements should preserve sequence.
4. The coach/athlete relationship should become a real identity relationship rather than a string-only association.

Migration questions

- Is `body_order` a measurement sequence, anatomical order or body-part ordering?
- What is the unit and meaning of `left_time` / `right_time`?
- Does `raw_data` reference a file or contain a small raw measurement value?
- Are catalog entries global, organization-specific or product-version-specific?

### 3.6 Golf

Core tables

- `project`
- `projectmember`
- `record`
- `recordtag`
- `subjectinfo`
- `subjectrelation`

What the schema tells us

- `subjectinfo` stores subject attributes including weight, height, shoe size, power leg, swing side and avatar.
- `record` stores spacing, golf clubs, weight, left/right angles, create time, duration, `FilePath`, optional `VideoPath`, `SnapShot`, creator, validity, remark and `Offset`.
- `recordtag` stores tag type and a `Position` value for a record.
- The dump defines a `golf_record` view that unions ten physical record shards.
- The `Offset` field and record tags suggest time-alignment or annotation use cases, but their exact semantics need confirmation.

Modeling implications

1. Golf illustrates why KWinHub needs a first-class asset and synchronization model: a single test can include raw data, video, snapshots and time-positioned annotations.
2. Athlete domain attributes should be classified: general athlete profile data versus sport-specific profile attributes.
3. `recordtag` is better modeled as an event/annotation with a typed position and timebase, not a generic free-form tag.
4. Video synchronization metadata should be explicit and unit-defined.

Migration questions

- Is `Offset` measured in milliseconds, frames, samples or another unit?
- What are the allowed `recordtag.Tag` values and the coordinate system of `Position`?
- What structure is stored in `GolfClubs`?
- Does `FilePath` identify raw swing sensor data, a complete record package or another asset?

## 4. Cross-database gap analysis

### 4.1 Shared concepts are duplicated and inconsistent

The same broad concepts recur across the six databases:

| Shared concept       | Legacy representations                                                                 | Gap                                                                                      |
| -------------------- | -------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| Organization         | `CompanyId` on many, but not all, tables                                               | Inconsistent tenant scoping; some child tables omit company scope                        |
| Athlete              | `SubjectId`, `subjectinfo`, `subjectrelation`, `subject_relation`                      | No unified athlete profile or explicit identity/membership model across products         |
| Coach relationship   | `Coach` / `Account` strings and `RelationId`                                           | Identity is string-based and product-local                                               |
| Project              | `project`, `projectmember` with varying fields                                         | Similar name, but semantics and lifecycle are not proven identical                       |
| Measurement          | `record`, `records`                                                                    | Product-specific headers, fields and raw payload conventions                             |
| Result metrics       | inline columns, `recordindicator`, `recordinfos`, `recordpeak`, `record_item`, `sign`  | No shared metric definition, value type, unit or dimension contract                      |
| Device/configuration | `ModelKey`, `DeviceInfo`, `forceplatesinfo`, `AssemblyKey`, `modulebom`, `action_data` | Different concepts are mixed: device model, assembly, protocol setup and action taxonomy |
| Baseline/reference   | `references`, `standarddata`, `standarddata_header/detail`                             | Personal baseline and population norms are not separated consistently                    |
| Training plan        | `trainingplan` family                                                                  | Repeated but product-specific structures                                                 |
| Asset                | `FilePath`, `RawData`, `FileCode`, `VideoPath`, `SnapShot`, `CameraInfo`               | No unified asset metadata, access control, checksum or lifecycle                         |
| Event/annotation     | `sign`, `recordpeak`, `recordtag`, ordered record items                                | Different timebases, event types and validation semantics                                |

### 4.2 Tenant isolation is not consistently represented

`CompanyId` is present on most top-level measurement tables, but it is absent from several relationship or child tables. Examples include some `projectmember`, `recordpeak`, `recordtag`, `moduleassembly`, `modulebom`, `trainingplandetail`, `trainingrecordmap` and ROM catalog/child structures.

This does not prove that the old applications have a tenant-isolation vulnerability; application code may enforce ownership through parent relationships. It does mean the schema alone does not consistently enforce tenant ownership at every relationship boundary.

KWinHub requirement: every organization-owned entity must be tenant-scoped, directly or through a rigorously enforced parent relationship. Every API query, object-storage operation, background job and export must use the authorized organization context.

### 4.3 Referential integrity is mostly implicit

The six dumps contain primary keys and indexes but no declared foreign-key constraints. The schema therefore does not itself guarantee that:

- A record's subject belongs to the same company.
- A child metric/peak/tag belongs to an existing record.
- A project member references a valid project and athlete.
- A training detail references an existing plan.
- A reference points to a record for the same subject and organization.

KWinHub should use explicit foreign keys and tenant-aware constraints wherever practical. Import staging should record and report orphaned or ambiguous legacy relationships rather than silently dropping them.

### 4.4 Metric modeling is fragmented

The six schemas use several result patterns:

- Fixed columns on record headers (`MaxForce_L`, `Torque_R`, `ForceRatio_L`, etc.).
- Generic indicator tables (`recordindicator`).
- Generic key/value tables (`recordinfos`).
- Repeated peak rows (`recordpeak`).
- Ordered measurement items (`record_item`).
- Time-positioned marker rows (`sign`, `recordtag`).

A single `Metric(Name, double Value)` table would lose essential semantics. It would not represent categorical results, bilateral values, repetitions, ordered series, event positions, units, or references to large time-series payloads.

### 4.5 Protocol, configuration and device concepts are conflated

Examples:

- `ModelKey` and `forceplatesinfo` relate to device/model configuration.
- `AssemblyKey`, `moduleassembly`, `modulebom` relate to MuscleStrength assemblies/BOM configuration.
- `Limb`, `Action`, `Pose`, `Side`, `body_part`, `body_action`, `body_position` describe test or movement context.
- Thresholds and per-record model settings describe measurement configuration.

These should not be collapsed into one `Protocol` or one `Device` object. KWinHub needs separate versioned concepts for device models, physical devices, test protocols, protocol steps, product configuration and movement/action taxonomy.

### 4.6 Raw data and media lack a common asset contract

The schemas use a mixture of long text, short strings and paths for raw data and media. Some records include video and snapshots; others include camera/device metadata. The SQL alone cannot tell whether each field stores the payload or a pointer.

KWinHub should not store large raw force data or video blobs in ordinary relational columns. Store object metadata and access-control ownership in PostgreSQL; store large payloads in private object storage. Preserve a checksum, byte size, media type, source, timebase and synchronization metadata.

### 4.7 Physical sharding is a legacy implementation detail

All six dumps contain tables with `_00` through `_09` suffixes:

| Database           | Sharded table families                                    |
| ------------------ | --------------------------------------------------------- |
| SportsPerformance  | `record`, `recordindicator`, `recordmodelsetting`, `sign` |
| PowerRack          | `record`, `recordinfos`, `recordpeak`                     |
| MuscleStrength     | `records`, `recordindicator`, `systemlog`                 |
| Hamstrings         | `records`, `recordinfos`, `recordpeak`                    |
| JointRangeOfMotion | `record`, `record_item`                                   |
| Golf               | `record`, `recordtag`                                     |

Several dumps define `UNION ALL` views over the shards. This is an important migration concern, but it is not a target-domain concept.

Recommendation: import all shards into one logical target table family, retaining source database, source table/shard and source record ID in migration lineage. Do not reproduce ten-table sharding in PostgreSQL by default. Consider declarative partitioning only after real query and retention measurements justify it.

### 4.8 Data type and naming inconsistencies

Observed schema-level inconsistencies include:

- Mixed `utf8mb3` and `utf8mb4` character sets.
- `float`, `double`, integer, string and `longtext` used inconsistently for similar values.
- `datetime`, `datetime(6)` and `bigint` timestamps coexisting.
- PascalCase and snake_case naming mixed across products.
- File-like references represented as `varchar(50)`, `varchar(45)` or `longtext`.
- Soft-delete/validity fields with inconsistent names and placement.

The PostgreSQL target should use one naming convention, UTC-aware timestamps (`timestamptz`), explicit units and appropriate numeric precision. Do not mechanically translate MySQL types without understanding the values.

## 5. Proposed KWinHub target architecture

### 5.1 Domain boundary

Keep the already agreed two-boundary platform design:

```
                    KWinHub Platform
                           |
             +-------------+-------------+
             |                           |
        Platform.Api                  Hub.Api
             |                           |
   Platform control plane       Organization workspace
   - tenant lifecycle           - organization membership
   - platform users             - athletes / teams
   - global catalogs             - projects / programs
   - catalog publication         - sessions / trials
   - governance                  - data / results / reports
             |                           |
             +------ published catalog --+
                     read model / events
```

- `Platform.Api` owns global platform users, tenant lifecycle, global catalog authoring and publication.
- `Hub.Api` owns organization membership, athletes, teams, projects, measurement sessions, trials, data objects, results, reports and organization-specific operations.
- Client applications call `Hub.Api`, not `Platform.Api`.
- `Hub.Api` consumes a versioned published-catalog read model. It should not directly query Platform's database.
- Keep the initial deployment a modular monolith. Separate bounded contexts in code and persistence conventions before considering separate services.

### 5.2 Canonical measurement lifecycle

```
Organization
   |
   +-- Athlete / Team / Membership
   |
   +-- Project or Assessment Program (only if confirmed semantics fit)
          |
          +-- MeasurementSession
                 |
                 +-- SessionParticipant
                 |
                 +-- SessionDevice (1..N)
                 |
                 +-- Trial (1..N)
                        |
                        +-- TrialContext / Product-specific details
                        |
                        +-- DataObject / Asset (raw data, video, snapshot)
                        |
                        +-- ProcessingRun (versioned, traceable)
                               |
                               +-- Result
                                      |
                                      +-- ResultMetricValue (typed)
                                      +-- ResultEvent / Segment
                                      +-- Specialized result details
```

Important semantic separation

- MeasurementSession: one operational testing occasion, with organization, time, operator, location and participating devices.
- Trial: one attempt or measurement unit within a session. A session may contain multiple trials and multiple athletes, depending on workflow.
- DataObject: a source payload or acquired artifact, such as a force-time file, timing stream or sensor package.
- Asset: a managed file such as video, snapshot, report or exported package. `DataObject` may reuse the same storage infrastructure but should retain scientific-data semantics.
- ProcessingRun: one execution of a specified processing/calculation version against identified inputs and configuration.
- Result: one immutable output set from a processing run or a source application's submitted calculation result.
- ResultMetricValue: a typed value identified by a versioned metric definition.
- ResultEvent / Segment: time- or sequence-positioned event/phase/annotation. Keep domain-specific meaning explicit.

Do not assume one session equals one legacy record. Define migration mapping per product after confirming actual workflows.

## 6. Proposed logical data model

### 6.1 Organization, identity and access

| Entity                     | Purpose                                 | Key relationships / constraints                                                           |
| -------------------------- | --------------------------------------- | ----------------------------------------------------------------------------------------- |
| `Organization`             | Tenant/customer workspace               | Stable ID; lifecycle and status                                                           |
| `User`                     | Platform identity                       | Managed identity subject; not duplicated per organization                                 |
| `OrganizationMembership`   | User's membership in an organization    | Unique `(OrganizationId, UserId)`; role/status                                            |
| `Athlete`                  | Organization-owned athlete profile      | Tenant-scoped; legacy `SubjectId` mapping                                                 |
| `Team`                     | Organization-owned group/team           | Tenant-scoped                                                                             |
| `TeamMembership`           | Athlete membership with effective dates | Unique active membership policy                                                           |
| `CoachAthleteRelationship` | Explicit coach/athlete relationship     | User/coach identity, athlete, organization, status and dates                              |
| `AthleteSportProfile`      | Sport-specific attributes               | E.g. golf swing side/power leg; extensible by sport domain without polluting core Athlete |

Decision: do not assume an athlete is globally visible across organizations. Begin with organization-scoped athlete records. Cross-organization identity resolution requires a separate privacy and consent design.

### 6.2 Project, program and planning

| Entity                           | Purpose                                                 | Notes                                                                              |
| -------------------------------- | ------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `Project` or `AssessmentProgram` | Organizational grouping/campaign                        | Keep only after legacy semantics are verified; do not use as a synonym for session |
| `ProjectAthlete`                 | Athlete membership in a project                         | Maps legacy `projectmember` where appropriate                                      |
| `TrainingPlan`                   | Reusable training prescription                          | Separate from assessment protocol                                                  |
| `TrainingPlanVersion`            | Immutable revision of a plan                            | Needed if plan history must be reproducible                                        |
| `TrainingPlanItem`               | Ordered exercise/action and dosage                      | Product-specific action references may be retained                                 |
| `TrainingRecordLink`             | Links a training plan item/trace to measurement records | Replaces ambiguous TraceId mapping only after semantics are understood             |

Training plans appear in PowerRack, MuscleStrength and Hamstrings. They are meaningful legacy capabilities, but they need not block the first KWinHub measurement-ingestion vertical slice.

### 6.3 Protocol, catalogs and devices

| Entity                        | Purpose                                                      | Notes                                                 |
| ----------------------------- | ------------------------------------------------------------ | ----------------------------------------------------- |
| `TestProtocol`                | Reusable test definition                                     | Platform catalog; stable identity                     |
| `TestProtocolVersion`         | Immutable published version                                  | Parameters, steps, supported products/devices         |
| `ProtocolStep`                | Ordered test/action steps                                    | Version-scoped                                        |
| `MetricDefinition`            | Meaning, type, unit and display metadata for a metric        | Versioned and published                               |
| `DeviceModel`                 | Manufacturer/model/capability definition                     | Global catalog                                        |
| `Device`                      | Physical or logical instrument registered to an organization | Serial/asset identity and status                      |
| `SessionDevice`               | Device used during a session                                 | Supports multiple devices per session                 |
| `DeviceConfigurationSnapshot` | Configuration captured at test time                          | Immutable provenance                                  |
| `MovementTaxonomy`            | Body part/action/position or movement hierarchy              | Versioned global catalog with localized labels        |
| `ProductConfiguration`        | Product-specific assembly/model/setup configuration          | Must not be confused with protocol or physical device |

The ROM `body_part → body_action → body_position` structure is a strong candidate for a reusable movement taxonomy. MuscleStrength BOM/assembly is a separate product-configuration concern. Force plate model information is a device-model concern.

### 6.4 Measurement and ingestion

| Entity               | Purpose                                      | Key fields / constraints                                                   |
| -------------------- | -------------------------------------------- | -------------------------------------------------------------------------- |
| `MeasurementSession` | Testing occasion                             | `OrganizationId`, start/end, operator, status, source client               |
| `SessionParticipant` | Athletes participating in a session          | Unique session/athlete relationship                                        |
| `Trial`              | Individual attempt or record                 | Session, athlete, protocol version, sequence, status                       |
| `TrialContext`       | Structured test-specific context             | Limb/action/pose/side, knee position, golf setup, thresholds, etc.         |
| `DataSource`         | Originating client/application/integration   | Client ID, product, version, trust/registration state                      |
| `DataObject`         | Raw source payload metadata                  | Type, storage key, checksum, byte size, format, timebase                   |
| `Asset`              | Managed media/report/file                    | Type, storage key, media type, access policy, checksum                     |
| `AssetLink`          | Links an asset to session/trial/result/event | Polymorphic links should be constrained or modeled as explicit link tables |
| `IngestionReceipt`   | Idempotent record of client submission       | Unique source/idempotency key; request hash and outcome                    |
| `ProcessingJob`      | Asynchronous processing lifecycle            | Status, retries, correlation ID, failure code                              |

The source applications may continue to calculate their scientific metrics and submit the resulting data to Hub. The Hub should validate, persist, display and compare those submitted results. Server-side scientific recalculation remains a separate decision and ADR.

### 6.5 Result and scientific provenance

| Entity                                     | Purpose                                               | Key fields / constraints                                                             |
| ------------------------------------------ | ----------------------------------------------------- | ------------------------------------------------------------------------------------ |
| `ProcessingRun`                            | One calculation/processing execution                  | Algorithm ID/version, input IDs/checksums, config snapshot, timestamps and status    |
| `AlgorithmDefinition` / `AlgorithmVersion` | Immutable algorithm metadata                          | Published version and compatibility                                                  |
| `Result`                                   | Result set associated with a trial and processing run | Source, status, created time, supersedes relationship                                |
| `ResultMetricValue`                        | Individual metric observation                         | Definition ID/version, typed value, unit, dimensions, repetition/index               |
| `ResultEvent`                              | Time/sequence-positioned event                        | Event type, position, timebase, confidence/source where applicable                   |
| `ResultSegment`                            | Interval or phase                                     | Start/end position and timebase, label/type                                          |
| `ResultBilateralValue` or dimensions       | Bilateral values where needed                         | Prefer metric dimensions (`side=left/right`) if it remains queryable and unambiguous |
| `ResultStructuredValue`                    | Structured non-scalar result                          | Schema/version and validated JSONB only for genuinely structured payloads            |
| `ResultSeriesReference`                    | Link to high-rate time-series data                    | DataObject/series metadata rather than millions of relational rows                   |

Metric value types to support

At minimum:

- Numeric.
- Integer/count.
- Boolean.
- Categorical/code.
- Text where scientifically appropriate.
- Bilateral/dimensioned.
- Structured.
- Time-series or event-series reference.

Do not make every value a `double`. Do not put large force-time arrays into `ResultMetricValue`.

Historical immutability rule

A new algorithm version must not silently overwrite historical results. Preserve the original `Result` and its provenance. If recalculation is approved, create a new `ProcessingRun` and a new `Result`, with an explicit `SupersedesResultId` or equivalent lineage.

### 6.6 Baselines, norms and comparisons

| Entity                    | Purpose                                                          | Legacy source examples                                                  |
| ------------------------- | ---------------------------------------------------------------- | ----------------------------------------------------------------------- |
| `PersonalBaseline`        | Athlete- or user-selected reference based on one or more records | PowerRack/Hamstrings `references`                                       |
| `PersonalBaselineItem`    | Metric/context-specific baseline value or source set             | `AvgCount`, action/context and source record                            |
| `NormativeDataset`        | Population reference dataset                                     | SportsPerformance standard header/detail; MuscleStrength `standarddata` |
| `NormativeDatasetVersion` | Immutable version of a norm set                                  | Gender, age band, model/protocol and publication                        |
| `NormativeValue`          | Metric/context-specific population value                         | Mean, outstanding/threshold or other defined statistic                  |

Do not merge personal reference records with population norms. They answer different questions and require different ownership, versioning and access rules.

### 6.7 Audit and operational data

- `AuditEvent`: actor, organization, action, target, timestamp, correlation ID and structured details.
- `UserPreference`: user display preferences, including metric selection. Do not store scientific results here.
- `IngestionReceipt`: request idempotency and integration trace.
- `ProcessingJob`: status, retries and failure information.
- Operational logs/telemetry should be retained in observability infrastructure, not copied wholesale into the domain database.

## 7. Legacy-to-KWinHub mapping matrix

This is a first-pass mapping proposal, not a final ETL specification.

| Legacy source                                  | KWinHub target                                   | Migration treatment                                                                                       |
| ---------------------------------------------- | ------------------------------------------------ | --------------------------------------------------------------------------------------------------------- |
| `CompanyId`                                    | `OrganizationId`                                 | Maintain an explicit source-company-to-organization mapping; never assume numeric IDs are globally unique |
| `subjectinfo` / `SubjectId`                    | `Athlete` / `AthleteSportProfile`                | Preserve source athlete ID and organization mapping; resolve duplicates with review rules                 |
| `subjectrelation` / `subject_relation`         | `CoachAthleteRelationship`                       | Resolve `Coach` string to a known user; quarantine unresolved identities                                  |
| `project`                                      | `Project` or `AssessmentProgram`                 | Map common fields; retain source payload and semantics until confirmed                                    |
| `projectmember`                                | `ProjectAthlete`                                 | Validate project and athlete belong to the same organization                                              |
| `record` / `records`                           | `MeasurementSession` + `Trial` (or Trial only)   | Product-by-product mapping; do not blindly map one legacy record to one session                           |
| `recordindicator`                              | `ResultMetricValue`                              | Map `IndicatorKey` to versioned MetricDefinition; preserve original key/value                             |
| `recordinfos`                                  | `ResultMetricValue` or specialized result detail | Classify each key; do not leave all values as untyped strings indefinitely                                |
| `recordpeak`                                   | `ResultEvent` / `ResultMetricValue`              | Preserve peak type, side, sequence, checked state, impulse and source                                     |
| `record_item`                                  | `ResultMetricValue` plus ordered item detail     | Preserve sequence and left/right angle/time semantics                                                     |
| `sign`                                         | `ResultEvent` / annotation                       | Resolve sign type, time unit and scientific meaning                                                       |
| `recordtag`                                    | `ResultEvent` / annotation                       | Resolve tag code and position timebase                                                                    |
| `FilePath`, `RawData`, `FileCode`              | `DataObject`                                     | Determine whether each is a path, object key or serialized payload before migration                       |
| `VideoPath`, `SnapShot`, `CameraInfo`          | `Asset` + synchronization metadata               | Preserve source URI/path, offset and camera configuration                                                 |
| `references`                                   | `PersonalBaseline`                               | Preserve owner/account, context, source record and averaging rules                                        |
| `standarddata*`, `standarddata`                | `NormativeDataset*`                              | Version and validate age/gender/context definitions                                                       |
| `forceplatesinfo`                              | `DeviceModel`                                    | Normalize model/capabilities and preserve original configuration                                          |
| `modulebom`, `moduleassembly`, `projectmodule` | `ProductConfiguration` / assembly snapshot       | Keep separate from device model and protocol                                                              |
| `body_part`, `body_action`, `body_position`    | `MovementTaxonomy`                               | Preserve source IDs, localization, hierarchy and validity                                                 |
| `trainingplan*`                                | `TrainingPlan*`                                  | Separate migration stream; not a blocker for measurement MVP                                              |
| `systemlog`                                    | `AuditEvent` or archived operational log         | Map only if audit semantics and actor identity are reliable                                               |

### 7.1 Migration lineage is mandatory

Every imported record should retain enough lineage to trace it back to the source:

- `SourceSystem` / legacy database name.
- `SourceTable` including shard suffix.
- `SourceRecordId`.
- Source company ID.
- Source created/updated timestamps.
- Import batch ID and import timestamp.
- Mapping version.
- Validation status and any warnings.

A unique constraint such as `(SourceSystem, SourceTable, SourceRecordId)` on a migration lineage table prevents accidental duplicate imports and supports re-runs.

## 8. PostgreSQL target design principles

1. Tenant ownership: organization-owned rows must have `OrganizationId` or a clearly enforced tenant-owned parent relationship.
2. Foreign keys: define FK constraints for core relationships; use composite tenant-aware constraints where they prevent cross-organization references.
3. Uniqueness: enforce uniqueness for organization membership, active team membership where appropriate, idempotency keys, source lineage and published catalog versions.
4. Delete behavior: default to restrictive deletes for historical measurement data. Prefer archival/soft deletion for business entities; do not cascade-delete scientific results or source assets casually.
5. Time: use UTC-aware `timestamptz` for instants. Store measurement-relative time separately with an explicit timebase and unit.
6. Numeric precision: choose `double precision` for high-resolution scientific floating-point values when appropriate; use `numeric` for exact decimal quantities where required. Document units.
7. JSONB: use only for versioned, validated configuration snapshots or genuinely variable structured data. Do not hide frequently queried domain fields in unvalidated JSON.
8. Large data: store high-rate raw series, video and large files in object storage; keep checksums, metadata and references in PostgreSQL.
9. Indexing: index tenant scope and common query paths, e.g. `(OrganizationId, AthleteId, MeasuredAt)` and `(OrganizationId, SessionId)`. Validate with actual query plans.
10. Naming: use one PostgreSQL naming convention (recommended snake_case for physical schema, consistent C# naming in code).
11. Catalog publication: global catalogs should be versioned and immutable once published; organization records should retain the catalog version used at measurement time.
12. Concurrency: use optimistic concurrency/version columns for mutable organization configuration and plan-like entities.

### 8.1 Suggested high-level relational constraints

Examples to refine during implementation:

- `OrganizationMembership`: unique `(OrganizationId, UserId)`.
- `TeamMembership`: unique active membership policy for `(OrganizationId, TeamId, AthleteId)`.
- `SessionParticipant`: unique session/athlete relationship unless repeated participation is a supported workflow.
- `SessionDevice`: unique `(OrganizationId, SessionId, DeviceId)`.
- `IngestionReceipt`: unique `(OrganizationId, DataSourceId, IdempotencyKey)`.
- `LegacyRecordMap`: unique `(SourceSystem, SourceTable, SourceRecordId)`.
- `ResultMetricValue`: index by `(OrganizationId, ResultId, MetricDefinitionId)`.
- `Trial`: index by `(OrganizationId, AthleteId, MeasuredAt)` or via session/participant access paths.

Do not add constraints until their business cardinality is confirmed. The purpose is to make the expected rules explicit and testable.

## 9. Prioritized delivery backlog

### Priority definitions

- P0 — Release/security foundation: required before real customer data or production pilot.
- P1 — First real measurement vertical slice: required to prove the platform's core value with at least one existing client.
- P2 — Multi-product normalization: required to onboard additional measurement products without redesign.
- P3 — Advanced platform capabilities: useful expansion after stable ingestion and core data contracts.

### Epic A — Tenant identity and authorization

| ID      | Priority | Backlog item                                              | Acceptance criteria                                                                                         |
| ------- | -------- | --------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| HUB-001 | P0       | Implement authentication and trusted organization context | API validates access tokens; organization context comes from authorized membership, not an untrusted header |
| HUB-002 | P0       | Implement organization membership and roles               | Membership lifecycle and role policies are persisted and tested                                             |
| HUB-003 | P0       | Enforce tenant-scoped resource access                     | Cross-tenant reads/writes fail for athlete, project, session, trial, result, asset and export               |
| HUB-004 | P0       | Add tenant isolation integration tests                    | Tests cover direct ID access, nested relationships, list filters, object storage and background jobs        |
| HUB-005 | P1       | Implement Athlete, Team and CoachAthleteRelationship      | Athlete/team CRUD and coach visibility are organization-scoped                                              |

### Epic B — Canonical measurement model

| ID      | Priority | Backlog item                                         | Acceptance criteria                                                                               |
| ------- | -------- | ---------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| HUB-010 | P0       | Define and approve canonical measurement terminology | Session, Trial, Result, DataObject and Asset have documented distinct meanings                    |
| HUB-011 | P1       | Implement MeasurementSession and SessionParticipant  | A session can represent a real test occasion and associate one or more athletes                   |
| HUB-012 | P1       | Implement Trial and product-specific TrialContext    | One session supports multiple trials; product context is structured and validated                 |
| HUB-013 | P1       | Implement SessionDevice and Device registration      | One session can include multiple devices; device identity and configuration are captured          |
| HUB-014 | P1       | Implement DataSource and idempotent IngestionReceipt | A client can retry a submission without duplicate sessions/trials/results                         |
| HUB-015 | P1       | Implement DataObject and Asset metadata              | Raw data, video and snapshots are represented with private storage references and checksums       |
| HUB-016 | P1       | Implement typed Result and ResultMetricValue         | Numeric, categorical, bilateral/dimensioned, structured and series-reference values are supported |
| HUB-017 | P1       | Implement ResultEvent / ResultSegment                | Time/sequence-positioned events retain type, position, timebase and source                        |
| HUB-018 | P1       | Implement ProcessingRun provenance                   | Result retains algorithm/version, input IDs/checksums and configuration snapshot                  |

### Epic C — Catalogs and versioning

| ID      | Priority | Backlog item                                      | Acceptance criteria                                                                               |
| ------- | -------- | ------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| HUB-020 | P1       | Implement MetricDefinition and metric units/types | Every submitted metric maps to a stable definition and explicit value type/unit                   |
| HUB-021 | P1       | Implement TestProtocol and immutable versions     | A trial references the exact published protocol version used                                      |
| HUB-022 | P2       | Implement DeviceModel catalog                     | Force plate and other device models have stable identities/capabilities                           |
| HUB-023 | P2       | Implement MovementTaxonomy                        | ROM body part/action/position hierarchy is represented with localization and versioning           |
| HUB-024 | P2       | Implement ProductConfiguration snapshots          | MuscleStrength assembly/BOM and other product setup are versioned separately from protocol/device |
| HUB-025 | P2       | Implement Platform-to-Hub catalog publication     | Hub receives versioned published catalog updates without direct cross-database reads              |

### Epic D — Legacy migration and interoperability

| ID      | Priority | Backlog item                                        | Acceptance criteria                                                                              |
| ------- | -------- | --------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| HUB-030 | P0       | Create legacy schema inventory and mapping register | All six schemas, shard families, views, key columns and unresolved semantics are documented      |
| HUB-031 | P1       | Build migration staging and lineage tables          | Every imported row is traceable to source DB/table/shard/ID and import batch                     |
| HUB-032 | P1       | Build athlete/project identity mapping              | Duplicate, missing and ambiguous athlete/project relationships are reported, not silently merged |
| HUB-033 | P1       | Build first product adapter/importer                | One selected product imports end-to-end with reconciliation counts and error report              |
| HUB-034 | P2       | Build remaining five product adapters               | Each product has explicit context/metric/event/media mapping and migration tests                 |
| HUB-035 | P2       | Normalize legacy sharded tables during import       | `_00`–`_09` sources load into logical target tables without reproducing the legacy shard layout  |
| HUB-036 | P2       | Add data-quality and reconciliation reports         | Counts, orphan references, missing assets, unknown enums and conversion errors are visible       |

### Epic E — Baselines, norms and training plans

| ID      | Priority | Backlog item                                    | Acceptance criteria                                                                                                               |
| ------- | -------- | ----------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| HUB-040 | P2       | Separate PersonalBaseline from NormativeDataset | User-selected references and population norms have different ownership and versioning                                             |
| HUB-041 | P2       | Migrate standard-data catalogs                  | Age/gender/context and metric mappings are validated and versioned                                                                |
| HUB-042 | P3       | Consolidate TrainingPlan model                  | PowerRack, MuscleStrength and Hamstrings plan structures map to a common plan model without losing product-specific action detail |
| HUB-043 | P3       | Link training and assessment history            | Plan execution can reference sessions/trials with explicit traceability                                                           |

### Epic F — Production readiness and operations

| ID      | Priority | Backlog item                                             | Acceptance criteria                                                                           |
| ------- | -------- | -------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| HUB-050 | P0       | Add EF Core migrations and deployment migration process  | Clean database can be provisioned and upgraded repeatably                                     |
| HUB-051 | P0       | Add PostgreSQL integration tests                         | CI runs against real PostgreSQL and validates constraints/migrations                          |
| HUB-052 | P0       | Remove development secrets from deployment configuration | No plaintext production secrets in source control; environment secret injection is documented |
| HUB-053 | P0       | Add API contract/build/test pipeline                     | Backend build, frontend type-check/lint, contract tests and security checks run in CI         |
| HUB-054 | P1       | Add observability and correlation IDs                    | Client ingestion can be traced across API, storage and worker processing                      |
| HUB-055 | P1       | Add backup/restore and object-storage recovery tests     | Recovery procedure is documented and exercised                                                |
| HUB-056 | P1       | Implement worker reliability patterns                    | Idempotency, retry policy, dead-letter handling and reconciliation are tested                 |

### Epic G — Web product integration

| ID      | Priority | Backlog item                                         | Acceptance criteria                                                                              |
| ------- | -------- | ---------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| HUB-060 | P0       | Select one maintained frontend                       | React or Vue is formally selected; the other is archived or explicitly designated non-production |
| HUB-061 | P1       | Replace mock services with typed Hub API client      | No production screen depends on mock repository data                                             |
| HUB-062 | P1       | Implement athlete and session vertical slice         | User can find an athlete, open a session, inspect trials and see real results                    |
| HUB-063 | P1       | Implement result visualization contract              | Common metric cards and specialized renderers can consume typed results                          |
| HUB-064 | P2       | Implement longitudinal comparisons and report export | Comparisons respect metric definition/version and organization permissions                       |

## 10. Recommended implementation sequence

The backlog should be delivered as vertical slices, not as a large up-front schema rewrite.

### Phase 0 — Decisions and safety baseline

1. Confirm the six product owners and domain experts who can explain legacy enums and payload formats.
2. Approve the canonical terms: Organization, Athlete, Project/Program, Session, Trial, DataObject, Asset, Result, Metric and Event.
3. Confirm the primary frontend.
4. Implement authentication, membership and tenant isolation.
5. Establish EF migrations, PostgreSQL CI and secret handling.

Exit criterion: no real customer data is exposed to an unauthenticated or cross-tenant caller; the schema can be provisioned and tested repeatably.

### Phase 1 — First production vertical slice

Select one product based on customer value and engineering readiness. SportsPerformance is a strong candidate if the immediate goal is to prove force-plate data, device metadata, metrics and event visualization; PowerRack or Hamstrings may be preferable if their current client integration is more stable.

Implement:

1. Athlete and organization context.
2. Session and trial.
3. Data source and idempotent ingestion.
4. One or more session devices.
5. Data object/asset references.
6. Typed result metrics and event/segment representation.
7. Real React/Vue UI integration.
8. End-to-end tenant and retry tests.

Exit criterion: an existing Kunwei client submits a real measurement, retries safely, and a coach can inspect the athlete's session and result in KWinHub.

### Phase 2 — Add a second, structurally different product

Choose a product that tests the model's generality. For example:

- ROM tests bilateral degrees and ordered items.
- Golf tests video, snapshots, time offsets and tags.
- MuscleStrength tests assembly configuration and indexed indicators.
- Hamstrings/PowerRack test bilateral force summaries and repeated peaks.

Do not choose a second product that is nearly identical to the first if the purpose is to validate the abstraction.

Exit criterion: the second product can be integrated through a product adapter and specialized renderer without changing the shared session/trial/result semantics.

### Phase 3 — Migrate the remaining products

- Build one adapter per legacy product.
- Import sharded tables into logical target tables.
- Normalize metric keys and units only after domain validation.
- Migrate assets and verify checksums.
- Produce reconciliation reports.
- Keep source IDs and lineage for audit and rollback.

Exit criterion: all six products have an approved mapping, measurable migration completeness and a documented exception process.

### Phase 4 — Advanced platform capabilities

- Personal baselines and normative datasets.
- Training plan consolidation.
- Richer longitudinal analytics and reporting.
- Additional integrations.
- Optional cloud processing only after a separate product and scientific-computation ADR.

## 11. Explicit non-goals and design guardrails

To keep the project focused:

1. Do not recreate each legacy database as a separate KWinHub schema. That would preserve the fragmentation.
2. Do not flatten all product data into a single generic JSON blob. It would weaken validation, querying, analytics and scientific provenance.
3. Do not force all measurements into one fixed set of columns. Use shared lifecycle entities plus typed metrics and product-specific context/details.
4. Do not make Project synonymous with MeasurementSession. Confirm the old product semantics first.
5. Do not make PersonalBaseline synonymous with NormativeDataset.
6. Do not treat a device model, physical device, assembly and test protocol as the same entity.
7. Do not put large raw force-time series or video in ordinary relational metric rows.
8. Do not reproduce ten-shard tables in PostgreSQL by default.
9. Do not silently overwrite old results when algorithms or metric definitions change.
10. Do not move scientific calculation into Hub as an assumed consequence of data consolidation. Current source applications remain the calculation authority unless a separate ADR approves a different model.
11. Do not introduce microservices, Kafka or Kubernetes merely to solve a schema-consolidation problem.
12. Do not migrate ambiguous legacy values without preserving the original value and recording the mapping decision.

## 12. Decisions required before implementation

These are the decisions that need product-owner or engineering confirmation:

| Decision                                                 | Why it matters                                                                    | Recommended default                                                       |
| -------------------------------------------------------- | --------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| What exactly is a legacy Project?                        | Determines whether it maps to Project, AssessmentProgram, Team or another concept | Preserve as a distinct grouping entity until confirmed                    |
| Can one Session contain multiple athletes and devices?   | Determines SessionParticipant and SessionDevice cardinality                       | Support multiple participants and devices                                 |
| Does one legacy Record represent a session or one trial? | Critical for migration correctness                                                | Map product by product; default to Trial unless workflow proves otherwise |
| Who owns athlete identity across organizations?          | Privacy, duplication and tenant isolation                                         | Organization-scoped Athlete records initially                             |
| What are all enum values and units?                      | Scientific validity and data conversion                                           | Extract from application source and real data; no guessed mappings        |
| What are RawData/FileCode/FilePath payload contracts?    | Determines object storage and migration                                           | Preserve as opaque source value until verified                            |
| Are references personal baselines or other statistics?   | Comparison semantics                                                              | Separate PersonalBaseline and NormativeDataset                            |
| Are training plans in the first release scope?           | Scope and migration effort                                                        | Defer consolidation to P3 unless required by a live workflow              |
| Is cloud scientific recalculation approved?              | Changes scientific authority and architecture                                     | No; keep source-app calculations authoritative for current scope          |

## 13. Final recommendation

The six databases point to a clear architectural opportunity:

KWinHub should unify the identity, organization, testing lifecycle, data assets, results, metric definitions and longitudinal history, while preserving product-specific scientific meaning.

The most important first deliverable is not a perfect universal schema. It is a secure, versioned and idempotent data contract that can accept one real measurement workflow end to end, retain its raw-data/media provenance, and display its results correctly.

A second structurally different product should then validate the abstraction before the remaining legacy systems are migrated.

The recommended sequence is:

Tenant security → canonical Session/Trial/Result contract → first real client integration → second-product validation → six-product migration → baselines/norms/training consolidation.

This gives KWinHub a stable shared foundation without prematurely turning the project into a universal scientific-computation platform.

## Appendix A — Suggested implementation artifacts

Before coding the full model, create and review:

1. `legacy-schema-inventory.xlsx` — all source tables, columns, types, indexes, shard families and views.
2. `legacy-enum-dictionary.md` — every tinyint/int/string code and its confirmed meaning.
3. `legacy-payload-contracts.md` — `RawData`, `FileCode`, `FilePath`, `DeviceInfo`, `CameraInfo`, `KeyPointInfo` formats.
4. `canonical-domain-glossary.md` — agreed definitions for Project, Session, Trial, Result, Event, Metric and Baseline.
5. `legacy-to-hub-mapping.xlsx` — per-product field mapping, transform rule, validation and exception policy.
6. `data-migration-runbook.md` — staging, dry run, reconciliation, cutover, rollback and audit steps.
7. `measurement-ingestion-contract.md` — versioned API request/response, idempotency, asset upload and error handling.
8. ADRs for athlete identity scope, project semantics, metric value typing, asset storage and scientific calculation authority.

These artifacts should be version-controlled alongside the KWinHub architecture documentation.

## Appendix B — Recommended immediate engineering priorities

If the team needs a shorter actionable starting list, I recommend the following order:

| Order | Work item                                                           | Why now                                                     |
| ----- | ------------------------------------------------------------------- | ----------------------------------------------------------- |
| 1     | Confirm canonical Session, Trial, Result and DataObject semantics   | Prevents foundational model ambiguity                       |
| 2     | Finish authentication, organization membership and tenant isolation | Required before production customer data                    |
| 3     | Define MetricDefinition and typed ResultMetricValue contracts       | Needed to integrate more than one product                   |
| 4     | Define idempotent ingestion and source lineage                      | Prevents duplicate records and makes retries/migration safe |
| 5     | Implement the first product end-to-end                              | Proves the model against real client data                   |
| 6     | Implement asset/raw-data metadata and storage references            | Preserves original scientific evidence and associated media |
| 7     | Integrate a structurally different second product                   | Validates whether the shared model is genuinely reusable    |
| 8     | Complete remaining product adapters and migration                   | Scales the platform after the abstractions are proven       |
| 9     | Consolidate personal baselines, norms and training plans            | Adds higher-level capabilities without blocking ingestion   |

My strongest recommendation: do not start by implementing every proposed entity. First approve the domain glossary and the first product's complete ingestion contract. The second product should then test and refine that contract before it becomes the permanent foundation for all six products.

End of document.

If you want to take this into implementation next, I can turn the backlog into a GitHub-ready epic/story breakdown with dependencies and a proposed release sequence.