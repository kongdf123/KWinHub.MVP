# KWinHub.Web.React vs. VALD Hub：产品差距分析与功能路线图

我的核心建议：KWinHub 下一阶段应该从“功能丰富的前端原型”转向“真实数据驱动的运动表现管理平台”，优先打通运动员档案、设备采集、测试结果、历史分析和报告的完整工作流。

我结合了三个方面进行评估：

- KWinHub.MVP 当前 React 前端的页面结构、服务层和技术实现。
- VALD 官方的 Hub 产品介绍及其帮助中心公开的实际使用流程。
- Kunwei 的设备生态和长期产品方向，包括测力台、测力起跑器、计时设备、高速摄像机和运动科学算法。

参考资料：[KWinHub.Web.React 源码](https://github.com/kongdf123/KWinHub.MVP/tree/main/src/frontend/KWinHub.Web.React?utm_source=chatgpt.com)、[KWinHub.MVP 仓库](https://github.com/kongdf123/KWinHub.MVP?utm_source=chatgpt.com)、[VALD Hub 官方介绍](https://support.vald.com/hc/en-au/articles/4799248847897-About-VALD-Hub?utm_source=chatgpt.com)。

## 1. Executive summary：整体判断

## KWinHub 当前的产品成熟度

前端功能覆盖：较好

已有运动员档案、测试会话、测试方案、结果、趋势、对比、生物力学、数据集、数据源、集成、同步、报告和管理页面。

这意味着目前不需要推倒重做 UI，而应该利用现有页面承载真正的业务流程。

真实业务闭环：主要短板

React 前端的 `App.tsx` 已经注册了丰富的页面，但当前应用使用模拟登录状态，服务层和演示数据仍需要与实际后端建立可靠连接。

对 VALD 的关键差距：不只是页面数量

VALD Hub 的价值来自多个测量系统共享同一套运动员身份、测试数据、分析工具、组织权限和报告工作流，而不是单纯拥有更多仪表盘。

来源：[VALD Hub 官方功能介绍](https://support.vald.com/hc/en-au/articles/4799248847897-About-VALD-Hub?utm_source=chatgpt.com)。

需要区分两个概念：前端页面已经存在，不代表对应的后端能力、真实数据或业务操作已经完成。 以下代码评估以公开仓库当前可见的实现为依据；我没有在此次评估中实际运行前端、执行端到端测试或验证线上部署。

## 2. What is already implemented in the React frontend?

The current [React application](https://github.com/kongdf123/KWinHub.MVP/tree/main/src/frontend/KWinHub.Web.React?utm_source=chatgpt.com) has a broad navigation and page structure. That is a useful starting point for an enterprise sports-performance platform.

| Functional area    | Existing frontend pages                              | Product assessment                                                            |
| ------------------ | ---------------------------------------------------- | ----------------------------------------------------------------------------- |
| Athlete management | Athletes, Athlete Profile                            | Strong starting point; needs reliable identity management and group workflows |
| Testing            | Sessions, Session Detail, Protocols                  | Good foundation; needs real device-to-Hub workflows                           |
| Results            | Results, Result Detail, Performance                  | Good starting point; needs traceable source data and metric definitions       |
| Analytics          | Trends, Comparisons, Dashboards, Biomechanics        | Promising; needs trustworthy, comparable real measurements                    |
| Data management    | Data Center, Datasets, Raw Data, Data Sources        | Valuable for a multi-device platform; needs a clear data lifecycle            |
| Integration        | Integrations, Sync Center                            | Strategically important; needs operational status and recovery workflows      |
| Reporting          | Reports                                              | Appropriate foundation; needs reproducible reports and export workflows       |
| Administration     | Organization, Users, Roles, Teams, Devices, Settings | Broad coverage; needs real authentication and enforced permissions            |

Source: the current [`App.tsx`](https://github.com/kongdf123/KWinHub.MVP/blob/main/src/frontend/KWinHub.Web.React/src/App.tsx?utm_source=chatgpt.com) page registry.

### The three most important code-level observations

1\. The frontend is still heavily demo-oriented.

[`services.ts`](https://github.com/kongdf123/KWinHub.MVP/blob/main/src/frontend/KWinHub.Web.React/src/lib/services.ts?utm_source=chatgpt.com) and [`demo-data.ts`](https://github.com/kongdf123/KWinHub.MVP/blob/main/src/frontend/KWinHub.Web.React/src/lib/demo-data.ts?utm_source=chatgpt.com) form part of the current service/data layer. The application also initializes its login state locally in `App.tsx`.

Implication: prioritize replacing the mock data path with real APIs and production authentication before expanding the number of screens.

2\. There is a substantial amount of product surface area.

The app has separate pages for data sources, raw data, datasets, results, performance, and reports. These can support distinct professional workflows, but they must share consistent domain entities and navigation context.

Implication: build a coherent athlete-to-session-to-result workflow rather than adding more disconnected sections.

3\. Frontend administration is not a substitute for server-side authorization.

User and role management pages are useful, but they do not prove that the API enforces access control. The broader repository review also identified missing production authentication and tenant-isolation safeguards.

Implication: organization scoping and resource permissions must be enforced by the backend, with corresponding frontend states and error handling.

These observations do not mean that every existing page is incomplete. They identify the highest-risk areas to verify when moving from a visual prototype to a real product.

## 3. What VALD Hub does particularly well

VALD's official documentation describes a connected platform for user permissions, athlete and client profiles, testing data, configurable dashboards, data exports, and additional training and client-engagement capabilities.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



[My Dashboard: Sean Baker | VALD Performance](https://images.openai.com/static-rsc-4/7cWmzmFf145aOuSLDAJaDgQFmsimWGDqnnhM3fLfWhIk3r8uBp0y2gObWRpp_sGjDQVBfcOnwUqMimNIjAVlDS48QrPozUTFjzvwI8rprCoj_gdORjwAqOxpqqL5Bj9IUEkqWI86qxrkRveW0KctHXElK9ejqoUqHrlvOwPIUEU?purpose=inline)

A. A unified athlete record

VALD lets users inspect individual results through profile overviews, testing views, result tables and a chronological timeline. Results from multiple VALD systems can be presented in a common athlete context.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



Lesson for KWinHub: make the athlete profile the central place for all measurements, not merely a demographic page.

[My Dashboard: Jo Clubb | VALD Performance](https://images.openai.com/static-rsc-4/GdHNCNerk3NeTOEssQcp3X0K6TK-DejrBHO-7eT--VEmX9C29xypul8hTDaevVk5TiNfLZFLsusjmPPaaiUGDgQ7lropqqRo5BgV73Cflv_h0lmKYfUDU2tSKelVhCxTxOn4RL7bZ_BrAQcW7-FUJuHQohFrURdI47AoEk5Z6sM?purpose=inline)

B. Configurable analytics and monitoring

VALD supports group monitoring charts, quadrant charts, configurable metrics and thresholds, and analytical reports.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



Lesson for KWinHub: let coaches choose which metrics to monitor and define thresholds that flag athletes requiring attention.

[Products | Tools that help you measure what matters](https://images.openai.com/static-rsc-4/OpivnG6VRN67wUNjACgUH8qIes_J4nXW0GY1ZiUyrTxck58MlNv9sHvlPZV0rRl1VFZZh8RLyvHStfLrdE-hnz2UVOvmCIyykrxFbt8JEdcgy6W_k7bohujJZx52Ij0-7prTMp-On5OeAT-iUsDIhNKEAoSTH5dY7pFXWD6WP-w?purpose=inline)

C. Group management connected to access control

VALD allows profiles to belong to multiple groups and supports restricting users to assigned categories and groups. Those assignments can also affect the dashboards users see.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



Lesson for KWinHub: teams and groups should influence both data analysis and data visibility.

[My Dashboard: Nicol van Dyk | VALD Health](https://images.openai.com/static-rsc-4/6WJcJPsbAmP8Heu0XZF1I9xZ3kwA530mCeVpzVxYK4TvL4Y5TsNzgsT3AQn0c2R8YVf2Llb1mU3cEHbmNzLFdfVcUwr0YDvKtF5nuU1L7kJ_qERyFj-wcxaxScmkZdWDo7iDXFjmo2P0yIAinXxc5AzkKtgrFYrbUYvbjDI31f4?purpose=inline)

D. Practical test-data management

Users can view tests through athlete profiles and system-wide tables, export results, and—in supported workflows—correct test records.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+2



Lesson for KWinHub: the platform should support operational correction and traceability, not just displaying calculated values.

VALD also offers program-building, educational content, questionnaires and patient-facing workflows through its broader ecosystem. These are real product capabilities, but I would not prioritize reproducing all of them in KWinHub V1.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



## 4. Gap analysis: what should KWinHub improve?

The following priorities distinguish product completeness from competitive differentiation.

| Capability                                | KWinHub today                                                    | Recommended next step                                                       | Priority                      |
| ----------------------------------------- | ---------------------------------------------------------------- | --------------------------------------------------------------------------- | ----------------------------- |
| Authentication and organization isolation | UI and admin pages exist; production security is not established | Real login, organization membership, server-enforced permissions            | P0                            |
| Athlete identity and groups               | Athlete/team pages exist                                         | Stable athlete IDs, bulk import, multi-group membership, duplicate merging  | P0                            |
| Device data integration                   | Data-source and integration pages exist                          | Real APIs, device registration, automatic data submission and sync status   | P0                            |
| Session and result traceability           | Session and result pages exist                                   | Link athletes, protocols, trials, devices, raw files and calculated metrics | P0                            |
| Test history                              | Athlete profile and trend pages exist                            | A unified chronological timeline across measurement systems                 | P0                            |
| Data correction                           | Results pages exist                                              | Authorized reassignment, correction, validation and audit history           | P1                            |
| Configurable dashboards                   | Dashboard and comparison pages exist                             | User-configurable metrics, thresholds, filters and saved views              | P1                            |
| Reports and exports                       | Reports page exists                                              | Reproducible PDF/CSV reports and bulk export                                | P1                            |
| Cross-device analysis                     | Biomechanics, comparison and raw-data pages exist                | Synchronized force, timing and video analysis                               | P1 — strategic differentiator |
| Normative benchmarks                      | Analytics foundation exists                                      | Versioned, scientifically defensible reference populations                  | P2                            |
| Client training and education             | Not a demonstrated core workflow                                 | Training programs, questionnaires and client app integration                | P2                            |

Priority definitions

- P0: required for a credible production pilot.
- P1: high-value functionality for the next release or an advanced pilot.
- P2: expansion after the core platform proves its value.

These priorities are my recommendations for Kunwei, not a claim that every capability listed is missing from the current code.

## 5. Six features I would add or substantially improve

### 5.1 A unified athlete profile and testing timeline

This should be the highest-priority user-facing improvement.

[Athlete Dashboards vs Group Dashboards: What's the difference?](https://images.openai.com/static-rsc-4/rYB_mZoI1_UYEaaTX01sM2Lhb_U3p_ESMj9DbdPoKX2kQ78aZRGjxUFfuMBpWSShih8Qi0u3V_tOE-utQp446-dWHH3kotxFxky9prkQthhfTpk6RUU28s9Djz7OfsQta2tVqVBqj_bSTj3fLFXQDaNzUZ1yMyM4-40dVILqbQA?purpose=inline)

[The 80-Second Repeated Squat Jump Test with Matt Jordan, PhD | VALD Performance](https://images.openai.com/static-rsc-4/eHm0-0_9P5oOGfsH1FMtruDJflGkbgQYB6rqTxZd5QGul_VLcMm1bR7tIE2ECFM0Cdq07qIp7CQi8shbvWj1aDs6ysDVhxGPZn_YYsIjBEOQ681Bn5NyG-Eqfq_83E5d3XlmN6NujOvISoaqhMx0BqLITXduhJlRC_wyWf5nDho?purpose=inline)

[AMS Coach Dashboard – Australian Institute of Sport](https://images.openai.com/static-rsc-4/zR3BoTPyBK8UvhSRWdfdx-DHk589bU545iyd3sLBa6RbFNPoELd_YZwy8dMmFZelHCE6CcY33Mz2NPK-Rf4yAZlSlYrld2CcOrdbhiF_X1T0NbXZrxf9TKi2FPvGN5O-_P9WgkSFAdluZYXNgBTgqL2SJWGn4TCYdapGZ31MUt4?purpose=inline)

10

The existing `AthleteProfilePage.tsx` is a natural foundation. Evolve it into the central workspace for an athlete's complete testing history.

Recommended structure:

Athlete overview

Profile, teams, sport, position, relevant attributes and latest testing status.

Testing timeline

Sessions from force plates, sprint tests, timing devices and video, organized chronologically.

Performance history

Longitudinal metric trends, left/right asymmetry, personal bests and comparisons with valid historical baselines.

Reports and raw data

Open detailed results, inspect source data and generate reports from the same session.

Product requirement: opening a test from the timeline must lead directly to the detailed result, including its device, protocol, date, units and data provenance.

This creates a strong foundation for longitudinal athlete monitoring and closely follows a documented VALD Hub workflow.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



### 5.2 A real device integration and synchronization center

KWinHub already has `DataSourcesPage`, `IntegrationsPage` and `SyncCenterPage`. Instead of creating another integration page, turn these into a functioning operational control center.

[SITRANS SCM IQ - Trial](https://images.openai.com/static-rsc-4/AINLdQihc1kEcXal8QlPEi5nQDfUQDsakNyoMB3zAkTfWcNDVfACnkN-3eevyLZpem10xE8cCRgjxwgDPFid77aUyw8wGtjV-WeZZ-2a8T6KwUHBzUInm54zuMgn8vnPdkHOxfofcMWnvu1AAnhW2WefQe_XotJtpIhSH0c0w8s?purpose=inline)

[Smart Building Device Management System | IQnext](https://images.openai.com/static-rsc-4/csQRu_LEHnyUBdmQHktptXg-xa8m3TJK--sM24pKHRcrX-auXcvQHfPDKFvrjhURl0cYAQynIBR9bce1ZQAVBrg4qDShpqIot17fRzNDhTi7bWBpTiSMMLERtDlnJdt1LgOPvH-4A0-hM3I8Szk1qVPkv2rWFZMknS0xGoVUT1I?purpose=inline)

8

I recommend three related views:

- Devices: device identity, type, serial number, software version, last contact and organization assignment.
- Integrations: registered client applications, API credentials lifecycle, permissions, supported data types and connection configuration.
- Synchronization: pending, uploading, processing, completed and failed jobs, including retry actions and actionable error details.

For example, a force-plate application should be able to submit a completed test without a user manually re-entering the results in Hub.

The workflow should be:

1. The desktop client completes a test and assigns a stable client-side session ID.
2. The client submits session metadata and results to the Hub API.
3. Large raw files or video are uploaded through a controlled file-upload flow.
4. Hub validates the submission and processes it asynchronously where necessary.
5. The client can query status, retry safely and reconcile incomplete submissions.
6. The new test appears in the correct athlete's timeline.

The UI must reflect actual server-side state, rather than display a simulated “connected” or “synchronized” badge.

### 5.3 Configurable dashboards with meaningful alerts

KWinHub already has a `DashboardsPage`, `TrendsPage` and `ComparisonsPage`. These should share a reusable analytics foundation instead of implementing independent charting logic.

[My Dashboard: Jo Clubb | VALD Performance](https://images.openai.com/static-rsc-4/GdHNCNerk3NeTOEssQcp3X0K6TK-DejrBHO-7eT--VEmX9C29xypul8hTDaevVk5TiNfLZFLsusjmPPaaiUGDgQ7lropqqRo5BgV73Cflv_h0lmKYfUDU2tSKelVhCxTxOn4RL7bZ_BrAQcW7-FUJuHQohFrURdI47AoEk5Z6sM?purpose=inline)

[Create a Group Dashboard monitoring chart – VALD Knowledge Base](https://images.openai.com/static-rsc-4/Qt4Ws8Ix-H4TGGSPzRubc2_T8ROVWZoN3tY7ON73pAXPGEg1e1Fig9nFoEKUM3FyMecrYMd19q5K59bd6QdekPvu_QOwVSATuzKWwM0xYASq7-XzmB5Tdorwen8AQWOpAI--QReJUFwCA-O_IWUJeeGgqQXT4deJ2jlbVPVbAKw?purpose=inline)

[The Quadrant of Boom: Sprint speed and hamstring strength | VALD Performance](https://images.openai.com/static-rsc-4/_v9vbvL3m9X3s5dhfKuIU81IEhxWaJluOOPXr1tmz5Xq9EHTe-L59py5PIwUKBNQvhNzkamaFDsh7OtyEVgqw4Yy_oWjGnPb0f3y_c8dN1NA8_LSwjJFqu52k_G4rPKHoguRgLeOC-H3WY0pEB02c2zF_wFxW6RKZczFAlPT0Bs?purpose=inline)

11

Recommended capabilities:

| Feature             | User value                                              |
| ------------------- | ------------------------------------------------------- |
| Metric selection    | Choose relevant indicators for a sport, team or athlete |
| Time-window filters | Compare the last test, a training block or a season     |
| Group filters       | Analyze teams, age groups, positions and other cohorts  |
| Threshold rules     | Identify results outside defined ranges                 |
| Quadrant charts     | Compare two indicators to classify performance profiles |
| Saved dashboards    | Reuse views without repeatedly configuring filters      |
| Drill-down          | Open the athletes or sessions behind an aggregate value |

VALD documents customizable tiles, charts, thresholds and group-based dashboards.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



For KWinHub, I would add an important scientific safeguard: an alert must distinguish a genuine performance change from a change caused by different protocols, units, or calculation versions. Thresholds should be configurable and should not automatically imply a medical or injury-risk diagnosis.

### 5.4 Cross-device force, timing and video analysis

This is the most compelling strategic opportunity for Kunwei.

[Force Plates 101: What are they and why are they used? | VALD Health](https://images.openai.com/static-rsc-4/1JVTveLf74r72l8BnbI9R4k9ozKPjqIunDXL5J2YO2FRawa6Dk92IHB7GD-oHcxGOZiY2-3KC2WvqvDRuJUIkapg-guEkapk4yoSEjKFqVs8Rkur7XBB3ejUKreuFOK9PzchSn4NTXmHw8pzpvgQxE-SERN8rvRRbqAw8BWV1Do?purpose=inline)

[Sport at UC - University of Canberra](https://images.openai.com/static-rsc-4/VbIESIlwh_p6y1TmsgG5-_frRz_0r_I01mFtuyR0-7vGv--QNifvSyqd2z0pf2I5IXQbKcRFp-c8NnPD3V5Fd0WIRGPXq3c5fGNbqzcLd2z7l3e3AT4dkUrxYkhBKmx_RPZ38Jj3c_InqIRcoANytClWJQkUp6lf-2tlfwqdIhk?purpose=inline)

[OVR Sprint · Wireless Timing Gates | OVR Performance](https://images.openai.com/static-rsc-4/u4XL_s2I7bchmzOURFUZOhbhKWj-xn9QjYMBEw4ZbmikdxMbFoOOUuhEY7q91ZhLezH9Oj7YtmxpgWvRDLZizVPEN6McfaCcyV-8Oh94kISaUHv62hxpVVpJQ8h-myjQ5QNYVeMLs904wMwuQqcuh6cOF4vgz6D5mEgumFQgb5o?purpose=inline)

8

VALD already has a mature multi-system ecosystem and has publicly documented force-data and video-related workflows. Therefore, synchronized analysis should not be presented as an entirely new concept in the market.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



Kunwei's opportunity is to deliver a particularly effective workflow across its own devices and algorithms.

I would add a Unified Test Analysis view that can display:

- Force-time curves from the force plate or sprint start system.
- Sprint split times and timing-gate events.
- High-speed video with frame-by-frame navigation.
- Common test events and timestamps.
- Algorithm-derived metrics and event markers.
- Original files, calibration information and processing versions.

The central technical requirement is a shared time model. Each data stream should preserve its own clock and acquisition metadata, with an explicit synchronization transform to the common timeline.

For example, a synchronized event should be represented conceptually as:

\\[ t\_{\mathrm{hub}} = a\\,t\_{\mathrm{device}} + b \\]

Here, \\(a\\) represents clock-rate adjustment when needed, and \\(b\\) represents the time offset. The actual synchronization method must be validated against device behavior; a shared timestamp field alone does not guarantee synchronization.

The UI should expose synchronization quality and uncertainty, and clearly indicate when video or sensor data are not aligned reliably.

This feature could make KWinHub more than a generic athlete-data dashboard: it could become the place where practitioners inspect the relationship between measured forces, movement and sprint performance.

### 5.5 A professional report builder

The existing `ReportsPage` is another useful foundation.

[LA클리퍼스의 NBA 오프시즌 선수관리 및 개발 | VALD Performance](https://images.openai.com/static-rsc-4/-uc6Db3XoQMgcRp2PMiDZm33o6OIErDOurpBNp_xtDNKW_WFXWrL7KU56w_kF5n5b5pfjgzZdw43aZxChY4FVqMsnmZCvyyNaeBYzzHHjW_dfXsXlx0bca4X_C_JQhwsSdFLqpYc30L24QBxlbH23vZRw5Nkm5kKQmrdg1axOdY?purpose=inline)

[How do I configure EUR and DSI Reports?](https://images.openai.com/static-rsc-4/VUVAn5G5MFfHM5-OWCFuOvW1tjs2-hTXdYesUpJUiC2OahQnKfzM1-eM84JzZDRz84LjddKeE0A38mnMvDpfTGKMNviCv0tH1A0WmvD5RpPrqXqdpEkCqxQ6h6cxMFowPKB2IebyK1pOmKnT-ycJnlJwVp4jDAcrR-vXH-uHhzE?purpose=inline)

[The Role Of Countermovement Jump Assessment In Baseball. Force Plate A – Premier Pitching Performance](https://images.openai.com/static-rsc-4/C1wxZXtP5mSCMwVm36-v_aQpRi4hs3AG75rY7ApIH6Uc7SOXQ9mCsMSeYzkipRY7rSFTJRIoNSHsKiNst-MKa0yZ6nPZfDw4vAMwXGeplBh-xQqFYuyxvYF1wFycA0nHIEGRoO7tdOGwyTMMRLZPKWbkjAmhE_NoFyeZgTwiPII?purpose=inline)

12

I would introduce three report types:

- Athlete report: key results, changes over time, individual test details and selected graphs.
- Team report: group averages, distributions, individual rankings and threshold exceptions.
- Technical report: raw-data references, protocol, equipment, algorithm version and calculation metadata.

Each report should support appropriate PDF or CSV export. Reports should preserve the test and calculation context that produced the values, so that the same report can be regenerated or audited later.

VALD publicly documents both test-data exports and printable profile reports.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



A later release could add organization logos, reusable templates, scheduled reports and controlled sharing.

### 5.6 Organization, groups and role-based data access

The frontend already has organization, users, roles and teams pages. I would improve the underlying workflow rather than add more administration screens.

Recommended capabilities:

- Multiple organizations per user, with explicit organization switching.
- Membership invitations and role assignment.
- Athletes assigned to multiple groups.
- Group-specific access for coaches, practitioners and analysts.
- Audit history for sensitive data changes and administrative actions.
- Clear handling of deleted, archived, merged and duplicated athlete records.

VALD documents category/group-based segmentation and permissions that can limit the profiles and test data a user can access.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://support.vald.com\&sz=32)

VALD Knowledge Base

+1



For KWinHub, the critical point is that permissions must be enforced by the API, not merely by hiding menu items in React.

## 6. The recommended development roadmap

I would deliver the improvements in four phases, with the first release focused on a real, demonstrable end-to-end workflow.

## P0

Phase 1

Production foundation and one complete workflow

Highest priority

- Real authentication and organization authorization.
- Replace React mock services with typed API services.
- Athlete and group management.
- Real device/client data ingestion and sync status.
- Session, result and athlete-history integration.
- Automated tests for authorization, persistence and synchronization.

Exit criterion: a real test can be submitted by a Kunwei client, persisted, associated with the correct athlete, viewed in Hub and retrieved again without duplicates.

## P1

Phase 2

Daily-use sports-science workflows

- Athlete testing timeline.
- Metric trends and historical comparisons.
- Reliable PDF/CSV reporting.
- Configurable dashboards and thresholds.
- Data correction, provenance and audit trails.

Exit criterion: a coach or sports scientist can use Hub for routine athlete review without relying on disconnected spreadsheets.

## P1+

Phase 3

Kunwei's differentiated experience

- Multi-device session association.
- Force, timing and video synchronization.
- Cross-device test analysis.
- Algorithm version tracking and advanced visualization.

Exit criterion: one supported testing protocol demonstrates a reliable, synchronized multi-device analysis workflow.

## P2

Phase 4

Scale and ecosystem expansion

- Normative datasets with validated populations.
- External AMS/PMS integrations.
- Advanced reporting and automated workflows.
- Training programs and client-facing experiences.
- Broader internationalization and organizational configuration.

Exit criterion: demonstrated customer demand, scientifically validated outputs and a sustainable support model.

The phase labels are relative delivery priorities, not fixed calendar commitments. The actual schedule depends on the backend API readiness, the number of device integrations and the engineering team's capacity.

## 7. What I would change in the frontend structure

Rather than adding many more pages, I would reorganize the user experience around five primary destinations.

1\. Home

Recent tests, pending syncs, data-quality warnings and important team metrics.

2\. Athletes & Teams

Athlete profiles, groups, testing timelines and performance history.

3\. Testing & Analysis

Sessions, protocols, results, biomechanics, comparisons and synchronized media.

4\. Analytics & Reports

Dashboards, trends, group analysis, exports and reusable report templates.

5\. Platform Administration

Organization, users, permissions, devices, integrations, synchronization and settings.

These are proposed user-facing groupings, not a requirement to delete the existing pages. Existing screens can be retained as nested routes or detail views.

From an implementation perspective, I would also establish:

- A shared typed API client generated from the backend OpenAPI specification.
- A consistent query/cache layer for loading and invalidating server data.
- Explicit loading, empty, error, stale-data and permission-denied states.
- Shared metric components that display units, test context and data provenance.
- Reusable chart and table components with consistent filtering.
- Frontend integration and end-to-end tests for the main workflows.

I would avoid migrating to another frontend framework simply for architectural novelty. The current React/TypeScript foundation is adequate for these requirements.

## 8. How to measure whether KWinHub is catching up

I would measure product outcomes, not count the number of pages.

| Product KPI                                  | What it measures                                                      |
| -------------------------------------------- | --------------------------------------------------------------------- |
| Test ingestion success rate                  | Whether clients can reliably deliver real measurements                |
| Duplicate session rate                       | Whether retries and reconnection are handled safely                   |
| Time from test completion to Hub visibility  | Whether synchronization and processing are effective                  |
| Athlete identity match rate                  | Whether data is assigned to the correct athlete                       |
| Percentage of tests with complete provenance | Whether results can be traced to their inputs and processing versions |
| Report generation success rate               | Whether users can reliably produce usable outputs                     |
| Cross-device analysis completion rate        | Whether the differentiated analysis workflow actually works           |
| Weekly active practitioners                  | Whether the platform is valuable in day-to-day work                   |

Set numerical targets after measuring the initial pilot. Do not manufacture baseline values before the product has reliable telemetry.

## Final recommendation

If I were the product manager responsible for the next KWinHub release, I would make three decisions now:

1. Freeze feature expansion temporarily. Complete the real API-backed athlete → test session → result → history workflow first.
2. Make the athlete profile the center of the product. Connect testing history, analytics, raw data and reports through a consistent data model.
3. Build toward cross-device analysis as the strategic differentiator. Once the foundation is reliable, combine Kunwei's force, sprint/timing, video and scientific-computation capabilities into one workflow.

VALD is a useful benchmark for platform completeness and usability. Kunwei should learn from its unified identity, group management, monitoring and reporting workflows while developing a differentiated experience around its own hardware and scientific analysis.

The key principle is: do not compete on how many features appear in the sidebar; compete on how much useful, trustworthy sports-science work a practitioner can complete in one place.