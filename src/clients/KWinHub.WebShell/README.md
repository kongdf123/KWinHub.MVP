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
│   └── build-with-msvc.ps1      sets MSVC env (vswhere auto-detect) + builds frontends + tauri build
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
2. Then — **two build entry points** depending on how MSVC is installed:

```powershell
# A) If link.exe / rc.exe are already on PATH (global MSVC env set up):
cd src/clients/KWinHub.WebShell
npm install
npm run build

# B) If MSVC is installed but env vars are not globally set (this machine):
.\scripts\build-with-msvc.ps1
```

Entry point B is fully portable. It auto-detects Visual Studio / Build Tools
via `vswhere`, locates the Windows SDK via the registry (Microsoft's canonical
`KitsRoot10` value in `HKLM\...\Windows Kits\Installed Roots`) with standard
env-var paths and all-drives scan as fallbacks, sets up `PATH` / `INCLUDE` /
`LIB` for MSVC + the SDK, then runs `build:frontends` + `tauri build` in one
step. It works for Community, Professional, Enterprise, and Build Tools editions
installed on any drive, with the SDK installed on any drive.

> **PowerShell execution policy:** if `npm` or `.ps1` scripts fail with
> `PSSecurityException / UnauthorizedAccess`, run the following **once** in an
> elevated PowerShell to allow local scripts:
> ```powershell
> Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
> ```

### Verified build environment

The versions below were used to produce a successful release build (EXE + NSIS
installer). Team members should match these where possible to avoid toolchain
surprises. Newer minor/patch versions usually work; if a build fails on a
different version, fall back to these.

| Tool | Version | Notes |
|---|---|---|
| OS | Windows 10.0.26300 | Windows 10 22H2 or later required |
| Node.js | v22.14.0 | LTS; any Node 20+ should work |
| npm | 11.12.1 | bundled with Node 22 |
| Rust (rustc) | 1.97.0 (2026-07-07) | via rustup; `rustup default stable-x86_64-pc-windows-msvc` |
| Cargo | 1.97.0 | shipped with the toolchain above |
| Rust toolchain | `stable-x86_64-pc-windows-msvc` | **MSVC, not GNU** (default on Windows) |
| Rust target | `x86_64-pc-windows-msvc` | required for the Windows build |
| Cargo.toml `rust-version` | 1.77.2 | minimum supported Rust version |
| Tauri CLI (npm) | `@tauri-apps/cli@2.12.4` | devDependency in `package.json` |
| Tauri crate (Rust) | `tauri@2.12.5` | resolved in `Cargo.lock` |
| tauri-build | `2.7.4` | resolved in `Cargo.lock` |
| Vite (KWinHub.Web.React) | 5.4.8 | frontend devDependency |
| Vite (KWinHub.PlatformConsole) | 5.4.21 | frontend devDependency |
| Visual Studio Build Tools | installed | workload: "Desktop development with C++" |
| WebView2 Runtime | 155.0.4283.62 | evergreen; installer downloads it if missing |

Quick verification commands (run in PowerShell):

```powershell
node --version                  # v22.x (Node 20+ OK)
npm.cmd --version               # 11.x   (use .cmd if execution policy blocks .ps1)
rustc --version                 # 1.97.0+
cargo --version                 # 1.97.0+
rustup show active-toolchain    # stable-x86_64-pc-windows-msvc
rustc --print host-tuple        # x86_64-pc-windows-msvc
cd src/clients/KWinHub.WebShell
npm.cmd list @tauri-apps/cli    # 2.12.4

# MSVC sanity check (run before building):
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
rustc --print sysroot           # ...\toolchains\stable-x86_64-pc-windows-msvc
```

Outputs:

- App exe: `src-tauri/target/release/kwinhub-webshell.exe`
- Installer: `src-tauri/target/release/bundle/nsis/KWinHub WebShell_<version>_x64-setup.exe`

Other scripts:

- `npm run build:frontends` — only rebuild/restage the two SPAs into `frontend/`
- `npm run dev` — build frontends, then `tauri dev`
- `npm run tauri -- <args>` — raw tauri CLI

### Building with a CJK / non-ASCII Windows username

If the Windows username contains non-ASCII characters (e.g. `孔东方`), be aware
of two toolchain quirks before attempting a build:

1. **MinGW / GNU tools (`ld.exe`, `dlltool.exe`) do not handle Unicode paths.**
   They fail with `Permission denied` / `No such file or directory` when any
   input or output path contains CJK or other non-ASCII characters. This is a
   well-known MinGW limitation, not a Rust or Tauri bug.

2. **Rust's GNU self-contained toolchain ships without `windres.exe`** — the
   MinGW resource compiler that Tauri's `tauri-winres` crate needs to compile
   `.rc` resource files. So even if all paths were ASCII, a pure GNU toolchain
   build still fails with:
   ```
   panicked at tauri-winres: called `Result::unwrap()` on an `Err` value:
   NotAttempted("windres")
   ```
   The GNU toolchain copy under `~/.rustup/toolchains/stable-x86_64-pc-windows-gnu/`
   (and its `self-contained` subdirectory) contains `ld.exe`, `dlltool.exe`,
   `as.exe`, `gcc.exe`, etc. but no `windres.exe`. Installing MSYS2 / standalone
   MinGW would supply `windres.exe`, but that is extra infrastructure most
   Windows dev machines do not have.

**Recommended solution: use the MSVC toolchain.** Visual Studio's native
`link.exe`, `rc.exe`, and the whole MSVC toolset handle Unicode paths correctly.
No path rewiring, junctions, or GNU copies are required. Your `USERPROFILE`
(`C:\Users\孔东方`) and `TEMP`/`TMP` are fully supported as-is.

Run:

```powershell
cd src/clients/KWinHub.WebShell
.\scripts\build-with-msvc.ps1
```

The script is **fully portable** across Windows dev machines. It finds Visual
Studio / Build Tools via `vswhere` (always present on systems that have it),
the Windows SDK via the `KitsRoot10` registry value with well-known env-var
paths and all-drives scan as fallbacks, then sets up `PATH` / `INCLUDE` / `LIB`.
No hardcoded drive letters, no user-account-specific paths, no junction setup,
no GNU toolchain copies needed.

### GNU workaround (kept for reference)

For environments where Visual Studio is not available or must use the GNU
toolchain (e.g. CI runners with MSYS2 that provide `windres.exe`), the original
ASCII-path workaround applies: copy the GNU toolchain to an ASCII-only directory,
link it with rustup, create an ASCII junction to the project, and redirect
`CARGO_TARGET_DIR`, `TEMP`, `TMP` all to ASCII paths. This repo no longer uses
the GNU path — MSVC is both simpler and more reliable here.

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