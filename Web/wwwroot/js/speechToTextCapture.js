var BlazorSpeechToTextRecorder = {};

(function () {
    var mCaller;

    BlazorSpeechToTextRecorder.Initialize = function (vCaller) {
        mCaller = vCaller;
    };

    BlazorSpeechToTextRecorder.DownloadBlob = function (vUrl, vName) {
        // Create a link element
        const link = document.createElement("a");

        // Set the link's href to point to the Blob URL
        link.href = vUrl;
        link.download = vName;

        // Append link to the body
        document.body.appendChild(link);

        // Dispatch click event on the link
        // This is necessary as link.click() does not work on the latest firefox
        link.dispatchEvent(
            new MouseEvent('click', {
                bubbles: true,
                cancelable: true,
                view: window
            })
        );

        // Remove the link from the body
        document.body.removeChild(link);
    };

    BlazorSpeechToTextRecorder.DownloadFromByteArray = function (byteArray, fileName, contentType) {
        // Wrap it by Blob object.
        const blob = new Blob([byteArray], { type: contentType });

        // Create "object URL" that is linked to the Blob object.
        const url = URL.createObjectURL(blob);

        // Invoke download helper function that implemented in 
        // the earlier section of this article.
        BlazorSpeechToTextRecorder.DownloadBlob(url, fileName);
    }
})();