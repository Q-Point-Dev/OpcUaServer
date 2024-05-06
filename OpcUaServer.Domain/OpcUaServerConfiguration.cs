/*
 * @license
 * © 2023 Ammann-Group Switzerland. All rights reserved
 * Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
 * purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.
 */

using System;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Opc.Ua.Server;

namespace OpcUaServer.Domain
{
    /// <summary>
    /// Stores the configuration the data access node manager.
    /// </summary>
    [DataContract(Namespace=OpcUaNamespaces.Empty)]
    public class OpcUaServerConfiguration
    {
        #region Constructors
        /// <summary>
        /// The default constructor.
        /// </summary>
        public OpcUaServerConfiguration()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the object during deserialization.
        /// </summary>
        [OnDeserializing()]
        private void Initialize(StreamingContext context)
        {
            Initialize();
        }

        /// <summary>
        /// Sets private members to default values.
        /// </summary>
        private void Initialize()
        {
        }
        #endregion

        #region Public Properties
        #endregion

        #region Private Members
        #endregion
    }
}
