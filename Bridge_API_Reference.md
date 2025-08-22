# Binho Mission Control Bridge API Reference

## Table of Contents

- [Overview](#overview)
- [Bridge Service Layer](#bridge-service-layer)
- [Command Adaptors](#command-adaptors)
- [Command and Response Structure](#command-and-response-structure)
- [Device Management Commands](#device-management-commands)
- [Protocol Specific Commands](#protocol-specific-commands)
  - [I3C Commands](#i3c-commands)
  - [I2C Commands](#i2c-commands)
  - [SPI Commands](#spi-commands)
  - [I3C Common Command Codes (CCC)](#i3c-common-command-codes-ccc)
- [Response Handling](#response-handling)
- [Error Handling](#error-handling)
- [Examples](#examples)

## Overview

The Binho Mission Control Bridge (BMC Bridge) is a service layer that provides a unified interface for communicating with Binho USB host adapters (Nova and Supernova). The Bridge accepts JSON-formatted commands via stdin and returns JSON-formatted responses via stdout, enabling easy integration with various programming languages and development environments.

### Key Features
- **Unified API**: Consistent interface across all supported host adapters
- **JSON Communication**: Standard JSON request/response format
- **Asynchronous Operations**: Support for promise-based command execution
- **Transaction Management**: Built-in transaction ID tracking
- **Protocol Support**: I3C, I2C, SPI, UART, GPIO, and more
- **Device Management**: Connection, configuration, and status monitoring

## Bridge Service Layer

The Bridge service acts as an intermediary between your application and the physical USB host adapter. It handles:

- Device discovery and connection management
- Protocol initialization and configuration
- Command queuing and execution
- Response formatting and delivery
- Error handling and status reporting

### Starting the Bridge

```bash
bmcbridge <CommandAdaptor>
```

**Command Adaptors:**
- `BinhoSupernova` - For Supernova devices
- `BinhoNova` - For Nova devices
- `SupernovaSimulatedPort` - For simulation/testing

## Command Adaptors

Command adaptors are device-specific modules that translate generic Bridge commands into device-specific operations. Each adaptor handles:

- Device-specific communication protocols
- Hardware abstraction
- Feature mapping
- Error translation

### Available Adaptors

| Adaptor | Device | Description |
|---------|--------|-------------|
| BinhoSupernova | Supernova | Full-featured USB host adapter |
| BinhoNova | Nova | Compact USB host adapter |
| SupernovaSimulatedPort | Simulation | Virtual device for testing |

## Command and Response Structure

### Command Format

All commands sent to the Bridge follow this JSON structure:

```json
{
  "transaction_id": "string",
  "command": "string",
  "params": {
    "parameter1": "value1",
    "parameter2": "value2"
  }
}
```

**Fields:**
- `transaction_id`: Unique identifier for command tracking
- `command`: Command name (e.g., "open", "i3c_init_bus")
- `params`: Object containing command-specific parameters

### Response Format

```json
{
  "transaction_id": "string",
  "status": "string",
  "type": "string",
  "is_promise": boolean,
  "data": object
}
```

**Fields:**
- `transaction_id`: Matches the command transaction ID
- `status`: Command execution status ("success", "error", etc.)
- `type`: Response type ("response", "notification", etc.)
- `is_promise`: `true` if more responses are expected
- `data`: Response payload containing results or error information

## Device Management Commands

### Device Connection

#### `open`
Opens connection to a host adapter.

**Parameters:**
```json
{
  "address": "string"  // Optional: specific device address
}
```

**Example:**
```json
{
  "transaction_id": "1",
  "command": "open",
  "params": {
    "address": "SupernovaSimulatedPort"
  }
}
```

#### `close`
Closes connection to the current device.

**Parameters:** None

#### `exit`
Terminates the Bridge service.

**Parameters:** None

### Device Information

#### `get_usb_string`
Retrieves USB device information strings.

**Parameters:**
```json
{
  "subCommand": "string"  // MANUFACTURER, PRODUCT_NAME, SERIAL_NUMBER, HW_VERSION, FW_VERSION
}
```

**Example:**
```json
{
  "transaction_id": "2",
  "command": "get_usb_string",
  "params": {
    "subCommand": "MANUFACTURER"
  }
}
```

## Protocol Specific Commands

### I3C Commands

#### Bus Initialization

##### `i3c_init_bus`
Initializes the I3C bus with specified voltage.

**Parameters:**
```json
{
  "busVoltageInV": "string"  // Bus voltage (e.g., "1.8", "3.3")
}
```

**Example:**
```json
{
  "transaction_id": "3",
  "command": "i3c_init_bus",
  "params": {
    "busVoltageInV": "3.3"
  }
}
```

#### Data Transfer

##### `i3c_write_using_subaddress`
Writes data to a device using subaddressing.

**Parameters:**
```json
{
  "address": "string",                        // Device address (hex)
  "subaddress": "string",                     // Register/subaddress (hex)
  "mode": "string",                           // Transfer mode ("SDR", "HDR-DDR")
  "pushPullClockFrequencyInMHz": "string",    // Push-pull clock frequency
  "openDrainClockFrequencyInKHz": "string",   // Open-drain clock frequency
  "writeBuffer": "string"                     // Data to write (hex)
}
```

**Example:**
```json
{
  "transaction_id": "4",
  "command": "i3c_write_using_subaddress",
  "params": {
    "address": "08",
    "subaddress": "0000",
    "mode": "SDR",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "1250",
    "writeBuffer": "04"
  }
}
```

##### `i3c_read_using_subaddress`
Reads data from a device using subaddressing.

**Parameters:**
```json
{
  "address": "string",                        // Device address (hex)
  "subaddress": "string",                     // Register/subaddress (hex)
  "mode": "string",                           // Transfer mode ("SDR", "HDR-DDR")
  "pushPullClockFrequencyInMHz": "string",    // Push-pull clock frequency
  "openDrainClockFrequencyInKHz": "string",   // Open-drain clock frequency
  "bytesToRead": "string"                     // Number of bytes to read
}
```

**Example:**
```json
{
  "transaction_id": "5",
  "command": "i3c_read_using_subaddress",
  "params": {
    "address": "08",
    "subaddress": "0000",
    "mode": "SDR",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "1250",
    "bytesToRead": "1"
  }
}
```

### I2C Commands

#### Bus Voltage Setup

##### `i2c_spi_uart_set_bus_voltage`
Sets the bus voltage for I2C/SPI/UART protocols.

**Parameters:**
```json
{
  "busVoltageInV": "string"  // Bus voltage as float (e.g., "1.8", "3.3")
}
```

**Example:**
```json
{
  "transaction_id": "1",
  "command": "i2c_spi_uart_set_bus_voltage",
  "params": {
    "busVoltageInV": "3.3"
  }
}
```

**Response:**
```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i2c_spi_uart_set_bus_voltage",
    "status": "success"
  }
}
```

##### `i2c_set_parameters`
Configures I2C communication parameters.

**Parameters:**
```json
{
  "clockFrequencyInKHz": "string"  // Clock frequency in KHz (e.g., "100", "400")
}
```

**Example:**
```json
{
  "transaction_id": "2",
  "command": "i2c_set_parameters",
  "params": {
    "clockFrequencyInKHz": "400"
  }
}
```

#### Data Transfer

##### `i2c_read`
Reads data from an I2C device.

**Parameters:**
```json
{
  "address": "string",        // Device address (2-digit hex, e.g., "50")
  "bytesToRead": "string",    // Number of bytes to read
  "busVoltageInV": "string"   // Bus voltage as float
}
```

**Example:**
```json
{
  "transaction_id": "3",
  "command": "i2c_read",
  "params": {
    "address": "50",
    "bytesToRead": "4",
    "busVoltageInV": "3.3"
  }
}
```

**Response:**
```json
{
  "transaction_id": "3",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i2c_read",
    "status": "success",
    "data": ["DE", "AD", "BE", "EF"]
  }
}
```

##### `i2c_write`
Writes data to an I2C device.

**Parameters:**
```json
{
  "address": "string",               // Device address (2-digit hex, e.g., "50")
  "writeBuffer": "string",           // Data to write (hex string, e.g., "DEADBEEF")
  "clockFrequencyInKHz": "string"    // Clock frequency in KHz
}
```

**Example:**
```json
{
  "transaction_id": "4",
  "command": "i2c_write",
  "params": {
    "address": "50",
    "writeBuffer": "DEADBEEF",
    "clockFrequencyInKHz": "400"
  }
}
```

##### `i2c_read_using_subaddress`
Reads data from an I2C device using subaddressing (register addressing).

**Parameters:**
```json
{
  "address": "string",        // Device address (2-digit hex, e.g., "50")
  "subaddress": "string",     // Register address (e.g., "0001")
  "bytesToRead": "string",    // Number of bytes to read
  "busVoltageInV": "string"   // Bus voltage as float
}
```

**Example:**
```json
{
  "transaction_id": "5",
  "command": "i2c_read_using_subaddress",
  "params": {
    "address": "50",
    "subaddress": "0001",
    "bytesToRead": "4",
    "busVoltageInV": "3.3"
  }
}
```

**Response:**
```json
{
  "transaction_id": "5",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i2c_read_using_subaddress",
    "status": "success",
    "data": ["DE", "AD", "BE", "EF"]
  }
}
```

##### `i2c_write_using_subaddress`
Writes data to an I2C device using subaddressing (register addressing).

**Parameters:**
```json
{
  "address": "string",               // Device address (2-digit hex, e.g., "50")
  "subaddress": "string",            // Register address (e.g., "0001")
  "writeBuffer": "string",           // Data to write (hex string, e.g., "DEADBEEF")
  "clockFrequencyInKHz": "string"    // Clock frequency in KHz
}
```

**Example:**
```json
{
  "transaction_id": "6",
  "command": "i2c_write_using_subaddress",
  "params": {
    "address": "50",
    "subaddress": "0001",
    "writeBuffer": "DEADBEEF",
    "clockFrequencyInKHz": "400"
  }
}
```

### SPI Commands

#### SPI Initialization

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "spi_init",
  "params": {
    "mode": "<0..3>",
    "clockFrequencyInKHz": "<Unsigned Integer>",
    "bitOrder": "<MSB|LSB>",
    "bitsPerTransfer": "<8|16>",
    "chipSelect": "<0..3>",
    "chipSelectPol": "<0,1>"
  }
}
```

**Responses:**

- Immediate Promise Response:

```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "spi_init"
  }
}
```

- Final Response:

```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "spi_init",
    "status": "success"
  }
}
```

#### SPI Configuration

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "spi_config",
  "params": {
    "mode": "<0..3>",
    "clockFrequencyInKHz": "<Unsigned Integer>",
    "bitOrder": "<MSB|LSB>",
    "bitsPerTransfer": "<8|16>",
    "chipSelect": "<0..3>",
    "chipSelectPol": "<0,1>"
  }
}
```

**Responses:**

- Immediate Promise Response:

```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "spi_config"
  }
}
```

- Final Response:

```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "spi_config",
    "status": "success"
  }
}
```

#### SPI Transfer

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "spi_transfer",
  "params": {
    "bytesToRead": "<Unsigned Integer>",
    "writeBuffer": "<Hex String (e.g. DEADBEEF)>"
  }
}
```

**Responses:**

- Immediate Promise Response:

```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "spi_transfer"
  }
}
```

- Final Response:

```json
{
  "transaction_id": "1",
  "status": "success",
  "is_promise": false,
  "data": {
    "is_response_to": "spi_transfer",
    "status": "success",
    "payload_length": "<Unsigned Integer>",
    "data": ["DE", "AD", "BE", "EF"]
  }
}
```

### I3C Common Command Codes (CCC)

Common Command Codes are standardized I3C commands for device management and configuration. For more information, see the [I3C Common Command Codes documentation](https://support.binho.io/user-guide/protocols-and-interfaces/i3c-common-command-codes).

#### `i3c_ccc_getpid`
Retrieves the Provisioned ID (PID) from an I3C device.

**Parameters:**
```json
{
  "address": "string",                        // Device address (hex)
  "pushPullClockFrequencyInMHz": "string",    // Push-pull clock frequency
  "openDrainClockFrequencyInKHz": "string"    // Open-drain clock frequency (optional)
}
```

**Example:**
```json
{
  "transaction_id": "6",
  "command": "i3c_ccc_getpid",
  "params": {
    "address": "08",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500"
  }
}
```

**Response:**
```json
{
  "transaction_id": "6",
  "status": "success",
  "type": "response",
  "is_promise": false,
  "data": {
    "result": {
      "payload": ["48", "01", "23", "45", "67", "89"]
    }
  }
}
```

#### Other CCC Commands

The Bridge supports additional CCC commands including:
- `i3c_ccc_getbcr` - Get Bus Characteristics Register
- `i3c_ccc_getdcr` - Get Device Characteristics Register
- `i3c_ccc_getstatus` - Get Device Status
- `i3c_ccc_setmrl` - Set Maximum Read Length
- `i3c_ccc_getmrl` - Get Maximum Read Length
- `i3c_ccc_setmwl` - Set Maximum Write Length
- `i3c_ccc_getmwl` - Get Maximum Write Length

For the complete list of supported CCCs, refer to the [supported CCCs documentation](https://support.binho.io/user-guide/protocols-and-interfaces/i3c-common-command-codes#supported-cccs).

## Response Handling

### Promise Responses

Some commands may return multiple responses. The `is_promise` field indicates whether additional responses are expected:

- `is_promise: true` - More responses will follow
- `is_promise: false` - This is the final response

### Notifications

Notifications are special responses with `transaction_id: "0"` that indicate asynchronous events such as:
- In-Band Interrupts (IBIs)
- Device state changes
- Error conditions

## Error Handling

### Error Response Format

```json
{
  "transaction_id": "string",
  "status": "error",
  "type": "response",
  "is_promise": false,
  "data": {
    "error": {
      "code": "string",
      "message": "string",
      "details": object
    }
  }
}
```

### Common Error Codes

| Code | Description |
|------|-------------|
| `DEVICE_NOT_FOUND` | Host adapter not found or connected |
| `INVALID_PARAMETER` | Invalid command parameter |
| `COMMUNICATION_ERROR` | Communication failure with device |
| `TIMEOUT` | Command execution timeout |
| `PROTOCOL_ERROR` | Protocol-specific error |

## Examples

### C# Integration

```csharp
using var bridgeClient = new BridgeClient("BinhoSupernova");
await bridgeClient.StartAsync();

// Open connection
await bridgeClient.SendCommand("open", 
    new Dictionary<string, object> { 
        { "address", "SupernovaSimulatedPort" } 
    });

// Initialize I3C bus
await bridgeClient.SendCommand("i3c_init_bus", 
    new Dictionary<string, object> { 
        { "busVoltageInV", "3.3" } 
    });

// Read device PID
var responses = await bridgeClient.SendCommand("i3c_ccc_getpid", 
    new Dictionary<string, object> {
        { "address", "08" },
        { "pushPullClockFrequencyInMHz", "5" }
    });

// Process response
var response = responses.Last();
var data = (JsonElement)response.Data;
var result = data.GetProperty("result");
var payload = result.GetProperty("payload")
    .EnumerateArray()
    .Select(x => Convert.ToByte(x.GetString(), 16))
    .ToArray();

Console.WriteLine($"PID: {string.Join(":", payload.Select(b => $"{b:X2}"))}");
```

### Command Line Usage

```bash
# Start Bridge
bmcbridge BinhoSupernova

# Send commands via stdin
echo '{"transaction_id":"1","command":"open","params":{"address":"SupernovaSimulatedPort"}}' | bmcbridge BinhoSupernova
```

### Python Integration

```python
import json
import subprocess

# Start bridge process
bridge = subprocess.Popen(
    ['bmcbridge', 'BinhoSupernova'],
    stdin=subprocess.PIPE,
    stdout=subprocess.PIPE,
    text=True
)

# Send command
command = {
    "transaction_id": "1",
    "command": "open",
    "params": {"address": "SupernovaSimulatedPort"}
}

bridge.stdin.write(json.dumps(command) + '\n')
bridge.stdin.flush()

# Read response
response = json.loads(bridge.stdout.readline())
print(f"Status: {response['status']}")
```

### Advanced Features

#### Handling IBIs (In-Band Interrupts)

```csharp
bridgeClient.OnNotificationReceived += (sender, notification) => {
    if (notification.TransactionId == "0") {
        // Handle IBI notification
        Console.WriteLine($"IBI received: {notification.Data}");
    }
};
```

#### Batch Operations

```csharp
// Configure device with multiple register writes
var configCommands = new[]
{
    ("76", "00"), ("4E", "20"), ("13", "05"),
    ("16", "40"), ("5F", "61"), ("60", "0F00")
};

foreach (var (reg, value) in configCommands)
{
    await bridgeClient.SendCommand("i3c_write_using_subaddress",
        new Dictionary<string, object>
        {
            { "address", "08" },
            { "subaddress", reg },
            { "mode", "SDR" },
            { "pushPullClockFrequencyInMHz", "5" },
            { "openDrainClockFrequencyInKHz", "1250" },
            { "writeBuffer", value }
        });
}
```

## Best Practices

1. **Always check response status** before processing data
2. **Handle promise responses** appropriately for multi-part operations
3. **Use appropriate clock frequencies** for your I3C devices
4. **Implement timeout handling** for long-running operations
5. **Subscribe to notifications** for event-driven applications
6. **Properly dispose of resources** using `using` statements in C#
7. **Use hex string format** for addresses and data payload
8. **Validate parameters** before sending commands

## Troubleshooting

### Common Issues

1. **Bridge not found**: Ensure `bmcbridge` is in your system PATH
2. **Device connection failures**: Check device is properly connected and powered
3. **Communication timeouts**: Verify clock frequencies are appropriate for your device
4. **Invalid addresses**: Ensure addresses are in correct hex string format
5. **Protocol errors**: Check device documentation for supported features

### Debug Information

Enable verbose logging by setting environment variables:
```bash
export BMC_DEBUG=1
bmcbridge BinhoSupernova
```

This documentation provides a comprehensive reference for the Binho Mission Control Bridge API. For additional examples and advanced usage, refer to the SDK examples and device-specific documentation.
