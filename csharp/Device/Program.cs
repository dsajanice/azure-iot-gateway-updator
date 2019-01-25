namespace Device
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.Configuration.Json;

    // Device side updator opens a device stream to IoTHub and waits for the service to connect to it
    // After the service connects it opens a connection to the SSH daemon running on the device and tunnels traffic to it
    class Program
    {
        static Task Main(string[] args)
        {
            Console.WriteLine("Gateway updator starting..");
            Console.WriteLine("Gateway updator running..");
            Console.CancelKeyPress += delegate {
                Console.WriteLine("Gateway updator exiting");
            };

            // TODO: auto format extension
            IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional:true, reloadOnChange:true)
            .Build();
      Console.WriteLine($"Contents of Message Property: {message}");
            // Open a connection to IoTHub streaming endpoint

            while(true);
        }
    }
}
