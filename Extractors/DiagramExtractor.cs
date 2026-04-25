using System;
using System.IO;
using System.Xml;

namespace Fin.Extractors
{
    internal sealed class DiagramExtractor : IExtractor
    {
        public string Name => "diagrams";

        public void Run(ControlBuilderSession session)
        {
            var tree = new XmlDocument();
            tree.LoadXml(session.Client.GetProjectTree("", -1, true));

            var instances = tree.GetElementsByTagName("DiagramInstance");
            Log.Info($"Found {instances.Count} DiagramInstance node(s)");

            var instancesDoc = NewDoc("DiagramInstances", out var instancesRoot);
            var typesDoc     = NewDoc("Types",            out var typesRoot);
            var combinedDoc  = NewDoc("AllDiagramTypes",  out var combinedRoot);

            foreach (XmlNode node in instances)
            {
                var name = node.Attributes?["Name"]?.Value;
                var type = node.Attributes?["Type"]?.Value;

                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(name))
                {
                    var typeElement = typesDoc.CreateElement("Type");
                    typeElement.InnerText = type;
                    typesRoot.AppendChild(typeElement);

                    try
                    {
                        var typeXml = session.Client.GetDiagramType(type);
                        var typeDoc = new XmlDocument();
                        typeDoc.LoadXml(typeXml);

                        var typeFile = Path.Combine(Paths.Diagrams, $"{type.Replace('.', '_')}.xml");
                        typeDoc.Save(typeFile);

                        combinedRoot.AppendChild(combinedDoc.ImportNode(typeDoc.DocumentElement, true));
                        Log.Info($"Saved {name} ({type})");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to fetch diagram type '{type}'", ex);
                    }
                }

                instancesRoot.AppendChild(instancesDoc.ImportNode(node, true));
            }

            instancesDoc.Save(Path.Combine(Paths.Diagrams, "Diagrams.xml"));
            typesDoc.Save(Path.Combine(Paths.Diagrams, "Types.xml"));
            combinedDoc.Save(Path.Combine(Paths.Diagrams, "DiagramTypeTotal.xml"));

            ExtractFDCodeBlocks(combinedDoc);
        }

        private static void ExtractFDCodeBlocks(XmlDocument combined)
        {
            var blocks = combined.GetElementsByTagName("FDCodeBlock");
            var doc = NewDoc("FDCodeBlocks", out var root);
            foreach (XmlNode block in blocks)
                root.AppendChild(doc.ImportNode(block, true));

            var path = Path.Combine(Paths.Diagrams, "FDCodeBlock.xml");
            doc.Save(path);
            Log.Info($"Extracted {blocks.Count} FDCodeBlock(s) to {path}");
        }

        private static XmlDocument NewDoc(string rootName, out XmlElement root)
        {
            var doc = new XmlDocument();
            root = doc.CreateElement(rootName);
            doc.AppendChild(root);
            return doc;
        }
    }
}
