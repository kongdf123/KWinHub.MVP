# 03 — KWinHub Design System & Theme Tokens

**Status:** Proposed  
**Scope:** `KWinHub.Web.React` and `KWinHub.PlatformConsole`  
**Purpose:** Establish a consistent, theme-aware design system for both KWinHub frontend applications.

---

## 1. Design System Objectives

The KWinHub design system should provide a consistent visual language across both frontend applications while supporting different product identities and usage scenarios.

The design system must:

1. Preserve the existing Kunwei corporate identity and `classic` theme.
2. Introduce a more professional, neutral `Performance Light` experience.
3. Introduce a premium, data-oriented `Performance Hub` experience.
4. Separate corporate brand colors from theme-specific interaction colors.
5. Use semantic design tokens instead of hard-coded color values.
6. Maintain consistent component behavior across both frontend applications.
7. Support both Chinese and English interfaces.
8. Provide accessible contrast, visible focus states, and predictable interaction feedback.
9. Support future themes without requiring individual components to be redesigned.

### 1.1 Three-theme strategy

The repository already implements semantic `classic` and `light` themes in both frontend applications. Preserve the existing theme identifiers and add a third theme.

| Theme ID | Display Name | Visual Direction | Primary Accent |
|---|---|---|---|
| `classic` | Kunwei Classic | Existing corporate identity, familiar and brand-forward | Deep green |
| `light` | Performance Light | Clean, professional, neutral, modern enterprise UI | Professional blue |
| `hub-light` | Performance Hub | Premium sports-performance analytics workspace | Warm gold / bronze |

The three themes should feel like different visual presentations of the same KWinHub product, not three independent applications.

### 1.2 Design inspiration

Performance Hub may take general design inspiration from VALD Hub:

- Clean white and light-neutral surfaces.
- Restrained use of accent colors.
- Compact and structured navigation.
- Strong metric and data hierarchy.
- Clear separation between primary actions and secondary actions.
- Subtle borders and minimal shadows.
- Professional sports-performance analytics presentation.

Do not copy VALD's exact brand colors, logos, assets, wording, or page layouts.

---

## 2. Brand Color vs. Theme Accent Color

This is a mandatory architectural principle.

The current implementation has a visual inconsistency: the deep-green company color is used for many interactive elements regardless of the selected theme.

For example, after switching to Performance Hub, primary buttons may still appear deep green. This makes the theme feel incomplete and weakens its visual identity.

The solution is to separate two different concepts.

### 2.1 Corporate brand colors

Corporate brand colors represent KWinwei's company identity.

They should remain stable regardless of the selected theme.

Examples:

- Company logo and corporate identity.
- Company-related brand graphics.
- Corporate identity elements where green is explicitly intended.
- Brand-specific materials.

Suggested semantic token:

`--brand-primary`

The existing corporate green must be preserved. Do not replace it globally with gold or blue.

### 2.2 Theme accent colors

Theme accent colors represent the active visual theme and control the appearance of interactive elements.

They should change automatically when users switch themes.

Examples:

- Primary buttons.
- Active navigation indicators.
- Selected navigation items.
- Active tabs.
- Selected filters.
- Focused form controls.
- Selected checkboxes and radio buttons.
- Workflow progress indicators.
- Important chart highlights.
- Interactive links where appropriate.

Suggested semantic tokens:

- `--accent-primary`
- `--accent-hover`
- `--accent-active`
- `--accent-soft`
- `--accent-highlight`
- `--accent-foreground`

### 2.3 Expected behavior

| UI Element | Kunwei Classic | Performance Light | Performance Hub |
|---|---|---|---|
| Company logo | Original brand | Original brand | Original brand |
| Corporate identity | Deep green | Deep green | Deep green |
| Primary button | Deep green | Blue | Gold |
| Button hover | Dark green | Dark blue | Dark bronze |
| Active navigation | Green | Blue | Gold |
| Selected tab | Green | Blue | Gold |
| Selected filter | Green | Blue | Gold |
| Focus indicator | Green | Blue | Gold |
| Soft selection background | Pale green | Pale blue | Pale gold |
| Success status | Semantic green | Semantic green | Semantic green |
| Warning status | Semantic amber | Semantic amber | Semantic amber |
| Error status | Semantic red | Semantic red | Semantic red |

