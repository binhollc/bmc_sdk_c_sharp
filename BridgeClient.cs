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
    private readonly Dictionary<string, (object commandObject, List<BaseResponse> responses, TaskCompletionSource<BaseResponse> tcs)> waitingList
        = new Dictionary<string, (object commandObject, List<BaseResponse> responses, TaskCompletionSource<BaseResponse> tcs)>();

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

    public async Task<List<BaseResponse>> SendCommand(string command, Dictionary<string, object> paramsDict = null)
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
        var tcs = new TaskCompletionSource<BaseResponse>();

        // Corrected tuple to match the expected types
        lock (waitingList)
        {
            waitingList.Add(strTransactionId, (commandObject: commandObject, responses: new List<BaseResponse>(), tcs: tcs));
        }

        // ---
        // Command sequencing logic ends
        // ---

        await WaitFor(strTransactionId);

        List<BaseResponse> responses;
        lock (waitingList)
        {
            responses = waitingList[strTransactionId].responses;
            waitingList.Remove(strTransactionId); // Clean up
        }

        return responses;
    }

    private async Task<BaseResponse> WaitFor(string transactionId)
    {
        TaskCompletionSource<BaseResponse> tcs;
        lock (waitingList)
        {
            if (!waitingList.TryGetValue(transactionId, out var entry))
            {
                throw new InvalidOperationException($"Transaction ID {transactionId} not found.");
            }

            tcs = entry.tcs ?? new TaskCompletionSource<BaseResponse>();
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
                      entry.responses.Add(response); // Add the response to the corresponding command entry.

                      if (!response.IsPromise)
                      {
                          entry.tcs?.TrySetResult(response); // Complete the TaskCompletionSource if is_promise is False
                      }
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
        SendCommand("exit"); // Ensure the process is signaled to exit before disposing.
        bridgeProcess?.Dispose();
    }
}
