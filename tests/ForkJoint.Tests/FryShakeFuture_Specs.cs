namespace ForkJoint.Tests
{
    using System;
    using System.Threading.Tasks;
    using Api.Components.Consumers;
    using Api.Components.Futures;
    using Api.Services;
    using Contracts;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    [TestFixture]
    public class FryShakeFuture_Specs :
        FutureTestFixture
    {
        [Test]
        public async Task Should_complete()
        {
            var orderId = NewId.NextGuid();
            var orderLineId = NewId.NextGuid();

            var startedAt = DateTime.UtcNow;

            var scope = Provider.CreateScope();

            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<OrderFryShake>>();

            Response<FryShakeCompleted> response = await client.GetResponse<FryShakeCompleted>(new
            {
                OrderId = orderId,
                OrderLineId = orderLineId,
                Flavor = "Chocolate",
                Size = Size.Medium
            });

            Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
            Assert.That(response.Message.OrderLineId, Is.EqualTo(orderLineId));
            Assert.That(response.Message.Size, Is.EqualTo(Size.Medium));
            Assert.That(response.Message.Created, Is.GreaterThan(startedAt));
            Assert.That(response.Message.Completed, Is.GreaterThan(response.Message.Created));
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton<IShakeMachine, ShakeMachine>();
            collection.AddSingleton<IFryer, Fryer>();
        }

        protected override void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator)
        {
            configurator.AddConsumer<PourShakeConsumer>();
            configurator.AddConsumer<CookFryConsumer>();

            configurator.AddRequestClient<OrderFryShake>();
        }

        protected override void ConfigureInMemoryBus(IInMemoryBusFactoryConfigurator configurator)
        {
            configurator.FutureEndpoint<FryFuture, OrderFry>(Provider);
            configurator.FutureEndpoint<ShakeFuture, OrderShake>(Provider);
            configurator.FutureEndpoint<FryShakeFuture, OrderFryShake>(Provider);
        }
    }
}