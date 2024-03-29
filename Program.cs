using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var examples = new Dictionary<string, Func<Task>>()
        {
            ["i3c_basics"]  = ExampleI3cBasics.Run,
            ["i3c_ibis"]    = ExampleI3cIbis.Run,
            // Add new examples here, like so:
            // ["example_key"] = ExampleClass.RunMethod,
        };

        if (args.Length > 0)
        {
            if (examples.TryGetValue(args[0].ToLower(), out var example))
            {
                await example();
            }
            else
            {
                Console.WriteLine($"Example not found. Available examples: {string.Join(", ", examples.Keys)}");
            }
        }
        else
        {
            Console.WriteLine("Please specify an example to run.");
            Console.WriteLine($"Available examples: {string.Join(", ", examples.Keys)}");
        }
    }
}
