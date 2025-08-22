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
  - [UART Commands](#uart-commands)
  - [GPIO Commands](#gpio-commands)
  - [I3C Common Command Codes (CCC)](#i3c-common-command-codes-ccc)
- [Response Handling](#response-handling)
- [Error Handling](#error-handling)
- [Examples](#examples)

## Overview

The Binho Mission Control Bridge (BMC Bridge) is a service layer that provides a unified interface for communicating with Binho USB host adapters (Supernova and Pulsar). The Bridge accepts JSON-formatted commands via stdin and returns JSON-formatted responses via stdout, enabling easy integration with various programming languages and development environments.

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
- `BinhoPulsar` - For Pulsar devices

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
| BinhoPulsar | Pulsar | I2C, SPI, UART, GPIO USB host adapter |

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

#### I3C Bus Initialization

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "i3c_init_bus",
  "params": {
    "busVoltageInV": "3.3"
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
    "command": "i3c_init_bus"
  }
}
```

- Final Response to Setting Bus Voltage:

```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_set_bus_voltage",
    "status": "success",
    "result": {}
  }
}
```

- Final Response to Bus Initialization:

```json
{
  "transaction_id": "2",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_init_bus",
    "status": "success",
    "result": {}
  }
}
```

- Response to Get Target Device Table:

```json
{
  "transaction_id": "3",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_get_target_device_table",
    "status": "success",
    "result": {
      "payload": [
        {"static_address": "50", "dynamic_address": "08", "bcr": "10", "dcr": "C3", "pid": ["65", "64", "00", "00", "00", "00"]},
        {"static_address": "51", "dynamic_address": "09", "bcr": "10", "dcr": "C3", "pid": ["65", "64", "00", "00", "00", "00"]},
        {"static_address": "52", "dynamic_address": "0A", "bcr": "10", "dcr": "C3", "pid": ["65", "64", "00", "00", "00", "00"]},
        {"static_address": "53", "dynamic_address": "0B", "bcr": "03", "dcr": "63", "pid": ["5A", "00", "1D", "0F", "17", "02"]}
      ]
    }
  }
}
```

#### Reset I3C Bus

**Command Request:**

```json
{
  "transaction_id": "2",
  "command": "i3c_reset_bus",
  "params": {}
}
```

**Responses:**

1. Initial promise indicating the command is queued:

```json
{
  "transaction_id": "2",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_reset_bus"
  }
}
```

2. Final response indicating the outcome of the reset command:

```json
{
  "transaction_id": "2",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_reset_bus",
    "status": "success",
    "result": {}
  }
}
```

3. Response indicating the state of the target device table after reset:

```json
{
  "transaction_id": "2",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_get_target_device_table",
    "status": "success",
    "result": {
      "payload": []
    }
  }
}
```

#### Set Bus Voltage

Set the bus voltage for the connected device.

**Command Request:**

```json
{
  "transaction_id": "4",
  "command": "i3c_set_bus_voltage",
  "params": {
    "busVoltageInV": "3.3"
  }
}
```

**Responses:**

- Immediate promise response:

```json
{
  "transaction_id": "4",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_set_bus_voltage"
  }
}
```

- Final response:

```json
{
  "transaction_id": "4",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_set_bus_voltage",
    "status": "success",
    "result": {}
  }
}
```

#### I3C Get Target Device Table

**Command Request:**

```json
{
  "transaction_id": "6",
  "command": "i3c_get_target_device_table",
  "params": {}
}
```

**Responses:**

- Immediate Promise Response:

```json
{
  "transaction_id": "6",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_get_target_device_table"
  }
}
```

- Final Response:

```json
{
  "transaction_id": "6",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_get_target_device_table",
    "status": "success",
    "result": {
      "payload": [
        {"static_address": "50", "dynamic_address": "08", "bcr": "10", "dcr": "C3", "pid": ["65", "64", "00", "00", "00", "00"]},
        {"static_address": "51", "dynamic_address": "09", "bcr": "10", "dcr": "C3", "pid": ["65", "64", "00", "00", "00", "00"]},
        {"static_address": "52", "dynamic_address": "0A", "bcr": "10", "dcr": "C3", "pid": ["65", "64", "00", "00", "00", "00"]},
        {"static_address": "53", "dynamic_address": "0B", "bcr": "03", "dcr": "63", "pid": ["5A", "00", "1D", "0F", "17", "02"]}
      ]
    }
  }
}
```

#### I3C Write

##### Write Using Subaddress

**Command Request:**

```json
{
  "transaction_id": "5",
  "command": "i3c_write_using_subaddress",
  "params": {
    "address": "08",
    "subaddress": "0000",
    "mode": "SDR",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "writeBuffer": "DEADBEEF"
  }
}
```

**Responses:**

- Immediate Promise:

```json
{
  "transaction_id": "5",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_write_using_subaddress"
  }
}
```

- Final Response:

```json
{
  "transaction_id": "5",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_write_using_subaddress",
    "status": "success",
    "result": {
      "payload": [],
      "payload_size": 0
    }
  }
}
```

##### Direct Write

**Command Request:**

```json
{
  "transaction_id": "6",
  "command": "i3c_write",
  "params": {
    "address": "08",
    "mode": "SDR",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "writeBuffer": "0000"
  }
}
```

**Responses:**

- Immediate Promise:

```json
{
  "transaction_id": "6",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_write"
  }
}
```

- Final Response:

```json
{
  "transaction_id": "6",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_write",
    "status": "success",
    "result": {
      "payload": [],
      "payload_size": 0
    }
  }
}
```

#### I3C Read

##### Basic Read

**Command Request:**

```json
{
  "transaction_id": "7",
  "command": "i3c_read",
  "params": {
    "address": "08",
    "mode": "SDR",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "bytesToRead": "5"
  }
}
```

**Responses:**

```json
{
  "transaction_id": "7",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_read"
  }
}
```

```json
{
  "transaction_id": "7",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_read",
    "status": "success",
    "result": {
      "payload": ["DE", "AD", "BE", "EF", "00"],
      "payload_size": 5
    }
  }
}
```

##### Read using Subaddress

**Command Request:**

```json
{
  "transaction_id": "8",
  "command": "i3c_read_using_subaddress",
  "params": {
    "address": "08",
    "mode": "SDR",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "subaddress": "0000",
    "bytesToRead": "5"
  }
}
```

**Responses:**

```json
{
  "transaction_id": "8",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_read_using_subaddress"
  }
}
```

```json
{
  "transaction_id": "8",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_read_using_subaddress",
    "status": "success",
    "result": {
      "payload": ["DE", "AD", "BE", "EF", "00"],
      "payload_size": 5
    }
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

### UART Commands

#### UART Initialization

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "uart_init",
  "params": {
    "baudRate": "<600|1200|2400|4800|9600|14400|19200|38400|56000|57600|115200>",
    "hardwareHandShake": <Boolean>,
    "parity": "<0|1|2>",
    "dataSize": "<0|1>",
    "stopBit": "1"
  }
}
```

**Field Descriptions:**
- **`parity`**: Sets the UART parity mode:
  - `0` = No parity
  - `1` = Even parity
  - `2` = Odd parity
- **`dataSize`**: Defines the number of data bits per frame:
  - `0` = 7-bit
  - `1` = 8-bit
- **`stopBit`**: Selects the number of stop bits:
  - `0` = 1 stop bit
  - `1` = 2 stop bits

**Responses:**

- Immediate Promise Response:

```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "uart_init"
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
    "is_response_to": "uart_init",
    "status": "success"
  }
}
```

#### UART Configuration

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "uart_config",
  "params": {
    "baudRate": "<600|1200|2400|4800|9600|14400|19200|38400|56000|57600|115200>",
    "hardwareHandShake": <Boolean>,
    "parity": "<0|1|2>",
    "dataSize": "<0|1>",
    "stopBit": "1"
  }
}
```

