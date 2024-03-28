using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;

public class BridgeClient : IDisposable
{
    private Process bridgeProcess;
    private int transactionId = 0;
    public event EventHandler<string> OnResponseReceived;

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
        transactionId++;
        var commandObject = new
        {
            transaction_id = transactionId.ToString(),
            command = command,
            @params = paramsDict ?? new Dictionary<string, object>() // Use an empty dictionary if paramsDict is null
        };

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
        }
    }

    public void Dispose()
    {
        SendCommand("{\"command\":\"exit\"}"); // Ensure the process is signaled to exit before disposing.
        bridgeProcess?.Dispose();
    }
}