**Important:** Do not use `--brand-primary` as the default primary-button color. Components must use the theme-aware `--accent-*` tokens instead.

---

## 3. Theme Color Specifications

The following values are proposed starting points for visual validation. They are not final corporate brand standards.

### 3.1 Kunwei Classic

**Design direction:** Preserve the existing corporate visual identity.

Requirements:

- Keep the existing deep-green primary color.
- Preserve existing classic-theme surfaces and component styling.
- Avoid introducing unnecessary visual changes.
- Keep existing saved theme preferences.
- Do not replace the current corporate green with the new gold or blue palettes.

The current Classic palette should remain the source of truth in the existing implementation.

Where existing components use literal green values, migrate them to semantic tokens without changing their rendered appearance.

### 3.2 Performance Light

**Design direction:** Clean, professional, neutral enterprise workspace.

Performance Light should be visually lighter and less brand-heavy than Kunwei Classic.

Recommended accent palette:

| Token | Value | Purpose |
|---|---|---|
| `--accent-primary` | `#3977A8` | Primary buttons and active controls |
| `--accent-hover` | `#2F638D` | Hover state |
| `--accent-active` | `#285577` | Pressed state |
| `--accent-soft` | `#EAF2F8` | Selected background |
| `--accent-highlight` | `#6599C0` | Highlights and indicators |
| `--accent-foreground` | `#FFFFFF` | Text on primary accent |

The blue accent should be restrained rather than saturated.

The objective is to establish a neutral professional appearance suitable for daily management, organization administration, athlete management, and operational workflows.

### 3.3 Performance Hub

**Design direction:** Premium, data-first sports-performance analytics workspace.

Recommended accent: warm gold / bronze.

The gold should be muted and sophisticated, not bright yellow or metallic gradient styling.

Recommended palette:

| Token | Value | Purpose |
|---|---|---|
| `--accent-primary` | `#B8892E` | Primary buttons and selected controls |
| `--accent-hover` | `#A57825` | Hover state |
| `--accent-active` | `#89631F` | Pressed state |
| `--accent-soft` | `#F7F0E1` | Selected backgrounds |
| `--accent-highlight` | `#D1AC61` | Highlights and progress indicators |
| `--accent-foreground` | `#20252B` | Text on primary gold |
| `--accent-text` | `#70551F` | Text and icons on light surfaces |

The warm gold accent should make Performance Hub feel more premium and distinctive while keeping the interface calm and professional.

#### Gold usage principles

Use gold for:

- Primary action buttons.
- Active sidebar indicators.
- Selected navigation items.
- Active tabs.
- Selected filters.
- Focused controls.
- Important workflow progress.
- Key performance metric highlights.
- Selected chart series or important chart annotations.

Do not use gold for:

- Every card border.
- Every icon.
- Every heading.
- Every chart series.
- Success or warning status indicators.
- Large background areas.
- Decorative gradients throughout the application.

Gold should be an accent, not the dominant surface color.

#### Button text contrast

For the proposed `#B8892E` primary gold, use dark text (`#20252B`) rather than white text.

This provides better contrast for the primary button.

The hover and pressed states should also be tested for text contrast. If a darker bronze shade is used, its foreground color may need to change to white.

---

## 4. Performance Hub Surface and Neutral Palette

Performance Hub should use cooler neutral surfaces and a clean white workspace.

Recommended tokens:

