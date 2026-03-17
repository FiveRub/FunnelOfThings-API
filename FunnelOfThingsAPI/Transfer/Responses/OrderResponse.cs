namespace FunnelOfThingsAPI.Transfer.Responses
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; }

        public string Status { get; set; } = null!;

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }
        public List<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();

    }

    public class OrderItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }

    }
}
