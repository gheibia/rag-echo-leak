namespace RagDemo.WebApp.Models;

public class EchoLeakTestResult
{
    public string Query { get; set; } = string.Empty;
    public bool FoundSensitiveContent { get; set; }
    public List<SearchResultItem> Results { get; set; } = new();
}

public class SearchResultItem
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string RenderedMarkdown { get; set; } = string.Empty;
    public double Score { get; set; }
    public bool ContainsSensitiveContent { get; set; }
}

public class EchoLeakTestRequest
{
    public string Query { get; set; } = string.Empty;
}
