using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.UnitTest.Events.EventHandlers;
using EventBus.UnitTest.Events.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.UnitTest
{
    [TestClass]
    public class EventBusUnitTest
    {
        //webapi projesinde serviceprovider programcs de paramatre olarak geldi�i i�in onu create etmiyoruz. ama burada webapi olmad��� i�in create etmemiz laz�m
        //��nk� �nce factory e rmq ve asb classlar� i�in gidiyoruz. oda bizden service prov,der istiyor. 
        private ServiceCollection services;

        public EventBusUnitTest()
        {
            services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
        }

        [TestMethod]
        public void subscribe_event_on_rabbitmq_test()
        {
            //bu integration i�lemlerini yapabilmek i�in IEventBs gerekli. ��nk� sub unsub ve publish edebilmek i�in inject ediyorum.
            services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });//her IEventBus istendi�inde sen i�erdekileri yap.


            var sp = services.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>(); //eventbusu al yani sat�r 29 - 45 �al��acak.

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>(); //subscription
            //eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>(); //Unsubscription

            //Task.Delay(5000).Wait();

        }

        //[TestMethod]
        //public void subscribe_event_on_azure_test()
        //{
        //    //bu integration i�lemlerini yapabilmek i�in IEventBs gerekli. ��nk� sub unsub ve publish edebilmek i�in inject ediyorum.
        //    services.AddSingleton<IEventBus>(sp =>
        //    {
        //        return EventBusFactory.Create(GetAzureConfig(), sp);
        //    });//her IEventBus istendi�inde sen i�erdekileri yap.


        //    var sp = services.BuildServiceProvider();

        //    var eventBus = sp.GetRequiredService<IEventBus>(); //eventbusu al yani sat�r 29 - 45 �al��acak.

        //    eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>(); //subscription
        //    eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>(); //Unsubscription

        //}


        [TestMethod]
        public void send_message_to_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });//her IEventBus istendi�inde sen i�erdekileri yap.


            var sp = services.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();

            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        }

        //[TestMethod]
        //public void send_message_to_azure_test()
        //{
        //    services.AddSingleton<IEventBus>(sp =>
        //    {
        //        return EventBusFactory.Create(GetAzureConfig(), sp);
        //    });//her IEventBus istendi�inde sen i�erdekileri yap.


        //    var sp = services.BuildServiceProvider();

        //    var eventBus = sp.GetRequiredService<IEventBus>();

        //    eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        //}

        //private EventBusConfig GetAzureConfig()
        //{
        //    return new EventBusConfig()
        //    {
        //        ConnectionRetryCount = 5,
        //        SubscriberClientAppName = "EventBus.UnitTest", //rmq ya kim ba�lanacak kim msj g�nderecek kim alacak.
        //        DefaultTopicName = "SellingBuddyTopicName", //exchange
        //        EventBusType = EventBusType.AzureServiceBus,
        //        EventNameSuffix = "IntegrationEvent", //bu yaz�lar asb ve rmq da g�r�nmesin trim edilsin.
        //        EventBusConnectionString = "Endpoint=sb://techbuddy.servicebus.windows.net/;SharedAccessKeyName=NewPloicyForYTVideos;SharedAccessKey=7sjghGWFOXaUaRblrbzOIIf4bQk6qkbTN/SEnKjXLpE="
        //    };
        //}

        private EventBusConfig GetRabbitMQConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest", //rmq ya kim ba�lanacak kim msj g�nderecek kim alacak.
                DefaultTopicName = "SellingBuddyTopicName", //exchange
                EventBusType = EventBusType.RabbitMQ,
                EventNameSuffix = "IntegrationEvent", //bu yaz�lar asb ve rmq da g�r�nmesin trim edilsin.
                //Connection = new ConnectionFactory()
                //{
                //    HostName = "localhost",
                //    Port = 5672,
                //    UserName = "guest",
                //    Password = "guest"
                //} //default ayarlar old. i�in vermesek de olur.
            };
        }
    }
}