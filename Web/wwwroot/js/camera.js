window.cameraCapture = {
    stream: null,
    currentDeviceId: null,
    availableCameras: [],

    async getAvailableCameras() {
        try {
            // Request camera access first (required on iOS)
            await navigator.mediaDevices.getUserMedia({ video: true });

            // Now, enumerate available cameras
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
                video: deviceId ? { deviceId: { exact: deviceId } } : { facingMode: "user" }
            };

            this.stream = await navigator.mediaDevices.getUserMedia(constraints);
            this.currentDeviceId = deviceId;
            video.srcObject = this.stream;
            await video.play();
        } catch (err) {
            console.error("Error accessing the camera: ", err);
            alert("Camera access is required.");
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
            // Try to access the camera
            await navigator.mediaDevices.getUserMedia({ video: true });
            return true;
        } catch (err) {
            console.error("Camera access denied: ", err);
            return false;
        }
    },

    captureImage(videoElementId, canvasElementId) {
        let video = document.getElementById(videoElementId);
        let canvas = document.getElementById(canvasElementId);

        if (!video || !canvas) {
            console.error("Video or Canvas element not found");
            return null;
        }

        let context = canvas.getContext("2d");
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        context.drawImage(video, 0, 0, canvas.width, canvas.height);

        return canvas.toDataURL("image/png");
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
