// Production build values. The real production apiBaseUrl is set at deploy
// time (later slice); services must always read it from here, never hardcode.
export const environment = {
  apiBaseUrl: '/api',
};
