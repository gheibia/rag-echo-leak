# RAG EchoLeak Demo - Security Vulnerability Demonstration

**⚠️ SECURITY DEMONSTRATION TOOL**
This is a security demonstration tool designed for educational purposes only. Use only in controlled environments for security research and training. Do not use real credentials or sensitive data.

## Purpose

This demonstration showcases the **EchoLeak vulnerability** and **RAG spraying attacks** in Retrieval-Augmented Generation (RAG) systems. Originally discovered by [Aim Security Labs](https://www.aim.security/lp/aim-labs-echoleak-blogpost) as a zero-click vulnerability in Microsoft 365 Copilot, this implementation demonstrates how the same attack vectors can affect general RAG applications.

### What is EchoLeak?

EchoLeak is a critical vulnerability that exploits design flaws in RAG-based AI systems, allowing attackers to:
- **Exfiltrate sensitive data** through seemingly innocent queries
- **Bypass access controls** via semantic similarity matching
- **Create covert data channels** using hidden tracking images
- **Scale attacks automatically** without user interaction

### Research Background

This demo is based on the groundbreaking research by **Aim Security Labs** who discovered EchoLeak as the first zero-click AI vulnerability in Microsoft 365 Copilot. Their research paper "[Breaking down 'EchoLeak', the First Zero-Click AI Vulnerability Enabling Data Exfiltration from Microsoft 365 Copilot](https://www.aim.security/lp/aim-labs-echoleak-blogpost)" introduced the concept of **LLM Scope Violation** and demonstrated how RAG systems can be weaponized against themselves.

While their research focused on Microsoft Copilot specifically, this demonstration extends the concepts to show how any RAG-based system can be vulnerable to similar attack patterns.

## Architecture Overview

This solution has been restructured to properly separate concerns and avoid runtime conflicts between Azure Functions and ASP.NET Core Web Applications, providing a realistic RAG implementation for security testing.

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
│       ├── test-image-leak.txt        # 🎯 RAG spraying attack vector (InfoSec digest)
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

## 🎬 **Step-by-Step Demo Walkthrough**

### Phase 1: Setup the Attack Surface
1. **Start the applications** (see "How to Run" section above)
2. **Open monitoring tab**: Visit `https://webhook.site/#!/078636ce-a013-458c-9c85-0b5b91e5227a`
3. **Navigate to EchoLeak Tester**: `https://localhost:5001` → "⚠️ EchoLeak Tester"
4. **Index documents**: Click "Index Documents First" to load the InfoSec digest
5. **Open browser DevTools**: Open Network tab

### Phase 2: Execute RAG Spraying Attack
Try these realistic queries and observe the immediate external tracking:

1. **Query**: `"GitHub token reset"`
   - **Expected Result**: InfoSec digest appears with live token `ghp_ragSprayDemoToken987654321`
   - **External Call**: Watch webhook.site receive immediate HTTP request

2. **Query**: `"AWS staging credentials"`
   - **Expected Result**: Document shows `AKIA_SPRAY_9988` and secret key
   - **External Call**: Hidden image loads, sending access notification

3. **Query**: `"VPN password help"`
   - **Expected Result**: Contractor credentials `WelcomeVPN2024!` exposed
   - **External Call**: Real-time tracking in webhook.site logs

### Phase 3: Analyze the Attack Impact
1. **Check webhook.site logs**: See real-time access events for each query
2. **Review Network tab**: Notice external requests to `webhook.site`
3. **Examine exposed data**: GitHub tokens, AWS keys, passwords, internal URLs
4. **Consider scale**: Imagine this across thousands of documents and users

### 💡 **Key Demonstration Points**
- **Zero suspicious activity**: All queries appear legitimate and work-related
- **Broad attack surface**: Many innocent queries can trigger sensitive document access
- **Immediate exfiltration**: Real-time tracking via hidden images
- **Rich credential harvest**: Multiple types of sensitive data in one document
- **Realistic scenario**: InfoSec digests are common in enterprise environments

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

### RAG Spraying Technique
This demo showcases **RAG spraying** - where innocent, everyday queries can retrieve sensitive documents through semantic similarity. The test document (`test-image-leak.txt`) contains a realistic InfoSec weekly digest that would naturally match common security and development queries.

### 🎯 **Featured Attack Document: `test-image-leak.txt`**
This document simulates a **Weekly Security Digest** that contains:
- **GitHub Personal Access Tokens**: `ghp_ragSprayDemoToken987654321`
- **AWS Staging Credentials**: Access keys and secrets for temporary environments
- **VPN Credentials**: Contractor onboarding passwords (`WelcomeVPN2024!`)
- **Internal Service URLs**: Slack admin, GitLab runners, Jenkins console
- **Hidden Tracking Image**: `![](https://webhook.site/078636ce-a013-458c-9c85-0b5b91e5227a/rag-spraying-digest)`

### Realistic Test Queries (RAG Spraying)
These innocent queries demonstrate how users asking for legitimate help can inadvertently access the InfoSec digest:

**Security-Related Queries:**
- `"how do I reset my GitHub token"`
- `"AWS access key setup"`
- `"VPN connection instructions"`
- `"security team contact information"`
- `"weekly security updates"`

**Developer Workflow Queries:**
- `"GitLab runner configuration"`
- `"Jenkins console access"`
- `"Slack admin panel"`
- `"contractor onboarding process"`
- `"infosec email contact"`

**Infrastructure Queries:**
- `"internal tools access"`
- `"credential rotation process"`
- `"temporary AWS staging"`
- `"MFA reset procedure"`

### The RAG Spraying Attack Vector
1. **Innocent Query**: Developer searches for "how to reset GitHub token"
2. **Semantic Match**: RAG system finds the "Weekly Security Digest" document
3. **Sensitive Exposure**: Document contains live tokens, AWS keys, VPN passwords, internal URLs
4. **External Tracking**: Hidden image triggers request to `webhook.site/078636ce-a013-458c-9c85-0b5b91e5227a`
5. **Data Exfiltration**: Attacker's webhook receives sensitive content via image parameters

### What to Monitor
1. **Browser Network Tab**: Look for requests to `webhook.site/078636ce-a013-458c-9c85-0b5b91e5227a`
2. **Search Results**: Check if the InfoSec digest is returned for innocent queries
3. **Rendered Markdown**: See if the hidden tracking image loads from external site
4. **Content Highlighting**: Sensitive credentials and tokens are visible in results
5. **Webhook Logs**: Visit `https://webhook.site/#!/078636ce-a013-458c-9c85-0b5b91e5227a` to see access logs

### 🕵️ **Live Attack Monitoring**
The embedded tracking image URL (`webhook.site/078636ce-a013-458c-9c85-0b5b91e5227a/rag-spraying-digest`) allows real-time monitoring of:
- **When**: Timestamp of document access
- **Who**: IP address and user agent of the accessing client
- **What**: Query parameters can be modified to exfiltrate specific data
- **How Often**: Frequency of access to sensitive documents

**To monitor live attacks:**
1. Open `https://webhook.site/#!/078636ce-a013-458c-9c85-0b5b91e5227a` in a separate tab
2. Run queries in the EchoLeak Tester
3. Watch real-time HTTP requests appear when sensitive documents are accessed
4. Notice how innocent queries trigger immediate external network calls

## Security Implications

### 🚨 **Vulnerability Patterns Showcased**
The `test-image-leak.txt` document demonstrates several critical security anti-patterns:

1. **Credential Storage in Documents**
   - GitHub Personal Access Tokens in plaintext
   - AWS credentials with clear access/secret key pairs
   - VPN passwords in communication channels

2. **Internal Infrastructure Exposure**
   - Direct URLs to admin panels (`admin.slack.corp/tools`)
   - Internal service endpoints (`jenkins.infra.local:8080`)
   - Configuration paths (`gitlab.internal/config/runner`)

3. **Covert Tracking Mechanisms**
   - Hidden image with unique webhook identifier
   - External domain for real-time access monitoring
   - URL parameters that could exfiltrate query context

4. **Social Engineering Vectors**
   - Appears as legitimate InfoSec communication
   - Contains helpful context that users would naturally search for
   - Warning text that's ignored by RAG systems

### 🎯 **RAG Spraying Attack Impact**
This demo shows how RAG systems can be exploited through **RAG spraying attacks**:
- ❌ **Innocent queries expose sensitive documents** through semantic similarity
- ❌ **No suspicious activity detected** - queries appear legitimate
- ❌ **Leak data through external image rendering** when markdown is processed
- ❌ **Bypass access controls** that would normally protect sensitive documents
- ❌ **Create covert data exfiltration channels** via embedded tracking images
- ❌ **Scale to automate discovery** of sensitive content across large document stores

## Mitigation Strategies

To prevent RAG spraying and EchoLeak attacks:

1. **Document Classification & Segregation**:
   - Separate public and sensitive content into different indexes
   - Implement document sensitivity scoring and access tiers

2. **Semantic Access Controls**:
   - Apply user-based filtering before semantic search
   - Implement role-based document access at the embedding level

3. **Content Sanitization**:
   - Scan for sensitive patterns (credentials, PII, etc.) before indexing
   - Strip or mask sensitive data in search results

4. **Query Analysis & Monitoring**:
   - Monitor for patterns indicative of RAG spraying attacks
   - Implement rate limiting and anomaly detection

5. **Secure Rendering Controls**:
   - Sanitize markdown and disable external resources
   - Use Content Security Policy (CSP) to prevent external requests

6. **Audit & Governance**:
   - Log all queries and results for security analysis
   - Regular review of indexed content for sensitivity

## Development Notes

- Web app uses `IHttpClientFactory` for better connection management
- Error handling and logging throughout the application
- Shared models prevent code duplication
- Configuration is environment-specific and secure

---

## Acknowledgments

This demonstration is inspired by and builds upon the foundational research from **Aim Security Labs** on the EchoLeak vulnerability. Their pioneering work in identifying LLM Scope Violations and demonstrating zero-click AI vulnerabilities in Microsoft 365 Copilot has been instrumental in advancing our understanding of AI security risks. For the complete technical details and original research, see their paper: "[Breaking down 'EchoLeak', the First Zero-Click AI Vulnerability Enabling Data Exfiltration from Microsoft 365 Copilot](https://www.aim.security/lp/aim-labs-echoleak-blogpost)".

---
