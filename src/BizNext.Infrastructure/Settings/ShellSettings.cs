using BizNext.Contracts.Models;

namespace BizNext.Infrastructure.Settings;

public sealed class ShellSettings
{
    public string? LastSelectedModuleId { get; set; }

    // Null until the user (or a one-time migration) chooses a theme.
    public ThemePreference? Theme { get; set; }
}
