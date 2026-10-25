import { useCallback, useEffect, useState } from 'react';

export type Theme = 'classic' | 'light';

const STORAGE_KEY = 'kwinhub.platform-console.theme';
const VALID_THEMES: Theme[] = ['classic', 'light'];

function readStoredTheme(): Theme {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored && VALID_THEMES.includes(stored as Theme)) {
      return stored as Theme;
    }
  } catch {
    // localStorage can be unavailable — fall through to the default.
  }
  return 'classic';
}

function applyTheme(theme: Theme) {
  const root = document.documentElement;
  root.setAttribute('data-theme', theme);
}

export function useTheme() {
  const [theme, setThemeState] = useState<Theme>(() => {
    const initial = readStoredTheme();
    if (typeof document !== 'undefined') applyTheme(initial);
    return initial;
  });

  useEffect(() => {
    applyTheme(theme);
    try {
      localStorage.setItem(STORAGE_KEY, theme);
    } catch {
      // Ignore storage failures — the in-memory theme still updates.
    }
  }, [theme]);

  const setTheme = useCallback((next: Theme) => {
    if (!VALID_THEMES.includes(next)) return;
    setThemeState(next);
  }, []);

  return { theme, setTheme };
}