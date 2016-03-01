﻿using System;
using PDS.Witsml.Server.Logging;

namespace PDS.Witsml.Server.Configuration
{
    /// <summary>
    /// Encapsulates all of the input parameters for each of the WITSML Store API methods.
    /// </summary>
    public struct RequestContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestContext"/> struct.
        /// </summary>
        /// <param name="function">The WITSML Store API function.</param>
        /// <param name="objectType">The WITSML type of data object.</param>
        /// <param name="xml">The XML for the data object.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="capabilities">The client's capabilities (capClient).</param>
        public RequestContext(Functions function, string objectType, string xml, string options, string capabilities)
        {
            Function = function;
            ObjectType = objectType;
            Xml = xml;
            Options = options;
            Capabilities = capabilities;
        }

        /// <summary>
        /// Gets the WITSML Store API function.
        /// </summary>
        /// <value>The function.</value>
        public Functions Function { get; private set; }

        /// <summary>
        /// Gets the WITSML type of the data object.
        /// </summary>
        /// <value>The WITSML type.</value>
        public string ObjectType { get; private set; }

        /// <summary>
        /// Gets the XML for the data object.
        /// </summary>
        /// <value>The XML string.</value>
        public string Xml { get; private set; }

        /// <summary>
        /// Gets the configuration options.
        /// </summary>
        /// <value>The configuration options.</value>
        public string Options { get; private set; }

        /// <summary>
        /// Gets the client's capabilities object.
        /// </summary>
        /// <value>The client's capabilities.</value>
        public string Capabilities { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(
                "{0}: Type: {1}; Options: {2}; CapClient:{5}{3}{5}XML:{5}{4}{5}",
                Function,
                ObjectType,
                Options,
                Capabilities,
                DebugExtensions.Format(Xml),
                Environment.NewLine);
        }
    }
}