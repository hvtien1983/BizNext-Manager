using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace BizNext.Modules.QuickScan.Controls;

public sealed class SvgIcon : FrameworkElement
{
    private static readonly XNamespace SvgNamespace = "http://www.w3.org/2000/svg";
    private static readonly ConcurrentDictionary<string, SvgDocument> Documents =
        new(StringComparer.OrdinalIgnoreCase);

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source),
        typeof(string),
        typeof(SvgIcon),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSourceChanged));

    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
        nameof(Foreground),
        typeof(Brush),
        typeof(SvgIcon),
        new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    private SvgDocument? _document;

    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (_document is null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        var viewBox = _document.ViewBox;
        var scale = Math.Min(ActualWidth / viewBox.Width, ActualHeight / viewBox.Height);
        var offsetX = (ActualWidth - viewBox.Width * scale) / 2 - viewBox.X * scale;
        var offsetY = (ActualHeight - viewBox.Height * scale) / 2 - viewBox.Y * scale;

        drawingContext.PushTransform(new MatrixTransform(
            scale,
            0,
            0,
            scale,
            offsetX,
            offsetY));
        if (_document.Transform is { } transform)
        {
            drawingContext.PushTransform(new MatrixTransform(transform));
        }

        drawingContext.DrawGeometry(Foreground, null, _document.Geometry);

        if (_document.Transform is not null)
        {
            drawingContext.Pop();
        }

        drawingContext.Pop();
    }

    private static void OnSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var icon = (SvgIcon)dependencyObject;
        if (e.NewValue is string source && !string.IsNullOrWhiteSpace(source))
        {
            icon._document = Documents.GetOrAdd(source, LoadDocument);
        }
        else
        {
            icon._document = null;
        }

        icon.InvalidateVisual();
    }

    private static SvgDocument LoadDocument(string source)
    {
        var assetPath = source.Replace('\\', '/').TrimStart('/');
        if (assetPath.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Invalid SVG resource path: {source}");
        }

        var resourceUri = new Uri(
            $"pack://application:,,,/BizNext.Modules.QuickScan;component/{assetPath.ToLowerInvariant()}",
            UriKind.Absolute);
        var resource = Application.GetResourceStream(resourceUri)
            ?? throw new FileNotFoundException($"SVG resource was not found: {source}");

        using var stream = resource.Stream;
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });
        var root = XDocument.Load(reader).Root
            ?? throw new InvalidDataException($"SVG resource has no root element: {source}");
        if (root.Name != SvgNamespace + "svg")
        {
            throw new InvalidDataException($"SVG resource has an invalid root element: {source}");
        }

        var viewBoxValues = (root.Attribute("viewBox")?.Value ?? string.Empty)
            .Split([' ', ',', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
            .ToArray();
        if (viewBoxValues.Length != 4 || viewBoxValues[2] <= 0 || viewBoxValues[3] <= 0)
        {
            throw new InvalidDataException($"SVG resource has an invalid viewBox: {source}");
        }

        var geometry = new GeometryGroup();
        Matrix? transform = null;
        foreach (var group in root.Elements(SvgNamespace + "g"))
        {
            if (transform is not null)
            {
                throw new InvalidDataException($"Nested SVG groups are not supported: {source}");
            }

            var groupTransform = group.Attribute("transform")?.Value;
            if (groupTransform is not null)
            {
                transform = ParseGroupTransform(groupTransform, source);
            }

            AddPaths(group, geometry, source);
        }

        AddPaths(root, geometry, source);
        if (geometry.Children.Count == 0)
        {
            throw new InvalidDataException($"SVG resource contains no paths: {source}");
        }

        return new SvgDocument(
            new Rect(viewBoxValues[0], viewBoxValues[1], viewBoxValues[2], viewBoxValues[3]),
            geometry,
            transform);
    }

    private static void AddPaths(XElement parent, GeometryGroup geometry, string source)
    {
        foreach (var path in parent.Elements(SvgNamespace + "path"))
        {
            var data = path.Attribute("d")?.Value;
            if (!string.IsNullOrWhiteSpace(data))
            {
                geometry.Children.Add(Geometry.Parse(data));
            }
        }
    }

    private static Matrix ParseGroupTransform(string value, string source)
    {
        const string number = @"[-+]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][-+]?\d+)?";
        var match = Regex.Match(
            value,
            $@"^\s*translate\(\s*({number})\s*[, ]\s*({number})\s*\)\s*scale\(\s*({number})\s*[, ]\s*({number})\s*\)\s*$",
            RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            throw new InvalidDataException($"SVG resource has an unsupported transform: {source}");
        }

        var translateX = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var translateY = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var scaleX = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
        var scaleY = double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
        return new Matrix(scaleX, 0, 0, scaleY, translateX, translateY);
    }

    private sealed record SvgDocument(Rect ViewBox, Geometry Geometry, Matrix? Transform);
}
