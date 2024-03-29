using System.Text.Json;

class ExampleI3cBasics
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
            Console.WriteLine($"Data: {response.Data}"); // Note: Data is a JsonElement.
            Console.WriteLine("----------------------------------------");
        }
    }

    public static async Task Run()
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
    }
}
