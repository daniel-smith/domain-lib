using System;
using DomainLib.Aggregates;
using NUnit.Framework;

namespace DomainLib.Tests.Aggregates
{
    [TestFixture]
    public class EventNameMappingTests
    {
        public const string AttributeEventName = "AttributeEventName";
        public const string DerivedAttributeEventName = "DerviedAttributeEventName";
        public const string ConstantEventName= "ConstantEventName";

        [Test]
        public void EventNameIsSelectedFromAttribute()
        {
            VerifyEventNameForEvent(typeof(OnlyAttributeEvent), AttributeEventName);
        }

        [Test]
        public void EventNameIsSelectedFromAttributeWhenConstantIsPresent()
        {
            VerifyEventNameForEvent(typeof(AttributeAndConstantEvent), AttributeEventName);
        }

        [Test]
        public void EventNameIsSelectedFromConstantWhenNoAttribute()
        {
            VerifyEventNameForEvent(typeof(OnlyConstantEvent), ConstantEventName);
        }

        [Test]
        public void EventNameIsSelectedFromClassNameWhenNoAttributeOrConstant()
        {
            VerifyEventNameForEvent(typeof(OnlyClassNameEvent), nameof(OnlyClassNameEvent));
        }

        [Test]
        public void AttributeOverridesConstantInDerivedClass()
        {
            VerifyEventNameForEvent(typeof(DerivedEvent), DerivedAttributeEventName);
        }

        [Test]
        public void EventNameIsSelectedCorrectlyForDerivedClass2()
        {
            VerifyEventNameForEvent(typeof(DerivedEventWithConstantOnly), AttributeEventName);
        }

        [Test]
        public void RegisteringTwoEventsWithTheSameNameThrows()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var eventNameMapping = new EventNameMapping();
                eventNameMapping.RegisterEvent<OnlyAttributeEvent>();
                eventNameMapping.RegisterEvent<OnlyAttributeEvent2>();
            });
        }

        [Test]
        public void CanOverrideEventRegistrationIfUserChoosesNotToThrow()
        {
            var eventNameMapping = new EventNameMapping();
            eventNameMapping.RegisterEvent<OnlyAttributeEvent>();
            eventNameMapping.RegisterEvent<OnlyAttributeEvent2>(throwOnConflict: false);

            Assert.That(eventNameMapping.GetClrTypeForEventName(AttributeEventName),
                        Is.EqualTo(typeof(OnlyAttributeEvent2)));
        }

        [Test]
        public void CanMergeEventNameMappings()
        {
            var eventNameMapping1 = new EventNameMapping();
            var eventNameMapping2 = new EventNameMapping();

            eventNameMapping1.RegisterEvent<OnlyAttributeEvent>();
            eventNameMapping2.RegisterEvent<OnlyConstantEvent>();

            eventNameMapping1.Merge(eventNameMapping2);

            Assert.That(eventNameMapping1.GetEventNameForClrType(typeof(OnlyAttributeEvent)),
                        Is.EqualTo(AttributeEventName));
            
            Assert.That(eventNameMapping1.GetEventNameForClrType(typeof(OnlyConstantEvent)),
                        Is.EqualTo(ConstantEventName));
        }

        private static void VerifyEventNameForEvent(Type eventType, string expectedEventName)
        {
            var eventNameMapping = new EventNameMapping();
            eventNameMapping.RegisterEvent(eventType);

            Assert.That(eventNameMapping.GetEventNameForClrType(eventType),
                        Is.EqualTo(expectedEventName));
        }
    }

    [EventName(EventNameMappingTests.AttributeEventName)]
    public class OnlyAttributeEvent
    {
    }

    [EventName(EventNameMappingTests.AttributeEventName)]
    public class OnlyAttributeEvent2
    {
    }

    [EventName(EventNameMappingTests.AttributeEventName)]
    public class AttributeAndConstantEvent
    {
        public const string EventName = EventNameMappingTests.ConstantEventName;
    }

    public class OnlyConstantEvent
    {
        public const string EventName = EventNameMappingTests.ConstantEventName;
    }

    public class OnlyClassNameEvent
    {
    }

    [EventName(EventNameMappingTests.DerivedAttributeEventName)]
    public class DerivedEvent : OnlyAttributeEvent
    {
    }

    public class DerivedEventWithConstantOnly : OnlyAttributeEvent
    {
        public const string EventName = EventNameMappingTests.ConstantEventName;
    }
}