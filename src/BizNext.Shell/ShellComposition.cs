using BizNext.Contracts;
using BizNext.Infrastructure.Modules;
using BizNext.Infrastructure.Settings;
using BizNext.Modules.Notes;
using BizNext.Modules.QuickScan;
using BizNext.Modules.QuickScan.Models;
using BizNext.Modules.WeatherCalendar;
using Microsoft.Extensions.DependencyInjection;

namespace BizNext.Shell;

internal static class ShellComposition
{
    internal static ServiceProvider Create(
        ApplicationSettings quickScanSettings,
        string? quickScanSettingsLoadError,
        ShellSettingsStore shellSettingsStore,
        ShellSettings shellSettings,
        string? shellSettingsLoadError)
    {
        var services = new ServiceCollection();
        services.AddSingleton(shellSettingsStore);
        services.AddSingleton(shellSettings);
        services.AddSingleton<IBizNextModule>(
            _ => new QuickScanModule(quickScanSettings, quickScanSettingsLoadError));
        services.AddSingleton<IBizNextModule, NotesModule>();
        services.AddSingleton<IBizNextModule, WeatherCalendarModule>();
        services.AddSingleton<ModuleCatalog>();
        services.AddSingleton<ModuleNavigationService>();
        services.AddSingleton(provider => new MainWindow(
            provider.GetRequiredService<ModuleCatalog>(),
            provider.GetRequiredService<ModuleNavigationService>(),
            provider.GetRequiredService<ShellSettingsStore>(),
            provider.GetRequiredService<ShellSettings>(),
            shellSettingsLoadError));

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
