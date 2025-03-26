namespace Domain.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public List<OrderItem> MenuItems { get; set; }
    }
}