| Token | Value | Use |
|---|---|---|
| `--surface-app` | `#F3F5F7` | Main application canvas |
| `--surface-panel` | `#FFFFFF` | Cards and content panels |
| `--surface-header` | `#FFFFFF` | Header |
| `--surface-hover` | `#F1F4F6` | General hover surface |
| `--surface-sidebar` | `#FFFFFF` | Sidebar |
| `--surface-sidebar-hover` | `#F3F5F7` | Sidebar hover |
| `--surface-sidebar-active` | `#F7F0E1` | Selected navigation |
| `--text-primary` | `#263442` | Main content |
| `--text-secondary` | `#687780` | Secondary content |
| `--text-tertiary` | `#89949C` | Metadata and hints |
| `--text-sidebar` | `#34434A` | Sidebar text |
| `--text-sidebar-active` | `#70551F` | Selected navigation text |
| `--text-sidebar-muted` | `#7B898F` | Sidebar metadata |
| `--border-default` | `#DDE2E7` | Standard borders |
| `--border-sidebar` | `#E3E8EA` | Sidebar separators |
| `--border-strong` | `#C6CED5` | Emphasized borders |

### 4.1 Performance Hub CSS starter

```css
[data-theme="hub-light"] {
  color-scheme: light;

  /* Brand identity: independent of theme accent */
  --brand-primary: var(--kunwei-brand-primary);

  /* Surfaces */
  --surface-app: #F3F5F7;
  --surface-panel: #FFFFFF;
  --surface-header: #FFFFFF;
  --surface-hover: #F1F4F6;
  --surface-sidebar: #FFFFFF;
  --surface-sidebar-hover: #F3F5F7;
  --surface-sidebar-active: #F7F0E1;

  /* Text */
  --text-primary: #263442;
  --text-secondary: #687780;
  --text-tertiary: #89949C;
  --text-sidebar: #34434A;
  --text-sidebar-active: #70551F;
  --text-sidebar-muted: #7B898F;

  /* Borders */
  --border-default: #DDE2E7;
  --border-sidebar: #E3E8EA;
  --border-strong: #C6CED5;

  /* Theme accent: Performance Hub gold */
  --accent-primary: #B8892E;
  --accent-hover: #A57825;
  --accent-active: #89631F;
  --accent-soft: #F7F0E1;
  --accent-highlight: #D1AC61;
  --accent-foreground: #20252B;
  --accent-text: #70551F;

  /* Status */
  --status-success: #21836E;
  --status-warning: #A65F12;
  --status-danger: #B64D4D;
  --status-info: #3977A8;
  --status-neutral: #718087;

  /* Focus */
  --focus-ring: #A57825;
}
```

`--kunwei-brand-primary` should resolve to the existing corporate brand color token. Do not create a second, inconsistent definition of the company green.

---

## 5. Semantic Token Contract

All shared UI components must use semantic tokens rather than literal palette classes.

The design system should provide the following token groups.

### 5.1 Brand tokens

- `--brand-primary`
- `--brand-secondary`
- `--brand-foreground`

These represent the company identity and should not change when users switch themes.

### 5.2 Surface tokens

- `--surface-app`
- `--surface-panel`
- `--surface-header`
- `--surface-hover`
- `--surface-selected`
- `--surface-sidebar`
- `--surface-sidebar-hover`
- `--surface-sidebar-active`
- `--surface-overlay`

### 5.3 Content tokens

- `--text-primary`
- `--text-secondary`
- `--text-tertiary`
- `--text-disabled`
- `--text-inverse`
- `--text-sidebar`
- `--text-sidebar-active`
- `--text-sidebar-muted`

### 5.4 Border tokens

- `--border-default`
- `--border-subtle`
- `--border-strong`
- `--border-sidebar`
- `--border-focus`

### 5.5 Accent tokens

- `--accent-primary`
- `--accent-hover`
- `--accent-active`
- `--accent-soft`
- `--accent-highlight`
- `--accent-foreground`
- `--accent-text`

### 5.6 Status tokens

- `--status-success`
- `--status-success-soft`
- `--status-warning`
- `--status-warning-soft`
- `--status-danger`
- `--status-danger-soft`
- `--status-info`
- `--status-info-soft`
- `--status-neutral`
- `--status-neutral-soft`

