// Bridge between Blazor and the vendored jsSID engine (see sid/LICENSE-jsSID.md — GPLv2).
//
// Twist: rather than handing playback off to pico.js as-is, we pull raw samples straight
// from the SID emulator ourselves and reshape them per selected machine before they reach
// the speakers:
//   - C64: untouched — the real SID chip emulation, as authored.
//   - Spectrum: crushed to 1-bit (on/off) to approximate the ZX beeper, which had no
//     volume levels at all, just a speaker being toggled.
//   - Amstrad: quantized to a coarser step count to approximate the AY-3-8912's simpler
//     (but not fully 1-bit) volume envelope.
// This only reshapes the *timbre* of whatever file the visitor uploads — it doesn't alter
// or synthesize any copyrighted composition, just how it "sounds" coming out of each chip.
(function () {
    let ctx = null;
    let sidPlayer = null;
    let processorNode = null;
    let playing = false;
    let chipMode = "c64"; // c64 | spectrum | amstrad

    const ALLOWED_RATES = [8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000];

    function ensureCtx() {
        if (!ctx) {
            ctx = new (window.AudioContext || window.webkitAudioContext)();
        }
        if (ctx.state === "suspended") {
            ctx.resume();
        }
        return ctx;
    }

    function closestAllowedRate(actual) {
        return ALLOWED_RATES.reduce((best, sr) =>
            Math.abs(sr - actual) < Math.abs(best - actual) ? sr : best, 44100);
    }

    function meta() {
        if (!sidPlayer) return null;
        const f = sidPlayer.getSidFile();
        const clean = (s) => (s || "").split("\0")[0].trim();
        return {
            name: clean(f.name) || "Untitled",
            author: clean(f.author) || "Unknown",
            published: clean(f.published) || "",
            currentTrack: f.getCurrentSong() + 1,
            totalTracks: f.getSubSongs() + 1
        };
    }

    function setChipMode(slug) {
        chipMode = (slug === "spectrum" || slug === "amstrad") ? slug : "c64";
    }

    // Reshape one raw SID sample (-1..1) to approximate a simpler chip's output.
    function shapeSample(v) {
        if (chipMode === "spectrum") {
            return v >= 0 ? 0.5 : -0.5; // 1-bit beeper: on or off, nothing between
        }
        if (chipMode === "amstrad") {
            const steps = 16; // AY-3-8912-ish coarse volume steps
            return Math.round(v * steps) / steps;
        }
        return v; // c64 — real SID output, untouched
    }

    function stopNode() {
        if (processorNode) {
            processorNode.disconnect();
            processorNode.onaudioprocess = null;
            processorNode = null;
        }
    }

    function loadFromBase64(base64) {
        if (window.threebitAudio) window.threebitAudio.stopMusic();
        stopNode();

        const c = ensureCtx();
        // jsSID's SIDPlayer reads its mix rate from the global pico object rather than
        // from constructor options, so we align pico's rate with our own AudioContext
        // to keep pitch/tempo accurate.
        if (window.pico && window.pico.setup) {
            window.pico.setup({ samplerate: closestAllowedRate(c.sampleRate) });
        }

        const binary = atob(base64);
        sidPlayer = new jsSID.SIDPlayer({ quality: jsSID.quality.medium });
        sidPlayer.loadFileFromData(binary);
        playing = false;
        return meta();
    }

    function play() {
        if (!sidPlayer) return;
        if (window.threebitAudio) window.threebitAudio.stopMusic();
        const c = ensureCtx();
        stopNode();

        processorNode = c.createScriptProcessor(2048, 0, 2);
        const mono = new Float32Array(processorNode.bufferSize);
        processorNode.onaudioprocess = function (e) {
            const outL = e.outputBuffer.getChannelData(0);
            const outR = e.outputBuffer.getChannelData(1);
            const written = sidPlayer.generateIntoBuffer(mono.length, mono, 0);
            for (let i = 0; i < mono.length; i++) {
                const v = i < written ? shapeSample(mono[i]) : 0;
                outL[i] = v;
                outR[i] = v;
            }
        };
        processorNode.connect(c.destination);
        playing = true;
    }

    function stop() {
        stopNode();
        playing = false;
    }

    function next() {
        if (!sidPlayer) return null;
        sidPlayer.nextTrack();
        if (playing) { play(); }
        return meta();
    }

    function prev() {
        if (!sidPlayer) return null;
        sidPlayer.prevTrack();
        if (playing) { play(); }
        return meta();
    }

    window.threebitSid = { loadFromBase64, play, stop, next, prev, meta, setChipMode };
})();