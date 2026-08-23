/** Runtime feature gates. Local/demo data is opt-in and never enabled by production builds. */
export const allowLocalFallbacks = import.meta.env.DEV && import.meta.env.VITE_ENABLE_LOCAL_FALLBACKS === 'true';
