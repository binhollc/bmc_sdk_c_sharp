using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

public class BaseResponse
{
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("is_promise")]
    public bool IsPromise { get; set; }

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}

public class BridgeClient : IDisposable
{
    private Process bridgeProcess;
    private int transactionId = 0;
    public event EventHandler<string> OnResponseReceived;
    private readonly Dictionary<string, (object commandObject, List<string> responses)> waitingList
        = new Dictionary<string, (object commandObject, List<string> responses)>();

    public BridgeClient()
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "bridge",
            Arguments = "BinhoSupernova",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        bridgeProcess = new Process { StartInfo = processStartInfo };
    }

    public async Task StartAsync()
    {
        if (bridgeProcess.Start())
        {
            _ = Task.Run(() => ReadFromProcessStdOutAsync());
        }
        else
        {
            throw new InvalidOperationException("Failed to start the bridge process.");
        }
    }

    public void SendCommand(string command, Dictionary<string, object> paramsDict = null)
    {
        var strTransactionId = (transactionId++).ToString();
        var commandObject = new
        {
            transaction_id = strTransactionId,
            command = command,
            @params = paramsDict ?? new Dictionary<string, object>() // Use an empty dictionary if paramsDict is null
        };

        // ---
        // Command sequencing logic starts
        // ---

        // Add the command object to the waiting list along with an empty list for responses.
        lock (waitingList)
        {
            waitingList.Add(strTransactionId, (commandObject, new List<string>()));
        }

        // ---
        // Command sequencing logic ends
        // ---

        var jsonString = JsonSerializer.Serialize(commandObject);
        bridgeProcess.StandardInput.WriteLine(jsonString);
        bridgeProcess.StandardInput.Flush();
    }

    private async Task ReadFromProcessStdOutAsync()
    {
        while (!bridgeProcess.StandardOutput.EndOfStream)
        {
            var line = await bridgeProcess.StandardOutput.ReadLineAsync();
            OnResponseReceived?.Invoke(this, line);

            // ---
            // Command sequencing logic starts
            // ---

            var response = JsonSerializer.Deserialize<BaseResponse>(line);

            if (response != null && !response.TransactionId.Equals("0"))
            {
              lock (waitingList)
              {
                  if (waitingList.TryGetValue(response.TransactionId, out var entry))
                  {
                      entry.responses.Add(line); // Add the response to the corresponding command entry.
                  }
              }
            }

            // ---
            // Command sequencing logic ends
            // ---
        }
    }

    public void Dispose()
    {
        SendCommand("{\"command\":\"exit\"}"); // Ensure the process is signaled to exit before disposing.
        bridgeProcess?.Dispose();
    }
}
