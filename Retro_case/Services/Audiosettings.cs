using Microsoft.JSInterop;

namespace Retro_case.Services;

/// <summary>Holds the SFX on/off preference (used for the BBS modem-connect sound).
/// The ambient per-machine music playlist was dropped in favour of the real SID player.</summary>
public class AudioSettings
{
    private readonly IJSRuntime _js;

    public bool SfxEnabled { get; private set; } = true;
    public bool Initialized { get; private set; }

    public event Action? OnChange;

    public AudioSettings(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        SfxEnabled = await _js.InvokeAsync<bool>("threebitAudio.getPref", "sfx-enabled", true);
        Initialized = true;
        OnChange?.Invoke();
    }

    public async Task ToggleSfxAsync()
    {
        SfxEnabled = !SfxEnabled;
        await _js.InvokeVoidAsync("threebitAudio.setPref", "sfx-enabled", SfxEnabled);
        OnChange?.Invoke();
    }

    public async Task PlayModemConnectAsync()
    {
        if (SfxEnabled)
        {
            await _js.InvokeVoidAsync("threebitAudio.playModemConnect");
        }
    }
}