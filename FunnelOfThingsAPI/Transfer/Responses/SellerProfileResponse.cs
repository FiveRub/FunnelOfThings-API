namespace FunnelOfThingsAPI.Transfer.Responses
{
    public class SellerProfileResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string BinIin { get; set; } = null!;
        public string LegalAddress { get; set; } = null!;
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? Bik { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
