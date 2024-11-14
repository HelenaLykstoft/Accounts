
using Accounts.Infrastructure.Kafka;

namespace Accounts.API.Services
{
    public class TestService  
    {
        private readonly KafkaProducer _kafkaProducer;
        public TestService(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        public async void DoStuff()
        {
            // Brug KafkaProducer
            await _kafkaProducer.ProduceAsync("topic", "Virker", "From DOSTUFF");
        }
    }
}
