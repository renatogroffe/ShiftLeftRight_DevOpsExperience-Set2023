using Confluent.Kafka;

namespace APIContagem.Kafka;

public static class KafkaExtensions
{
    private static int _QtdPartitions = 1;

    public static int QtdPartitions
    {
        get => _QtdPartitions;
    }

    public static void CheckNumPartitions(
        IConfiguration configuration)
    {
        AdminClientConfig kafkaConfig;
        var password = configuration["ApacheKafka:Password"];
        if (!String.IsNullOrWhiteSpace(password))
            kafkaConfig = new ()
            {
                BootstrapServers = configuration["ApacheKafka:Host"],
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = configuration["ApacheKafka:Username"],
                SaslPassword = password
            };
        else
            kafkaConfig = new ()
            {
                BootstrapServers = configuration["ApacheKafka:Host"],
            };

        using var adminClient = new AdminClientBuilder(kafkaConfig).Build();

        var infoTopic = adminClient.GetMetadata(configuration["ApacheKafka:Topic"],
            TimeSpan.FromSeconds(25)).Topics.FirstOrDefault();
        if (infoTopic is not null)
            _QtdPartitions = infoTopic.Partitions.Count;
    }

    public static IProducer<Null, string> CreateProducer(
        IConfiguration configuration)
    {
        var password = configuration["ApacheKafka:Password"];
        if (!String.IsNullOrWhiteSpace(password))
            return new ProducerBuilder<Null, string>(
                new ProducerConfig()
                {
                    BootstrapServers = configuration["ApacheKafka:Host"],
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = configuration["ApacheKafka:Username"],
                    SaslPassword = password
                }).Build();
        else
            return new ProducerBuilder<Null, string>(
                new ProducerConfig()
                {
                    BootstrapServers = configuration["ApacheKafka:Host"]
                }).Build();
    }
}