namespace BizNext.Infrastructure.Modules;

public sealed class ModuleNavigationService(ModuleCatalog catalog)
{
    public string? SelectedModuleId { get; private set; }

    public event Action<string?>? SelectedModuleChanged;

    public void SelectModule(string? moduleId)
    {
        if (moduleId is not null)
        {
            _ = catalog.GetRequiredModule(moduleId);
        }

        if (string.Equals(SelectedModuleId, moduleId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SelectedModuleId = moduleId;
        SelectedModuleChanged?.Invoke(moduleId);
    }
}
