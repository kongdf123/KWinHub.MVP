# Coding Agent Execution Prompt — KWinHub Subscription Center

Repository: https://github.com/kongdf123/KWinHub.MVP

Read `KWinHub_Subscription_Center_Specification.md` entirely and implement it in the repository. Inspect the branch before editing. This specification was prepared against `main` on 2026-10-28; reconcile source drift rather than overwriting unrelated code.

## Work order
1. Inspect current `Organization`, membership/authentication, EF DbContext, backend DI and existing API contracts. Summarize actual code structures and any gaps.
2. Add subscriptions, immutable plan versions, feature definitions/grants, quotas, overrides and audit persistence using EF Core migration.
3. Implement customer read APIs and platform management endpoints with precise tenant and platform staff authorization.
4. Add entitlement resolution, expiry semantics, atomic/idempotent usage enforcement; wire checks into at least one real gated business operation and add reusable service for others.
5. Add customer Subscription Center view to existing `KWinHub.Web.React` settings navigation, with real data and robust loading/empty/error handling; make feature navigation server-aware.
6. Add plans/subscriptions management to existing PlatformConsole routing and navigation, plus organization entitlement/override UI.
7. Add/execute integration tests including cross-tenant, override expiry, feature deny, concurrent quota and duplicate event tests. Run builds and report actual results.

## Constraints
- V1 sales-managed; do not implement or pretend to support Stripe, invoice creation or automatic payment renewal.
- Do not rely on `DEMO_ORG`, editable client role state, or request headers for authorization.
- Preserve old scientific results; no automatic deletion for expired subscriptions.
- No new frontend router, no new microservice, no direct database mutation from frontend.
- New feature codes stable and defined in centralized catalog.
- Changes must be reviewable and compatible with existing branch conventions. No secrets or hardcoded customer credentials.

## Output from agent
- Source file inventory of all changes.
- Migration name and rollback notes.
- Test and build outputs with accurate status.
- Configuration required for local/pilot deployment.
- Explicit known limitations and further phases.
