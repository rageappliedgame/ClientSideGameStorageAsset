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

    /// <summary>
    /// Values that represent serializable formats.
    /// </summary>
    ///
    /// <remarks>
    /// Disabled Binary Support because of lack of support for PCL's.
    /// </remarks>
    public enum SerializingFormat
    {
        ///// <summary>
        ///// An enum constant representing the binary option.
        ///// </summary>
        //Binary,

        /// <summary>
        /// An enum constant representing the XML option.
        /// </summary>
        Xml,

        /// <summary>
        /// An enum constant representing the JSON option.
        /// </summary>
        Json
    };

    /// <summary>
    /// Interface for serializer. <br/><br/>The idea is that you can implement one or
    /// more formats on the bridge, but formats might have a fallback
    /// implementation (like binary/xml and json on some platforms).
    /// </summary>
    ///
    /// <remarks>   Should be usable for Binary,Json and Xml Serialization. </remarks>
    /// <remarks>   Binary and Xml Serialization have default implementation from
    ///             .NET3.5. </remarks>
    /// <remarks>   Json Serialization can be implemented using Unity's
    ///             UnityEngine.JsonUtility. </remarks>
    /// <remarks>   Json Serialization can have a default implementation from
    ///             .Net 4.5 (preferably without using DataContracts)? </remarks>
    /// <remarks> Unity also uses System.Serializable attribute. </remarks>
    /// <remarks> Binary needs to be Base64 Encoded. </remarks>
    public interface ISerializer
    {
        /// <summary>
        /// Supports the given format.
        /// </summary>
        ///
        /// <param name="format"> Describes the format to use. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        bool Supports(SerializingFormat format);

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
        object Deserialize(Type t, string text, SerializingFormat format);

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
        string Serialize(object obj, SerializingFormat format);
    }
}
