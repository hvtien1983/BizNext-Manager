using System.Windows;
using System.Windows.Controls;
using BizNext.Contracts;
using BizNext.Contracts.Models;

namespace BizNext.Modules.Notes;

public sealed class NotesModule : IBizNextModule
{
    public BizNextModuleMetadata Metadata { get; } = new(
        "biznext.module.notes",
        "Ghi chú",
        "Ghi lại ý tưởng và công việc cần nhớ.",
        "Năng suất");

    public FrameworkElement CreateView() => new NotesModuleView();
}

internal sealed class NotesModuleView : UserControl
{
    public NotesModuleView()
    {
        SetResourceReference(BackgroundProperty, "SurfaceCanvasBrush");

        var title = new TextBlock
        {
            Text = "Ghi chú",
            FontSize = 24,
            FontWeight = FontWeights.Bold
        };
        title.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        var description = new TextBlock
        {
            Text = "Tiện ích ghi chú sẽ được bổ sung trong giai đoạn tiếp theo.",
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap
        };
        description.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");

        var card = new Border
        {
            Margin = new Thickness(24),
            Padding = new Thickness(28),
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    title,
                    description
                }
            }
        };
        card.SetResourceReference(StyleProperty, "SectionCard");
        Content = card;
    }
}
