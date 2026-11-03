# KWinHub MVP — 产品需求对齐、项目改造分析与下一阶段实施计划

**文档类型：** 产品与研发对齐 / 项目改造分析 / 阶段实施计划  
**版本：** V1.1  
**状态：** 评审后执行基线（PRD 与排期仍需 G0 冻结）  
**日期：** 2026-11-03（双 API 架构决策修订）  
**适用项目：** `kongdf123/KWinHub.MVP`  
**主要输入：** KWinHub MVP PRD V1.2 修订 2、KWinHub 项目全景与一期流程图册 V0.3、现有仓库代码审阅及项目讨论  
**已确认决策：** Option A——PlatformConsole 负责平台级全局管理，Web.React 负责面向机构客户的 Hub 业务。

> 本文用于解释“为什么改、改什么、先改哪里、如何验收”。需求依据以 PRD V1.2 为准；双 API 边界及其理由以 ADR-0001 为准；架构图册用于解释目标边界；代码现状描述仅代表本轮检查到的内容，实施前须再次对照默认分支核实。建议方案、推断和待决策项不会自动变成已批准需求。

## 1. 执行摘要

KWinHub V1.0 的改造重点不是重写现有 MVP，也不是先把所有前端页面做完整，而是将当前原型逐步改造成一条真实、可靠、可授权的业务闭环：

**iOS 2.0 完成测试和科学计算 → KWinHub 接收会话/试次/指标 → 文件和视频独立上传 → Web.React 查阅人员历史与结果 → 选择测试方案并生成报告 → 对 Ready 视频进行授权在线播放。**

建议保留现有 .NET、PostgreSQL、Worker 和两个前端项目，但后端明确演进为两个独立 API 项目：Platform.Api 服务 PlatformConsole，Hub.Api 服务 Web.React 与获准客户端。先建立两套 API 的责任、契约、身份策略和数据所有权，再贯通真实数据；不将这一决定扩大为全面微服务化，也不重建所有页面。

### 1.1 已确认的产品边界

| 组件 | V1 职责 | 不承担的职责 |
|---|---|---|
| 现有采集 App（首个为运动功能表现 iOS 2.0） | 设备连接、采集、校准、本地保存、既有算法计算、指标/App 报告输出；按独立队列同步结构化数据和附件 | 不依赖 Hub 在线才能完成本地测试 |
| **KWinHub.Web.React** | 面向机构客户的业务工作台：机构成员、人员/分组、测试记录、历史、基础比较、方案、报告、同步状态、授权视频播放 | 不承担平台运营人员的全局租户管理 |
| **KWinHub.PlatformConsole** | 内部平台运营：机构生命周期、平台用户、全局目录、审计、运行状态；订阅功能视 V1 商业需求决定 | 不复制客户的运动员、测试、报告工作流 |
| `KWinHub.Platform.Api` | 平台级机构生命周期、平台用户/角色、全局目录、平台审计与运营治理 | 不承接机构客户日常运动员、测试、报告和视频业务 |
| `KWinHub.Hub.Api` | 机构成员/角色、人员/分组、测试接入、指标、资产媒体、历史、方案报告、导出与播放授权 | 不承担全局机构生命周期和平台运营治理 |
| KWinHub.Workers | 媒体处理、报告等可靠异步任务 | V1 不作为 App 科学指标的云端权威计算引擎 |

### 1.2 已确认的核心技术/产品原则

- 一期科学指标由 App 计算；Hub 负责存储、校验、展示和有限的基础统计。
- Session/Trial/Metric 与 Raw Data/App Report/Video 采用可独立恢复的上传流程；视频失败不应导致会话或指标不可用。
- 视频纳入一期上传和在线报告播放范围，但格式、大小、时长、保留、并发和费用仍需冻结。
- 约 300 家机构、1,000 人同时在线是容量规划输入，不是已经验证的性能指标；必须分别建模 API 请求、上传并发、播放并发和后台吞吐。
- 推荐模块化单体、PostgreSQL、私有 OSS、托管 MQ、Transactional Outbox 和独立 Worker；具体云产品、地域、规格和成本需 PoC 验证。
- 2027 年 2 月底是从会议表述推定的目标窗口，需确认展会日期、资源投入和交付定义。

## 2. 现有 MVP 的定位与改造原则

### 2.1 不推倒重来

