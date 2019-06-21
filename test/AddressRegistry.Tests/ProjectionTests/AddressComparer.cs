namespace AddressRegistry.Tests.ProjectionTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using GeoAPI.Geometries;
    using KellermanSoftware.CompareNetObjects;
    using KellermanSoftware.CompareNetObjects.TypeComparers;
    using NetTopologySuite.Geometries;

    public class IPointComparer : BaseTypeComparer
    {
        public IPointComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return type1 == typeof(IPoint) || type1 == typeof(Point);
        }

        public override void CompareType(CompareParms parms)
        {
            var point1 = (IPoint)parms.Object1;
            var point2 = (IPoint)parms.Object2;

            if (!point1.EqualsExact(point2))
                AddDifference(parms);
        }
    }

    public class AddressComparer<TEntity> : IEntityComparer<TEntity>
    {
        private readonly CompareLogic _logic;

        public AddressComparer()
        {
            _logic = new CompareLogic();
            _logic.Config.CustomComparers.Add(new IPointComparer(RootComparerFactory.GetRootComparer()));
        }

        public IEnumerable<EntityComparisonDifference<TEntity>> Compare(TEntity expected, TEntity actual)
            => _logic
                .Compare(expected, actual)
                .Differences
                .Select(diff => new EntityComparisonDifference<TEntity>(expected, actual, diff.ToString()));
    }
}