### 5.7 Focus and interaction tokens

- `--focus-ring`
- `--focus-ring-offset`
- `--interactive-disabled-opacity`
- `--interactive-transition-duration`

### 5.8 Visualization tokens

- `--viz-series-1`
- `--viz-series-2`
- `--viz-series-3`
- `--viz-series-4`
- `--viz-series-5`
- `--viz-grid`
- `--viz-axis`
- `--viz-reference`
- `--viz-highlight`

---

## 6. Button and Interactive Component Rules

Theme-aware button styling is a mandatory implementation requirement.

### 6.1 Button hierarchy

All buttons should follow a consistent hierarchy.

| Button Type | Visual Treatment | Purpose |
|---|---|---|
| Primary | Theme accent background | Main page action |
| Secondary | Neutral surface and border | Supporting action |
| Tertiary / Ghost | Transparent background | Low-emphasis action |
| Destructive | Semantic danger color | Delete or irreversible operation |
| Link | Text accent | Navigation or inline action |

### 6.2 Primary button

The primary button must use:

- Background: `--accent-primary`
- Hover: `--accent-hover`
- Active: `--accent-active`
- Text: `--accent-foreground`
- Focus: `--focus-ring`

Example:

```css
.button-primary {
  background-color: var(--accent-primary);
  color: var(--accent-foreground);
  border-color: transparent;
}

.button-primary:hover {
  background-color: var(--accent-hover);
}

.button-primary:active {
  background-color: var(--accent-active);
}

.button-primary:focus-visible {
  outline: 2px solid var(--focus-ring);
  outline-offset: 2px;
}
```

Do not hard-code green in shared primary button components.

### 6.3 Secondary button

Secondary buttons should remain visually neutral across all three themes.

Recommended treatment:

- White or neutral background.
- Subtle border.
- Primary text color.
- Neutral hover surface.

The theme accent may be used for focus or selected states, but secondary buttons should not compete with primary actions.

### 6.4 Destructive button

Destructive actions must use semantic danger colors rather than the current theme accent.

Examples:

- Delete athlete.
- Remove organization.
- Delete a protocol.
- Permanently remove a record.

Do not use gold, blue, or green to communicate destructive actions.

### 6.5 Loading and disabled states

- Loading buttons must prevent duplicate submission.
- Preserve button dimensions during loading.
- Use a clear progress indicator.
- Disabled controls must remain distinguishable from enabled controls.
- Do not rely only on opacity to communicate disabled state.

---

## 7. Status Colors Must Remain Independent

Status colors must not inherit the active theme accent.

This is especially important in Performance Hub because gold is used for primary actions, while amber is commonly associated with warnings.

Use a distinct semantic status palette.

| Status | Suggested Color | Purpose |
|---|---|---|
| Success | `#21836E` | Successful operation |
| Warning | `#A65F12` | Attention required |
| Danger | `#B64D4D` | Error or failure |
| Info | `#3977A8` | Informational message |
| Neutral | `#718087` | Pending or inactive |

Each status should also have a corresponding soft background token.

Status communication must combine color with text or an icon. Never rely on color alone.

Examples:

- Success: green icon + "Completed"
- Warning: amber icon + "Needs attention"
- Error: red icon + "Failed"
- Information: blue icon + "Processing"

A gold primary button must not be mistaken for a warning indicator.

---

## 8. Visualization and Chart Tokens

KWinHub is a scientific and sports-performance platform. Its charts and metric visualizations need a separate color system.

Do not automatically use the primary button color for every chart.

Suggested Performance Hub chart palette:

| Token | Value | Purpose |
|---|---|---|
| `--viz-series-1` | `#B8892E` | Primary highlighted series |
| `--viz-series-2` | `#4777A8` | Secondary series |
| `--viz-series-3` | `#8A6BB0` | Third series |
| `--viz-series-4` | `#5F8D68` | Fourth series |
| `--viz-series-5` | `#C47755` | Fifth series |
| `--viz-grid` | `#E7ECEE` | Chart grid |
| `--viz-axis` | `#718087` | Axis labels |
| `--viz-reference` | `#8A969C` | Reference lines |
| `--viz-highlight` | `#F7F0E1` | Selected data region |

