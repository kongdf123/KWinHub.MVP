import { useEffect, useRef, useState } from 'react';
import { Check, Palette } from 'lucide-react';
import { useTheme, type Theme } from '../lib/use-theme';

const OPTIONS: { value: Theme; label: string; desc: string }[] = [
  { value: 'classic', label: 'Kunwei Classic', desc: 'Dark navigation, classic palette' },
  { value: 'light', label: 'Performance Light', desc: 'Light surfaces, restrained accents' },
];

export function ThemeSwitcher() {
  const { theme, setTheme } = useTheme();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  return (
    <div className="relative" ref={ref}>
      <button
        type="button"
        onClick={() => setOpen(!open)}
        className="flex h-8 items-center gap-1.5 rounded-md border border-default bg-surface-panel px-2.5 text-sm text-content-secondary transition-colors hover:bg-surface-hover"
        title="Theme"
        aria-haspopup="listbox"
        aria-expanded={open}
      >
        <Palette className="h-4 w-4" />
      </button>
      {open && (
        <div
          className="absolute right-0 top-full z-20 mt-1 w-64 rounded-lg border border-default bg-surface-panel py-1 shadow-lg"
          role="listbox"
        >
          <div className="px-3 py-1.5 text-[11px] font-semibold uppercase tracking-wide text-content-secondary">
            Theme
          </div>
          {OPTIONS.map((opt) => (
            <button
              key={opt.value}
              type="button"
              onClick={() => {
                setTheme(opt.value);
                setOpen(false);
              }}
              className={`flex w-full items-start gap-2 px-3 py-2 text-left transition-colors hover:bg-surface-hover ${
                opt.value === theme ? 'text-accent' : 'text-content-primary'
              }`}
              role="option"
              aria-selected={opt.value === theme}
            >
              <div className="mt-0.5 shrink-0">
                {opt.value === theme ? (
                  <Check className="h-4 w-4 text-accent" />
                ) : (
                  <div className="h-4 w-4 rounded-full border border-default" />
                )}
              </div>
              <div className="min-w-0">
                <div className="text-sm font-medium">{opt.label}</div>
                <div className="text-xs text-content-secondary">{opt.desc}</div>
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}