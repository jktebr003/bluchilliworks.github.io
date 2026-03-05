window.videoRecorder = {
    mediaRecorder: null,
    videoChunks: [],
    startRecording: async function () {
        this.videoChunks = [];
        const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        this.mediaRecorder = new MediaRecorder(stream);
        this.mediaRecorder.ondataavailable = (e) => {
            this.videoChunks.push(e.data);
        };
        this.mediaRecorder.start();
    },
    stopRecording: async function () {
        return new Promise((resolve) => {
            this.mediaRecorder.onstop = async () => {
                const videoBlob = new Blob(this.videoChunks, { type: 'video/mp4' });
                const reader = new FileReader();
                reader.onloadend = function () {
                    resolve(reader.result.split(',')[1]);
                };
                reader.readAsDataURL(videoBlob);
            };
            this.mediaRecorder.stop();
        });
    }
};

window.downloadFileFromUrl = (url, filename) => {
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};