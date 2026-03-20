namespace FunnelOfThingsAPI.Transfer.Requests
{
    public class RegisterSellerRequest
    {
        public int UserId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string BinIin { get; set; } = null!;
        public string LegalAddress { get; set; } = null!;
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? Bik { get; set; }
    }
}
