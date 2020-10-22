using System;
using System.Runtime.Serialization;

namespace DomainLib.Serialization
{
    [Serializable]
    public class InvalidEventTypeException : Exception
    {
        private readonly string _serializedEventType;
        private readonly Type _runtTimeType;

        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidEventTypeException(string serializedEventType, Type runtTimeType)
        {
            _serializedEventType = serializedEventType;
            _runtTimeType = runtTimeType;
        }

        public InvalidEventTypeException(string message, string serializedEventType, Type runtTimeType) : base(message)
        {
            _serializedEventType = serializedEventType;
            _runtTimeType = runtTimeType;
        }

        public InvalidEventTypeException(string message, string serializedEventType, Type runtTimeType, Exception inner) : base(message, inner)
        {
            _serializedEventType = serializedEventType;
            _runtTimeType = runtTimeType;
        }

        protected InvalidEventTypeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}