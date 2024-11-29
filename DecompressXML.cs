using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;


namespace Fin
{
    public class XmlDecompressor
    {
        private string inputDirectory;
        private string outputDirectory;
        private int totalFiles;
        private int successfulDecompressions;
        private int failedDecompressions;

        public XmlDecompressor(string inputDirectory, string outputDirectory)
        {
            this.inputDirectory = inputDirectory;
            this.outputDirectory = outputDirectory;
            totalFiles = 0;
            successfulDecompressions = 0;
            failedDecompressions = 0;
        }

        public void DecompressAll()
        {
            try
            {
                // Get all files in the specified directory
                var files = Directory.GetFiles(inputDirectory);
                totalFiles = files.Length;

                foreach (var filePath in files)
                {
                    string originalFileName = Path.GetFileName(filePath); // Get the original file name with extension
                    string outputFilePath = CreateUniqueFilePath(originalFileName, outputDirectory); // Create a unique output file path

                    // Attempt to decompress the file
                    try
                    {
                        Decompress(filePath, outputFilePath);
                        Console.WriteLine($"Successfully decompressed: {filePath} to {outputFilePath}");
                        successfulDecompressions++;
                    }
                    catch (InvalidDataException)
                    {
                        // If it fails due to invalid data, assume it's not a gzip file and copy it directly
                        File.Copy(filePath, outputFilePath, true); // Use filePath correctly
                        Console.WriteLine($"Copied non-decompressable file: {filePath} to {outputFilePath}");
                        successfulDecompressions++; // Count as a successful copy operation
                    }
                    catch (Exception ex)
                    {
                        // Handle any other unrelated exceptions
                        Console.WriteLine($"Failed to decompress: {filePath}. Error: {ex.Message}");
                        failedDecompressions++;
                    }
                }

                // Print the summary of the process
                Console.WriteLine($"\nSummary of Decompression Process:");
                Console.WriteLine($"Total Files Attempted: {totalFiles}");
                Console.WriteLine($"Successful Decompressions: {successfulDecompressions}");
                Console.WriteLine($"Failed Decompressions: {failedDecompressions}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private string CreateUniqueFilePath(string originalFileName, string outputDirectory)
        {
            string outputFilePath = Path.Combine(outputDirectory, originalFileName); // Use original file name with extension
            int counter = 1;

            // Check if the file already exists and create a unique name if it does
            while (File.Exists(outputFilePath))
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
                string fileExtension = Path.GetExtension(originalFileName);
                outputFilePath = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}_{counter}{fileExtension}");
                counter++;
            }

            return outputFilePath;
        }

        private void Decompress(string compressedFilePath, string outputFilePath)
        {
            using (FileStream compressedFileStream = new FileStream(compressedFilePath, FileMode.Open))
            {
                // Try to create a GZipStream for decompression
                using (GZipStream decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create))
                {
                    decompressionStream.CopyTo(outputFileStream);
                }
            }
        }
    }
}
