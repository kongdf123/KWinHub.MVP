/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string;
  readonly VITE_DATA_MODE?: string;
  readonly VITE_DEV_USER_ID?: string;
  readonly VITE_DEV_ORG_ID?: string;
  readonly VITE_PLATFORM_CONSOLE_URL?: string;
}