当前代码已经包含可继续演进的项目结构：ASP.NET Core API、Domain/Application/Infrastructure/Contracts、PostgreSQL/EF Core、Worker，以及 PlatformConsole 和 Web.React。已有的 Subject、MeasurementSession、Result、Metric 等概念可作为起点。

建议采用“保留有价值的结构，按真实业务契约补齐”的方式：

1. 保留 .NET 后端和 PostgreSQL，先做领域模型、授权、迁移和测试补强。
2. 保留 Web.React 作为客户主工作台，优先把关键页面从 Demo 数据切换到真实 API。
3. 保留 PlatformConsole，但收敛到平台运营职责。
4. 保留 Worker 项目，调整生产职责为媒体/报告异步处理；科学结果模拟仅作为 Demo/Test Fixture。
5. 用真实 iOS payload 校验领域模型，不根据当前 UI Demo DTO 反向定义数据库。

### 2.2 本轮检查到的主要改造风险

以下是代码审阅时观察到的风险，不代表所有情况均未实现；开始实施前要再次检查最新代码。

| 领域 | 本轮观察 | 改造方向 |
|---|---|---|
| PlatformConsole | API client 的 mock 开关默认值为 true | 生产构建必须 API-only；Mock 仅用于本地开发、演示或测试 |
| Web.React | service layer 支持 Demo 数据模式 | 生产环境不得因 API 失败而静默回退 Demo 数据 |
| Web.React 数据映射 | 部分 mapper 会为缺失字段补默认值 | 不得伪造性别、单位、指标值、试次数或质量状态；缺失应显式呈现 |
| API 身份 | 已存在 JWT Bearer 注册和认证中间件 | 仍需确认身份提供方、密钥管理、Token 生命周期和生产配置 |
| 租户隔离 | SubjectsController 已采用当前用户上下文做机构范围约束 | 需逐项审计其他 API、关联资源、导出、文件、视频及 Worker |
| 科学 Worker | 当前 Worker 路径包含 ResultSimulator | 生产路径不应生成与 App 权威结果冲突的科学指标 |
| 领域模型 | Session/Result/Metric 已有基础结构 | 对照真实 payload 评估 Trial、SessionDevice、MetricDefinition、ResultMetricValue、DataAsset 等扩展 |
| 数据库 | PostgreSQL/EF Core 已有基础 | 明确 FK、唯一约束、索引、删除策略和可重复迁移 |

### 2.3 需要避免的两种改造误区

**误区 A：先完成所有前端页面，再接后端。**  
这样会让 Demo DTO、假数据和 UI 默认值固化为错误的业务契约。应优先完成一个真实纵向切片。

**误区 B：一次性实现完整 SaaS 平台。**  
订阅计费、复杂组织树、BI 大屏、常模库、AI 分析、第三方开放平台和全球多区域部署不应挤占一期真实数据闭环的资源。

## 3. 两个前端的最终职责与信息架构

Option A 已确认，建议把边界写入架构文档、导航、API Policy 和验收用例。

### 3.1 PlatformConsole：内部全局平台管理

建议导航：

- Dashboard
- Organizations
- Platform Users
- Global Catalogs
- Security & Audit
- Operations
- Subscriptions（仅在确认一期需要商业套餐/配额时启用）

主要职责：

- 创建、启用、停用和管理机构元数据。
- 管理平台级用户和受控开通流程。
- 维护全局 Protocol、Metric、Device 等目录及版本元数据。
- 查看平台级审计和任务运行状态。
- 在最小权限原则下处理故障；运维角色不默认获得所有运动员原始数据访问权。

### 3.2 Web.React：机构客户 Hub

建议导航：

- Overview
- Athletes
- Testing
- Analysis
- Plans & Reports
- Data & Synchronization
- Organization Settings

主要职责：

- 当前机构成员和业务角色。
- 人员档案、分组及跨 App 身份映射。
- 测试会话、试次、指标、来源和附件状态。
- 人员历史、基础指标比较和可视化。
- 选择 AssessmentPlan 版本、确认入选数据、校验缺项、生成报告。
- 对有权限且 Ready 的视频进行在线播放。
- 机构自己的设置与授权管理。

### 3.3 双 API 后端边界与共享规则

两个前端不共享同一个后端 API 项目。PlatformConsole → Platform.Api；Web.React/客户端 → Hub.Api。两个 API 可共享身份提供方、日志/追踪基础设施和小型 BuildingBlocks，但各自拥有独立的 API Host、OpenAPI、audience/scope/authorization policies、配置、健康检查、限流、发布流水线和回滚策略。

