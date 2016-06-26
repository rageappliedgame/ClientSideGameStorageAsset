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
    using System.Runtime.Serialization;
#if PORTABLE
    using AssetManagerPackage; //fixup for missing ISerializable interface
#else
    using System.Runtime.Serialization.Formatters.Binary;
#endif
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using AssetManagerPackage;
    #region Enumerations

    // DONE Storage Type (per node and add inherited).
    // DONE Add Storage Type Filter option to PrefixEnumerator.
    // DONE Xml De-Serialization
    // DONE Xml Serialization
    // DONE Binary De-Serialization
    // DONE Binary Serialization
    // DONE Utility Methods like Clear().
    // DONE Tie to IVirtualProperty Interface
    // DONE Rename asset to GameStorageClientAsset.
    // 
    // TODO Decide: Skip Root Item in Prefix/PostFix Enumerators?
    // TODO Add search for Path.
    // TODO Read-Only flag
    // 
    // TODO Tie to UCM Tracker/GameStorage.
    // TODO Add AddSibling() Method to make definitions more fluid?
    //
    // TODO Json De-Serialization (use external serializer like NewtonSoft.net that comes with an MIT license)?
    // TODO Json Serialization
    //
    // TODO Test performance without generic parameter & casting.
    //
    // TODO Extend Json Serialization (now a flat array of path/(xml)value).
    //      Define a interface IJson for this?
    //
    // ISSUE When doing ToXml() the code seems to be using some kind of copy as Purpose and Owner are no longer set.
    //       This gives issues with the Value property.
    // ISSUE Supplying data during tree construction to Nodes with a StorageLocation Game (or inherited) is possible but illegal.
    //       Maybe add some checks for it.
    //
    //! Serialize Structure with Xml/Binary, values with Json (so a simple format like "node: { path: xx; value: yy}")
    //! Data types zijn ValueType (no quotes), Dates (formattted) en de rest strings & lists/arrays.
    //! Als we Xml-Json conversie gebruiken moet het format moet bi-directioneel zijn. bv @ prefix voor attributen

    // StorageType.Local:
    //     Can change isDirty.
    //     Is serialized including a value.
    //     ReadOnly is false.
    //     Can be retrieved and stored on a server.
    // 
    // StorageType.Transient:
    //     Cannot change isDirty.
    //     Only definition is serialized.
    //     ReadOnly is false.
    //     Will not be retrieved or stored on a server.
    // 
    // StorageType.Game:
    //     Cannot change IsDirty.
    //     Only definition is serialized.
    //     ReadOnly is true.
    //     Will not be retrieved or stored on a server.
    //     Value is supplied by Bridge/Game Engine.
    // 
    // StorageType.Server:
    //     Cannot change IsDirty.
    //     Only definition is serialized.
    //     ReadOnly is true.
    //     Cannot be stored on a server.
    //     Value is retrieved once from a server.

    /// <summary>
    /// Values that represent storage locations.
    /// </summary>
    public enum StorageLocations
    {
        /// <summary>
        /// An enum constant representing the inherited option.
        /// </summary>
        Inherited,
        /// <summary>
        /// An enum constant representing the local option.
        /// </summary>
        Local,
        /// <summary>
        /// An enum constant representing the transient option.
        /// </summary>
        Transient,
        /// <summary>
        /// An enum constant representing the server option.
        /// </summary>
        Server,
        /// <summary>
        /// An enum constant representing the game option.
        /// </summary>
        Game
    }

    #endregion Enumerations

    [XmlRoot("node")]
#if PORTABLE
#else
    [Serializable]
#endif
    [DebuggerDisplay("Name={Name}, Path={Path}, Count={Count}")]
    public class Node : IEqualityComparer, IXmlSerializable