### 8.1 Chart design rules

1. Always label chart series.
2. Do not rely on red/green alone to distinguish data.
3. Use consistent colors for the same metric across pages.
4. Keep left/right and limb mapping stable.
5. Provide clear units and axis labels.
6. Use reference lines and event markers where scientifically meaningful.
7. Use the theme accent for emphasis, not as a replacement for a complete chart palette.
8. Ensure chart colors remain distinguishable in all three themes.

For example, left and right limb data should retain consistent visual mapping across Athlete Profile, Session Detail, and Analytics pages.

### 8.2 Chart theme configuration

Centralize chart colors in one theme-aware configuration.

Avoid duplicating chart palette definitions in individual page components.

---

## 9. Typography and Layout

### 9.1 Font family

Retain the existing font stack:

```css
font-family:
  Inter,
  "PingFang SC",
  "Microsoft YaHei",
  system-ui,
  sans-serif;
```

Ensure Chinese and English content render consistently.

### 9.2 Typography scale

| Role | Suggested Size | Weight |
|---|---:|---|
| Page title | 24–28 px | Semibold |
| Section title | 16–18 px | Semibold |
| Card title | 14–16 px | Semibold |
| Body | 13–14 px | Regular |
| Metadata | 12 px | Regular |
| Table header | 11–12 px | Medium |
| Large metric | 32–48 px | Semibold |
| Metric unit | 12–14 px | Regular |

Use tabular numerals for:

- Performance metrics.
- Timestamps.
- Numeric table columns.
- Durations.
- Percentages.
- Measurement values.

### 9.3 Layout dimensions

These are recommended starting points for visual validation.

| Element | Suggested Dimension |
|---|---:|
| Expanded sidebar | 240–248 px |
| Compact sidebar | 64–72 px |
| Top header | 64 px |
| Navigation item | 36–40 px |
| Navigation icon | 16–18 px |
| Page content padding | 24 px desktop |
| Compact page padding | 16 px |
| Card padding | 16–20 px |
| Grid gap | 16–24 px |
| Card radius | 8–12 px |
| Control radius | 6–8 px |
| Form control height | 36–40 px |

### 9.4 Spacing scale

Use a consistent spacing scale based on 4 px increments.

| Token | Value |
|---|---:|
| `--space-1` | 4 px |
| `--space-2` | 8 px |
| `--space-3` | 12 px |
| `--space-4` | 16 px |
| `--space-5` | 20 px |
| `--space-6` | 24 px |
| `--space-8` | 32 px |
| `--space-10` | 40 px |
| `--space-12` | 48 px |

Avoid arbitrary spacing values when an existing spacing token can be used.

---

## 10. Component Design Rules

### 10.1 Navigation

- Use a clean sidebar with clear hierarchy.
- Active items should have a subtle tinted background.
- Use a narrow accent indicator where appropriate.
- Selected text and icons should use the theme accent.
- Avoid saturated active pills.
- Preserve readable contrast in expanded and compact modes.
- Use tooltips in compact mode.

### 10.2 Cards

- Use white or theme-defined panel surfaces.
- Use subtle borders.
- Keep shadows minimal.
- Avoid unnecessary nested cards.
- Use consistent card padding and spacing.
- Avoid excessive use of accent-colored borders.

### 10.3 Tables

- Use subtle row separators.
- Align numeric columns consistently.
- Use tabular numerals.
- Provide clear empty, loading, error, and no-results states.
- Use theme accent for selected rows or active table controls, not every row.
- Keep status colors independent.

### 10.4 Forms

