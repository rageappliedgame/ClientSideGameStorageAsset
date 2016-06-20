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
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Serialization;
    using AssetManagerPackage;

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
        private Dictionary<SerializingFormat, String> extensions = new Dictionary<SerializingFormat, string>();

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
            prefixes.Add(SerializingFormat.Xml, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<PocStringValues><nodes>");

            separators.Add(SerializingFormat.Json, ",");

            suffixes.Add(SerializingFormat.Json, "] }");
            suffixes.Add(SerializingFormat.Xml, "</nodes></PocStringValues>");

            extensions.Add(SerializingFormat.Json, ".json");
            extensions.Add(SerializingFormat.Xml, ".xml");
            extensions.Add(SerializingFormat.Binary, ".bin");

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
            IWebServiceRequest ds = getInterface<IWebServiceRequest>();

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
                Log(Severity.Verbose, "Caching Type FullName for {0}", typename);

                Type type = Type.GetType(typename);

                Types.Add(type.FullName, type);
            }

            return Types[typename];
        }

        /// <summary>
        /// Loads a models Structure.
        /// </summary>
        ///
        /// <remarks>
        /// Local serialization format is Xml, for the server the same Xml is base64
        /// encoded into Json as the server is MongoDB based without Xml support.
        /// </remarks>
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
                        if (storage.Exists(model + extensions[SerializingFormat.Xml]))
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

                            Models[model].FromXml(storage.Load(model + extensions[SerializingFormat.Xml]));

                            return true;
                        }
                    }
                    else
                    {
                        Log(Severity.Warning, "IDataStorage interface not found a Bridge");
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
                                    Byte[] bytes = Convert.FromBase64String(base64);
                                    String structure = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                                    this[model].FromXml(structure);
                                    //this[model].FromBinary(base64, true);

                                    Log(Severity.Information, "Structure of Model[{0}] is Restored", model);

                                    return true;
                                }
                            }
                            else
                            {
                                Log(Severity.Warning, "Problem restoring structure from the GameStorage Server.");
                            }
                        }
                        else
                        {
                            Log(Severity.Warning, "Not connected to the GameStorage Server.");
                        }
                    }
                    break;

                default:
                    Log(Severity.Warning, "Not implemented yet");
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
                            //if (storage.Exists(model + extensions[format]))
                            //{
                            //    if (Models.ContainsKey(model))
                            //    {
                            //        Models[model].ClearData(location);
                            //    }
                            //    else
                            //    {
                            //        Models[model] = new Node(this, model);
                            //    }

                            //    Models[model].FromXml(storage.Load(model + extensions[format]));

                            //    return true;
                            //}
                            //break;

                            case SerializingFormat.Json:
                                if (storage.Exists(model + extensions[format]))
                                {
                                    if (Models.ContainsKey(model))
                                    {
                                        Models[model].ClearData(location);
                                    }
                                    else
                                    {
                                        Models[model] = new Node(this, model);
                                    }

                                    string data = storage.Load(model + extensions[format]);

                                    DeSerializeData(model, data, location, format);

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
                        Log(Severity.Warning, "IDataStorage interface not found a Bridge");
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
                                Log(Severity.Warning, "Problem restoring data from the GameStorage Server.");
                            }
                        }
                        else
                        {
                            Log(Severity.Warning, "Not connected to the GameStorage Server.");
                        }
                    }
                    break;

                default:
                    Log(Severity.Warning, "Not implemented yet");
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
                switch (location)
                {
                    case StorageLocations.Local:
                        {
                            IDataStorage storage = getInterface<IDataStorage>();

                            if (storage != null)
                            {
                                storage.Save(model + extensions[format], SerializeData(model, location, format));
                            }
                            else
                            {
                                Log(Severity.Warning, "IDataStorage interface not found a Bridge");
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

                            Log(Severity.Verbose, response.body);
                        }
                        break;

                    default:
                        Log(Severity.Warning, "Not implemented yet");
                        break;
                }
            }
            else
            {
                Log(Severity.Warning, "Model not found");
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
                    switch (format)
                    {
                        case SerializingFormat.Xml:
                            return Serialize(new InternalXmlSerializer(), Models[model], location, format);

                        default:
                            Log(Severity.Warning, String.Format("ISerializer interface for {0} not found a Bridge", format));
                            break;
                    };
                }
            }
            else
            {
                Log(Severity.Warning, "Model not found");
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

                String json = String.Empty;

                IPocValue nodePocs;

                //! 3) Adjust value to a String for Classes (not being a string).
                // xx
                if (!(node.Value is String) && nt.IsClassFix() && nt.IsSerializableFix() && !nt.IsArray)
                {
                    //! Serialize Classes except Strings.
                    //
                    //! Serializes as a Json Class (not a String).
                    //! So we need to convert later.
                    //! TODO Cache these?
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

                    MethodInfo methodInfo = typeof(Enumerable).MethodInfoFix("ToList");
                    MethodInfo method = methodInfo.MakeGenericMethod(new Type[] { nt.GetElementType() });

                    Type listType = typeof(List<>).MakeGenericType(new Type[] { nt.GetElementType() });
                    nodePocs = (IPocValue)Activator.CreateInstance(pocValueType.MakeGenericType(listType));

                    nodePocs.SetValue(method.Invoke(null, new Object[] { node.Value }));

                    //"[\r\n  1,\r\n  2,\r\n  3,\r\n  4,\r\n  5\r\n]"
                    // versus:
                    //[1,2,3,4,5]
                }
                else if (nt.IsPrimitiveFix())
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

                switch (format)
                {
                    case SerializingFormat.Json:
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

                        break;
                    case SerializingFormat.Xml:
                        //! Surround content of Value tag with <![CDATA[ and ]]>
                        //DecoderReplacementFallback contents
                        json = json.Replace("<Value>", "<Value><![CDATA[").Replace("</Value>", "]]></Value>");
                        break;
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
            //! Get a list of things to deserialize.
            // 
            PocStringValues sNodes = (PocStringValues)serializer.Deserialize<PocStringValues>(data, format);

            // PocValues nodes = (PocValues)serializer.Deserialize<PocValues>(data, SerializingFormat.Json);

            //! This works without types[] paramaters as there is only a single matching method.
            //
            MethodInfo method = serializer.GetType().MethodInfoFix("Deserialize" /*, new Type[] { typeof(String), typeof(SerializingFormat) }*/);

            //! Nicer but fails to compile on the <T> of p.Deserialize.
            // 
            //MethodInfo method1 = RageExtensions.GetMethodInfo<ISerializer>(p => p.Deserialize<T>(String.Empty, format));

            //! 1) Enumerate all deserialized nodes.
            // 
            for (Int32 i = 0; i < sNodes.nodes.Length; i++)
            {
                PocStringValue poc = sNodes.nodes[i];

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

#warning Json Specific Fixups Ahead!
                switch (format)
                {
                    case SerializingFormat.Json:
                        if (poc.Value.ToString().StartsWith("{"))
                        {
                            //! Create a Generic Method to call the Deserializer.
                            // 
                            MethodInfo genericMethod = method.MakeGenericMethod(tt);

                            //! Invoke the Deserializer.
                            // 
                            fixedpoc.ValueAsObject = genericMethod.Invoke(serializer, new Object[] { poc.Value, format });
                        }
                        else if (poc.Value.ToString().StartsWith("["))
                        {
                            //! Create a Generic Class to Serialize the Value into.
                            // 
                            Type pocType = typeof(PocValue<>);
                            IPocValue nodePoc = (IPocValue)Activator.CreateInstance(pocType.MakeGenericType(tt));

                            //! Create a Generic Method to call the Deserializer.
                            // 
                            MethodInfo genericMethod = method.MakeGenericMethod(nodePoc.GetType());

                            //! Deserialize into the Generic Class and extract the Value.
                            // 
                            String s = String.Format("{{ \"Value\": {0} }}", poc.Value);
                            nodePoc = (IPocValue)genericMethod.Invoke(serializer, new Object[] { s, format });
                            fixedpoc.ValueAsObject = nodePoc.GetValue();
                        }
                        else
                        {
                            //! Fallback is using Convert.ChangeType (for Primitive types for example).
                            //
                            fixedpoc.ValueAsObject = Convert.ChangeType(poc.Value, tt);
                        }
                        break;

                    case SerializingFormat.Xml:
#warning TODO
                        //TODO Copy Value into string togeter with ValueType.
                        break;
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
                    //! Try Default one for xml and binary.
                    // 
                    switch (format)
                    {
                        case SerializingFormat.Xml:
                            Deserialize(new InternalXmlSerializer(), Models[model], data, location, format);
                            break;
                        default:
                            Log(Severity.Warning, String.Format("ISerializer interface for {0} not found a Bridge", format));
                            break;
                    }
                }
            }
            else
            {
                Log(Severity.Warning, "Model not found");
            }
        }

        /// <summary>
        /// Saves the Model structure.
        /// </summary>
        ///
        /// <remarks>
        /// Local serialization format is Xml, for the server the same Xml is base64 encoded into
        /// Json as the server is MongoDB based without Xml support.
        /// </remarks>
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
                            storage.Save(model + extensions[SerializingFormat.Xml], Models[model].ToXml());

                            return true;
                        }
                        else
                        {
                            Log(Severity.Warning, "IDataStorage interface not found a Bridge");
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

                                //! Base64 Encode Xml Structure so we do not have issues with quotes ect.
                                // 
                                Byte[] structure = Encoding.UTF8.GetBytes(this[model].ToXml(true));
                                String data = String.Format("{{\r\n \"structure\": \"{0}\"}}", Convert.ToBase64String(structure));

                                //this[model].ToBinary(true))

                                RequestResponse response = IssueRequest2(
                                    String.Format("storage/model/{0}", model),
                                    "PUT",
                                    headers,
                                    data,
                                    settings.Port);

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
                                    Log(Severity.Warning, "Problem persisting structure on the GameStorage Server.");
                                }
                            }
                            else
                            {
                                Log(Severity.Warning, "Not connected to the GameStorage Server.");
                            }
                        }
                        break;

                    default:
                        Log(Severity.Warning, "Not implemented yet");
                        break;
                }
            }
            else
            {
                Log(Severity.Warning, "Model not found");
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
                                return storage.Delete(model + extensions[SerializingFormat.Xml]);
                            }
                            else
                            {
                                Log(Severity.Warning, "IDataStorage interface not found a Bridge");
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
                                    Log(Severity.Warning, "Problem deleting structure on the GameStorage Server.");
                                }
                            }
                            else
                            {
                                Log(Severity.Warning, "Not connected to the GameStorage Server.");
                            }
                        }
                        break;

                    default:
                        Log(Severity.Warning, "Not implemented yet");
                        break;
                }
            }
            else
            {
                Log(Severity.Warning, "Model not found");
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

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        ///
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be
        /// used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return Models.GetEnumerator();
        }

        public void TestCode()
        {
            PocStringValues v = new PocStringValues();
            v.nodes = new PocStringValue[]
            {
                new PocStringValue { Path="xyz",Value="abc", ValueType="System.Int32"},
                new PocStringValue { Path="def",Value="123", ValueType="System.Int32"}
            };

            ISerializer ser = new InternalXmlSerializer();
            Log(Severity.Warning, ser.Serialize(v, SerializingFormat.Xml));

            short[] tmp = new short[] { 1, 2, 3, 4, 5 };

            Debug.Print(ser.Serialize(tmp, SerializingFormat.Xml));
        }

        #endregion Methods

        /// <summary>
        /// An internal XML serializer.
        /// </summary>
        private class InternalXmlSerializer : ISerializer
        {
            /// <summary>
            /// Deserialize this object to the given textual representation and format.
            /// </summary>
            ///
            /// <typeparam name="T"> Generic type parameter. </typeparam>
            /// <param name="text">   The text to deserialize. </param>
            /// <param name="format"> Describes the format to use. </param>
            ///
            /// <returns>
            /// An object.
            /// </returns>
            public object Deserialize<T>(string text, SerializingFormat format)
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));

                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                {
                    return (T)ser.Deserialize(ms);
                }
            }

            /// <summary>
            /// Serialize this object to the given textual representation and format.
            /// </summary>
            ///
            /// <param name="obj">    The object to serialize. </param>
            /// <param name="format"> Describes the format to use. </param>
            ///
            /// <returns>
            /// A string.
            /// </returns>
            public string Serialize(object obj, SerializingFormat format)
            {
                XmlSerializer ser = new XmlSerializer(obj.GetType());

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                using (StringWriterUtf8 textWriter = new StringWriterUtf8())
                {
                    using (XmlWriter tw = XmlWriter.Create(textWriter, settings))
                    {
                        ser.Serialize(tw, obj, ns);
                    }

                    textWriter.Flush();

                    return textWriter.ToString();
                }
            }

            /// <summary>
            /// Supports the given format.
            /// </summary>
            ///
            /// <remarks>
            /// Supports only XML.
            /// </remarks>
            ///
            /// <param name="format"> Describes the format to use. </param>
            ///
            /// <returns>
            /// true if it succeeds, false if it fails.
            /// </returns>
            public bool Supports(SerializingFormat format)
            {
                return format == SerializingFormat.Xml;
            }
        }
    }
}