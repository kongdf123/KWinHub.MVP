# KWinHub Subscription UX Demo Patch

Replace `src/frontend/KWinHub.Web.React/src/features/subscriptions/SubscriptionPage.tsx` with the included file.

The existing `api.ts`, navigation and `App.tsx` can stay unchanged. The page uses existing `useApp`, `TopBar`, `getCurrentOrgId`, translations, API response shapes and Tailwind style conventions.

## Demo mode

For local development (`import.meta.env.DEV`), a **View demo** toggle is available. For a non-development demo build, explicitly set `VITE_ENABLE_SUBSCRIPTION_DEMO=true` at build time. Do not set this for production deployments.

Demo values are hardcoded and clearly marked as illustrative. The app defaults to **live**; no synthetic data is shown as a real subscription.

## Improvements

* Rich subscription overview, usage summary, included feature snapshot
* Dedicated usage cards, progress bars, warning at >=75%
* Friendly entitlement names with enabled/locked states
* Separation between missing subscription and endpoint errors
* Responsive layouts, bilingual labels, three tabs

## Separate security work required

`src/lib/app-context.tsx` currently fails open when access lookup is unavailable. Change it to track loading/error states and deny feature-gated views until server access resolves, without locking users out of unrelated login/recovery screens. Frontend gating is UX only; backend endpoints must validate tenant authorization and entitlements.

## Testing

Run `npm ci`, `npm run build`, `npm run lint`, and `npm test` from the React project, adjusting script names as needed. Test active/demo, no subscription, API 401/403/500, missing org ID, 75% usage threshold, language switch, keyboard tab switching, and responsive layouts.

This file is a proposed replacement; tests were not executed in this session.
