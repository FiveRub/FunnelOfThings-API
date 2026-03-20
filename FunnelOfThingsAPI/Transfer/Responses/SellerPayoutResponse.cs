namespace FunnelOfThingsAPI.Transfer.Responses
{
    public class SellerPayoutResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionPct { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}