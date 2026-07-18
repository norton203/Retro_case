namespace Retro_case.Models;

public enum Machine
{
    C64,
    Spectrum,
    Amstrad
}

public static class MachineInfo
{
    public static string Slug(this Machine m) => m switch
    {
        Machine.C64 => "c64",
        Machine.Spectrum => "spectrum",
        Machine.Amstrad => "amstrad",
        _ => "c64"
    };

    public static string DisplayName(this Machine m) => m switch
    {
        Machine.C64 => "Commodore 64",
        Machine.Spectrum => "ZX Spectrum",
        Machine.Amstrad => "Amstrad CPC",
        _ => ""
    };

    /// <summary>
    /// Lines played back character-by-character in the boot screen.
    /// Loosely modelled on each machine's real startup banner.
    /// </summary>
    public static string[] BootLines(this Machine m) => m switch
    {
        Machine.C64 => new[]
        {
            "    **** COMMODORE 64 BASIC V2 ****",
            " 64K RAM SYSTEM  38911 BASIC BYTES FREE",
            "",
            "READY.",
            "LOAD\"THREEBIT\",8,1",
            "SEARCHING FOR THREEBIT",
            "LOADING",
            "RUN"
        },
        Machine.Spectrum => new[]
        {
            "  (C) 1982 Sinclair Research Ltd",
            "",
            "  \u00A9 THREEBIT",
            "",
            "  PROGRAM: \"THREEBIT\"",
            "  LOAD \"\"",
            "  0 OK, 0:1"
        },
        Machine.Amstrad => new[]
        {
            "Amstrad 64K Microcomputer (v1)",
            "BASIC 1.0",
            "",
            "Ready",
            "RUN\"THREEBIT",
            "Loading..."
        },
        _ => Array.Empty<string>()
    };
}