**Field Descriptions:**
- **`parity`**: Sets the UART parity mode:
  - `0` = No parity
  - `1` = Even parity
  - `2` = Odd parity
- **`dataSize`**: Defines the number of data bits per frame:
  - `0` = 7-bit
  - `1` = 8-bit
- **`stopBit`**: Selects the number of stop bits:
  - `0` = 1 stop bit
  - `1` = 2 stop bits

**Responses:**

- Immediate Promise Response:

```json
{
  "transaction_id": "1",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "uart_config"
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
    "is_response_to": "uart_config",
    "status": "success"
  }
}
```

#### UART Send

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "uart_send",
  "params": {
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
    "command": "uart_send"
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
    "is_response_to": "uart_send",
    "status": "success"
  }
}
```

#### UART Interruption Notification Message

```json
{
  "transaction_id": "0",
  "status": "success",
  "type": "notification",
  "is_promise": false,
  "data": {
    "type": "UART_MESSAGE_RECEIVED",
    "payload": ["DE", "AD", "BE", "EF"],
    "payload_length": 4
  }
}
```

### GPIO Commands

#### GPIO Configuration

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "gpio_config_pin",
  "params": {
    "pinNumber": "<1..6>",
    "functionality": "<DIN|DOUT>"
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
    "command": "gpio_config_pin"
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
    "is_response_to": "gpio_config_pin",
    "status": "success"
  }
}
```

