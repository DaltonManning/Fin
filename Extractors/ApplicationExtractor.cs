using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Fin.Extractors
{
    internal sealed class ApplicationExtractor : IExtractor
    {
        private static readonly XNamespace Ns = "CBOpenIFSchema3_0";

        public string Name => "applications";

        public void Run(ControlBuilderSession session)
        {
            var doc = XDocument.Parse(session.Client.GetProjectTree("Applications", 1, true));

            var treePath = Path.Combine(Paths.Applications, "Applications.xml");
            doc.Save(treePath);
            Log.Info($"Saved applications tree to {treePath}");

            var names = doc.Descendants(Ns + "Application")
                           .Select(a => (string)a.Attribute("Name"))
                           .Where(n => !string.IsNullOrEmpty(n))
                           .ToList();

            var namesPath = Path.Combine(Paths.Applications, "application_names.txt");
            File.WriteAllLines(namesPath, names);
            Log.Info($"Saved {names.Count} application name(s) to {namesPath}");
        }
    }
}
