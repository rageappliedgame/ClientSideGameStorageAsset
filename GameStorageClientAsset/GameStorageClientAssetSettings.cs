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
    /// A tracker asset settings.
    /// </summary>
    public class GameStorageClientAssetSettings : BaseSettings
    {
        /// <summary>
        /// Initializes a new instance of the AssetPackage.TrackerAssetSettings
        /// class.
        /// </summary>
        public GameStorageClientAssetSettings() : base()
        {
            // Apply 'Factory' defaults.
            // 
            Port = 3000;
            Secure = false;
        }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        ///
        /// <value>
        /// The host.
        /// </value>
        public String Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the A2 Authentication Service port.
        /// </summary>
        ///
        /// <value>
        /// The host.
        /// </value>
        public int A2Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the GameStorage Service port.
        /// </summary>
        ///
        /// <value>
        /// The host.
        /// </value>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use http or https.
        /// </summary>
        ///
        /// <value>
        /// true if secure, false if not.
        /// </value>
        public bool Secure
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full pathname of the base file.
        /// </summary>
        ///
        /// <remarks>
        /// Should either be empty or else start with a /. Should not include a
        /// trailing /.
        /// </remarks>
        ///
        /// <value>
        /// The full pathname of the base file.
        /// </value>
        public String BasePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user Authentication token.
        /// </summary>
        ///
        /// <value>
        /// The user token.
        /// </value>
        public String UserToken
        {
            get;
            set;
        }
    }
}
