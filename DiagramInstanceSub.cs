using CONTROLBUILDERLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace Fin
{
    public class DiagramInstance
    {
        public void GetAllDiagrams()
        {
            CBOpenIF cb = null;
            try
            {
                // Initialize CONTROLBUILDERLib
                cb = new CBOpenIF();
                // Enter Strings for variable holding.
                string projectTree = cb.GetProjectTree("", -1, true);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(projectTree);

                // Create xmlContainer directory if it doesn't exist
                string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "xmlContainer");
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // Save the project tree to an XML file
                string projectTreeFilePath = Path.Combine(outputDirectory, "ProjectTree.xml");
                xmlDoc.Save(projectTreeFilePath);
                Console.WriteLine($"Saved project tree to: {projectTreeFilePath}");

                // Find all <DiagramInstance> nodes
                XmlNodeList diagramInstanceNodes = xmlDoc.GetElementsByTagName("DiagramInstance");

                // Create the new XMLDoc that will hold all results.
                XmlDocument outputDoc = new XmlDocument();
                XmlElement root = outputDoc.CreateElement("DiagramInstances");
                outputDoc.AppendChild(root);

                // Create a new XMLDoc for types only
                XmlDocument typesDoc = new XmlDocument();
                XmlElement typesRoot = typesDoc.CreateElement("Types");
                typesDoc.AppendChild(typesRoot);

                // Create a new XMLDoc to combine all diagram type data
                XmlDocument combinedTypesDoc = new XmlDocument();
                XmlElement combinedRoot = combinedTypesDoc.CreateElement("AllDiagramTypes");
                combinedTypesDoc.AppendChild(combinedRoot);

                // Loop through each <DiagramInstance> node
                foreach (XmlNode node in diagramInstanceNodes)
                {
                    // Get the Name and Type attributes
                    XmlAttribute typeAttribute = node.Attributes["Type"];
                    XmlAttribute nameAttribute = node.Attributes["Name"];

                    if (typeAttribute != null && nameAttribute != null)
                    {
                        // Print out the Name and Type attribute values
                        Console.WriteLine($"DiagramInstance Name: {nameAttribute.Value}, Type: {typeAttribute.Value}");

                        // Create an element to store the Type value
                        XmlElement typeElement = typesDoc.CreateElement("Type");
                        typeElement.InnerText = typeAttribute.Value; // Set the Type value
                        typesRoot.AppendChild(typeElement); // Add to the types document

                        try
                        {
                            // Call GetDiagramType for each Type
                            string diagramTypeData = cb.GetDiagramType(typeAttribute.Value); // Fetch the diagram type

                            // Save this data to its own XML file in xmlContainer
                            string fileName = $"{typeAttribute.Value.Replace('.', '_')}.xml"; // Create a filename
                            string filePath = Path.Combine(outputDirectory, fileName); // Path to xmlContainer
                            XmlDocument diagramDoc = new XmlDocument();
                            diagramDoc.LoadXml(diagramTypeData); // Load the data into a new XmlDocument
                            diagramDoc.Save(filePath); // Save the diagram type XML to file

                            Console.WriteLine($"Saved type data to: {filePath}");

                            // Import the diagram type data into the combined XML document
                            XmlNode importedCombinedNode = combinedTypesDoc.ImportNode(diagramDoc.DocumentElement, true);
                            combinedRoot.AppendChild(importedCombinedNode); // Append to combined types document
                        }
                        catch (Exception ex)
                        {
                            // Log the error and continue with the next iteration
                            Console.WriteLine($"Error fetching diagram type for '{typeAttribute.Value}': {ex.Message}");
                        }
                    }

                    // Import the original node to the output document
                    XmlNode importedNode = outputDoc.ImportNode(node, true);
                    root.AppendChild(importedNode);
                }

                // Save the new XML document for all instances to a file in xmlContainer
                string outputFilePath = Path.Combine(outputDirectory, "Diagrams.xml");
                outputDoc.Save(outputFilePath);
                Console.WriteLine($"Extracted {diagramInstanceNodes.Count} <DiagramInstance> nodes to {outputFilePath}");

                // Save the types only to a separate file in xmlContainer
                string typesOutputFilePath = Path.Combine(outputDirectory, "Types.xml");
                typesDoc.Save(typesOutputFilePath);
                Console.WriteLine($"Extracted <Type> attributes to {typesOutputFilePath}");

                // Save the combined types XML document in xmlContainer
                string combinedOutputFilePath = Path.Combine(outputDirectory, "DiagramTypeTotal.xml");
                combinedTypesDoc.Save(combinedOutputFilePath);
                Console.WriteLine($"Combined diagrams saved to: {combinedOutputFilePath}");
            }
            catch (COMException ex)
            {
                Console.WriteLine($"COM Error: {ex.Message}");
                Console.WriteLine($"Error Code: 0x{ex.ErrorCode:X}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Clean up COM object
                if (cb != null)
                {
                    Marshal.ReleaseComObject(cb);
                    cb = null;
                }

                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
        public void ExtractFDCodeBlocksFromDiagramsFile()
        {
            try
            {
                string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "xmlContainer");
                string inputFilePath = Path.Combine(outputDirectory, "DiagramTypeTotal.xml");

                // Load the Diagrams.xml file
                XmlDocument outputDoc = new XmlDocument();
                outputDoc.Load(inputFilePath);

                // Find all <FDCodeBlock> elements
                XmlNodeList fdCodeBlockNodes = outputDoc.GetElementsByTagName("FDCodeBlock");

                // Create new XML document to store FDCodeBlock elements
                XmlDocument fdCodeBlockDoc = new XmlDocument();
                XmlElement fdCodeBlockRoot = fdCodeBlockDoc.CreateElement("FDCodeBlocks");
                fdCodeBlockDoc.AppendChild(fdCodeBlockRoot);

                foreach (XmlNode fdNode in fdCodeBlockNodes)
                {
                    XmlNode importedFDNode = fdCodeBlockDoc.ImportNode(fdNode, true);
                    fdCodeBlockRoot.AppendChild(importedFDNode);
                }

                // Save the FDCodeBlock nodes to a separate XML file
                string fdCodeBlockFilePath = Path.Combine(outputDirectory, "FDCodeBlock.xml");
                fdCodeBlockDoc.Save(fdCodeBlockFilePath);
                Console.WriteLine($"Saved all <FDCodeBlock> elements to: {fdCodeBlockFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting <FDCodeBlock> elements: {ex.Message}");
            }
        }
    }
}
