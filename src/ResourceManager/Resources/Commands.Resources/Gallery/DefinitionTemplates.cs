// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Hyak.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.Resources.Models.Gallery
{
    /// <summary>
    /// A gallery item definition template.
    /// </summary>
    public partial class DefinitionTemplates
    {
        private string _defaultDeploymentTemplateId;

        /// <summary>
        /// Optional. Gets or sets definition template file ID.
        /// </summary>
        public string DefaultDeploymentTemplateId
        {
            get { return _defaultDeploymentTemplateId; }
            set { _defaultDeploymentTemplateId = value; }
        }

        private IDictionary<string, string> _deploymentTemplateFileUrls;

        public IDictionary<string, string> DeploymentTemplateFileUrls
        {
            get { return _deploymentTemplateFileUrls; }
            set { _deploymentTemplateFileUrls = value; }
        }

        /// <summary>
        /// Initializes a new instance of the DefinitionTemplates class.
        /// </summary>
        public DefinitionTemplates()
        {
            DeploymentTemplateFileUrls = new LazyDictionary<string, string>();
        }
    }
}