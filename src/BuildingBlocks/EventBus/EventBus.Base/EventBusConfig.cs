using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base
{
    public class EventBusConfig
    {
        //rMQ ya bağlanırken en fazla 5 defa dene. 5 ten sonra hata fırlat
        //bazen network hataları oluyor. bu network hataları olduğğunda sistemi durdurmak yerine
        //bir retry mekanizması olacak. bağlanamadıysa 5 defa ya kadar bağlanmayı deneyecek.
        public int ConnectionRetryCount { get; set; } = 5;

        //bu topic name altında birden fazla queue larımızı oluşturacağız. eğer dılşardan herhangi bir topicname verilmezse, sistem hata verememesi için default bir isim verdim.
        public string DefaultTopicName { get; set; } = "SellingBuddyEventBus";

        public string EventBusConnectionString { get; set; } = String.Empty;

        public string SubscriberClientAppName { get; set; } = String.Empty; //hangi servis yeni bir queue yaratacak? kuyruk isimlendirmede de kullanılacak.

        public string EventNamePrefix { get; set; } = String.Empty; //trim işlemleri için ön ek

        public string EventNameSuffix { get; set; } = "IntegrationEvent"; //trim işlemleri için son ek

        public EventBusType EventBusType { get; set; } = EventBusType.RabbitMQ; //default olarak bağlanacağımız service bus rabbitmq olacak.

        //connection işlemleri için. rMQ veya ASB hangisi kullanılacaksa onun connectionuna cast edilecek.
        public object Connection { get; set; }



        public bool DeleteEventPrefix => !String.IsNullOrEmpty(EventNamePrefix);

        public bool DeleteEventSuffix => !String.IsNullOrEmpty(EventNameSuffix);
    }

    public enum EventBusType
    {
        RabbitMQ = 0,
        AzureServiceBus = 1
    }
}
