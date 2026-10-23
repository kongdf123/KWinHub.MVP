import { execFileSync } from 'node:child_process';
import { cpSync, existsSync, mkdirSync, readdirSync, rmSync, statSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

// Builds both KWinHub web frontends as static SPAs (base './') and stages a
// single `frontend/` folder that Tauri bundles as the app asset root:
//   frontend/index.html          <- shell home (from shell/)
//   frontend/web/index.html      <- KWinHub.Web.React
//   frontend/console/index.html  <- KWinHub.PlatformConsole

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const frontendSrc = resolve(root, '..', '..', 'frontend');
const outDir = join(root, 'frontend');

const apps = [
  { name: 'web', dir: join(frontendSrc, 'KWinHub.Web.React') },
  { name: 'console', dir: join(frontendSrc, 'KWinHub.PlatformConsole') },
];

const npm = process.platform === 'win32' ? 'npm.cmd' : 'npm';

function run(cmd, args, cwd) {
  console.log(`\n> ${cmd} ${args.join(' ')}  (cwd: ${cwd})`);
  // shell: true is required on Windows for .cmd shims (npm.cmd) since Node 20.12.
  execFileSync(cmd, args, { cwd, stdio: 'inherit', shell: true });
}

function copyDir(src, dst) {
  mkdirSync(dst, { recursive: true });
  cpSync(src, dst, { recursive: true });
}

function dirStats(dir) {
  let files = 0;
  let bytes = 0;
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const p = join(dir, entry.name);
    if (entry.isDirectory()) {
      const s = dirStats(p);
      files += s.files;
      bytes += s.bytes;
    } else {
      files += 1;
      bytes += statSync(p).size;
    }
  }
  return { files, bytes };
}

for (const app of apps) {
  if (!existsSync(join(app.dir, 'node_modules'))) {
    run(npm, ['install'], app.dir);
  }
  run(npm, ['run', 'build', '--', '--base=./'], app.dir);
}

console.log(`\n> staging assets into ${outDir}`);
rmSync(outDir, { recursive: true, force: true });
mkdirSync(outDir, { recursive: true });

copyDir(join(root, 'shell'), outDir);
for (const app of apps) {
  copyDir(join(app.dir, 'dist'), join(outDir, app.name));
}

console.log('\nStaged asset sizes:');
for (const entry of readdirSync(outDir, { withFileTypes: true })) {
  if (entry.isDirectory()) {
    const s = dirStats(join(outDir, entry.name));
    console.log(`  ${entry.name}/  ${s.files} files, ${(s.bytes / 1024).toFixed(1)} KiB`);
  }
}
console.log('Done.');
