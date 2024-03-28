using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class BridgeClient : IDisposable
{
    private Process bridgeProcess;
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

    public void SendCommand(string command)
    {
        bridgeProcess.StandardInput.WriteLine(command);
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