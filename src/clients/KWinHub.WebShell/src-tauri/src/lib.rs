//! KWinHub WebShell — single-window Tauri host for the two KWinHub web
//! frontends. A native "Navigate" menu switches the webview between the
//! shell home page, KWinHub Web, and Platform Console. All assets are
//! bundled locally; no web server is involved.

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
            let path = match event.id().as_ref() {
                NAV_HOME => "index.html",
                NAV_WEB => "web/index.html",
                NAV_CONSOLE => "console/index.html",
                _ => return,
            };
            if let Some(window) = app.get_webview_window("main") {
                // Tauri maps tauri://localhost to the platform-specific asset
                // protocol base (http://tauri.localhost on Windows).
                let url = tauri::Url::parse(&format!("tauri://localhost/{path}"))
                    .expect("valid app url");
                let _ = window.navigate(url);
            }
        })
        .run(tauri::generate_context!())
        .expect("error while running KWinHub WebShell");
}