Platform.Api 是机构全局生命周期状态的权威写入方；Hub.Api 是机构内业务数据和成员授权的权威写入方。Hub.Api 服务端必须检查机构是否有效及用户当前成员资格。机构启停通过版本化 Platform→Hub 事件、Transactional Outbox、幂等消费、重试和对账同步；关键停用路径按生效时限增加权威状态核验，不依赖可能过期的缓存。

初期可共用一个 PostgreSQL 集群，但使用独立 schema、DbContext、迁移和数据库账号/写权限。禁止跨上下文直接写表、共享巨型 DbContext 或使用跨服务分布式事务。各上下文内部使用本地 ACID 事务；跨上下文采用最终一致性。前端隐藏菜单不是安全控制，所有操作必须由对应 API Policy 和资源归属校验保护。

## 4. 产品需求到技术实现的关键映射

| PRD 业务能力 | 后端/数据能力 | Web.React | PlatformConsole |
|---|---|---|---|
| FR-01/02 账号、机构、权限 | User、OrganizationMembership、Role/Policy、CurrentUserContext | 登录、机构切换、机构成员 | 平台用户、机构生命周期 |
| FR-03 人员档案与身份映射 | Subject、ExternalIdentity/Source mapping | 人员列表、详情、映射冲突处理 | 必要时受控运营支持 |
| FR-04 分组/团队 | Team、TeamMembership | 分组和成员管理 | 不负责日常业务分组 |
| FR-05 协议/方案 | 版本化 TestProtocol、AssessmentPlan | 选择方案、查看可用版本 | 全局目录和受控发布 |
| FR-06/07 数据接入与文件 | Session、Trial、ResultMetricValue、DataAsset、Idempotency | 测试详情、逐项同步状态 | 运行状态与失败任务 |
| FR-08 历史与详情 | 查询 API、租户范围 | 人员历史、Session 详情 | 不作为客户历史入口 |
| FR-09 基础统计与比较 | 兼容性校验、聚合查询 | 比较表、基础图表 | 平台级不做业务分析 |
| FR-10 报告 | AssessmentPlan、Report snapshot、Export job | 在线报告、导出、视频播放器 | 运行与审计视图 |
| FR-11/12 治理与审计 | AuditEvent、授权/脱敏/删除流程 | 机构授权范围及用户可见状态 | 平台审计与受控操作 |
| FR-13 迁移 | Mapping、批次、校验、对账 | 迁移后的业务数据 | 运营状态与审计 |
| FR-14 易用性/展会闭环 | 真实端到端流程和错误恢复 | 业务流程 | 支持运营，不替代客户入口 |
| FR-15 容量/可靠性 | Outbox、MQ、Worker、Observability | 可理解的同步/媒体状态 | 队列、失败、运行监控 |

具体实体与 API 字段须以真实 iOS payload、PRD 和技术设计评审结果为准；此表是能力映射，不是最终数据库设计。

## 5. 建议的核心数据与流程改造

### 5.1 领域模型

建议重点验证以下概念，而不是立即机械增加所有表：

- **Organization / User / OrganizationMembership：** 机构租户、账号与成员关系。
- **Subject：** Hub 内统一人员身份；前端可显示为 Athlete。
- **ExternalIdentity：** App/外部系统的人员 ID 映射，处理离线新建和冲突。
- **Team / TeamMembership：** 机构内分组及成员关系。
- **TestProtocol：** 单项测试及其不可变版本。
- **AssessmentPlan：** 组合多个测试/指标并定义选数、完整性和报告规则的版本化方案。
- **MeasurementSession：** 一次测试事件。
- **Trial：** Session 中的单次尝试。
- **SessionDevice：** 当一次测试涉及多个设备时表达设备关系。
- **MetricDefinition / ResultMetricValue：** 指标定义与实际测量值分离。
- **DataAsset：** 原始文件、App 报告、视频及派生媒资的元数据和状态。
- **ProcessingJob：** 媒体/报告异步任务状态和幂等键。
- **Report：** 方案版本、入选源数据和生成快照。
- **AuditEvent：** 敏感管理、授权、下载和导出行为。

### 5.2 结构化数据与文件双链路

