namespace AssetPackage
{
    using System;

    /// <summary>
    /// (Serializable)nodes  'Plain Old Class', used for building an array of
    /// (de)serialized values.
    /// </summary>
    ///
    /// <remarks>
    /// No properties as Unity3D will, also case sensitive.
    /// </remarks>
    [Serializable]
    public class NodesPoc
    {
        public NodesPoc()
        {
            nodes = new NodePoc[0];
        }
        /// <summary>
        /// The nodes.
        /// </summary>
        public NodePoc[] nodes;
    }

    /// <summary>
    /// (Serializable)A node 'Plain Old Class'.
    /// </summary>
    ///
    /// <remarks>
    /// No properties as Unity3D will, also case sensitive.
    /// </remarks>
    [Serializable]
    public class NodePoc
    {
        /// <summary>
        /// Initializes a new instance of the AssetPackage.NodeJson class.
        /// </summary>
        public NodePoc()
        {
            Path = String.Empty;
            Value = new ValuePoc();
        }

        /// <summary>
        /// Gets or sets the full pathname of the file.
        /// </summary>
        ///
        /// <value>
        /// The full pathname of the file.
        /// </value>
        public String Path;

        /// <summary>
        /// Gets the node value.
        /// </summary>
        ///
        /// <value>
        /// .
        /// </value>
        public ValuePoc Value;
    }

    /// <summary>
    /// (Serializable) A node value 'Plain Old Class'.
    /// </summary>
    ///
    /// <remarks>
    /// No properties as Unity3D will, also case sensitive.
    /// </remarks>
    [Serializable]
    public class ValuePoc
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        ///
        /// <value>
        /// The type.
        /// </value>
        public String ValueType;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        ///
        /// <value>
        /// The value.
        /// </value>
        public String Value;

        /// <summary>
        /// The object value.
        /// </summary>
        internal Object ObjectValue;
    }
}
