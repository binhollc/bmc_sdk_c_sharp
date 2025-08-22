using System.Text.Json;

/// <summary>
/// Example demonstrating basic I2C EEPROM operations using the BMC Bridge.
/// This example shows how to:
/// - Initialize I2C bus and set parameters
/// - Write individual bytes and pages to EEPROM
/// - Read data from EEPROM using subaddressing
/// - Perform batch operations for efficient EEPROM programming
/// - Display hex data in readable format
/// 
/// Target Device: I2C EEPROM at address 0x50 (typical for 24LC series)
/// Common EEPROM types: 24LC01, 24LC02, 24LC04, 24LC08, 24LC16, etc.
/// </summary>
class ExampleI2cEeprom
{
    private static void PrintResponses(List<CommandResponse> responses, string operationName = "")
    {
        if (!string.IsNullOrEmpty(operationName))
        {
            Console.WriteLine($"=== {operationName} ===");
        }
        
        foreach (var response in responses)
        {
            Console.WriteLine($"Transaction ID: {response.TransactionId}");
            Console.WriteLine($"Status: {response.Status}");
            Console.WriteLine($"Type: {response.Type}");
            Console.WriteLine($"Is Promise: {response.IsPromise}");
            
            if (response.Status == "success" && response.Data is JsonElement data)
            {
                // Check if this is a read response with data array
                if (data.TryGetProperty("data", out JsonElement dataElement))
                {
                    if (dataElement.ValueKind == JsonValueKind.Array)
                    {
                        // Handle array response (typical for read operations)
                        var hexData = dataElement.EnumerateArray()
                            .Select(x => x.GetString())
                            .ToArray();
                        Console.WriteLine($"Data: [{string.Join(", ", hexData.Select(h => $"0x{h}"))}]");
                        
                        // Try to display as ASCII if printable
                        try
                        {
                            var bytes = hexData.Select(h => Convert.ToByte(h, 16)).ToArray();
                            var ascii = System.Text.Encoding.ASCII.GetString(bytes);
                            // Check if all characters are printable ASCII (32-126) or common control chars
                            if (ascii.All(c => (c >= 32 && c <= 126) || c == '\0' || c == '\n' || c == '\r' || c == '\t'))
                            {
                                Console.WriteLine($"ASCII: \"{ascii.Replace('\0', '.')}\"");
                            }
                        }
                        catch
                        {
                            // Ignore ASCII conversion errors
                        }
                    }
                    else if (dataElement.ValueKind == JsonValueKind.String)
                    {
                        // Handle string response (typical for device info)
                        Console.WriteLine($"Data: \"{dataElement.GetString()}\"");
                    }
                    else
                    {
                        // Handle other data types
                        Console.WriteLine($"Data: {dataElement}");
                    }
                }
                else
                {
                    // Print the entire data object if no 'data' property
                    Console.WriteLine($"Response Data: {data}");
                }
            }
            Console.WriteLine("----------------------------------------");
        }
        Console.WriteLine();
    }

    private static async Task<List<CommandResponse>> WriteEepromByte(BridgeClient client, string address, string memoryAddress, string data)
    {
        return await client.SendCommand("i2c_write_using_subaddress", new Dictionary<string, object>
        {
            { "address", address },
            { "subaddress", memoryAddress },
            { "writeBuffer", data },
            { "clockFrequencyInKHz", "100" }  // Standard I2C speed for EEPROM
        });
    }

    private static async Task<List<CommandResponse>> ReadEepromBytes(BridgeClient client, string address, string memoryAddress, int bytesToRead)
    {
        return await client.SendCommand("i2c_read_using_subaddress", new Dictionary<string, object>
        {
            { "address", address },
            { "subaddress", memoryAddress },
            { "bytesToRead", bytesToRead.ToString() },
            { "busVoltageInV", "3.3" }
        });
    }

