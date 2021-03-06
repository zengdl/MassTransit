﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.MongoDbIntegration.Tests.Courier
{
    using System;
    using System.Threading.Tasks;
    using MassTransit.Courier.Contracts;
    using MongoDbIntegration.Courier;
    using MongoDbIntegration.Courier.Consumers;
    using MongoDbIntegration.Courier.Documents;
    using MongoDbIntegration.Courier.Events;
    using MongoDB.Driver;
    using NUnit.Framework;
    using Util;


    [TestFixture]
    public class When_a_routing_slip_is_completed :
        MongoDbTestFixture
    {
        [Test]
        public async Task Should_process_the_event()
        {
            ConsumeContext<RoutingSlipCompleted> context = await _completed;

            Assert.AreEqual(_trackingNumber, context.Message.TrackingNumber);

            Assert.IsTrue(context.CorrelationId.HasValue);
            Assert.AreEqual(_trackingNumber, context.CorrelationId.Value);
        }

        [Test]
        public async Task Should_upsert_the_event_into_the_routing_slip()
        {
            ConsumeContext<RoutingSlipCompleted> context = await _completed;

            await Task.Delay(1000);

            FilterDefinition<RoutingSlipDocument> query = Builders<RoutingSlipDocument>.Filter.Eq(x => x.TrackingNumber, _trackingNumber);

            var routingSlip = await (await _collection.FindAsync(query).ConfigureAwait(false)).SingleOrDefaultAsync().ConfigureAwait(false);

            Assert.IsNotNull(routingSlip);
            Assert.IsNotNull(routingSlip.Events);
            Assert.AreEqual(1, routingSlip.Events.Length);

            var completed = routingSlip.Events[0] as RoutingSlipCompletedDocument;
            Assert.IsNotNull(completed);
            Assert.IsTrue(completed.Variables.ContainsKey("Client"));
            Assert.AreEqual(27, completed.Variables["Client"]);
            //Assert.AreEqual(received.Timestamp.ToMongoDbDateTime(), read.Timestamp);
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            _trackingNumber = NewId.NextGuid();

            Console.WriteLine("Tracking Number: {0}", _trackingNumber);

            await Bus.Publish<RoutingSlipCompleted>(new RoutingSlipCompletedEvent(_trackingNumber, DateTime.UtcNow, TimeSpan.FromSeconds(1)));
        }

        protected override void ConfigureBus(IInMemoryBusFactoryConfigurator configurator)
        {
            base.ConfigureBus(configurator);

            configurator.ConfigureRoutingSlipEventCorrelation();
        }

        protected override void ConfigureInputQueueEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            base.ConfigureInputQueueEndpoint(configurator);

            _collection = Database.GetCollection<RoutingSlipDocument>(EventCollectionName);

            var persister = new RoutingSlipEventPersister(_collection);

            configurator.Consumer(() => new RoutingSlipCompletedConsumer(persister));

            _completed = Handled<RoutingSlipCompleted>(configurator, x => x.Message.TrackingNumber == _trackingNumber);
        }

        IMongoCollection<RoutingSlipDocument> _collection;
        Guid _trackingNumber;
        Task<ConsumeContext<RoutingSlipCompleted>> _completed;

        protected override void SetupActivities(IInMemoryBusFactoryConfigurator configurator)
        {
        }
    }
}