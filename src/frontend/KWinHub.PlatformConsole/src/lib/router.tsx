import { useEffect, useState } from 'react';

export type Route = { path: string; params: Record<string, string>; query: URLSearchParams };

function readPath(): string {
  const raw = window.location.hash.replace(/^#/, '');
  const path = raw.split('?')[0];
  if (!path) return '/';
  return path.startsWith('/') ? path : `/${path}`;
}

function readQuery(): URLSearchParams {
  const raw = window.location.hash.replace(/^#/, '');
  const queryPart = raw.split('?')[1] ?? '';
  return new URLSearchParams(queryPart);
}

export function useHashRoute(): Route {
  const [path, setPath] = useState<string>(readPath);
  const [query, setQuery] = useState<URLSearchParams>(readQuery);

  useEffect(() => {
    const handleChange = () => {
      setPath(readPath());
      setQuery(readQuery());
    };
    window.addEventListener('hashchange', handleChange);
    return () => window.removeEventListener('hashchange', handleChange);
  }, []);

  return { path, params: {}, query };
}

export function navigate(path: string): void {
  window.location.hash = path.startsWith('/') ? path : `/${path}`;
}

export function matchRoute(pattern: string, path: string): Record<string, string> | null {
  const patternParts = pattern.split('/').filter(Boolean);
  const pathParts = path.split('/').filter(Boolean);

  if (patternParts.length !== pathParts.length) return null;

  const params: Record<string, string> = {};

  for (let i = 0; i < patternParts.length; i += 1) {
    const patternPart = patternParts[i];
    const pathPart = pathParts[i];

    if (patternPart.startsWith(':')) {
      params[patternPart.slice(1)] = decodeURIComponent(pathPart);
    } else if (patternPart !== pathPart) {
      return null;
    }
  }

  return params;
}