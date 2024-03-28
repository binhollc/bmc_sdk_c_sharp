class Program
{
    static async Task Main(string[] args)
    {
        using var bridgeClient = new BridgeClient();
        bridgeClient.OnResponseReceived += (sender, response) =>
        {
            Console.WriteLine("Received response: " + response);
        };

        await bridgeClient.StartAsync();

        bridgeClient.SendCommand("open", new Dictionary<string, object> { {"address", "SupernovaSimulatedPort"} });
        bridgeClient.SendCommand("i3c_init_bus", new Dictionary<string, object> { {"busVoltageInV", "3.3"} });
        bridgeClient.SendCommand("get_usb_string", new Dictionary<string, object> { {"subCommand", "MANUFACTURER"} });
        bridgeClient.SendCommand("get_usb_string", new Dictionary<string, object> { {"subCommand", "PRODUCT_NAME"} });
        bridgeClient.SendCommand("get_usb_string", new Dictionary<string, object> { {"subCommand", "SERIAL_NUMBER"} });
        bridgeClient.SendCommand("get_usb_string", new Dictionary<string, object> { {"subCommand", "HW_VERSION"} });
        bridgeClient.SendCommand("get_usb_string", new Dictionary<string, object> { {"subCommand", "FW_VERSION"} });
        bridgeClient.SendCommand("i3c_write_using_subaddress", new Dictionary<string, object> {
          {"address", "7E"},
          {"subaddress", "0000"},
          {"mode", "SDR"},
          {"pushPullClockFrequencyInMHz", "5"},
          {"openDrainClockFrequencyInKHz", "1250"},
          {"writeBuffer", "04"},
        });

        await Task.Delay(1000); // Wait before exit
    }
}
