# Copilot Instructions for BMC C# SDK

## Prerequisites

### BMC Bridge Installation
The C# SDK requires the BinhoMissionControl Bridge (bmcbridge) utility to be installed and added to the PATH environment variable. The Bridge is maintained separately from the SDK for independent versioning and updates.

#### Windows Setup
1. Download BMC Bridge installer from [Binho Support](https://support.binho.io/getting-started/c++-sdk/installation):
   - Windows 32-bit or 64-bit version
2. Default installation path: `C:\Program Files (x86)\BinhoMissionControlBridge`
3. Add to PATH:
   - Open System Properties (`Win + R`, type `sysdm.cpl`)
   - Advanced tab → Environment Variables
   - Edit System Variables → PATH
   - Add `C:\Program Files (x86)\BinhoMissionControlBridge`

#### Linux/macOS Setup
1. Download appropriate binary from [Binho Support](https://support.binho.io/getting-started/c++-sdk/installation):
   - macOS ARM/Intel
   - Linux
2. Extract to preferred location
3. Add to PATH in shell config:
   ```bash
   # Add to ~/.zshrc (macOS) or ~/.bashrc (Linux)
   export PATH=/path/to/bmcbridge:$PATH
   ```
4. Apply changes: `source ~/.zshrc` or `source ~/.bashrc`

Verify installation: `bmcbridge --version`

## Project Architecture
This SDK facilitates communication with Binho Nova and Supernova USB host adapters through the BMC Bridge process. Key components:

### Core Components
- [`BridgeClient`](BridgeClient.cs) - Manages Bridge process communication via stdin/stdout
- Example implementations:
  - [`ExampleI3cBasics`](ExampleI3cBasics.cs) - Core I3C operations
  - [`ExampleI3cIbis`](ExampleI3cIbis.cs) - IBI (In-Band Interrupt) handling
  - [`ExampleI3cCcc`](ExampleI3cCcc.cs) - Common Command Code operations

## Bridge Communication Patterns

### Command Structure
All Bridge commands follow a consistent JSON structure with transaction IDs, command names, and parameters:
```csharp
var responses = await bridgeClient.SendCommand(
    "i3c_ccc_getpid",
    new Dictionary<string, object> {
        { "address", "08" },
        { "pushPullClockFrequencyInMHz", "5" },
        { "openDrainClockFrequencyInKHz", "2500" }
    }
);
```

### Standard Command Sequence
Always follow this initialization pattern:
```csharp
// 1. Open connection
await bridgeClient.SendCommand("open", 
    new Dictionary<string, object> { 
        { "address", "SupernovaSimulatedPort" } 
    });

// 2. Initialize protocol bus
await bridgeClient.SendCommand("i3c_init_bus", 
    new Dictionary<string, object> { 
        { "busVoltageInV", "3.3" } 
    });

// 3. Perform operations...
```

### Response Processing
Responses contain transaction IDs and should be processed as JsonElement. Always extract the final response for results:
```csharp
var response = responses.Last(); // Get final response
var data = (JsonElement)response.Data;
var result = data.GetProperty("result");

// For payload data (hex string arrays):
var payload = result.GetProperty("payload")
                   .EnumerateArray()
                   .Select(x => Convert.ToByte(x.GetString(), 16))
                   .ToArray();
```

### Promise Response Handling
Some commands return multiple responses. Check `is_promise` field:
```csharp
foreach (var response in responses)
{
    if (response.IsPromise)
    {
        // More responses expected
        continue;
    }
    // Final response - process data
}
```

## Development Workflow

### Build and Run
```bash
dotnet build
dotnet run i3c_basics    # Run basic I3C example
```

### Requirements
- .NET 8.0 SDK
- BMC Bridge in system PATH

## Project Conventions

### Data Formats
- **Device addresses**: Hex strings without "0x" prefix (e.g., "08", "4E", "50")
- **Subaddresses/Registers**: Hex strings (e.g., "0000", "76", "4E")
- **Data payload**: Hex strings for writeBuffer (e.g., "04", "0F00", "DEADBEEF")
- **Clock frequencies**: 
  - I3C Push-pull: MHz as string (e.g., "5")
  - I3C Open-drain: KHz as string (e.g., "1250", "2500")
  - I2C Clock: KHz as string (e.g., "100", "400")
- **Bus voltage**: Voltage as string (e.g., "1.8", "3.3")

### Common I2C Parameters
Standard parameter sets for I2C commands:
```csharp
// Basic I2C read/write parameters
var i2cParams = new Dictionary<string, object>
{
    { "address", "50" },                    // Target device address
    { "subaddress", "0000" },              // Register address
    { "clockFrequencyInKHz", "100" },      // Clock frequency (100/400 KHz typical)
    { "busVoltageInV", "3.3" }             // Bus voltage
};
```

### Common I3C Parameters
Standard parameter sets for I3C commands:
```csharp
// Basic I3C read/write parameters
var i3cParams = new Dictionary<string, object>
{
    { "address", "08" },                        // Target device address
    { "subaddress", "0000" },                  // Register address
    { "mode", "SDR" },                         // Transfer mode (SDR, HDR-DDR)
    { "pushPullClockFrequencyInMHz", "5" },    // PP clock frequency
    { "openDrainClockFrequencyInKHz", "1250" } // OD clock frequency
};
```

### Client Lifecycle
Always use `using` statement for proper resource cleanup:
```csharp
using var bridgeClient = new BridgeClient("BinhoSupernova");
await bridgeClient.StartAsync();
// Operations...
// Dispose called automatically
```

### Event Handling
Subscribe to notifications for IBI and async events:
```csharp
bridgeClient.OnNotificationReceived += (sender, notification) => {
    if (notification.TransactionId == "0") {
        Console.WriteLine($"IBI/Notification: {notification.Data}");
    }
};
```

### Error Handling
Always check response status and handle errors appropriately:
```csharp
if (response.Status != "success") {
    var data = (JsonElement)response.Data;
    if (data.TryGetProperty("error", out JsonElement error)) {
        var message = error.GetProperty("message").GetString();
        throw new InvalidOperationException($"Bridge error: {message}");
    }
}
```

### Helper Methods
Create extension methods for common operations:
```csharp
// I3C helper method
public async Task<List<CommandResponse>> SendI3cWriteUsingSubaddress(
    string address, string subaddress, string writeBuffer)
{
    return await SendCommand("i3c_write_using_subaddress",
        new Dictionary<string, object>
        {
            { "address", address },
            { "subaddress", subaddress },
            { "mode", "SDR" },
            { "pushPullClockFrequencyInMHz", "5" },
            { "openDrainClockFrequencyInKHz", "1250" },
            { "writeBuffer", writeBuffer }
        });
}

// I2C helper methods
public async Task<List<CommandResponse>> SendI2cWriteUsingSubaddress(
    string address, string subaddress, string writeBuffer)
{
    return await SendCommand("i2c_write_using_subaddress",
        new Dictionary<string, object>
        {
            { "address", address },
            { "subaddress", subaddress },
            { "writeBuffer", writeBuffer },
            { "clockFrequencyInKHz", "100" }
        });
}

public async Task<List<CommandResponse>> SendI2cReadUsingSubaddress(
    string address, string subaddress, int bytesToRead)
{
    return await SendCommand("i2c_read_using_subaddress",
        new Dictionary<string, object>
        {
            { "address", address },
            { "subaddress", subaddress },
            { "bytesToRead", bytesToRead.ToString() },
            { "busVoltageInV", "3.3" }
        });
}
```

## Common Bridge Commands

### Device Management
```csharp
// Open connection to device
await bridgeClient.SendCommand("open", 
    new Dictionary<string, object> { { "address", "SupernovaSimulatedPort" } });

// Get device information
await bridgeClient.SendCommand("get_usb_string", 
    new Dictionary<string, object> { { "subCommand", "MANUFACTURER" } });
await bridgeClient.SendCommand("get_usb_string", 
    new Dictionary<string, object> { { "subCommand", "FW_VERSION" } });

// Close connection
await bridgeClient.SendCommand("close");
```

### I2C Operations
```csharp
// Set bus voltage for I2C/SPI/UART
await bridgeClient.SendCommand("i2c_spi_uart_set_bus_voltage", 
    new Dictionary<string, object> { { "busVoltageInV", "3.3" } });

// Configure I2C parameters
await bridgeClient.SendCommand("i2c_set_parameters", 
    new Dictionary<string, object> { { "clockFrequencyInKHz", "100" } });

// Write to I2C device register
await bridgeClient.SendCommand("i2c_write_using_subaddress", new Dictionary<string, object>
{
    { "address", "50" },
    { "subaddress", "0000" },
    { "writeBuffer", "42" },
    { "clockFrequencyInKHz", "100" }
});

// Read from I2C device register
await bridgeClient.SendCommand("i2c_read_using_subaddress", new Dictionary<string, object>
{
    { "address", "50" },
    { "subaddress", "0000" },
    { "bytesToRead", "1" },
    { "busVoltageInV", "3.3" }
});

// Direct I2C write (no subaddress)
await bridgeClient.SendCommand("i2c_write", new Dictionary<string, object>
{
    { "address", "50" },
    { "writeBuffer", "DEADBEEF" },
    { "clockFrequencyInKHz", "100" }
});

// Direct I2C read (no subaddress)
await bridgeClient.SendCommand("i2c_read", new Dictionary<string, object>
{
    { "address", "50" },
    { "bytesToRead", "4" },
    { "busVoltageInV", "3.3" }
});
```

### I3C Operations
```csharp
// Initialize I3C bus
await bridgeClient.SendCommand("i3c_init_bus", 
    new Dictionary<string, object> { { "busVoltageInV", "3.3" } });

// Write to device register
await bridgeClient.SendCommand("i3c_write_using_subaddress", new Dictionary<string, object>
{
    { "address", "08" },
    { "subaddress", "0000" },
    { "mode", "SDR" },
    { "pushPullClockFrequencyInMHz", "5" },
    { "openDrainClockFrequencyInKHz", "1250" },
    { "writeBuffer", "04" }
});

// Read from device register
await bridgeClient.SendCommand("i3c_read_using_subaddress", new Dictionary<string, object>
{
    { "address", "08" },
    { "subaddress", "0000" },
    { "mode", "SDR" },
    { "pushPullClockFrequencyInMHz", "5" },
    { "openDrainClockFrequencyInKHz", "1250" },
    { "bytesToRead", "1" }
});

// Get device PID (Provisioned ID)
await bridgeClient.SendCommand("i3c_ccc_getpid", new Dictionary<string, object>
{
    { "address", "08" },
    { "pushPullClockFrequencyInMHz", "5" },
    { "openDrainClockFrequencyInKHz", "2500" }
});
```

## Best Practices

### Command Sequencing
1. **Always open connection first** with `open` command
2. **Initialize protocol bus** before operations (e.g., `i3c_init_bus`)
3. **Process responses sequentially** - wait for completion before next command
4. **Handle promise responses** - check `is_promise` field for multi-part responses
5. **Clean up resources** - use `using` statements and proper disposal

### Response Validation
```csharp
// Always validate response before processing
var response = responses.Last();
if (response.Status == "success")
{
    var data = (JsonElement)response.Data;
    var result = data.GetProperty("result");
    // Process result...
}
else
{
    // Handle error case
    Console.WriteLine($"Command failed: {response.Status}");
}
```

### Hex Data Handling
```csharp
// Convert hex string array to byte array
var payload = result.GetProperty("payload")
    .EnumerateArray()
    .Select(x => Convert.ToByte(x.GetString(), 16))
    .ToArray();

// Format bytes as hex for display
Console.WriteLine($"Data: [{string.Join(", ", payload.Select(b => $"0x{b:X2}"))}]");
```

### Configuration Batching
```csharp
// Batch multiple I3C register configurations
var i3cConfigCommands = new[]
{
    ("76", "00"), ("4E", "20"), ("13", "05"),
    ("16", "40"), ("5F", "61"), ("60", "0F00")
};

foreach (var (reg, value) in i3cConfigCommands)
{
    await bridgeClient.SendCommand("i3c_write_using_subaddress", new Dictionary<string, object>
    {
        { "address", "08" },
        { "subaddress", reg },
        { "mode", "SDR" },
        { "pushPullClockFrequencyInMHz", "5" },
        { "openDrainClockFrequencyInKHz", "1250" },
        { "writeBuffer", value }
    });
}

// Batch multiple I2C EEPROM writes
var eepromData = new[]
{
    ("0020", "01"), ("0021", "02"), ("0022", "03"),
    ("0023", "04"), ("0024", "FF"), ("0025", "AA")
};

foreach (var (address, value) in eepromData)
{
    await bridgeClient.SendCommand("i2c_write_using_subaddress", new Dictionary<string, object>
    {
        { "address", "50" },
        { "subaddress", address },
        { "writeBuffer", value },
        { "clockFrequencyInKHz", "100" }
    });
    await Task.Delay(5); // EEPROM write cycle time
}
```
