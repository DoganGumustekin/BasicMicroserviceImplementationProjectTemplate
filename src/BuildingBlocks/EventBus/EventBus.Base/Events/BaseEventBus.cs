using EventBus.Base.Abstraction;
using EventBus.Base.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus, IDisposable
    {
        public readonly IServiceProvider _serviceProvider;
        public readonly IEventBusSubscriptionManager SubsManager;

        public EventBusConfig EventBusConfig;

        public BaseEventBus(EventBusConfig config, IServiceProvider serviceProvider)
        {
            EventBusConfig = config;
            _serviceProvider = serviceProvider;
            SubsManager = new InMemoryEventBusSubscriptionManager(ProcessEventName); //default olarak inmemory i kullandık.
        }


        public virtual string ProcessEventName(string eventName) //ex: notificationservice.ordercreatedintegrationevent içinde integrationevent i kırpabilmek için.
        {
            if (EventBusConfig.DeleteEventPrefix) //eventBusConfig te DeleteEventPrefix seçilmiş ise 
                eventName = eventName.TrimStart(EventBusConfig.EventNamePrefix.ToArray()); //başındaki 
            if (EventBusConfig.DeleteEventSuffix) //eventBusConfig te DeleteEventSuffix seçilmiş ise  ordercreatedintegrationevent => ordercreated
                eventName = eventName.TrimEnd(EventBusConfig.EventNameSuffix.ToArray());
                
            return eventName;
        }

        //Subscriptionun ismi getirilmek istendiğinde SubscriberClientAppName ve sonuna ProcessEventName den gelen nameyi(ordercreated) ekle. ex: notificationservice.ordercreatedintegrationevent 
        public virtual string GetSubName(string eventName)
        {
            return $"{EventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }

        public virtual void Dispose()
        {
            EventBusConfig = null;
            SubsManager.Clear();
        }

        //message = rmq veya qsb den bize ulaştırılmmış bir mesaj. 
        //diyelim ki  ordercreatedintegrationevent i dinliyorum. eğer rmq haber verirse (buradan bir event fırlaıldı diye) bu metota düşecek.
        public async Task<bool> ProcessEvent(string eventName, string message) // eventin process edilmesi.
        {
            eventName = ProcessEventName(eventName); //kırpma işlemi gerekiyorsa kırp.

            var processed = false;

            if (SubsManager.HasSubscriptionsForEvent(eventName)) // bu event consume edilmiş mi? eğerki dinliyorsam bu işlemi yapabilirim.
            {
                var subscriptions = SubsManager.GetHandlersForEvent(eventName); //bunun bana bütün subscriptionlarını ver.

                using (var scope = _serviceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = _serviceProvider.GetService(subscription.HandlerType); //inject edilen servisi(dependency injection) .met core tarafıından alacağız.
                        if (handler == null) continue; //yoksa geç.

                        var eventType = SubsManager.GetEventTypeByName($"{EventBusConfig.EventNamePrefix}{eventName}{EventBusConfig.EventNameSuffix}"); //kırpmıştık eski aline alıyoruz.
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);


                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        // buradaki implementasyonlar base kısmında olduğu için, dışardan method ismini alamam (örneğin ordercreatedintegrationevent) bu yüzden reflection yöntemi ile aıyorum.
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }

                processed = true;
            }
            return processed;
        }


        //alttaki üç metot kullanacağımız message brokerlara özel olacak. rmq ve asb için.
        public abstract void Publish(IntegrationEvent @event);
        public abstract void Subscribe<T, TH>() where T : IntegrationEvent where TH: IIntegrationEventHandler<T>;
        public abstract void UnSubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;

    }
}
