window.threebit = {
    setMachine: function (slug) {
        document.body.setAttribute('data-machine', slug);
    }
};

// Small safe shim so ThemeState can always call this, whether or not the SID
// bridge script (only relevant on the C64 hub page's player) happens to be loaded.
window.threebitSidSetChipModeSafe = function (slug) {
    if (window.threebitSid && window.threebitSid.setChipMode) {
        window.threebitSid.setChipMode(slug);
    }
};