window.audioCapture = {
    recognition: null,
    isListening: false,
    dotNetObject: null,

    startListening: function (dotNetObject) {
        if (!window.SpeechRecognition && !window.webkitSpeechRecognition) {
            alert("Speech Recognition is not supported in this browser.");
            return;
        }

        if (window.audioCapture.isListening) {
            console.log("Already listening...");
            return;
        }

        window.audioCapture.dotNetObject = dotNetObject;

        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        window.audioCapture.recognition = new SpeechRecognition();
        let recognition = window.audioCapture.recognition;

        recognition.lang = 'en-ZA';
        recognition.interimResults = false;
        recognition.maxAlternatives = 1;
        recognition.continuous = true;

        recognition.onstart = function () {
            window.audioCapture.isListening = true;
            console.log("Speech recognition started...");
            dotNetObject.invokeMethodAsync('OnStatusChanged', "🎙️ Recording...");
        };

        recognition.onresult = function (event) {
            let transcript = event.results[event.results.length - 1][0].transcript;
            console.log("Transcription: ", transcript);
            if (window.audioCapture.dotNetObject) {
                window.audioCapture.dotNetObject.invokeMethodAsync('OnSpeechRecognized', transcript);
            }
        };

        recognition.onerror = function (event) {
            console.error("Speech recognition error:", event.error);
            dotNetObject.invokeMethodAsync('OnStatusChanged', "⚠️ Error: " + event.error);
            if (event.error === 'no-speech' || event.error === 'network') {
                console.log("Restarting speech recognition...");
                recognition.start();
            }
        };

        recognition.onend = function () {
            window.audioCapture.isListening = false;
            console.log("Speech recognition stopped.");
            dotNetObject.invokeMethodAsync('OnStatusChanged', "🛑 Stopped");
            if (!window.audioCapture.isStoppedManually) {
                console.log("Restarting speech recognition...");
                recognition.start();
            }
        };

        recognition.start();
    },

    stopListening: function () {
        if (window.audioCapture.recognition) {
            window.audioCapture.isStoppedManually = true;
            window.audioCapture.recognition.stop();
            window.audioCapture.isListening = false;
            console.log("Stopped listening.");
            window.audioCapture.dotNetObject.invokeMethodAsync('OnStatusChanged', "🛑 Stopped");
        }
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
