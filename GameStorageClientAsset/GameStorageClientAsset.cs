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
namespace AssetPackage
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Text.RegularExpressions;

    //! Not sure if this needs to stay here. Better is using Dictionary of Models instead of nesting them.
    //! Make Models keys case-insesitive?
    //
    //! Somewhat more utility methods like Remove, Clear!
    //
    //[XmlRoot]
    public class GameStorageClientAsset : BaseAsset, /*IWebServiceResponse,*/ IEnumerable
    {
        #region Fields

        /// <summary>
        /// Filename of the settings file.
        /// </summary>
        const String SettingsFileName = "GameStorageClientAssetSettings.xml";

        /// <summary>
        /// The TimeStamp Format.
        /// </summary>
        private const string TimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// The RegEx to extract a JSON Object. Used to extract 'actor'.
        /// </summary>
        ///
        /// <remarks>
        /// NOTE: This regex handles matching brackets by using balancing groups. This should be tested in Mono if it works there too.<br />
        /// NOTE: {} brackets must be escaped as {{ and }} for String.Format statements.<br />
        /// NOTE: \ must be escaped as \\ in strings.<br />
        /// 
        /// TEST "Value":(?:\s?)(\{(?>[^{}]+|\{(?<number>)|\}(?<-number>))*(?(number)(?!))\}) (with spaces after the :)
        /// </remarks>
        private const string ObjectRegEx =
            "\"{0}\":(" +                   // {0} is replaced by the proprty name, capture only its value in {} brackets.
            "\\{{" +                        // Start with a opening brackets.
            "(?>" +
            "    [^{{}}]+" +                // Capture each non bracket chracter.
            "    |    \\{{ (?<number>)" +   // +1 for opening bracket.
            "    |    \\}} (?<-number>)" +  // -1 for closing bracket.
            ")*" +
            "(?(number)(?!))" +             // Handle unaccounted left brackets with a fail.
            "\\}})"; // Stop at matching bracket.

        /// <summary>
        /// The RegEx to extract a plain quoted JSON Value. Used to extract 'token'.
        /// </summary>
        private const string TokenRegEx = "\"{0}\":\"(.+?)\"";

        /// <summary>
        /// A Regex to extact the authentication token value from JSON.
        /// </summary>
        private Regex jsonAuthToken = new Regex(String.Format(TokenRegEx, "authToken"), RegexOptions.Singleline);

        /// <summary>
        /// A Regex to extact the token value from JSON.
        /// </summary>
        private Regex jsonToken = new Regex(String.Format(TokenRegEx, "token"), RegexOptions.Singleline);

        /// <summary>
        /// A Regex to extact the structure value from JSON.
        /// </summary>
        private Regex jsonStructure = new Regex(String.Format(TokenRegEx, "structure"), RegexOptions.Singleline);

        /// <summary>
        /// A Regex to extact the status value from JSON.
        /// </summary>
        private Regex jsonHealth = new Regex(String.Format(TokenRegEx, "status"), RegexOptions.Singleline);

        /// <summary>
        /// A Regex to extact the message value from JSON.
        /// </summary>
        private Regex jsonMessage = new Regex(String.Format(TokenRegEx, "message"), RegexOptions.Singleline);

        /// <summary>
        /// The JSON value.
        /// </summary>
        private const String jsonValueRegEx = "\"Value\":(?:\\s?)(\\{(?>[^{}]+|\\{(?<number>)|\\}(?<-number>))*(?(number)(?!))\\})";

        private Regex jsonValue = new Regex(jsonValueRegEx, RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

        private const String jsonArrayRegEx = "\"Value\":(?:\\s?)(\\[(?>[^\\[\\]]+|\\[(?<number>)|\\](?<-number>))*(?(number)(?!))\\])";

        private Regex jsonArray = new Regex(jsonArrayRegEx, RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private GameStorageClientAssetSettings settings = null;

        private Dictionary<SerializingFormat, String> prefixes = new Dictionary<SerializingFormat, string>();
        private Dictionary<SerializingFormat, String> separators = new Dictionary<SerializingFormat, string>();
        private Dictionary<SerializingFormat, String> suffixes = new Dictionary<SerializingFormat, string>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the AssetPackage.GameStorageClientAsset class.
        /// </summary>
        public GameStorageClientAsset()
            : base()
        {
            Types = new Dictionary<String, Type>();

            Types.Add(typeof(Byte).FullName, typeof(Byte));
            Types.Add(typeof(Int32).FullName, typeof(Int32));
            Types.Add(typeof(String).FullName, typeof(String));
            Types.Add(typeof(Double).FullName, typeof(Double));
            Types.Add(typeof(DateTime).FullName, typeof(DateTime));

            settings = new GameStorageClientAssetSettings();

            prefixes.Add(SerializingFormat.Json, "{ \"nodes\" : [");
            prefixes.Add(SerializingFormat.Xml, "<Nodes>");

            separators.Add(SerializingFormat.Json, ",");

            suffixes.Add(SerializingFormat.Json, "] }");
            suffixes.Add(SerializingFormat.Xml, "</Nodes>");

            if (LoadSettings(SettingsFileName))
            {
                // ok
            }
            else
            {
                settings.Secure = false;
                settings.Host = "127.0.0.1";
                settings.Port = 3000;
                settings.BasePath = "/api/";

                settings.UserToken = "a:";

                SaveSettings(SettingsFileName);
            }

            Models = new Dictionary<String, Node>();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the connection active (ie the ActorObject
        /// and ObjectId have been extracted).
        /// </summary>
        ///
        /// <value>
        /// true if active, false if not.
        /// </value>
        public Boolean Active { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the connected (ie a UserToken is present and no Fail() has occurred).
        /// </summary>
        ///
        /// <value>
        /// true if connected, false if not.
        /// </value>
        public Boolean Connected { get; private set; }

        /// <summary>
        /// Gets the health.
        /// </summary>
        ///
        /// <value>
        /// The health.
        /// </value>
        public String Health { get; private set; }

        /// <summary>
        /// Gets the number of Model Keys. 
        /// </summary>
        ///
        /// <value>
        /// The count.
        /// </value>
        public Int32 Count
        {
            get
            {
                return Models.Keys.Count;
            }
        }

        /// <summary>
        /// Gets or sets options for controlling the operation.
        /// </summary>
        ///
        /// <remarks>   Besides the toXml() and fromXml() methods, we never use this property but use
        ///                it's correctly typed backing field 'settings' instead. </remarks>
        /// <remarks> This property should go into each asset having Settings of its own. </remarks>
        /// <remarks>   The actual class used should be derived from BaseAsset (and not directly from
        ///             ISetting). </remarks>
        ///
        /// <value>
        /// The settings.
        /// </value>
        public override ISettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = (value as GameStorageClientAssetSettings);
            }
        }

        /// <summary>
        /// Storage of Multiple Models.
        /// </summary>
        ///
        /// <value>
        /// The models.
        /// </value>
        private Dictionary<String, Node> Models
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        ///
        /// <value>
        /// The types.
        /// </value>
        public Dictionary<String, Type> Types
        {
            get;
            set;
        }

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Indexer to get items within this collection using array index syntax.
        /// </summary>
        ///
        /// <param name="name"> The name. </param>
        ///
        /// <returns>
        /// The indexed item.
        /// </returns>
        public Node this[string name]
        {
            get
            {
                return Models[name];
            }
            set
            {
                Models[name] = value;
            }
        }

        /// <summary>
        /// Indexer to get items within this collection using array index syntax.
        /// </summary>
        ///
        /// <param name="i"> The key index. </param>
        ///
        /// <returns>
        /// The indexed item.
        /// </returns>
        public Node this[Int32 i]
        {
            get
            {
                return Models[Models.Keys.ElementAt(i)];
            }
            set
            {
                Models[Models.Keys.ElementAt(i)] = value;
            }
        }

        #endregion Indexers

        #region Methods

        public void RegisterTypes(Type[] types)
        {
            foreach (Type type in types)
            {
                if (!Types.ContainsKey(type.FullName))
                {
                    Types.Add(type.FullName, type);
                }
            }
        }

        /// <summary>
        /// Adds a model.
        /// </summary>
        ///
        /// <param name="purpose"> The purpose. </param>
        public Node AddModel(String purpose)
        {
            Models.Add(purpose, new Node(this, purpose));

            return Models[purpose];
        }

        /// <summary>
        /// Checks the health of the UCM Tracker.
        /// </summary>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public Boolean CheckHealth()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            headers.Add("Content-Type", "application/json");
            headers.Add("Accept", "application/json");

            RequestResponse response = IssueRequest2("health", "GET", headers, String.Empty);

            if (response.ResultAllowed)
            {
                if (jsonHealth.IsMatch(response.body))
                {
                    Health = jsonHealth.Match(response.body).Groups[1].Value;

                    Log(Severity.Information, "Health Status={0}", Health);
                }
            }
            else
            {
                Log(Severity.Error, "Request Error: {0}-{1}", response.responseCode, response.responsMessage);
            }

            return response.ResultAllowed;
        }

        /// <summary>
        /// Issue a HTTP Webrequest.
        /// </summary>
        ///
        /// <param name="path">   Full pathname of the file. </param>
        /// <param name="method"> The method. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private RequestResponse IssueRequest2(string path, string method)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            headers.Add("Content-Type", "application/json");
            headers.Add("Accept", "application/json");

            return IssueRequest2(path, method, new Dictionary<string, string>(), String.Empty);
        }

        /// <summary>
        /// Issue a HTTP Webrequest.
        /// </summary>
        ///
        /// <param name="path">    Full pathname of the file. </param>
        /// <param name="method">  The method. </param>
        /// <param name="headers"> The headers. </param>
        /// <param name="body">    The body. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private RequestResponse IssueRequest2(string path, string method, Dictionary<string, string> headers, string body = "")
        {
            return IssueRequest2(path, method, headers, body, settings.Port);
        }

        /// <summary>
        /// Query if this object issue request 2.
        /// </summary>
        ///
        /// <param name="path">    Full pathname of the file. </param>
        /// <param name="method">  The method. </param>
        /// <param name="headers"> The headers. </param>
        /// <param name="body">    The body. </param>
        /// <param name="port">    The port. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        private RequestResponse IssueRequest2(string path, string method, Dictionary<string, string> headers, string body, Int32 port)
        {
            IWebServiceRequest2 ds = getInterface<IWebServiceRequest2>();

            RequestResponse response = new RequestResponse();

            if (ds != null)
            {
                ds.WebServiceRequest(
                   new RequestSetttings
                   {
                       method = method,
                       uri = new Uri(string.Format("http{0}://{1}{2}{3}/{4}",
                                   settings.Secure ? "s" : String.Empty,
                                   settings.Host,
                                   port == 80 ? String.Empty : String.Format(":{0}", port),
                                   String.IsNullOrEmpty(settings.BasePath.TrimEnd('/')) ? "" : settings.BasePath.TrimEnd('/'),
                                   path.TrimStart('/')
                                   )),
                       requestHeaders = headers,
                       //! allowedResponsCodes,     // TODO default is ok
                       body = body, // or method.Equals("GET")?string.Empty:body
                   }, out response);
            }

            return response;
        }

        /// <summary>
        /// Gets a type.
        /// </summary>
        ///
        /// <param name="typename"> The type. </param>
        ///
        /// <returns>
        /// The type.
        /// </returns>
        private Type LookuptType(String typename)
        {
            if (!Types.ContainsKey(typename))
            {
                Debug.Print("Caching Type FullName for {0}", typename);

                Type type = Type.GetType(typename);

                Types.Add(type.FullName, type);
            }

            return Types[typename];
        }

        /// <summary>
        /// Loads a structure.
        /// </summary>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public Boolean LoadStructure(String model, StorageLocations location)
        {
            //! TODO Add binary parameter.
            //
            switch (location)
            {
                case StorageLocations.Local:
                    IDataStorage storage = getInterface<IDataStorage>();

                    if (storage != null)
                    {
                        if (storage.Exists(model + ".xml"))
                        {
                            if (Models.ContainsKey(model))
                            {
#warning bit rough to clear all data, better to clear only those matching the location.
                                Models[model].Clear();
                            }
                            else
                            {
                                Models[model] = new Node(this, model);
                            }

                            Models[model].FromXml(storage.Load(model + ".xml"));

                            return true;
                        }
                    }
                    else
                    {
                        Debug.Print("IDataStorage interface not found a Bridge");
                    }
                    break;

                case StorageLocations.Server:
                    {
                        if (Connected)
                        {
                            Dictionary<string, string> headers = new Dictionary<string, string>();

                            //headers["Content-Type"] = "application/json";
                            headers["Accept"] = "application/json";
                            headers["Authorization"] = String.Format("Bearer {0}", settings.UserToken);

                            RequestResponse response = IssueRequest2(
                                String.Format("storage/model/{0}", model),
                                "GET", headers, String.Empty, settings.Port);

                            if (response.ResultAllowed)
                            {
                                if (jsonStructure.IsMatch(response.body))
                                {
                                    String base64 = jsonStructure.Match(response.body).Groups[1].Value;

                                    this[model].FromBinary(base64, true);

                                    Log(Severity.Information, "Structure of Model[{0}] is Restored", model);

                                    return true;
                                }
                            }
                            else
                            {
                                Debug.Print("Problem restoring structure from the GameStorage Server.");
                            }
                        }
                        else
                        {
                            Debug.Print("Not connected to the GameStorage Server.");
                        }
                    }
                    break;

                default:
                    Debug.Print("Not implemented yet");
                    break;
            }

            return false;
        }

        public Boolean LoadData(String model, StorageLocations location, SerializingFormat format)
        {
            //! TODO Add binary parameter.
            //
            switch (location)
            {
                case StorageLocations.Local:
                    IDataStorage storage = getInterface<IDataStorage>();

                    if (storage != null)
                    {
                        switch (format)
                        {
                            case SerializingFormat.Xml:
                                if (storage.Exists(model + ".xml"))
                                {
                                    if (Models.ContainsKey(model))
                                    {
                                        Models[model].ClearData(location);
                                    }
                                    else
                                    {
                                        Models[model] = new Node(this, model);
                                    }

                                    Models[model].FromXml(storage.Load(model + ".xml"));

                                    return true;
                                }
                                break;

                            case SerializingFormat.Json:
                                if (storage.Exists(model + ".json"))
                                {
                                    if (Models.ContainsKey(model))
                                    {
                                        Models[model].ClearData(location);
                                    }
                                    else
                                    {
                                        Models[model] = new Node(this, model);
                                    }

                                    string json = storage.Load(model + ".json");

                                    DeSerializeData(model, json, location, format);

                                    return true;
                                }
                                break;

                            case SerializingFormat.Binary:
                                // TODO Implement 
                                break;
                        }
                    }
                    else
                    {
                        Debug.Print("IDataStorage interface not found a Bridge");
                    }
                    break;

                case StorageLocations.Server:
                    {
                        if (Connected)
                        {
                            Dictionary<string, string> headers = new Dictionary<string, string>();

                            headers["Accept"] = "application/json";
                            headers["Authorization"] = String.Format("Bearer {0}", settings.UserToken);

                            RequestResponse response = IssueRequest2(
                                String.Format("storage/data/{0}", model),
                                "GET", headers, String.Empty, settings.Port);

                            if (response.ResultAllowed)
                            {
                                //! Clear data of nodes to be restored.
                                // 
                                Models[model].ClearData(location);

                                //! Deserialize data.
                                // 
                                DeSerializeData(model, response.body, location, format);
                            }
                            else
                            {
                                Debug.Print("Problem restoring data from the GameStorage Server.");
                            }
                        }
                        else
                        {
                            Debug.Print("Not connected to the GameStorage Server.");
                        }
                    }
                    break;

                default:
                    Debug.Print("Not implemented yet");
                    break;
            }

            return false;
        }

        /// <summary>
        /// Login with a Username and Password.
        ///
        /// After this call, the Success method will extract the token from the returned .
        /// </summary>
        ///
        /// <param name="username"> The username. </param>
        /// <param name="password"> The password. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public Boolean Login(string username, string password)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            headers.Add("Content-Type", "application/json");
            headers.Add("Accept", "application/json");

            //return IssueRequest("login", "POST", headers,
            //    String.Format("{{\r\n \"username\": \"{0}\",\r\n \"password\": \"{1}\"\r\n}}",
            //    username, password), settings.A2Port);

            RequestResponse response = IssueRequest2("login", "POST", headers,
                String.Format("{{\r\n \"username\": \"{0}\",\r\n \"password\": \"{1}\"\r\n}}",
                username, password), settings.A2Port);

            if (response.ResultAllowed)
            {
                if (jsonToken.IsMatch(response.body))
                {
                    settings.UserToken = jsonToken.Match(response.body).Groups[1].Value;
                    if (settings.UserToken.StartsWith("Bearer "))
                    {
                        settings.UserToken.Remove(0, "Bearer ".Length);
                    }
                    Log(Severity.Information, "Token= {0}", settings.UserToken);

                    Connected = true;
                }
            }
            else
            {
                Log(Severity.Error, "Request Error: {0}-{1}", response.responseCode, response.responsMessage);

                Connected = false;
            }

            return Connected;
        }

        /// <summary>
        /// Saves a data.
        /// </summary>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        /// <param name="format">   Describes the format to use. </param>
        public void SaveData(String model, StorageLocations location, SerializingFormat format)
        {
            if (Models.ContainsKey(model))
            {
                ISerializer serializer = getInterface<ISerializer>();

                if (serializer != null && serializer.Supports(format))
                {
                    switch (location)
                    {
                        case StorageLocations.Local:
                            {
                                IDataStorage storage = getInterface<IDataStorage>();

                                if (storage != null)
                                {
                                    storage.Save(model + ".json", SerializeData(model, location, format));
                                }
                                else
                                {
                                    Debug.Print("IDataStorage interface not found a Bridge");
                                }
                            }
                            break;

                        case StorageLocations.Server:
                            {
                                Dictionary<string, string> headers = new Dictionary<string, string>();

                                headers.Add("Content-Type", "application/json");
                                headers.Add("Accept", "application/json");
                                headers.Add("Authorization", String.Format("Bearer {0}", settings.UserToken));

                                //#error Format for mongo is not correct. should be "path": "value"

                                String json = SerializeData(model, location, format);

                                RequestResponse response = IssueRequest2(
                                            String.Format("storage/data/{0}", model),
                                            "PUT",
                                            headers,
                                            json,
                                            (Settings as GameStorageClientAssetSettings).Port);

                                Debug.Print(response.body);
                            }
                            break;

                        default:
                            Debug.Print("Not implemented yet");
                            break;
                    }
                }
                else
                {
                    Debug.Print(String.Format("ISerializer interface for {0} not found a Bridge", format));
                }
            }
            else
            {
                Debug.Print("Model not found");
            }
        }

        /// <summary>
        /// Serialize data.
        /// </summary>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        /// <param name="format">   Describes the format to use. </param>
        ///
        /// <returns>
        /// A String.
        /// </returns>
        private String SerializeData(String model, StorageLocations location, SerializingFormat format)
        {
            if (Models.ContainsKey(model))
            {
                ISerializer serializer = getInterface<ISerializer>();

                if (serializer != null && serializer.Supports(format))
                {
                    return Serialize(serializer, Models[model], location, format);
                }
                else
                {
                    //! Try Default one for xml and binary.
                    // 
                    Debug.Print(String.Format("ISerializer interface for {0} not found a Bridge", format));
                }
            }
            else
            {
                Debug.Print("Model not found");
            }

            return String.Empty;
        }

        /// <summary>
        /// Serialize this object to the given stream.
        /// </summary>
        ///
        /// <param name="serializer"> The serializer. </param>
        /// <param name="root">       The root. </param>
        /// <param name="location">   The location. </param>
        /// <param name="format">     Describes the format to use. </param>
        ///
        /// <returns>
        /// A String.
        /// </returns>
        private String Serialize(ISerializer serializer, Node root, StorageLocations location, SerializingFormat format)
        {
            StringBuilder serialized = new StringBuilder();

            //! 1) Open array.
            //
            if (prefixes.ContainsKey(format))
            {
                serialized.AppendLine(prefixes[format]);
            }

            Type pocValueType = typeof(PocValue<>);

            //! 2) Enumerate all nodes to be save to the specified location. 
            // 
            foreach (Node node in root.PrefixEnumerator(new List<StorageLocations> { location }))
            {
#warning TODO: Not optimal location for nodePoc & nt inside the loop, but usefull during debugging.
                Type nt = node.Value.GetType();

                //Type poc = typeof(NodePocOut<>);
                //BaseNodePoc nodePoc = (BaseNodePoc)Activator.CreateInstance(
                //    poc.MakeGenericType(nt),
                //    new object[] { node.Path.Replace('.', '|'), node.Value });

                // String tmp2 = serializer.Serialize(nodePoc, format);

                /*
                // was node.Value;
                //nodePoc.Path = node.Path.Replace('.', '|');

                //! 3) If class, it was serialize as string 
                //!    Note: Structs (other then internally handled internally by the serializer i.e. Guid, DataTime) are not supported yet.
                //!    Note: Code should be improved (there should be no data in it that is not marker with a [Serializable] attribute).
                if (node.Value is String)
                {
                    nodePoc.Value = node.Value.ToString();
                }
                else if (node.Value is DateTime)
                {
                    nodePoc.Value = ((DateTime)(node.Value)).ToString("O");
                }
                else if (nt.IsClass && nt.IsSerializable)
                {
                    //https://msdn.microsoft.com/en-us/library/system.type.makegenerictype%28v=vs.110%29.aspx
                    //Type helper = typeof(Helper<>);
                    //Type tmp = helper.MakeGenericType(new Type[] { nt });
                    //Helper<nt> o = new Helper(node.Value); //(Helper)Activator.CreateInstance(tmp);
                    //o.Value = node.Value;
                    //nodePoc.Value.Value = serializer.Serialize(o, format); // was node.Value;
                    // 

                    //! The problem here is that Unity3D's serializier looks at the type so 
                    //  won't serialize an Object field, no matter the content.

                    nodePoc.Value = serializer.Serialize(node.Value, format); // was node.Value;
                    string tmp0 = serializer.Serialize(node, format);

                    IBaseHelper h = new Helper<int>(45);
                    string tmp1 = serializer.Serialize(h, format);

                    //using (MemoryStream ms = new MemoryStream())
                    //{
                    //    new BinaryFormatter().Serialize(ms, node.Value);
                    //    ms.Flush();

                    //    nodePoc.Value.Value = Convert.ToBase64String(ms.ToArray());
                    //}
                }
                else
                {
                    nodePoc.Value = node.Value.ToString();
                }

                nodePoc.ValueType = node.Value.GetType().FullName;
                */

                String json = String.Empty;

                IPocValue nodePocs;

                //! 3) Adjust value to a String for Classes (not being a string).
                // xx
                if (!(node.Value is String) && nt.IsClass && nt.IsSerializable && !nt.IsArray)
                {
                    //! Serialize Classes except Strings.
                    //
                    //! Serializes as a Json Class (not a String).
                    //! So we need to convert later. 
                    nodePocs = (IPocValue)Activator.CreateInstance(pocValueType.MakeGenericType(nt));
                    nodePocs.SetValue(node.Value);

                    //"{\r\n  \"a\": 15,\r\n  \"b\": \"vijftien\",\r\n  \"c\": \"2016-04-21T00:05:04.4571539+02:00\"\r\n}",
                    // 
                    //! versus:
                    // 
                    //{
                    //"a": 15,
                    //"b": "vijftien",
                    //"c": "2016-04-21T00:00:33.4479899+02:00"
                    //}
                }
                else if (nt.IsArray)
                {
                    //! Serializes as a Json Array (not a String).
                    //! So we need to convert later. 

                    MethodInfo methodInfo = typeof(Enumerable).GetMethod("ToList");
                    MethodInfo method = methodInfo.MakeGenericMethod(new Type[] { nt.GetElementType() });

                    Type listType = typeof(List<>).MakeGenericType(new Type[] { nt.GetElementType() });
                    nodePocs = (IPocValue)Activator.CreateInstance(pocValueType.MakeGenericType(listType));

                    nodePocs.SetValue(method.Invoke(null, new Object[] { node.Value }));

                    //"[\r\n  1,\r\n  2,\r\n  3,\r\n  4,\r\n  5\r\n]"
                    // versus:
                    //[1,2,3,4,5]
                }
                else if (nt.IsPrimitive)
                {
                    //! The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
                    // 
                    nodePocs = new PocValue<String>();

                    nodePocs.SetValue(node.Value.ToString());
                }
                else if (node.Value is DateTime)
                {
                    nodePocs = new PocValue<String>();

                    nodePocs.SetValue(((DateTime)(node.Value)).ToString("O"));
                }
                else
                {
                    nodePocs = new PocValue<String>();

                    nodePocs.SetValue(node.Value.ToString());
                }

                nodePocs.SetPath(node.Path);
                nodePocs.SetValueType(nt.FullName);

                json = serializer.Serialize(nodePocs, format);

                //! Fixups for classes and arrays.
                // 
                if (jsonValue.IsMatch(json))
                {
                    Match m = jsonValue.Match(json);

                    //m.Index, m.Length
                    String cls = m.Groups[1].Value;

                    cls = cls.Replace("\r", "\\r");
                    cls = cls.Replace("\n", "\\n");
                    cls = cls.Replace("\"", "\\\"");

                    json = json.Remove(m.Index, m.Length);
                    json = json.Insert(m.Index, String.Format("\"Value\": \"{0}\"", cls));
                }
                else if (jsonArray.IsMatch(json))
                {
                    Match m = jsonArray.Match(json);

                    //m.Index, m.Length
                    String cls = m.Groups[1].Value;

                    cls = cls.Replace("\r", "\\r");
                    cls = cls.Replace("\n", "\\n");
                    cls = cls.Replace("\"", "\\\"");

                    json = json.Remove(m.Index, m.Length);
                    json = json.Insert(m.Index, String.Format("\"Value\": \"{0}\"", cls));
                }
                serialized.Append(String.Format("{0}", json));

                //! 5) Write separator if any.
                // 
                if (separators.ContainsKey(format))
                {
                    serialized.AppendLine(separators[format]);
                }
                else
                {
                    serialized.AppendLine(String.Empty);
                }
            }

            //! 6) Json Fixup: Trim trailing ','.
            // 
            if (format == SerializingFormat.Json && separators.ContainsKey(format))
            {
                Int32 ndx = serialized.ToString().LastIndexOf(separators[format]);
                if (ndx != -1)
                {
                    serialized.Length = ndx;
                    serialized.AppendLine(String.Empty);
                }
            }

            //! 7) Close array.
            // 
            if (suffixes.ContainsKey(format))
            {
                serialized.AppendLine(suffixes[format]);
            }

            return serialized.ToString();
        }

        //public interface IBaseHelper
        //{
        //}

        //[Serializable]
        //public struct Helper<T> : IBaseHelper
        //{
        //    public Helper(T Value)
        //    {
        //        this.Value = Value;
        //    }

        //    public T Value;
        //}

        /// <summary>
        /// Deserialize this object to the given stream.
        /// </summary>
        ///
        /// <param name="serializer"> The serializer. </param>
        /// <param name="root">       The root. </param>
        /// <param name="data">       The data. </param>
        /// <param name="location">   The location. </param>
        /// <param name="format">     Describes the format to use. </param>
        private void Deserialize(ISerializer serializer, Node root, String data, StorageLocations location, SerializingFormat format)
        {
            PocStringValues pv = new PocStringValues();
            pv.nodes = new PocStringValue[] { new PocStringValue { Path = "aa", Value = "bb", ValueType = "cc" } };
            String test = serializer.Serialize(pv, SerializingFormat.Json);

            PocStringValues nodes1 = (PocStringValues)serializer.Deserialize<PocStringValues>(data, SerializingFormat.Json);

            // PocValues nodes = (PocValues)serializer.Deserialize<PocValues>(data, SerializingFormat.Json);

            //! 1) Enumerate all deserialized nodes.
            // 
            for (Int32 i = 0; i < nodes1.nodes.Length; i++)
            {
                PocStringValue poc = nodes1.nodes[i];

                //! 2) Problem, in Unity all serialized Value.Value's are empty (probably due to the object type in nodepoc).
                //!             so we now use 2 fields, Value for serializing and object for deserializing.
                //!             also the generic parameter is not that easy to handle as we have only a type.
                //!             We need to create a suitable generic method for custom types.

                //! Type's FullName works (even without the extra's between the [[ ]].
                // 
                //! Type.GetType("System.Collections.Generic.List`1[[System.String]]")
                //! instead of "System.Collections.Generic.List`1[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"
                // 
                //! Better make them numeric (smaller) and preregister the most common (IsPrimitive) ones.

                //! 3) Check if type was registered.
                // 
                //Type nt = node.Value.GetType();
                Type tt = LookuptType(poc.ValueType);

                // This does not change the node in nodes.
                // 
                poc.Path = poc.Path.Replace('|', '.');

                // The "null" is because it's a static method
                PocObjectValue fixedpoc = new PocObjectValue();

                if (poc.Value.ToString().StartsWith("{"))
                {
                    //! Create a Generic Method to call the Deserializer.
                    MethodInfo method = serializer.GetType().GetMethod("Deserialize", new Type[] { typeof(String), typeof(SerializingFormat) });
                    method = method.MakeGenericMethod(tt);

                    fixedpoc.ValueAsObject = method.Invoke(serializer, new Object[] { poc.Value, format });
                }
                else if (poc.Value.ToString().StartsWith("["))
                {
                    //! Create a Generic Class to Serialize the Value into.
                    Type pocType = typeof(PocValue<>);
                    IPocValue nodePoc = (IPocValue)Activator.CreateInstance(pocType.MakeGenericType(tt));

                    //! Create a Generic Method to call the Deserializer.
                    MethodInfo method = serializer.GetType().GetMethod("Deserialize", new Type[] { typeof(String), typeof(SerializingFormat) });
                    method = method.MakeGenericMethod(nodePoc.GetType());

                    //! Deserialize into the Generic Class and extract the Value.
                    String s = String.Format("{{ \"Value\": {0} }}", poc.Value);
                    nodePoc = (IPocValue)method.Invoke(serializer, new Object[] { s, format });
                    fixedpoc.ValueAsObject = nodePoc.GetValue();
                }
                else
                {
                    //! Fallback is using Convert.ChangeType (for Primitive types for example).
                    fixedpoc.ValueAsObject = Convert.ChangeType(poc.Value, tt);
                }

                //! Update Tree by path (not very optimized yet).
                // 
                Node n = root.PrefixEnumerator(new List<StorageLocations> { location }).FirstOrDefault(p => p.Path.Equals(poc.Path));
                if (n != null)
                {
                    n.Value = fixedpoc.ValueAsObject;
                }
            }
        }

        private void DeSerializeData(String model, String data, StorageLocations location, SerializingFormat format)
        {
            if (Models.ContainsKey(model))
            {
                ISerializer serializer = getInterface<ISerializer>();

                if (serializer != null && serializer.Supports(format))
                {
                    Deserialize(serializer, Models[model], data, location, format);
                }
                else
                {
                    Debug.Print(String.Format("ISerializer interface for {0} not found a Bridge", format));
                }
            }
            else
            {
                Debug.Print("Model not found");
            }
        }


        /// <summary>
        /// Saves.
        /// </summary>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public Boolean SaveStructure(String model, StorageLocations location)
        {
            //! TODO Add binary parameter.
            //
            if (Models.ContainsKey(model))
            {
                switch (location)
                {
                    case StorageLocations.Local:
                        IDataStorage storage = getInterface<IDataStorage>();

                        if (storage != null)
                        {
                            storage.Save(model + ".xml", Models[model].ToXml());
                            return true;
                        }
                        else
                        {
                            Debug.Print("IDataStorage interface not found a Bridge");
                        }
                        break;

                    case StorageLocations.Server:
                        {
                            if (Connected)
                            {
                                Dictionary<string, string> headers = new Dictionary<string, string>();

                                headers["Content-Type"] = "application/json";
                                headers["Accept"] = "application/json";
                                headers["Authorization"] = String.Format("Bearer {0}", settings.UserToken);

                                RequestResponse response = IssueRequest2(
                                    String.Format("storage/model/{0}", model),
                                    "PUT", headers,
                                    String.Format("{{\r\n \"structure\": \"{0}\"}}",
                                    this[model].ToBinary(true)), settings.Port);

                                if (response.ResultAllowed)
                                {
                                    if (jsonMessage.IsMatch(response.body))
                                    {
                                        Log(Severity.Information, "Message={0}", jsonMessage.Match(response.body).Groups[1].Value);

                                        Log(Severity.Information, "Structure of Model[{0}] is Saved", model);
                                    }

                                    return true;
                                }
                                else
                                {
                                    Debug.Print("Problem persisting structure on the GameStorage Server.");
                                }
                            }
                            else
                            {
                                Debug.Print("Not connected to the GameStorage Server.");
                            }
                        }
                        break;

                    default:
                        Debug.Print("Not implemented yet");
                        break;
                }
            }
            else
            {
                Debug.Print("Model not found");
            }

            return false;
        }

        /// <summary>
        /// Deletes the Stucture.
        /// </summary>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public Boolean DeleteStructure(String model, StorageLocations location)
        {
            //! TODO Add binary parameter.
            //
            if (Models.ContainsKey(model))
            {
                switch (location)
                {
                    case StorageLocations.Local:
                        {
                            IDataStorage storage = getInterface<IDataStorage>();

                            if (storage != null)
                            {
                                return storage.Delete(model + ".xml");
                            }
                            else
                            {
                                Debug.Print("IDataStorage interface not found a Bridge");
                            }
                        }
                        break;

                    case StorageLocations.Server:
                        {
                            if (Connected)
                            {
                                Dictionary<string, string> headers = new Dictionary<string, string>();

                                headers["Content-Type"] = "application/json";
                                headers["Accept"] = "application/json";
                                headers["Authorization"] = String.Format("Bearer {0}", settings.UserToken);

                                RequestResponse response = IssueRequest2(
                                    String.Format("storage/model/{0}", model),
                                    "DELETE", headers, String.Empty, settings.Port);

                                if (response.ResultAllowed)
                                {
                                    if (jsonMessage.IsMatch(response.body))
                                    {
                                        Log(Severity.Information, "Message={0}", jsonMessage.Match(response.body).Groups[1].Value);
                                    }

                                    Log(Severity.Information, "Structure of Model[{0}] is Removed", model);

                                    return true;
                                }
                                else
                                {
                                    Debug.Print("Problem deleting structure on the GameStorage Server.");
                                }
                            }
                            else
                            {
                                Debug.Print("Not connected to the GameStorage Server.");
                            }
                        }
                        break;

                    default:
                        Debug.Print("Not implemented yet");
                        break;
                }
            }
            else
            {
                Debug.Print("Model not found");
            }

            return false;
        }

        /// <summary>
        /// Gets the interface.
        /// </summary>
        ///
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        ///
        /// <returns>
        /// The interface.
        /// </returns>
        internal T GetInterface<T>()
        {
            return base.getInterface<T>();
        }

        public IEnumerator GetEnumerator()
        {
            return Models.GetEnumerator();
        }

        #endregion Methods
    }
}