#### GPIO Read

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "gpio_digital_read",
  "params": {
    "pinNumber": "<1..6>"
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
    "command": "gpio_digital_read"
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
    "is_response_to": "gpio_digital_read",
    "status": "success",
    "logic_level": "<LOW|HIGH>"
  }
}
```

#### GPIO Write

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "gpio_digital_write",
  "params": {
    "pinNumber": "<1..6>",
    "logicLevel": "<0|1>"
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
    "command": "gpio_digital_write"
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
    "is_response_to": "gpio_digital_write",
    "status": "success"
  }
}
```

#### Configure GPIO Interruptions

**Command Request:**

```json
{
  "transaction_id": "1",
  "command": "gpio_set_interrupt",
  "params": {
    "pinNumber": "<1..6>",
    "edgeTrigger": "<RISING|FALLING|BOTH>"
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
    "command": "gpio_set_interrupt"
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
    "is_response_to": "gpio_set_interrupt",
    "status": "success"
  }
}
```

#### GPIO Interruption Notification Message

```json
{
  "transaction_id": "0",
  "status": "success",
  "type": "notification",
  "is_promise": false,
  "data": {
    "payload": {
      "pin": "<1..6>"
    }
  }
}
```

### I3C Common Command Codes (CCC)

The CCC (Common Command Codes) provides a set of universal commands supported across multiple devices. The Bridge for Supernova has a few CCCs which can be used to interact with the downstream devices. This section details these CCCs. For more information on this topic see [I3C Common Command Codes](https://support.binho.io/user-guide/protocols-and-interfaces/i3c-common-command-codes#what-are-common-command-codes) section.

#### GETPID

**Command Request:**

```json
{
  "transaction_id": "9",
  "command": "i3c_ccc_send",
  "params": {
    "cccName": "GETPID",
    "address": "08",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "cccParams": {}
  }
}
```

**Responses:**

1. Immediate promise:

```json
{
  "transaction_id": "9",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_ccc_getpid"
  }
}
```

2. Command result:

```json
{
  "transaction_id": "9",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_ccc_getpid",
    "status": "success",
    "result": {
      "payload": ["0", "0", "0", "0", "64", "65"],
      "payload_size": 6
    }
  }
}
```

#### DIRECTSETMRL

**Command Request:**

```json
{
  "transaction_id": "10",
  "command": "i3c_ccc_send",
  "params": {
    "cccName": "DIRECTSETMRL",
    "address": "08",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "cccParams": { 
      "cccDataBuffer": "10"
    }
  }
}
```

**Responses:**

1. Immediate promise:

```json
{
  "transaction_id": "10",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_ccc_direct_setmrl"
  }
}
```

2. Command result:

```json
{
  "transaction_id": "10",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_ccc_direct_setmrl",
    "status": "success",
    "result": {
      "payload": ["00", "00"],
      "payload_size": 2
    }
  }
}
```

#### DIRECTENEC

**Command Request:**

```json
{
  "transaction_id": "11",
  "command": "i3c_ccc_send",
  "params": {
    "cccName": "DIRECTENEC",
    "address": "08",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "cccParams": { 
      "events": ["ENINT", "ENCR", "ENHJ"]
    }
  }
}
```

**Responses:**

1. Immediate promise:

```json
{
  "transaction_id": "11",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_ccc_direct_enec"
  }
}
```

2. Command result:

```json
{
  "transaction_id": "11",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_ccc_direct_enec",
    "status": "success"
  }
}
```

**Note:** For `DIRECTDISEC` the response's format is very similar, except that the accepted events are ["DISINT", "DISCR", "DISHJ"]. For `BROADCASTENEC` and `BROADCASTDISEC` is also similar but address parameter is not required.

#### SETAASA

**Command Request:**

```json
{
  "transaction_id": "12",
  "command": "i3c_ccc_send",
  "params": {
    "cccName": "SETAASA",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "cccParams": {
      "staticAddresses": ["08", "09", "0A"]
    }
  }
}
```

**Responses:**

1. Immediate promise:

```json
{
  "transaction_id": "12",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_ccc_setaasa"
  }
}
```

2. Command result:

```json
{
  "transaction_id": "12",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_ccc_setaasa",
    "status": "success"
  }
}
```

#### ENTDAA

**Command Request:**

```json
{
  "transaction_id": "13",
  "command": "i3c_ccc_send",
  "params": {
    "cccName": "ENTDAA",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "cccParams": {
      "targetDeviceTable": {
        "BMM350": {
          "staticAddress": "0x14",
          "dynamicAddress": "0x0C",
          "i3cFeatures": "0x0B",
          "maxIbiPayloadLength": "0xE9",
          "bcr": "0x26",
          "dcr": "0x43",
          "pid": ["0x07", "0x70", "0x10", "0x33", "0x00", "0x00"]
        }
      }
    }
  }
}
```

**Responses:**

1. Immediate promise:

```json
{
  "transaction_id": "13",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_ccc_entdaa"
  }
}
```

2. Command result:

```json
{
  "transaction_id": "13",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_ccc_entdaa",
    "status": "success"
  }
}
```

**Note:** When using this command is necessary to previously run `i3c_init_bus` followed by `i3c_reset_bus`.

#### BROADCASTRSTACT

**Command Request:**

```json
{
  "transaction_id": "14",
  "command": "i3c_ccc_send",
  "params": {
    "cccName": "BROADCASTRSTACT",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "cccParams": {
      "definingByte": "02"
    }
  }
}
```

**Responses:**

1. Immediate promise:

```json
{
  "transaction_id": "14",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_ccc_broadcast_rstact"
  }
}
```

2. Command result:

```json
{
  "transaction_id": "14",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_ccc_broadcast_rstact",
    "status": "success"
  }
}
```

**Note:** Similar to `DIRECTRSTACT` but does not require an address parameter.

#### DIRECTENDXFER

**Command Request:**

```json
{
  "transaction_id": "15",
  "command": "i3c_ccc_send",
  "params": {
    "cccName": "DIRECTENDXFER",
    "address": "08",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "cccParams": {
      "definingByte": "AA",
      "data": "20"
    }
  }
}
```

**Responses:**

1. Immediate promise:

```json
{
  "transaction_id": "15",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_ccc_direct_endxfer"
  }
}
```

2. Command result:

```json
{
  "transaction_id": "15",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_ccc_direct_endxfer",
    "status": "success",
    "result": {
      "payload": [0],
      "payload_size": 1
    }
  }
}
```

**Note:** Similar to `BROADCASTENDXFER`, but requires an address parameter.

#### BROADCASTSETXTIME

**Command Request:**

```json
{
  "transaction_id": "16",
  "command": "i3c_ccc_send",
  "params": {
    "cccName": "BROADCASTSETXTIME",
    "pushPullClockFrequencyInMHz": "5",
    "openDrainClockFrequencyInKHz": "2500",
    "cccParams": {
      "definingByte": "3F",
      "data": ["DE", "AD", "BE", "EF"]
    }
  }
}
```

**Responses:**

1. Immediate promise:

```json
{
  "transaction_id": "16",
  "status": "success",
  "type": "command_response",
  "is_promise": true,
  "data": {
    "command": "i3c_ccc_broadcast_setxtime"
  }
}
```

2. Command result:

```json
{
  "transaction_id": "16",
  "status": "success",
  "type": "command_response",
  "is_promise": false,
  "data": {
    "is_response_to": "i3c_ccc_broadcast_setxtime",
    "status": "success",
    "result": {
      "payload": [0, 0, 0, 0, 0],
      "payload_size": 5
    }
  }
}
```

**Note:** Similar to `DIRECTSETXTIME`, but does not require an address parameter.

#### Currently Supported CCCs

Refer to [this table](https://support.binho.io/user-guide/protocols-and-interfaces/i3c-common-command-codes#supported-cccs) for current support status (see Bridge column).

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
# Start Bridge in interactive mode
bmcbridge BinhoSupernova

# The Bridge will start and wait for JSON commands on stdin
# Type commands line by line (REPL style):

{"transaction_id":"1","command":"open","params":{"address":"SupernovaSimulatedPort"}}
{"transaction_id":"2","command":"i3c_init_bus","params":{"busVoltageInV":"3.3"}}
{"transaction_id":"3","command":"i3c_read","params":{"address":"08","mode":"SDR","pushPullClockFrequencyInMHz":"5","openDrainClockFrequencyInKHz":"2500","bytesToRead":"4"}}
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

# Read all responses for open command
print("Open command responses:")
while True:
    response = json.loads(bridge.stdout.readline())
    print(f"  Response: {json.dumps(response, indent=2)}")
    if not response.get("is_promise", False):
        break  # This is the final response

# Send close command
close_command = {
    "transaction_id": "2",
    "command": "close",
    "params": {}
}

bridge.stdin.write(json.dumps(close_command) + '\n')
bridge.stdin.flush()

# Read all responses for close command
print("\nClose command responses:")
while True:
    close_response = json.loads(bridge.stdout.readline())
    print(f"  Response: {json.dumps(close_response, indent=2)}")
    if not close_response.get("is_promise", False):
        break  # This is the final response

# Close the bridge process
bridge.terminate()
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
