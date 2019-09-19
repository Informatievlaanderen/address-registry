namespace AddressRegistry.Projections.LastChangedList
{
    public class LastChangedListContextMigrationFactory
        : Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.LastChangedListContextMigrationFactory
    {
        public LastChangedListContextMigrationFactory()
            : base("LastChangedListAdmin") { }
    }
}
