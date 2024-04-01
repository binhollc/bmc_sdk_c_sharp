# Binho Mission Control C# SDK

## Introduction

Welcome to the Binho Mission Control C# SDK (BMC C# SDK), a software tool designed to streamline your interaction with USB host adapters. Our SDK facilitates communication with Binho Nova and Supernova host adapters, offering a seamless integration for developers aiming to control these devices programmatically.

## Highlights

- **Unified Interface**: The SDK operates through the BMC Bridge, a REPL service that provides a consistent API for interacting with various host adapters. It accepts JSON-formatted command requests and returns JSON-formatted responses, ensuring a standardized communication protocol.
- **Extensibility**: Designed with extensibility in mind, the SDK allows for easy integration and control of future host adapters.
- **Examples Included**: Comes with practical examples (`i3c_basics` and `i3c_ibis`) to help you get started quickly.

## Prerequisites

- **BMC Bridge Installation**: The BMC Bridge must be installed separately and located in your system's PATH to run the examples provided with this SDK. This ensures that the SDK can communicate with the host adapters through the Bridge seamlessly.
  - On **Windows**, the bridge executable can typically be found in `C:\Program Files (x86)\BinhoMissionControlBridge` if the installer was used.
  - On **Mac and Linux**, the location will be where the bridge package was unzipped.
- **.NET SDK**: Ensure that the .NET SDK is installed on your development machine to build and execute the examples.

## Installation

Clone this repository to your local machine using the following command:

```
git clone https://github.com/binhollc/bmc_sdk_c_sharp.git
```

Navigate to the cloned directory and build the project:

```
cd bmc_sdk_c_sharpo
dotnet build
```

## Usage

To run the provided examples, use the following command:

```
dotnet run [example]
```

Replace `[example]` with the name of the example you wish to execute (`i3c_basics` or `i3c_ibis`).

## Examples

- **i3c_basics**: Demonstrates basic I3C commands using the Supernova simulated host adapter.
- **i3c_ibis**: Shows how to configure an IMU (ICM42605) to fire IBIs (In-Band Interrupts) and catch these notifications.

## Repository Contents

- `BridgeClient.cs`: A service for interacting with the BMC Bridge via stdin/stdout.
- `ExampleI3cBasics.cs`: A basic example showcasing I3C commands.
- `ExampleI3cIbis.cs`: An advanced example demonstrating the catching of IBIs.

## Bridge's API

For detailed information about the Bridge's API, visit [Binho Support](https://support.binho.io/bridge-supernova-api-1.0).

## Contributing

We welcome contributions! If you have suggestions for improvements or bug fixes, please feel free to make a pull request or open an issue.
