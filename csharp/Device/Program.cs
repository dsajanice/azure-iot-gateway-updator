namespace Device
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;
    using Microsoft.Extensions.Configuration.EnvironmentVariables;

    class Program
    {
        const string settings_file = "appsettings.json";
        static Task Main(string[] args)
        {
            Console.WriteLine("Gateway updator starting..");
            Console.WriteLine("Gateway updator running..");
            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Gateway updator exiting");
            };
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .AddJsonFile(settings_file)
                    .AddEnvironmentVariables()
                    .Build();
            string message = configuration.GetValue<string>("Message");
            Console.WriteLine(message);
            while (true) ;
        }
    }
}
