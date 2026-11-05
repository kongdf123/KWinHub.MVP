# 03 — KWinHub Design System & Theme Tokens

**Status:** Proposed · **Scope:** Both frontends

## 1. Theme recommendation

The repository already implements semantic `classic` and `light` themes in both `src/index.css` files, and both apps have theme switchers. Preserve both identifiers and add a third theme rather than replacing the current light theme.

| Theme ID | Display name | Direction |
|---|---|---|
| `classic` | Kunwei Classic | Existing brand-forward appearance; preserve |
| `light` | Performance Light | Existing clean light theme; preserve |
| `hub-light` | Performance Hub | New neutral, data-first, VALD-inspired workspace aesthetic |

Use general design qualities—clarity, restrained color, compact navigation, calm surfaces and strong metric hierarchy. Do not copy VALD's exact brand, assets, wording or page layout.

Performance Light already uses light surfaces and green accents. Performance Hub should therefore be more than a white sidebar: use a cooler neutral canvas, restrained deeper teal, stronger metric typography, subtle borders, fewer decorative colors and deliberate analytical density.

## 2. Proposed Performance Hub tokens

Starting values for visual validation, not final brand standards:

| Token | Value | Use |
|---|---|---|
| `--surface-app` | `#F3F5F7` | Main canvas |
| `--surface-panel` | `#FFFFFF` | Cards/dialogs |
| `--surface-header` | `#FFFFFF` | Header |
| `--surface-hover` | `#F1F4F5` | Quiet hover surface |
| `--surface-sidebar` | `#FFFFFF` | Sidebar |
| `--surface-sidebar-hover` | `#F2F5F5` | Sidebar hover |
| `--surface-sidebar-active` | `#E8F2F0` | Selected navigation |
| `--text-primary` | `#17242B` | Main text |
| `--text-secondary` | `#687780` | Secondary text |
| `--text-sidebar` | `#34434A` | Sidebar text |
| `--text-sidebar-active` | `#176F63` | Selected item |
| `--text-sidebar-muted` | `#7B898F` | Sidebar metadata |
| `--border-default` | `#DFE5E8` | Standard border |
| `--border-sidebar` | `#E3E8EA` | Sidebar divider |
| `--accent-primary` | `#16786B` | Primary action |
| `--accent-hover` | `#105F55` | Primary hover |
| `--accent-soft` | `#E7F2EF` | Soft accent surface |
| `--accent-highlight` | `#4A9E91` | Indicator |
| `--status-success` | `#21836E` | Success |
| `--status-warning` | `#A96F16` | Warning |
| `--status-danger` | `#B64D4D` | Error |
| `--focus-ring` | `#16786B` | Keyboard focus |

### CSS starter

```css
[data-theme="hub-light"] {
  color-scheme: light;
  --surface-app: #F3F5F7;
  --surface-panel: #FFFFFF;
  --surface-header: #FFFFFF;
  --surface-hover: #F1F4F5;
  --surface-sidebar: #FFFFFF;
  --surface-sidebar-hover: #F2F5F5;
  --surface-sidebar-active: #E8F2F0;
  --text-primary: #17242B;
  --text-secondary: #687780;
  --text-sidebar: #34434A;
  --text-sidebar-active: #176F63;
  --text-sidebar-muted: #7B898F;
  --border-default: #DFE5E8;
  --border-sidebar: #E3E8EA;
  --accent-primary: #16786B;
  --accent-hover: #105F55;
  --accent-soft: #E7F2EF;
  --accent-highlight: #4A9E91;
  --status-success: #21836E;
  --status-warning: #A96F16;
  --status-danger: #B64D4D;
  --focus-ring: #16786B;
}
```

## 3. Visualization tokens

Suggested chart palette: `--viz-series-1: #16786B`, `--viz-series-2: #4777A8`, `--viz-series-3: #8A6BB0`, `--viz-series-4: #C18A3B`, `--viz-series-5: #5F8D68`, `--viz-grid: #E7ECEE`, `--viz-axis: #718087`, `--viz-reference: #8A969C`, `--viz-highlight: #DCEFEA`.

Keep left/right and limb mapping stable. Always label series; do not rely on red/green alone.

## 4. Semantic token contract

Components should use semantic roles rather than literal theme palette classes. Groups: surfaces (app/panel/header/hover/sidebar/active), content (primary/secondary/sidebar), borders (default/sidebar/strong), accent (primary/hover/soft/highlight), status (success/warning/danger/info/neutral), focus, and visualization.

Keep Tailwind semantic mappings consistent between apps. Existing `surface`, `content`, `border` and `accent` mappings are a good foundation. Audit hard-coded `ink`, `kunwei`, `teal` and status utilities that bypass semantic variables.

## 5. Typography and layout

Retain `Inter, "PingFang SC", "Microsoft YaHei", system-ui, sans-serif`.

| Role | Suggested size |
|---|---:|
| Page title | 24–28 px, semibold |
| Section title | 16–18 px, semibold |
| Card title | 14–16 px, semibold |
| Body | 13–14 px |
| Metadata | 12 px with adequate contrast |
| Table header | 11–12 px |
| Large metric | 32–48 px |
| Metric unit | 12–14 px |

Use tabular numerals for metrics, timestamps and numeric columns. Layout starting points: expanded sidebar 240–248 px; compact 64–72 px; header 64 px; nav item 36–40 px; icon 16–18 px; card radius 8–12 px; control radius 6–8 px; page padding 24 px desktop/16 px compact; card padding 16–20 px; grid gap 16–24 px; form control height 36–40 px.

## 6. Component rules

- **Navigation:** subtle tinted active surface, readable text and narrow accent indicator; avoid saturated active pills.
- **Cards:** white surface, subtle border, minimal shadow; avoid unnecessary nested cards.
- **Buttons:** primary/secondary/tertiary/destructive hierarchy; loading prevents duplicate submission.
- **Tables:** subtle separators, tabular numeric alignment, clear empty/loading/error/no-results states.
- **Forms:** persistent labels, inline validation, preserve values after recoverable errors.
- **Status:** pair color with text/icon.
- **Charts:** correct units, axes, legends and event markers; provide textual summaries for key values.

## 7. Theme behavior and accessibility

Keep `classic` as default unless product owners approve a change. Preserve saved `classic`/`light` preferences. Add `hub-light` to both switchers and theme types. Persist independently per app and apply before first paint where practical. Keep selector location consistent and display theme names, not just a palette icon. Switching must not reload or reset workflow state.

Target WCAG 2.2 AA for normal text and interactive controls. Verify text, disabled, active, focus, badge and chart contrast. Keyboard focus must be visible; respect reduced motion; support Chinese and English without clipping.

## 8. Implementation order

1. Add identical `hub-light` values to both `src/index.css` files.
2. Add option to both ThemeSwitcher components and `Theme` type definitions.
3. Keep Tailwind semantic mappings aligned.
4. Migrate shared shell and high-traffic screens away from hard-coded colors.
5. Centralize theme-aware chart colors.
6. Capture visual regression screenshots for representative pages in all three themes.
