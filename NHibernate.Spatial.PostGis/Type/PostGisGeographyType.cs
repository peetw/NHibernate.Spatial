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

using System;
using System.Data.Common;
using NetTopologySuite.Geometries;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;
using Npgsql;
using NpgsqlTypes;

namespace NHibernate.Spatial.Type
{
    [Serializable]
    public class PostGisGeographyType : GeometryTypeBase<Geometry>
    {
        private static readonly NullableType GeometryType = new CustomGeometryType();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostGisGeographyType"/> class.
        /// </summary>
        public PostGisGeographyType()
            : base(GeometryType)
        { }

        /// <summary>
        /// Converts from GeoAPI geometry type to database geography type.
        /// </summary>
        /// <param name="value">The GeoAPI geometry value.</param>
        /// <returns></returns>
        protected override Geometry FromGeometry(object value)
        {
            var geometry = value as Geometry;
            if (geometry == null)
            {
                return null;
            }

            SetDefaultSRID(geometry);

            return geometry;
        }

        /// <summary>
        /// Converts to GeoAPI geometry type from database geography type.
        /// </summary>
        /// <param name="value">The database geography value.</param>
        /// <returns></returns>
        protected override Geometry ToGeometry(object value)
        {
            var geometry = value as Geometry;
            if (geometry == null)
            {
                return null;
            }

            SetDefaultSRID(geometry);

            return geometry;
        }

        [Serializable]
        private class CustomGeometryType : MutableType
        {
            internal CustomGeometryType() : base(new BinarySqlType())
            { }

            public override System.Type ReturnedClass => typeof(Geometry);

            public override string Name => "Geography";

            public override object Get(DbDataReader rs, int index, ISessionImplementor session)
            {
                return (Geometry) rs.GetValue(index);
            }

            public override object Get(DbDataReader rs, string name, ISessionImplementor session)
            {
                return Get(rs, rs.GetOrdinal(name), session);
            }

            public override void Set(DbCommand cmd, object value, int index, ISessionImplementor session)
            {
                var parameter = (NpgsqlParameter) cmd.Parameters[index];
                parameter.NpgsqlDbType = NpgsqlDbType.Geography;
                parameter.Value = value;
            }

            public override object DeepCopyNotNull(object value)
            {
                var obj = (Geometry) value;
                return obj.Copy();
            }
        }
    }
}