    public static async Task Run()
    {
        Console.WriteLine("=== BMC I2C EEPROM Example ===");
        Console.WriteLine("Demonstrating basic I2C EEPROM operations at address 0x50");
        Console.WriteLine("This example will write and read data from an I2C EEPROM\n");

        using var bridgeClient = new BridgeClient("BinhoSupernova");
        await bridgeClient.StartAsync();

        const string eepromAddress = "50"; // 0x50 - typical I2C EEPROM address

        try
        {
            // 1. Open connection to device
            Console.WriteLine("1. Opening connection to device...");
            PrintResponses(
                await bridgeClient.SendCommand("open", 
                    new Dictionary<string, object> { { "address", "SupernovaSimulatedPort" } }),
                "Open Connection"
            );

            // 2. Set bus voltage for I2C operations
            Console.WriteLine("2. Setting bus voltage to 3.3V...");
            PrintResponses(
                await bridgeClient.SendCommand("i2c_spi_uart_set_bus_voltage", 
                    new Dictionary<string, object> { { "busVoltageInV", "3.3" } }),
                "Set Bus Voltage"
            );

            // 3. Configure I2C parameters
            Console.WriteLine("3. Configuring I2C parameters (100 KHz)...");
            PrintResponses(
                await bridgeClient.SendCommand("i2c_set_parameters", 
                    new Dictionary<string, object> { { "clockFrequencyInKHz", "100" } }),
                "Set I2C Parameters"
            );

            // 4. Get device information
            Console.WriteLine("4. Getting device information...");
            PrintResponses(
                await bridgeClient.SendCommand("get_usb_string", 
                    new Dictionary<string, object> { { "subCommand", "MANUFACTURER" } }),
                "Device Manufacturer"
            );

            PrintResponses(
                await bridgeClient.SendCommand("get_usb_string", 
                    new Dictionary<string, object> { { "subCommand", "PRODUCT_NAME" } }),
                "Product Name"
            );

            // 5. Write single byte to EEPROM
            Console.WriteLine("5. Writing single byte (0x42) to address 0x0000...");
            PrintResponses(
                await WriteEepromByte(bridgeClient, eepromAddress, "0000", "42"),
                "Write Single Byte"
            );
            
            // Small delay for EEPROM write cycle
            await Task.Delay(10);

            // 6. Read single byte from EEPROM
            Console.WriteLine("6. Reading single byte from address 0x0000...");
            PrintResponses(
                await ReadEepromBytes(bridgeClient, eepromAddress, "0000", 1),
                "Read Single Byte"
            );

            // 7. Write a string to EEPROM (batch operation)
            Console.WriteLine("7. Writing string \"Hello EEPROM!\" starting at address 0x0010...");
            var message = "Hello EEPROM!";
            var messageBytes = System.Text.Encoding.ASCII.GetBytes(message);
            var hexMessage = string.Join("", messageBytes.Select(b => $"{b:X2}"));
            
            PrintResponses(
                await WriteEepromByte(bridgeClient, eepromAddress, "0010", hexMessage),
                "Write String"
            );
            
            // Delay for EEPROM write cycle
            await Task.Delay(50);

            // 8. Read back the string
            Console.WriteLine("8. Reading back the string...");
            PrintResponses(
                await ReadEepromBytes(bridgeClient, eepromAddress, "0010", message.Length),
                "Read String"
            );

            // 9. Batch operations - write configuration data
            Console.WriteLine("9. Performing batch configuration write...");
            var configData = new[]
            {
                ("0020", "01"), // Config register 1
                ("0021", "02"), // Config register 2  
                ("0022", "03"), // Config register 3
                ("0023", "04"), // Config register 4
                ("0024", "FF"), // Status register
                ("0025", "AA"), // Test pattern 1
                ("0026", "55"), // Test pattern 2
                ("0027", "00")  // Reserved
            };

            foreach (var (address, value) in configData)
            {
                Console.WriteLine($"  Writing 0x{value} to address 0x{address}");
                await WriteEepromByte(bridgeClient, eepromAddress, address, value);
                await Task.Delay(5); // Short delay between writes
            }
            Console.WriteLine("Batch write completed!\n");

            // 10. Batch read - verify configuration data
            Console.WriteLine("10. Performing batch configuration read...");
            for (int i = 0; i < configData.Length; i++)
            {
                var address = configData[i].Item1;
                Console.WriteLine($"  Reading from address 0x{address}:");
                PrintResponses(
                    await ReadEepromBytes(bridgeClient, eepromAddress, address, 1),
                    $"Read Config {i + 1}"
                );
            }

            // 11. Sequential read of entire configuration block
            Console.WriteLine("11. Reading entire configuration block (8 bytes)...");
            PrintResponses(
                await ReadEepromBytes(bridgeClient, eepromAddress, "0020", 8),
                "Read Configuration Block"
            );

            // 12. Write and read test pattern
            Console.WriteLine("12. Writing test pattern...");
            var testPattern = "DEADBEEFCAFEBABE"; // 8 bytes of test data
            PrintResponses(
                await WriteEepromByte(bridgeClient, eepromAddress, "0030", testPattern),
                "Write Test Pattern"
            );
            
            await Task.Delay(20);

            Console.WriteLine("13. Reading back test pattern...");
            PrintResponses(
                await ReadEepromBytes(bridgeClient, eepromAddress, "0030", 8),
                "Read Test Pattern"
            );

            Console.WriteLine("=== I2C EEPROM Example Complete ===");
            Console.WriteLine("All operations completed successfully!");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during I2C operations: {ex.Message}");
            throw;
        }
    }
}
