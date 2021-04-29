using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcClient
{
    internal static class Program
    {
        static readonly string ServiceAddress = "http://localhost:5000";
        
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Starting GRPC client");

            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            // // Return "true" to allow certificates that are untrusted/invalid
            // var httpHandler = new HttpClientHandler
            // {
            //     ServerCertificateCustomValidationCallback =
            //         HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            // };
            //
            // // The port number(5001) must match the port of the gRPC server.
            // using var channel = GrpcChannel.ForAddress("https://localhost:5001",
            //     new GrpcChannelOptions { HttpHandler = httpHandler });            

            var channel = GrpcChannel.ForAddress(ServiceAddress);
            var client = new Greeter.GreeterClient(channel);

            var response = await client.SayHelloAsync(new HelloRequest
            {
                Name = "World"
            });
            
            Console.WriteLine($"Response from GRPC service: {response.Message}");

            await GetShuntReadings();
        }

        private static async Task GetShuntReadings()
        {
            using var channel = GrpcChannel.ForAddress(ServiceAddress);
            var client = new ShuntReader.ShuntReaderClient(channel);

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var request = new ShuntSensorGetReadingsStreamRequest
            {
                Id = "123"
            };
            using var streamingCall = client.GetReadingsStream(request, null, null, cancellationTokenSource.Token);
            try
            {
                await foreach (var shuntData in
                    streamingCall.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
                {
                    Console.WriteLine($"Received reading: Timestamp: {shuntData.DateTimeStamp}, Current: {shuntData.CurrentInAmps}A, Voltage: {shuntData.VoltageInVolts}V");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine($"Stream cancelled: {ex}");
            }
        }
    }
}