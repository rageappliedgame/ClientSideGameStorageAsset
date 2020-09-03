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
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using AssetPackage;

    public partial class Form1 : Form
    {
        #region Fields

        /// <summary>
        /// The Regex to match the generic name so `nn suffix can be replaces by &lt;
        /// &gt; brackets paramaters. Only the `nn will actually be a match group.
        /// </summary>
        private static Regex gargs = new Regex(@"(?:.+)(`(?:\d+))(?:.?)");

        /// <summary>
        /// The password.
        /// </summary>
        private String pass = "test";

        /// <summary>
        /// The storage.
        /// </summary>
        GameStorageClientAsset storage = new GameStorageClientAsset();

        /// <summary>
        /// The text writer.
        /// </summary>
        TextBoxTraceListener textWriter = null;

        /// <summary>
        /// The username.
        /// </summary>
        private String user = "student1";

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the UserModel.Form1 class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            textWriter = new TextBoxTraceListener(textBox3);

            storage.Bridge = new Bridge();

            storage.AddModel("Hint");
            storage.AddModel("User");
            storage.AddModel("Test");

            storage.AddModel("Wiki");

            //! Pre-register DemoClass so it's known to the GSM.
            // 
            storage.Types.Add(typeof(DemoClass).FullName, typeof(DemoClass));

            (storage.Settings as GameStorageClientAssetSettings).Host = "145.20.132.23";
            (storage.Settings as GameStorageClientAssetSettings).A2Port = 3000;
            (storage.Settings as GameStorageClientAssetSettings).Port = 3400;
            (storage.Settings as GameStorageClientAssetSettings).Secure = false;
            (storage.Settings as GameStorageClientAssetSettings).BasePath = "/api/";

            // Catch debugging output.
            //
            Debug.Listeners.Add(textWriter);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Event handler. Called by btnBinarySaveLoad for click events.
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btnBinarySaveLoad_Click(object sender, EventArgs e)
        {
            //    BuildDemo();

            //    textBox1.Clear();
            //    textBox2.Clear();

            //    String base64 = String.Empty;

            //    Stopwatch sw = new Stopwatch();

            //    {
            //        sw.Reset();
            //        sw.Start();

            //        base64 = storage["Test"].ToBinary(false);

            //        Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            //    }

            //    Debug.Print(base64);

            //    storage["Test"].Clear();
            //    {
            //        sw.Reset();
            //        sw.Start();

            //        storage["Test"].FromBinary(base64, false);

            //        Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            //    }
            //    Debug.Print(storage["Test"].Purpose);

            //    textBox1.Text = storage["Test"].ToXml(false);
            //    textBox2.Text = storage["Test"].ToXml();

            //    Debug.Print(storage["Test"].Purpose);
        }

        /// <summary>
        /// Event handler. Called by btnConnect for click events.
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btnConnect_Click(object sender, EventArgs e)
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

        /// <summary>
        /// Event handler. Called by btnFilter for click events.
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btnFilter_Click(object sender, EventArgs e)
        {
            BuildDemo();

            Debug.WriteLine("Modified Wiki Example Tree Traversal and Filtering");

            Debug.WriteLine("PostFix");
            Debug.WriteLine("-------");

            foreach (Node node in storage["Wiki"].PostfixEnumerator())
            {
                Debug.Write(node.Name + " ");
            }
            Debug.WriteLine(String.Empty);

            foreach (StorageLocations loc in Enum.GetValues(typeof(StorageLocations)))
            {
                Debug.Write(String.Format("{0}-", loc.ToString()));
                foreach (Node node in storage["Wiki"].PostfixEnumerator(new List<StorageLocations> { loc }))
                {
                    Debug.Write(node.Name + " ");
                }
                Debug.WriteLine(String.Empty);
            }

            Debug.WriteLine("PreFix");
            Debug.WriteLine("------");

            foreach (Node node in storage["Wiki"].PrefixEnumerator())
            {
                Debug.Write(node.Name + " ");
            }
            Debug.WriteLine(String.Empty);

            foreach (StorageLocations loc in Enum.GetValues(typeof(StorageLocations)))
            {
                Debug.Write(String.Format("{0}-", loc.ToString()));
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
        /// Event handler. Called by btnLoadData for click events.
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btnLoadData_Click(object sender, EventArgs e)
        {
            BuildDemo();

            if (storage.Connected)
            {
                //{
                //"nodes": [
                //                  {
                //      "Path": "F|B|D",
                //                    "Value": {
                //        "ValueType": "System.DateTime",
                //                      "Value": "2016-04-12T10:31:01.2628781+02:00"
                //      }
                //    },
                //    {
                //      "Path": "F|B|D|C",
                //      "Value": {
                //        "ValueType": "UserModel.Form1+DemoStruct",
                //        "Value": "{\r\n  \"a\": 15,\r\n  \"b\": \"vijftien\",\r\n  \"c\": \"2016-04-12T10:31:01.2628781+02:00\"\r\n}"
                //      }
                //    },
                //    {
                //      "Path": "F|B|D|E",
                //      "Value": {
                //        "ValueType": "System.Byte",
                //        "Value": 5
                //      }
                //    }
                //  ]
                //}

                textBox1.Text = storage["Wiki"].ToString(false);

                //! Clear is performed internally in LoadData().
                //
                //storage["Wiki"].ClearData(StorageLocations.Server);

                Stopwatch sw = new Stopwatch();
                {
                    sw.Reset();
                    sw.Start();
                    storage.LoadData("Wiki", StorageLocations.Server, SerializingFormat.Json);
                    sw.Stop();
                    Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
                }

                textBox2.Text = storage["Wiki"].ToString(false);
            }
            else
            {
                MessageBox.Show("Not Connected");
            }
        }

        /// <summary>
        /// Event handler. Called by btnLoadStructure for click events.
        /// </summary>
        ///
        /// <param name="sender">    Source of the event. </param>
        /// <param name="e">         Event information. </param>
        private void btnLoadStructure_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            BuildDemo();

            Stopwatch sw = new Stopwatch();
            {
                sw.Reset();
                sw.Start();
                storage.LoadStructure("Test", StorageLocations.Local, SerializingFormat.Xml);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            textBox1.Text = storage["Test"].ToString(false);
            textBox2.Text = storage["Test"].ToString(true);
        }

        /// <summary>
        /// Event handler. Called by btnSaveData for click events.
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btnSaveData_Click(object sender, EventArgs e)
        {
            BuildDemo();

            if (storage.Connected)
            {
                storage.SaveData("Wiki", StorageLocations.Server, SerializingFormat.Json);
            }
            else
            {
                MessageBox.Show("Not Connected");
            }
        }

        /// <summary>
        /// Event handler. Called by btnSaveLoadStructure for click events.
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btnSaveLoadStructure_Click(object sender, EventArgs e)
        {
            BuildDemo();

            if (storage.Connected)
            {
                foreach (KeyValuePair<String, Node> kvp in storage)
                {
                    //! Structure + Data.
                    //
                    textBox1.Text = storage[kvp.Key].ToString(false);

                    storage.DeleteStructure(kvp.Key, StorageLocations.Server);

                    storage.SaveStructure(kvp.Key, StorageLocations.Server, SerializingFormat.Json);
                    storage[kvp.Key].Clear();
                    storage.LoadStructure(kvp.Key, StorageLocations.Server, SerializingFormat.Json);

                    //! Structure Only.
                    //
                    textBox2.Text = storage[kvp.Key].ToString(false);
                }
            }
            else
            {
                MessageBox.Show("Not Connected");
            }
        }

        /// <summary>
        /// Event handler. Called by btnSaveStructure for click events.
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btnSaveStructure_Click(object sender, EventArgs e)
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
                textBox1.Text = storage["Test"].ToString(false);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            //GameStorageAsset x = usermodel.ResolveStorage("Hints.Test.Age");

            //Object value = usermodel.ResolveValue(("Hints.Test.Age"));
            {
                sw.Reset();
                sw.Start();
                storage.SaveStructure("Test", StorageLocations.Local, SerializingFormat.Xml);
                sw.Stop();
                Debug.Print("Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            Debug.Print("{0}={1}", "Hints.Test.Age", storage["User"]["Age"]);
        }

        /// <summary>
        /// Event handler. Called by btnVirtual for click events.
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btnVirtual_Click(object sender, EventArgs e)
        {
            BuildDemo();

            textBox1.Text += storage["User"]["Virtual"].Value.ToString();
        }

        /// <summary>
        /// Builds the demo.
        /// </summary>
        private void BuildDemo()
        {
            textBox1.Clear();
            textBox2.Clear();

            storage["Hint"].Clear();
            storage["User"].Clear();
            storage["Test"].Clear();
            storage["Wiki"].Clear();

            storage.RegisterTypes(new Type[] {
                typeof(DemoClass),
                typeof(List<byte>),
                typeof(List<string>),
            });

            //#warning todo pretty print generic types?

            // See https://msdn.microsoft.com/en-us/library/windows/apps/system.type.makegenerictype(v=vs.105).aspx
            // See https://msdn.microsoft.com/en-us/library/windows/apps/system.type.getgenerictypedefinition(v=vs.105).aspx
            //
            // Type.MakeGenericType
            // Type.GetGenericArguments
            //
            // Type t = Type.GetType("System.Collections.Generic.Dictionary`2[System.String,System.Object]");

            // 0) Complete payload.
            storage["User"].AddChild("STRUCT", new DemoClass
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
            B.AddChild("A1", new DemoClass
            {
                a = 15,
                b = "vijftien",
                c = DateTime.Now
            });
            B.AddChild("A2", 42);

            //for (Int32 i = 0; i < 100; i++)
            //{
            //    B.AddChild(String.Format("A{0:000}", i), i);
            //}

            //B.Value = data;

            Node D = B.AddChild("D", DateTime.Now, StorageLocations.Server);
            D.AddChild("C", new DemoClass
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

        #endregion Methods

        #region Nested Types

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

        /// <summary>
        /// See http://www.codeproject.com/KB/trace/TextBoxTraceListener.aspx.
        /// </summary>
        public class TextBoxTraceListener : TraceListener
        {
            #region Fields

            /// <summary>
            /// The invoke write.
            /// </summary>
            private StringSendDelegate fInvokeWrite;

            /// <summary>
            /// Target for the Writer.
            /// </summary>
            private TextBox fTarget;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the Swiss.DebugForm.NextGridTraceListener
            /// class.
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

        /// <summary>
        /// Event handler. Called by button1 for click events.
        /// </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        private void button1_Click(object sender, EventArgs e)
        {
            BuildDemo();

            Stopwatch sw = new Stopwatch();
            {
                sw.Reset();
                sw.Start();
                storage.SaveData("Wiki", StorageLocations.Local, SerializingFormat.Json);
                sw.Stop();
                Debug.Print("JSon serialize Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            textBox1.Text = storage["Wiki"].ToString(false);

            storage["Wiki"]["F"].Value = 42;
            {
                sw.Reset();
                sw.Start();
                storage.LoadData("Wiki", StorageLocations.Local, SerializingFormat.Json);
                sw.Stop();
                Debug.Print("JSon deserialize Elapsed: {0} ms", sw.ElapsedMilliseconds);
            }

            textBox2.Text = storage["Wiki"].ToString(false);
        }
    }
}