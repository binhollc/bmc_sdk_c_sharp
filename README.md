# Binho Mission Control C# SDK

## Introduction

Welcome to the Binho Mission Control C# SDK (BMC C# SDK), a software tool designed to streamline your interaction with USB host adapters. Our SDK facilitates communication with Binho Nova and Supernova host adapters, offering a seamless integration for developers aiming to control these devices programmatically.

## Highlights

- **Unified Interface**: The SDK operates through the BMC Bridge, a REPL service that provides a consistent API for interacting with various host adapters. It accepts JSON-formatted command requests and returns JSON-formatted responses, ensuring a standardized communication protocol.
- **Extensibility**: Designed with extensibility in mind, the SDK allows for easy integration and control of future host adapters.
- **Multiple Examples**: Comes with comprehensive examples including basic I3C operations, IBI handling, and Common Command Code (CCC) operations to help you get started quickly.
- **Asynchronous Operations**: Built with modern C# async/await patterns for efficient, non-blocking operations.

## Prerequisites

### BMC Bridge Installation
The BMC Bridge must be installed separately and located in your system's PATH to run the examples provided with this SDK. This ensures that the SDK can communicate with the host adapters through the Bridge seamlessly.

#### Installation Locations:
- **Windows**: The bridge executable can typically be found in `C:\Program Files (x86)\BinhoMissionControlBridge` if the installer was used.
- **macOS and Linux**: The location will be where the bridge package was unzipped.

#### Verification:
After installation, verify the bridge is accessible by running:
```bash
bmcbridge --version
```

### .NET Requirements
- **.NET 8.0 SDK**: This project targets .NET 8.0. Ensure that the .NET 8.0 SDK is installed on your development machine to build and execute the examples.

## Installation

Clone this repository to your local machine using the following command:

```
git clone https://github.com/binhollc/bmc_sdk_c_sharp.git
```

Navigate to the cloned directory and build the project:

```bash
cd bmc_sdk_c_sharp
dotnet build
```

## Usage

To run the provided examples, use the following command:

```bash
dotnet run [example]
```

Replace `[example]` with the name of the example you wish to execute. Available examples:
- `i3c_basics` - Basic I3C operations
- `i3c_ibis` - In-Band Interrupt (IBI) handling  
- `i3c_ccc` - Common Command Code operations

If no example is specified, the program will list all available examples.

## Examples

### Available Examples

- **`i3c_basics`**: Demonstrates fundamental I3C commands using the Supernova simulated host adapter. Shows basic bus initialization, device communication, and USB string retrieval.

- **`i3c_ibis`**: Advanced example showing how to configure an IMU (ICM42605) to generate IBIs (In-Band Interrupts) and how to handle these notifications. Includes extended functionality through the `ExtendedBridgeClient` class.

- **`i3c_ccc`**: Demonstrates Common Command Code (CCC) operations, specifically the GETPID command to retrieve device Provisioned IDs. Shows how to process structured responses and extract payload data.

### Example Output Processing

The SDK uses structured JSON responses that can be processed as follows:

```csharp
// Extract data from command responses
var data = (JsonElement)response.Data;
var result = data.GetProperty("result");
var payload = result.GetProperty("payload")
                   .EnumerateArray()
                   .Select(x => Convert.ToByte(x.GetString(), 16))
                   .ToArray();
```

## Repository Contents

### Core Components

- **`BridgeClient.cs`**: The main service class for interacting with the BMC Bridge via stdin/stdout. Handles asynchronous command execution, response processing, and event notifications. Includes transaction management and proper resource disposal.

- **`Program.cs`**: Entry point that manages example selection and execution. Provides a clean interface for running different demonstration scenarios.

### Example Implementations

- **`ExampleI3cBasics.cs`**: Fundamental I3C operations including bus initialization, device communication, and basic command execution.

- **`ExampleI3cIbis.cs`**: Advanced IBI (In-Band Interrupt) handling with IMU configuration. Features an extended bridge client with specialized I3C write operations.

- **`ExampleI3cCcc.cs`**: Common Command Code operations demonstrating structured data extraction and device identification commands.

### Project Configuration

- **`bmc_sdk_c_sharp.csproj`**: .NET 8.0 project file with necessary dependencies and build configuration.

## Architecture

### Communication Flow

1. **Bridge Process**: The SDK launches and manages the BMC Bridge process
2. **JSON Commands**: Commands are sent as structured JSON via stdin
3. **Async Responses**: Responses are processed asynchronously with transaction tracking
4. **Event Handling**: Support for both command responses and asynchronous notifications

### Key Features

- **Transaction Management**: Each command gets a unique transaction ID for response correlation
- **Resource Management**: Proper disposal patterns with `using` statements
- **Event-Driven**: Support for response and notification event handlers
- **Type Safety**: Strongly-typed response objects with JSON serialization

## API Reference

### Bridge's API Documentation

For detailed information about the Bridge's API, visit [Binho Support](https://support.binho.io/user-guide/protocols-and-interfaces/bridge-1.1-api).

### Basic Usage Pattern

```csharp
using var bridgeClient = new BridgeClient("BinhoSupernova");
await bridgeClient.StartAsync();

var responses = await bridgeClient.SendCommand(
    "command_name",
    new Dictionary<string, object> {
        { "parameter1", "value1" },
        { "parameter2", "value2" }
    }
);
```

### Command Response Structure

```csharp
public class CommandResponse
{
    public string TransactionId { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public bool IsPromise { get; set; }
    public JsonElement Data { get; set; }
}
```

## Development and Troubleshooting

### Common Issues

1. **Bridge Not Found**: Ensure `bmcbridge` is in your system PATH and verify with `bmcbridge --version`
2. **Permission Issues**: On Linux/macOS, you may need appropriate permissions for USB device access
3. **Build Errors**: Ensure .NET 8.0 SDK is installed and project dependencies are restored

### Adding New Examples

To add a new example:

1. Create a new class file (e.g., `ExampleNewFeature.cs`)
2. Implement a static `Run()` method that returns `Task`
3. Add your example to the dictionary in `Program.cs`:

```csharp
["example_key"] = ExampleNewFeature.Run,
```

### Event Handling

The `BridgeClient` supports event handlers for responses and notifications:

```csharp
bridgeClient.OnResponseReceived += (sender, response) => {
    Console.WriteLine($"Response: {response.Status}");
};

bridgeClient.OnNotificationReceived += (sender, notification) => {
    Console.WriteLine($"Notification: {notification.Data}");
};
```

## Contributing

We welcome contributions! If you have suggestions for improvements or bug fixes, please feel free to make a pull request or open an issue.
