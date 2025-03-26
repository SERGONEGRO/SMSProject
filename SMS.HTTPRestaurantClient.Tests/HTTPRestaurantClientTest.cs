using Domain.Models;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SMS.HTTPRestaurantClient.Tests
{
    public class HTTPRestaurantClientTest
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly HTTPRestaurantClient _client;

        public HTTPRestaurantClientTest()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);
            _client = new HTTPRestaurantClient("https://example.com/api", "username", "password", _httpClient);
        }

        [Fact]
        public async Task GetMenuAsync_ReturnsListOfDishes()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    Command = "GetMenu",
                    Success = true,
                    ErrorMessage = "",
                    Data = new
                    {
                        MenuItems = new List<MenuItem>
                    {
                        new MenuItem { Id = "5979224", Name = "Каша гречневая", Price = 50m },
                        new MenuItem { Id = "9084246", Name = "Конфеты Коровка", Price = 300m }
                    }
                    }
                }), Encoding.UTF8, "application/json")
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _client.GetMenuAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Каша гречневая", result[0].Name);
            Assert.Equal("Конфеты Коровка", result[1].Name);
        }

        [Fact]
        public async Task SendOrderAsync_ReturnsTrueOnSuccess()
        {
            // Arrange
            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                MenuItems = new List<OrderItem>
            {
                new OrderItem { Id = "5979224", Quantity = 1 },
                new OrderItem { Id = "9084246", Quantity = 0.408m }
            }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    Command = "SendOrder",
                    Success = true,
                    ErrorMessage = ""
                }), Encoding.UTF8, "application/json")
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _client.SendOrderAsync(order);

            // Assert
            Assert.True(result);
        }
    }
}
