/*
 * Copyright 2016 Open University of the Netherlands
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * This project has received funding from the European Union’s Horizon
 * 2020 research and innovation programme under grant agreement No 644187.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace NodeTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AssetManagerPackage;
    using AssetPackage;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitTest1
    {
        #region Fields

        const string modelId = "test";
        const string restoredId = "restored";

        private GameStorageClientAsset asset = new GameStorageClientAsset();
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
#warning Check if all test code still works after the latest changes.
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

            if (AssetManager.Instance.Bridge == null)
            {
                AssetManager.Instance.Bridge = new Bridge();
            }

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
        /// (Unit Test Method) tests binary (de)serialize of structure + data.
        /// </summary>
        //[TestMethod]
        //public void TestChild_Serialize_06()
        //{
        //    // https://en.wikipedia.org/wiki/Tree_traversal
        //    WikiExampleTree();
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        //StreamingContext sc = new StreamingContext(StreamingContextStates.All, false);
        //        //BinaryFormatter bf = new BinaryFormatter(null, sc);
        //        Stopwatch sw = new Stopwatch();
        //        sw.Reset();
        //        sw.Start();
        //        String base64 = root.ToBinary();
        //        sw.Stop();
        //        Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //        Node restored = asset.AddModel(restoredId);
        //        //sw.Reset();
        //        sw.Start();
        //        restored.FromBinary(base64);
        //        sw.Stop();
        //        Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //        sw.Reset();
        //        sw.Start();
        //        Debug.Print(restored.ToXml());
        //        sw.Stop();
        //        Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //        //! Fixup purpose as it cannot be restored (matches the asst.Models keys).
        //        //
        //        String xml2 = restored.ToXml().Replace(
        //            String.Format("purpose=\"{0}\"", restoredId),
        //            String.Format("purpose=\"{0}\"", modelId));
        //        Assert.AreEqual(root.ToXml(), xml2);
        //        Assert.AreEqual(root["F"]["G"]["I"].Value, restored["F"]["G"]["I"].Value);
        //    }
        //}
        /// <summary>
        /// (Unit Test Method) tests binary (de)serialize of structure + data.
        /// </summary>
        //[TestMethod]
        //public void TestChild_Serialize_07()
        //{
        //    // https://en.wikipedia.org/wiki/Tree_traversal
        //    WikiExampleTree();
        //    Stopwatch sw = new Stopwatch();
        //    sw.Reset();
        //    sw.Start();
        //    String sonly = root.ToBinary(true);
        //    sw.Stop();
        //    Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //    sw.Reset();
        //    sw.Start();
        //    String sandd = root.ToBinary(false);
        //    sw.Stop();
        //    Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //    Assert.AreNotEqual(sonly, sandd);
        //}
        /// <summary>
        /// (Unit Test Method) tests binary (de)serialize of structure + data.
        /// </summary>
        //[TestMethod]
        //public void TestChild_Serialize_08()
        //{
        //    // https://en.wikipedia.org/wiki/Tree_traversal
        //    WikiExampleTree();
        //    Stopwatch sw = new Stopwatch();
        //    sw.Reset();
        //    sw.Start();
        //    String base64 = root.ToBinary();
        //    sw.Stop();
        //    Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //    Debug.Print(root.ToXml());
        //    sw.Reset();
        //    sw.Start();
        //    Node restored = asset.AddModel(restoredId);
        //    restored.FromBinary(base64);
        //    sw.Stop();
        //    Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //    Debug.Print(restored.ToXml());
        //    //! Fixup purpose as it cannot be restored (matches the asst.Models keys).
        //    //
        //    String xml2 = restored.ToXml().Replace(
        //        String.Format("purpose=\"{0}\"", restoredId),
        //        String.Format("purpose=\"{0}\"", modelId));
        //    Assert.AreEqual(root.ToXml(), xml2);
        //}
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
            String xml = root.ToString(false, SerializingFormat.Xml);
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
            WikiExampleTree(asset.AddModel("Wiki"));

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            String xml1 = asset["Wiki"].ToString(true, SerializingFormat.Xml);
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            Node restored = asset.AddModel(restoredId);

            sw.Reset();
            sw.Start();
            //! Note that restored is not restored inside FromString (but the Owner Asset's model collection is).
            //! So we need an assignment if we want to use restored further on or get it from the asssets nodes.
            restored.FromString(xml1, true);
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            //! Fixup purpose as it cannot be restored (matches the asst.Models keys).
            //
            String xml2 = asset[restoredId].ToString(true, SerializingFormat.Xml).Replace(
                String.Format("<Purpose>{0}</Purpose>", restoredId),
                String.Format("<Purpose>{0}</Purpose>", modelId));

            //Debug.Print(xml2);

            Assert.AreEqual(xml1, xml2);
        }

        /// <summary>
        /// (Unit Test Method) tests child xml (de)serialize.
        /// </summary>
        [TestMethod]
        public void TestChild_Serialize_03()
        {
            // https://en.wikipedia.org/wiki/Tree_traversal
            WikiExampleTree(asset.AddModel("Wiki"));

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            String xmls = asset["Wiki"].ToString(true, SerializingFormat.Xml);
            String xmld = asset["Wiki"].ToString(false, SerializingFormat.Xml);
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            Node restored = asset.AddModel(restoredId);

            String xml2s = xmls.Replace(
            String.Format("<Purpose>{0}</Purpose>", "Wiki"),
            String.Format("<Purpose>{0}</Purpose>", restoredId));

            sw.Reset();
            sw.Start();
            //! Note that restored is not restored inside FromString (but the Owner Asset's model collection is).
            //! So we need an assignment if we want to use restored further on or get it from the asssets nodes.
            restored.FromString(xml2s, true);
            restored.FromString(xmld, false);
            sw.Stop();
            Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);

            //! Fixup purpose as it cannot be restored (matches the asst.Models keys).
            //
            String xml2 = asset[restoredId].ToString(false, SerializingFormat.Xml).Replace(
                String.Format("<Purpose>{0}</Purpose>", restoredId),
                String.Format("<Purpose>{0}</Purpose>", "Wiki"));

            //Debug.Print(xml2);

            Assert.AreEqual(xmld, xml2);
        }

        ///// <summary>
        ///// (Unit Test Method) tests binary serialize of structre.
        ///// </summary>
        //[TestMethod]
        //public void TestChild_Serialize_03()
        //{
        // https://en.wikipedia.org/wiki/Tree_traversal
        //WikiExampleTree(asset.AddModel("Wiki"));

        //using (MemoryStream ms = new MemoryStream())
        //{
        //    BinaryFormatter bf = new BinaryFormatter();

        //    Stopwatch sw = new Stopwatch();

        //    sw.Reset();
        //    sw.Start();
        //    bf.Serialize(ms, root);
        //    sw.Stop();
        //    Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //}
        //}

        /// <summary>
        /// (Unit Test Method) tests binary (de)serialize of structure.
        /// </summary>
        //[TestMethod]
        //public void TestChild_Serialize_04()
        //{
        //// https://en.wikipedia.org/wiki/Tree_traversal
        //WikiExampleTree();
        //using (MemoryStream ms = new MemoryStream())
        //{
        //    Stopwatch sw = new Stopwatch();
        //    sw.Reset();
        //    sw.Start();
        //    String base64 = root.ToBinary(true);
        //    sw.Stop();
        //    Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //    Node restored = asset.AddModel(restoredId);
        //    ms.Flush();
        //    ms.Seek(0, SeekOrigin.Begin);
        //    sw.Reset();
        //    sw.Start();
        //    restored.FromBinary(base64, true);
        //    sw.Stop();
        //    Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //    Debug.Print(restored.ToXml());
        //    //! Fixup purpose as it cannot be restored (matches the asst.Models keys).
        //    //
        //    String xml2 = restored.ToXml().Replace(
        //        String.Format("purpose=\"{0}\"", restoredId),
        //        String.Format("purpose=\"{0}\"", modelId));
        //    Assert.AreEqual(root.ToXml(), xml2);
        //    //! These should not be equal as we restore only the structure.
        //    Assert.AreNotEqual(root.ToXml(false), xml2);
        //    //! These should not be equal as we restore only the structure.
        //    Assert.AreNotEqual(root["F"]["B"]["D"]["C"].Value, restored["F"]["B"]["D"]["C"].Value);
        //    Assert.IsNull(restored["F"]["B"]["D"]["C"].Value);
        //    Assert.IsFalse(restored.ToXml().Contains("<value>"));
        //}
        //}

        ///// <summary>
        ///// (Unit Test Method) tests binary serialize of structure + data.
        ///// </summary>
        //[TestMethod]
        //public void TestChild_Serialize_05()
        //{
        //    // https://en.wikipedia.org/wiki/Tree_traversal
        //    WikiExampleTree(asset.AddModel("Wiki"));

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        StreamingContext sc = new StreamingContext(StreamingContextStates.All, false);

        //        BinaryFormatter bf = new BinaryFormatter(null, sc);

        //        Stopwatch sw = new Stopwatch();

        //        sw.Reset();
        //        sw.Start();
        //        bf.Serialize(ms, root);
        //        sw.Stop();
        //        Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
        //    }
        //}

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

        private void WikiExampleTree(Node root)
        {
            root.Clear();

            //! Data Still Fails (as it's serialized as an Array).
            //
            List<byte> data = new List<byte>();
            data.AddRange(new byte[] { 1, 2, 3, 4, 5 });
            short[] sa = new short[] { 1, 2, 3, 4, 5 };
            sa.ToList<short>();
            Node F = root.AddChild("F", "F");
            //Node B = F.AddChild("B", data);
            Node B = F.AddChild("B", new short[] { 1, 2, 3, 4, 5 });
            B.AddChild("A", DateTime.Now);
            //B.AddChild("A1", new DemoClass
            //{
            //    a = 15,
            //    b = "vijftien",
            //    c = DateTime.Now
            //});
            B.AddChild("A2", 42);

            //for (Int32 i = 0; i < 100; i++)
            //{
            //    B.AddChild(String.Format("A{0:000}", i), i);
            //}

            //B.Value = data;

            Node D = B.AddChild("D", DateTime.Now, StorageLocations.Server);
            //D.AddChild("C", new DemoClass
            //{
            //    a = 15,
            //    b = "vijftien",
            //    c = DateTime.Now
            //});

            D.AddChild("E", (Byte)5);

            F.AddChild("G", StorageLocations.Game)
                .AddChild("I", "I")
                .AddChild("H", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" }
                //.AddChild("H", "H"
                );

            // F +-- B +-- A
            //   |     +-- D +-- C
            //   |           +-- E
            //   |
            //   +-- G --- I --- H
        }

        #endregion Methods
    }

    #region Nested Types
    public class Bridge : IBridge, ILog, IVirtualProperties
    {

        /// <summary>
        /// Looks up a given key to find its associated value.
        /// </summary>
        ///
        /// <param name="model"> The model. </param>
        /// <param name="key">   The key. </param>
        ///
        /// <returns>
        /// An Object.
        /// </returns>
        public object LookupValue(string model, string key)
        {
            if (key.Equals("Virtual"))
            {
                return DateTime.Now;
            }

            return null;
        }

        public void Log(Severity severity, string msg)
        {
            Debug.Print("{0} - {1}", severity, msg);
        }
    }

    /// <summary>
    /// (Serializable)a demo class.
    /// </summary>
    [Serializable]
    public class DemoClass
    {
        #region Fields

        public int a;
        public string b;
        public DateTime c;

        #endregion Fields
    }

    #endregion Nested Types
}