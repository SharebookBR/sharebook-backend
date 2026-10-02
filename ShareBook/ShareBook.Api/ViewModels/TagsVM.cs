using System.Collections.Generic;

namespace ShareBook.Api.ViewModels;

public class TagSummaryVM
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Family { get; set; } = string.Empty;
}

public class TagVM : TagSummaryVM
{
    public IList<string> Aliases { get; set; } = new List<string>();
    public string? Description { get; set; }
    public string? UsageNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
}

public class UpsertTagVM
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IList<string> Aliases { get; set; } = new List<string>();
    public string Family { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? UsageNotes { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsPublic { get; set; } = true;
}

public class UpdateBookTagsVM
{
    public IList<string> TagIds { get; set; } = new List<string>();
}
