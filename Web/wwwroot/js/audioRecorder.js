// wwwroot/js/audioRecorder.js
window.audioRecorder = {
    mediaRecorder: null,
    audioChunks: [],
    startRecording: async function () {
        this.audioChunks = [];
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        this.mediaRecorder = new MediaRecorder(stream);
        this.mediaRecorder.ondataavailable = (e) => {
            this.audioChunks.push(e.data);
        };
        this.mediaRecorder.start();
    },
    stopRecording: async function () {
        return new Promise((resolve) => {
            this.mediaRecorder.onstop = async () => {
                const audioBlob = new Blob(this.audioChunks, { type: 'audio/mp4' });
                const reader = new FileReader();
                reader.onloadend = function () {
                    // Return base64 string
                    resolve(reader.result.split(',')[1]);
                };
                reader.readAsDataURL(audioBlob);
            };
            this.mediaRecorder.stop();
        });
    },
    hasMicrophoneAccess: async function () {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            stream.getTracks().forEach(track => track.stop()); // Stop the stream immediately
            return true;
        } catch (error) {
            console.error('Microphone access denied:', error);
            return false;
        }
    }
};