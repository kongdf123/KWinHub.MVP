//! KWinHub WebShell — single-window Tauri host for the two KWinHub web
//! frontends. A persistent HTML shell (`index.html`) keeps both frontends
//! mounted in hidden iframes; the native "Navigate" menu and its keyboard
//! shortcuts toggle which iframe is visible instead of reloading the page,
//! so each frontend's route and in-memory UI state are preserved. All assets
//! are bundled locally; no web server is involved.

use tauri::menu::{MenuBuilder, MenuItemBuilder, SubmenuBuilder};
use tauri::Manager;

const NAV_HOME: &str = "nav.home";
const NAV_WEB: &str = "nav.web";
const NAV_CONSOLE: &str = "nav.console";

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .setup(|app| {
            let home = MenuItemBuilder::with_id(NAV_HOME, "Home")
                .accelerator("CmdOrCtrl+0")
                .build(app)?;
            let web = MenuItemBuilder::with_id(NAV_WEB, "KWinHub Web")
                .accelerator("CmdOrCtrl+1")
                .build(app)?;
            let console = MenuItemBuilder::with_id(NAV_CONSOLE, "Platform Console")
                .accelerator("CmdOrCtrl+2")
                .build(app)?;

            let navigate = SubmenuBuilder::new(app, "Navigate")
                .items(&[&home, &web, &console])
                .build()?;
            let menu = MenuBuilder::new(app).items(&[&navigate]).build()?;
            app.set_menu(menu)?;
            Ok(())
        })
        .on_menu_event(|app, event| {
            let view = match event.id().as_ref() {
                NAV_HOME => "home",
                NAV_WEB => "web",
                NAV_CONSOLE => "console",
                _ => return,
            };
            if let Some(window) = app.get_webview_window("main") {
                // Delegate to the persistent shell: it toggles iframe visibility
                // rather than reloading the page, so each frontend keeps its
                // current route and in-memory state.
                let script = format!("if (typeof window.switchView === 'function') {{ window.switchView({:?}); }}", view);
                let _ = window.eval(script);
            }
        })
        .run(tauri::generate_context!())
        .expect("error while running KWinHub WebShell");
}