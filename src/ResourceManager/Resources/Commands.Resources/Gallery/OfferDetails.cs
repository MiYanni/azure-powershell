// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Hyak.Common;

namespace Microsoft.Azure.Commands.Resources.Models.Gallery
{
    /// <summary>
    /// A gallery item offer details.
    /// </summary>
    public partial class OfferDetails
    {
        private string _offerIdentifier;

        /// <summary>
        /// Optional. Gets or sets offer identifier.
        /// </summary>
        public string OfferIdentifier
        {
            get { return _offerIdentifier; }
            set { _offerIdentifier = value; }
        }

        private IList<Plan> _plans;

        /// <summary>
        /// Optional. Gets or sets plans.
        /// </summary>
        public IList<Plan> Plans
        {
            get { return _plans; }
            set { _plans = value; }
        }

        private string _publisherIdentifier;

        /// <summary>
        /// Optional. Gets or sets publisher identifier.
        /// </summary>
        public string PublisherIdentifier
        {
            get { return _publisherIdentifier; }
            set { _publisherIdentifier = value; }
        }

        /// <summary>
        /// Initializes a new instance of the OfferDetails class.
        /// </summary>
        public OfferDetails()
        {
            Plans = new LazyList<Plan>();
        }
    }
}