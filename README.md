# RAG EchoLeak Demo - Multi-Project Architecture

This solution has been restructured to properly separate concerns and avoid runtime conflicts between Azure Functions and ASP.NET Core Web Applications.

## Project Structure

```
rag-demo/
├── README.md                          # This file
├── README-Original.md                 # Original single-project README
├── rag-demo.sln                      # Solution file
├── RagDemo.Functions/                 # Azure Functions project
│   ├── Program.cs
│   ├── RagDemo.Functions.csproj
│   ├── IndexDocuments.cs
│   ├── QueryRag.cs
│   ├── host.json
│   ├── local.settings.json
│   ├── Models/
│   │   └── RagDocument.cs
│   ├── Services/
│   │   ├── AzureOpenAIService.cs
│   │   ├── AzureSearchService.cs
│   │   └── TextChunkingService.cs
│   └── rag-data/
│       ├── customer-incident-report.txt
│       ├── test-image-leak.txt      # New document with webhook.site images
│       └── ... (other documents)
└── RagDemo.WebApp/                   # ASP.NET Core Web Application
    ├── Program.cs
    ├── RagDemo.WebApp.csproj
    ├── Pages/
    │   ├── Index.cshtml                # Updated home page
    │   ├── EchoLeakTester.cshtml      # Main EchoLeak testing interface
    │   └── EchoLeakTester.cshtml.cs   # Page model
    ├── Models/
    │   └── EchoLeakModels.cs          # UI models and DTOs
    ├── Services/
    │   └── EchoLeakApiService.cs      # Service to call Azure Functions APIs
    └── appsettings.Development.json   # Configuration
```

## Architecture Benefits

### 🎯 **Clean Separation**
- **Azure Functions**: Pure backend APIs for document processing
- **Web Application**: Modern Razor Pages UI with its own models
- **Simple Communication**: HTTP/JSON between the two projects

### 🚀 **Independent Deployment**
- Each project can be deployed and scaled independently
- No runtime conflicts between function host and web application host
- Different configuration and hosting options
- No shared dependencies to manage

### 🔧 **Easy Development**
- Run Azure Functions on port 7071
- Run Web App on port 5000/5001
- Web app calls Functions APIs over HTTP

## How to Run

### 1. Start Azure Functions (Terminal 1)
```bash
cd RagDemo.Functions
func start
```
This will start the functions on `http://localhost:7071`

### 2. Start Web Application (Terminal 2)
```bash
cd RagDemo.WebApp
dotnet run
```
This will start the web app on `https://localhost:5001`

### 3. Use the EchoLeak Tester
1. Navigate to `https://localhost:5001`
2. Click on "⚠️ EchoLeak Tester" in the navigation
3. Click "Index Documents First" to index all documents
4. Enter queries to test the vulnerability
5. Monitor browser network tab for external image requests

## Configuration

### Azure Functions (`RagDemo.Functions/local.settings.json`)
```json
{
    "Values": {
        "AzureOpenAI:Endpoint": "https://your-openai.openai.azure.com/",
        "AzureOpenAI:ApiKey": "your-api-key",
        "AzureOpenAI:EmbeddingModel": "text-embedding-ada-002",
        "AzureSearch:Endpoint": "https://your-search.search.windows.net",
        "AzureSearch:ApiKey": "your-search-key",
        "AzureSearch:IndexName": "your-index-name"
    }
}
```

### Web Application (`RagDemo.WebApp/appsettings.Development.json`)
```json
{
    "AzureFunctions": {
        "BaseUrl": "http://localhost:7071"
    },
    "AzureSearch": {
        "Endpoint": "https://your-search.search.windows.net",
        "ApiKey": "your-search-key",
        "IndexName": "your-index-name"
    }
}
```

## EchoLeak Testing Features

### 🕷️ **Web-Based Interface**
- Modern Bootstrap UI with responsive design
- Real-time feedback and status updates
- Example dangerous queries for testing

### 🖼️ **Image Rendering Vulnerability**
- Markdown rendering with external image loading
- Tests `webhook.site` integration for tracking
- Demonstrates data exfiltration through image requests

### 🔍 **Sensitive Content Detection**
- Automatic pattern matching for sensitive information
- Visual indicators for dangerous results
- Raw content vs. rendered markdown comparison

## Testing the Vulnerability

### Sample Dangerous Queries
- `"AWS credentials deployment incident"`
- `"test image webhook site"`
- `"database connection string password"`
- `"internal only confidential"`

### What to Monitor
1. **Browser Network Tab**: Look for requests to `webhook.site`
2. **Search Results**: Check if sensitive documents are returned
3. **Rendered Markdown**: See if images are loaded from external sites
4. **Content Highlighting**: Sensitive content is marked with warnings

## Security Implications

This demo shows how RAG systems can:
- ❌ Expose sensitive documents through semantic similarity
- ❌ Leak data through external image rendering
- ❌ Allow unauthorized access to confidential information
- ❌ Create attack vectors for data exfiltration

## Mitigation Strategies

1. **Document Classification**: Separate public and sensitive content
2. **Access Controls**: Implement proper authentication and authorization
3. **Content Filtering**: Scan for sensitive patterns before indexing
4. **Rendering Controls**: Sanitize markdown and disable external resources
5. **Monitoring**: Log and audit all queries and results

## Development Notes

- Web app uses `IHttpClientFactory` for better connection management
- Error handling and logging throughout the application
- Shared models prevent code duplication
- Configuration is environment-specific and secure

---

**⚠️ Disclaimer**: This is a security demonstration tool. Use only in controlled environments for educational purposes. Do not use real credentials or sensitive data.