- Use persistent labels.
- Provide clear required-field indicators.
- Display inline validation messages.
- Preserve entered values after recoverable errors.
- Use theme accent for focus and selected controls.
- Avoid relying on placeholder text as the only field label.

### 10.5 Dialogs and drawers

- Use consistent overlay and panel surfaces.
- Keep modal dimensions appropriate to the task.
- Use clear primary and secondary actions.
- Ensure focus management and keyboard accessibility.
- Avoid unnecessary confirmation dialogs for reversible operations.

### 10.6 Metrics and KPI cards

- Give the metric value clear visual priority.
- Use appropriate units and contextual labels.
- Use tabular numerals.
- Use accent colors selectively for key highlights.
- Do not make every metric value gold or blue.
- Use status colors only when the metric communicates a status or threshold.

### 10.7 Upload components

Upload-related components should distinguish:

- File selection.
- Upload progress.
- Validation.
- Processing.
- Completion.
- Failure.
- Retry.

Use consistent progress and status patterns.

In simulated environments, clearly label simulated operations. Do not imply that a file has been uploaded to a real backend or cloud storage service.

---

## 11. Theme Behavior and Persistence

### 11.1 Theme identifiers

Preserve the existing theme identifiers:

```ts
type Theme = "classic" | "light" | "hub-light";
```

Display names:

```ts
const themeOptions = [
  {
    value: "classic",
    label: "Kunwei Classic",
  },
  {
    value: "light",
    label: "Performance Light",
  },
  {
    value: "hub-light",
    label: "Performance Hub",
  },
];
```

Adapt the exact type and option structure to the existing implementation.

### 11.2 Theme switching

When users switch themes:

- Update all theme-aware semantic tokens.
- Update primary buttons.
- Update active navigation.
- Update selected tabs and filters.
- Update focused controls.
- Update chart palettes.
- Preserve corporate brand identity.
- Preserve current page and workflow state.
- Do not reload the application.
- Do not reset forms or other user-entered data.

### 11.3 Persistence

- Persist the selected theme independently in each frontend application.
- Preserve existing saved `classic` and `light` preferences.
- Do not override existing user preferences.
- Apply the theme before the first visible paint where practical to avoid a flash of the wrong theme.
- For fresh demo installations without a saved preference, Performance Hub (`hub-light`) may be used as the default if approved by product owners.

### 11.4 Theme selector

Keep the theme selector location consistent across both applications.

Display the theme names clearly rather than relying only on a palette icon.

The selected theme should be immediately recognizable.

---

## 12. Accessibility and Responsive Behavior

Target WCAG 2.2 AA for normal text and interactive controls.

Requirements:

1. Verify text and background contrast.
2. Verify primary and secondary button contrast.
3. Verify active, disabled, hover, and focus states.
4. Verify badge and status indicator contrast.
5. Verify chart series distinguishability.
6. Provide visible keyboard focus.
7. Respect reduced-motion preferences.
8. Support keyboard navigation.
9. Support Chinese and English without clipping.
10. Avoid communicating meaning through color alone.

### 12.1 Gold accessibility requirements

For Performance Hub:

- Use dark foreground text on the proposed gold primary background.
- Test the hover and pressed shades independently.
- Use dark bronze text on soft gold backgrounds.
- Keep warning status visually distinct from gold primary actions.
- Do not use gold text on white for small labels unless contrast is sufficient.

### 12.2 Responsive behavior

- Preserve the desktop sidebar structure.
- Use a drawer or compact navigation on narrow screens.
- Avoid horizontal overflow in tables.
- Allow metric cards to wrap appropriately.
- Keep primary actions accessible on smaller screens.
- Ensure dialogs and drawers fit the available viewport.

---

## 13. Implementation Requirements

The implementation must apply the design system consistently to both frontend applications.

### 13.1 CSS variables

Add or update semantic theme variables in:

- `KWinHub.Web.React/src/index.css`
- `KWinHub.PlatformConsole/src/index.css`

Both applications must use consistent semantic token names and equivalent values for the same theme.

