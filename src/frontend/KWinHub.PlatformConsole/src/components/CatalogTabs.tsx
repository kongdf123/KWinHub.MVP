import { Binary, ClipboardList, Cpu, Ruler, type LucideIcon } from 'lucide-react';
import { useI18n, type TranslationKey } from '../lib/i18n';
import { useHashRoute } from '../lib/router';

type CatalogTab = {
  path: string;
  label: TranslationKey;
  icon: LucideIcon;
};

const TABS: CatalogTab[] = [
  { path: '/catalogs/protocols', label: 'nav.protocols', icon: ClipboardList },
  { path: '/catalogs/metrics', label: 'nav.metrics', icon: Ruler },
  { path: '/catalogs/devices', label: 'nav.devices', icon: Cpu },
  { path: '/catalogs/algorithms', label: 'nav.algorithms', icon: Binary },
];

export function CatalogTabs() {
  const { t } = useI18n();
  const { path } = useHashRoute();

  return (
    <nav
      aria-label={t('nav.catalogs')}
      className="mb-6 flex gap-1 overflow-x-auto border-b border-default scrollbar-thin"
    >
      {TABS.map((tab) => {
        const Icon = tab.icon;
        const active = path === tab.path;

        return (
          <a
            key={tab.path}
            href={`#${tab.path}`}
            aria-current={active ? 'page' : undefined}
            className={`-mb-px flex shrink-0 items-center gap-2 border-b-2 px-4 py-3 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-accent motion-reduce:transition-none ${
              active
                ? 'border-accent text-accent'
                : 'border-transparent text-content-secondary hover:text-content-primary'
            }`}
          >
            <Icon size={16} strokeWidth={1.75} />
            {t(tab.label)}
          </a>
        );
      })}
    </nav>
  );
}