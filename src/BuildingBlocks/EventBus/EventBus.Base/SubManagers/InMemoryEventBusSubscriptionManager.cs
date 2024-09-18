using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.SubManagers
{
    public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers; //handlerları tuttuk. 
        private readonly List<Type> _eventTypes;

        public event EventHandler<string> OnEventRemoved;
        public Func<string, string> eventNameGetter;
        
        
        //func içinde string parametresi alacak geriye string dönecek. burada rMQ veya ASB de kuyruklar oluştururken kuyrukların isimlerindeki gereksiz kısımları trimlemek için.
        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGetter) 
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
            this.eventNameGetter = eventNameGetter;
        }

        public bool IsEmpty => !_handlers.Keys.Any(); //hanndlerin key i var mı?
        public void Clear() => _handlers.Clear(); //clear et


        public void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            AddSubscription(typeof(TH), eventName); //subscription işlemini gerçekleştir.

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }

        private void AddSubscription(Type handlerType, string eventName)
        {
            if (!HasSubscriptionsForEvent(eventName)) //dictionary de bu isimde bir key varmı yokmu? yani subscribe edilmişmi daha önceden?
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>()); //edilmemişse listeye ekle.
            }
            if (_handlers[eventName].Any(s => s.HandlerType == handlerType)) //eğer IIntegrationEventHandler dan gelen tip ten bu eventten daha önce var ise hata fırlat.
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType)); //SubscriptionInfo ya bunun tipini gönder.
        }

        public void RemoveSubscription<T, TH>() where TH : IIntegrationEventHandler<T> where T : IntegrationEvent //burada silme işlemi yaptık.
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            RemoveHandler(eventName, handlerToRemove);
        }

        private void RemoveHandler(string eventName, SubscriptionInfo subsToRemove) 
        {
            if (subsToRemove != null)
            {
                _handlers[eventName].Remove(subsToRemove);

                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }

                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent //eventhandlerların listesini geriye döndüğümüz bir method
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);

        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName]; //gönderilen key in bütün value lerini geriye dönüyor.

        private void RaiseOnEventRemoved(string eventName) //bir event silindiyse(unsubscribe olduysa) bu eventi kullananlara haber vereceğiz. 
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T> //silinecek eventi bul.
        {
            var eventName = GetEventKey<T>();
            return FindSubscriptionToRemove(eventName, typeof(TH));
        }

        private SubscriptionInfo FindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }

        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();

            return HasSubscriptionsForEvent(key);
        } 

        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName); //gelen eventName adında bir key varmı yok mu?

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name ==  eventName);

        public string GetEventKey<T>() //bize gönderilen event tipinin name sini aldık. (ex. OrderCreatedIntegratiionEvent)
        {
            string eventName = typeof(T).Name;
            return eventNameGetter(eventName); //bu name yi, ctordaki func a gönderdik. trim işlemi için.
        }
    }
}