1. App 本地完成测试和计算，并持久化会话、试次、指标和文件清单。
2. App 提交结构化 Session/Trial/Metric；API 校验机构、人员映射、协议、版本和幂等键。
3. API 事务持久化数据并返回 Hub ID 与资产占位信息。
4. Web.React 可在指标接受后查询和展示结果。
5. App 对每个资产分别申请短时上传授权，直传私有 OSS。
6. 服务端核验对象、大小、元数据和适用校验值。
7. 服务端在同一事务写入 UploadedVerified 与 Outbox 事件。
8. Relay 投递 MQ；Worker 幂等处理 VOD/媒资任务并更新状态。
9. API 在用户访问报告或视频时重新检查权限，Ready 视频才签发短时播放凭证。
10. Callback 丢失或乱序由主动对账补偿；重复消息允许发生，但不能产生重复有效资产/任务版本。

### 5.3 科学计算与统计边界

- App 是一期科学指标计算的权威来源。
- Hub 可以执行经批准的基础统计，例如平均值、最大值，并显示样本数和选取规则。
- Hub 不从原始力信号重新计算跳高、事件检测、动作识别或 AI 结论。
- 协议、指标定义、算法版本和输入来源必须可追溯；新版本不能静默改写历史结果。
- 视频转码、封面生成和媒体分发属于媒体处理，不等于科学计算。

### 5.4 方案与报告

区分单项 TestProtocol 与组合 AssessmentPlan。第一期先落地一个真实展会方案：

选择方案版本 → 选择人员和时间范围 → 选择候选 Session/Trial → 校验必需指标与兼容性 → 明确缺项 → 预览 → 生成报告快照 → 在线查看/导出。

缺失值不得按零处理；不兼容指标不得静默混算；报告应保存方案版本和精确的源记录引用。非 Ready 视频不能阻断有效指标和报告内容的查看。

## 6. 推荐技术架构与实现策略

### 6.1 保留当前技术路线

- `KWinHub.Platform.Api` 与 `KWinHub.Hub.Api` 两个独立 ASP.NET Core API 项目；各自内部按 Domain/Application/Infrastructure 模块化组织。
- PostgreSQL + EF Core 作为事务数据源。
- 私有 OSS 保存原始文件与视频。
- Managed MQ 作为异步投递通道。
- Transactional Outbox 保障 DB 状态与待发事件一致。
- 独立 Worker 负责媒体/报告处理。
- VOD/CDN 负责媒体处理和分发，具体产品与地域须验证。
- Platform.Api 与 Hub.Api 分别维护 OpenAPI；PlatformConsole 和 Web.React 各自生成/维护对应 typed TypeScript client，禁止误用对方 API 契约。
- Platform→Hub 使用版本化事件契约、Outbox、幂等消费者和对账；不使用分布式事务。
- OpenTelemetry、结构化日志、指标、追踪、告警与健康检查。

### 6.2 暂不建议的复杂化

除非压测或运维要求证明必要，否则一期不引入 Kafka、Kubernetes、把每个业务模块继续拆成微服务、全局分布式事务、通用 BI 引擎或科学算法云端迁移。注意：Platform.Api 与 Hub.Api 两个独立 API 项目是已选定的应用边界，并不等于全面微服务化。Redis/分布式缓存不是双 API 的必选项，只在缓存、分布式限流或热点读等明确场景有实测收益时引入，不作为核心业务事实源。

### 6.3 生产配置要求

- Development/Mock 与 Production/API 模式明确隔离。
- 生产构建不允许 API 失败时回退假数据。
- 密钥与连接字符串使用环境注入/托管密钥，不提交真实凭证。
- 数据库迁移可重复、可审计，部署流程有回退方案。
- 对象存储和媒体链接均为私有、短时授权；不能依赖前端隐藏 URL。

## 7. 分阶段实施计划

以下日期沿用 PRD V1.2 建议窗口。它们是规划基线，不是已批准工期。

### G0 — 需求、契约与决策冻结（2026-11-03 至 11-30）

**目标：** 把“应该做什么”转成可开发、可联调、可验收的契约。

- 完成 FR/AC 到代码和测试的 Gap Matrix。
- 选定真实 iOS 2.0 展会/试点方案及报告样例。
- 收集并脱敏真实 Session/Trial/Metric/Asset payload。
- 确认认证源、机构角色矩阵、资源权限范围。
- 确认 Session/Trial/Metric/Asset API、幂等、错误码和状态查询。
- 冻结视频格式、大小、时长、上传策略、保留与播放并发目标。
- 验证 OSS、VOD、SMQ 的 SDK、地域、配额、回调和费用。
- 明确 PDF/CSV/XLSX 范围、订阅功能是否纳入、迁移执行范围。
- 建立 CI、真实 PostgreSQL 测试和第一批验收用例。

