using System;

namespace AssetPackage
{
    /// <summary>
    /// Values that represent serializable formats.
    /// </summary>
    public enum SerializingFormat
    {
        /// <summary>
        /// An enum constant representing the binary option.
        /// </summary>
        Binary,
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
        /// <param name="type"> The type to deserialize. </param>
        ///
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="text">   The text to deserialize. </param>
        /// <param name="format"> Describes the format to use. </param>
        ///
        /// <returns>
        /// An object.
        /// </returns>
        object Deserialize<T>(string text, SerializingFormat format);

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
