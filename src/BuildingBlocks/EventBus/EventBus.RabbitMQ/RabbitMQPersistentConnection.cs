using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    //rabbitMQ connect ve diğer işlemler için oluşturduğumuz class.
    public class RabbitMQPersistentConnection : IDisposable
    {
        private IConnection connection; //hangi connection
        private IConnectionFactory _connectionFactory; //rabbitMQ bağlantısı için
        private object lock_object = new object();//aynı proje içinde birden fazla kere TryConnect çağrılabilir. bu yüzden bir kilitleme mekanizması kuruyoruz.
        private readonly int _retryCount;
        private bool _disposed; //dispose edilmiş mi?

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            this._connectionFactory = connectionFactory;
            this._retryCount = retryCount;
        }


        public bool IsConnection => connection != null && connection.IsOpen; //o an connection aktif mi değil mi? şartları sağlıyorsa true dönecek.
        
        public IModel CreateModel()
        {
            return connection.CreateModel();
        }

        public void Dispose()
        {
            _disposed = true;
            connection.Dispose();
        }

        public bool TryConnect()
        {
            lock (lock_object) //aynı metod çağrıldığında bir önceki işlemin bitmesini beklicek.
            {
                var policy = Policy.Handle<SocketException>() //poly retry mekanizması geliştirecek.
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) => //5 kere dene
                    {
                    }
                );

                policy.Execute(() =>
                {
                    connection = _connectionFactory.CreateConnection(); //connectionu oluştur.
                });

                if (IsConnection)
                {
                    //oluşturulan bağlantıların sürekli bağlı kalmasını sağlamak.
                    connection.ConnectionShutdown += Connection_ConnectionShutdown;
                    connection.CallbackException += Connection_CallbackException;
                    connection.ConnectionBlocked += Connection_ConnectionBlocked;


                    //log

                    return true; //bağlandı se true
                }

                return false;
            }
        }

        private void Connection_CallbackException(object? sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            //Log Connection_CallbackException

            if (_disposed) return; //dispose edildiğinden dolayı kanmışsa bağlama.
            TryConnect();
        }

        private void Connection_ConnectionBlocked(object? sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            //Log Connection_ConnectionBlocked

            if (_disposed) return; //dispose edildiğinden dolayı kanmışsa bağlama.
            TryConnect();
        }

        private void Connection_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            //Log Connection_ConnectionShutdown
            
            if (_disposed) return; //dispose edildiğinden dolayı kanmışsa bağlama.
            TryConnect();
        }
    }
}
