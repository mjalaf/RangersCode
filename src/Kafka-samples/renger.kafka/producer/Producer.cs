using Confluent.Kafka;
using System;
using Microsoft.Extensions.Configuration;



IConfiguration configuration = new ConfigurationBuilder()
.AddJsonFile("./appsettings.json")
.Build();

var pConfig = configuration.GetSection("Producer").Get<ProducerConfig>();
var topicName = configuration.GetValue<string>("General:TopicName");

using var producer = new ProducerBuilder<Null, string>(pConfig).Build();


for(int i=0; i <= 10; i++){
    Console.WriteLine("Producing message with value: 'testvalue'...");
    await producer.ProduceAsync(topicName, new Message<Null, string> { Value = "testvalue: " + i });

}
