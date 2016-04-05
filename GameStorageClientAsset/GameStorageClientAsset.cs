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
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;

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
        /// Options for controlling the operation.
        /// </summary>
        private GameStorageClientAssetSettings settings = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the AssetPackage.GameStorageClientAsset class.
        /// </summary>
        public GameStorageClientAsset()
            : base()
        {
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

        /*
        /// <summary>
        /// Errors.
        /// </summary>
        ///
        /// <param name="url"> URL of the document. </param>
        /// <param name="msg"> The error message. </param>
        public void Error(string url, string msg)
        {
            Log(Severity.Error, "{0} - [{1}]", msg, url);

            Connected = false;
        }
        */

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
#warning bit rough to clear all data.
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

        public void SaveData(String model, StorageLocations location)
        {
            if (Models.ContainsKey(model))
            {
                switch (location)
                {
                    case StorageLocations.Local:
                        IDataStorage storage = getInterface<IDataStorage>();

                        if (storage != null)
                        {
                            storage.Save(model + ".json", Models[model].ToJson(new List<StorageLocations> { location }));
                        }
                        else
                        {
                            Debug.Print("IDataStorage interface not found a Bridge");
                        }
                        break;

                    case StorageLocations.Server:
                    //IssueRequest("proxy/gleaner/collector/track", "POST", headers, data);

                    default:
                        Debug.Print("Not implemented yet");
                        break;
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
                                    "POST", headers,
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

        /*
        public void Success(string url, int code, Dictionary<string, string> headers, string body)
        {
            //! Part of the TrackerAsset Code.
            //
            Log(Severity.Verbose, "Success: {0} - [{1}]", code, url);

            foreach (KeyValuePair<string, string> kvp in headers)
            {
                Log(Severity.Verbose, "{0}: {1}", kvp.Key, kvp.Value);
            }
            Log(Severity.Verbose, body);
            Log(Severity.Verbose, "");

            // 1a) If we use a: as Authorization value on the /start/ call (and do not login),
            // 1b) We have to take the 'authToken' value from the /start/ request for subsequent calls.
            // 2a) If we login with username/password, we get a temporary Authorization value from the 'token' value.
            // 2b) This Authorization value we use for /start/ and replace it inside success() with the 'authToken' value for subsequent calls.
            // 3a) The 'token' value from 2a) can also be used directly for a start() call.

            //! /HEALTH/
            //
            if (url.EndsWith("/health"))
            {
                Health = jsonHealth.Match(body).Groups[1].Value;

                Log(Severity.Information, "Health Status= {0}", Health);
            }

            //! /LOGIN/
            //
            if (url.EndsWith("/login") && jsonToken.IsMatch(body))
            {
                settings.UserToken = jsonToken.Match(body).Groups[1].Value;
                if (settings.UserToken.StartsWith("Bearer "))
                {
                    settings.UserToken.Remove(0, "Bearer ".Length);
                }
                Log(Severity.Information, "Token= {0}", settings.UserToken);

                Connected = true;
            }

            if (url.Contains("/model/") && jsonStructure.IsMatch(body))
            {
                String base64 = jsonStructure.Match(body).Groups[1].Value;
                String model = url.Substring(url.LastIndexOf('/') + 1);

                this[model].FromBinary(base64, true);

                Log(Severity.Information, "Structure of Model[{0}] is Restored", model);
            }
        }
        */

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

        /*
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
        private bool IssueRequest(string path, string method)
        {
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
        private bool IssueRequest(string path, string method, Dictionary<string, string> headers, string body)
        {
            return IssueRequest(path, method, headers, body, settings.Port);
        }

        /// <summary>
        /// Issue a HTTP Webrequest.
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
        private bool IssueRequest(string path, string method, Dictionary<string, string> headers, string body, Int32 port)
        {
            IWebServiceRequest ds = getInterface<IWebServiceRequest>();

            if (ds != null)
            {
                //Log(LogLevel.Verbose, "****");

                Uri uri = new Uri(string.Format("http{0}://{1}{2}{3}/{4}",
                    settings.Secure ? "s" : String.Empty,
                    settings.Host,
                    port == 80 ? String.Empty : String.Format(":{0}", port),
                    String.IsNullOrEmpty(settings.BasePath.TrimEnd('/')) ? "" : settings.BasePath.TrimEnd('/'),
                    path.TrimStart('/')));

                Log(Severity.Verbose, "{0} [{1}]", method, uri.ToString());

                foreach (KeyValuePair<string, string> kvp in headers)
                {
                    Log(Severity.Verbose, "{0}: {1}", kvp.Key, kvp.Value);
                }

                if (!string.IsNullOrEmpty(body))
                {
                    Log(Severity.Verbose, body);
                }

                ds.WebServiceRequest(
                    method,
                    uri,
                    headers,
                    body,
                    this);

                return true;
            }

            return false;
        }
        */

        public IEnumerator GetEnumerator()
        {
            return Models.GetEnumerator();
        }

        #endregion Methods
    }
}