# VALD Hub 产品研究报告：面向 Kunwei KWinHub 的竞品分析与 PRD 规划

Product Strategy & Requirements Discovery · 2026 年 10 月 · V1.0

## 1. 研究目的与核心结论

这份报告以 VALD Performance 的 VALD Hub 为核心研究对象，结合你提供的 VALD 官方帮助文档、Kunwei 的国际竞品研究，以及现有 KWinHub MVP 技术评审材料，形成下一阶段产品需求文档（PRD）的建议。

核心目标不是复制 VALD 的界面，而是理解它如何将多种运动测试设备、运动员数据、测试结果和专业分析组织成统一的平台，再判断 Kunwei 应当优先建设什么。

[My Dashboard: Sean Baker | VALD Performance](https://images.openai.com/static-rsc-4/7cWmzmFf145aOuSLDAJaDgQFmsimWGDqnnhM3fLfWhIk3r8uBp0y2gObWRpp_sGjDQVBfcOnwUqMimNIjAVlDS48QrPozUTFjzvwI8rprCoj_gdORjwAqOxpqqL5Bj9IUEkqWI86qxrkRveW0KctHXElK9ejqoUqHrlvOwPIUEU?purpose=inline)

结论一：Hub 应当是产品生态的核心，而非单纯的数据看板。

它的战略价值在于把设备、测试数据、运动员档案和跨时间分析连接起来，降低客户使用多种测试设备的管理成本。

[L’hotel vacanza in famiglia e allenamento per giovani attleti](https://images.openai.com/static-rsc-4/1R8gn4lDry2w-2N2xOK5G9sy845IC1XszEDGaqSzskCT9U_32x0y6BBdY9wLPgt9xeohvjsgf_UYequrUh_HCE2Wbc2u05AE89mK3cwC5TrzdJDEthEd4mUUBH5xdX-3tmJ0fWKVyPv4jZfwVaE_E58TZJ6-pjtcMrZT5EE4RHE?purpose=inline)

结论二：Kunwei 应优先打通真实数据链路。

从设备采集到标准化测试结果，再到运动员历史记录和报告，先实现一个端到端的闭环，比先开发大量独立页面更重要。

[VALD Sports Assessment - Measure What Matters](https://images.openai.com/static-rsc-4/nob8e2V70ROHLY-ovFhPQ_GjLMMkmZVZQpZRUSErjRulYMfy3u8ZPUATXLnBy-SV64JgV2GmXxOf1wtWBF48ebFEA6E-uzqi3zYaHvDYBb_Qv7LSwxx3He_xSh3U0u5N9KQNHF-qCWEZfU1SmkhAHNPwSKZYaHGj1uV1N6VvJa4?purpose=inline)

结论三：Kunwei 的差异化机会是跨设备数据融合。

结合现有的测力台、测力起跑器、计时设备和 Basler 高速摄像机，KWinHub 可以进一步探索同步测试数据、视频和专业算法的统一工作流。

需要明确一个研究边界：目前我能够依据你提供的 VALD 官方帮助文档链接和已有竞品材料开展分析，但不能将未经逐项核实的 VALD 功能、权限、接口或商业规则当作已确认事实。以下将区分已知的产品方向、产品经理的推断，以及建议 Kunwei 实现的功能。

## 2. VALD Hub 的产品定位与核心能力

### 2.1 官方产品定位

VALD 官方将 Hub 定义为所有 VALD 测量系统的中央数据和管理平台。官方介绍明确覆盖用户与权限、运动员/客户档案、测试与训练数据，以及训练计划、问卷和教育内容等。

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



官方产品页进一步强调：通过统一登录，把不同设备的数据、团队看板、运动员档案、规范参考数据、权限管理和报告集中到一个平台。

[image](https://www.google.com/s2/favicons?domain=https://valdperformance.com\&sz=32)

valdperformance.com

+1



## VALD Hub 的产品架构逻辑

用户、组织与运动员档案

Users · Organizations · Athletes · Groups

设备测试与数据采集

ForceDecks · ForceFrame · NordBord · DynaMo · SmartSpeed 等

统一数据与测试结果管理

Test Results · Athlete History · Categories · Groups

分析与监控

Group dashboards、个体趋势、Norms

报告与协作

可打印报告、外部系统集成

这是基于官方公开功能归纳的逻辑架构，不是 VALD 内部系统的真实技术架构图。

### 2.2 核心功能拆解

以下功能均可在 VALD 官方公开产品页面或帮助中心中找到依据。这里的优先级是针对 Kunwei 的建议，而不是 VALD 自己的开发优先级。

| 功能模块       | VALD 的产品做法       | 对 Kunwei 的启示     | Kunwei 建议优先级 |
| ---------- | ---------------- | ---------------- | ------------ |
| 组织与账号      | 多组织账号、用户及权限管理    | 从第一版建立可靠的租户边界    | P0           |
| 运动员档案      | 统一管理运动员和客户资料     | 统一身份，避免不同设备重复建档  | P0           |
| 分组与分类      | 支持按群组组织和分析数据     | 支持队伍、项目、年龄组和位置   | P0           |
| 测试数据       | 汇集多个 VALD 系统的数据  | 建立跨设备统一数据模型      | P0           |
| 个体档案视图     | 根据运动员需求定制展示指标    | 让教练聚焦自己关注的指标     | P1           |
| 团队仪表盘      | 组间、组内指标可视化       | 快速发现异常、差异和趋势     | P1           |
| 历史趋势       | 跟踪运动员测试进展        | 形成纵向数据价值         | P1           |
| Norms 参考数据 | 将结果与参考人群进行比较     | 后续建设有依据的参考基准     | P2           |
| 报告输出       | 生成并分享专业报告        | 建立统一报告模板和导出能力    | P1           |
| 外部系统集成     | 支持 AMS/PMS 等系统对接 | 提供版本化 API 与集成能力  | P1           |
| 训练与教育      | 包括训练计划、问卷及教育内容   | 可作为未来扩展，不宜首期全面铺开 | P2           |

官方资料支持上述核心方向，特别是组织权限、运动员档案、分组看板、Norms、报告和第三方系统集成。

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+3



### 2.3 最值得深入学习的五个设计

A. 一个运动员身份，连接多种测试数据

这是整个 Hub 的数据基础。运动员应当有稳定的内部 ID；测试会话、设备数据和结果通过明确的关联关系连接到该运动员，而不是由每个客户端独立维护一套运动员数据库。

B. 以组织和群组驱动业务工作流

教练实际管理的通常不是孤立的测量结果，而是队伍、年龄组、运动项目和运动员群体。群组既应服务于数据筛选，也应服务于权限范围和团队报告。

C. 个体与群体使用不同的分析视角

个体页面回答“这个运动员最近发生了什么变化”；群组页面回答“谁存在差异、整体表现如何、需要进一步关注谁”。两者应共用数据和指标定义，而非独立开发。

D. 报告是产品能力，不是最后的导出按钮

报告应支持统一的指标定义、团队标识、测试信息和趋势上下文。它既是教练的工作成果，也是向运动员、家长、医疗团队或管理层传递信息的工具。

E. 平台要允许数据进入其他业务系统

VALD 官方列出了与多种 AMS/PMS 系统的集成，并提供 API 开发资料。

[image](https://www.google.com/s2/favicons?domain=https://valdperformance.com\&sz=32)

Integrations at no additional cost

&#x20;Kunwei 不应把全部价值限制在自家前端，而应允许授权的外部系统使用标准化数据。



## 3. 从竞品研究看 Kunwei 的机会

Kunwei 与 VALD 的差异不能只比较功能数量，更应该比较目标用户、设备生态、数据融合能力和交付成本。

| 竞争维度  | VALD 已体现的优势    | Kunwei 可以重点探索的方向          |
| ----- | -------------- | ------------------------- |
| 设备生态  | 多种测量系统接入统一 Hub | 先把自有设备形成完整的数据闭环           |
| 测试数据  | 跨产品汇总与分析       | 统一测力、计时、起跑和视频数据模型         |
| 科学分析  | 指标、群组分析、参考数据   | 复用 KWinScience 计算能力及版本化算法 |
| 视频与信号 | 具体能力取决于各系统与工作流 | 把同步时间轴作为可验证的差异化方向         |
| 集成能力  | 对接外部 AMS/PMS   | 设计公开 API、数据导出与客户端同步机制     |
| 国际化   | 已有成熟的国际产品生态    | 逐步建立多语言、时区、单位与区域化数据策略     |

这里有一项重要的战略判断：Kunwei 不必在第一版成为覆盖所有运动表现领域的完整 AMS 平台。 更现实的路径是先把自身最有优势的设备和专业分析连接起来，再扩展到更广泛的运动员管理场景。

## 4. KWinHub V1 的产品定位建议

我建议将 KWinHub 定位为：

> A unified sports-performance data platform that connects Kunwei devices, scientific analysis, athlete profiles, and performance reporting.

中文定义：

统一运动表现数据平台：连接运动设备、科学计算、运动员档案与表现分析。

V1 的目标用户应优先选择拥有 Kunwei 测试设备、需要长期管理运动员测试数据的组织，例如体育科研机构、运动队、体能训练中心和运动表现实验室。

### 4.1 产品边界

V1 必须具备

- 组织、用户、角色与权限
- 运动员档案及群组管理
- 测试会话与多设备关联
- 测试结果统一存储和查询
- 客户端/API 数据接入
- 个体历史记录与基础趋势
- 基础报告和数据导出
- 数据同步状态、失败重试和错误追踪

V1.1 / V2 逐步增加

- 团队分析仪表盘
- 自定义运动员指标视图
- 视频与测量信号同步回放
- 参考基准与规范数据比较
- 外部 AMS/PMS 深度集成
- 更丰富的报表模板和自动化工作流

暂不建议作为 V1 主目标

- 完整训练计划与处方系统
- 全面的康复管理工作流
- 自建大型运动员规范数据库
- AI 自动教练与复杂预测模型
- 复杂的微服务及多区域部署体系

这些是建议的产品边界，不代表 VALD 的功能缺失或 Kunwei 当前已经具备这些能力。

## 5. 建议的 V1 PRD 结构

下面的内容可以直接作为正式 PRD 的目录和需求骨架。

### 5.1 功能需求清单

| ID      | 模块    | 核心需求                  | 优先级 |
| ------- | ----- | --------------------- | --- |
| HUB-001 | 身份认证  | 登录、退出、令牌校验            | P0  |
| HUB-002 | 组织管理  | 创建/切换组织，维护成员关系        | P0  |
| HUB-003 | 权限管理  | 角色授权及组织级数据隔离          | P0  |
| HUB-004 | 运动员管理 | 新建、编辑、查询、归档运动员        | P0  |
| HUB-005 | 群组管理  | 创建队伍、项目组、年龄组等         | P0  |
| HUB-006 | 设备管理  | 设备注册、设备身份、设备与组织关联     | P0  |
| HUB-007 | 数据接入  | API 提交测试会话、试次、结果与文件引用 | P0  |
| HUB-008 | 同步管理  | 状态查询、幂等重试、失败记录        | P0  |
| HUB-009 | 测试历史  | 按运动员、设备、测试类型、时间检索     | P0  |
| HUB-010 | 结果展示  | 展示指标、单位、测试信息及来源       | P0  |
| HUB-011 | 趋势分析  | 查看指标随时间的变化            | P1  |
| HUB-012 | 报告输出  | 生成 PDF/CSV 等适用的导出文件   | P1  |
| HUB-013 | 群组仪表盘 | 群体分布、排名、异常结果筛选        | P1  |
| HUB-014 | 审计日志  | 记录关键管理和数据操作           | P0  |
| HUB-015 | 外部集成  | 版本化公开 API 与集成配置       | P1  |

P0 表示首期发布的必要条件；P1 表示首期之后或首期末尾交付，具体取决于试点客户的实际工作流。

### 5.2 核心用户故事

US-01：创建并管理运动员

作为教练，我希望在 KWinHub 中创建一个运动员档案，并把运动员分配到所属队伍或测试群组，以便不同客户端使用同一个身份记录测试结果。

验收标准：

- 每个运动员具有唯一的系统 ID。
- 同一组织内可以检查重复档案。
- 运动员可以被分配至群组。
- 未获授权的用户不能查看或修改该运动员的资料。
- 档案变更不会破坏历史测试记录。

US-02：从 Kunwei 客户端自动上传测试结果

作为测试人员，我希望桌面或移动客户端能够通过 API 自动上传已完成的测试，而不必手工填写重复的数据。

验收标准：

- 客户端通过受支持的认证机制获得访问权限。
- 每次提交包含稳定的客户端会话 ID 和必要的测试元数据。
- 重复提交不会生成重复测试。
- 大型原始数据文件通过受控的文件上传流程传输。
- 上传失败时可以查询状态并重新提交。
- 服务端能识别上传成功、处理中、处理失败等状态。

US-03：查看运动员历史测试

作为教练，我希望在运动员档案中查看不同日期的测试结果，以便比较变化并了解运动员的发展趋势。

验收标准：

- 支持按时间、测试类型和设备筛选。
- 结果显示指标名称、单位、测试日期及来源。
- 同一指标在不同时间的结果必须使用可比较的定义和单位。
- 不同算法版本产生的结果能够被识别。
- 历史记录不会因为新版算法上线而被静默覆盖。

US-04：跨设备关联同一次测试

作为运动科学人员，我希望将一次测试中的测力、计时和视频数据关联起来，以便在同一测试记录中检查相关结果。

验收标准：

- 一个测试会话可以关联多个设备。
- 每份数据都保留来源设备和必要的时间信息。
- 系统区分原始数据与计算结果。
- 对于需要同步的信号，保存同步基准、偏移量及相关配置。
- 缺失或未同步的数据会明确标记，而不是显示为已成功同步。

## 6. 核心技术设计：借鉴 VALD 的平台思想，但适配 Kunwei

结合你此前的 KWinHub MVP 技术评审，我建议在 PRD 阶段就确定几个不可妥协的设计原则。

### 6.1 统一的数据实体模型

Organization / Membership

组织、用户、成员权限

Athlete / Team / Group

运动员身份与组织结构

Measurement Session

测试会话、协议版本、多个关联设备

Trial / Data Object

原始信号、视频、文件引用

Processing / Result

算法、指标值、计算版本

Analysis / Reports / Exports

历史趋势、群组比较、报告输出

关键要求：

- 一个会话可以关联多个设备，不能将 `MeasurementSession` 限定为仅有一个 `DeviceId`。
- 将可复用的 `MetricDefinition` 与实际测量的 `ResultMetricValue` 分开。
- 保留原始数据、处理结果、算法版本及必要的计算配置，保证科学结果可追溯。
- 使用对象存储保存大型原始文件和视频，数据库存储元数据及访问控制信息。
- 所有业务资源都必须有明确的组织归属和服务端授权路径。

这些建议也与现有 KWinHub MVP 代码评审中指出的数据模型、租户隔离和数据接入问题一致。

### 6.2 客户端集成：不要依赖频繁轮询作为唯一机制

对于 Kunwei 的桌面和移动客户端，我建议采用以下模式：

KWin Desktop / Mobile

本地采集、计算、缓存与待同步队列

KWinHub API

认证、数据校验、幂等处理、同步状态

PostgreSQL + Object Storage

业务元数据、结果、原始文件与处理任务

推荐的同步策略：

1. 客户端通过受支持的认证机制登录，并获得授权的组织和功能范围。
2. 客户端在本地采集并保存测试，生成稳定的客户端会话 ID。
3. 测试完成后主动提交数据；大文件使用单独的上传流程。
4. 服务端返回接收状态和可追踪的服务端 ID。
5. 客户端在网络恢复后重试失败任务；服务端利用幂等键避免重复写入。
6. 客户端通过按需查询、增量同步或有限频率的状态轮询更新界面。

心跳 API 只适合辅助判断客户端或设备的在线状态，不应代替测试数据的主动上传机制。 这样可以降低不必要的网络请求，并在网络不稳定的场地环境中保证测试数据最终能够同步。

## 7. 研发路线图与交付验收

## 01

基础设施与安全

目标：建立可靠的多租户基础。

- 确定唯一的生产前端。
- 实现认证、组织成员关系和权限控制。
- 建立数据库迁移、约束和集成测试。
- 明确组织级数据隔离策略。

## 02

完整的数据接入闭环

目标：一个真实 Kunwei 客户端成功接入。

- 运动员、设备、会话、试次及结果接口。
- 幂等上传、重试和同步状态。
- 原始文件上传及安全访问。
- 完成一个真实测试的端到端验证。

## 03

运动员管理与分析

目标：让真实测试数据产生可见业务价值。

- 运动员档案和历史记录。
- 基础趋势图和结果比较。
- 报告导出。
- 根据真实客户反馈调整工作流。

## 04

平台能力扩展

目标：形成更完整的运动表现平台。

- 群组仪表盘与高级分析。
- 视频与多设备数据同步。
- 规范参考数据。
- 外部集成和更多客户端。

### V1 发布前必须通过的验收门槛

| 维度     | 建议验收条件                 |
| ------ | ---------------------- |
| 数据安全   | 未授权用户无法跨组织读取或修改业务资源    |
| 数据完整性  | 重复上传不会创建重复测试           |
| 可靠性    | 网络中断后，客户端能够重试并确认最终同步状态 |
| 科学可追溯性 | 结果能追溯到原始数据及适用的算法版本     |
| 多设备能力  | 同一测试会话能够关联多个设备         |
| 产品可用性  | 用户能在 Web 平台找到真实上传的测试记录 |
| 运维能力   | 关键错误、失败任务和异常请求可被追踪     |

## 8. 下一阶段最值得验证的产品假设

VALD 的公开产品资料支持一个重要方向：把多种设备的测量结果放在同一平台中，通过运动员档案、群组分析、参考数据和报告，形成持续的数据使用价值。

[image](https://www.google.com/s2/favicons?domain=https://valdperformance.com\&sz=32)

valdperformance.com

+1



对 Kunwei 来说，还需要通过客户访谈和试点验证以下问题：

- 客户最迫切的需求是跨设备数据统一，还是运动员长期管理？
- 视频同步是首期必须交付的能力，还是第二阶段的差异化功能？
- 现有客户端能否稳定提供统一的运动员 ID、测试 ID 和设备标识？
- 客户是否需要把 KWinHub 接入已有 AMS，还是先使用独立平台？
- 客户是否愿意持续使用团队仪表盘和报告，还是主要关注原始测试结果与专业分析？

这些问题会直接影响 MVP 的投入顺序，因此应当在完整 PRD 冻结之前验证。

## 9. 我的最终建议

我建议下一阶段不是立即开发一个“功能齐全的 VALD Hub 仿制品”，而是形成两份互相衔接的交付物：

1. KWinHub V1 产品需求文档（PRD）：明确目标用户、业务流程、页面结构、功能需求、验收标准及非目标。
2. KWinHub V1 技术需求与接口规范：明确核心实体、API 契约、认证授权、客户端同步、原始文件存储和算法可追溯性。

先以一个真实的 Kunwei 测试工作流验证平台的价值，再逐步扩展到跨设备分析、群组仪表盘和高级报告。这条路线既吸收了 VALD Hub 的平台化经验，也更符合 Kunwei 当前的产品和研发阶段。

为了把这份研究报告进一步转化为可以交给设计和研发团队执行的 PRD，下面有两个值得优先确定的方向。

确认首期客户和数据范围后，我可以据此细化页面、接口和验收标准。

回答 3 个问题

### 参考资料

- [VALD Hub 官方产品介绍](https://valdperformance.com/products/valdhub?utm_source=chatgpt.com) — 平台定位、团队仪表盘、运动员档案、Norms、权限与报告。

  [image](https://www.google.com/s2/favicons?domain=https://valdperformance.com\&sz=32)

  valdperformance.com

- [VALD Hub 官方帮助文档：About VALD Hub](https://support.vald.com/hc/en-au/articles/4799248847897-About-VALD-Hub?utm_source=chatgpt.com) — 组织管理、用户权限、运动员档案及数据管理。

  [image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

  VALD Knowledge Base

- [VALD Hub Starter's Guide](https://support.vald.com/hc/en-au/articles/4799248629017-VALD-Hub-Starter-s-Guide?utm_source=chatgpt.com) — 组织设置、用户配置、运动员建档和设备测试流程。

  [image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

  VALD Knowledge Base

- [VALD Integrations](https://valdperformance.com/products/integrations?utm_source=chatgpt.com) — 外部系统集成及 API 开发资源。

  [image](https://www.google.com/s2/favicons?domain=https://valdperformance.com\&sz=32)

  Integrations at no additional cost
