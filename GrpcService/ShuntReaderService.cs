using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcTest
{
    public class ShuntReaderService : ShuntReader.ShuntReaderBase
    {
        private readonly ILogger<ShuntReaderService> _logger;

        public ShuntReaderService(ILogger<ShuntReaderService> logger)
        {
            _logger = logger;
        }

        public override async Task GetReadingsStream(ShuntSensorGetReadingsStreamRequest request, 
            IServerStreamWriter<ShuntSensorGetReadingsStreamResponse> responseStream,
            ServerCallContext context)
        {
            var randomValue = new Random();
            //var now = DateTime.UtcNow;

            var i = 0;
            while (!context.CancellationToken.IsCancellationRequested && i < 10)
            {
                await Task.Delay(500);
                
                var newReading = new ShuntSensorGetReadingsStreamResponse
                {
                    CurrentInAmps = randomValue.Next(1, 30),
                    VoltageInVolts = randomValue.Next(11, 13),
                    DateTimeStamp = new Timestamp()
                        
                };
                i++;
                
                _logger.LogInformation("Sending new shunt reading response");

                await responseStream.WriteAsync(newReading);
            }
        }
    }
}