**退出条件：** 产品、App、后端、前端、测试共同评审通过同一套 payload、接口、角色、方案/报告样例及 P0 验收清单。

### G1 — 真实数据闭环（2026-12）

**目标：** 真实 iOS 数据可上传、存储、查询、展示和恢复。

- 登录、机构上下文、成员和服务端租户授权。
- 人员/外部身份映射与基础分组。
- Session/Trial/Metric API、幂等、状态查询和持久化。
- Web.React 人员/测试/结果关键路径切换真实 API。
- 逐资产授权上传、服务端核验、状态跟踪。
- Outbox/MQ/Worker 的第一版媒体处理链路。
- 跨机构、重复提交、断网恢复和文件失败测试。

**退出条件：** 一个真实 iOS 测试可上传并在 Web.React 中查看；重试不重复；视频未完成时指标仍可访问；租户隔离通过。

### G2 — 历史、比较与方案报告（2027-01）

**目标：** 客户能从真实数据得到可追溯的报告和媒体体验。

- 人员历史、Session 详情、Trial/Metric 展示。
- 兼容指标比较与基础统计/图表。
- AssessmentPlan 版本、选数、缺项校验。
- 在线报告、批准的 PDF/CSV 导出。
- VOD 处理、授权播放、报告内播放器和状态提示。
- PlatformConsole 所需的机构/用户/目录/审计管理路径。
- 迁移样本验证和报告数值核对。

**退出条件：** 授权用户可完成选定方案、预览、生成报告、导出，并播放 Ready 视频；缺项/失败状态准确。

### G3 — 安全、恢复与试点（2027-02）

**目标：** 形成可演练、可运维的试点版本。

- 全量 P0 权限与跨租户测试。
- MQ 不可用、Outbox 重投、Worker 崩溃、VOD 模糊结果、回调丢失/乱序测试。
- 负载、上传恢复、视频播放并发和存储容量验证。
- 备份恢复和代表性迁移/切换演练。
- UAT、操作手册、支持流程、展会/客户演练。
- 缺陷修复与风险接受记录。

**退出条件：** 冻结 P0 验收通过，关键故障可恢复，运维和产品团队共同签署试点放行。

## 8. 第一个月的执行重点

| 周次 | 时间 | 后端/集成 | Web.React | PlatformConsole/QA |
|---|---|---|---|---|
| W1 | 11/03–11/09 | 盘点 API/领域；认证与租户决策；收集 payload | 页面与 PRD 对照；识别 Demo 依赖 | 明确平台管理边界；Mock/API 盘点；测试矩阵 |
| W2 | 11/10–11/16 | Session/Trial/Metric/Asset 契约、幂等规则 | 登录壳、API client、导航和类型 | 平台权限策略；管理员 API 盘点 |
| W3 | 11/17–11/23 | EF Migration、租户集成测试、首个 ingestion endpoint | 人员列表/详情接入真实 API | 机构/用户管理接入可用 API；CI 检查 |
| W4 | 11/24–11/30 | 完成首条数据闭环与资产契约，G0 评审 | 展示真实 Athlete/Session/Metric | 平台权限验证、跨租户测试 |

每周都应演示可运行软件和测试证据，而不是只展示静态页面。

## 9. 风险与应对

| 风险 | 影响 | 应对 |
|---|---|---|
| iOS payload 未冻结 | API 和数据模型返工 | 用真实脱敏样本先行评审；契约变更版本化 |
| 身份/租户边界不完整 | 跨机构数据泄漏 | API 资源级授权测试覆盖所有读写/文件/导出/播放路径 |
| Demo fallback 混入生产 | 用户看到虚假数据或误判同步成功 | 生产 API-only；失败显式呈现 |
| 科学计算职责混淆 | Hub 与 App 结果不一致 | App 权威计算；Hub 只做批准的基础统计 |
| 视频与 Session 强耦合 | 视频失败导致测试不可用 | 双链路、逐资产状态和独立重试 |
| MQ/Worker/VOD 外部副作用 | 丢任务或重复媒资 | Outbox、任务唯一键、状态对账、回调幂等 |
| 报告缺项/混算 | 报告误导专业用户 | 方案版本、显式选数、单位兼容和缺项校验 |
| 迁移范围膨胀 | 挤压 V1 核心开发 | 先盘点与样本验证；全量切换另设门槛 |
| 1,000 并发定义模糊 | 规格与成本估算失真 | 分开建模 API、上传、播放、Worker 吞吐 |
| 时间窗口过于乐观 | G2/G3 质量下降 | 每周演示、持续 QA、P0 优先和范围变更记录 |

