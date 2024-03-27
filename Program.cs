using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "bridge",
            Arguments = "BinhoSupernova",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true, // This prevents the command window from showing up
        };

        using (var bridge = Process.Start(processStartInfo))
        {
            if (bridge != null)
            {
                var responses = new List<string>();

                // Asynchronously read the output from the Bridge
                var readTask = ReadFromProcessStdOutAsync(bridge, responses);

                // Write a command to the Bridge's standard input
                bridge.StandardInput.WriteLine("{\"command\":\"exit\"}");
                bridge.StandardInput.Flush();

                // Wait for the reading task to complete
                await readTask;

                bridge.StandardInput.Close();

                Console.WriteLine("Output from the bridge:");
                foreach (var response in responses)
                {
                    Console.WriteLine(response);
                }
            }
            else
            {
                Console.WriteLine("Failed to start bridge.");
            }
        }
    }

    static async Task ReadFromProcessStdOutAsync(Process bridge, List<string> responses)
    {
        while (true)
        {
            var line = await bridge.StandardOutput.ReadLineAsync();
            if (line != null)
            {
                responses.Add(line);
                if (line.Contains("exit"))
                {
                    break;
                }
            }
        }
    }
}
