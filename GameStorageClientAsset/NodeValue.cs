namespace AssetPackage
{
    using System;

    /// <summary>
    /// A node value, used for building an array of serialized values.
    /// </summary>
    public class NodeValue
    {
        /// <summary>
        /// Gets or sets the full pathname of the file.
        /// </summary>
        ///
        /// <value>
        /// The full pathname of the file.
        /// </value>
        public String Path { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        ///
        /// <value>
        /// The type.
        /// </value>
        public String ValueType { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        ///
        /// <value>
        /// The value.
        /// </value>
        public Object Value { get; set; }
    }
}
