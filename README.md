# Infrastructure.Utils

A comprehensive .NET utility library providing common infrastructure components and extensions for modern C# applications.

## Features

Infrastructure.Utils is organized into specialized modules:

### 🔧 Core Extensions

- **String Extensions** - Powerful string manipulation, parsing, and transformation utilities
- **Collection Extensions** - LINQ enhancements, async enumerable support, chunking, and more
- **DateTime Extensions** - Date/time calculations, repeating patterns, Jewish dates, sunrise/sunset
- **Number Extensions** - Mathematical operations and number formatting utilities

### 🌐 HTTP & Networking

- **RestClient** - Enhanced HTTP client with built-in retry logic, cookie management, and fake user-agent support
- **WebSocket Client** - Managed WebSocket connections with automatic reconnection
- **Multiple Request Types** - Support for JSON, form data, multipart, and streaming requests

### 💾 Data Storage & Caching

- **Persistent Collections** - File-based collections with automatic JSON serialization
- **Key-Value Store** - Simple persistent key-value storage
- **Scoped Cache** - Request-scoped caching for web applications
- **Queues** - Various queue implementations including throttling and accumulator queues

### 🔒 Security

- **Encryption** - AES encryption/decryption utilities
- **Hashing** - SHA and MD5 hashing helpers
- **Random** - Cryptographically secure random generators
- **Signature Validation** - Digital signature utilities

### 🧵 Threading & Async

- **Task Pool** - Managed task execution with concurrency limits
- **Keyed Locker** - Named locks for synchronization
- **Async Extensions** - Utilities for working with async enumerables
- **Timeout Helpers** - Task timeout management

### 📝 Text Processing

- **Frequency Analyzer** - Text analysis with persistent storage
- **HTML Parser** - Simple HTML tag parsing and manipulation
- **Hebrew Text** - Hebrew language parsing and processing utilities
- **String Processor** - Advanced string processing pipelines

### 💉 Dependency Injection

- **Auto-Registration** - Automatic service registration using marker interfaces
- **Keyed Services** - Support for keyed service registration
- **Lazy Resolution** - Lazy dependency resolution support
- **Scoped Services** - Enhanced scoped service patterns

### 🔊 Additional Features

- **Text-to-Speech** - TTS integration with multiple language support
- **Telegram Integration** - Send messages via Telegram bot API
- **Process Management** - Enhanced process execution utilities
- **File Storage** - Organized file storage patterns

## Installation

### Package Manager

```bash
Install-Package Infrastructure.Utils
```

### .NET CLI

```bash
dotnet add package Infrastructure.Utils
```

### PackageReference

```xml
<PackageReference Include="Infrastructure.Utils" Version="*" />
```

## Quick Start

### 1. Register Services

```csharp
using Infrastructure.Utils;

// In your Startup.cs or Program.cs
services.AddInfrastructureServices<YourAppMarker>();
```

### 2. HTTP Client Usage

```csharp
using Infrastructure.Utils.Http.Models;

public class MyService
{
    private readonly IRestClient _client;

    public MyService(IRestClient client) => _client = client;

    public async Task<T> GetDataAsync<T>()
    {
        var options = new RestCallOptions
        {
            Url = "https://api.example.com/data",
            TimeoutMs = 5000
        };

        return await _client.GetJsonAsync<T>(options);
    }
}
```

### 3. Persistent Storage

```csharp
using Infrastructure.Utils.Repositories.Persistent;

// Key-Value Store
var store = new KeyValueStore("./data/store.json");
await store.SetAsync("key", "value");
var value = await store.GetAsync<string>("key");

// Persistent Collection
var collection = new PersistentCollection<User>("./data/users.json");
await collection.AddAsync(new User { Name = "John" });
var users = await collection.GetAllAsync();
```

### 4. Security Utilities

```csharp
using Infrastructure.Utils.Security;

// Encryption
string encrypted = AES.Encrypt("secret data", "password");
string decrypted = AES.Decrypt(encrypted, "password");

// Hashing
string hash = SHA.Hash256("data to hash");
bool isValid = SHA.Verify256("data", hash);

// Secure Random
int randomNumber = RandomExtensions.RandomNumber(1, 100);
string randomString = RandomExtensions.RandomString(16);
```

### 5. Dependency Injection with Marker Interfaces

```csharp
// Define your services with marker interfaces

public class MyService : ISingletonService
{
    public Task<string> GetDataAsync() => Task.FromResult("data");
}

// Services are automatically registered!
```

### 6. Text Frequency Analysis

```csharp
using Infrastructure.Utils.Text.Analyzers;

var analyzer = new FrequencyAnalyzer();
analyzer.AddText("sample text for analysis");
var frequencies = analyzer.GetFrequencies();

// With persistent storage
var persistentAnalyzer = new FrequencyAnalyzer(
    new JsonFrequencyAnalyzerStore("./data/frequencies.json")
);
```

## Requirements

- .NET 8.0 or higher
- C# 12.0 or higher (uses latest language features)

## Dependencies

- MediatorCore (1.7.8) - Mediator pattern implementation
- MemoryCore (1.6.3) - Memory caching
- Microsoft.AspNetCore.Http.Abstractions (2.3.0) - HTTP abstractions
- Microsoft.Extensions.Logging.Abstractions (8.0.0) - Logging abstractions
- NAudio (2.2.1) - Audio processing for TTS
- System.Text.Json (8.0.5) - JSON serialization

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/yourusername/Infrastructure.Utils).
