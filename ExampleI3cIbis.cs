using System.Text.Json;

public class ExtendedBridgeClient : BridgeClient
{
    public async Task<List<CommandResponse>> SendI3cWriteUsingSubaddress(string address, string subaddress, string writeBuffer)
    {
        return await SendCommand(
            "i3c_write_using_subaddress",
            new Dictionary<string, object>
            {
                { "address", address },
                { "subaddress", subaddress },
                { "mode", "SDR" },
                { "pushPullClockFrequencyInMHz", "5" },
                { "openDrainClockFrequencyInKHz", "1250" },
                { "writeBuffer", writeBuffer },
            }
        );
    }
}

class ExampleI3cIbis
{
    public static async Task Run()
    {
        using var bridgeClient = new ExtendedBridgeClient();
        bridgeClient.OnNotificationReceived += (sender, notification) =>
        {
            Console.WriteLine("Received notification: ");
            Console.WriteLine($"Transaction ID: {notification.TransactionId}");
            Console.WriteLine($"Status: {notification.Status}");
            Console.WriteLine($"Type: {notification.Type}");
            Console.WriteLine($"Is Promise: {notification.IsPromise}");
            Console.WriteLine($"Data: {notification.Data}"); // Note: Data is a JsonElement.
            Console.WriteLine("----------------------------------------");
        };

        await bridgeClient.StartAsync();

        await bridgeClient.SendCommand("open");
        await bridgeClient.SendCommand("i3c_init_bus", new Dictionary<string, object> { { "busVoltageInV", "3.3" } });
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "76", "00");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "4E", "20");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "13", "05");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "16", "40");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "5F", "61");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "60", "0F00");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "50", "0E");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "76", "01");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "03", "38");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "7A", "02");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "7C", "1F");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "76", "04");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "4F", "04");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "76", "00");
        await bridgeClient.SendI3cWriteUsingSubaddress("08", "4E", "02");

        await Task.Delay(5000);
    }
}