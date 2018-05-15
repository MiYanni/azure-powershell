// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.Azure.Commands.Resources.Models.Gallery
{
    /// <summary>
    /// A gallery item artifact.
    /// </summary>
    public partial class Artifact
    {
        private string _name;

        /// <summary>
        /// Optional. Gets or sets artifact name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _type;

        /// <summary>
        /// Optional. Gets or sets artifact type.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _uri;

        /// <summary>
        /// Optional. Gets or sets artifact Uri.
        /// </summary>
        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        /// <summary>
        /// Initializes a new instance of the Artifact class.
        /// </summary>
        public Artifact()
        {
        }
    }
}