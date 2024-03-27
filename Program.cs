using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
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
                // Write a command to the process's standard input
                string jsonString = "{\"command\":\"exit\"}";
                process.StandardInput.WriteLine(jsonString);
                process.StandardInput.Flush();
                process.StandardInput.Close(); // Signal the end of input

                // Read the output from the process
                string output = process.StandardOutput.ReadToEnd();

                Console.WriteLine("Output from the process:");
                Console.WriteLine(output);
            }
            else
            {
                Console.WriteLine("Failed to start process.");
            }
        }
    }
}
