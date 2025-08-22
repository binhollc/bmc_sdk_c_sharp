using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

public class ExampleI3cCcc
{
    public static async Task Run()
    {
        using var bridgeClient = new BridgeClient("BinhoSupernova");
        await bridgeClient.StartAsync();

        // First, open the connection and initialize the I3C bus
        await bridgeClient.SendCommand(
            "open",
            new Dictionary<string, object> { { "address", "SupernovaSimulatedPort" } }
        );
        
        await bridgeClient.SendCommand(
            "i3c_init_bus",
            new Dictionary<string, object> { { "busVoltageInV", "3.3" } }
        );

        // Send the GETPID command
        var responses = await bridgeClient.SendCommand(
            "i3c_ccc_getpid",
            new Dictionary<string, object>
            {
                { "address", "08" },
                { "pushPullClockFrequencyInMHz", "5" },
                { "openDrainClockFrequencyInKHz", "2500" }
            }
        );

        Console.WriteLine($"Received {responses.Count} responses");

        // The actual result is typically in the last response
        var response = responses.Last();

        // Extract payload as JsonElement
        var data = (JsonElement)response.Data;
        var result = data.GetProperty("result");
        var payload = result.GetProperty("payload")
                           .EnumerateArray()
                           .Select(x => Convert.ToByte(x.GetString(), 16))
                           .ToArray();

        // PID as byte array
        Console.WriteLine($"PID = [{string.Join(", ", payload.Select(b => $"0x{b:X2}"))}]");
        Console.WriteLine($"PID bytes: [{string.Join(", ", payload)}]");
    }
}