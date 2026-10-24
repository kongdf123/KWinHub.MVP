# KWinHub WebShell

Windows desktop application (Tauri 2) that hosts **KWinHub Web** (`KWinHub.Web.React`)
and **Platform Console** (`KWinHub.PlatformConsole`) inside a single window, with no
web server, no dev server, and no backend required for the MVP.

## Architecture

```
KWinHub WebShell.exe   (Tauri 2, Windows WebView2)
└── main window
    └── index.html            persistent shell: top tab bar + view stage
        ├── #home             landing cards (optional destination)
        ├── web/index.html    KWinHub Web SPA      (demo data by default)  <- iframe
        └── console/index.html Platform Console SPA (mock API by default) <- iframe
```

- Both frontends are production Vite builds (`--base=./`), packaged as local assets.
- The shell keeps both frontends mounted in hidden iframes and switches which one is
  visible. Switching does **not** reload the page, so each frontend keeps its current
  route and in-memory UI state (e.g. an unsaved KWinHub Web form or the Platform
  Console hash route) when you switch away and back.
- Navigation has three equivalent entry points that all call the same `switchView()`:
  the visible **tab bar** in the shell header, the native **Navigate** menu
  (`Ctrl+0` Home, `Ctrl+1` KWinHub Web, `Ctrl+2` Platform Console), and the shell's
  own keyboard shortcuts.
- KWinHub Web's in-app "Platform Console" link (platform admin login) is intercepted by
  the shell (same-origin iframe access) and switches to the Console view, so the demo's
  in-app navigation still works without modifying either frontend.
- No ASP.NET Core, PostgreSQL, Redis, or MinIO process is started or required.
- Tauri capabilities are minimal: only `core:default` for the `main` window
  (no filesystem, shell, or http permissions).

## Layout

```
KWinHub.WebShell/
├── package.json                 npm scripts (see below)
├── shell/index.html             shell home page (copied into frontend/)
├── scripts/
│   ├── build-frontends.mjs      builds both SPAs, stages frontend/
│   ├── make-icon.ps1            regenerates src-tauri/icons from scripts/icon-source.png
│   └── build-with-msvc.ps1      (fallback) MSVC env + npm run build
├── frontend/                    GENERATED staging dir (git-ignored)
└── src-tauri/
    ├── tauri.conf.json          app config; NSIS + WebView2 downloadBootstrapper
    ├── capabilities/default.json
    └── src/lib.rs               native Navigate menu + webview navigation
```

## Build (reproducible)

Prerequisites on a normal Windows machine:

1. Node.js LTS, Rust (rustup, `stable-x86_64-pc-windows-msvc` default),
   Visual Studio Build Tools ("Desktop development with C++"), WebView2 Runtime.
2. Then:

```powershell
cd src/clients/KWinHub.WebShell
npm install
npm run build          # = build:frontends + cargo release build + NSIS bundling
```

Outputs:

- App exe: `src-tauri/target/release/kwinhub-webshell.exe`
- Installer: `src-tauri/target/release/bundle/nsis/KWinHub WebShell_<version>_x64-setup.exe`

Other scripts:

- `npm run build:frontends` — only rebuild/restage the two SPAs into `frontend/`
- `npm run dev` — build frontends, then `tauri dev`
- `npm run tauri -- <args>` — raw tauri CLI

### This repo's build machine quirks (CJK Windows username)

If the Windows username contains non-ASCII characters (e.g. `孔东方`), MinGW's `ld`
fails with `Permission denied` / `No such file or directory` when writing output
files, and VS Installer components fail with `0x80070005` / exit 5002. Workaround
used here — build with the GNU toolchain from ASCII-only paths:

```powershell
# one-time: copy toolchain to ASCII path and register it
robocopy "$env:USERPROFILE\.rustup\toolchains\stable-x86_64-pc-windows-gnu" `
         D:\KWB\toolchains\stable-x86_64-pc-windows-gnu /E
