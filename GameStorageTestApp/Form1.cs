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
namespace UserModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using AssetPackage;

    public partial class Form1 : Form
    {
        GameStorageClientAsset storage = new GameStorageClientAsset();

        [Serializable]
        public struct DemoStruct
        {
            public int a;
            public string b;
            public DateTime c;
        }

        private String user = "student1";
        private String pass = "test";

        public Form1()
        {
            InitializeComponent();

            storage.Bridge = new Bridge();

            storage.AddModel("Hint");
            storage.AddModel("User");
            storage.AddModel("Test");

            storage.AddModel("Wiki");

            (storage.Settings as GameStorageClientAssetSettings).Host = "145.20.132.23";
            (storage.Settings as GameStorageClientAssetSettings).A2Port = 3000;
            (storage.Settings as GameStorageClientAssetSettings).Port = 3400;
            (storage.Settings as GameStorageClientAssetSettings).Secure = false;
            (storage.Settings as GameStorageClientAssetSettings).BasePath = "/api/";
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
        private void WikiExampleTree(Node root)
        {
            root.Clear();

            List<byte> data = new List<byte>();
            data.AddRange(new byte[] { 1, 2, 3, 4, 5 });

            Node F = root.AddChild("F", "F");
            Node B = F.AddChild("B", "B");
            B.AddChild("A", "A");
            B.Value = data;
            Node D = B.AddChild("D", DateTime.Now);
            D.AddChild("C", new DemoStruct
            {
                a = 15,
                b = "vijftien",
                c = DateTime.Now
            });

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

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            BuildDemo();

            Stopwatch sw = new Stopwatch();
            {
                sw.Reset();
                sw.Start();
                //textBox2.Text = storage["Test"].ToXml();
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }
            {
                sw.Reset();
                sw.Start();
                textBox1.Text = storage["Test"].ToXml(false);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            //GameStorageAsset x = usermodel.ResolveStorage("Hints.Test.Age");

            //Object value = usermodel.ResolveValue(("Hints.Test.Age"));
            {
                sw.Reset();
                sw.Start();
                storage.SaveStructure("Test", StorageLocations.Local);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            Debug.Print("{0}={1}", "Hints.Test.Age", storage["User"]["Age"]);
        }

        private void BuildDemo()
        {
            storage["Hint"].Clear();
            storage["User"].Clear();
            storage["Test"].Clear();
            storage["Wiki"].Clear();

#warning todo pretty print generic types?

            // See https://msdn.microsoft.com/en-us/library/windows/apps/system.type.makegenerictype(v=vs.105).aspx
            // See https://msdn.microsoft.com/en-us/library/windows/apps/system.type.getgenerictypedefinition(v=vs.105).aspx
            // 
            // Type.MakeGenericType
            // Type.GetGenericArguments
            // 
            // Type t = Type.GetType("System.Collections.Generic.Dictionary`2[System.String,System.Object]");

            // 0) Complete payload.
            storage["User"].AddChild("STRUCT", "Bla");

            // 1) Game based, not saved to server, retrieved on access.
            //
            storage["User"].AddChild("Virtual", StorageLocations.Game);

            // 2) Server based, not saved to server? retrieved on access?
            //
            storage["User"].AddChild("Server", String.Empty, StorageLocations.Server);

            // 3) Local, value not serialized, not saved to server.
            //
            storage["User"].AddChild("Transient", Double.MaxValue, StorageLocations.Transient);

            // 4) Local, serialized and saved to server.
            //
            storage["User"].AddChild("Name", "Wim van der Vegt");

            storage["User"].AddChild("Age", 25);

            List<String> hints = new List<String>();
            hints.Add("Hint1");
            hints.Add("Hint2");
            hints.Add("Hint3");

            storage["Hint"].AddChild("Hints", hints);

            storage["Hint"].AddChild("Age", 54);

            //! Combining Models works but perhaps functionally better to them separated in the GameStorageAsset Models Dictonary.
            //
            //storage["Hint"].AddChild("TESTMODEL", storage["Test"]);

            //storage["User"].AddChild("HINTMODEL", storage["Hint"]);

            storage["User"].AddChild("Hints", hints);

            WikiExampleTree(storage["Test"]);
            WikiExampleTree(storage["Wiki"]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            BuildDemo();

            Stopwatch sw = new Stopwatch();
            {
                sw.Reset();
                sw.Start();
                storage.LoadStructure("Test", StorageLocations.Local);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            textBox1.Text = storage["Test"].ToXml(false);
            textBox2.Text = storage["Test"].ToXml(true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BuildDemo();

            textBox1.Clear();
            textBox2.Clear();

            textBox1.Text += storage["User"]["Virtual"].Value.ToString();

            //usermodel.Clear();

            //usermodel.Bridge = new Bridge();

            //usermodel.Add<Int32>("Virtual", Locations.Game);
            //usermodel.Add<String>("Hello", Locations.Game);
            //usermodel.Add<DateTime>("Now", Locations.Game);

            //usermodel.Add<DateTime>("Server", Locations.Server);

            //usermodel.Add("Real", 1.00);

            //textBox1.Text += String.Format("IsDirty={0}\r\n", usermodel.isDirty);

            //// Will change the value.
            //usermodel["Real"] = 1.01;

            //// Will not change the value.
            //usermodel["Virtual"] = 15;

            //// Will not change the value.
            //usermodel["Server"] = new DateTime(0);

            //usermodel["newby"] = 15;

            //foreach (String key in usermodel.Keys)
            //{
            //    textBox1.Text += String.Format("{0}={1}; {2}\r\n", key, usermodel[key]);
            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            WikiExampleTree(storage["Wiki"]);

            Stopwatch sw = new Stopwatch();
            {
                sw.Reset();
                sw.Start();

                foreach (Node node in storage["Wiki"].PrefixEnumerator())
                {
                    if (node.Value != null)
                    {
                        string xml = node.ToXmlValue();
                    }
                }

                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            textBox2.Text += storage["Wiki"].ToJson(new List<StorageLocations>() { StorageLocations.Local });

            foreach (Node node in storage["Wiki"].PrefixEnumerator())
            {
                if (node.Value != null)
                {
                    textBox1.AppendText(String.Format("{0}\r\n{1}\r\n\r\n", node.Path, node.ToXmlValue()));
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            BuildDemo();

            textBox1.Clear();
            textBox2.Clear();

            String base64 = String.Empty;

            Stopwatch sw = new Stopwatch();

            {
                sw.Reset();
                sw.Start();

                base64 = storage["Test"].ToBinary(false);

                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            storage["Test"].Clear();
            {
                sw.Reset();
                sw.Start();

                storage["Test"].FromBinary(base64, false);

                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }
            Debug.Print(storage["Test"].Purpose);

            textBox1.Text = storage["Test"].ToXml(false);
            textBox2.Text = storage["Test"].ToXml();

            Debug.Print(storage["Test"].Purpose);
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            CheckHealth();
        }

        private bool CheckHealth()
        {
            //! Make CheckHealth() async.
            // 
            return Task.Factory.StartNew<bool>(() => { return storage.CheckHealth(); }).Result;

            //Task<bool> taskName = Task.Factory.StartNew<bool>(() => { return storage.CheckHealth(); });

            //Debug.Print("Hello1 (during request)");

            //bool Result = taskName.Result;

            //Debug.Print("Hello2 (after request)");

            //return Result;
        }

        private bool Login()
        {
            Task<bool> taskName = Task.Factory.StartNew<bool>(() => { return storage.Login(user, pass); });

            return taskName.Result;
        }

        private async Task<bool> SaveStructureToServer(string key)
        {
            return await Task.Factory.StartNew(() =>
            {
                storage.SaveStructure(key, StorageLocations.Server);

                return storage.Connected;
            });
        }

        private async Task<bool> LoadStructureFromServer(string key)
        {
            return await Task.Factory.StartNew(() =>
            {
                storage.LoadStructure(key, StorageLocations.Server);

                return storage.Connected;
            });
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            CheckHealth();
            Login();

            BuildDemo();

            foreach (KeyValuePair<String, Node> kvp in storage)
            {
                textBox1.Text = storage[kvp.Key].ToXml(false);

                await SaveStructureToServer(kvp.Key);
                storage[kvp.Key].Clear();
                await LoadStructureFromServer(kvp.Key);

                textBox2.Text = storage[kvp.Key].ToXml(false);
            }
        }
    }
}
