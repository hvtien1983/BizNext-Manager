using BizNext.Infrastructure.Modules;
using BizNext.Tests.TestSupport;

namespace BizNext.Tests.Shell;

public sealed class ModuleNavigationTests
{
    [Fact]
    public void CatalogRejectsEmptyAndDuplicateIds()
    {
        Assert.Throws<InvalidOperationException>(() => new ModuleCatalog([new TestModule(" ")]));
        Assert.Throws<InvalidOperationException>(() =>
            new ModuleCatalog([new TestModule("notes"), new TestModule("NOTES")]));
    }

    [Fact]
    public void CatalogLooksUpIdsWithoutCaseAndPreservesRegistrationOrder()
    {
        var first = new TestModule("notes");
        var second = new TestModule("scan");
        var catalog = new ModuleCatalog([first, second]);

        Assert.Same(first, catalog.GetRequiredModule("NOTES"));
        Assert.Equal(new[] { first, second }, catalog.Modules);
        Assert.False(catalog.TryGetModule("missing", out _));
    }

    [Fact]
    public void NavigationRaisesOnlyForChangesAndPreservesSelectionOnInvalidId()
    {
        var service = new ModuleNavigationService(new ModuleCatalog([new TestModule("notes")]));
        var changes = new List<string?>();
        service.SelectedModuleChanged += changes.Add;

        service.SelectModule("notes");
        service.SelectModule("NOTES");
        Assert.Throws<KeyNotFoundException>(() => service.SelectModule("unknown"));
        Assert.Equal("notes", service.SelectedModuleId);
        service.SelectModule(null);

        Assert.Equal(new string?[] { "notes", null }, changes);
    }
}
