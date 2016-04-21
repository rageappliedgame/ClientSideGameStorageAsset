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
    /// Interface for poc value classes.
    /// </summary>
    public interface IPocValue
    {
        Object GetValue();
        void SetValue(Object Value);

        String GetPath();
        void SetPath(String Path);

        String GetValueType();
        void SetValueType(String Path);
    }

    /// <summary>
    /// (Serializable)a poc string values. Used to deserialize the Json nodes
    /// Array as all Values are encoded as string.
    /// </summary>
    [Serializable]
    public class PocStringValues
    {
        public PocStringValues()
        {
            nodes = new PocStringValue[0];
        }

        public PocStringValue[] nodes;
    }

    /// <summary>
    /// (Serializable)a poc string value.
    /// </summary>
    [Serializable]
    public class PocStringValue
    {
        public String Value;
        public String Path;
        public String ValueType;
    }

    /// <summary>
    /// A poc object value with an additional object to store deserialzied values.
    /// </summary>
    public class PocObjectValue : PocStringValue
    {
        internal Object ValueAsObject;
    }

    /// <summary>
    /// (Serializable)a poc value. It is used druing deserialization of Json to
    /// cast values dynamically.
    /// </summary>
    ///
    /// <typeparam name="T"> Generic type parameter. </typeparam>
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
