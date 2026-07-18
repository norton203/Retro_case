// Sound effects only (the ambient per-machine chiptune playlist was dropped in favour
// of the real SID player). stopMusic() is kept as a no-op so the SID bridge's calls to
// it don't need special-casing.

(function () {
    let ctx = null;

    function ensureCtx() {
        if (!ctx) {
            ctx = new (window.AudioContext || window.webkitAudioContext)();
        }
        if (ctx.state === "suspended") {
            ctx.resume();
        }
        return ctx;
    }

    function stopMusic() {
        // no-op — kept so other modules (e.g. the SID bridge) can call this safely
    }

    function playModemConnect() {
        const c = ensureCtx();
        const now = c.currentTime;
        const totalDuration = 2.2;

        // Frequency sweep standing in for a dial/negotiate tone.
        const osc = c.createOscillator();
        const oscGain = c.createGain();
        osc.type = 'sine';
        osc.frequency.setValueAtTime(400, now);
        osc.frequency.linearRampToValueAtTime(2400, now + 0.5);
        osc.frequency.linearRampToValueAtTime(1200, now + 1.0);
        oscGain.gain.setValueAtTime(0.05, now);
        oscGain.gain.linearRampToValueAtTime(0, now + 1.1);
        osc.connect(oscGain);
        oscGain.connect(c.destination);
        osc.start(now);
        osc.stop(now + 1.1);

        // Noise burst standing in for handshake static.
        const bufferSize = Math.floor(c.sampleRate * 1.0);
        const buffer = c.createBuffer(1, bufferSize, c.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = (Math.random() * 2 - 1) * 0.2;
        }
        const noise = c.createBufferSource();
        noise.buffer = buffer;
        const noiseGain = c.createGain();
        noiseGain.gain.setValueAtTime(0, now + 0.9);
        noiseGain.gain.linearRampToValueAtTime(0.18, now + 1.0);
        noiseGain.gain.linearRampToValueAtTime(0, now + totalDuration);
        noise.connect(noiseGain);
        noiseGain.connect(c.destination);
        noise.start(now + 0.9);
        noise.stop(now + totalDuration);

        return new Promise(resolve => setTimeout(resolve, totalDuration * 1000));
    }

    function getPref(key, fallback) {
        const v = localStorage.getItem(key);
        if (v === null) return fallback;
        return v === 'true';
    }

    function setPref(key, value) {
        localStorage.setItem(key, value ? 'true' : 'false');
    }

    window.threebitAudio = { stopMusic, playModemConnect, getPref, setPref };
})();