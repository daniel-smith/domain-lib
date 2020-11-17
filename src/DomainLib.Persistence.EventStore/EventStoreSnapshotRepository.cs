﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainLib.Serialization;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

namespace DomainLib.Persistence.EventStore
{
    public class EventStoreSnapshotRepository : ISnapshotRepository
    {
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _serializer;
        private const string SnapshotVersionMetadataKey = "SnapshotVersion";

        public EventStoreSnapshotRepository(IEventStoreConnection connection, IEventSerializer serializer)
        {
            _connection = connection;
            _serializer = serializer;
        }

        public async Task SaveSnapshot<TState>(string snapshotStreamName, long snapshotVersion, TState snapshotState)
        {
            var snapshotData = _serializer.ToEventData(snapshotState, KeyValuePair.Create(SnapshotVersionMetadataKey, snapshotVersion.ToString()));
            await _connection.AppendToStreamAsync(snapshotStreamName, ExpectedVersion.Any, snapshotData);
        }

        public async Task<Snapshot<TState>> LoadSnapshot<TState>(string streamName)
        {
            var eventsSlice = await _connection.ReadStreamEventsBackwardAsync(streamName, 0, 1, false);

            switch (eventsSlice.Status)
            {
                case SliceReadStatus.Success:
                    break;
                case SliceReadStatus.StreamNotFound:
                    // TOOO: Better exception
                    throw new InvalidOperationException($"Unable to find stream {streamName}");
                case SliceReadStatus.StreamDeleted:
                    throw new StreamDeletedException(streamName);
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var @event = eventsSlice.Events[0];

            var snapshotData = _serializer.DeserializeEvent<TState>(@event.OriginalEvent.Data, @event.OriginalEvent.EventType);
            var snapshotVersion = long.Parse(_serializer.DeserializeMetadata(@event.OriginalEvent.Metadata)[SnapshotVersionMetadataKey]);

            return new Snapshot<TState>(snapshotData, snapshotVersion);
        }
    }
}