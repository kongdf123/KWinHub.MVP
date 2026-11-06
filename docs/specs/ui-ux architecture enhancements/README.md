# KWinHub UI/UX Architecture & Information Architecture Specification

Version 1.0 · Proposed · 2026-11-05

This set is intended for the repository `docs/` directory.

| File | Purpose |
|---|---|
| `01-uiux-architecture.md` | UX architecture, app boundaries, capability model |
| `02-information-architecture.md` | Navigation, page hierarchy, personas and journeys |
| `03-design-system-and-theme-tokens.md` | Design system and three-theme token contract |
| `04-screen-and-interaction-spec.md` | Screen inventory, states, interaction and acceptance criteria |

## Repository baseline

The default branch was reviewed for `KWinHub.Web.React`, `KWinHub.PlatformConsole`, their sidebars, theme switchers, `src/index.css` and Tailwind configurations. Both apps already define semantic CSS variables and `classic` / `light` theme selectors. Extend the existing implementation rather than replacing it.

Preserve the accepted boundaries: PlatformConsole is the internal control plane and calls Platform.Api; Web.React is the customer workspace and calls Hub.Api. Backend authorization is authoritative. Under the current MVP boundary, source applications calculate scientific metrics; Hub validates, stores and presents results with bounded basic statistics. This specification does not approve cloud scientific recalculation, microservices migration, runtime remote plugins or arbitrary remote JavaScript.

I've organized the specification into four Markdown documents plus an index, packaged in a ZIP file. The archive contains a `docs/` directory, so you can extract its contents directly into the KWinHub repository's `docs/` folder.

KWinHub UI/UX Architecture Specification v1.0

5 Markdown files · ZIP archive · 14 KB

Download specification package

## 1. Document structure

| Document                               | Main contents                                                                              |
| -------------------------------------- | ------------------------------------------------------------------------------------------ |
| `README.md`                            | Document index, scope and architecture constraints                                         |
| `01-uiux-architecture.md`              | Experience architecture, Web.React vs PlatformConsole boundaries, capability-module model  |
| `02-information-architecture.md`       | Navigation trees, athlete profile, screen hierarchy, personas and workflows                |
| `03-design-system-and-theme-tokens.md` | Three themes, semantic design tokens, typography, layout and component rules               |
| `04-screen-and-interaction-spec.md`    | Screen inventory, interaction patterns, data states, accessibility and acceptance criteria |

The specifications distinguish between the existing implementation and the proposed target design. They also preserve the existing Platform.Api / Hub.Api responsibility boundary and the current MVP restriction on scientific calculations.

## 2. Theme recommendation

I reviewed the current frontend implementation, including the two `src/index.css` files, Tailwind configurations, theme switchers and navigation components.

Both applications already have semantic CSS variables and two theme identifiers: `classic` and `light`. I recommend retaining them and adding a third theme.

Three-theme strategy

Kunwei Classic

Existing

Deep green sidebar, existing Kunwei brand identity. Preserve the current appearance.

Performance Light

Existing

Clean light surfaces, readable typography and restrained green accents.

Performance Hub

Proposed new theme

Neutral, refined, data-first sports-performance workspace inspired by the general VALD Hub design direction.

### Proposed Performance Hub core tokens

| Semantic token   | Suggested value |
| ---------------- | --------------- |
| App background   | `#F3F5F7`       |
| Panel background | `#FFFFFF`       |
| Sidebar          | `#FFFFFF`       |
| Sidebar selected | `#E8F2F0`       |
| Primary text     | `#17242B`       |
| Secondary text   | `#687780`       |
| Border           | `#DFE5E8`       |
| Primary accent   | `#16786B`       |
| Accent hover     | `#105F55`       |
| Accent soft      | `#E7F2EF`       |
| Success          | `#21836E`       |
| Warning          | `#A96F16`       |
| Danger           | `#B64D4D`       |

These are proposed starting values, not VALD's actual brand tokens. The full document also specifies chart colors, typography, spacing, component behavior and accessibility requirements.

One important implementation recommendation: keep the semantic token names identical in Web.React and PlatformConsole. The two applications can maintain separate CSS files initially, but should not gradually develop different interpretations of the same theme.

## 3. Recommended next-stage implementation order

1. Establish the shared UX architecture and navigation rules.
2. Implement the `hub-light` semantic tokens in both applications.
3. Standardize the athlete profile, session detail and result page patterns in Web.React.
4. Standardize tenant, global catalog and audit workflows in PlatformConsole.
5. Add capability-specific result renderers for Sprint and Performance first.
6. Validate all three themes with visual regression screenshots.

I recommend treating these documents as the UI/UX enhancement baseline v1.0, then deriving implementation tasks from them rather than asking an AI coding agent to redesign both frontends in one large step.
