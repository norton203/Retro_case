using Microsoft.JSInterop;
using Retro_case.Models;

namespace Retro_case.Services;

public class ThemeState
{
    private readonly IJSRuntime _js;

    public Machine Current { get; private set; } = Machine.C64;

    public event Action? OnChange;

    public ThemeState(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SetMachineAsync(Machine machine)
    {
        if (machine == Current) return;
        Current = machine;
        OnChange?.Invoke();
        await _js.InvokeVoidAsync("threebit.setMachine", machine.Slug());
        await _js.InvokeVoidAsync("threebitSidSetChipModeSafe", machine.Slug());
    }
}
