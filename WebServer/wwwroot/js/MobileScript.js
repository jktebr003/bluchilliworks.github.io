window.getScreenWidth = function () {
    return window.innerWidth;
};

window.deviceInfo = {
    isMobile: function () {
        return window.innerWidth <= 768;
    }
};
