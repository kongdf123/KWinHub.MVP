import { CreditCard, ShieldCheck, type LucideIcon } from 'lucide-react';
import { useI18n, type TranslationKey } from '../lib/i18n';
import { useHashRoute } from '../lib/router';

type SubscriptionTab = {
  path: string;
  label: TranslationKey;
  icon: LucideIcon;
};

const TABS: SubscriptionTab[] = [
  { path: '/subscriptions/plans', label: 'nav.subscriptionPlans', icon: CreditCard },
  { path: '/subscriptions', label: 'nav.subscriptions', icon: ShieldCheck },
];

export function SubscriptionTabs() {
  const { t } = useI18n();
  const { path } = useHashRoute();

  return (
    <nav
      aria-label={t('nav.subscriptions')}
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