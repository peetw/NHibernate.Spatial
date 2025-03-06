using System;
using NetTopologySuite.Geometries;

namespace Tests.NHibernate.Spatial.Models
{
    [Serializable]
    public class Geography
    {
        public int Id { get; set; }

        public Geometry Geog { get; set; }
    }
}
