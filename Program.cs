using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Fin.Extractors;

namespace Fin
{
    internal static class Program
    {
        private static readonly Dictionary<string, Func<IExtractor>> Available =
            new Dictionary<string, Func<IExtractor>>(StringComparer.OrdinalIgnoreCase)
            {
                { "tree",         () => new ProjectTreeExtractor() },
                { "diagrams",     () => new DiagramExtractor() },
                { "hardware",     () => new HardwareExtractor() },
                { "applications", () => new ApplicationExtractor() },
            };

        private static int Main(string[] args)
        {
            var selected = Resolve(args);
            if (selected == null)
            {
                PrintUsage();
                return 1;
            }

            Log.Info($"Output directory: {Paths.Root}");

            try
            {
                using (var session = new ControlBuilderSession())
                {
                    foreach (var extractor in selected)
                    {
                        Log.Info($"=== {extractor.Name} ===");
                        try
                        {
                            extractor.Run(session);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Extractor '{extractor.Name}' failed", ex);
                        }
                    }
                }
            }
            catch (COMException ex)
            {
                Log.Error("Could not initialize Control Builder. Is it installed and is a project open?", ex);
                return 2;
            }

            Log.Info("Done.");
            return 0;
        }

        private static List<IExtractor> Resolve(string[] args)
        {
            if (args.Length == 0 || args.Any(a => a.Equals("all", StringComparison.OrdinalIgnoreCase)))
                return Available.Values.Select(f => f()).ToList();

            var result = new List<IExtractor>();
            foreach (var arg in args)
            {
                if (!Available.TryGetValue(arg, out var factory))
                {
                    Console.Error.WriteLine($"Unknown extractor: {arg}");
                    return null;
                }
                result.Add(factory());
            }
            return result;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: Fin [extractor ...]");
            Console.WriteLine();
            Console.WriteLine("Extractors:");
            foreach (var name in Available.Keys)
                Console.WriteLine($"  {name}");
            Console.WriteLine("  all  (default when no args — runs every extractor)");
        }
    }
}
