﻿/*
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
    using System.ComponentModel;
    using System.Xml.Serialization;

#if PORTABLE
#else
    [Serializable]
#endif

    /// <summary>
    /// (Serializable) a node paths.
    /// </summary>
    [XmlRoot("Model")]
    public class NodePaths
    {
        public NodePaths()
        {
            Nodes = new NodePath[0];
        }

        [XmlArray("Nodes")]
        //[XmlArrayItem("Node")]
        public NodePath[] Nodes;
    }

#if PORTABLE
#else
    [Serializable]
#endif
    public class NodePath
    {
        /// <summary>
        /// Full pathname of the file.
        /// </summary>
        public String Path;

        /// <summary>
        /// The purpose.
        /// </summary>
        //[XmlAttribute]
        [DefaultValue("")]
        public String Purpose;

        /// <summary>
        /// The location.
        /// </summary>
        //[XmlAttribute]
        [DefaultValue(StorageLocations.Inherited)]
        public StorageLocations Location;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NodePath()
        {
            this.Path = String.Empty;
            this.Purpose = String.Empty;
            this.Location = StorageLocations.Inherited;
        }

        public NodePath(String Path)
            : this()
        {
            this.Path = Path;
        }
        public NodePath(String Path, StorageLocations Location)
            : this(Path)
        {
            this.Location = Location;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        ///
        /// <param name="Path">     Full pathname of the file. </param>
        /// <param name="Location"> The location. </param>
        /// <param name="Purpose">  Full pathname of the file. </param>
        public NodePath(String Path, StorageLocations Location, String Purpose)
            : this(Path, Location)
        {
            this.Purpose = Purpose;
        }
    }

#if PORTABLE
#else
    [Serializable]
#endif

    /// <summary>
    /// (Serializable)a node with string values. Used to deserialize the Json nodes
    /// Array as all Values are encoded as string.
    /// </summary>
    ///
    /// <remarks>
    /// For Unity3D's Json Serializer only fields are allowed (so no properties).
    /// </remarks>
    [XmlRoot("Model")]
    public class NodeStringValues
    {
        public NodeStringValues()
        {
            Nodes = new NodeStringValue[0];
        }

        [XmlArray("Nodes")]
        [XmlArrayItem("Node")]
        public NodeStringValue[] Nodes;
    }

#if PORTABLE
#else
    [Serializable]
#endif

    /// <summary>
    /// (Serializable)a Node as String Value.
    /// </summary>
    ///
    /// <remarks>
    /// For Unity3D's Json Serializer only fields are allowed (so no properties).
    /// </remarks>
    public class NodeStringValue
    {
        /// <summary>
        /// The value.
        /// </summary>
        public String Value;

        /// <summary>
        /// Full pathname of the file.
        /// </summary>
        public String Path;

        /// <summary>
        /// Type of the value.
        /// </summary>
        public String ValueType;
    }

    /// <summary>
    /// A Node String Object value with an additional object to store deserialzied values.
    /// </summary>
    [XmlRoot("Model")]
    public class NodeObjectValue : NodeStringValue
    {
        internal Object ValueAsObject;
    }

    /// <summary>
    /// Interface for node value classes. Used as a helper during Json Deserialzation for easuer
    /// handling of created generic types.
    /// </summary>
    public interface INodeValue
    {
        Object GetValue();
        void SetValue(Object Value);

        String GetPath();
        void SetPath(String Path);

        String GetValueType();
        void SetValueType(String Path);
    }

#if PORTABLE
#else
    [Serializable]
#endif

    /// <summary>
    /// (Serializable)a Typed Node Value. It is used during deserialization of Json to
    /// cast values dynamically.
    /// </summary>
    ///
    /// <remarks>
    /// For Unity3D's Json Serializer only fields are allowed (so no properties).
    /// Is serialized as NodeStringValue.
    /// </remarks>
    ///
    /// <typeparam name="T"> Generic type parameter. </typeparam>
    [XmlRoot("Node")]
    public class NodeValue<T> : INodeValue
    {
        public T Value;
        public String Path;
        public String ValueType;

        public object GetValue()
        {
            return Value;
        }

        public void SetValue(object Value)
        {
            this.Value = (T)Value;
        }

        public String GetPath()
        {
            return Path;
        }

        public void SetPath(string Path)
        {
            this.Path = Path;
        }

        public string GetValueType()
        {
            return ValueType;
        }

        public void SetValueType(string ValueType)
        {
            this.ValueType = ValueType;
        }
    }
}
