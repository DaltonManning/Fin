using CBOpenIFHelper;
using System;
using System.IO;

namespace Fin
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a new instance of the project downloader
            DiagramInstance getXML = new DiagramInstance();
             try
            {
               getXML.GetAllDiagrams();
            }
            catch (Exception ex)
             {
                Console.WriteLine(ex.Message);
            }
            getXML.ExtractFDCodeBlocksFromDiagramsFile();


            // Create an instance of HWExtractor
            //HWExtractor hwextractor = new HWExtractor();

            // Call GetAllHardware 
            //hwextractor.GetAllHardware();
            //Console.WriteLine("Process complete. Press any key to exit.");
            Console.ReadKey();






            // Specify the path to the directory containing compressed XML files
            //string inputDirectory = @"C:\Users\gl8152\OneDrive - DuPont\Desktop\Tasks Weekly\Tasks-11-19-2024\ABB Industrial IT Data\Engineer IT Data\Control Builder M Professional\Projects\CopolyProject";

            // Optionally, specify an output directory; here it creates a "DecompressedFiles" folder in the input directory
            //string outputDirectory = Path.Combine(inputDirectory, "DecompressedFiles");

            // Create the output directory if it doesn't exist
            //Directory.CreateDirectory(outputDirectory);

            // Create an instance of XmlDecompressor for the specified directory and call the DecompressAll method
            //XmlDecompressor decompressor = new XmlDecompressor(inputDirectory, outputDirectory);
            //decompressor.DecompressAll();
        }
    }
}