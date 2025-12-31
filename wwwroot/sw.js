self.addEventListener('install', (e) => { console.log('Shifa App Installed'); });
self.addEventListener('fetch', (e) => { e.respondWith(fetch(e.request)); });
