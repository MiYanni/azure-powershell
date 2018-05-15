// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using Hyak.Common;
using Microsoft.Azure;

namespace Microsoft.Azure.Commands.Resources.Models.Gallery
{
    public partial class GalleryClient : ServiceClient<GalleryClient>, IGalleryClient
    {
        private string _apiVersion;

        /// <summary>
        /// Gets the API version.
        /// </summary>
        public string ApiVersion
        {
            get { return _apiVersion; }
        }

        private Uri _baseUri;

        /// <summary>
        /// Gets the URI used as the base for all cloud service requests.
        /// </summary>
        public Uri BaseUri
        {
            get { return _baseUri; }
        }

        private SubscriptionCloudCredentials _credentials;

        /// <summary>
        /// Gets subscription credentials which uniquely identify Microsoft
        /// Azure subscription. The subscription ID forms part of the URI for
        /// every service call.
        /// </summary>
        public SubscriptionCloudCredentials Credentials
        {
            get { return _credentials; }
        }

        private int _longRunningOperationInitialTimeout;

        /// <summary>
        /// Gets or sets the initial timeout for Long Running Operations.
        /// </summary>
        public int LongRunningOperationInitialTimeout
        {
            get { return _longRunningOperationInitialTimeout; }
            set { _longRunningOperationInitialTimeout = value; }
        }

        private int _longRunningOperationRetryTimeout;

        /// <summary>
        /// Gets or sets the retry timeout for Long Running Operations.
        /// </summary>
        public int LongRunningOperationRetryTimeout
        {
            get { return _longRunningOperationRetryTimeout; }
            set { _longRunningOperationRetryTimeout = value; }
        }

        private IItemOperations _items;

        /// <summary>
        /// Operations for working with gallery items.
        /// </summary>
        public virtual IItemOperations Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Initializes a new instance of the GalleryClient class.
        /// </summary>
        public GalleryClient()
            : base()
        {
            _items = new ItemOperations(this);
            _apiVersion = "2015-04-01";
            _longRunningOperationInitialTimeout = -1;
            _longRunningOperationRetryTimeout = -1;
            HttpClient.Timeout = TimeSpan.FromSeconds(300);
        }

        /// <summary>
        /// Initializes a new instance of the GalleryClient class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Gets subscription credentials which uniquely identify
        /// Microsoft Azure subscription. The subscription ID forms part of
        /// the URI for every service call.
        /// </param>
        /// <param name='baseUri'>
        /// Optional. Gets the URI used as the base for all cloud service
        /// requests.
        /// </param>
        public GalleryClient(SubscriptionCloudCredentials credentials, Uri baseUri)
            : this()
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
            _credentials = credentials;
            _baseUri = baseUri;

            Credentials.InitializeServiceClient(this);
        }

        /// <summary>
        /// Initializes a new instance of the GalleryClient class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Gets subscription credentials which uniquely identify
        /// Microsoft Azure subscription. The subscription ID forms part of
        /// the URI for every service call.
        /// </param>
        public GalleryClient(SubscriptionCloudCredentials credentials)
            : this()
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            _credentials = credentials;
            _baseUri = new Uri("https://gallery.azure.com/");

            Credentials.InitializeServiceClient(this);
        }

        /// <summary>
        /// Initializes a new instance of the GalleryClient class.
        /// </summary>
        /// <param name='httpClient'>
        /// The Http client
        /// </param>
        public GalleryClient(HttpClient httpClient)
            : base(httpClient)
        {
            _items = new ItemOperations(this);
            _apiVersion = "2015-04-01";
            _longRunningOperationInitialTimeout = -1;
            _longRunningOperationRetryTimeout = -1;
            HttpClient.Timeout = TimeSpan.FromSeconds(300);
        }

        /// <summary>
        /// Initializes a new instance of the GalleryClient class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Gets subscription credentials which uniquely identify
        /// Microsoft Azure subscription. The subscription ID forms part of
        /// the URI for every service call.
        /// </param>
        /// <param name='baseUri'>
        /// Optional. Gets the URI used as the base for all cloud service
        /// requests.
        /// </param>
        /// <param name='httpClient'>
        /// The Http client
        /// </param>
        public GalleryClient(SubscriptionCloudCredentials credentials, Uri baseUri, HttpClient httpClient)
            : this(httpClient)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
            _credentials = credentials;
            _baseUri = baseUri;

            Credentials.InitializeServiceClient(this);
        }

        /// <summary>
        /// Initializes a new instance of the GalleryClient class.
        /// </summary>
        /// <param name='credentials'>
        /// Required. Gets subscription credentials which uniquely identify
        /// Microsoft Azure subscription. The subscription ID forms part of
        /// the URI for every service call.
        /// </param>
        /// <param name='httpClient'>
        /// The Http client
        /// </param>
        public GalleryClient(SubscriptionCloudCredentials credentials, HttpClient httpClient)
            : this(httpClient)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            _credentials = credentials;
            _baseUri = new Uri("https://gallery.azure.com/");

            Credentials.InitializeServiceClient(this);
        }

        /// <summary>
        /// Clones properties from current instance to another GalleryClient
        /// instance
        /// </summary>
        /// <param name='client'>
        /// Instance of GalleryClient to clone to
        /// </param>
        protected override void Clone(ServiceClient<GalleryClient> client)
        {
            base.Clone(client);

            if (client is GalleryClient)
            {
                GalleryClient clonedClient = (GalleryClient)client;

                clonedClient._credentials = _credentials;
                clonedClient._baseUri = _baseUri;
                clonedClient._apiVersion = _apiVersion;
                clonedClient._longRunningOperationInitialTimeout = _longRunningOperationInitialTimeout;
                clonedClient._longRunningOperationRetryTimeout = _longRunningOperationRetryTimeout;

                clonedClient.Credentials.InitializeServiceClient(clonedClient);
            }
        }
    }
}