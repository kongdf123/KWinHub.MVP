import { useEffect, useRef, useState } from 'react';
import { Palette, Check } from 'lucide-react';
import { useTheme, type Theme } from '@/lib/use-theme';

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
        className="flex items-center gap-1.5 px-2.5 py-2 text-sm font-medium text-content-secondary hover:bg-surface-hover rounded-lg transition-colors"
        title="Theme"
        aria-haspopup="listbox"
        aria-expanded={open}
      >
        <Palette className="w-4 h-4" />
      </button>
      {open && (
        <div className="absolute right-0 top-full mt-1 w-64 bg-surface-panel rounded-lg shadow-lg border border-default py-1 z-20 animate-slide-up" role="listbox">
          <div className="px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-content-secondary">Theme</div>
          {OPTIONS.map((opt) => (
            <button
              key={opt.value}
              type="button"
              onClick={() => { setTheme(opt.value); setOpen(false); }}
              className={`w-full flex items-start gap-2 px-3 py-2 text-left hover:bg-surface-hover transition-colors ${opt.value === theme ? 'text-accent' : 'text-content-primary'}`}
              role="option"
              aria-selected={opt.value === theme}
            >
              <div className="mt-0.5 shrink-0">
                {opt.value === theme ? (
                  <Check className="w-4 h-4 text-accent" />
                ) : (
                  <div className="w-4 h-4 rounded-full border border-default" />
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