rustup toolchain link kwh-gnu D:\KWB\toolchains\stable-x86_64-pc-windows-gnu
rustup override set kwh-gnu --path <this repo>\src\clients\KWinHub.WebShell
# one-time: junction with ASCII name pointing at this project (sandboxed agent
# processes may only write inside the workspace; a junction keeps paths ASCII
# while writes land in the workspace)
New-Item -ItemType Junction -Path C:\KWB -Value <this repo>\src\clients\KWinHub.WebShell

# every build: ASCII target + temp through the junction; self-contained dir on
# PATH so rustc finds dlltool.exe / x86_64-w64-mingw32-gcc.exe for windows-gnu
$env:PATH = "D:\KWB\toolchains\stable-x86_64-pc-windows-gnu\lib\rustlib\x86_64-pc-windows-gnu\bin\self-contained;$env:USERPROFILE\.cargo\bin;$env:PATH"
$env:CARGO_TARGET_DIR = "C:\KWB\ct"
$env:TEMP = "C:\KWB\tmp2"; $env:TMP = "C:\KWB\tmp2"
npm run build
```

Do **not** pre-create the target dir; let cargo create it (sandbox overlay quirk:
creating a subdirectory inside a manually created dir outside the workspace is denied).

## Runtime configuration (env vars, baked at frontend build time)

| Variable | Project | Default | Meaning |
|---|---|---|---|
| `VITE_USE_MOCK_API` | PlatformConsole | `true` | `false` → call the real API |
| `VITE_API_BASE_URL` | both | `/api/v1` | REST base URL for API mode |
| `VITE_DATA_MODE` | Web.React | `demo` | `api` → call the real backend |
| `VITE_PLATFORM_CONSOLE_URL` | Web.React | `http://localhost:5174` | console link target outside the shell |

To produce an API-mode desktop build, set these before `npm run build:frontends`
(e.g. `VITE_USE_MOCK_API=false`, `VITE_DATA_MODE=api`,
`VITE_API_BASE_URL=http://localhost:5000/api/v1`).

## Features that still require the real backend

MVP desktop build (defaults above) is fully offline. The following need the
ASP.NET Core API (`src/backend`) once it exists:

- **Real authentication** — both login pages use demo/mock auth; API mode expects
  `POST {base}/DevAuth/token`-style endpoints (see `KWinHub.Web.React/src/lib/api/http.ts`).
- **Persistence of any change** — org/user/device/protocol/metric catalog edits in
  Platform Console; sessions, results, reports, sync jobs, admin CRUD in KWinHub Web.
  All edits currently live in memory and reset on restart.
- **KWinHub Web API coverage** — `services.ts` already delegates to the API in `api`
  mode for the core domains it has mappers for (subjects/athletes, protocols,
  sessions, metric values, devices, etc.); pages without mappers stay on demo data
  until the backend contracts are finalized.
- **Cross-device / multi-user data**, audit log queries, integrations, file/report
  storage (MinIO), and background sync.

## Adding the ASP.NET Core backend later

Two deployment options, both compatible with this shell:

1. **Remote hosted API (simplest)** — run the backend on a server; build the
   frontends in API mode with `VITE_API_BASE_URL` pointing at it. No change to the
   Tauri project.
2. **Local sidecar API (offline desktop)** — publish the API as a self-contained
   exe, register it in `tauri.conf.json` under `bundle.externalBin`, spawn it from
   `src-tauri/src/lib.rs` on startup (random localhost port), and point
   `VITE_API_BASE_URL` at that port. Keep PostgreSQL/Redis/MinIO as separate
   services or move to embedded alternatives for the desktop SKU.

Either way: keep all data access behind the existing service interfaces
(`lib/api.ts`, `lib/services.ts`) so the shell and navigation do not change.
Store any future desktop-only state under the app data dir
(`%APPDATA%/com.kunwei.kwinhub.webshell`), never in the install directory.

## Known limitations

- Windows only (WebView2). NSIS installer downloads WebView2 at install time if
  missing (`downloadBootstrapper`); needs internet on first install unless the
  runtime is already present.
- In-memory state is preserved **within a running session** (the iframes stay
  mounted). It is not persisted across application restarts — mock/demo edits are
  still lost on close, same as before.
- No auto-updater configured.