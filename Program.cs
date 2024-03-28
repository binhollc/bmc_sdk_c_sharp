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

        bridgeClient.SendCommand("{\"command\":\"open\",\"params\":{\"address\":\"SupernovaSimulatedPort\"}}");
        bridgeClient.SendCommand("{\"command\":\"i3c_init_bus\",\"params\":{\"busVoltageInV\":\"3.3\"}}");
        bridgeClient.SendCommand("{\"command\":\"get_usb_string\",\"params\":{\"subCommand\":\"MANUFACTURER\"}}");
        bridgeClient.SendCommand("{\"command\":\"get_usb_string\",\"params\":{\"subCommand\":\"PRODUCT_NAME\"}}");
        bridgeClient.SendCommand("{\"command\":\"get_usb_string\",\"params\":{\"subCommand\":\"SERIAL_NUMBER\"}}");
        bridgeClient.SendCommand("{\"command\":\"get_usb_string\",\"params\":{\"subCommand\":\"HW_VERSION\"}}");
        bridgeClient.SendCommand("{\"command\":\"get_usb_string\",\"params\":{\"subCommand\":\"FW_VERSION\"}}");
        bridgeClient.SendCommand("{\"command\":\"i3c_write_using_subaddress\",\"params\":{\"address\":\"7E\",\"subaddress\":\"0000\",\"mode\":\"SDR\",\"pushPullClockFrequencyInMHz\":\"5\",\"openDrainClockFrequencyInKHz\":\"1250\",\"writeBuffer\":\"04\"}}");

        await Task.Delay(1000); // Wait before exit
    }
}
