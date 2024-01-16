// Copyright 2007 - Ricardo Stuven (rstuven@gmail.com)
//
// This file is part of NHibernate.Spatial.
// NHibernate.Spatial is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// NHibernate.Spatial is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NHibernate.Spatial; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using NetTopologySuite.Geometries;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.Spatial.Dialect;
using NHibernate.SqlCommand;
using System;
using System.Linq;
using System.Text;

namespace NHibernate.Spatial.Criterion
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class SpatialRelationCriterion : AbstractCriterion
    {
        private readonly string propertyName;
        private readonly SpatialRelation relation;
        private readonly object anotherGeometry;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpatialRelationCriterion"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="relation">The relation.</param>
        /// <param name="anotherGeometry">Another geometry.</param>
        public SpatialRelationCriterion(string propertyName, SpatialRelation relation, object anotherGeometry)
        {
            this.propertyName = propertyName;
            this.relation = relation;
            this.anotherGeometry = anotherGeometry;
        }

        /// <summary>
        /// Return typed values for all parameters in the rendered SQL fragment
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="criteriaQuery"></param>
        /// <returns>
        /// An array of TypedValues for the Expression.
        /// </returns>
        public override TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
        {
            if (anotherGeometry is Geometry)
            {
                return new[] { criteriaQuery.GetTypedValue(criteria, propertyName, anotherGeometry) };
            }
            return Array.Empty<TypedValue>();
        }

        public override IProjection[] GetProjections()
        {
            return null;
        }

        /// <summary>
        /// Render a SqlString for the expression.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="criteriaQuery"></param>
        /// <returns>
        /// A SqlString that contains a valid Sql fragment.
        /// </returns>
        public override SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery)
        {
            //criteriaQuery.AddUsedTypedValues(GetTypedValues(criteria, criteriaQuery));
            var spatialDialect = (ISpatialDialect) criteriaQuery.Factory.Dialect;
            string[] columns1 = GetColumnNames(criteria, criteriaQuery, propertyName);

            var builder = new SqlStringBuilder(10*columns1.Length);
            for (int i = 0; i < columns1.Length; i++)
            {
                if (i > 0)
                {
                    builder.Add(" AND ");
                }
                if (anotherGeometry is Geometry)
                {
                    var parameters = criteriaQuery.NewQueryParameter(GetTypedValues(criteria, criteriaQuery)[0]).ToArray();
                    builder.Add(spatialDialect.GetSpatialRelationString(columns1[i], relation, parameters.Single(), true));
                }
                else
                {
                    string[] columns2 = GetColumnNames(criteria, criteriaQuery, (string) anotherGeometry);
                    builder.Add(spatialDialect.GetSpatialRelationString(columns1[i], relation, columns2[i], true));
                }
            }
            return builder.ToSqlString();
        }

        /// <summary>
        /// Gets a string representation of the <see cref="T:NHibernate.Criterion.AbstractCriterion"/>.
        /// </summary>
        /// <returns>
        /// A String that shows the contents of the <see cref="T:NHibernate.Criterion.AbstractCriterion"/>.
        /// </returns>
        /// <remarks>
        /// This is not a well formed Sql fragment.  It is useful for logging what the <see cref="T:NHibernate.Criterion.AbstractCriterion"/>
        /// looks like.
        /// </remarks>
        public override string ToString()
        {
            return new StringBuilder()
                .Append(relation)
                .Append("(")
                .Append(propertyName)
                .Append(", ")
                .Append(anotherGeometry is Geometry ? "<Geometry>" : anotherGeometry.ToString())
                .Append(")")
                .ToString();
        }

        /// <summary>
        /// Gets the column names.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="criteriaQuery">The criteria query.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private string[] GetColumnNames(ICriteria criteria, ICriteriaQuery criteriaQuery, string propertyName)
        {
            string[] columns = criteriaQuery.GetColumnsUsingProjection(criteria, propertyName);
            var type = criteriaQuery.GetTypeUsingProjection(criteria, propertyName);
            if (!typeof(Geometry).IsAssignableFrom(type.ReturnedClass))
            {
                throw new QueryException(string.Format("Type mismatch in {0}: {1} expected type {2}, actual type {3}", GetType(), propertyName, typeof(Geometry), type.ReturnedClass));
            }
            if (type.IsCollectionType)
            {
                throw new QueryException(string.Format("cannot use collection property ({0}.{1}) directly in a criterion, use ICriteria.CreateCriteria instead", criteriaQuery.GetEntityName(criteria), propertyName));
            }
            return columns;
        }
    }
}
