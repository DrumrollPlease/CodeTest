﻿using System;
using System.IO;
using System.IO.MemoryMappedFiles;
//using VbFileSearcher;
using FileSearcherProject;

namespace CodeTest
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      // Run on sample.bin in the test_data subfolder. This sample is always copied to the bin folder.
      const string testFilePath = "test_data/sample.bin";

      using (var mmFile = MemoryMappedFile.CreateFromFile(testFilePath, FileMode.Open))
      {
        using (var accessor = mmFile.CreateViewAccessor())
        {
          var jpegFinder = new JpegFinder(new AccessorAdapter(accessor));
          var jpegs = jpegFinder.FindJpegs();

          foreach (var bounds in jpegs)
          {
            Console.WriteLine(bounds);
          }
        }
      }
    }
  }
}