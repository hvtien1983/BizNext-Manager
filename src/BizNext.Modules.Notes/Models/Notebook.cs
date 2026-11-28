using System;

namespace BizNext.Modules.Notes.Models;

public sealed class Notebook
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    /// <summary>Khác null nghĩa là notebook đang ở thùng rác (xóa mềm).</summary>
    public DateTime? DeletedAtUtc { get; set; }

    public bool IsDeleted => DeletedAtUtc.HasValue;
}