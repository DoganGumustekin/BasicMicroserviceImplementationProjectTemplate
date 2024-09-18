using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    public interface IEventBusSubscriptionManager
    {
        bool IsEmpty { get; } //herhangi bir eventi dinliyor muyuz? herhangi bir subscription var mı?
        
        event EventHandler<string> OnEventRemoved; //bu event silindiği zaman, bir event oluşturacağız ve dışardan bize gelen unscribe mmetodu çalıştığı zaman u metodu da tetikleyeceğiz.

        void AddSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>; //subscriptionu eklicez.

        void RemoveSubscription<T, TH>() where TH : IIntegrationEventHandler<T> where T : IntegrationEvent; //subscriptionu silecez.

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent; //dışardan gelen event daha önce subscribe edilmiş mi edilmemiş mi?

        bool HasSubscriptionsForEvent(string eventName); //adına göre sub edilmiş mi edilmemeiş mi?

        Type GetEventTypeByName(string eventName);//bir eventname gönderildiğinde eventin tipini geri döneceğiz. 

        void Clear(); //listeyi silebileceğiz. bütün subscriptionları silebileceğiz.

        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;//dışarıdan gönderilen eventin bütün sub larını döneceğiz

        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);//evenName ye göre sub larını döneceğiz.

        string GetEventKey<T>(); //eventlerin isimleri olacak bu isimler uniq olacak. içerde de bu eventlerin isimlerine göre implementasyonlar yapılıyor. buda integrationevent için kullanılan bir key.
    }
}
