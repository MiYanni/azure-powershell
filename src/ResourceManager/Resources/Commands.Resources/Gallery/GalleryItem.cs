// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Hyak.Common;

namespace Microsoft.Azure.Commands.Resources.Models.Gallery
{
    public partial class GalleryItem
    {
        private IList<Artifact> _artifacts;

        /// <summary>
        /// Optional. Gets or sets gallery item artifacts.
        /// </summary>
        public IList<Artifact> Artifacts
        {
            get { return _artifacts; }
            set { _artifacts = value; }
        }

        private IList<string> _categories;

        /// <summary>
        /// Optional. Gets or sets gallery item category identifiers.
        /// </summary>
        public IList<string> Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        private DefinitionTemplates _definitionTemplates;

        /// <summary>
        /// Optional. Gets or sets gallery item definition template.
        /// </summary>
        public DefinitionTemplates DefinitionTemplates
        {
            get { return _definitionTemplates; }
            set { _definitionTemplates = value; }
        }

        private string _description;

        /// <summary>
        /// Optional. Gets or sets gallery item description.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private string _displayName;

        /// <summary>
        /// Optional. Gets or sets gallery item display name.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        private IList<Filter> _filters;

        /// <summary>
        /// Optional. Gets or sets gallery item filters.
        /// </summary>
        public IList<Filter> Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }

        private IDictionary<string, string> _iconFileUris;

        /// <summary>
        /// Optional. Gets or sets gallery item screenshot Uris
        /// </summary>
        public IDictionary<string, string> IconFileUris
        {
            get { return _iconFileUris; }
            set { _iconFileUris = value; }
        }

        private string _identity;

        /// <summary>
        /// Optional. Gets or sets gallery item identity.
        /// </summary>
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        private IList<Link> _links;

        /// <summary>
        /// Optional. Gets or sets gallery item links.
        /// </summary>
        public IList<Link> Links
        {
            get { return _links; }
            set { _links = value; }
        }

        private string _longSummary;

        /// <summary>
        /// Optional. Gets or sets gallery item long summary.
        /// </summary>
        public string LongSummary
        {
            get { return _longSummary; }
            set { _longSummary = value; }
        }

        private MarketingMaterial _marketingMaterial;

        /// <summary>
        /// Optional. Gets or sets gallery item marketing information.
        /// </summary>
        public MarketingMaterial MarketingMaterial
        {
            get { return _marketingMaterial; }
            set { _marketingMaterial = value; }
        }

        private IDictionary<string, string> _metadata;

        /// <summary>
        /// Optional. Gets or sets gallery item metadata.
        /// </summary>
        public IDictionary<string, string> Metadata
        {
            get { return _metadata; }
            set { _metadata = value; }
        }

        private string _name;

        /// <summary>
        /// Optional. Gets or sets gallery item name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private IList<Product> _products;

        /// <summary>
        /// Optional. Gets or sets gallery item product definition.
        /// </summary>
        public IList<Product> Products
        {
            get { return _products; }
            set { _products = value; }
        }

        private IDictionary<string, string> _properties;

        /// <summary>
        /// Optional. Gets or sets gallery item user visible properties.
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        private string _publisher;

        /// <summary>
        /// Optional. Gets or sets gallery item publisher.
        /// </summary>
        public string Publisher
        {
            get { return _publisher; }
            set { _publisher = value; }
        }

        private string _publisherDisplayName;

        /// <summary>
        /// Optional. Gets or sets gallery item publisher display name.
        /// </summary>
        public string PublisherDisplayName
        {
            get { return _publisherDisplayName; }
            set { _publisherDisplayName = value; }
        }

        private IList<string> _screenshotUris;

        /// <summary>
        /// Optional. Gets or sets gallery item screenshot Uris
        /// </summary>
        public IList<string> ScreenshotUris
        {
            get { return _screenshotUris; }
            set { _screenshotUris = value; }
        }

        private string _summary;

        /// <summary>
        /// Optional. Gets or sets gallery item summary.
        /// </summary>
        public string Summary
        {
            get { return _summary; }
            set { _summary = value; }
        }

        private string _uiDefinitionUri;

        /// <summary>
        /// Optional. Gets or sets Azure Portal Uder Interface Definition
        /// artificat Uri.
        /// </summary>
        public string UiDefinitionUri
        {
            get { return _uiDefinitionUri; }
            set { _uiDefinitionUri = value; }
        }

        private string _version;

        /// <summary>
        /// Optional. Gets or sets gallery item version.
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// Initializes a new instance of the GalleryItem class.
        /// </summary>
        public GalleryItem()
        {
            Artifacts = new LazyList<Artifact>();
            Categories = new LazyList<string>();
            Filters = new LazyList<Filter>();
            IconFileUris = new LazyDictionary<string, string>();
            Links = new LazyList<Link>();
            Metadata = new LazyDictionary<string, string>();
            Products = new LazyList<Product>();
            Properties = new LazyDictionary<string, string>();
            ScreenshotUris = new LazyList<string>();
        }
    }
}