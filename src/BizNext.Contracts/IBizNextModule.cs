using System.Windows;
using BizNext.Contracts.Models;

namespace BizNext.Contracts;

public interface IBizNextModule
{
    BizNextModuleMetadata Metadata { get; }

    FrameworkElement CreateView();
}
