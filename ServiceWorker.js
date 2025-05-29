const cacheName = "Umter Studios-the_clash_of_civilizations-0.1";
const contentToCache = [
    "Build/Export-The-Clash-Of-Civilizations.loader.js",
    "Build/Export-The-Clash-Of-Civilizations.framework.js",
    "Build/Export-The-Clash-Of-Civilizations.data",
    "Build/Export-The-Clash-Of-Civilizations.wasm",
    "TemplateData/style.css"

];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
});

self.addEventListener('fetch', function (e) {
    e.respondWith((async function () {
      let response = await caches.match(e.request);
      console.log(`[Service Worker] Fetching resource: ${e.request.url}`);
      if (response) { return response; }

      response = await fetch(e.request);
      const cache = await caches.open(cacheName);
      console.log(`[Service Worker] Caching new resource: ${e.request.url}`);
      cache.put(e.request, response.clone());
      return response;
    })());
});
