window.cameraRecorder = {
    stream: null,
    currentDeviceId: null,
    availableCameras: [],
    mediaRecorder: null,
    recordedChunks: [],

    async getAvailableCameras() {
        try {
            await navigator.mediaDevices.getUserMedia({ video: true });

            const devices = await navigator.mediaDevices.enumerateDevices();
            let videoDevices = devices.filter(device => device.kind === "videoinput");

            this.availableCameras = videoDevices;
            return videoDevices.map(device => ({ deviceId: device.deviceId, label: device.label || "Camera" }));
        } catch (err) {
            console.error("Error detecting cameras: ", err);
            return [];
        }
    },

    async startCamera(videoElementId, deviceId = null) {
        try {
            let video = document.getElementById(videoElementId);
            if (!video) {
                console.error("Video element not found");
                return;
            }

            if (this.stream) {
                this.stopCamera(videoElementId);
            }

            let constraints = {
                video: deviceId ? { deviceId: { exact: deviceId } } : { facingMode: "user" },
                audio: true  // ✅ Enable microphone
            };

            this.stream = await navigator.mediaDevices.getUserMedia(constraints);
            this.currentDeviceId = deviceId;
            video.srcObject = this.stream;
            await video.play();
        } catch (err) {
            console.error("Error accessing camera/microphone: ", err);
            alert("Camera and microphone access is required.");
        }
    },

    async switchCamera(videoElementId) {
        if (this.availableCameras.length < 2) {
            console.warn("No multiple cameras found.");
            return;
        }

        let currentIndex = this.availableCameras.findIndex(c => c.deviceId === this.currentDeviceId);
        let nextIndex = (currentIndex + 1) % this.availableCameras.length;
        let nextDeviceId = this.availableCameras[nextIndex].deviceId;

        await this.startCamera(videoElementId, nextDeviceId);
    },

    async hasCameraAccess() {
        try {
            await navigator.mediaDevices.getUserMedia({ video: true });
            return true;
        } catch (err) {
            console.error("Camera access denied: ", err);
            return false;
        }
    },

    startRecording() {
        if (!this.stream) {
            console.error("Camera stream is not available.");
            return;
        }

        this.recordedChunks = [];

        let options = { mimeType: "video/webm;codecs=vp9,opus" };  // ✅ Ensure both video & audio are recorded
        this.mediaRecorder = new MediaRecorder(this.stream, options);

        this.mediaRecorder.ondataavailable = (event) => {
            if (event.data.size > 0) {
                this.recordedChunks.push(event.data);
            }
        };

        this.mediaRecorder.onstop = () => {
            let blob = new Blob(this.recordedChunks, { type: "video/webm" });
            let videoUrl = URL.createObjectURL(blob);
            window.cameraRecorder.onVideoRecorded(videoUrl);
        };

        this.mediaRecorder.start();
        console.log("Recording started...");
    },

    stopRecording() {
        if (this.mediaRecorder && this.mediaRecorder.state !== "inactive") {
            this.mediaRecorder.stop();
            console.log("Recording stopped.");
        }
    },

    onVideoRecorded(videoUrl) {
        DotNet.invokeMethodAsync('WCG.POCS.SafetyApp.Web.PWA', 'OnVideoCaptured', videoUrl);
    },

    onVideoRecorded(videoUrl) {
        DotNet.invokeMethodAsync('WCG.POCS.SafetyApp.Web.PWA', 'OnVideoCapture', videoUrl);
    },

    stopCamera(videoElementId) {
        let video = document.getElementById(videoElementId);
        if (video && video.srcObject) {
            let stream = video.srcObject;
            let tracks = stream.getTracks();
            tracks.forEach(track => track.stop());
            video.srcObject = null;
        }
    }
};
