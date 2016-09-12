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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Serialization;

    using AssetManagerPackage;

    //! IWebServiceResponse
    //! Not sure if this needs to stay here. Better is using Dictionary of Models instead of nesting them.
    //! Make Models keys case-insesitive?
    //
    //! Somewhat more utility methods like Remove, Clear!
    //
    //[XmlRoot]
    public class GameStorageClientAsset : BaseAsset, IEnumerable
    {
        #region Fields

        public static List<StorageLocations> AllStorageLocations = new List<StorageLocations>();

        private const String jsonArrayRegEx = "\"Value\":(?:\\s?)(\\[(?>[^\\[\\]]+|\\[(?<number>)|\\](?<-number>))*(?(number)(?!))\\])";

        /// <summary>
        /// The JSON value.
        /// </summary>
        private const String jsonValueRegEx = "\"Value\":(?:\\s?)(\\{(?>[^{}]+|\\{(?<number>)|\\}(?<-number>))*(?(number)(?!))\\})";

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
        /// Filename of the settings file.
        /// </summary>
        const String SettingsFileName = "GameStorageClientAssetSettings.xml";

        /// <summary>
        /// The TimeStamp Format.
        /// </summary>
        private const string TimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// The RegEx to extract a plain quoted JSON Value. Used to extract 'token'.
        /// </summary>
        private const string TokenRegEx = "\"{0}\":\"(.+?)\"";

        private Dictionary<SerializingFormat, String> extensions = new Dictionary<SerializingFormat, string>();
        private Regex jsonArray = new Regex(jsonArrayRegEx, RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// A Regex to extact the authentication token value from JSON.
        /// </summary>
        private Regex jsonAuthToken = new Regex(String.Format(TokenRegEx, "authToken"), RegexOptions.Singleline);

        /// <summary>
        /// A Regex to extact the status value from JSON.
        /// </summary>
        private Regex jsonHealth = new Regex(String.Format(TokenRegEx, "status"), RegexOptions.Singleline);

        /// <summary>
        /// A Regex to extact the message value from JSON.
        /// </summary>
        private Regex jsonMessage = new Regex(String.Format(TokenRegEx, "message"), RegexOptions.Singleline);

        /// <summary>
        /// A Regex to extact the structure value from JSON.
        /// </summary>
        private Regex jsonStructure = new Regex(String.Format(TokenRegEx, "structure"), RegexOptions.Singleline);

        /// <summary>
        /// A Regex to extact the token value from JSON.
        /// </summary>
        private Regex jsonToken = new Regex(String.Format(TokenRegEx, "token"), RegexOptions.Singleline);
        private Regex jsonValue = new Regex(jsonValueRegEx, RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
        private Dictionary<SerializingFormat, String> prefixes = new Dictionary<SerializingFormat, string>();
        private Dictionary<SerializingFormat, String> separators = new Dictionary<SerializingFormat, string>();

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private GameStorageClientAssetSettings settings = null;
        private Dictionary<SerializingFormat, String> suffixes = new Dictionary<SerializingFormat, string>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Static constructor.
        /// </summary>
        static GameStorageClientAsset()
        {
            foreach (StorageLocations location in Enum.GetValues(typeof(StorageLocations)))
            {
                AllStorageLocations.Add(location);
            }
        }

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

            prefixes.Add(SerializingFormat.Json, "{ \"nodes\" : [");
            prefixes.Add(SerializingFormat.Xml, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Model>\r\n<Nodes>");

            separators.Add(SerializingFormat.Json, ",");

            suffixes.Add(SerializingFormat.Json, "] }");
            suffixes.Add(SerializingFormat.Xml, "</Nodes>\r\n</Model>");

            extensions.Add(SerializingFormat.Json, ".json");
            extensions.Add(SerializingFormat.Xml, ".xml");
            //extensions.Add(SerializingFormat.Binary, ".bin");

            //! Create Settings Object so they can be loaded or saved as default.
            //
            settings = new GameStorageClientAssetSettings();

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
        /// Gets the health.
        /// </summary>
        ///
        /// <value>
        /// The health.
        /// </value>
        public String Health { get; private set; }

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
        /// Gets or sets the types.
        /// </summary>
        ///
        /// <value>
        /// The types.
        /// </value>
        public Dictionary<String, Type> Types { get; set; }

        /// <summary>
        /// Storage of Multiple Models.
        /// </summary>
        ///
        /// <value>
        /// The models.
        /// </value>
        private Dictionary<String, Node> Models { get; set; }

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

            RequestResponse response = IssueRequest("health", "GET", headers, String.Empty);

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

                                RequestResponse response = IssueRequest(
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

        /// <summary>
        /// Loads a data.
        /// </summary>
        ///
        /// <remarks>
        /// The A2 Server (MongoDB based) currently only supports JSON.
        /// </remarks>
        ///
        /// <remarks>
        /// By default the data matching the location parameter is cleared before restoring.
        /// </remarks>
        ///
        /// <remarks>
        /// <pre>
        /// LoadData()
        ///    DeSerializeData()
        ///      DeserializeDataJson()
        ///      or
        ///      DeserializeDataXml()
        ///         Bridge/ISerializer.Deserialize()
        ///         or
        ///         built-in (xml)
        /// </pre>
        /// </remarks>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        /// <param name="format">   Describes the format to use. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public Boolean LoadData(String model, StorageLocations location, SerializingFormat format)
        {
            switch (location)
            {
                case StorageLocations.Local:
                    IDataStorage storage = getInterface<IDataStorage>();

                    if (storage != null)
                    {

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

                            RequestResponse response = IssueRequest(
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
        /// Loads a models Structure.
        /// </summary>
        ///
        /// <remarks>
        /// The A2 Server (MongoDB based) currently only supports JSON.
        /// </remarks>
        ///
        /// <remarks>
        /// <pre>
        /// LoadStructure()
        ///    DeSerializeStructure()
        ///      DeserializeStructureJson()
        ///      or
        ///      DeserializeStructureXml()
        ///         Bridge/ISerializer.Deserialize()
        ///         or
        ///         built-in (xml)
        /// </pre>
        /// </remarks>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        /// <param name="format">   Describes the format to use. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public Boolean LoadStructure(String model, StorageLocations location, SerializingFormat format)
        {
            //! TODO Add binary parameter.
            //
            switch (location)
            {
                case StorageLocations.Local:
                    IDataStorage storage = getInterface<IDataStorage>();

                    if (storage != null)
                    {
                        string fn = string.Format("{0}_structure{1}", model, extensions[format]);

                        if (storage.Exists(fn))
                        {
                            if (Models.ContainsKey(model))
                            {
                                Models[model].Clear();
                            }
                            else
                            {
                                Models[model] = new Node(this, model);
                            }

                            string data = storage.Load(fn);

                            DeSerializeStructure(model, data, location, format);

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

                            RequestResponse response = IssueRequest(
                                String.Format("storage/model/{0}", model),
                                "GET", headers, String.Empty, settings.Port);

                            if (response.ResultAllowed)
                            {
                                DeSerializeStructure(model, response.body, location, format);
                            }

                            //if (jsonStructure.IsMatch(response.body))
                            //{
                            //    String base64 = jsonStructure.Match(response.body).Groups[1].Value;
                            //    Byte[] bytes = Convert.FromBase64String(base64);
                            //    String structure = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                            //    this[model].FromXml(structure);

                            //    //this[model].FromBinary(base64, true);

                            //    Log(Severity.Information, "Structure of Model[{0}] is Restored", model);

                            //    return true;
                            //}
                            //}

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

            RequestResponse response = IssueRequest("login", "POST", headers,
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
        /// Registers the types described by types.
        /// </summary>
        ///
        /// <param name="types"> The types. </param>
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
        /// Saves a data.
        /// </summary>
        ///
        /// <remarks>
        /// The A2 Server (MongoDB based) currently only supports JSON.
        /// </remarks>
        ///
        /// <remarks>
        /// <pre>
        /// LoadData()
        ///    SerializeData()
        ///      SerializeDataJson()
        ///      or
        ///      SerializeDataXml()
        ///         Bridge/ISerializer.Serialize()
        ///         or
        ///         built-in (xml)
        /// </pre>
        /// </remarks>
        ///
        /// <remarks>
        /// If enumeration is ommitted (or null), the Data is filtered on location. So by default the
        /// storage location target matches the filtering.
        /// </remarks>
        ///
        /// <param name="model">        The model. </param>
        /// <param name="location">     The location. </param>
        /// <param name="format">       Describes the format to use. </param>
        /// <param name="enumeration">  (Optional) The enumeration. </param>
        public void SaveData(String model, StorageLocations location, SerializingFormat format, List<StorageLocations> enumeration = null)
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
                                storage.Save(model + extensions[format], SerializeData(model, location, format, enumeration));
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

                            String data = SerializeData(model, location, format, enumeration);

                            RequestResponse response = IssueRequest(
                                        String.Format("storage/data/{0}", model),
                                        "PUT",
                                        headers,
                                        data,
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
        /// Saves the Model structure.
        /// </summary>
        ///
        /// <remarks>
        /// The A2 Server (MongoDB based) currently only supports JSON.
        /// </remarks>
        ///
        /// <remarks>
        /// <pre>
        /// SaveStructure()
        ///    SerializeStructure()
        ///      serializeStructureJson()
        ///      or
        ///      serializeStructureXml()
        ///         Bridge/ISerializer.Serialize()
        ///         or
        ///         built-in (xml)
        /// </pre>
        /// </remarks>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        /// <param name="format">   Describes the format to use. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public Boolean SaveStructure(String model, StorageLocations location, SerializingFormat format)
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
                            String data = SerializeStructure(model, format);

                            storage.Save(string.Format("{0}_structure{1}", model, extensions[format]), data);

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

                                String data = SerializeStructure(model, format);

                                //! Base64 Encode Xml Structure so we do not have issues with quotes ect.
                                //
                                //Byte[] structure = Encoding.UTF8.GetBytes(this[model].ToXml(true));
                                //String data = String.Format("{{\r\n \"structure\": \"{0}\"}}", Convert.ToBase64String(structure));

                                //this[model].ToBinary(true))

                                RequestResponse response = IssueRequest(
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

        public void TestCode(Object v)
        {
            //NodeStringValues v = new NodeStringValues();
            //v.nodes = new NodeStringValue[]
            //{
            //    new NodeStringValue { Path="xyz",Value="abc", ValueType="System.Int32"},
            //    new NodeStringValue { Path="def",Value="123", ValueType="System.Int32"}
            //};
            ISerializer ser = new InternalXmlSerializer();

            Log(Severity.Verbose, ser.Serialize(v, SerializingFormat.Xml));

            short[] tmp = new short[] { 1, 2, 3, 4, 5 };

            Log(Severity.Verbose, ser.Serialize(tmp, SerializingFormat.Xml));
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
        /// De serialize data.
        /// </summary>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="data">     The data. </param>
        /// <param name="location"> The location. </param>
        /// <param name="format">   Describes the format to use. </param>
        private void DeSerializeData(String model, String data, StorageLocations location, SerializingFormat format)
        {
            if (Models.ContainsKey(model))
            {
                ISerializer serializer = getInterface<ISerializer>();

                if (serializer != null && serializer.Supports(format))
                {
                    switch (format)
                    {
                        case SerializingFormat.Json:
                            DeserializeDataJson(serializer, Models[model], data, location, format);
                            break;
                        case SerializingFormat.Xml:
                            DeserializeDataXml(serializer, Models[model], data, location, format);
                            break;
                    }
                }
                else
                {
                    //! Try Default one for xml and binary.
                    //
                    switch (format)
                    {
                        case SerializingFormat.Xml:
                            DeserializeDataXml(new InternalXmlSerializer(), Models[model], data, location, format);
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

        private void DeSerializeStructure(String model, String data, StorageLocations location, SerializingFormat format)
        {
            if (Models.ContainsKey(model))
            {
                ISerializer serializer = getInterface<ISerializer>();

                if (serializer != null && serializer.Supports(format))
                {
                    switch (format)
                    {
                        case SerializingFormat.Json:
                            DeSerializeStructureJson(serializer, Models[model], data, location, format);
                            break;
                        case SerializingFormat.Xml:
                            DeSerializeStructureXml(serializer, Models[model], data, location, format);
                            break;
                    }
                }
                else
                {
                    //! Try Default one for xml and binary.
                    //
                    switch (format)
                    {
                        case SerializingFormat.Xml:
                            DeSerializeStructureXml(new InternalXmlSerializer(), Models[model], data, location, format);
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
        /// De serialize strucure XML.
        /// </summary>
        ///
        /// <param name="serializer">   The serializer. </param>
        /// <param name="root">         The root. </param>
        /// <param name="data">         The data. </param>
        /// <param name="location">     The location. </param>
        /// <param name="format">       Describes the format to use. </param>
        private void DeSerializeStructureXml(ISerializer serializer, Node root, String data, StorageLocations location, SerializingFormat format)
        {
            //! Get a list of things to deserialize.
            //
            NodePaths sNodes = (NodePaths)serializer.Deserialize<NodePaths>(data, format);

            for (Int32 i = 0; i < sNodes.Nodes.Count; i++)
            {
                NodePath np = sNodes.Nodes[i];

                if (String.IsNullOrEmpty(np.Path))
                {
                    //! Root
                    root.Name = "root";
                    root.Purpose = np.Purpose;
                    root.StorageLocation = np.Location;
                }
                else if (!np.Path.Contains("."))
                {
                    //! Children of root.
                    string childPath = np.Path.Substring(np.Path.LastIndexOf(".") + 1);
                    root.AddChild(childPath, np.Location);
                }
                else
                {
                    //! Children of Childeren
                    string parentPath = np.Path.Substring(0, np.Path.LastIndexOf("."));
                    string childPath = np.Path.Substring(np.Path.LastIndexOf(".") + 1);

                    Node n = root.PrefixEnumerator(AllStorageLocations).FirstOrDefault(p => p.Path.Equals(parentPath));
                    if (n != null)
                    {
                        n.AddChild(childPath, np.Location);
                    }
                }
            }
        }

        /// <summary>
        /// De serialize strucure JSON.
        /// </summary>
        ///
        /// <param name="serializer">   The serializer. </param>
        /// <param name="root">         The root. </param>
        /// <param name="data">         The data. </param>
        /// <param name="location">     The location. </param>
        /// <param name="format">       Describes the format to use. </param>
        private void DeSerializeStructureJson(ISerializer serializer, Node root, string data, StorageLocations location, SerializingFormat format)
        {
            //! Get a list of things to deserialize.
            //
            NodePaths sNodes = (NodePaths)serializer.Deserialize<NodePaths>(data, format);

            for (Int32 i = 0; i < sNodes.Nodes.Count; i++)
            {
                NodePath np = sNodes.Nodes[i];

                if (String.IsNullOrEmpty(np.Path))
                {
                    //! Root
                    root.Name = "root";
                    root.Purpose = np.Purpose;
                    root.StorageLocation = np.Location;
                }
                else if (!np.Path.Contains("."))
                {
                    //! Children of root.
                    string childPath = np.Path.Substring(np.Path.LastIndexOf(".") + 1);
                    root.AddChild(childPath, np.Location);
                }
                else
                {
                    //! Children of Childeren
                    string parentPath = np.Path.Substring(0, np.Path.LastIndexOf("."));
                    string childPath = np.Path.Substring(np.Path.LastIndexOf(".") + 1);

                    Node n = root.PrefixEnumerator(AllStorageLocations).FirstOrDefault(p => p.Path.Equals(parentPath));
                    if (n != null)
                    {
                        n.AddChild(childPath, np.Location);
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize this object to the given stream.
        /// </summary>
        ///
        /// <remarks>
        /// This method might seem complex and has various fixups for data. Mostly caused by not knowing
        /// the data type in advance &amp; Unity3D's simple yet fast json support.
        /// </remarks>
        ///
        /// <param name="serializer">   The serializer. </param>
        /// <param name="root">         The root. </param>
        /// <param name="data">         The data. </param>
        /// <param name="location">     The location. </param>
        /// <param name="format">       Describes the format to use. </param>
        private void DeserializeDataJson(ISerializer serializer, Node root, String data, StorageLocations location, SerializingFormat format)
        {
            //! Get a list of things to deserialize.
            //
            NodeStringValues sNodes = (NodeStringValues)serializer.Deserialize<NodeStringValues>(data, format);

            //! This works without types[] paramaters as there is only a single matching method.
            //
            MethodInfo method = serializer.GetType().MethodInfoFix("Deserialize" /*, new Type[] { typeof(String), typeof(SerializingFormat) }*/);

            //! Nicer but fails to compile on the <T> of p.Deserialize.
            //
            //MethodInfo method1 = RageExtensions.GetMethodInfo<ISerializer>(p => p.Deserialize<T>(String.Empty, format));

            //! 1) Enumerate all deserialized nodes.
            //
            for (Int32 i = 0; i < sNodes.Nodes.Length; i++)
            {
                NodeStringValue nodeStringValue = sNodes.Nodes[i];

                //! 2) Problem, in Unity all serialized Value.Value's are empty (probably due to the object type in a NodeValue).
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
                Type tt = LookuptType(nodeStringValue.ValueType);

                // This does not change the node in nodes.
                //
                nodeStringValue.Path = nodeStringValue.Path.Replace('|', '.');

#warning Json Specific Fixups Ahead!

                // The "null" is because it's a static method
                NodeObjectValue fixNodeValue = new NodeObjectValue();

                //! Handle Class
                //
                if (nodeStringValue.Value.ToString().StartsWith("{"))
                {
                    //! Create a Generic Method to call the Deserializer.
                    //
                    MethodInfo genericMethod = method.MakeGenericMethod(tt);

                    //! Invoke the Deserializer.
                    //
                    fixNodeValue.ValueAsObject = genericMethod.Invoke(serializer, new Object[] { nodeStringValue.Value, format });
                }
                //! Handle Array
                //
                else if (nodeStringValue.Value.ToString().StartsWith("["))
                {
                    //! Create a Generic Class to Serialize the Value into.
                    //
                    Type nodeType = typeof(NodeValue<>);
                    INodeValue nodeValue = (INodeValue)Activator.CreateInstance(nodeType.MakeGenericType(tt));

                    //! Create a Generic Method to call the Deserializer.
                    //
                    MethodInfo genericMethod = method.MakeGenericMethod(nodeValue.GetType());

                    //! Deserialize into the Generic Class and extract the Value.
                    //
                    String s = String.Format("{{ \"Value\": {0} }}", nodeStringValue.Value);
                    nodeValue = (INodeValue)genericMethod.Invoke(serializer, new Object[] { s, format });
                    fixNodeValue.ValueAsObject = nodeValue.GetValue();
                }
                //! Handle Everything Else with Convert
                //
                else
                {
                    //! Fallback is using Convert.ChangeType (for Primitive types for example).
                    //
                    fixNodeValue.ValueAsObject = Convert.ChangeType(nodeStringValue.Value, tt);
                }

                //! Update Tree by path (not very optimized yet).
                //
                Node n = root.PrefixEnumerator(new List<StorageLocations> { location }).FirstOrDefault(p => p.Path.Equals(nodeStringValue.Path));
                if (n != null)
                {
                    n.Value = fixNodeValue.ValueAsObject;
                }
            }
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
        private void DeserializeDataXml(ISerializer serializer, Node root, String data, StorageLocations location, SerializingFormat format)
        {
            //! Get a list of things to deserialize.
            //
            NodeStringValues sNodes = (NodeStringValues)serializer.Deserialize<NodeStringValues>(data, format);

            //! This works without types[] paramaters as there is only a single matching method.
            //
            MethodInfo method = serializer.GetType().MethodInfoFix("Deserialize" /*, new Type[] { typeof(String), typeof(SerializingFormat) }*/);

            //! Nicer but fails to compile on the <T> of p.Deserialize.
            //
            //MethodInfo method1 = RageExtensions.GetMethodInfo<ISerializer>(p => p.Deserialize<T>(String.Empty, format));

            //! 1) Enumerate all deserialized nodes.
            //
            for (Int32 i = 0; i < sNodes.Nodes.Length; i++)
            {
                NodeStringValue nodeStringValue = sNodes.Nodes[i];

                //! 2) Problem, in Unity all serialized Value.Value's are empty (probably due to the object type in a NodeValue).
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
                Type tt = LookuptType(nodeStringValue.ValueType);

                // This does not change the node in nodes.
                //
                nodeStringValue.Path = nodeStringValue.Path.Replace('|', '.');

                NodeObjectValue fixNodeValue = new NodeObjectValue();

                //! Create a Generic Method to call the Deserializer.
                //
                MethodInfo genericMethod = method.MakeGenericMethod(tt);

                //! Invoke the Deserializer.
                //
                fixNodeValue.ValueAsObject = genericMethod.Invoke(serializer, new Object[] { nodeStringValue.Value, format });
                fixNodeValue.Path = nodeStringValue.Path;
                fixNodeValue.ValueType = nodeStringValue.ValueType;

                if (fixNodeValue.ValueAsObject != null && !fixNodeValue.ValueAsObject.GetType().FullName.Equals(fixNodeValue.ValueType))
                {
                    Log(Severity.Verbose, "Casting {0} into {1}", fixNodeValue.ValueAsObject.GetType().Name, tt.Name);
                    fixNodeValue.ValueAsObject = Convert.ChangeType(fixNodeValue.ValueAsObject, tt);
                }

                //! Update Tree by path (not very optimized yet).
                //
                Node n = root.PrefixEnumerator(new List<StorageLocations> { location }).FirstOrDefault(p => p.Path.Equals(nodeStringValue.Path));
                if (n != null)
                {
                    n.Value = fixNodeValue.ValueAsObject;
                }
            }
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
        private RequestResponse IssueRequest(string path, string method)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            headers.Add("Content-Type", "application/json");
            headers.Add("Accept", "application/json");

            return IssueRequest(path, method, new Dictionary<string, string>(), String.Empty);
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
        private RequestResponse IssueRequest(string path, string method, Dictionary<string, string> headers, string body = "")
        {
            return IssueRequest(path, method, headers, body, settings.Port);
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
        private RequestResponse IssueRequest(string path, string method, Dictionary<string, string> headers, string body, Int32 port)
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
                Log(Severity.Verbose, "Caching Type for {0}", typename);

                Type type = Type.GetType(typename);

                Types.Add(type.FullName, type);
            }

            return Types[typename];
        }

        /// <summary>
        /// Serialize data.
        /// </summary>
        ///
        /// <remarks>
        /// If enumeration is ommitted (or null), the Data is filtered on location. So by default the
        /// storage location target matches the filtering.
        /// </remarks>
        ///
        /// <param name="model">        The model. </param>
        /// <param name="location">     The location. </param>
        /// <param name="format">       Describes the format to use. </param>
        /// <param name="enumeration">  (Optional) the enumeration. </param>
        ///
        /// <returns>
        /// A String.
        /// </returns>
        internal String SerializeData(String model, StorageLocations location, SerializingFormat format, List<StorageLocations> enumeration = null)
        {
            if (Models.ContainsKey(model))
            {
                ISerializer serializer = getInterface<ISerializer>();

                if (serializer != null && serializer.Supports(format))
                {
                    switch (format)
                    {
                        case SerializingFormat.Json:
                            return SerializeDataJson(serializer, Models[model], location, format, enumeration);
                        case SerializingFormat.Xml:
                            return SerializeDataXml(serializer, Models[model], location, format, enumeration);
                    }
                }
                else
                {
                    //! Try Default one for xml and binary.
                    //
                    switch (format)
                    {
                        case SerializingFormat.Xml:
                            return SerializeDataXml(new InternalXmlSerializer(), Models[model], location, format, enumeration);

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
        /// <remarks>
        /// If enumeration is ommitted (or null), the Data is filtered on location. So by default the
        /// storage location target matches the filtering.
        /// </remarks>
        /// 
        /// <param name="serializer">   The serializer. </param>
        /// <param name="root">         The root. </param>
        /// <param name="location">     The location. </param>
        /// <param name="format">       Describes the format to use. </param>
        /// <param name="enumeration">  The enumeration. </param>
        ///
        /// <returns>
        /// A String containing Json.
        /// </returns>
        private String SerializeDataJson(ISerializer serializer, Node root, StorageLocations location, SerializingFormat format, List<StorageLocations> enumeration)
        {
            StringBuilder serialized = new StringBuilder();

            //! 1) Open array.
            //
            if (prefixes.ContainsKey(format))
            {
                serialized.AppendLine(prefixes[format]);
            }

            Type nodeValueType = typeof(NodeValue<>);

            //! 2) Enumerate all nodes to be save to the specified location.
            //
            foreach (Node node in root.PrefixEnumerator(enumeration != null ? enumeration : new List<StorageLocations> { location }))
            {
                //! As the data is cleared before a restore and thus a node with a null value does not have to be saved.
                if (node.Value != null)
                {
#warning TODO: Not optimal location for nodeValue & nt inside the loop, but usefull during debugging.
                    Type nt = node.Value.GetType();

                    String json = String.Empty;

                    INodeValue nodeValue;

#warning Move this Code into Node (like ToNodeStructure())?

                    //! 3) Adjust value to a String for Classes (not being a string).
                    // xx
                    if (!(node.Value is String) && nt.IsClassFix() && nt.IsSerializableFix() && !nt.IsArray)
                    {
                        //! Serialize Classes except Strings.
                        //
                        //! Serializes as a Json Class (not a String).
                        //! So we need to convert later.
                        //! TODO Cache these?
                        nodeValue = (INodeValue)Activator.CreateInstance(nodeValueType.MakeGenericType(nt));
                        nodeValue.SetValue(node.Value);

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
                        nodeValue = (INodeValue)Activator.CreateInstance(nodeValueType.MakeGenericType(listType));

                        nodeValue.SetValue(method.Invoke(null, new Object[] { node.Value }));

                        //"[\r\n  1,\r\n  2,\r\n  3,\r\n  4,\r\n  5\r\n]"
                        // versus:
                        //[1,2,3,4,5]
                    }
                    else if (nt.IsPrimitiveFix())
                    {
                        //! The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
                        //
                        nodeValue = new NodeValue<String>();

                        nodeValue.SetValue(node.Value.ToString());
                    }
                    else if (node.Value is DateTime)
                    {
                        nodeValue = new NodeValue<String>();

                        nodeValue.SetValue(((DateTime)(node.Value)).ToString("O"));
                    }
                    else
                    {
                        nodeValue = new NodeValue<String>();
                        nodeValue.SetValue(node.Value.ToString());
                    }

                    nodeValue.SetPath(node.Path);
                    nodeValue.SetValueType(nt.FullName);

                    json = serializer.Serialize(nodeValue, format);

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
                else
                {
                    Log(Severity.Warning, "Null Node Value encountered for Path '{0}'.", node.Path);
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
        /// Serialize XML.
        /// </summary>
        ///
        /// <remarks>
        /// If enumeration is ommitted (or null), the Data is filtered on location. So by default the
        /// storage location target matches the filtering.
        /// </remarks>
        ///
        /// <param name="serializer">   The serializer. </param>
        /// <param name="root">         The root. </param>
        /// <param name="location">     The location. </param>
        /// <param name="format">       Describes the format to use. </param>
        /// <param name="enumeration">  The enumeration. </param>
        ///
        /// <returns>
        /// A String containing Xml.
        /// </returns>
        private String SerializeDataXml(ISerializer serializer, Node root, StorageLocations location, SerializingFormat format, List<StorageLocations> enumeration)
        {
            StringBuilder serialized = new StringBuilder();

            //! 1) Open array.
            //
            if (prefixes.ContainsKey(format))
            {
                serialized.AppendLine(prefixes[format]);
            }

            Type nodeValueType = typeof(NodeValue<>);

            //! 2) Enumerate all nodes to be save to the specified location.
            //
            foreach (Node node in root.PrefixEnumerator(enumeration != null ? enumeration : new List<StorageLocations> { location }))
            {
                //! As the data is cleared before a restore and thus a node with a null value does not have to be saved.
                if (node.Value != null)
                {
#warning TODO: Not optimal location for nodeValue & nt inside the loop, but usefull during debugging.
                    Type nt = node.Value.GetType();

                    INodeValue nodeValue;

                    String val = serializer.Serialize(node.Value, format);
                    nodeValue = new NodeValue<String>();
                    nodeValue.SetValue(val);
                    nodeValue.SetPath(node.Path);
                    nodeValue.SetValueType(nt.FullName);

                    String xml = serializer.Serialize(nodeValue, format);

                    serialized.Append(String.Format("{0}", xml));

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
                else
                {
                    Log(Severity.Warning, "Null Node Value encountered for Path '{0}'.", node.Path);
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
        /// Serialize structure.
        /// </summary>
        ///
        /// <param name="enumeration">  The enumeration. </param>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="format">   Describes the format to use. </param>
        ///
        /// <returns>
        /// A String.
        /// </returns>
        internal String SerializeStructure(String model, SerializingFormat format)
        {
            if (Models.ContainsKey(model))
            {
                ISerializer serializer = getInterface<ISerializer>();

                if (serializer != null && serializer.Supports(format))
                {
                    switch (format)
                    {
                        case SerializingFormat.Json:
                            return SerializeStructureJson(serializer, Models[model], format, AllStorageLocations);
                        case SerializingFormat.Xml:
                            return SerializeStructureXml(serializer, Models[model], format, AllStorageLocations);
                    }
                }
                else
                {
                    //! Try Default one for xml and binary.
                    //
                    switch (format)
                    {
                        case SerializingFormat.Xml:
                            return SerializeStructureXml(new InternalXmlSerializer(), Models[model], format, AllStorageLocations);

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
        /// <param name="serializer">   The serializer. </param>
        /// <param name="root">         The root. </param>
        /// <param name="format">       Describes the format to use. </param>
        /// <param name="enumeration">  The enumeration. </param>
        ///
        /// <returns>
        /// A String containing Json.
        /// </returns>
        private String SerializeStructureJson(ISerializer serializer, Node root, SerializingFormat format, List<StorageLocations> enumeration)
        {
            StringBuilder serialized = new StringBuilder();

            //! 1) Open array.
            //
            if (prefixes.ContainsKey(format))
            {
                serialized.AppendLine(prefixes[format]);
            }

            //! 1a) Write Root Node.
            //
            NodePath nodeStructure = new NodePath();

            nodeStructure = root.ToNodeStructure();

            String json = serializer.Serialize(nodeStructure, format);

            serialized.Append(String.Format("{0}", json));

            //! 1b) Write separator if any.
            //
            if (separators.ContainsKey(format))
            {
                serialized.AppendLine(separators[format]);
            }
            else
            {
                serialized.AppendLine(String.Empty);
            }

            //! 2) Enumerate all nodes to be save to the specified location.
            //
            foreach (Node node in root.PrefixEnumerator(enumeration))
            {
                nodeStructure = node.ToNodeStructure();

                json = serializer.Serialize(nodeStructure, format);

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
        /// Serialize structure XML.
        /// </summary>
        ///
        /// <param name="serializer">   The serializer. </param>
        /// <param name="root">         The root. </param>
        /// <param name="format">       Describes the format to use. </param>
        /// <param name="enumeration">  The enumeration. </param>
        ///
        /// <returns>
        /// A String.
        /// </returns>
        private String SerializeStructureXml(ISerializer serializer, Node root, SerializingFormat format, List<StorageLocations> enumeration)
        {
            StringBuilder serialized = new StringBuilder();

            //! 1) Open array.
            //
            if (prefixes.ContainsKey(format))
            {
                serialized.AppendLine(prefixes[format]);
            }

            //! 1a) Write Root Node.
            //
            NodePath nodeStructure = new NodePath();

            nodeStructure = root.ToNodeStructure();

            String xml = serializer.Serialize(nodeStructure, format);

            serialized.Append(String.Format("{0}", xml));

            //! 1b) Write separator if any.
            //
            if (separators.ContainsKey(format))
            {
                serialized.AppendLine(separators[format]);
            }
            else
            {
                serialized.AppendLine(String.Empty);
            }

            //! 2) Enumerate all nodes to be save to the specified location.
            //
            foreach (Node node in root.PrefixEnumerator(enumeration))
            {
                xml = serializer.Serialize(node.ToNodeStructure(), format);

                serialized.Append(String.Format("{0}", xml));

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

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// An internal XML serializer.
        /// </summary>
        private class InternalXmlSerializer : ISerializer
        {
            /// <summary>
            /// The XmlSerializer cache.
            /// </summary>
            private static Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

            /// <summary>
            /// Gets a serializer.
            /// </summary>
            ///
            /// <param name="type"> The type. </param>
            ///
            /// <returns>
            /// The serializer.
            /// </returns>
            public static XmlSerializer GetSerializer(Type type)
            {
                if (!serializers.ContainsKey(type))
                {
                    //Log(Severity.Verbose, "Caching XmlSerializer for {0}", type.FullName);

                    serializers.Add(type, new XmlSerializer(type));
                    //serializers.Add(type, XmlSerializer.FromTypes(new[] { type })[0]);
                }

                return serializers[type];
            }

            #region Methods

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
                XmlSerializer ser = GetSerializer(typeof(T));

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
                XmlSerializer ser = GetSerializer(obj.GetType());

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;

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

            #endregion Methods
        }

        #endregion Nested Types
    }
}