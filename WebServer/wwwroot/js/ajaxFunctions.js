var BlazorJavascriptFunctions = {};
(function () {
    var mCaller;

    BlazorJavascriptFunctions.Initialize = function (vCaller) {
        mCaller = vCaller;
    };

    BlazorJavascriptFunctions.ScrollToTop = function () {
        document.documentElement.scrollTop = 0;
    }; 

    BlazorJavascriptFunctions.DownloadFromUrl = async function (options) {
        var _a;
        var anchorElement = document.createElement('a');
        anchorElement.href = options.url;
        anchorElement.download = (_a = options.fileName) !== null && _a !== void 0 ? _a : '';
        anchorElement.click();
        anchorElement.remove();
    };

    BlazorJavascriptFunctions.DownloadFromByteArray = async function (options) {
        var url = typeof (options.byteArray) === 'string' ? "data:" + options.contentType + ";base64," + options.byteArray : URL.createObjectURL(new Blob([options.byteArray], { type: options.contentType }));

        BlazorJavascriptFunctions.DownloadFromUrl({ url: url, fileName: options.fileName });
        if (typeof (options.byteArray) !== 'string')
            URL.revokeObjectURL(url);
    };
})();