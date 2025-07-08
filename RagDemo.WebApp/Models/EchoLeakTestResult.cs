namespace RagDemo.WebApp.Models;

public class EchoLeakTestResult
{
    public string Query { get; set; } = string.Empty;
    public bool FoundSensitiveContent { get; set; }
    public List<SearchResultItem> Results { get; set; } = new();
}
