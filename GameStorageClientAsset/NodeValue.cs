namespace AssetPackage
{
    using System;
    using System.Collections.Generic;
    public class BaseNodePoc
    {
        public String Path;
        public String ValueType;

        public String Value;

        internal Object ValueAsObject;
    }

    /// <summary>
    /// (Serializable)nodes  'Plain Old Class', used for building an array of
    /// (de)serialized values.
    /// </summary>
    ///
    /// <remarks>
    /// No properties as Unity3D will, also case sensitive.
    /// </remarks>
    //[Serializable]
    //public class NodesPocOut
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the AssetPackage.NodesPocOut class.
    //    /// </summary>
    //    public NodesPocOut()
    //    {
    //        nodes = new NodePocOut<Object>[0];
    //    }

    //    /// <summary>
    //    /// The nodes.
    //    /// </summary>
    //    public NodePocOut<Object>[] nodes;
    //}

    [Serializable]
    public class NodesPocIn
    {
        /// <summary>
        /// Initializes a new instance of the AssetPackage.NodesPocIn class.
        /// </summary>
        public NodesPocIn()
        {
            nodes = new NodePocIn<String>[0];
        }

        /// <summary>
        /// The nodes.
        /// </summary>
        public NodePocIn<String>[] nodes;
    }

    /// <summary>
    /// (Serializable)A node 'Plain Old Class'.
    /// </summary>
    ///
    /// <remarks>
    /// No properties as Unity3D will, also case sensitive.
    /// </remarks>
    //[Serializable]
    //public class NodePocOut<T> : BaseNodePoc
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the AssetPackage.NodePoc&lt;T&gt; class.
    //    /// </summary>
    //    ///
    //    /// <param name="Path">  Full pathname of the file. </param>
    //    /// <param name="Value"> The value. </param>
    //    public NodePocOut(String Path, T Value)
    //    {
    //        this.Path = Path;
    //        ValueType = typeof(T).FullName;
    //        this.Value = Value;
    //    }

    //    public new T Value;
    //}

    /// <summary>
    /// (Serializable)A node 'Plain Old Class'.
    /// </summary>
    ///
    /// <remarks>
    /// No properties as Unity3D will, also case sensitive.
    /// </remarks>
    [Serializable]
    public class NodePocIn<T> : BaseNodePoc
    {
        /// <summary>
        /// Initializes a new instance of the AssetPackage.NodePoc&lt;T&gt; class.
        /// </summary>
        ///
        /// <param name="Path">  Full pathname of the file. </param>
        /// <param name="Value"> The value. </param>
        public NodePocIn(String Path, String Value)
        {
            this.Path = Path;
            ValueType = typeof(T).FullName;
            this.Value = Value.ToString();
            this.ValueAsObject = default(T);
        }

        public new T ValueAsObject;
    }

    public interface IPocValue
    {
        Object GetValue();
        void SetValue(Object Value);

        String GetPath();
        void SetPath(String Path);

        String GetValueType();
        void SetValueType(String Path);
    }

    [Serializable]
    public class PocStringValues
    {
        public PocStringValues()
        {
            nodes = new PocStringValue[0];
        }

        public PocStringValue[] nodes;
    }

    [Serializable]
    public class PocStringValue
    {
        public String Value;
        public String Path;
        public String ValueType;
    }

    [Serializable]
    public class PocValues
    {
        public PocValues()
        {
            nodes = new IPocValue[0];
        }

        public IPocValue[] nodes;
    }

    [Serializable]
    public class PocValue<T> : IPocValue
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
