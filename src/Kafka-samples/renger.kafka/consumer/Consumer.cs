using Confluent.Kafka;
using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Linq;

IConfiguration configuration = new ConfigurationBuilder() 
.AddJsonFile("./appsettings.json")
.Build();

var cConfig = configuration.GetSection("Consumer").Get<ConsumerConfig>();
var topicName = configuration.GetValue<string>("General:TopicName");

CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => {
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

using (var consumer = new ConsumerBuilder<Null, string>(cConfig)
.SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
.SetStatisticsHandler((_, json) => Console.WriteLine($"Statistics: {json}"))
.SetPartitionsAssignedHandler((c, partitions) =>
{
    Console.WriteLine(
                "Partitions incrementally assigned: [" +
                string.Join(',', partitions.Select(p => p.Partition.Value)) +
                "], all: [" +
                string.Join(',', c.Assignment.Concat(partitions).Select(p => p.Partition.Value)) +
                "]");
})
.SetPartitionsRevokedHandler((c, partitions) =>
{
        var remaining = c.Assignment.Where(atp => partitions.Where(rtp => rtp.TopicPartition == atp).Count() == 0);
                Console.WriteLine(
                    "Partitions incrementally revoked: [" +
                    string.Join(',', partitions.Select(p => p.Partition.Value)) +
                    "], remaining: [" +
                    string.Join(',', remaining.Select(p => p.Partition.Value)) +
                    "]");
})
.SetPartitionsLostHandler((c, partitions) =>
{
    // The lost partitions handler is called when the consumer detects that it has lost ownership
    // of its assignment (fallen out of the group).
    Console.WriteLine($"Partitions were lost: [{string.Join(", ", partitions)}]");
})
.Build())
{
    consumer.Subscribe(topicName);

    try
    {
        while (true)
        {
            try
            {
                var consumeResult = consumer.Consume(cts.Token);

                if (consumeResult.IsPartitionEOF)
                {
                    Console.WriteLine(
                        $"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");

                    continue;
                }

           //     Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}: {consumeResult.Message.Value}");
                try
                {
                    consumer.StoreOffset(consumeResult);
                }
                catch (KafkaException e)
                {
                    Console.WriteLine($"Store Offset error: {e.Error.Reason}");
                }
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"Consume error: {e.Error.Reason}");
            }
        }
    }
    catch (OperationCanceledException)
    {
            Console.WriteLine("Closing consumer.");
            consumer.Close();
    }
}