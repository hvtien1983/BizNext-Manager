namespace BizNext.Contracts.Models;

public sealed record BizNextModuleMetadata(
    string Id,
    string Name,
    string Description,
    string Category)
{
    /// <summary>Glyph biểu tượng hiển thị ở sidebar/Dashboard (tùy chọn).</summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>Thứ tự sắp xếp hiển thị; nhỏ hơn đứng trước (mặc định 0).</summary>
    public int Order { get; init; }
}