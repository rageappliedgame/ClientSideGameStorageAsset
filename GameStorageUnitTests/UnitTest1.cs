﻿namespace NodeTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using AssetPackage;

    [TestClass]
    public class UnitTest1
    {
        #region Constants
#warning TEST IF RESTORING WORKS (STRUCTURE-ONLY BT DEFAULT).
        const string modelId = "test";
        const string restoredId = "restored";

        #endregion Constants

        #region Fields

        private GameStorageClientAsset asset;

        private Node root;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Cleanup() is called once during test execution after test methods in this
        /// class have executed unless this test class' Initialize() method throws an
        /// exception.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            asset[modelId].Clear();
        }

        /// <summary>
        /// Initialize() is called once during test execution before test methods in
        /// this test class are executed.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            Debug.Print("-------");

            asset.AddModel(modelId);

            root = asset[modelId];
        }

        /// <summary>
        /// (Unit Test Method) tests adding a child.
        /// </summary>
        [TestMethod]
        public void TestChild()
        {
            Node child = root.AddChild("Property1", 42);

            DumpNode(child);

            Assert.AreEqual(child.Path, "Property1");
            Assert.AreEqual(child.Value, 42);
        }

        /// <summary>
        /// (Unit Test Method) tests child at int index.
        /// </summary>
        [TestMethod]
        public void TestChild_At_IntIndex()
        {
            Node child = root.AddChild("Property2", 42);

            DumpNode(child);

            Assert.AreEqual(child.Path, root[0].Path);
            Assert.AreEqual(child.Value, root[0].Value);
        }

        /// <summary>
        /// (Unit Test Method) tests child at string index.
        /// </summary>
        [TestMethod]
        public void TestChild_At_StringIndex_01()
        {
            Node child = root.AddChild("Property3", 42);

            DumpNode(child);

            Assert.AreEqual(child.Path, root["Property3"].Path);
            Assert.AreEqual(child.Value, root["Property3"].Value);
        }

        /// <summary>
        /// (Unit Test Method) tests two childs at string index.
        /// </summary>
        [TestMethod]
        public void TestChild_At_StringIndex_02()
        {
            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);

            Assert.AreEqual(child1.Path, root["Property4"].Path);
            Assert.AreEqual(child2.Path, root["Property5"].Path);

            Assert.AreEqual(child1.Name, root["Property4"].Name);
            Assert.AreEqual(child2.Name, root["Property5"].Name);

            Assert.AreEqual(child1.Value, root["Property4"].Value);
            Assert.AreEqual(child2.Value, root["Property5"].Value);

            Assert.AreSame(child1, root["Property4"]);
            Assert.AreSame(child2, root["Property5"]);
        }

        /// <summary>
        /// (Unit Test Method) tests a small tree and some common indexing errors.
        /// </summary>
        [TestMethod]
        public void TestChild_Children()
        {
            Assert.IsNull(root["Property6"]);
            Assert.IsNull(root[0]);

            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);

            Assert.IsNull(root["Property6"]);
            Assert.IsNull(root[-1]);
            Assert.IsNull(root[2]);

            Assert.IsNotNull(root[0]);
            Assert.IsNotNull(root[1]);
        }

        /// <summary>
        /// (Unit Test Method) tests child comparisment.
        /// </summary>
        [TestMethod]
        public void TestChild_Compare()
        {
            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);

            Assert.IsTrue(child1.Equals(root["Property4"]));
            Assert.IsTrue(child1 == root["Property4"]);

            Assert.IsFalse(child1.Equals(root["Property5"]));
            Assert.IsFalse(child1 == root["Property5"]);
        }

        /// <summary>
        /// (Unit Test Method) tests simple children enumerate using the indexer.
        /// </summary>
        [TestMethod]
        public void TestChild_Enumerate_01()
        {
            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);
            Node child3 = child1.AddChild("SubProperty6", 43);

            foreach (Node child in root.Children)
            {
                DumpNode(child);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests prefix child enumerate of root.
        /// </summary>
        [TestMethod]
        public void TestChild_Enumerate_02()
        {
            // F,B,A,D,C,E,G,I,H
            foreach (Node child in WikiExampleTree().PrefixEnumerator())
            {
                DumpNode(child);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests child prefix enumerate of a node.
        /// </summary>
        [TestMethod]
        public void TestChild_Enumerate_03()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();
            // F +-- B +-- A
            //   |     +-- D +-- C
            //   |           +-- E
            //   |
            //   +-- G --- I --- H
            //   
            // F,B,A,D,C,E,G,I,H
            foreach (Node child in root["F"]["B"]["D"].PrefixEnumerator())
            {
                DumpNode(child);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests child enumerate using the Children list.
        /// </summary>
        [TestMethod]
        public void TestChild_Enumerate_04()
        {
            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);
            Node child3 = child1.AddChild("SubProperty6", 43);
            // Order is Property4, Property5
            foreach (Node child in root.Children)
            {
                DumpNode(child);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests child postfix enumerate.
        /// </summary>
        [TestMethod]
        public void TestChild_Enumerate_05()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();
            // F +-- B +-- A
            //   |     +-- D +-- C
            //   |           +-- E
            //   |
            //   +-- G --- I --- H
            //   
            // A,C,E,D,B,H,I,G,F
            foreach (Node child in root.PostfixEnumerator())
            {
                DumpNode(child);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests child postfix enumerate with filtering on StorageLocation.
        /// </summary>
        [TestMethod]
        public void TestChild_Enumerate_06()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            // A,C,E,D,B,(H,I,G),F
            List<StorageLocations> filter = new List<StorageLocations>();
            filter.Add(StorageLocations.Game);

            foreach (Node child in root.PostfixEnumerator(filter))
            {
                DumpNode(child);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests child grand children using Parent.
        /// </summary>
        [TestMethod]
        public void TestChild_GrandChildren()
        {
            Node child1 = root.AddChild("Property4", 42);
            Node child2 = child1.AddChild("SubProperty5", 43);

            DumpNode(child2);

            Assert.AreEqual(child2.Parent, child1);
        }

        /// <summary>
        /// (Unit Test Method) tests child modification.
        /// </summary>
        [TestMethod]
        public void TestChild_Modify_01()
        {
            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);
            Node child3 = child1.AddChild("SubProperty6", 43);

            DumpNode(child3);
            child1["SubProperty6"].Value = 44;
            DumpNode(child3);

            Assert.AreEqual(child3.Value, 44);
            Assert.AreEqual(child1["SubProperty6"].Value, 44);
        }

        /// <summary>
        /// (Unit Test Method) tests child range check on indexers.
        /// </summary>
        [TestMethod]
        public void TestChild_RangeCheck()
        {
            Assert.IsNull(root["Property6"]);
            Assert.IsNull(root[0]);

            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);

            Assert.IsNull(root["Property6"]);
            Assert.IsNull(root[-1]);
            Assert.IsNull(root[2]);

            Assert.IsNotNull(root[0]);
            Assert.IsNotNull(root[1]);
        }

        /// <summary>
        /// (Unit Test Method) tests child removal.
        /// </summary>
        [TestMethod]
        public void TestChild_Remove_01()
        {
            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);
            Node child3 = child1.AddChild("SubProperty6", 43);

            root.Remove(child2);

            foreach (Node child in root.Children)
            {
                DumpNode(child);
            }

            Assert.AreEqual(root.Count, 1);
        }

        /// <summary>
        /// (Unit Test Method) tests child removal.
        /// </summary>
        [TestMethod]
        public void TestChild_Remove_02()
        {
            Node child1 = root.AddChild("Property4", 42);
            Node child2 = root.AddChild("Property5", 43);
            Node child3 = child1.AddChild("SubProperty6", 43);

            // Not a Child.
            Assert.IsFalse(child1.Remove(child2));
            Assert.IsFalse(child1.Remove(null));

            child1.Remove(child3);

            // No Children.
            Assert.IsFalse(child2.Remove(child1));
            Assert.IsFalse(child1.Remove(child3));

            foreach (Node child in root.Children)
            {
                DumpNode(child);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests xml serialize.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_01()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            String xml = root.ToXml();
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            Debug.Print(xml);
        }

        /// <summary>
        /// (Unit Test Method) tests child xml (de)serialize.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_02()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            String xml1 = root.ToXml();
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            Node restored = asset.AddModel(restoredId);

            sw.Reset();
            sw.Start();
            restored.FromXml(xml1);
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            String xml2 = restored.ToXml();

            //Debug.Print(xml2);

            Assert.AreEqual(xml1, xml2);
        }

        /// <summary>
        /// (Unit Test Method) tests binary serialize of structre.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_03()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();

                Stopwatch sw = new Stopwatch();

                sw.Reset();
                sw.Start();
                bf.Serialize(ms, root);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests binary (de)serialize of structure.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_04()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            using (MemoryStream ms = new MemoryStream())
            {
                StreamingContext sc = new StreamingContext(StreamingContextStates.All, true);

                BinaryFormatter bf = new BinaryFormatter(null, sc);

                Stopwatch sw = new Stopwatch();

                sw.Reset();
                sw.Start();
                bf.Serialize(ms, root);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

                Node restored = asset.AddModel(restoredId);

                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                sw.Reset();
                sw.Start();
                restored = (Node)bf.Deserialize(ms);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

                Debug.Print(restored.ToXml());

                // We restore structure only, so root and restore will differ.
                // 
                Assert.AreNotEqual(root.ToXml(), restored.ToXml());
                Assert.AreNotEqual(root["F"]["G"]["I"].Value, restored["F"]["G"]["I"].Value);
                Assert.IsNull(restored["F"]["G"]["I"].Value);
                Assert.IsFalse(restored.ToXml().Contains("<value>"));
            }
        }

        /// <summary>
        /// (Unit Test Method) tests binary serialize of structure + data.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_05()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            using (MemoryStream ms = new MemoryStream())
            {
                StreamingContext sc = new StreamingContext(StreamingContextStates.All, false);

                BinaryFormatter bf = new BinaryFormatter(null, sc);

                Stopwatch sw = new Stopwatch();

                sw.Reset();
                sw.Start();
                bf.Serialize(ms, root);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests binary (de)serialize of structure + data.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_06()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            using (MemoryStream ms = new MemoryStream())
            {
                StreamingContext sc = new StreamingContext(StreamingContextStates.All, false);

                BinaryFormatter bf = new BinaryFormatter(null, sc);

                Stopwatch sw = new Stopwatch();

                sw.Reset();
                sw.Start();
                bf.Serialize(ms, root);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

                Node restored = asset.AddModel(restoredId);

                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                sw.Reset();
                sw.Start();
                restored = (Node)bf.Deserialize(ms);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

                sw.Reset();
                sw.Start();
                Debug.Print(restored.ToXml());
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

                Assert.AreEqual(root.ToXml(), restored.ToXml());
                Assert.AreEqual(root["F"]["G"]["I"].Value, restored["F"]["G"]["I"].Value);
            }
        }

        /// <summary>
        /// (Unit Test Method) tests binary (de)serialize of structure + data.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_07()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            String sonly = root.ToBinary(true);
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            String sandd = root.ToBinary(false);
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            Assert.AreNotEqual(sonly, sandd);
        }

        /// <summary>
        /// (Unit Test Method) tests binary (de)serialize of structure + data.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_08()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            String base64 = root.ToBinary();
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            Debug.Print(root.ToXml());

            sw.Reset();
            sw.Start();
            Node restored = asset.AddModel(restoredId);
            restored.FromBinary(base64);
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            Debug.Print(restored.ToXml());

            Assert.AreEqual(root.ToXml(), restored.ToXml());
        }

        /// <summary>
        /// (Unit Test Method) tests clearing the tree.
        /// </summary>
        [TestMethod]
        public void TestChild_Clear()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree();

            Assert.AreEqual(root.Count, 1);

            root.Clear();

            Assert.AreEqual(0, root.Count);
        }

        /// <summary>
        /// (Unit Test Method) tests root.
        /// </summary>
        [TestMethod]
        public void TestRoot()
        {
            Assert.IsNull(root.Parent);

            Assert.AreEqual(root.Path, string.Empty);
            Assert.AreEqual(root.Name, "root");

            DumpNode(root);
        }

        private static void DumpNode(Node node)
        {
            Debug.Print("Path: {0}", node.Path);
            Debug.Print("Name: {0}", node.Name);
            Debug.Print("Value: {0}", node.Value);
            Debug.Print("Storage: {0}", node.StorageLocation);

            if (node.Value != null)
            {
                Debug.Print("Type: {0}", node.Value.GetType().Name);
            }

            if (node.Count != 0)
            {
                Debug.Print("Children: {0}", node.Count);
            }
            Debug.Print(String.Empty);
        }

        /// <summary>
        /// Build Wiki example tree.
        /// 
        /// See https://en.wikipedia.org/wiki/Tree_traversal
        /// </summary>
        ///
        /// <returns>
        /// A Node&lt;object&gt;
        /// </returns>
        private Node WikiExampleTree()
        {
            Node F = root.AddChild("F", "F");
            Node B = F.AddChild("B", "B");
            B.AddChild("A", "A");
            Node D = B.AddChild("D", "D");
            D.AddChild("C", "C");
            D.AddChild("E", (Byte)5);

            List<byte> data = new List<byte>();
            data.AddRange(new byte[] { 1, 2, 3, 4, 5 });

            F.AddChild("G", "G", StorageLocations.Game)
                .AddChild("I", "I")
                .AddChild("H", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" }
                //.AddChild("H", "H"
                );

            // F +-- B +-- A
            //   |     +-- D +-- C
            //   |           +-- E
            //   |
            //   +-- G --- I --- H

            return F;
        }

        #endregion Methods
    }

    /// <summary>
    /// A string writer utf-8.
    /// </summary>
    ///
    /// <remarks>
    /// Fix-up for XDocument Serialization defaulting to utf-16.
    /// </remarks>
    internal class StringWriterUtf8 : StringWriter
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="T:System.Text.Encoding" /> in which the output is
        /// written.
        /// </summary>
        ///
        /// <value>
        /// The Encoding in which the output is written.
        /// </value>
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        #endregion Properties
    }
}