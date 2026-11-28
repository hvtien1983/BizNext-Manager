using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BizNext.Modules.Notes.Models;

public sealed class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;

    /// <summary>Nội dung dạng Markdown.</summary>
    public string Content { get; set; } = string.Empty;

    public Guid? NotebookId { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsPinned { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Khác null nghĩa là note đang ở thùng rác (xóa mềm).</summary>
    public DateTime? DeletedAtUtc { get; set; }

    [JsonIgnore]
    public bool IsDeleted => DeletedAtUtc.HasValue;
}