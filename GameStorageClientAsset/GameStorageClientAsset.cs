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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Serialization;

    //! Not sure if this needs to stay here. Better is using Dictionary of Models instead of nesting them.
    //! Make Models keys case-insesitive?
    // 
    //! Somewhat more utility methods like Remove, Clear!
    //  
    //[XmlRoot]
    public class GameStorageClientAsset : BaseAsset
    {
        /// <summary>
        /// Initializes a new instance of the AssetPackage.GameStorageClientAsset class.
        /// </summary>
        public GameStorageClientAsset()
        {
            Models = new Dictionary<String, Node>();
        }

        /// <summary>
        /// Adds a model.
        /// </summary>
        ///
        /// <param name="purpose"> The purpose. </param>
        public Node AddModel(String purpose)
        {
            Models.Add(purpose, new Node(this, purpose));

            return Models[purpose];
        }

#warning ADD SETTINGS CLASS

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
            get
            {
                return Models[name];
            }
            set
            {
                Models[name] = value;
            }
        }

        /// <summary>
        /// Indexer to get items within this collection using array index syntax.
        /// </summary>
        ///
        /// <param name="i"> The key index. </param>
        ///
        /// <returns>
        /// The indexed item.
        /// </returns>
        public Node this[Int32 i]
        {
            get
            {
                return Models[Models.Keys.ElementAt(i)];
            }
            set
            {
                Models[Models.Keys.ElementAt(i)] = value;
            }
        }

        /// <summary>
        /// Gets the number of Model Keys. 
        /// </summary>
        ///
        /// <value>
        /// The count.
        /// </value>
        public Int32 Count
        {
            get
            {
                return Models.Keys.Count;
            }
        }

        /// <summary>
        /// Storage of Multiple Models.
        /// </summary>
        ///
        /// <value>
        /// The models.
        /// </value>
        private Dictionary<String, Node> Models
        {
            // Make this private and add indexer on GameStorageAsset
            get;
            set;
        }

        /// <summary>
        /// Saves.
        /// </summary>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        public void SaveStructure(String model, StorageLocations location)
        {
            //! TODO Add binary parameter.
            // 
            if (Models.ContainsKey(model))
            {
                switch (location)
                {
                    case StorageLocations.Local:
                        IDataStorage storage = getInterface<IDataStorage>();

                        if (storage != null)
                        {
                            storage.Save(model + ".xml", Models[model].ToXml());
                        }
                        else
                        {
                            Debug.Print("IDataStorage interface not found a Bridge");
                        }
                        break;

                    default:
                        Debug.Print("Not implemented yet");
                        break;
                }
            }
            else
            {
                Debug.Print("Model not found");
            }
        }

        /// <summary>
        /// Loads a structure.
        /// </summary>
        ///
        /// <param name="model">    The model. </param>
        /// <param name="location"> The location. </param>
        public void LoadStructure(String model, StorageLocations location)
        {
            //! TODO Add binary parameter.
            // 
            switch (location)
            {
                case StorageLocations.Local:
                    IDataStorage storage = getInterface<IDataStorage>();

                    if (storage != null)
                    {
                        if (storage.Exists(model + ".xml"))
                        {
                            if (Models.ContainsKey(model))
                            {
#warning bit rough to clear all data.
                                Models[model].Clear();
                            }
                            else
                            {
                                Models[model] = new Node(this, model);
                            }

                            Models[model].FromXml(storage.Load(model + ".xml"));
                        }
                    }
                    else
                    {
                        Debug.Print("IDataStorage interface not found a Bridge");
                    }
                    break;

                default:
                    Debug.Print("Not implemented yet");
                    break;
            }
        }

        public void SaveData(String model, StorageLocations location)
        {
            if (Models.ContainsKey(model))
            {
                switch (location)
                {
                    case StorageLocations.Local:
                        IDataStorage storage = getInterface<IDataStorage>();

                        if (storage != null)
                        {
                            storage.Save(model + ".json", Models[model].ToJson(new List<StorageLocations> { location }));
                        }
                        else
                        {
                            Debug.Print("IDataStorage interface not found a Bridge");
                        }
                        break;

                    default:
                        Debug.Print("Not implemented yet");
                        break;
                }
            }
            else
            {
                Debug.Print("Model not found");
            }
        }
        /// <summary>
        /// Gets the interface.
        /// </summary>
        ///
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        ///
        /// <returns>
        /// The interface.
        /// </returns>
        internal T GetInterface<T>()
        {
            return base.getInterface<T>();
        }
    }
}
