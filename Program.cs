using System.Text.Json;

class Program
{
    private static void printResponses(List<CommandResponse> responses)
    {
        Console.WriteLine("Printing responses...");
        foreach (var response in responses)
        {
            Console.WriteLine($"Transaction ID: {response.TransactionId}");
            Console.WriteLine($"Status: {response.Status}");
            Console.WriteLine($"Type: {response.Type}");
            Console.WriteLine($"Is Promise: {response.IsPromise}");

            // Assuming the Data might be a simple value or complex object, you'll serialize it to a JSON string for printing.
            // You could also implement custom handling depending on the expected structure of Data.
            string responseData = JsonSerializer.Serialize(
                response.Data,
                new JsonSerializerOptions { WriteIndented = true }
            );
            Console.WriteLine($"Data: {responseData}");
            Console.WriteLine("----------------------------------------");
        }
    }

    static async Task Main(string[] args)
    {
        using var bridgeClient = new BridgeClient();
        // bridgeClient.OnResponseReceived += (sender, response) =>
        // {
        //     Console.WriteLine("Received response: " + response);
        // };

        await bridgeClient.StartAsync();

        printResponses(
            await bridgeClient.SendCommand(
                "open",
                new Dictionary<string, object> { { "address", "SupernovaSimulatedPort" } }
            )
        );
        printResponses(
            await bridgeClient.SendCommand(
                "i3c_init_bus",
                new Dictionary<string, object> { { "busVoltageInV", "3.3" } }
            )
        );
        printResponses(
            await bridgeClient.SendCommand(
                "get_usb_string",
                new Dictionary<string, object> { { "subCommand", "MANUFACTURER" } }
            )
        );
        printResponses(
            await bridgeClient.SendCommand(
                "get_usb_string",
                new Dictionary<string, object> { { "subCommand", "PRODUCT_NAME" } }
            )
        );
        printResponses(
            await bridgeClient.SendCommand(
                "get_usb_string",
                new Dictionary<string, object> { { "subCommand", "SERIAL_NUMBER" } }
            )
        );
        printResponses(
            await bridgeClient.SendCommand(
                "get_usb_string",
                new Dictionary<string, object> { { "subCommand", "HW_VERSION" } }
            )
        );
        printResponses(
            await bridgeClient.SendCommand(
                "get_usb_string",
                new Dictionary<string, object> { { "subCommand", "FW_VERSION" } }
            )
        );
        printResponses(
            await bridgeClient.SendCommand(
                "i3c_write_using_subaddress",
                new Dictionary<string, object>
                {
                    { "address", "08" },
                    { "subaddress", "0000" },
                    { "mode", "SDR" },
                    { "pushPullClockFrequencyInMHz", "5" },
                    { "openDrainClockFrequencyInKHz", "1250" },
                    { "writeBuffer", "04" },
                }
            )
        );
        printResponses(
            await bridgeClient.SendCommand(
                "i3c_read_using_subaddress",
                new Dictionary<string, object>
                {
                    { "address", "08" },
                    { "subaddress", "0000" },
                    { "mode", "SDR" },
                    { "pushPullClockFrequencyInMHz", "5" },
                    { "openDrainClockFrequencyInKHz", "1250" },
                    { "bytesToRead", "1" },
                }
            )
        );

        await Task.Delay(1000); // Wait before exit
    }
}
