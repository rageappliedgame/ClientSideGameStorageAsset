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
    /// Interface for virtual properties.
    /// </summary>
    public interface IVirtualProperties
    {
        #region Methods

        /// <summary>
        /// Looks up a given key to find its associated value.
        /// </summary>
        ///
        /// <param name="model"> The model. </param>
        /// <param name="key">   The key. </param>
        ///
        /// <returns>
        /// An Object.
        /// </returns>
        Object LookupValue(String model, String key);

        #endregion Methods
    }
}