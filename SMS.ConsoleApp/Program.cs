using Domain.Models;
using Microsoft.Extensions.Configuration;
using NLog;
using Npgsql;

namespace SMS.ConsoleApp
{
    class Program
    {
        private static IConfigurationRoot _configuration;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                InitializeDatabase();

                var baseUrl = _configuration["ServerUrl"];
                var username = _configuration["ServerUsername"];
                var password = _configuration["ServerPassword"];
                var httpClient = new HTTPRestaurantClient.HTTPRestaurantClient(baseUrl, username, password);

                var dishes = httpClient.GetMenuAsync().GetAwaiter().GetResult();

                SaveDishesToDatabase(dishes);
                Console.WriteLine("Dishes retrieved:");
                foreach (var dish in dishes)
                {
                    Console.WriteLine($"{dish.Name} – {dish.Id} – {dish.Price}");
                }

                var order = new Order { OrderId = Guid.NewGuid().ToString() };

                var input = string.Empty;
                while (true)
                {
                    Console.WriteLine("Enter dishes in format Code1:Quantity1;Code2:Quantity2;...");
                    input = Console.ReadLine();

                    if (ValidateInput(input, out var orderItems))
                    {
                        order.MenuItems = orderItems;
                        Logger.Info($"Введено {orderItems.Count} блюд");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please try again.");
                        Logger.Warn("Invalid input of dishes");
                    }
                }

                var result = httpClient.SendOrderAsync(order).GetAwaiter().GetResult();
                Console.WriteLine(result ? "УСПЕХ" : "Ошибка при отправке заказа");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while processing the request.");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static void InitializeDatabase()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                using var cmd = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS Dishes (Id TEXT PRIMARY KEY, Article TEXT, Name TEXT, Price NUMERIC)",
                    connection);
                cmd.ExecuteNonQuery();
                Console.WriteLine("БД Инициализирована");
                Logger.Error("БД Инициализирована");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Error(ex.Message);
            }
        }

        private static void SaveDishesToDatabase(List<Dish> dishes)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            foreach (var dish in dishes)
            {
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO Dishes (Id, Article, Name, Price) VALUES (@Id, @Article, @Name, @Price) ON CONFLICT (Id) DO NOTHING",
                    connection);
                cmd.Parameters.AddWithValue("Id", dish.Id);
                cmd.Parameters.AddWithValue("Article", dish.Id);
                cmd.Parameters.AddWithValue("Name", dish.Name);
                cmd.Parameters.AddWithValue("Price", dish.Price);
                cmd.ExecuteNonQuery();
            }
            Logger.Info($"Сохранено {dishes.Count} блюд");
        }

        private static bool ValidateInput(string input, out List<OrderItem> orderItems)
        {
            orderItems = new List<OrderItem>();
            var items = input.Split(';');

            foreach (var item in items)
            {
                var parts = item.Split(':');
                if (parts.Length != 2 || !decimal.TryParse(parts[1], out var quantity) || quantity <= 0)
                {
                    return false;
                }

                orderItems.Add(new OrderItem { Id = parts[0], Quantity = quantity });
            }

            return true;
        }
    }
}