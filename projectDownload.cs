using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using CONTROLBUILDERLib;

namespace Fin
{
    public class projectDownload
    {
        public void projectTree()
        {
            CBOpenIF cb = null;
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Project");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Console.WriteLine($"Creating directory at {folderPath}");
            }

            try
            {
                Console.WriteLine("Initializing CONTROLBUILDERLib...");
                cb = new CBOpenIF();

                // Retrieve project tree and gather individual controller data
                string projectTree = cb.GetProjectTree("", -1, false);
                Console.WriteLine("Project tree retrieved successfully.");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(projectTree);
                string projectTreePath = Path.Combine(folderPath, "ProjectTree.xml");
                xmlDoc.Save(projectTreePath);
                Console.WriteLine($"Saved project tree to: {projectTreePath}");
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
    }
    }

