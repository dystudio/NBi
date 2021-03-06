﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NBi.Xml;
using NBi.Xml.Decoration;
using NBi.Xml.Decoration.Condition;
using NUnit.Framework;

namespace NBi.Testing.Unit.Xml.Decoration
{
    [TestFixture]
    public class ConditionXmlTest
    {

        protected TestSuiteXml DeserializeSample()
        {
            // Declare an object variable of the type to be deserialized.
            var manager = new XmlManager();

            // A Stream is needed to read the XML document.
            using (Stream stream = Assembly.GetExecutingAssembly()
                                           .GetManifestResourceStream("NBi.Testing.Unit.Xml.Resources.ConditionXmlTestSuite.xml"))
            using (StreamReader reader = new StreamReader(stream))
            {
                manager.Read(reader);
            }
            return manager.TestSuite;
        }
        
        [Test]
        public void Deserialize_SampleFile_Check()
        {
            int testNr = 0;

            // Create an instance of the XmlSerializer specifying type and namespace.
            TestSuiteXml ts = DeserializeSample();

            // Check the properties of the object.
            Assert.That(ts.Tests[testNr].Condition, Is.TypeOf<ConditionXml>());
        }


        [Test]
        public void Deserialize_SampleFile_SetupCountCommands()
        {
            int testNr = 0;

            // Create an instance of the XmlSerializer specifying type and namespace.
            TestSuiteXml ts = DeserializeSample();

            // Check the properties of the object.
            Assert.That(ts.Tests[testNr].Condition.Predicates, Has.Count.EqualTo(2));
        }

        [Test]
        public void Deserialize_SampleFile_RunningService()
        {
            int testNr = 0;

            // Create an instance of the XmlSerializer specifying type and namespace.
            TestSuiteXml ts = DeserializeSample();

            // Check the properties of the object.
            Assert.That(ts.Tests[testNr].Condition.Predicates[0], Is.TypeOf<ServiceRunningXml>());
            var check = ts.Tests[testNr].Condition.Predicates[0] as ServiceRunningXml;
            Assert.That(check.TimeOut, Is.EqualTo("5000")); //Default value
            Assert.That(check.ServiceName, Is.EqualTo("MyService")); 

            // Check the properties of the object.
            Assert.That(ts.Tests[testNr].Condition.Predicates[1], Is.TypeOf<ServiceRunningXml>());
            var check2 = ts.Tests[testNr].Condition.Predicates[1] as ServiceRunningXml;
            Assert.That(check2.TimeOut, Is.EqualTo("1000")); //Value Specified
            Assert.That(check2.ServiceName, Is.EqualTo("MyService2")); 
        }

        [Test]
        public void Deserialize_SampleFile_Custom()
        {
            int testNr = 1;

            // Create an instance of the XmlSerializer specifying type and namespace.
            TestSuiteXml ts = DeserializeSample();

            // Check the properties of the object.
            Assert.That(ts.Tests[testNr].Condition.Predicates[0], Is.TypeOf<CustomConditionXml>());
            var condition = ts.Tests[testNr].Condition.Predicates[0] as CustomConditionXml;
            Assert.That(condition.AssemblyPath, Is.EqualTo("myAssembly.dll"));
            Assert.That(condition.TypeName, Is.EqualTo("myType"));
            Assert.That(condition.Parameters, Has.Count.EqualTo(2));
            Assert.That(condition.Parameters[0].Name, Is.EqualTo("firstParam"));
            Assert.That(condition.Parameters[0].StringValue, Is.EqualTo("2012-10-10"));
            Assert.That(condition.Parameters[1].Name, Is.EqualTo("secondParam"));
            Assert.That(condition.Parameters[1].StringValue, Is.EqualTo("102"));
        }

        [Test]
        public void Serialize_Custom_Correct()
        {
            var root = new ConditionXml()
            {
                Predicates = new List<DecorationConditionXml>()
                {
                    new CustomConditionXml()
                    {
                        AssemblyPath = "myAssembly.dll",
                        TypeName = "myType",
                    }
                }
            };

            var manager = new XmlManager();
            var xml = manager.XmlSerializeFrom(root);
            Console.WriteLine(xml);
            Assert.That(xml, Is.StringContaining("<custom "));
            Assert.That(xml, Is.StringContaining("assembly-path=\"myAssembly.dll\""));
            Assert.That(xml, Is.StringContaining("type=\"myType\""));
            Assert.That(xml, Is.Not.StringContaining("<parameter"));
        }

        [Test]
        public void Serialize_CustomWithParameters_Correct()
        {
            var root = new ConditionXml()
            {
                Predicates = new List<DecorationConditionXml>()
                {
                    new CustomConditionXml()
                    {
                        AssemblyPath = "myAssembly.dll",
                        TypeName = "myType",
                        Parameters = new List<CustomConditionParameterXml>()
                        {
                            new CustomConditionParameterXml() { Name="firstParam", StringValue="myValue" }
                        }
                    }
                }
            };

            var manager = new XmlManager();
            var xml = manager.XmlSerializeFrom(root);
            Console.WriteLine(xml);
            Assert.That(xml, Is.StringContaining("<parameter name=\"firstParam\">myValue</parameter>"));
        }
    }
}
