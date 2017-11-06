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

#undef ASYNC
//#define ASYNC

namespace UserModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
#if ASYNC
    using System.Threading.Tasks;
#endif

#if PORTABLE
    //x
#else
    //x
#endif

    using AssetPackage;

    using Newtonsoft.Json;

    class Bridge : IBridge, IVirtualProperties, IDataStorage, IWebServiceRequest, ILog, ISerializer
    {
        readonly String StorageDir = String.Format(@".{0}DataStorage", Path.DirectorySeparatorChar);

        /// <summary>
        /// Initializes a new instance of the asset_proof_of_concept_demo_CSharp.Bridge class.
        /// </summary>
        public Bridge()
        {
            if (!Directory.Exists(StorageDir))
            {
                Directory.CreateDirectory(StorageDir);
            }
        }

        #region IDataStorage Members

        /// <summary>
        /// Exists the given file.
        /// </summary>
        ///
        /// <param name="fileId"> The file identifier to delete. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public bool Exists(string fileId)
        {
            return File.Exists(Path.Combine(StorageDir, fileId));
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        ///
        /// <returns>
        /// A List&lt;String&gt;
        /// </returns>
        public String[] Files()
        {
            return Directory.GetFiles(StorageDir).ToList().ConvertAll(
    new Converter<String, String>(p => p.Replace(StorageDir + Path.DirectorySeparatorChar, ""))).ToArray();

            //! EnumerateFiles not supported in Unity3D.
            // 
            //return Directory.EnumerateFiles(StorageDir).ToList().ConvertAll(
            //    new Converter<String, String>(p => p.Replace(StorageDir +  Path.DirectorySeparatorChar, ""))).ToList();
        }

        /// <summary>
        /// Saves the given file.
        /// </summary>
        ///
        /// <param name="fileId">   The file identifier to delete. </param>
        /// <param name="fileData"> Information describing the file. </param>
        public void Save(string fileId, string fileData)
        {
            File.WriteAllText(Path.Combine(StorageDir, fileId), fileData);
        }

        /// <summary>
        /// Loads the given file.
        /// </summary>
        ///
        /// <param name="fileId"> The file identifier to delete. </param>
        ///
        /// <returns>
        /// A String.
        /// </returns>
        public string Load(string fileId)
        {
            return File.ReadAllText(Path.Combine(StorageDir, fileId));
        }

        /// <summary>
        /// Deletes the given fileId.
        /// </summary>
        ///
        /// <param name="fileId"> The file identifier to delete. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public bool Delete(string fileId)
        {
            if (Exists(fileId))
            {
                File.Delete(Path.Combine(StorageDir, fileId));

                return true;
            }

            return false;
        }

        #endregion

        #region ILog Members

        /// <summary>
        /// Executes the log operation.
        /// 
        /// Implement this in Game Engine Code.
        /// </summary>
        ///
        /// <param name="severity"> The severity. </param>
        /// <param name="msg">      The message. </param>
        public void Log(Severity severity, string msg)
        {
            // if (((int)LogLevel.Info & (int)severity) == (int)severity)
            {
                if (String.IsNullOrEmpty(msg))
                {
                    Debug.WriteLine("");
                }
                else
                {
                    Debug.WriteLine(String.Format("{0}: {1}", severity, msg));
                }
            }
        }

        #endregion ILog Members

        #region IVirtualProperties Members

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

        #endregion IVirtualProperties Members

        #region IWebServiceRequest Members

        // See http://stackoverflow.com/questions/12224602/a-method-for-making-http-requests-on-unity-ios
        // for persistence.
        // See http://18and5.blogspot.com.es/2014/05/mono-unity3d-c-https-httpwebrequest.html

#if ASYNC
        public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestReponse)
        {
            // Wrap the actual method in a Task. Neccesary because we cannot:
            // 1) Make this method async (out is not allowed) 
            // 2) Return a Task<RequestResponse> as it breaks the interface (only void does not break it).
            //
            Task<RequestResponse> taskName = Task.Factory.StartNew<RequestResponse>(() =>
            {
                return WebServiceRequestAsync(requestSettings).Result;
            });

            requestReponse = taskName.Result;
        }

        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings"> Options for controlling the operation. </param>
        ///
        /// <returns>
        /// A RequestResponse.
        /// </returns>
        private async Task<RequestResponse> WebServiceRequestAsync(RequestSetttings requestSettings)
#else
        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings">  Options for controlling the operation. </param>
        /// <param name="requestResponse"> The request response. </param>
        public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestResponse)
        {
            requestResponse = WebServiceRequest(requestSettings);
        }

        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings">  Options for controlling the operation. </param>
        ///
        /// <returns>
        /// A RequestResponse.
        /// </returns>
        private RequestResponse WebServiceRequest(RequestSetttings requestSettings)
