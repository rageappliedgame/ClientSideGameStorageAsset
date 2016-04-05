using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetPackage
{
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
