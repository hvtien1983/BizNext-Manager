using System.Windows;
using BizNext.Contracts;
using BizNext.Contracts.Models;

namespace BizNext.Tests.TestSupport;

internal sealed class TestModule(string id) : IBizNextModule
{
    public BizNextModuleMetadata Metadata { get; } = new(id, id, "Test module", "Tests");

    public FrameworkElement CreateView() => new System.Windows.Controls.TextBlock { Text = Metadata.Name };
}
