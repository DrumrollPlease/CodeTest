using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using VbFileSearcher;

namespace CodeTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Run on sample.bin located in the CodeTest project folder in the test_data subfolder.
            var projectFolder = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            var testFilePath = new DirectoryInfo(Path.Combine(projectFolder, "test_data/sample.bin")).FullName;

            using (var mmFile = MemoryMappedFile.CreateFromFile(testFilePath, FileMode.Open))
            using (var accessor = mmFile.CreateViewAccessor())
            {
                var jpegFinder = new JpegFinder(new AccessorAdapter(accessor));
                foreach (var bounds in jpegFinder.FindJpegs()) Console.WriteLine(bounds);
            }
        }
    }
}