using EventBus.AzureServiceBus;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Factory
{
    //dışardan rmq veya asb ye bağlanmak isteyenler direk değilde bu classa parametre gönderip ona göre bağlansınlar.
    public static class EventBusFactory
    {
        public static IEventBus Create(EventBusConfig config, IServiceProvider serviceProvider)
        {
            //switch case nin farklı bir kullanımı alttaki.
            return config.EventBusType switch
            {
                EventBusType.AzureServiceBus => new EventBusServiceBus(config, serviceProvider), //asb ise git bu parametreler ile create et
                _ => new EventBusRabbitMQ(config, serviceProvider), //değil ise rmq ile create et.
            };

        }
    }
}
