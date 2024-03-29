using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class CommandResponse
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
    public event EventHandler<CommandResponse> OnResponseReceived;
    public event EventHandler<CommandResponse> OnNotificationReceived;
    private readonly Dictionary<
        string,
        (
            object commandObject,
            List<CommandResponse> responses,
            TaskCompletionSource<CommandResponse> tcs
        )
    > waitingList =
        new Dictionary<
            string,
            (
                object commandObject,
                List<CommandResponse> responses,
                TaskCompletionSource<CommandResponse> tcs
            )
        >();

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

    public async Task<List<CommandResponse>> SendCommand(
        string command,
        Dictionary<string, object> paramsDict = null
    )
    {
        transactionId++;
        var strTransactionId = transactionId.ToString();
        var commandObject = new
        {
            transaction_id = strTransactionId,
            command = command,
            @params = paramsDict ?? new Dictionary<string, object>() // Use an empty dictionary if paramsDict is null
        };

        var jsonString = JsonSerializer.Serialize(commandObject);
        bridgeProcess.StandardInput.WriteLine(jsonString);
        bridgeProcess.StandardInput.Flush();

        // ---
        // Command sequencing logic starts
        // ---

        // Initialize TaskCompletionSource for this command
        var tcs = new TaskCompletionSource<CommandResponse>();

        // Corrected tuple to match the expected types
        lock (waitingList)
        {
            waitingList.Add(
                strTransactionId,
                (commandObject: commandObject, responses: new List<CommandResponse>(), tcs: tcs)
            );
        }

        await WaitFor(strTransactionId);

        // ---
        // Command sequencing logic ends
        // ---

        List<CommandResponse> responses;
        lock (waitingList)
        {
            responses = waitingList[strTransactionId].responses;
            waitingList.Remove(strTransactionId); // Clean up
        }

        return responses;
    }

    private async Task<CommandResponse> WaitFor(string transactionId)
    {
        TaskCompletionSource<CommandResponse> tcs;
        lock (waitingList)
        {
            if (!waitingList.TryGetValue(transactionId, out var entry))
            {
                throw new InvalidOperationException($"Transaction ID {transactionId} not found.");
            }

            tcs = entry.tcs ?? new TaskCompletionSource<CommandResponse>();
            entry.tcs = tcs; // Ensure the TCS is stored back in case it was just created
        }

        // This will asynchronously wait until the TCS is set elsewhere
        return await tcs.Task;
    }

    private async Task ReadFromProcessStdOutAsync()
    {
        while (!bridgeProcess.StandardOutput.EndOfStream)
        {
            var line = await bridgeProcess.StandardOutput.ReadLineAsync();

            var response = JsonSerializer.Deserialize<CommandResponse>(line);

            if (response != null)
            {
                if (response.TransactionId.Equals("0"))
                {
                    OnNotificationReceived?.Invoke(this, response);
                }
                else
                {
                    OnResponseReceived?.Invoke(this, response);

                    // ---
                    // Command sequencing logic starts
                    // ---

                    lock (waitingList)
                    {
                        if (waitingList.TryGetValue(response.TransactionId, out var entry))
                        {
                            entry.responses.Add(response); // Add the response to the corresponding command entry.

                            if (!response.IsPromise)
                            {
                                entry.tcs?.TrySetResult(response); // Complete the TaskCompletionSource if is_promise is False
                            }
                        }
                    }

                    // ---
                    // Command sequencing logic ends
                    // ---
                }
            }
        }
    }

    public void Dispose()
    {
        SendCommand("exit"); // Ensure the process is signaled to exit before disposing.
        bridgeProcess?.Dispose();
    }
}
