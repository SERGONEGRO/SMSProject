using Grpc.Net.Client;
using Sms.Test;

namespace SMS.GrpcRestaurantClient
{
    public class GrpcRestaurantClient
    {
        private readonly SmsTestService.SmsTestServiceClient _client;

        public GrpcRestaurantClient(string baseUrl)
        {
            var channel = GrpcChannel.ForAddress(baseUrl);
            _client = new SmsTestService.SmsTestServiceClient(channel);
        }

        public async Task<List<MenuItem>> GetMenuAsync()
        {
            var response = await _client.GetMenuAsync(new Google.Protobuf.WellKnownTypes.BoolValue { Value = true });

            if (response.Success)
            {
                return new List<MenuItem>(response.MenuItems);
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public async Task<bool> SendOrderAsync(Order order)
        {
            var response = await _client.SendOrderAsync(order);

            if (response.Success)
            {
                return true;
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }
    }
}
