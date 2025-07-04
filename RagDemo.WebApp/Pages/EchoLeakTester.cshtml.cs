using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RagDemo.WebApp.Models;
using RagDemo.WebApp.Services;

namespace RagDemo.WebApp.Pages;

public class EchoLeakTesterModel : PageModel
{
    private readonly IEchoLeakApiService _echoLeakApiService;
    private readonly ILogger<EchoLeakTesterModel> _logger;

    public EchoLeakTesterModel(IEchoLeakApiService echoLeakApiService, ILogger<EchoLeakTesterModel> logger)
    {
        _echoLeakApiService = echoLeakApiService;
        _logger = logger;
    }

    [BindProperty]
    public string Query { get; set; } = string.Empty;

    public EchoLeakTestResult? Result { get; set; }

    public void OnGet()
    {
        // Initialize the page
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Query))
        {
            ModelState.AddModelError(string.Empty, "Query is required.");
            return Page();
        }

        try
        {
            _logger.LogInformation("Testing EchoLeak vulnerability with query: {Query}", Query);
            Result = await _echoLeakApiService.TestEchoLeakAsync(Query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing EchoLeak vulnerability");
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostIndexDocumentsAsync()
    {
        try
        {
            _logger.LogInformation("Indexing documents for EchoLeak demonstration");
            var result = await _echoLeakApiService.IndexDocumentsAsync();
            return new JsonResult(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing documents");
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}
