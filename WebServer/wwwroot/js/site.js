window.getSouthAfricaDateTime = function () {
    const options = {
        timeZone: 'Africa/Johannesburg',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false
    };

    return new Intl.DateTimeFormat('en-ZA', options).format(new Date());
};

window.getSouthAfricaTime = function () {
    // South Africa Standard Time is UTC+2, no daylight saving
    const nowUtc = new Date();
    // Get UTC time in milliseconds
    const utc = nowUtc.getTime() + (nowUtc.getTimezoneOffset() * 60000);
    // Add 2 hours for South Africa
    const saTime = new Date(utc + (2 * 60 * 60 * 1000));
    return saTime.toISOString();
};

window.getCurrentPosition = function () {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject("Geolocation is not supported by this browser.");
            return;
        }

        navigator.geolocation.getCurrentPosition(
            (position) => {
                resolve({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                });
            },
            (error) => {
                reject(error.message);
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0,
            }
        );
    });
};

window.getScreenDimensions = function () {
    return {
        width: window.innerWidth,
        height: window.innerHeight,
        availableHeight: window.innerHeight - window.outerHeight + window.innerHeight,
        devicePixelRatio: window.devicePixelRatio || 1
    };
};

window.getViewportInfo = function () {
    return {
        width: Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0),
        height: Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0),
        scrollHeight: document.body.scrollHeight,
        clientHeight: document.documentElement.clientHeight,
        isLandscape: window.innerWidth > window.innerHeight
    };
};

window.addResizeListener = function (dotNetHelper, methodName) {
    const resizeHandler = () => {
        const viewportInfo = window.getViewportInfo();
        dotNetHelper.invokeMethodAsync(methodName, viewportInfo);
    };

    window.addEventListener('resize', resizeHandler);
    window.addEventListener('orientationchange', resizeHandler);

    // Return initial viewport info
    return window.getViewportInfo();
};

window.removeResizeListener = function () {
    // Clean up event listeners if needed
    window.removeEventListener('resize', window.resizeHandler);
    window.removeEventListener('orientationchange', window.resizeHandler);
};

window.areMediaDevicesActive = function () {
    // Check if there are any active media streams (camera or microphone)
    const streams = [];
    if (window.localStream) streams.push(window.localStream);
    if (window.localAudioStream) streams.push(window.localAudioStream);
    if (window.localVideoStream) streams.push(window.localVideoStream);

    // Check all tracks in all known streams
    for (const stream of streams) {
        if (!stream) continue;
        const tracks = stream.getTracks();
        if (
            tracks.some((track) => track.readyState === "live" && track.enabled)
        ) {
            return true;
        }
    }
    // Optionally, check all media devices (if you manage them globally)
    return false;
};

window.safeGetCurrentPosition = async function () {
    if (window.areMediaDevicesActive()) {
        return Promise.reject(
            "Please turn off camera and microphone before using location."
        );
    }
    return window.getCurrentPosition();
};