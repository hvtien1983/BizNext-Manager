using System.Windows;
using System.Windows.Controls;
using BizNext.Contracts;
using BizNext.Contracts.Models;

namespace BizNext.Modules.WeatherCalendar;

public sealed class WeatherCalendarModule : IBizNextModule
{
    public BizNextModuleMetadata Metadata { get; } = new(
        "biznext.module.weathercalendar",
        "Lịch & Thời tiết",
        "Theo dõi lịch trình và thông tin thời tiết.",
        "Tiện ích");

    public FrameworkElement CreateView() => new WeatherCalendarModuleView();
}

internal sealed class WeatherCalendarModuleView : UserControl
{
    public WeatherCalendarModuleView()
    {
        SetResourceReference(BackgroundProperty, "SurfaceCanvasBrush");

        var title = new TextBlock
        {
            Text = "Lịch & Thời tiết",
            FontSize = 24,
            FontWeight = FontWeights.Bold
        };
        title.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        var description = new TextBlock
        {
            Text = "Tiện ích lịch và thời tiết sẽ được bổ sung trong giai đoạn tiếp theo.",
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
