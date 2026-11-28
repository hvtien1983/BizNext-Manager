using BizNext.Contracts;

namespace BizNext.Infrastructure.Modules;

public sealed class ModuleCatalog
{
    private readonly Dictionary<string, IBizNextModule> _modules;

    public ModuleCatalog(IEnumerable<IBizNextModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        _modules = new Dictionary<string, IBizNextModule>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in modules)
        {
            ArgumentNullException.ThrowIfNull(module);
            var metadata = module.Metadata
                ?? throw new InvalidOperationException("A BizNext module must provide metadata.");
            if (string.IsNullOrWhiteSpace(metadata.Id))
            {
                throw new InvalidOperationException("A BizNext module must have a non-empty ID.");
            }

            if (!_modules.TryAdd(metadata.Id, module))
            {
                throw new InvalidOperationException($"A BizNext module with ID '{metadata.Id}' is already registered.");
            }
        }

        // Sắp xếp theo Order (ổn định, giữ thứ tự đăng ký khi bằng nhau).
        Modules = Array.AsReadOnly(_modules.Values.OrderBy(module => module.Metadata.Order).ToArray());
    }

    public IReadOnlyList<IBizNextModule> Modules { get; }

    public bool TryGetModule(string moduleId, out IBizNextModule? module) =>
        _modules.TryGetValue(moduleId, out module);

    public IBizNextModule GetRequiredModule(string moduleId) =>
        _modules.TryGetValue(moduleId, out var module)
            ? module
            : throw new KeyNotFoundException($"No BizNext module is registered with ID '{moduleId}'.");
}
