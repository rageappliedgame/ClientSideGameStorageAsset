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
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using AssetPackage;

    public partial class Form1 : Form
    {
        GameStorageClientAsset storage = new GameStorageClientAsset();

        TextBoxTraceListener textWriter = null;

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

            textWriter = new TextBoxTraceListener(textBox3);

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

            // Catch debugging output.
            // 
            Debug.Listeners.Add(textWriter);
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
            Node D = B.AddChild("D", DateTime.Now, StorageLocations.Server);
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
            storage["User"].AddChild("STRUCT", new DemoStruct
            {
                a = 10,
                b = "elf",
                c = new DateTime(12)
            });

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

            //textBox2.Text += storage["Wiki"].ToJson(StorageLocations.Local);

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

        private /*async*/ void button6_Click(object sender, EventArgs e)
        {
            if (storage.CheckHealth())
            {
                Debug.Print(storage.Health);

                if (storage.Login(user, pass))
                {
                    Debug.Print("Logged-in");
                }
            }
        }

        /*
        private bool CheckHealth()
        {
            //! Make CheckHealth() async (warning: gives dead-lock with TraceListener).
            // 
            {
                //return Task.Factory.StartNew<bool>(() => { return storage.CheckHealth(); }).Result;
            }
            {
                //Task<bool> taskName = Task.Factory.StartNew<bool>(() => { return storage.CheckHealth(); });

                //Debug.Print("Hello1 (during request)");

                //bool Result = taskName.Result;

                //Debug.Print("Hello2 (after request)");
            }
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

        private async Task<bool> DeleteStructureFromServer(string key)
        {
            return await Task.Factory.StartNew(() =>
            {
                storage.DeleteStructure(key, StorageLocations.Server);

                return storage.Connected;
            });
        }
        */

        private /*async*/ void button7_Click(object sender, EventArgs e)
        {
            if (storage.Connected)
            {
                BuildDemo();

                foreach (KeyValuePair<String, Node> kvp in storage)
                {
                    //! Structure + Data.
                    // 
                    textBox1.Text = storage[kvp.Key].ToXml(false);

                    storage.DeleteStructure(kvp.Key, StorageLocations.Server);

                    storage.SaveStructure(kvp.Key, StorageLocations.Server);
                    storage[kvp.Key].Clear();
                    storage.LoadStructure(kvp.Key, StorageLocations.Server);

                    //! Structure Only.
                    // 
                    textBox2.Text = storage[kvp.Key].ToXml(false);
                }
            }
        }

        #region Nested Types

        /// <summary>
        /// See http://www.codeproject.com/KB/trace/TextBoxTraceListener.aspx.
        /// </summary>
        public class TextBoxTraceListener : TraceListener
        {
            #region Fields

            private StringSendDelegate fInvokeWrite;

            /// <summary>
            /// Target for the.
            /// </summary>
            private TextBox fTarget;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the Swiss.DebugForm.NextGridTraceListener class.
            /// </summary>
            ///
            /// <param name="target"> Target for the. </param>
            public TextBoxTraceListener(TextBox target)
            {
                fTarget = target;
                fInvokeWrite = new StringSendDelegate(SendString);
            }

            #endregion Constructors

            #region Delegates

            /// <summary>
            /// String send delegate.
            /// </summary>
            ///
            /// <param name="message"> The message. </param>
            private delegate void StringSendDelegate(string message);

            #endregion Delegates

            #region Methods

            /// <summary>
            /// When overridden in a derived class, writes the specified message to the listener you create
            /// in the derived class.
            /// </summary>
            ///
            /// <param name="message"> A message to write. </param>
            public override void Write(string message)
            {
                fTarget.Invoke(fInvokeWrite, new object[] { message });
            }

            /// <summary>
            /// When overridden in a derived class, writes a message to the listener you create in the
            /// derived class, followed by a line terminator.
            /// </summary>
            ///
            /// <param name="message"> A message to write. </param>
            public override void WriteLine(string message)
            {
                fTarget.Invoke(fInvokeWrite, new object[] { message + Environment.NewLine });
            }

            /// <summary>
            /// Sends a string.
            /// </summary>
            ///
            /// <param name="message"> A message to write. </param>
            private void SendString(string message)
            {
                // No need to lock text box as this function will only
                // ever be executed from the UI thread!
                fTarget.AppendText(message);
            }

            #endregion Methods
        }

        #endregion Nested Types

        private void button8_Click(object sender, EventArgs e)
        {
            BuildDemo();

            //Debug.Print(storage.SerializeData("Wiki", StorageLocations.Local, SerializingFormat.Json));
            Debug.Print(storage.SerializeData("Wiki", StorageLocations.Server, SerializingFormat.Json));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            BuildDemo();

            Debug.WriteLine("PostFix");

            foreach (Node node in storage["Wiki"].PostfixEnumerator())
            {
                Debug.Write(node.Name + " ");
            }
            Debug.WriteLine(String.Empty);

            foreach (StorageLocations loc in Enum.GetValues(typeof(StorageLocations)))
            {
                Debug.WriteLine(String.Format("{0}-", loc.ToString()));
                foreach (Node node in storage["Wiki"].PostfixEnumerator(new List<StorageLocations> { loc }))
                {
                    Debug.Write(node.Name + " ");
                }
                Debug.WriteLine(String.Empty);
            }

            Debug.WriteLine("PreFix");

            foreach (Node node in storage["Wiki"].PrefixEnumerator())
            {
                Debug.Write(node.Name + " ");
            }
            Debug.WriteLine(String.Empty);

            foreach (StorageLocations loc in Enum.GetValues(typeof(StorageLocations)))
            {
                Debug.WriteLine(String.Format("{0}-", loc.ToString()));
                foreach (Node node in storage["Wiki"].PrefixEnumerator(new List<StorageLocations> { loc }))
                {
                    Debug.Write(node.Name + " ");
                }
                Debug.WriteLine(String.Empty);
            }
            Debug.WriteLine(String.Empty);

            //List<int> x = new List<int>();
            //x.Add(10);
            //x.Add(11);
            //x.Add(12);

            ////List`1
            //Debug.Print(ResolveType(x.GetType()));
            //Debug.Print(ResolveType(typeof(Dictionary<String, Int32>)));

            //Debug.WriteLine(JsonConvert.SerializeObject(storage["Wiki"]["F"]["B"], Formatting.Indented));
        }

        /// <summary>
        /// The Regex to match the generic name so `nn suffix can be replaces by &lt;
        /// &gt; brackets paramaters. Only the `nn will actually be a match group.
        /// </summary>
        private static Regex gargs = new Regex(@"(?:.+)(`(?:\d+))(?:.?)");

        private static String ResolveType(Type t)
        {
            String cls = t.Name;

            if (t.IsGenericType)
            {
                //Match m = gargs.Match(cls);
                //if (m.Groups.Count == 2)
                //{
                //    cls = cls.Remove(m.Groups[1].Index, m.Groups[1].Length);
                //    cls = cls.Insert(m.Groups[1].Index, String.Format("<{0}>", String.Join(",", t.GenericTypeArguments.Select(p => p.Name))));
                //}

                cls = t.GetGenericTypeDefinition().Name;
            }

            return cls;
        }
    }
}
