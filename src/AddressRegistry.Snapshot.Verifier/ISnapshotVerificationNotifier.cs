namespace AddressRegistry.Snapshot.Verifier
{
    public interface ISnapshotVerificationNotifier
    {
        void NotifyInvalidSnapshot(int snapshotId, string differences);
    }
}
