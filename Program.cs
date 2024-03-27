using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Define the process using ProcessStartInfo
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "bridge",
            Arguments = "BinhoSupernova",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true, // This prevents the command window from showing up
        };

        // Start the process
        using (var process = Process.Start(processStartInfo))
        {
            if (process != null)
            {
                // Asynchronously write a command to the process's standard input
                await WriteToProcessStdInAsync(process.StandardInput, "{\"command\":\"exit\"}");

                // Asynchronously read the output from the process
                string output = await ReadFromProcessStdOutAsync(process);
                Console.WriteLine("Output from the process:");
                Console.WriteLine(output);
            }
            else
            {
                Console.WriteLine("Failed to start process.");
            }
        }
    }

    static async Task WriteToProcessStdInAsync(StreamWriter stdin, string content)
    {
        await stdin.WriteLineAsync(content);
        await stdin.FlushAsync();
        stdin.Close(); // Signal the end of input
    }

    static async Task<string> ReadFromProcessStdOutAsync(Process process)
    {
        // Read the output from the process asynchronously
        string output = await process.StandardOutput.ReadToEndAsync();
        return output;
    }
}
