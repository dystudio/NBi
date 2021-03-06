﻿using System;
using NBi.Xml;
using NUnit.Framework;
using System.Xml;
using System.IO;

namespace NBi.Testing.Unit.Xml
{
    [TestFixture]
    public class XmlManagerWithDtdTest
    {
        private string filename { get; set; }
        private string includedFilename { get; set; }
        
        [SetUp]
        public void Setup()
        {
            includedFilename = DiskOnFile.CreatePhysicalFile("TestSuiteIncludedTestSuite.xml", "NBi.Testing.Unit.Xml.Resources.TestSuiteIncludedTestSuite.xml");
            Console.WriteLine("Included file created at '{0}'", includedFilename);
            filename = DiskOnFile.CreatePhysicalFile("TestSuiteWithIncludeTestSuite.nbits", "NBi.Testing.Unit.Xml.Resources.TestSuiteWithIncludeTestSuite.xml");
            Console.WriteLine("Main file created at '{0}'", filename);
        }
            
        [Test]
        public void Load_ValidFile_Success()
        {
            var manager = new XmlManager();
            manager.Load(filename, true);

            Assert.That(manager.TestSuite, Is.Not.Null);
        }

        [Test]
        public void Load_ValidFile_TwoTestsLoaded()
        {
            var manager = new XmlManager();
            manager.Load(filename, true);

            Assert.That(manager.TestSuite.Tests, Has.Count.EqualTo(2));
        }

        [Test]
        public void Load_ValidFileInSubFolder_TwoTestsLoaded()
        {
            //Delete the eventually existing file
            if (File.Exists(filename))
                File.Delete(filename);
            if (File.Exists(includedFilename))
                File.Delete(includedFilename);

            //Recreate them in a subdirectory
            includedFilename = DiskOnFile.CreatePhysicalFile(@"Dtd\TestSuiteIncludedTestSuite.xml", "NBi.Testing.Unit.Xml.Resources.TestSuiteIncludedTestSuite.xml");
            Console.WriteLine("Included file created at '{0}'", includedFilename);
            filename = DiskOnFile.CreatePhysicalFile(@"Dtd\TestSuiteWithIncludeTestSuite.nbits", "NBi.Testing.Unit.Xml.Resources.TestSuiteWithIncludeTestSuite.xml");
            Console.WriteLine("Main file created at '{0}'", filename);

            var manager = new XmlManager();
            manager.Load(filename, true);

            Assert.That(manager.TestSuite.Tests, Has.Count.EqualTo(2));
        }

        [Test]
        public void Load_ValidFileButWithoutDtdProcessingSetToTrue_Successfully()
        {
            var manager = new XmlManager();
            var ex = Assert.Throws<ArgumentException>(delegate { manager.Load(filename, false); });
            Assert.That(ex.Message, Is.StringContaining("DTD is prohibited. To activate it, set the flag allow-dtd-processing to true in the config file associated to this test-suite"));
        }
    }
}
