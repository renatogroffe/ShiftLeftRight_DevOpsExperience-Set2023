using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace WorkerContagem.Kafka;

public static class KafkaExtensions
{
    public static IConsumer<Ignore, string> CreateConsumer(
        IConfiguration configuration)
    {
        var password = configuration["ApacheKafka:Password"];
        if (!String.IsNullOrWhiteSpace(password))
            return new ConsumerBuilder<Ignore, string>(
                new ConsumerConfig()
                {
                    BootstrapServers = configuration["ApacheKafka:Host"],
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = configuration["ApacheKafka:Username"],
                    SaslPassword = password,
                    GroupId = configuration["ApacheKafka:GroupId"],
                    AutoOffsetReset = AutoOffsetReset.Earliest
                }).Build();
        else
            return new ConsumerBuilder<Ignore, string>(
                new ConsumerConfig()
                {
                    BootstrapServers = configuration["ApacheKafka:Host"],
                    GroupId = configuration["ApacheKafka:GroupId"],
                    AutoOffsetReset = AutoOffsetReset.Earliest
                }).Build();
    }

    public static bool CreateTopicForTestsIfNotExists(IConfiguration configuration)
    {
        if (String.IsNullOrWhiteSpace(configuration["ApacheKafka:Password"]))
        {
            using var adminClient = new AdminClientBuilder(
                new AdminClientConfig
                {
                    BootstrapServers = configuration["ApacheKafka:Host"]
                }).Build();
            var topicName = configuration["ApacheKafka:Topic"];
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            if (!metadata.Topics.Exists(t => t.Topic == topicName))
            {
                adminClient.CreateTopicsAsync(new TopicSpecification[] {
                                new TopicSpecification
                                {
                                    Name = topicName,
                                    NumPartitions = 10 // Assumida 10 como número de partições para testes
                                }}).Wait();
                return true;
            }
        }

        return false;
    }
}