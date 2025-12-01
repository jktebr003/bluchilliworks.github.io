// Service Worker version management
const EXPECTED_SW_VERSION = '2.0';

window.updateAvailable = new Promise((resolve, reject) => {
    if (!('serviceWorker' in navigator)) {
        const errorMessage = `This browser doesn't support service workers`;
        console.error(errorMessage);
        reject(errorMessage);
        return;
    }

    // Clear all caches before registering new service worker
    caches.keys().then(cacheNames => {
        return Promise.all(
            cacheNames.map(cacheName => {
                if (cacheName.startsWith('offline-cache-')) {
                    console.log('Deleting old cache:', cacheName);
                    return caches.delete(cacheName);
                }
            })
        );
    }).then(() => {
        // Unregister existing service workers to force fresh registration
        return navigator.serviceWorker.getRegistrations().then(registrations => {
            return Promise.all(registrations.map(registration => {
                console.log('Unregistering old service worker');
                return registration.unregister();
            }));
        });
    }).then(() => {
        // Now register the new service worker
        return navigator.serviceWorker.register('/service-worker.js', { scope: '/' });
    }).then(registration => {
        console.info(`Service worker registration successful (scope: ${registration.scope})`);

        registration.onupdatefound = () => {
            const installingServiceWorker = registration.installing;
            installingServiceWorker.onstatechange = () => {
                if (installingServiceWorker.state === 'installed') {
                    resolve(!!navigator.serviceWorker.controller);
                }
            }
        };
    }).catch(error => {
        console.error('Service worker registration failed with error:', error);
        reject(error);
    });
});

window.registerForUpdateAvailableNotification = (caller, methodName) => {
    window.updateAvailable.then(isUpdateAvailable => {
        if (isUpdateAvailable) {
            caller.invokeMethodAsync(methodName).then();
        }
    });
};