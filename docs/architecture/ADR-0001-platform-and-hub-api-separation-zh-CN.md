# ADR-0001：Platform API 与 Hub API 分离

- **状态：** Accepted（项目架构决策；实施细节进入 G0 冻结）
- **日期：** 2026-11-03
- **决策范围：** KWinHub 后端应用边界、API Host、身份授权和数据所有权
- **关联文档：** PRD V1.2 §8/§9；项目蓝图 V0.2 §技术架构总图、图 2；MVP 对齐计划 §3/§6；实施 Backlog E0/E1/E8

## 1. 背景

KWinHub 有两个目的不同的前端：`KWinHub.PlatformConsole` 面向内部平台运营，负责全局机构生命周期和平台治理；`KWinHub.Web.React` 面向机构客户，负责机构内运动表现业务。两者的用户群、授权域、API 生命周期、数据访问范围和业务变更节奏不同。将两者接入同一个后端 API 项目，会使平台管理与客户业务边界依赖运行时约定，增加策略误配、部署耦合和权限面扩大的风险。

## 2. 决策

建立两个独立的 ASP.NET Core API 项目和部署单元：

| API | 调用方 | 数据/业务所有权 |
|---|---|---|
| `KWinHub.Platform.Api` | `KWinHub.PlatformConsole` | 平台级机构生命周期、平台用户与角色、全局目录/版本元数据、平台审计和运营治理 |
| `KWinHub.Hub.Api` | `KWinHub.Web.React`、获准客户端 App | 机构成员与业务角色、人员/分组、测试协议使用、会话/试次/指标、资产/媒体、历史、方案/报告、导出与播放授权 |

调用边界：PlatformConsole 只调用 Platform.Api；Web.React 和获准客户端只调用 Hub.Api。两个 API 独立构建、配置、发布、回滚和扩容，各自拥有 OpenAPI、健康检查、限流、日志/指标/追踪和授权策略。

## 3. 领域与数据所有权

- Platform.Api 是机构全局生命周期状态的权威写入方。
- Hub.Api 是机构内业务数据、成员关系和资源授权的权威写入方。
- Hub.Api 每次访问机构资源时，在服务端校验当前成员资格、机构状态和资源归属。
- 初期允许共用一个 PostgreSQL 集群，但必须分离 Platform/Hub schema、DbContext、迁移、数据库账号及写权限。
- 各上下文只能写入自己拥有的表；禁止跨上下文直接写表、共享巨型 DbContext、跨上下文 Repository 或跨库事务。
- 可共享身份提供方、日志/追踪、消息 envelope 和小型 BuildingBlocks；不得共享领域实体或把业务规则放进通用共享库。

## 4. 跨上下文集成

机构创建、启用、停用等 Platform 生命周期变更，通过版本化事件从 Platform 传递到 Hub。Platform 在本地事务中同时提交状态变更和 Outbox 记录；Relay 发布事件；Hub 消费端以事件 ID/版本幂等处理，记录消费结果并支持重试、死信/失败处理和定期对账。

机构停用必须有明确的生效时限。Hub 可维护机构状态读模型以避免每个请求跨服务调用；但高风险操作不得仅依赖可能过期的读模型或缓存。按安全要求对关键路径做权威状态核验或可靠的失效/版本机制。

不采用分布式事务。各 API 在本地事务内保证自身数据一致；跨上下文最终一致，并对失败提供可观测、可重试和可对账的恢复路径。

## 5. DDD、微服务、缓存与事务取舍

- **轻量 DDD：采用。** 按 Platform 与 Hub bounded context 组织领域模型、应用服务和基础设施；不要求完整战术 DDD、CQRS 或 Event Sourcing。
- **微服务：不全面采用。** 两个 API 项目是有意设置的独立应用边界，但各自内部仍可采用模块化单体；不为每个领域模块拆服务。媒体/报告 Worker 是独立后台进程，不因此自动成为独立业务微服务。
- **Redis/分布式缓存：非必需。** 只有热点读、分布式限流或协调等场景经测量证明有价值时再引入。明确 TTL、失效、故障降级和安全撤权语义；缓存不作为业务事实源。
- **事务：本地事务优先。** 会话/试次/指标、资产状态与 Outbox 等同一所有权边界内的变更使用本地 ACID 事务；OSS/VOD/MQ 等外部副作用通过持久化工作流、幂等、重试和对账协调。禁止全局分布式事务。

## 6. 后果

**正面：** API 与授权责任清晰；前端和后端可独立发布；减少平台运营权限与机构业务数据混用；为未来独立扩容和区域化部署保留空间。

**成本/约束：** 需要维护两套 API Host、OpenAPI、身份策略、部署配置、健康检查和 CI；需要定义 Platform→Hub 事件契约；数据迁移必须逐步拆分所有权；共用基础库必须克制。

**明确不承诺：** 两个 API 不代表两套物理数据库、全量微服务、Redis、Kubernetes 或分布式事务。初期可共用 PostgreSQL 集群和基础设施资源，但不能共用数据写入所有权。

## 7. 实施验收条件

1. PlatformConsole 生产构建只配置 Platform.Api；Web.React/客户端只配置 Hub.Api。
2. 两个 API 可独立 build、test、publish、deploy、rollback，并分别暴露 OpenAPI 与健康检查。
3. 两套 API 的 audience/scope/authorization policy 分离；错误 audience/scope 的 token 被拒绝。
4. Platform/Hub 有独立 schema、DbContext、migration 和数据库权限；架构测试阻止跨边界直接访问/写入。
5. Platform→Hub 机构生命周期事件具备 Outbox、幂等消费、重试、对账和停用生效测试。
6. 不存在跨上下文分布式事务；Redis 仅在单独 ADR/性能证据批准后引入。

## 8. 目标代码结构（建议，不代表当前仓库已具备）

```text
src/backend/
├── KWinHub.Platform.Api/
├── KWinHub.Platform.Domain/
├── KWinHub.Platform.Application/
├── KWinHub.Platform.Infrastructure/
├── KWinHub.Hub.Api/
├── KWinHub.Hub.Domain/
├── KWinHub.Hub.Application/
├── KWinHub.Hub.Infrastructure/
├── KWinHub.Contracts/
├── KWinHub.BuildingBlocks/
└── KWinHub.Workers/
```

实施时先建立两个 API Host 和数据所有权规则，再按纵向切片迁移现有代码；不要求一次性机械拆分所有项目。