#if BINARY
, ISerializable
#endif
    {
        #region Fields

        /// <summary>
        /// The Root Nodes Name.
        /// </summary>
        private const string root = "root";

        /// <summary>
        /// The childs.
        /// </summary>
        private List<Node> children = null;

        /// <summary>
        /// The storage location.
        /// </summary>
        private StorageLocations storageLocation = StorageLocations.Inherited;

        /// <summary>
        /// The value backing field.
        /// </summary>
        private Object value = null;

        /// <summary>
        /// The XmlSerializer cache.
        /// </summary>
        private static Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

        /// <summary>
        /// The type mapper cache.
        /// </summary>
        //[Obsolete]
        //private static Dictionary<String, Type> typeMapper = new Dictionary<String, Type>();

#warning DEBUG CODE!!!
        private static Boolean xmlStructureOnly = true;

        /// <summary>
        /// The owner, only valid on Root Node.
        /// </summary>
        private GameStorageClientAsset Owner = null;

        /// <summary>
        /// The purpose, only valid on Root Node.
        /// </summary>
        public String Purpose
        {
            get;
            private set;
        }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes static members of the AssetPackage.Node&lt;T&gt; class.
        /// </summary>
        static Node()
        {
            //! Preload Nodeit self (very costly).

            //Elapsed(Caching Serializers): 865 ms
            //Elapsed (Caching types): 0 ms
            //Caching Serializer for DateTime
            //Caching Serializer for DemoStruct
            //Caching Serializer for Byte
            //Caching Serializer for List`1
            //Caching Serializer for String[]
            //Elapsed: 689 ms

            Stopwatch sw = new Stopwatch();

#warning creating the serializers for Node<T> adn List<String> is very costly (1.5 seconds)!

            sw.Reset();
            sw.Start();
            {
                // 750 ms for Node
                // 
                //! These class/list/array based ones now give a 'File Not Found' Exception
                //! Solution 1) (NOT WORKING)   Just change the Generate serialization assembly drop-down to "On", instead of "Auto".
                //! Solution 2) (WORKING)       Use another method.

                // serializers.Add(typeof(Node), new XmlSerializer(typeof(Node)));
                // serializers.Add(typeof(List<Node>), new XmlSerializer(typeof(List<Node>)));
                serializers.Add(typeof(Node), XmlSerializer.FromTypes(new[] { typeof(Node) })[0]);
                serializers.Add(typeof(List<Node>), XmlSerializer.FromTypes(new[] { typeof(List<Node>) })[0]);

                // 1150 ms for List<String>
                //serializers.Add(typeof(List<String>), new XmlSerializer(typeof(List<String>)));
                //serializers.Add(typeof(String[]), new XmlSerializer(typeof(String[])));
                // 1725 ms for both
                // 

                serializers.Add(typeof(String), new XmlSerializer(typeof(String)));
                serializers.Add(typeof(Boolean), new XmlSerializer(typeof(Boolean)));
                serializers.Add(typeof(Byte), new XmlSerializer(typeof(Byte)));
                serializers.Add(typeof(Int16), new XmlSerializer(typeof(Int16)));
                serializers.Add(typeof(Int32), new XmlSerializer(typeof(Int32)));
                serializers.Add(typeof(Double), new XmlSerializer(typeof(Double)));
                serializers.Add(typeof(DateTime), new XmlSerializer(typeof(DateTime)));
            }
            sw.Stop();

            // Log(Severity.Verbose, "Elapsed (Caching XmlSerializers): {0} ms", sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            {
                //! Preload Xsd simple types.
                // 
                //typeMapper.Add("boolean", typeof(bool));                    //xsd

                //typeMapper.Add("dateTime", typeof(DateTime));               //xsd

                //typeMapper.Add("byte", typeof(sbyte));                      //xsd
                //typeMapper.Add("short", typeof(short));                     //xsd
                //typeMapper.Add("int", typeof(int));                         //xsd
                //typeMapper.Add("long", typeof(long));                       //xsd

                //typeMapper.Add("unsignedByte", typeof(Byte));               //xsd
                //typeMapper.Add("unsignedShort", typeof(ushort));            //xsd
                //typeMapper.Add("unsignedInt", typeof(uint));                //xsd
                //typeMapper.Add("unsignedLong", typeof(ulong));              //xsd

                //typeMapper.Add("float", typeof(short));                     //xsd
                //typeMapper.Add("double", typeof(double));                   //xsd
                //typeMapper.Add("decimal", typeof(decimal));                 //xsd

                //typeMapper.Add("string", typeof(string));                   //xsd

                //! Preload Simple types.
                // 
                //typeMapper.Add("bool", typeof(bool));
                //typeMapper.Add("char", typeof(char));
                //typeMapper.Add("sbyte", typeof(sbyte));
                //typeMapper.Add("datetimeoffset", typeof(DateTimeOffset));
                //typeMapper.Add("int32", typeof(int));

                //typeMapper.Add("int64", typeof(long));

                //typeMapper.Add("object", typeof(object));

                //typeMapper.Add("timespan", typeof(TimeSpan));

                //typeMapper.Add("uint16", typeof(ushort));
                //typeMapper.Add("ushort", typeof(ushort));

                //typeMapper.Add("uint32", typeof(uint));
                //typeMapper.Add("uint", typeof(uint));

                //typeMapper.Add("uint64", typeof(ulong));
                //typeMapper.Add("ulong", typeof(ulong));
                //typeMapper.Add("unsignedLong", typeof(ulong));

                //! Preload Xsd Array/List Types.
                //// 
                //typeMapper.Add("ArrayOfString", typeof(List<string>));
                //typeMapper.Add("ArrayOfBoolean", typeof(List<bool>));
                //typeMapper.Add("ArrayOfFloat", typeof(List<float>));
                //typeMapper.Add("ArrayOfDouble", typeof(List<double>));
                //typeMapper.Add("ArrayOfDecimal", typeof(List<decimal>));
                //typeMapper.Add("ArrayOfLong", typeof(List<long>));
                //typeMapper.Add("ArrayOfInt", typeof(List<int>));
                //typeMapper.Add("ArrayOfShort", typeof(List<short>));
                //typeMapper.Add("ArrayOfUnsignedByte", typeof(List<byte>));
            }
            sw.Stop();
            //Log(Severity.Verbose, "Elapsed (Caching xml types): {0} ms", sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Prevents a default instance of the AssetPackage.Node class from being
        /// created.
        /// </summary>
        private Node()
        {
            children = new List<Node>();
        }

        /// <summary>
        /// Initializes a new instance of the AssetPackage.Node class.
        /// </summary>
        /// <remarks>This method generates a Root Node with a StorageLocation set to Local</remarks>
        public Node(GameStorageClientAsset owner, string purpose)
            : this(null, root, null, StorageLocations.Local)
        {
            Owner = owner;
            Purpose = purpose;
        }

        /// <summary>
        /// Initializes a new instance of the AssetPackage.Node class.
        /// </summary>
        ///
        /// <remarks>
        /// This method generates a Root Node.
        /// </remarks>
        ///
        /// <param name="owner">           The owner. </param>
        /// <param name="StorageLocation"> The storage location. </param>
        public Node(GameStorageClientAsset owner, string purpose, StorageLocations StorageLocation)
            : this(null, root, null, StorageLocation)
        {
            Owner = owner;
            Purpose = purpose;
        }

        /// <summary>
        /// Initializes a new instance of the AssetPackage.Node class.
        /// </summary>
        ///
        /// <param name="previousNode"> The previous node. </param>
        public Node(Node previousNode)
        {
            Name = previousNode.Name;
            Purpose = previousNode.Purpose;
            storageLocation = previousNode.storageLocation;
            value = previousNode.value;
            children = previousNode.children;
        }


        /// <summary>
        /// Initializes a new instance of the AssetPackage.Node class with a StorageLocation set to Inherited.
        /// </summary>
        ///
        /// <param name="Parent"> The parent. </param>
        /// <param name="Name">   The name. </param>
        /// <param name="Value">  The value. </param>
        public Node(Node Parent, String Name, Object Value)
            : this(Parent, Name, Value, StorageLocations.Inherited)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the AssetPackage.Node class.
        /// </summary>
        ///
        /// <param name="Parent">          The parent. </param>
        /// <param name="Name">            The name. </param>
        /// <param name="Value">           The value. </param>
        /// <param name="StorageLocation"> The storage location. </param>
        public Node(Node Parent, String Name, Object Value, StorageLocations StorageLocation) : this()
        {
            this.Parent = Parent;
            this.Name = Name;
            this.Value = Value;
            this.StorageLocation = StorageLocation;
        }

#if BINARY
                /// <summary>
        /// Initializes a new instance of the AssetPackage.Node class.
        /// </summary>
        ///
        /// <param name="info">     The
        ///                         <see cref="T:System.Runtime.Serialization.SerializationInfo" />
        ///                         to populate with data. </param>
        /// <param name="context">  The destination (see
        ///                         <see cref="T:System.Runtime.Serialization.StreamingContext" />)
        ///                         for this serialization. </param>
        protected Node(SerializationInfo info, StreamingContext context)
        {
            this.Name = info.GetString("name");
            this.storageLocation = (StorageLocations)info.GetInt32("location");
            Int32 count = info.GetInt32("count");

            //if (3 + count != info.MemberCount)
            //{
            //Debug.Print("Member Mismatch: {0} != {1}", 3+count, info.MemberCount);
            //}

            if (count != 0)
            {
                for (Int32 i = 0; i < count; i++)
                {
                    Node node = (Node)info.GetValue(String.Format("child_{0}", i), typeof(Node));
                    this.AddChild(node);
                }
            }

            if (3 + count + 2 == info.MemberCount)
            {
                if (context.Context == null || context.Context.Equals(false))
                {
                    // Restore Values if present!
                    Type type = (Type)info.GetValue("type", typeof(Type));
                    this.Value = info.GetValue("value", type);
                }
                else
                {
                    // Skip Values.
                    Type type = (Type)info.GetValue("type", typeof(Type));
                    info.GetValue("value", type);
                }
            }
            else
            {
                // No Values present so can't restore.
            }
        }
#endif

        //~Node()
        //{
        //    Dispose(false);
        //}

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the children.
        /// </summary>
        ///
        /// <value>
        /// The children.
        /// </value>
        [XmlIgnore]
        public List<Node> Children
        {
            get
            {
                return children;
            }
            set
            {
#warning Added for testing newtonsoft
                children = value;
            }
        }

        /// <summary>
        /// Gets the number of Children. 
        /// </summary>
        ///
        /// <value>
        /// The count.
        /// </value>
        [XmlIgnore]
        public int Count
        {
            get
            {
                return children != null ? children.Count : 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object is the root Node.
        /// </summary>
        ///
        /// <value>
        /// true if this object is root, false if not.
        /// </value>
        [XmlIgnore]
        public Boolean IsRoot
        {
            get
            {
                return Parent == null && Name.Equals(root);
            }
        }

        /// <summary>
        /// Gets the name of the Node.
        /// </summary>
        ///
        /// <value>
        /// The name.
        /// </value>
        public String Name { get; private set; }

        /// <summary>
        /// Gets the parent of the Node.
        /// </summary>
        ///
        /// <value>
        /// The parent.
        /// </value>
        public Node Parent { get; private set; }

        /// <summary>
        /// Gets the full pathname of the file.
        /// </summary>
        ///
        /// <value>
        /// The full pathname of the file.
        /// </value>
        [XmlIgnore]
        public String Path
        {
            get
            {
                List<String> path = new List<String>();

                Node tmp = this;

                while (tmp.Parent != null)
                {
                    path.Add(tmp.Name);
                    tmp = tmp.Parent;
                }

                path.Reverse();

                return String.Join(".", path.ToArray());
            }
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        ///
        /// <value>
        /// The root.
        /// </value>
        [XmlIgnore]
        public Node Root
        {
            get
            {
                Node tmp = this;

                while (tmp.Parent != null)
                {
                    tmp = tmp.Parent;
                }

                return tmp;
            }
        }

        /// <summary>
        /// Gets the storage location.
        /// </summary>
        ///
        /// <value>
        /// The storage location.
        /// </value>
        [XmlIgnore]
        public StorageLocations StorageLocation
        {
            get
            {
                if (storageLocation == StorageLocations.Inherited && Parent != null)
                {
                    return Parent.StorageLocation;
                }
                else
                {
                    return storageLocation;
                }
            }
            private set
            {
                storageLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the Node.
        /// </summary>
        ///
        /// <value>
        /// The value.
        /// </value>
        public Object Value
        {
            get
            {
                switch (StorageLocation)
                {
                    case StorageLocations.Game:
                        {
                            if (!(Root == null || Root.Owner == null || String.IsNullOrEmpty(Root.Purpose)))
                            {
                                IVirtualProperties ds = Root.Owner.GetInterface<IVirtualProperties>();

                                if (ds != null)
                                {
                                    return ds.LookupValue(Root.Purpose, Name);
                                }


                                //! Bridge not present.
                                // 
                                return null;
                            }
                            else
                            {
                                //! BUG Owner is not correct after FromBinary followed by ToXml(false).
                                return null;
                            }
                        }
                    default:
                        return this.value;
                }
            }
            set
            {
                switch (StorageLocation)
                {
                    case StorageLocations.Game:
                        //! Should not happen.
                        break;
                    default:
                        //! TODO Add Dirty Bit?
                        this.value = value;
                        break;
                }
            }
        }

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Indexer to get items within this collection using array index syntax.
        /// </summary>
        ///
        /// <param name="index"> Zero-based index of the entry to access. </param>
        ///
        /// <returns>
        /// The indexed item.
        /// </returns>
        public Node this[int index]
        {
            //! Return the Node or it's Value?
            get
            {
                return children != null && index >= 0 && index < children.Count ? children[index] : null;
            }
        }

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
            //! Return the Node<T> or it's Value?
            get
            {
                if (children != null)
                {
                    foreach (Node child in children)
                    {
                        if (child.Name.Equals(name))
                        {
                            return child;
                        }
                    }
                }

                return null;
            }
        }

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Clears this object to its blank/initial state.
        /// </summary>
        public void Clear()
        {
            // Recusively Clear children in the correct order (
            foreach (Node child in this.PostfixEnumerator())
            {
                if (child.Count != 0)
                {
                    child.children.Clear();
                }
            }

            // Clear the node itself too
            if (Count != 0)
            {
                children.Clear();
            }
        }

        /// <summary>
        /// Clears the data described by location.
        /// </summary>
        ///
        /// <param name="location"> The location. </param>
        public void ClearData(StorageLocations location)
        {
            //! Recusively Clear Data.
            foreach (Node child in this.PostfixEnumerator(new List<StorageLocations> { location }))
            {
                child.Value = null;
            }
        }

        /// <summary>
        /// Adds a child to 'Value'.
        /// </summary>
        ///
        /// <param name="Name">  The name. </param>
        /// <param name="Value"> The value. </param>
        ///
        /// <returns>
        /// A Node&lt;T&gt;
        /// </returns>
        public Node AddChild(String Name, StorageLocations Location)
        {
            Node node = new Node();
            node.Name = Name;
            node.storageLocation = Location;

            AddChild(node);

            return node;
        }

        /// <summary>
        /// Adds a child to 'Value'.
        /// </summary>
        ///
        /// <param name="Name">  The name. </param>
        /// <param name="Value"> The value. </param>
        ///
        /// <returns>
        /// A Node&lt;T&gt;
        /// </returns>
        public Node AddChild(String Name, Object Value)
        {
            return AddChild(Name, Value, StorageLocations.Inherited);
        }

        /// <summary>
        /// Adds a child to 'Value'.
        /// </summary>
        ///
        /// <param name="Name">     The name. </param>
        /// <param name="Value">    The value. </param>
        /// <param name="Location"> The location. </param>
        ///
        /// <returns>
        /// A Node&lt;T&gt;
        /// </returns>
        public Node AddChild(String Name, Object Value, StorageLocations Location)
        {
            if (children == null)
            {
                children = new List<Node>();
            }

            Node child = new Node(this, Name, Value, Location);

            children.Add(child);

            return child;
        }

        /// <summary>
        /// Adds a child to 'Value'.
        /// </summary>
        ///
        /// <param name="node"> The node. </param>
        private void AddChild(Node node)
        {
            node.Parent = this;
            if (children == null)
            {
                children = new List<Node>();
            }
            children.Add(node);

            //this.AddChild(node.Name, node.Value, node.StorageLocation).childs = node.childs;
        }

        /// <summary>
        /// Tests if this Node is considered equal to another.
        /// </summary>
        ///
        /// <param name="value"> The value. </param>
        ///
        /// <returns>
        /// true if the objects are considered equal, false if they are not.
        /// </returns>
        public bool Equals(Node value)
        {
            return this == value;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Node's Childrens.
        /// </summary>
        ///
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be
        /// used to iterate through the collection.
        /// </returns>
        //public IEnumerator GetEnumerator()
        //{
        //    if (children != null)
        //    {
        //        return children.GetEnumerator();
        //    }
        //    else
        //    {
        //        return new object[0].GetEnumerator();
        //    }
        //}

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        ///
        /// <param name="obj">  The <see cref="T:System.Object" /> for which a hash
        ///                     code is to be returned. </param>
        ///
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        public int GetHashCode(object obj)
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        ///
        /// <param name="x"> The first object to compare. </param>
        /// <param name="y"> The second object to compare. </param>
        ///
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        bool IEqualityComparer.Equals(object x, object y)
        {
            return Equals(x as Node, y as Node);
        }

        /// <summary>
        /// Returns a postfix enumerator (depth-first)in this collection. 
        /// It visits 'leaves' before 'nodes'.
        /// </summary>
        ///
        /// <returns>
        /// An enumerator that allows foreach to be used to process post enumerator
        /// in this collection.
        /// </returns>
        public IEnumerable<Node> PostfixEnumerator()
        {
            //return PostfixEnumerator(new List<StorageLocations>());

            for (int i = 0; i < Count; i++)
            {
                foreach (Node child in this[i].PostfixEnumerator())
                {
                    yield return child;
                }
            }

            if (!IsRoot)
            {
                yield return this;
            }
        }

        /// <summary>
        /// Returns a postfix enumerator (depth-first)in this collection.
        /// It visits 'leaves' before 'nodes'.
        /// </summary>
        ///
        /// <param name="filter"> Specifies the filter. </param>
        ///
        /// <returns>
        /// An enumerator that allows foreach to be used to process post enumerator
        /// in this collection.
        /// </returns>
        public IEnumerable<Node> PostfixEnumerator(List<StorageLocations> filter)
        {
            foreach (Node node in PostfixEnumerator())
            {
                //! veg: skip root item?
                // 
                if (!node.IsRoot && (filter.Count == 0 || filter.Contains(node.StorageLocation)))
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Returns a prefix enumerator (depth-first) in this tree. It visits 'nodes'
        /// before 'leaves'.
        /// </summary>
        ///
        /// <remarks>
        /// this enumerator allows building &amp; filling the structure.
        /// </remarks>
        ///
        /// <returns>
        /// An enumerator that allows foreach to be used to process prefix enumerator
        /// in this collection.
        /// </returns>
        public IEnumerable<Node> PrefixEnumerator()
        {
            if (!IsRoot)
            {
                yield return this;
            }

            for (int i = 0; i < Count; i++)
            {
                foreach (Node child in this[i].PrefixEnumerator())
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Returns a prefix enumerator (depth-first) in this tree.
        /// It visits 'nodes' before 'leaves'.
        /// </summary>
        ///
        /// <remarks>
        /// this enumerator allows building &amp; filling the structure.
        /// </remarks>
        ///
        /// <param name="filter"> Specifies the filter. </param>
        ///
        /// <returns>
        /// An enumerator that allows foreach to be used to process prefix enumerator
        /// in this tree.
        /// </returns>
        public IEnumerable<Node> PrefixEnumerator(List<StorageLocations> filter)
        {
            foreach (Node node in PrefixEnumerator())
            {
                //! veg: skip root item?
                // 
                if (!node.IsRoot && (filter.Count == 0 || filter.Contains(node.StorageLocation)))
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Removes the given item.
        /// </summary>
        ///
        /// <param name="item"> The item to remove. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public bool Remove(Node item)
        {
            return children != null ? children.Remove(item) : false;
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the
        /// IXmlSerializable interface, you should return null (Nothing in Visual
        /// Basic) from this method, and instead, if specifying a custom schema is
        /// required, apply the
        /// <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to
        /// the class.
        /// </summary>
        ///
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML
        /// representation of the object that is produced by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" />
        /// method and consumed by the
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        /// method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        ///
        /// <param name="reader">   The <see cref="T:System.Xml.XmlReader" /> stream
        ///                         from which the object is deserialized. </param>
        public void ReadXml(XmlReader reader)
        {
            //! 1) Read <node> attributes.
            // 
            this.Name = reader["name"];
            String loc = reader["location"];

            //! 2) Process location attribute.

            if (String.IsNullOrEmpty(loc))
            {
                this.storageLocation = StorageLocations.Inherited;
            }
            else
            {
                this.storageLocation = (StorageLocations)Enum.Parse(typeof(StorageLocations), loc);
            }

            //! 3) First tag can be either <value> or <children>.
            // 
            reader.ReadStartElement();

            //! 4) Process <value> if present.
            // 
            //            if (reader.IsStartElement("value"))
            //            {
            //                //! 5) Move to value content tag.
            //                // 
            //                reader.ReadStartElement();

            //                //! 6) Check for supported non-simple types (List<Int32> in this case).
            //                //     See https://msdn.microsoft.com/en-us/library/ms531031(v=vs.85).aspx
            //                if (typeMapper.ContainsKey(reader.Name))
            //                {
            //                    //! 7) Process non-simple type (these have xmlns attributes).
            //                    // 
            //                    this.Value = GetSerializer(typeMapper[reader.Name]).Deserialize(reader);
            //                }
            //                else if (reader.Name.Equals("base64Binary"))
            //                {
            //                    this.Value = reader.ReadElementContentAsObject();
            //                }
            //                else {
            //                    //! 8) Process plain value types.
            //                    // 
            //                    if (Type.GetType(reader.Name) != null)
            //                    {
            //                        typeMapper.Add(reader.Name, Type.GetType(reader.Name));
            //                        this.Value = GetSerializer(typeMapper[reader.Name]).Deserialize(reader);
            //                    }
            //                    else {
            //#warning reads all things as a string.
            //                        this.Value = reader.ReadElementContentAsObject();
            //                    }
            //                }

            //                //! 9) Skip </value>
            //                // 
            //                reader.ReadEndElement();
            //            }

            //! 10) Check for non-empty children tag (if present it's not empty).
            // 
            if (reader.IsStartElement("children"))
            {
                //! 11) Read child count.
                // 
                Int32 count = Int32.Parse(reader["count"]);

                //! 12) Read children opening tag.
                // 
                reader.ReadStartElement();

                //! 13) Process child nodes.
                //
                for (Int32 i = 0; i < count; i++)
                {
                    Node node = (Node)GetSerializer(this.GetType()).Deserialize(reader);

                    //Debug.Print("Adding {0} to {1}", node.Name, this.Name);

                    this.AddChild(node);
                }

                //! 14) Skip </children>
                reader.ReadEndElement();

                //! 15) Skip </node>.
                reader.ReadEndElement();
            }
            //else
            //{
            //    //! 15) Skip </node>.
            //    //reader.ReadEndElement();
            //}

            ////! 15) Skip </node>.
            //// 
            //if (!reader.IsStartElement("node"))
            //{
            //    //reader.ReadEndElement();
            //}
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        ///
        /// <param name="writer">   The <see cref="T:System.Xml.XmlWriter" /> stream
        ///                         to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
        {
            //Debug.Print(this.Name);

            //! 1) Add name as an attribute to <node>.
            // 
            writer.WriteAttributeString("name", this.Name);

            if (IsRoot)
            {
                writer.WriteAttributeString("purpose", this.Purpose);
            }

            // Debug Code.
            // writer.WriteAttributeString("path", this.Path);

            //! 2) Add StorageLocation as an attribute to <node> if not inherited.
            //!    Note we store the actual value and not the resoled one.  
            // 
            if (this.storageLocation != StorageLocations.Inherited)
            {
                writer.WriteAttributeString("location", this.storageLocation.ToString());
            }

            //! 3) If there is a value present, serialize it as a <value> tag.
            //!    Using the Value property is not an option as it tries to retrieve Virtual Propeties too.
            //!    The model being serialized seems a kind of incomplete copy.
            // 
            if (!xmlStructureOnly && this.value != null)
            {
#warning DEBUG CODE
                writer.WriteStartElement("value");
                GetSerializer(this.value.GetType()).Serialize(writer, this.value);
                writer.WriteEndElement();
            }

            //! 4) If there are children present, serialize it as a <children> tag.
            if (this.Count != 0)
            {
                writer.WriteStartElement("children");

                //! 5) Add (child)count as an attribute to <children>
                //
                writer.WriteAttributeString("count", this.Count.ToString());

                //! 6) Serialize all children (recursively).
                foreach (Node node in this.Children)
                {
                    GetSerializer(node.GetType()).Serialize(writer, node);
                }

                //! 7) Write </children> tag. 
                // 
                writer.WriteEndElement();
            }
        }

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
                // Log(Severity.Verbose, "Caching XmlSerializer for {0}", type.FullName);

                serializers.Add(type, new XmlSerializer(type));
                //serializers.Add(type, XmlSerializer.FromTypes(new[] { type })[0]);
            }

            return serializers[type];
        }

        /// <summary>
        /// Converts this object to an XML.
        /// </summary>
        ///
        /// <returns>
        /// This object as a String.
        /// </returns>
        public String ToXml(Boolean structureOnly = true)
        {
            using (StringWriterUtf8 textWriter = new StringWriterUtf8())
            {
#warning DEBUG CODE
                xmlStructureOnly = structureOnly;

                XmlSerializer ser = GetSerializer(GetType());

                ser.Serialize(textWriter, this);

                textWriter.Flush();

#warning DEBUG CODE
                xmlStructureOnly = true;

                return textWriter.ToString();
            }
        }

#if BINARY
        /// <summary>
        /// Convert this object into a binary (Base64 Encoded) representation.
        /// </summary>
        ///
        /// <param name="structureOnly"> true to serialize only the structure. </param>
        ///
        /// <returns>
        /// A binary representation of this object.
        /// </returns>
        public String ToBinary(Boolean structureOnly = false)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                StreamingContext sc = new StreamingContext(StreamingContextStates.All, structureOnly);

                new BinaryFormatter(null, sc).Serialize(ms, this);
                ms.Flush();

                return Convert.ToBase64String(ms.ToArray());
            }
        }
#endif

#if BINARY

        /// <summary>
        /// From binary.
        /// </summary>
        ///
        /// <param name="base64">        The fourth base 6. </param>
        /// <param name="structureOnly">    true to serialize only the structure. </param>
        public void FromBinary(String base64, Boolean structureOnly = false)
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
            {
                StreamingContext sc = new StreamingContext(StreamingContextStates.All, structureOnly);

                //! Deserializes the whole tree.
                //
                Node tmp = (Node)new BinaryFormatter(null, sc).Deserialize(ms);

                //! We now have a complete deserialized tree in tmp, including a new root node.
                //! We now have to copy some fields from tmp to this, the root node
                //! of the model in the GameStorageClientAsset Models collection.

                //! Purpose and Owner are still set correctly in the Models Node (a.k.a this) 
                //! as we are only restoring it's children and storageLocation backing fields.
                // 
                //! tmp.Name should be 'root' and tmp.Parent and tmp.Value should both be null.
                // 
                this.children = tmp.children;

                //this.Purpose = tmp.Purpose;

                this.storageLocation = tmp.storageLocation;
                if (!structureOnly)
                {
                    //! Root should not have  a value.
                    // 
                    this.Value = tmp.Value;
                }
            }
        }
#endif

        /// <summary>
        /// Initializes this object from the given from XML.
        /// </summary>
        ///
        /// <param name="xml"> The XML. </param>
        ///
        /// <returns>
        /// A Node&lt;T&gt;
        /// </returns>
        public void FromXml(String xml)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Byte[] bytes = Encoding.UTF8.GetBytes(xml);
                ms.Write(bytes, 0, bytes.Length);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                XmlSerializer ser = GetSerializer(GetType());

                Node tmp = (Node)ser.Deserialize(ms);

                this.Name = tmp.Name;
                this.Parent = tmp.Parent;
                this.children = tmp.children;
                this.storageLocation = tmp.storageLocation;
                this.Value = tmp.Value;
            }
        }

#if BINARY

        /// <summary>
        /// Populates a
        /// <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the
        /// data needed to serialize the target object.
        /// </summary>
        ///
        /// <param name="info">     The
        ///                         <see cref="T:System.Runtime.Serialization.SerializationInfo" />
        ///                         to populate with data. </param>
        /// <param name="context">  The destination (see
        ///                         <see cref="T:System.Runtime.Serialization.StreamingContext" />)
        ///                         for this serialization. </param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", this.Name);
            info.AddValue("location", this.storageLocation);
            info.AddValue("count", Count);

            //if (Purpose != null)
            //{
            //    info.AddValue("purpose", Count);
            //}

            if (this.Count != 0)
            {
                for (Int32 i = 0; i < Count; i++)
                {
                    info.AddValue(String.Format("child_{0}", i), this.Children[i], typeof(Node));
                }
            }

            if ((context.Context == null || context.Context.Equals(false)) && this.Value != null)
            {
                info.AddValue("type", this.Value.GetType());
                info.AddValue("value", this.Value);
            }
        }
#endif

        #endregion Methods
    }


}