#endif
        {
            RequestResponse result = new RequestResponse(requestSettings);

            try
            {
                //! Might throw a silent System.IOException on .NET 3.5 (sync).
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);

                request.Method = requestSettings.method;

                // Both Accept and Content-Type are not allowed as Headers in a HttpWebRequest.
                // They need to be assigned to a matching property.

                if (requestSettings.requestHeaders.ContainsKey("Accept"))
                {
                    request.Accept = requestSettings.requestHeaders["Accept"];
                }

                if (!String.IsNullOrEmpty(requestSettings.body))
                {
                    byte[] data = Encoding.UTF8.GetBytes(requestSettings.body);

                    if (requestSettings.requestHeaders.ContainsKey("Content-Type"))
                    {
                        request.ContentType = requestSettings.requestHeaders["Content-Type"];
                    }

                    foreach (KeyValuePair<string, string> kvp in requestSettings.requestHeaders)
                    {
                        if (kvp.Key.Equals("Accept") || kvp.Key.Equals("Content-Type"))
                        {
                            continue;
                        }
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }

                    request.ContentLength = data.Length;

                    // See https://msdn.microsoft.com/en-us/library/system.net.servicepoint.expect100continue(v=vs.110).aspx
                    // A2 currently does not support this 100-Continue response for POST requets.
                    request.ServicePoint.Expect100Continue = false;

#if ASYNC
                    Stream stream = await request.GetRequestStreamAsync();
                    await stream.WriteAsync(data, 0, data.Length);
                    stream.Close();
#else
                    Stream stream = request.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();
#endif
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in requestSettings.requestHeaders)
                    {
                        if (kvp.Key.Equals("Accept") || kvp.Key.Equals("Content-Type"))
                        {
                            continue;
                        }
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

#if ASYNC
                WebResponse response = await request.GetResponseAsync();
#else
                WebResponse response = request.GetResponse();
#endif
                if (response.Headers.HasKeys())
                {
                    foreach (string key in response.Headers.AllKeys)
                    {
                        result.responseHeaders.Add(key, response.Headers.Get(key));
                    }
                }

                result.responseCode = (int)(response as HttpWebResponse).StatusCode;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
#if ASYNC
                    if (result.hasBinaryResponse)
                    {
                        result.binaryResponse = await StreamToByteArrayAsync(reader.BaseStream);
                    }
                    else
                    {
                        result.body = await reader.ReadToEndAsync();
                    }
#else
                    if (result.hasBinaryResponse)
                    {
                        result.binaryResponse = StreamToByteArray(reader.BaseStream);
                    }
                    else
                    {
                        result.body = reader.ReadToEnd();
                    }
#endif
                }
            }
            catch (Exception e)
            {
                result.responsMessage = e.Message;

                Log(Severity.Error, String.Format("{0} - {1}", e.GetType().Name, e.Message));
            }

            return result;
        }

#if ASYNC
        /// <summary>
        /// Stream to byte array asynchronous.
        /// </summary>
        ///
        /// <param name="inputStream">  Stream to read data from. </param>
        ///
        /// <returns>
        /// A byte[].
        /// </returns>
        private async Task<byte[]> StreamToByteArrayAsync(Stream inputStream)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                await inputStream.CopyToAsync(memStream);

                return memStream.ToArray();
            }
        }
#else 
        /// <summary>
        /// Stream to byte array.
        /// </summary>
        ///
        /// <param name="inputStream">  Stream to read data from. </param>
        ///
        /// <returns>
        /// A byte[].
        /// </returns>
        private byte[] StreamToByteArray(Stream inputStream)
        {
            byte[] bytes = new byte[4069];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;

                while ((count = inputStream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    memoryStream.Write(bytes, 0, count);
                }

                return memoryStream.ToArray();
            }
        }
#endif

        #endregion IWebServiceRequest Members

        #region ISerializer Members

        /// <summary>
        /// Supports the given format.
        /// </summary>
        ///
        /// <param name="format"> Describes the format to use. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public bool Supports(SerializingFormat format)
        {
            switch (format)
            {
                //case SerializingFormat.Binary:
                //    return false;
                case SerializingFormat.Xml:
                    return false;
                case SerializingFormat.Json:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Deserialize this object to the given textual representation and format.
        /// </summary>
        ///
        /// <param name="text">   The text to deserialize. </param>
        /// <param name="type">   The type to deserialize. </param>
        /// <param name="format"> Describes the format to use. </param>
        ///
        /// <returns>
        /// An object.
        /// </returns>
        //public object Deserialize<T>(string text, SerializingFormat format)
        //{
        //    return JsonConvert.DeserializeObject(text, typeof(T));
        //}

        /// <summary>
        /// Deserialize this object to the given textual representation and format.
        /// </summary>
        ///
        /// <param name="t">      A Type to process. </param>
        /// <param name="text">   The text to deserialize. </param>
        /// <param name="format"> Describes the format to use. </param>
        ///
        /// <returns>
        /// An object.
        /// </returns>
        public object Deserialize(Type t, string text, SerializingFormat format)
        {
            return JsonConvert.DeserializeObject(text, t);
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
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        #endregion ISerializer Members


    }
}