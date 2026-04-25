using System.IO;
using System.Xml;

namespace Fin.Extractors
{
    internal sealed class ProjectTreeExtractor : IExtractor
    {
        public string Name => "tree";

        public void Run(ControlBuilderSession session)
        {
            var doc = new XmlDocument();
            doc.LoadXml(session.Client.GetProjectTree("", -1, true));

            var path = Path.Combine(Paths.ProjectTree, "ProjectTree.xml");
            doc.Save(path);
            Log.Info($"Saved project tree to {path}");
        }
    }
}
