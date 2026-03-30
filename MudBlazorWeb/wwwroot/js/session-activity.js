let activityHandler = null;
let lastRefreshAt = 0;
let refreshUrl = null;
let throttleMs = 60000;
let loginUrl = '/authentication/login';

async function pingSession() {
    const now = Date.now();
    if (now - lastRefreshAt < throttleMs) {
        return;
    }

    lastRefreshAt = now;

    const response = await fetch(refreshUrl, {
        method: 'POST',
        credentials: 'same-origin'
    });

    if (response.status === 401) {
        window.location.assign(loginUrl);
    }
}

export function start(targetRefreshUrl, targetThrottleMs, targetLoginUrl) {
    refreshUrl = targetRefreshUrl;
    throttleMs = targetThrottleMs;
    loginUrl = targetLoginUrl;

    if (activityHandler) {
        return;
    }

    activityHandler = () => {
        void pingSession();
    };

    ['click', 'keydown', 'mousemove', 'scroll', 'touchstart'].forEach(eventName => {
        window.addEventListener(eventName, activityHandler, { passive: true });
    });
}

export function stop() {
    if (!activityHandler) {
        return;
    }

    ['click', 'keydown', 'mousemove', 'scroll', 'touchstart'].forEach(eventName => {
        window.removeEventListener(eventName, activityHandler);
    });

    activityHandler = null;
}