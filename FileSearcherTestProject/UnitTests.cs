using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileSearcherProject;
using System.IO.MemoryMappedFiles;
using System.IO;
using System;

namespace UnitTests
{

  [TestClass]
  public class UnitTests
  {
    private const string _testFilePath = @"../../../../CodeTest/test_data/sample.bin";
    private MemoryMappedFile _mmFile = null;
    private MemoryMappedViewAccessor _accessor = null;

    /// <summary>
    /// Tests the validity of the file being examined
    /// </summary>
    [TestMethod]
    public void TestFileValidity()
    {
      try
      {
        _mmFile = MemoryMappedFile.CreateFromFile(_testFilePath, FileMode.Open);
      }
      catch { }
      finally
      {
        Assert.IsNotNull(_mmFile, "memory file is null");
      }
    }

    /// <summary>
    /// Create and test the accessor
    /// </summary>
    [TestMethod]
    public void TestAccessor()
    {
      try
      {
        _accessor = _mmFile.CreateViewAccessor();
      }
      catch { }
      finally
      {
        Assert.IsNotNull(_accessor, "accessor is null");
      }
    }


    /// <summary>
    /// Tests the jpeg iterator
    /// </summary>
    [TestMethod]
    public void TestJpegIterator()
    {
      TestFileValidity();
      TestAccessor();

      var jpegFinder = new JpegFinder(new AccessorAdapter(_accessor));

      Assert.IsNotNull(jpegFinder.FindJpegs(), "no jpegs not found");
    }

    /// <summary>
    /// Sample to test a private method
    /// </summary>
    [TestMethod]
    public void TestPrivateFindSoi()
    {

#if DEBUG

      TestFileValidity();
      TestAccessor();

      var jpegFinder = new JpegFinder(new AccessorAdapter(_accessor));

      Assert.AreNotEqual(jpegFinder.TestPrivateFindSoi(0), -1, "no soi found");

#else

      Assert.Fail("this test requires debug mode");

#endif

    }


  }
}
