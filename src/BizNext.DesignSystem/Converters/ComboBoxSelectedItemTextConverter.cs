using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BizNext.DesignSystem.Converters;

public sealed class ComboBoxSelectedItemTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is null)
        {
            return string.Empty;
        }

        var selectedItem = values[0];
        if (selectedItem is ContentControl contentControl)
        {
            return contentControl.Content?.ToString() ?? string.Empty;
        }

        if (values[1] is not string { Length: > 0 } displayMemberPath)
        {
            return selectedItem.ToString() ?? string.Empty;
        }

        object? displayValue = selectedItem;
        foreach (var memberName in displayMemberPath.Split('.'))
        {
            if (displayValue is null)
            {
                return string.Empty;
            }

            var property = TypeDescriptor.GetProperties(displayValue)
                .Find(memberName, ignoreCase: true);
            if (property is null)
            {
                return selectedItem.ToString() ?? string.Empty;
            }

            displayValue = property.GetValue(displayValue);
        }

        return displayValue?.ToString() ?? string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        Enumerable.Repeat(Binding.DoNothing, targetTypes.Length).ToArray();
}
