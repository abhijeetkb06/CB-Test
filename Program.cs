using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Couchbase;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await new CloudExample().Main();

class CloudExample
{
    
    public async Task Main()
    {
         IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => builder
        .AddFilter(level => level >= LogLevel.Trace)
        );

        var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
        loggerFactory.AddFile("C:\\\\logs\\WithPrivateEndpoint-{Date}.txt", LogLevel.Debug);

        //ILoggerFactory factory = new LoggerFactory();
        //factory.AddLog4Net("log4net.config");

        var options = new ClusterOptions
        {
            // Update these credentials for your Capella instance
            ConnectionString = "couchbases://private-endpoint.ui0cn1dkh45zmrb.cloud.couchbase.com",
            UserName = "dhruv",
            Password = "Couchbase@123",
            KvConnectTimeout = TimeSpan.FromMilliseconds(2000),
            NumKvConnections = 10,
            EnableConfigPolling = false,
            //NetworkResolution = "external",
        };
        options.WithLogging(loggerFactory);

        // Sets a pre-configured profile called "wan-development" to help avoid latency issues
        // when accessing Capella from a different Wide Area Network
        // or Availability Zone (e.g. your laptop).
        options.ApplyProfile("wan-development");
        //options.NetworkResolution = NetworkResolution.External;

        var cluster = await Cluster.ConnectAsync(
            // Update these credentials for your Capella instance
            //"couchbases://cb.zaw-fpiwljginp8p.cloud.couchbase.com",
            //"couchbases://cb.ui0cn1dkh45zmrb.cloud.couchbase.com",
            options
        );
        await cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
        // get a bucket reference
        var bucket = await cluster.BucketAsync("travel-sample");

        // get a user-defined collection reference
        var scope = await bucket.ScopeAsync("tenant_agent_00");
        var collection = await scope.CollectionAsync("users");

        // Upsert Document
        var upsertResult = await collection.UpsertAsync("my-document-key", new { Name = "Ted", Age = 31 });
        var getResult = await collection.GetAsync("my-document-key");

        Console.WriteLine(getResult.ContentAs<dynamic>());

        // Call the QueryAsync() function on the scope object and store the result.
        var inventoryScope = bucket.Scope("inventory");
        var queryResult = await inventoryScope.QueryAsync<dynamic>("SELECT * FROM airline WHERE id = 10");

        // Iterate over the rows to access result data and print to the terminal.
        await foreach (var row in queryResult)
        {
            Console.WriteLine(row);
        }
    }
}