using System.Collections.Generic;

namespace BizNext.Modules.Notes.Models;

/// <summary>Toàn bộ dữ liệu Notes, lưu trong một file JSON.</summary>
public sealed class NotesData
{
    public int Version { get; set; } = 1;
    public List<Notebook> Notebooks { get; set; } = new();
    public List<Note> Notes { get; set; } = new();
}