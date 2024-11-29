using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CONTROLBUILDERLib;

namespace Fin
{
    public class AlarmDatabase
    {
        //getapplicationcontrolmodules
        public void getCM()
        {
            CBOpenIF cb = null;
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmDatabase");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Console.WriteLine($"Creating directory at {folderPath}");
            }

            try
            {
                Console.WriteLine("Initializing CONTROLBUILDERLib...");
                cb = new CBOpenIF();
                string whatareYouLookingfor = "Applications";
                // Retrieve project tree and gather individual controller data
                string projectTree = cb.GetProjectTree(whatareYouLookingfor, 1, true);
                // Specify the file path where you want to save the XML
                // Save the XML string directly to the file
                File.WriteAllText(Path.Combine(folderPath, whatareYouLookingfor), projectTree);
                Console.WriteLine($"XML saved to {whatareYouLookingfor}");
                Console.WriteLine("Project tree retrieved successfully.");
                ParseAndPrintApplications(projectTree);
            }
            catch (COMException ex)
            {
                Console.WriteLine($"COM Error: {ex.Message}");
                Console.WriteLine($"Error Code: 0x{ex.ErrorCode:X}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                if (cb != null)
                {
                    Marshal.ReleaseComObject(cb);
                    cb = null;
                    Console.WriteLine("Released COM object.");
                }

                Console.WriteLine("\nProcess complete. Check log file for details.");
            }
        }
        static void ParseAndPrintApplications(string xmlData)
        {
            List<string> applicationNames = new List<string>(); // List to store application names

            try
            {
                // Load XML data
                XDocument xDoc = XDocument.Parse(xmlData);
                // Define Namespace
                XNamespace ns = "CBOpenIFSchema3_0";
                // Query for all application elements 
                var appls = xDoc.Descendants(ns + "Application");

                foreach (var app in appls)
                {
                    // Extract attributes from each application 
                    string appName = app.Attribute("Name")?.Value;

                    // Add to the application names list
                    if (!string.IsNullOrEmpty(appName))
                    {
                        applicationNames.Add(appName); // Add the name to the list
                    }

                    // Write the information to the console
                    Console.WriteLine($"Application Name: {appName}");
                }

                // Save application names to a text file
                string filePath = "application_names.txt"; // Specify the file path
                SaveApplicationNamesToFile(applicationNames, filePath);
                Console.WriteLine($"\nApplication names saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing XML: " + ex.Message);
            }
        }

        static void SaveApplicationNamesToFile(List<string> applicationNames, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var appName in applicationNames)
                    {
                        writer.WriteLine(appName); // Write each application name to the file
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to file: " + ex.Message);
            }
        }
    }
    }