Avoid maintaining different definitions of Performance Hub colors in the two applications.

### 13.2 ThemeSwitcher

Update both ThemeSwitcher implementations to support:

- `classic`
- `light`
- `hub-light`

Keep the existing theme switching behavior and persistence mechanism unless a specific defect requires correction.

### 13.3 Theme types

Update TypeScript theme definitions in both applications.

Avoid duplicating theme-specific color literals in page components.

### 13.4 Tailwind mappings

Keep Tailwind semantic mappings consistent between the two applications.

Existing `surface`, `content`, `border`, and `accent` mappings are a good foundation.

Extend them as necessary to support the complete semantic token contract.

For example:

```js
colors: {
  surface: {
    app: "var(--surface-app)",
    panel: "var(--surface-panel)",
    header: "var(--surface-header)",
    hover: "var(--surface-hover)",
    selected: "var(--surface-selected)",
  },
  content: {
    primary: "var(--text-primary)",
    secondary: "var(--text-secondary)",
    tertiary: "var(--text-tertiary)",
    inverse: "var(--text-inverse)",
  },
  border: {
    DEFAULT: "var(--border-default)",
    subtle: "var(--border-subtle)",
    strong: "var(--border-strong)",
  },
  accent: {
    DEFAULT: "var(--accent-primary)",
    hover: "var(--accent-hover)",
    active: "var(--accent-active)",
    soft: "var(--accent-soft)",
    highlight: "var(--accent-highlight)",
    foreground: "var(--accent-foreground)",
  },
}
```

Adapt this example to the actual Tailwind configuration and version used by the repository.

### 13.5 Hard-coded color audit

Audit both applications for hard-coded color utilities and CSS values that bypass semantic tokens.

Pay particular attention to:

- `ink`
- `kunwei`
- `teal`
- Green button classes.
- Hard-coded selected-navigation colors.
- Hard-coded focus-ring colors.
- Chart colors.
- Status colors.

Do not blindly replace every green value.

Classify each usage first:

1. Corporate brand color — preserve.
2. Theme accent — migrate to `--accent-*`.
3. Status color — migrate to `--status-*`.
4. Visualization color — migrate to `--viz-*`.
5. Neutral surface or text — migrate to the corresponding semantic token.

The goal is to eliminate accidental theme coupling without destroying intentional brand or status colors.

### 13.6 Shared implementation principle

The two applications should follow the same semantic design contract even if their component implementations differ.

Do not introduce an unnecessary shared component framework or restructure the frontend architecture solely to implement theme support.

---

## 14. Visual Regression and QA Requirements

Theme support is not complete until all three themes have been verified across representative screens.

### 14.1 Theme switching tests

Verify:

- Kunwei Classic retains its original green visual identity.
- Performance Light uses its blue accent for primary interactive elements.
- Performance Hub uses gold/bronze for primary interactive elements.
- Company logo and corporate identity remain consistent.
- Theme switching does not reload the application.
- Theme switching does not reset workflow state.
- Saved theme preferences survive application restarts.

### 14.2 Critical Performance Hub checks

The following checks are mandatory:

- Primary buttons use gold rather than corporate green.
- Active navigation uses gold-related styling.
- Selected tabs use gold-related styling.
- Selected filters use gold-related styling.
- Focus indicators use the theme accent.
- Secondary buttons remain neutral.
- Destructive actions remain red.
- Success and warning indicators retain their semantic colors.
- Chart series use the dedicated chart palette.
- No unintended deep-green primary buttons remain.

Deep green is still allowed where the company identity is intentionally displayed.

### 14.3 Representative screens

Capture screenshots for both applications in all three themes.

At minimum, verify:

**KWinHub.Web.React**
- Overview.
- Athlete List.
- Athlete Profile.
- Sessions List.
- Session Detail.
- Sync Center.
- Workflow Explorer.
- Client Upload Simulator under the 演示环境 navigation area.

