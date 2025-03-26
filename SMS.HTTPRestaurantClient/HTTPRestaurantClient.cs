using Domain;
using Domain.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SMS.HTTPRestaurantClient
{
    public class HTTPRestaurantClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;

        public HTTPRestaurantClient(string baseUrl, string username, string password, HttpClient httpClient = null)
        {
            _baseUrl = baseUrl;
            _username = username;
            _password = password;
            _httpClient = httpClient ?? new HttpClient();
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
        {
            var authToken = Encoding.ASCII.GetBytes($"{_username}:{_password}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            return await _httpClient.SendAsync(request);
        }

        public async Task<List<Dish>> GetMenuAsync()
        {
            var requestContent = new StringContent(
                JsonSerializer.Serialize(new { Command = "GetMenu", CommandParameters = new { WithPrice = true } }),
                Encoding.UTF8,
                "application/json"
            );

            var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl) { Content = requestContent };
            var response = await SendRequestAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<Response<MenuData>>(content);

                if (responseObject.Success)
                {
                    return responseObject.Data.MenuItems.Select(item => new Dish
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = item.Price
                    }).ToList();
                }
                else
                {
                    throw new Exception(responseObject.ErrorMessage);
                }
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public async Task<bool> SendOrderAsync(Order order)
        {
            var requestContent = new StringContent(
                JsonSerializer.Serialize(new { Command = "SendOrder", CommandParameters = order }),
                Encoding.UTF8,
                "application/json"
            );

            var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl) { Content = requestContent };
            var response = await SendRequestAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<Response<bool>>(content);

                if (responseObject.Success)
                {
                    return responseObject.Success;
                }
                else
                {
                    throw new Exception(responseObject.ErrorMessage);
                }
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}
