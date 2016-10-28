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
#if PORTABLE
    using AssetManagerPackage; //fixup for missing ISerializable interface
#else
    using System.Runtime.Serialization.Formatters.Binary;
#endif

    #region Enumerations

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

#if PORTABLE
#else
    [Serializable]
#endif
    [DebuggerDisplay("Name={Name}, Path={Path}, Count={Count}")]
    public class Node : IEqualityComparer
#if XML
        , IXmlSerializable 
#endif
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
        /// The owner, only valid on Root Node.
        /// </summary>
        private GameStorageClientAsset Owner = null;

        /// <summary>
        /// The purpose, only valid on Root Node.
        /// </summary>
        public String Purpose
        {
            get;
            internal set;
        }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes static members of the AssetPackage.Node&lt;T&gt; class.
        /// </summary>
        static Node()
        {
            //
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
        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the children.
        /// </summary>
        ///
        /// <value>
        /// The children.
        /// </value>
        public List<Node> Children
        {
            get
            {
                return children;
            }
            //            set
            //            {
            //#warning Added for testing newtonsoft
            //                children = value;
            //            }
        }

        /// <summary>
        /// Gets the number of Children. 
        /// </summary>
        ///
        /// <value>
        /// The count.
        /// </value>
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
        public String Name { get; internal set; }

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
            internal set
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
        /// Converts this object to a node structure.
        /// </summary>
        ///
        /// <returns>
        /// This object as a NodeStructure.
        /// </returns>
        public NodePath ToNodeStructure()
        {
            return IsRoot ?
                new NodePath(this.Path, this.storageLocation, this.Purpose) :
                (storageLocation == StorageLocations.Inherited ?
                    new NodePath(this.Path) :
                    new NodePath(this.Path, this.storageLocation)
                );
        }

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
        /// <param name="location"> The location to match when enumerating. </param>
        public void ClearData(StorageLocations location)
        {
            //! Recusively Clear Data.
            foreach (Node child in this.PostfixEnumerator(new List<StorageLocations> { location }))
            {
                child.Value = null;
            }
        }

        /// <summary>
        /// Clears the data described by location.
        /// </summary>
        ///
        /// <remarks>
        /// This method clears all writeable data.
        /// </remarks>
        public void ClearData()
        {
            ClearData(GameStorageClientAsset.AllStorageLocations);
        }

        /// <summary>
        /// Clears the data described by location.
        /// </summary>
        ///
        /// <param name="enumeration">  The list of locations to match when enumerating. </param>
        public void ClearData(List<StorageLocations> enumeration)
        {
            //! Recursively Clear Data. 
            foreach (Node child in this.PostfixEnumerator(enumeration))
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
        public enum ToStringSaveOptions
        {
            Data,
            Structure,
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        ///
        /// <returns>
        /// A String that represents this object.
        /// </returns>
        public override String ToString()
        {
            return ToString(true, SerializingFormat.Xml);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        ///
        /// <remarks>
        /// 1) This method might not show the complete tree when StructureOnly=false as null data is
        /// ommitted.
        /// 
        /// 2) When StructureOnly = true it should show the whole structure.
        /// </remarks>
        ///
        /// <remarks>
        /// For testing purposes only as it stores everything.
        /// </remarks>
        ///
        /// <param name="StructureOnly">    Options for controlling the operation. </param>
        /// <param name="format">           (Optional) Describes the format to use. </param>
        /// <param name="enumeration">
        /// (Optional)
        /// The list of locations to match when enumerating. </param>
        ///
        /// <returns>
        /// A String that represents this object.
        /// </returns>
        public String ToString(Boolean StructureOnly, SerializingFormat format = SerializingFormat.Xml, List<StorageLocations> enumeration = null)
        {
            switch (StructureOnly)
            {
                case true:
                    return Root.Owner.SerializeStructure(
                        this.Purpose,
                        format);
                case false:
                    return Root.Owner.SerializeData(
                        this.Purpose,
                        StorageLocations.Local,
                        format,
                        enumeration == null ? GameStorageClientAsset.AllWriteableStorageLocations : new List<StorageLocations> { StorageLocations.Local });
            }

            return String.Empty;
        }

        /// <summary>
        /// From string.
        /// </summary>
        ///
        /// <remarks>
        /// For testing purposes only as it restores everything.
        /// 
        /// Note: Game Location will not be restored.
        /// </remarks>
        ///
        /// <param name="data">             The data. </param>
        /// <param name="StructureOnly">    Options for controlling the operation. </param>
        /// <param name="format">           (Optional) Describes the format to use. </param>
        ///
        /// <returns>
        /// A Node.
        /// </returns>
        public Node FromString(String data, Boolean StructureOnly, SerializingFormat format = SerializingFormat.Xml, List<StorageLocations> enumeration = null)
        {
            switch (StructureOnly)
            {
                case true:
                    Root.Owner.DeSerializeStructure(
                        this.Purpose,
                        data,
                        format);
                    break;
                case false:
                    Root.Owner.DeSerializeData(
                        this.Purpose,
                        data,
                        StorageLocations.Local,
                        format,
                        enumeration == null ? GameStorageClientAsset.AllWriteableStorageLocations : new List<StorageLocations> { StorageLocations.Local });
                    break;
            }

            return Root.Owner[this.Purpose];
        }

        #endregion Methods
    }
}