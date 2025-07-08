namespace RagDemo.WebApp.Models;

public class SearchResultItem
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string RenderedMarkdown { get; set; } = string.Empty;
    public double Score { get; set; }
    public bool ContainsSensitiveContent { get; set; }
}
