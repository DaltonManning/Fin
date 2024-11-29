using System;
using System.IO;
using CONTROLBUILDERLib;
using System.Runtime.InteropServices;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Fin
{
    public class HWExtractor
    {
        private string _logFilePath;

        public HWExtractor()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"HWExtractorLog_{timestamp}.log");
            LogInfo("Log file created: " + _logFilePath);
        }

        public void GetAllHardware()
        {
            CBOpenIF cb = null;
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xmlControllers");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                LogInfo($"Creating directory at {folderPath}");
            }

            try
            {
                LogInfo("Initializing CONTROLBUILDERLib...");
                cb = new CBOpenIF();
                
                // Retrieve project tree and gather individual controller data
                string projectTree = cb.GetProjectTree("", 1, true);
                LogInfo("Project tree retrieved successfully.");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(projectTree);
                string projectTreePath = Path.Combine(folderPath, "ContProjectTree.xml");
                xmlDoc.Save(projectTreePath);
                LogInfo($"Saved project tree to: {projectTreePath}");

                List<string> controllerNames = GetControllerNames(projectTreePath);
                LogInfo($"Found {controllerNames.Count} controller(s).");

                foreach (var name in controllerNames)
                {
                    LogInfo($"Processing hardware unit for controller: {name}");
                    var hardwareData = cb.GetHardwareUnit(name, true); // Retrieve XML data for the controller
                    XElement controllerElement = XElement.Parse(hardwareData);

                    // Save each controller's data into its own XML file
                    string controllerFilePath = Path.Combine(folderPath, $"{name}.xml");
                    controllerElement.Save(controllerFilePath);
                    LogInfo($"Saved hardware data for {name} to: {controllerFilePath}");
                }
            }
            catch (COMException ex)
            {
                LogError($"COM Error: {ex.Message}");
                LogError($"Error Code: 0x{ex.ErrorCode:X}");
            }
            catch (Exception ex)
            {
                LogError($"General Error: {ex.Message}");
                LogError($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                if (cb != null)
                {
                    Marshal.ReleaseComObject(cb);
                    cb = null;
                    LogInfo("Released COM object.");
                }
                LogInfo("\nProcess complete. Check log file for details.");
            }
        }

        private List<string> GetControllerNames(string xmlPath)
        {
            List<string> controllerNames = new List<string>();
            try
            {
                LogInfo($"Loading XML from path: {xmlPath}");

                XDocument xdoc = XDocument.Load(xmlPath);
                LogInfo("XML loaded successfully.");

                XNamespace ns = "CBOpenIFSchema3_0"; // Adjust according to your XML
                controllerNames = xdoc.Descendants(ns + "Controller")
                                       .Select(c => (string)c.Attribute("Name"))
                                       .Where(name => !string.IsNullOrEmpty(name))
                                       .ToList();

                LogInfo($"Extracted {controllerNames.Count} controller name(s).");
                foreach (var name in controllerNames)
                {
                    LogInfo($"Controller name: {name}");
                }
            }
            catch (FileNotFoundException ex)
            {
                LogError($"File not found: {ex.Message}");
            }
            catch (XmlException ex)
            {
                LogError($"XML Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogError($"Error while loading XML: {ex.Message}");
                LogError($"Stack Trace: {ex.StackTrace}");
            }

            return controllerNames;
        }

        private void LogInfo(string message) => WriteLog("INFO: " + message);
        private void LogError(string message) => WriteLog("ERROR: " + message);
        private void WriteLog(string message) => File.AppendAllText(_logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
    }
}

