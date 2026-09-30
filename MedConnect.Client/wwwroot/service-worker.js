// MedConnect Offline Service Worker
// Cache-first for same-origin assets; navigation falls back to the cached
// app shell so the SPA still loads when the network is unavailable. Bump
// CACHE_VERSION after a deploy that changes the shell or app code.
const CACHE_VERSION = 'v2';
const CACHE_NAME = `medconnect-${CACHE_VERSION}`;
const APP_SHELL = ['./', './index.html', './manifest.webmanifest', './app.css', './css/tokens.css'];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(APP_SHELL)).then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(keys.filter((k) => k !== CACHE_NAME).map((k) => caches.delete(k))))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const { request } = event;
  if (request.method !== 'GET') return;

  const url = new URL(request.url);
  if (url.origin !== self.location.origin) return;

  // Never cache API endpoints, SignalR hubs, or dynamic config/boot files.
  // These must always be fetched fresh from the network / Cloudflare Pages proxy.
  if (
    url.pathname.startsWith('/api') ||
    url.pathname.startsWith('/hubs') ||
    url.pathname.endsWith('/appsettings.json') ||
    url.pathname.endsWith('appsettings.json') ||
    url.pathname.endsWith('blazor.boot.json')
  ) {
    return;
  }

  if (request.mode === 'navigate') {
    event.respondWith(
      fetch(request)
        .then((response) => {
          const copy = response.clone();
          caches.open(CACHE_NAME).then((cache) => cache.put(request, copy));
          return response;
        })
        .catch(() => caches.match('./index.html'))
    );
    return;
  }

  event.respondWith(
    caches.match(request).then((cached) => {
      if (cached) return cached;
      return fetch(request).then((response) => {
        if (response.ok && response.type === 'basic') {
          const copy = response.clone();
          caches.open(CACHE_NAME).then((cache) => cache.put(request, copy));
        }
        return response;
      });
    })
  );
});