## 10. 必须关闭的决策

| 决策 ID | 内容 | 建议初始方向 | 建议完成时间 |
|---|---|---|---|
| D-01 | 身份认证源 | 优先评估托管 OIDC/OAuth；自建方案需安全评审 | 11/09 |
| D-02 | 首套展会方案 | 一套真实 iOS 2.0 测试和报告样例 | 11/09 |
| D-03 | iOS 数据契约 | 使用真实脱敏 payload | 11/16 |
| D-04 | 角色与数据范围 | 平台管理员、机构管理员、教练/科研、操作员；确认分组权限 | 11/16 |
| D-05 | 视频规格 | 格式、大小、时长、保留、上传恢复和并发播放 | 11/16 |
| D-06 | 导出格式 | PDF + CSV 作为候选基线；XLSX 单独确认 | 11/23 |
| D-07 | 阿里云产品 | OSS/VOD/SMQ PoC，验证地域、SDK、回调、配额与成本 | 11/23 |
| D-08 | 订阅中心 | 无明确商业试点要求则延期 | 11/30 |
| D-09 | 迁移范围 | 区分迁移盘点/样本演练与实际客户切换 | 11/30 |
| D-10 | 容量目标 | 把 300 机构/1,000 在线拆为 API、上传、播放、Worker 场景 | 11/30 |

## 11. 项目管理与交付方式

建议在 GitHub 建立四个 Milestone：

- `G0 — Scope & Contracts — 2026-11`
- `G1 — Real Data Loop — 2026-12`
- `G2 — Review & Reports — 2027-01`
- `G3 — Pilot Readiness — 2027-02`

Issue 按 Epic 建立父项，再拆为可独立验收的子项。每项必须包含目标、范围/非范围、依赖、涉及项目/文件、验收标准、测试证据和文档更新。建议标签：`epic`、`priority:P0`、`priority:P1`、`area:backend`、`area:web-react`、`area:platform-console`、`area:ios-integration`、`area:media-worker`、`area:qa-security`、`blocked`、`decision-required`。

产品、开发和测试共同评审关键流程。需求变更记录原因、影响、版本和排期，不允许 App、API 和前端分别采用不同的隐含规则。

## 12. V1.0 试点完成定义

| 验收领域 | 必须证明 |
|---|---|
| 身份与租户 | 真实登录、机构成员校验、跨租户请求被拒绝 |
| 结构化数据 | 真实 iOS Session/Trial/Metric 上传、持久化、查询 |
| 幂等恢复 | 超时、重复请求、断网、App 重启不会产生重复逻辑记录 |
| 科学数据 | 单位、指标定义、协议/算法版本与来源可追溯；Hub 不静默科学重算 |
| 资产 | 原始文件、App 报告、视频独立上传和服务端核验 |
| 媒体 | Ready 视频授权播放；失败/处理中视频不阻断有效指标 |
| 历史与比较 | Web.React 使用真实数据并遵守指标兼容规则 |
| 报告 | 指定方案版本、选数来源、缺项、在线报告和批准导出均可追溯 |
| PlatformConsole | 平台运营角色能执行批准的全局管理操作 |
| 运维 | 监控、告警、备份恢复、任务重放和试点手册通过演练 |

## 13. 文档关系与维护规则

建议将本文与《KWinHub MVP V1.0 — Detailed Implementation Backlog》作为一组项目执行文档：

- 本文回答产品需求如何映射到现有项目、应用边界、改造原则、阶段计划和风险。
- Detailed Implementation Backlog 负责 E0–E8 的具体工作项、优先级、验收标准、Issue 模板和 Definition of Done。
- PRD V1.2 负责产品需求、范围和验收基线。
- 项目蓝图 V0.3 负责产品/系统/业务流程的图示解释。
- API、数据模型和部署设计文档负责具体技术契约，不由本计划替代。

当 PRD、蓝图、API 契约或实现发生冲突时，记录差异并由产品/技术负责人评审；不要在某个子团队内静默覆盖其他文档。每次批准的范围变更同步更新 PRD、Backlog、接口契约和验收用例。

**当前状态说明：** 本文是基于现有资料和项目讨论形成的改造计划，不代表已创建 GitHub Issues、修改业务代码、完成云产品采购或执行构建/测试。
