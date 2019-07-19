using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace OrleansRepo
{
    class Program
    {
        public interface IHelloGrain : IGrainWithIntegerKey
        {
            Task<string> Hello();
        }

        public class HelloGrain : Grain, IHelloGrain
        {
            public Task<string> Hello()
            {
                Console.WriteLine("Hello");

                return Task.FromResult("Hello");
            }
        }

        static  async Task Main(string[] args)
        {
            var siloPort = 11111;
            int gatewayPort = 30000;
            var siloAddress = IPAddress.Loopback;

            var silo =
                new SiloHostBuilder()
                    .UseDevelopmentClustering(options => options.PrimarySiloEndpoint = new IPEndPoint(siloAddress, siloPort))
                    .UseInMemoryReminderService()
                    .ConfigureEndpoints(siloAddress, siloPort, gatewayPort)
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "helloworldcluster";
                        options.ServiceId = "1";
                    })
                    .ConfigureApplicationParts(appParts => appParts.AddApplicationPart(typeof(IHelloGrain).Assembly))
                    .ConfigureLogging(builder =>
                    {
                        builder.AddConsole();
                    })
                    .Build();

            TestGrain(silo, 0);

            await silo.StartAsync();

            TestGrain(silo, 1);

            Console.ReadLine();
        }

        private static void TestGrain(ISiloHost silo, int i)
        {
            Task.Run(async () =>
            {
                try
                {
                    var grain = silo.Services.GetRequiredService<IGrainFactory>().GetGrain<IHelloGrain>(i);

                    var result = await grain.Hello();

                    Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }
    }
}
