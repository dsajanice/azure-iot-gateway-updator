namespace Device
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    class Program
    {
        static Task Main(string[] args)
        {
            Console.WriteLine("Gateway updator starting..");
            Console.WriteLine("Gateway updator running..");
            Console.CancelKeyPress += delegate {
                Console.WriteLine("Gateway updator exiting");
            };
            while(true);
        }
    }
}
