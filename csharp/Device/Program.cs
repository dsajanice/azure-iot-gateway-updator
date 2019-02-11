namespace Device
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    using System.Net.WebSockets;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Common;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;
    using Microsoft.Extensions.Configuration.EnvironmentVariables;

    static class Program
    {
        const string settings_file = "appsettings.json";
        static async Task Main(string[] args)
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
            IDeviceRegistration registration = new DeviceRegistration();
            await registration.RegisterDevice(configuration); 
            while (true) ;
        }
    }

    public interface IDeviceRegistration
    {
            Task RegisterDevice(IConfigurationRoot configuration);        
    }

    public class DeviceRegistration : IDeviceRegistration
    {
        public async Task RegisterDevice(IConfigurationRoot configuration)
        {
            string deviceConnectionString = configuration.GetValue<string>("DeviceConnectionString"); 
            Console.WriteLine($"DeviceConnectionString: {deviceConnectionString}");
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp_WebSocket_Only);
            while (true)
            {
                try {
                    Console.WriteLine("Waiting for stream request");
                    Console.WriteLine($"Before: {DateTime.Now.ToString()}");
                    DeviceStreamRequest streamRequest = await deviceClient.WaitForDeviceStreamRequestAsync();
                    Console.WriteLine("Received stream request");
                    if (streamRequest != null)
                    {
                    await deviceClient.AcceptDeviceStreamRequestAsync(streamRequest);
                    using (ClientWebSocket webSocket = await GetStreamingClientAsync(streamRequest.Url, streamRequest.AuthorizationToken))
                    {
                        using (TcpClient tcpClient = new TcpClient())
                        {
                            string host = configuration.GetValue<string>("ProxyHostName");
                            int port = configuration.GetValue<int>("ProxyPort");
                            await tcpClient.ConnectAsync(host, port).ConfigureAwait(false);

                            using (NetworkStream localStream = tcpClient.GetStream())
                            {
                                Console.WriteLine("Starting streaming");

                                await Task.WhenAny(
                                    HandleIncomingDataAsync(localStream, webSocket),
                                    HandleOutgoingDataAsync(localStream, webSocket));

                                localStream.Close();
                            }
                    }

                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                    }
                    }
                } 
                catch (Exception e)
                {
                    Console.WriteLine($"After: {DateTime.Now.ToString()}");
                    Console.WriteLine($"Exception: {e.ToString()}");
                }
            }
        }

        static async Task<ClientWebSocket> GetStreamingClientAsync(Uri url, string authorizationToken)
        {
            ClientWebSocket wsClient = new ClientWebSocket();
            wsClient.Options.SetRequestHeader("Authorization", "Bearer " + authorizationToken);

            await wsClient.ConnectAsync(url, CancellationToken.None).ConfigureAwait(false);

            return wsClient;
        }

        static async Task HandleIncomingDataAsync(NetworkStream localStream, ClientWebSocket remoteStream)
        {
            byte[] buffer = new byte[10240];

            while (remoteStream.State == WebSocketState.Open)
            {
                var receiveResult = await remoteStream.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);

                await localStream.WriteAsync(buffer, 0, receiveResult.Count).ConfigureAwait(false);
            }
        }

        static async Task HandleOutgoingDataAsync(NetworkStream localStream, ClientWebSocket remoteStream)
        {
            byte[] buffer = new byte[10240];

            while (localStream.CanRead)
            {
                int receiveCount = await localStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    
                await remoteStream.SendAsync(new ArraySegment<byte>(buffer, 0, receiveCount), WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }
}
