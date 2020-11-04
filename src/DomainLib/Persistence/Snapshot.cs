﻿namespace DomainLib.Persistence
{
    public class Snapshot<T>
    {
        public Snapshot(T snapshotState, long version)
        {
            SnapshotState = snapshotState;
            Version = version;
        }

        public T SnapshotState { get; }
        public long Version { get; }
    }
}