**KWinHub.PlatformConsole**
- Dashboard.
- Organization Directory.
- Organization Detail.
- Platform Users.
- Protocol Catalog.
- Metric Catalog.
- Algorithm Versions.
- Audit Logs.

### 14.4 Interaction tests

Verify:

- Primary and secondary button states.
- Hover and pressed states.
- Disabled and loading states.
- Input focus and validation.
- Active navigation.
- Tab switching.
- Modal and drawer presentation.
- Table selection.
- Status badges.
- Chart tooltips and legends.

### 14.5 Localization tests

Verify:

- Chinese labels do not clip.
- English labels do not overflow.
- Navigation widths remain appropriate.
- Buttons can accommodate longer localized labels.
- Tables and forms remain readable in both languages.

---

## 15. Implementation Order

Implement the design system incrementally. Avoid a full frontend redesign.

### Phase 1 — Theme foundation

1. Audit existing theme variables and theme-switching mechanisms in both applications.
2. Preserve existing `classic` and `light` implementations.
3. Define the semantic brand/accent separation.
4. Add `hub-light` variables to both CSS files.
5. Define the Performance Light blue accent palette.
6. Define the Performance Hub gold accent palette.
7. Add the new theme to both ThemeSwitcher components and TypeScript theme definitions.
8. Verify theme persistence and initialization.

### Phase 2 — Shared shell and common components

Migrate:

- Sidebar.
- Top header.
- Navigation items.
- Buttons.
- Inputs.
- Selects.
- Tabs.
- Cards.
- Dialogs.
- Status badges.

Prioritize components used throughout both applications.

### Phase 3 — High-traffic pages

Migrate the main pages to semantic tokens.

Pay particular attention to:

- Athlete Profile.
- Session Detail.
- Sessions List.
- Overview.
- Sync Center.
- Organization Directory.
- Protocol Catalog.

### Phase 4 — Visualization and specialized components

1. Centralize chart palettes.
2. Migrate metric cards.
3. Migrate chart legends and highlights.
4. Verify scientific measurement visualizations.
5. Verify upload and processing indicators.

### Phase 5 — Visual regression and refinement

1. Capture screenshots in all three themes.
2. Compare the three theme presentations.
3. Correct inconsistent colors.
4. Verify accessibility.
5. Verify Chinese and English layouts.
6. Run frontend type checks, linting, and production builds.

---

## 16. Definition of Done

The design system enhancement is complete when:

- [ ] All three themes are available in both frontend applications.
- [ ] Existing Kunwei Classic appearance is preserved.
- [ ] Performance Light uses its own professional blue accent.
- [ ] Performance Hub uses a consistent gold/bronze accent.
- [ ] Corporate brand color is independent of theme accent color.
- [ ] Primary buttons use theme-aware accent tokens.
- [ ] Active navigation and selected controls use theme-aware tokens.
- [ ] Status colors remain independent of theme accent colors.
- [ ] Chart palettes are centralized and theme-aware.
- [ ] Shared components no longer contain unintended hard-coded theme colors.
- [ ] Existing theme persistence remains functional.
- [ ] Switching themes does not reset application state.
- [ ] Both applications use consistent semantic token definitions.
- [ ] Chinese and English layouts are verified.
- [ ] WCAG 2.2 AA contrast is checked for important interactive elements.
- [ ] Representative screenshots are captured for all three themes.
- [ ] Both frontend applications pass type checks, linting, and production builds.

---

## 17. Final Design Principle

KWinHub should maintain one corporate identity while offering different visual experiences for different product contexts.

The intended relationship is:

- **Kunwei Classic:** Corporate identity — deep green.
- **Performance Light:** Professional everyday workspace — blue.
- **Performance Hub:** Premium performance analytics workspace — warm gold/bronze.

The corporate green remains part of KWinwei's identity. It should not, however, dictate the appearance of every interactive component in every theme.

**Theme-specific accent colors must be controlled through semantic design tokens.**

This separation is the foundation for a scalable, consistent, and maintainable KWinHub design system.