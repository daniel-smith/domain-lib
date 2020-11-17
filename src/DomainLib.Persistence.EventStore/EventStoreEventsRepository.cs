﻿using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainLib.Serialization;

namespace DomainLib.Persistence.EventStore
{
    public class EventStoreEventsRepository : IEventsRepository
    {
        
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _serializer;

        public EventStoreEventsRepository(IEventStoreConnection connection, IEventSerializer serializer)
        {
            _connection = connection;
            _serializer = serializer;
        }

        public async Task<long> SaveEventsAsync<TEvent>(string streamName, long expectedStreamVersion, IEnumerable<TEvent> events)
        {
            var eventDatas = events.Select(e => _serializer.ToEventData(e));
            var streamVersion = MapStreamVersionToEventStoreStreamVersion(expectedStreamVersion);
            
            // TODO: Handle failure cases
            var writeResult = await _connection.AppendToStreamAsync(streamName, streamVersion, eventDatas);

            return writeResult.NextExpectedVersion;
        }

        public async Task<TEvent[]> LoadEventsAsync<TEvent>(string streamName, long startPosition = 0)
        {
            // TODO: Handle large streams in  batches
            var eventsSlice = await _connection.ReadStreamEventsForwardAsync(streamName, startPosition, ClientApiConstants.MaxReadSize, false);

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

            var events = eventsSlice.Events;

            return events.Select(e => _serializer.DeserializeEvent<TEvent>(e.OriginalEvent.Data, e.OriginalEvent.EventType))
                         .ToArray();
        }

        private static long MapStreamVersionToEventStoreStreamVersion(long streamVersion)
        {
            switch (streamVersion)
            {
                case StreamVersion.Any:
                    return ExpectedVersion.Any;
                default:
                    return streamVersion;
            }
        }
    }
}