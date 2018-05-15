// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Management.Sql.LegacySdk.Models;
using System;

namespace Microsoft.Azure.Commands.Sql.Model
{
    public class IndexRecommendation : RecommendedIndexProperties
    {
        /// <summary>
        /// Copy constructor from base class
        /// </summary>
        /// <param name="other">Source object</param>
        public IndexRecommendation(RecommendedIndexProperties other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            Action = other.Action;
            Columns = other.Columns;
            Created = other.Created;
            EstimatedImpact = other.EstimatedImpact;
            IncludedColumns = other.IncludedColumns;
            IndexScript = other.IndexScript;
            IndexType = other.IndexType;
            LastModified = other.LastModified;
            ReportedImpact = other.ReportedImpact;
            Schema = other.Schema;
            State = other.State;
            Table = other.Table;
            Columns = other.Columns;
        }

        /// <summary>
        /// Azure SQL Database name on which this index should be created.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Name of recommended index.
        /// </summary>
        public string Name { get; set; }
    }
}
