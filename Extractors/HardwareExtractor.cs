using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Fin.Extractors
{
    internal sealed class HardwareExtractor : IExtractor
    {
        private static readonly XNamespace Ns = "CBOpenIFSchema3_0";

        public string Name => "hardware";

        public void Run(ControlBuilderSession session)
        {
            var tree = XDocument.Parse(session.Client.GetProjectTree("", 1, true));

            var treePath = Path.Combine(Paths.Hardware, "HardwareTree.xml");
            tree.Save(treePath);
            Log.Info($"Saved hardware tree to {treePath}");

            var controllers = tree.Descendants(Ns + "Controller")
                                  .Select(c => (string)c.Attribute("Name"))
                                  .Where(n => !string.IsNullOrEmpty(n))
                                  .ToList();

            Log.Info($"Found {controllers.Count} controller(s)");

            foreach (var name in controllers)
            {
                try
                {
                    var element = XElement.Parse(session.Client.GetHardwareUnit(name, true));
                    var path = Path.Combine(Paths.Hardware, $"{name}.xml");
                    element.Save(path);
                    Log.Info($"Saved hardware data for {name}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to fetch hardware unit '{name}'", ex);
                }
            }
        }
    }
}
